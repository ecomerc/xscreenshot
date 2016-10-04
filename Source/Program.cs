using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace xscreenshot {

    public class ApplicationArguments {

        public enum PlatformEnum {
            iOS,
            Android,
            WindowsPhone
        }
        public PlatformEnum Platform { get; set; }
        public string Config { get; set; }
        public string Script { get; set; }
        public bool Tests { get; set; }
        public bool Init { get; set; }

        public bool ShowVersion { get; set; }

        public string AppPath { get; set; }
        public string AppName { get; set; }

        public string MagicPath { get; set; }

        public void FixPaths() {
            MagicPath = FixPath(MagicPath);
            AppPath = FixPath(AppPath);
            Config = FixPath(Config);
            Script = FixPath(Script);

        }

        private string FixPath(string path) {
            if (path.StartsWith("::/"))
                return path.Substring(2);

            return path;
        }
    }



    class Program {


        static void Main(string[] args) {
            Console.WriteLine(string.Join("::", args));

            System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;


            for (var i = 0; i < args.Length; i++)
                if (args[i][0] == '/')
                    args[i] = "::" + args[i];


            var p = new Fclp.FluentCommandLineParser<ApplicationArguments>() { IsCaseSensitive = false };

            // specify which property the value will be assigned too.
            p.Setup(arg => arg.Script)
             .As('s', "script") // define the short and long option name
             .WithDescription("Script file to run once the emulator is started")
             .Required(); // using the standard fluent Api to declare this Option as required.

            p.Setup(arg => arg.Config)
             .As('c', "config")
             .WithDescription("Configuration file with settings");

            p.Setup(arg => arg.Tests)
             .As('t', "test")
             .WithDescription("Run tests do not run simulator")
             .SetDefault(false);

            p.Setup(arg => arg.Platform)
             .As('p', "platform")
             .WithDescription("Select platform, ios/android/wp")
             .SetDefault(ApplicationArguments.PlatformEnum.iOS);

            p.Setup(arg => arg.MagicPath)
                .As("magic")
                .WithDescription("Path to StatusMagic source");

            p.Setup(arg => arg.AppPath)
                .As("appPath")
                .WithDescription("Path to app file");

            p.Setup(arg => arg.AppName)
                .As("appName")
                .WithDescription("Name of the app");

            p.SetupHelp("h", "help", "?").Callback(s => Console.WriteLine(s));


            p.Setup(arg => arg.ShowVersion)
             .As('v', "version")
             .SetDefault(false);


            p.Setup(arg => arg.Init)
             .As('i', "init")
             .SetDefault(false);

            var result = p.Parse(args);

            if (result.HelpCalled)
                return;

            if (p.Object.Init) {
                try {
                    Console.WriteLine("Increasing features in init");
                    var init = Cecil.IncreaseFeatures.Init();
                    if (init)
                        Console.WriteLine("UITest was already inited");
                    else
                        Console.WriteLine("UITest is ready to party");

                } catch (Exception ex) {
                    Console.WriteLine(ex.ToString());
                }
            }

            if (p.Object.ShowVersion) {
                Console.WriteLine("Version ::::");
                Console.WriteLine("For command line argument help: [-? | -h | --help]\n\n");
                Console.WriteLine("Exiting.");
                return;
            }
            if (p.Object.Tests) {
                PerformTest();
            }

            if (result.HasErrors) {
                Console.WriteLine(result.ErrorText);
                Console.WriteLine("For command line argument help: [-? | -h | --help]\n\n");
                Console.WriteLine("Exiting.");
            } else {
                p.Object.FixPaths();

                try {
                    Console.WriteLine("Increasing features");
                    var init = Cecil.IncreaseFeatures.Init();
                    if (!init) {
                        Console.WriteLine("UITest was just inited, please restart");
                        return;
                    }
                } catch (Exception ex) {
                    Console.WriteLine(ex.ToString());
                }

                try {

                    MainCore(p.Object);
                } catch (Exception ex) {
                    Console.WriteLine(ex.ToString());

                }

                Console.WriteLine("All done");

            }

#if DEBUG
            Console.ReadLine();
#endif

        }


        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e) {
            Console.WriteLine(e.ExceptionObject.ToString());
            Console.WriteLine("Press Enter to continue");
            Console.ReadLine();
            Environment.Exit(1);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void MainCore(ApplicationArguments args) {
            if (!Utilities.TryLoadConfig(args.Config))
                Console.WriteLine("Please make sure you directly supply sensible inputs for the required configuration options");


            if (!Utilities.TryLoadScript(args.Script))
                return;

            switch (args.Platform) {
                case ApplicationArguments.PlatformEnum.iOS:


                    Console.WriteLine("Magic: " + args.MagicPath);
                    if (!string.IsNullOrWhiteSpace(args.MagicPath) && Directory.Exists(args.MagicPath))
                        JsonConfig.Config.Global.iOS.SimulatorStatusMagicPath = args.MagicPath;

                    Console.WriteLine("Magic: " + JsonConfig.Config.Global.iOS.SimulatorStatusMagicPath);


                    if (!string.IsNullOrWhiteSpace(args.AppName)) {
                        JsonConfig.Config.Global.iOS.AppName = args.AppName;
                    }

                    if (!string.IsNullOrWhiteSpace(args.AppPath) && File.Exists(args.AppPath))
                        JsonConfig.Config.Global.iOS.AppPath = args.AppPath;

                    if (JsonConfig.Config.Global.iOS.AppPath == "{auto}") {
                        var path = iOS.iOSHelpers.GetXamarinAppAutomatically(JsonConfig.Config.Global.iOS.AppName);
                        if (Directory.Exists(path))
                            JsonConfig.Config.Global.iOS.AppPath = path;
                    }

                    //JsonConfig.Config.SetUserConfig

                    if (Core.SystemManager.OperatingSystem == Core.OS.Mac) {
                        if (args.Tests) {
                            PerformTest();
                        } else {
                            iOS.iOSHelpers.RunSimulatorsForAction(TakeScreenshots, args.Config);
                        }
                    } else {
                        Console.WriteLine("In order to complete iOS screenshots, you must run this program on a Mac, sorry");
                    }
                    break;

                case ApplicationArguments.PlatformEnum.Android:
                    // Incomplete

                    // start the emulator

                    Xamarin.UITest.IApp app = Xamarin.UITest.ConfigureApp.Android.EnableLocalScreenshots()
                               .ApkFile("")
                               .StartApp();


                    break;

                case ApplicationArguments.PlatformEnum.WindowsPhone:
                //Todo: Windows Phone is oddly missing?
                default:

                    break;

            }






        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        static void PerformTest() {   // your class as shown is Program

            Console.WriteLine("Will do tests now");
            Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var asm in asms) {
                Console.WriteLine(asm.FullName);
            }

            var sample = Xamarin.UITest.TestPlatform.Local;
            Console.WriteLine("Got sample:" + sample.ToString());
            Cecil.IncreaseFeatures.SetAdditionalLaunchParametersForiOS("-AppleLanguages");
            var r = Cecil.IncreaseFeatures.GetAdditionalLaunchParametersForiOS();
            Console.WriteLine("GetAdditionalLaunchParameters:" + r);
        }



        static void TakeScreenshots(Xamarin.UITest.IApp app, string outputPath) {

            var screenShotIndex = 0;

            Action<string> screenshotAction = (string title) => {
                try {
                    screenShotIndex++;
                    var fi = app.Screenshot(title);
                    var filename = outputPath + "." + screenShotIndex.ToString() + ".png";
                    Console.WriteLine(filename);
                    if (File.Exists(filename))
                        File.Delete(filename);
                    fi.MoveTo(filename);
                } catch (Exception ex) {
                    Console.WriteLine(string.Format("Could not capture screenshot: {0}", ex.Message));
                }
            };


            iOS.SimulatorHelpers.EnableHardwareKeyboard();
            Action<string, string> typeAction = (e, s) => {

                if (Core.SystemManager.OperatingSystem == Core.OS.Mac)
                    app.Query(r => r.Raw("* marked:'" + e + "'", new[] { new { setAutocorrectionType = 1 } }));

                try {
                    app.EnterText(e, s);

                } catch {
                }
                app.TapCoordinates(1, 200);
                System.Threading.Thread.Sleep(500);
                app.DismissKeyboard();

            };


            if (Utilities.ScriptObject != null) {
                MethodInfo o = Utilities.ScriptObject.GetType().GetMethod("c");

                o.Invoke(o, new object[] { app, screenshotAction, typeAction });
            } else {
                screenshotAction("single");
            }

        }

    }
}
