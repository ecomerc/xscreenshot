﻿using System;
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

        public bool ShowVersion { get; set; }

        public string AppPath { get; set; }
        public string AppName { get; set; }

        public string MagicPath { get; set; }


    }

    class Program {


        static void Main(string[] args) {

            var p = new Fclp.FluentCommandLineParser<ApplicationArguments>() { IsCaseSensitive = false };

            // specify which property the value will be assigned too.
            p.Setup(arg => arg.Script)
             .As('s', "script") // define the short and long option name
             .Required(); // using the standard fluent Api to declare this Option as required.

            p.Setup(arg => arg.Config)
             .As('c', "config")
             .Required();

            p.Setup(arg => arg.Tests)
             .As('t', "test")
             .SetDefault(false);

            p.Setup(arg => arg.Platform)
             .As('p', "platform")
             .SetDefault(ApplicationArguments.PlatformEnum.iOS);

            p.Setup(arg => arg.MagicPath)
                .As("magic");

            p.Setup(arg => arg.AppPath)
                .As("appPath");

            p.Setup(arg => arg.AppName)
                .As("appName");

            p.SetupHelp("h", "help", "?").Callback(s => Console.WriteLine(s));


            p.Setup(arg => arg.ShowVersion)
             .As('v', "version")
             .SetDefault(false);

            var result = p.Parse(args);

            if (result.HelpCalled)
                return;
            if (result.HasErrors) {
                Console.WriteLine(result.ErrorText);
                Console.WriteLine("For command line argument help: [-? | -h | --help]\n\n");
                Console.WriteLine("Exiting.");
                return;
            }

            if (p.Object.ShowVersion) {
                Console.WriteLine("Version ::::");
                Console.WriteLine("For command line argument help: [-? | -h | --help]\n\n");
                Console.WriteLine("Exiting.");
                return;
            }

            if (result.HasErrors == false) {
                try {
                    Console.WriteLine("Increasing features");
                    Cecil.IncreaseFeatures.Init();
                } catch (Exception ex) {
                    Console.WriteLine(ex.ToString());
                }


                MainCore(p.Object);


                Console.WriteLine("All done");

            }

        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void MainCore(ApplicationArguments args) {
            if (!Utilities.TryLoadConfig(args.Config))
                return;


            if (!Utilities.TryLoadScript(args.Script))
                return;

            switch (args.Platform) {
                case ApplicationArguments.PlatformEnum.iOS:


                    if (!string.IsNullOrWhiteSpace(args.MagicPath) && Directory.Exists(args.MagicPath))
                        JsonConfig.Config.Global.iOS.SimulatorStatusMagicPath = args.MagicPath;

                    if (!string.IsNullOrWhiteSpace(args.AppName)) {
                        var path = iOS.iOSHelpers.GetXamarinAppAutomatically(args.AppName);
                        if (File.Exists(path))
                            JsonConfig.Config.Global.iOS.AppPath = args.AppPath;
                    }

                    if (!string.IsNullOrWhiteSpace(args.AppPath) && File.Exists(args.AppPath))
                        JsonConfig.Config.Global.iOS.AppPath = args.AppPath;

                    //JsonConfig.Config.SetUserConfig

                    if (Core.SystemManager.OperatingSystem == Core.OS.Mac) {
                        if (args.Tests) {
                            PerformTest();
                        } else {
                            iOS.iOSHelpers.RunSimulatorsForAction(TakeScreenshots);
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
