using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace xscreenshot.Cecil {
    internal class IncreaseFeatures {

        public static void Init() {

            string path = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            var localDirectory = new Uri(path).LocalPath;
            Console.WriteLine("Codebase:" + localDirectory);

            var directory = System.IO.Path.GetDirectoryName(localDirectory);



            var initialUiTestDLL = Path.Combine(directory, "Xamarin.UITest.dll");
            var originalUiTestDLL = Path.Combine(directory, "Xamarin.UITest.Original.dll");
            var modifiedUiTestDLL = Path.Combine(directory, "Xamarin.UITest.Modified.dll");

            if (System.IO.File.Exists(initialUiTestDLL)) {
                System.IO.File.Move(initialUiTestDLL, originalUiTestDLL);
            }

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;

            /* really not needed as we load the dll explicitly */
            currentDomain.AssemblyResolve += new ResolveEventHandler(ResolveEventHandler);

            var definition = UpgradeAssembly();
            definition.Write(modifiedUiTestDLL);
            /*var path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            path = System.IO.Path.GetDirectoryName(path);
            path = System.IO.Path.Combine(path, "Xamarin.UITest.dll");
            Assembly.LoadFile(path);*/
            GetAssembly(definition);
        }

        private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args) {
            //Console.WriteLine("Loading:" + args.LoadedAssembly.FullName + " " + args.LoadedAssembly.CodeBase);

        }

        static Assembly ResolveEventHandler(object sender, ResolveEventArgs args) {
            Console.WriteLine("Resolving...");
            var previouslyLoaded = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
            if (previouslyLoaded != null)
                return previouslyLoaded;

            if (args.Name.Contains("Xamarin.UITest")) {
                var definition = UpgradeAssembly();
                return GetAssembly(definition);
            }
            return null;
        }





        public static void SetAdditionalLaunchParametersForiOS(string parameters) {
            var type = typeof(Xamarin.UITest.iOS.iOSApp);
            var field = type.GetField("AdditionalLaunchParameters", BindingFlags.Static | BindingFlags.Public);
            field.SetValue(null, parameters);
        }
        public static string GetAdditionalLaunchParametersForiOS() {
            var type = typeof(Xamarin.UITest.iOS.iOSApp);
            var field = type.GetField("AdditionalLaunchParameters", BindingFlags.Static | BindingFlags.Public);
            return (string)field.GetValue(null);
        }

        private static AssemblyDefinition UpgradeAssembly() {

            string path = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            var localDirectory = new Uri(path).LocalPath;
            Console.WriteLine("Codebase:" + localDirectory);

            var directory = System.IO.Path.GetDirectoryName(localDirectory);


            string assemblyName = "Xamarin.UITest.Original.dll";
            string assemblyPath = Path.Combine(directory, assemblyName);


            var definition = AssemblyDefinition.ReadAssembly(assemblyPath);


            FieldDefinition fieldDefinition = new FieldDefinition(
                                "AdditionalLaunchParameters",
                                Mono.Cecil.FieldAttributes.Static | Mono.Cecil.FieldAttributes.Public, definition.MainModule.TypeSystem.String);


            var staticFieldDestination = definition.MainModule.GetType("Xamarin.UITest.iOS.iOSApp");
            staticFieldDestination.Fields.Add(fieldDefinition);

            StringHelper.FindString("-w \"{5}\" -D \"{0}\" -t \"{1}\" \"{4}\" -e UIARESULTSPATH \"{2}\" -e UIASCRIPT \"{3}\"", definition,
                (m, instString, index) => {


                    instString.Operand = "-w \"{5}\" -D \"{0}\" -t \"{1}\" \"{4}\" -e UIARESULTSPATH \"{2}\" -e UIASCRIPT \"{3}\" {6}";
                    var arrayLengthInst = instString.Next;

                    arrayLengthInst.OpCode = OpCodes.Ldc_I4_7;

                    var location = m.Body.Instructions[index + 27];

                    m.Body.GetILProcessor().InsertAfter(location, Instruction.Create(OpCodes.Stelem_Ref));
                    m.Body.GetILProcessor().InsertAfter(location, Instruction.Create(OpCodes.Ldsfld, fieldDefinition));
                    m.Body.GetILProcessor().InsertAfter(location, Instruction.Create(OpCodes.Ldc_I4_6));
                    m.Body.GetILProcessor().InsertAfter(location, Instruction.Create(OpCodes.Dup));

                    return true; //Only do one replacement
                }
                );

            return definition;
        }

        private static Assembly GetAssembly(AssemblyDefinition assembly) {

            byte[] assemblyData;

            using (var memStream = new MemoryStream()) {

                assembly.Write(memStream);

                assemblyData = memStream.ToArray();
            }
            var inMemory = AppDomain.CurrentDomain.Load(assemblyData);
            //Assembly.Load(assemblyData);
            return inMemory;
        }
    }
}
