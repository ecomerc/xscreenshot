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

        public static bool TryLoadConfig(string file) {
            try {
                if (File.Exists(file) && (file.EndsWith(".conf") || file.EndsWith(".config") || file.EndsWith(".json"))) {
                    var fileInfo = new FileInfo(file.ExpandPath());
                    Console.WriteLine("Loading config: " + fileInfo.FullName);
                    var config = JsonConfig.Config.ApplyJsonFromPath(fileInfo.FullName);
                    JsonConfig.Config.SetUserConfig(config);
                    return true;
                }
            } catch (Exception ex) {
                Console.WriteLine(string.Format("Supplied config file was invalid: {0}", ex.Message));
            }
            return false;
        }

        public static bool TryLoadScript(string file) {
            try {
                if (File.Exists(file) && (file.EndsWith(".cs") || file.EndsWith(".script"))) {
                    var fileInfo = new FileInfo(file.ExpandPath());
                    Console.WriteLine("Loading script: " + fileInfo.FullName);
                    ScriptObject = Utilities.Eval(File.ReadAllText(fileInfo.FullName));
                    return true;
                }
            } catch (Exception ex) {
                Console.WriteLine(string.Format("Supplied script file was invalid: {0}", ex.Message));
            }
            return false;
        }



        public static void Run(string command, string arguments, string workingDir = null, bool useShellExecute = false) {
            var pi = new ProcessStartInfo(command, arguments) { UseShellExecute = useShellExecute };
            if (!string.IsNullOrWhiteSpace(workingDir)) {
                pi.WorkingDirectory = workingDir;
            }
            var process = Process.Start(pi);
            process.WaitForExit ();


        }


        public static string RunWithOutput(string command, string arguments, string workingDir = null, bool useShellExecute = false) {
            var pi = new ProcessStartInfo(command, arguments) { UseShellExecute = useShellExecute,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            if (!string.IsNullOrWhiteSpace(workingDir)) {
                pi.WorkingDirectory = workingDir;
            }
            var sb = new StringBuilder();
            var process = Process.Start(pi);
            while (!process.StandardOutput.EndOfStream) {
                sb.AppendLine( process.StandardOutput.ReadLine());
                // do something with line
            }
            return sb.ToString();

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


            var path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            path = System.IO.Path.GetDirectoryName(path);
            path = System.IO.Path.Combine(path, "Xamarin.UITest.Original.dll");

            if (File.Exists(typeof(Xamarin.UITest.IApp).Assembly.Location))
                path = typeof(Xamarin.UITest.IApp).Assembly.Location;

            var csc = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v3.5" } });
            var p = new CompilerParameters(new[] { "mscorlib.dll", "System.Core.dll" }, null, true);
            
            p.ReferencedAssemblies.Add(path);
            p.GenerateInMemory = true; p.GenerateExecutable = false;
            var type = "";

            CompilerResults r = csc.CompileAssemblyFromSource(p, "using System; class p {public static void c(Xamarin.UITest.IApp app, Action<string> Screenshot, Action<string, string> EnterTextInto){" + code + "}}");
            if (r.Errors.Count > 0) { r.Errors.Cast<CompilerError>().ToList().ForEach(error => Console.WriteLine(error.ErrorText)); return null; }
            System.Reflection.Assembly a = r.CompiledAssembly;
            var instance = a.CreateInstance("p");
            return instance;
        }
        

    }
}
