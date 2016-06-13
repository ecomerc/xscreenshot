using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace xscreenshot {
    class Program {


        static void Main(string[] args) {
            Cecil.IncreaseFeatures.Init();


            MainCore(args);


            Console.WriteLine("All done");

        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        static void MainCore(string[] args) {   // your class as shown is Program

            if (args.Count() > 0 && File.Exists(args[0])) {
                Utilities.TryLoadConfig(args[0]);
                Utilities.TryLoadScript(args[0]);
            }

            if (args.Count() > 1 && File.Exists(args[1])) {
                Utilities.TryLoadConfig(args[1]);
                Utilities.TryLoadScript(args[1]);
            }

            if (args.Contains("tests", StringComparer.OrdinalIgnoreCase)) {
                Console.WriteLine("Will do tests now");
                Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();

                foreach (var asm in asms) {
                    Console.WriteLine(asm.FullName);
                }
                PerformTest();
                Console.ReadKey();
            }


            if (args.Contains("ios", StringComparer.OrdinalIgnoreCase)) {
                if (Core.SystemManager.OperatingSystem == Core.OS.Mac) {
                    iOS.iOSHelpers.RunSimulatorsForAction(TakeScreenshots);
                } else {
                    Console.WriteLine("In order to complete iOS screenshots, you must run this program on a Mac, sorry");
                }
            }

            // Incomplete
            if (args.Contains("droid", StringComparer.OrdinalIgnoreCase)) {

                // start the emulator

                Xamarin.UITest.IApp app = Xamarin.UITest.ConfigureApp.Android.EnableLocalScreenshots()
                       .ApkFile("")
                       .StartApp();


            }

            //Todo: Windows Phone is oddly missing?


            }


        [MethodImpl(MethodImplOptions.NoInlining)]
        static void PerformTest() {   // your class as shown is Program


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
