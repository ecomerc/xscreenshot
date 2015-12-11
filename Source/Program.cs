using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using JsonConfig;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Diagnostics;

namespace xscreenshot {
    class Program {

        private static void TryLoadConfig(string file) {
            try {
                if (File.Exists(file) && (file.EndsWith(".conf") || file.EndsWith(".config") || file.EndsWith(".json"))) {
                    var fileInfo = new FileInfo(file);
                    Console.WriteLine("Loading config: " + fileInfo.FullName);
                    var config = JsonConfig.Config.ApplyJsonFromPath(fileInfo.FullName);
                    JsonConfig.Config.SetUserConfig(config);
                }
            } catch (Exception ex) {
                Console.WriteLine(string.Format("Supplied config file was invalid: {0}", ex.Message));
            }
        }

        private static void TryLoadScript(string file) {
            try {
                if (File.Exists(file) && (file.EndsWith(".cs") || file.EndsWith(".script"))) {
                    var fileInfo = new FileInfo(file);
                    Console.WriteLine("Loading script: " + fileInfo.FullName);
                    var config = JsonConfig.Config.ApplyJsonFromPath(fileInfo.FullName);
                    JsonConfig.Config.SetUserConfig(config);
                }
            } catch (Exception ex) {
                Console.WriteLine(string.Format("Supplied script file was invalid: {0}", ex.Message));
            }
        }

        static void Main(string[] args) {

            if (args.Count() > 0 && File.Exists(args[0])) {
                TryLoadConfig(args[0]);
                TryLoadScript(args[0]);
            }

            if (args.Count() > 1 && File.Exists(args[1])) {
                TryLoadConfig(args[1]);
                TryLoadScript(args[1]);
            }


            if (Directory.Exists(Config.Global.iOS.DevicesPath)) {
                string path = Config.Global.iOS.DevicesPath;
                Console.WriteLine(string.Format("Loading devices from: {0}", path));


                IEnumerable<string> excludedDevices = null; ;
                if (IsSet(Config.Global.iOS.ExcludeDevices) && Config.Global.iOS.ExcludeDevices.Length > 0) {
                    excludedDevices = Config.Global.iOS.ExcludeDevices;
                }
                IEnumerable<string> includeDevices = null;
                if (IsSet(Config.Global.iOS.IncludeDevices) && Config.Global.iOS.IncludeDevices.Length > 0) {
                    includeDevices = Config.Global.iOS.IncludeDevices;
                }

                Process.Start(new ProcessStartInfo("killall \"iOS Simulator\")") { UseShellExecute = false });
                Process.Start(new ProcessStartInfo("killall \"Simulator\")") { UseShellExecute = false });

                var devices = SimulatorHelpers.LoadSimulators(path, includeDevices, excludedDevices, Config.Global.iOS.OSVersion);


                foreach (Simulator device in devices) {
                    Console.WriteLine(string.Format("Beginning screenshots for device: {0} {1} {2}", device.Name, device.UDID, device.iOSVersion));
                    screenShotIndex = 0;


                    Process.Start(new ProcessStartInfo(string.Format("xcrun simctl erase {0}", device.UDID)) { UseShellExecute = false });

                    string outputPath = Path.Combine(Config.Global.iOS.OutputPath, device.Name + "." + device.iOSVersion);

                    Directory.CreateDirectory(Config.Global.iOS.OutputPath);

                    try {


                        var app = Xamarin.UITest.ConfigureApp.iOS.EnableLocalScreenshots()
                               .AppBundle((string)Config.Global.iOS.AppPath)
                               .DeviceIdentifier(device.UDID)
                           .StartApp();


                        app.Back();
                        app = null;

                    } catch (Exception ex) {
                        Console.WriteLine(string.Format("Could not run test: {0}", ex.Message));
                    }

                }

                Console.WriteLine("Done");

            } else {
                Console.WriteLine(string.Format("An invalid iOS devices path was provided: {0}", Config.Global.iOS.DevicesPath));
            }
            Console.WriteLine("All done");


            Process.Start(new ProcessStartInfo("killall \"iOS Simulator\")") { UseShellExecute = false });
            Process.Start(new ProcessStartInfo("killall \"Simulator\")") { UseShellExecute = false });

        }

        private static int screenShotIndex = 0;

        private static void Screenshot(Xamarin.UITest.IApp app, string path, string title) {
            try {
                screenShotIndex++;
                var fi = app.Screenshot(title);
                var filename = path + "." + screenShotIndex.ToString() + ".png";
                Console.WriteLine(filename);

                fi.MoveTo(filename);
            } catch (Exception ex) {
                Console.WriteLine(string.Format("Could not capture screenshot: {0}", ex.Message));
            }
        }

        private static bool IsSet(dynamic obj) {
            if (obj == null) {
                return false;
            }
            if (obj is bool) {
                return obj;
            }
            return true;

        }

        private static object Eval(string code) {
            var csc = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v3.5" } });
            var p = new CompilerParameters(new[] { "mscorlib.dll", "System.Core.dll" }, null, true);
            p.GenerateInMemory = true; p.GenerateExecutable = false;
            CompilerResults r = csc.CompileAssemblyFromSource(p, "using System; class p {public static object c(){" + code + "}}");
            if (r.Errors.Count > 0) { r.Errors.Cast<CompilerError>().ToList().ForEach(error => Console.WriteLine(error.ErrorText)); return null; }
            System.Reflection.Assembly a = r.CompiledAssembly;
            var instance = a.CreateInstance("p");
            MethodInfo o = .GetType().GetMethod("c");
            return o.Invoke(o, null);
        }
    }
}
