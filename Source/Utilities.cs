using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using xscreenshot.Extensions;
using System.Text;
using System.Threading.Tasks;

namespace xscreenshot {
    public static class Utilities {

        public static object ScriptObject;

        public static void TryLoadConfig(string file) {
            try {
                if (File.Exists(file) && (file.EndsWith(".conf") || file.EndsWith(".config") || file.EndsWith(".json"))) {
                    var fileInfo = new FileInfo(file.ExpandPath());
                    Console.WriteLine("Loading config: " + fileInfo.FullName);
                    var config = JsonConfig.Config.ApplyJsonFromPath(fileInfo.FullName);
                    JsonConfig.Config.SetUserConfig(config);
                }
            } catch (Exception ex) {
                Console.WriteLine(string.Format("Supplied config file was invalid: {0}", ex.Message));
            }
        }

        public static void TryLoadScript(string file) {
            try {
                if (File.Exists(file) && (file.EndsWith(".cs") || file.EndsWith(".script"))) {
                    var fileInfo = new FileInfo(file.ExpandPath());
                    Console.WriteLine("Loading script: " + fileInfo.FullName);
                    ScriptObject = Utilities.Eval(File.ReadAllText(fileInfo.FullName));
                }
            } catch (Exception ex) {
                Console.WriteLine(string.Format("Supplied script file was invalid: {0}", ex.Message));
            }
        }



        public static void Run(string command, string arguments, string workingDir = null, bool useShellExecute = false) {
            var pi = new ProcessStartInfo(command, arguments) { UseShellExecute = useShellExecute };
            if (!string.IsNullOrWhiteSpace(workingDir)) {
                pi.WorkingDirectory = workingDir;
            }
            var process = Process.Start(pi);
			process.WaitForExit ();
        }


        public static bool IsSet(dynamic obj) {
            if (obj == null) {
                return false;
            }
            if (obj is bool) {
                return obj;
            }
            if (obj is string) {
                return !string.IsNullOrWhiteSpace(obj);
            }
                
            return true;

        }


        public static object Eval(string code) {
            var csc = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v3.5" } });
            var p = new CompilerParameters(new[] { "mscorlib.dll", "System.Core.dll" }, null, true);
            p.ReferencedAssemblies.Add(typeof(Xamarin.UITest.IApp).Assembly.Location);
            p.GenerateInMemory = true; p.GenerateExecutable = false;
            CompilerResults r = csc.CompileAssemblyFromSource(p, "using System; class p {public static void c(Xamarin.UITest.IApp app, Action<string> Screenshot){" + code + "}}");
            if (r.Errors.Count > 0) { r.Errors.Cast<CompilerError>().ToList().ForEach(error => Console.WriteLine(error.ErrorText)); return null; }
            System.Reflection.Assembly a = r.CompiledAssembly;
            var instance = a.CreateInstance("p");
            return instance;
        }

    }
}
