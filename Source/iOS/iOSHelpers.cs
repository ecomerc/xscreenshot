using JsonConfig;
using System;
using System.Collections.Generic;
using System.IO;
using xscreenshot.Extensions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xscreenshot.iOS {
    internal class iOSHelpers {

        internal static string GetXamarinAppAutomatically(string appName) {
            var appPath = ("~/Library/Caches/Xamarin/mtbs/builds/" + appName + "/").ExpandPath();
            Console.WriteLine(appPath);

            var mainDir = (new DirectoryInfo(appPath))
                                .EnumerateDirectories()
                                .OrderByDescending(d => d.LastWriteTimeUtc)
                                .FirstOrDefault();

            if (mainDir == null) {
                Console.WriteLine("No build directories found");
                return null;
            }
            
            Console.WriteLine(mainDir.Name);

            var app = (new DirectoryInfo(Path.Combine(mainDir.FullName , "bin/iPhoneSimulator/Debug/")))
                .EnumerateDirectories("*.app")
                .OrderByDescending(f => f.CreationTimeUtc)
                .FirstOrDefault();

            if (app == null) {
                Console.WriteLine("No app was found in the build directory");
                return null;
            }

            return app.FullName;

        }

        internal static void ExecuteApplescript(string script) {

            Utilities.Run("osascript", string.Format("-e '{0}' 0", script));

        }


        internal static string ExecuteApplescriptWithOutput(string script) {

            return Utilities.RunWithOutput("osascript", string.Format("-e '{0}' 0", script));

        }


        internal static void RunSimulatorsForAction(Action<Xamarin.UITest.IApp, string> simulatorTest, string configurationPath) {
            string path = ((string)Config.Global.iOS.DevicesPath).ExpandPath();
            if (Directory.Exists(path)) {
                Console.WriteLine(string.Format("Loading devices from: {0}", path));

                IEnumerable<string> languages = null;
                if (Utilities.IsSet(Config.Global.iOS.ExcludeDevices) && Config.Global.iOS.ExcludeDevices.Length > 0) {
                    languages = Config.Global.iOS.Languages;
                }
                if (languages == null || languages.Count() == 0) {
                    languages = new[] { "" };
                }


                IEnumerable<string> excludedDevices = null;
                if (Utilities.IsSet(Config.Global.iOS.ExcludeDevices) && Config.Global.iOS.ExcludeDevices.Length > 0) {
                    excludedDevices = Config.Global.iOS.ExcludeDevices;
                }
                IEnumerable<string> includeDevices = null;
                if (Utilities.IsSet(Config.Global.iOS.IncludeDevices) && Config.Global.iOS.IncludeDevices.Length > 0) {
                    includeDevices = Config.Global.iOS.IncludeDevices;
                }

                iOS.SimulatorHelpers.ShutdownSimulator();

                IEnumerable<Simulator> devices = iOS.SimulatorHelpers.LoadSimulators(path, includeDevices, excludedDevices, Config.Global.iOS.OSVersion);




                string outputPath = ((string)Config.Global.iOS.OutputPath).ExpandPath(configurationPath);


                if (string.IsNullOrWhiteSpace(outputPath))
                    throw new ArgumentNullException("Missing output path");

                string appPath = ((string)Config.Global.iOS.AppPath).ExpandPath();

                if (string.IsNullOrWhiteSpace(appPath))
                    throw new ArgumentNullException("Missing app path");


                var bundle = SimulatorHelpers.GetBundleIdentifierFromApp(appPath);

                if (string.IsNullOrWhiteSpace(bundle))
                    throw new ArgumentNullException("Missing bundle identifier");

                Console.WriteLine("BundleId: " + bundle);

                foreach (Simulator device in devices) {
                    Console.WriteLine(string.Format("Beginning screenshots for device: {0} {1} {2}", device.Name, device.UDID, device.iOSVersion));

                    if (Utilities.IsSet(Config.Global.iOS.SimulatorStatusMagicPath)) {
                        Console.WriteLine("Building Magic Status from " + Config.Global.iOS.SimulatorStatusMagicPath);
                        StatusMagic.Build(Config.Global.iOS.SimulatorStatusMagicPath, "iOS", new[] { device.UDID });
                        System.Threading.Thread.Sleep(1000);
                    }

                    try {

                        //SimulatorHelpers.Install(appPath);
                        // SimulatorHelpers.Launch(bundle);


                        foreach (var language in languages) {
                            iOS.SimulatorHelpers.ShutdownSimulator();
                            iOS.SimulatorHelpers.CleanSimulator(device.UDID);
                            iOS.SimulatorHelpers.DisableAutocorrect(device.UDID);

                            if (Utilities.IsSet(Config.Global.iOS.SimulatorStatusMagicPath)) {
                                SimulatorHelpers.StartSimulator(device.UDID);
                                Console.WriteLine("Installing status magic");
                                StatusMagic.Install("iOS");
                                StatusMagic.Launch(true);
                            }

                            string outputPathFinal = Path.Combine(outputPath, language);
                            Directory.CreateDirectory(outputPathFinal);

                            string outputFile = Path.Combine(outputPathFinal, device.Name + "." + device.iOSVersion);




                            var lang = string.IsNullOrWhiteSpace(language) ? "" : string.Format(" -AppleLanguages \"({0})\"", language);

                            Cecil.IncreaseFeatures.SetAdditionalLaunchParametersForiOS(lang);

                            Xamarin.UITest.IApp app = Xamarin.UITest.ConfigureApp.iOS.EnableLocalScreenshots()
                                   .AppBundle(appPath)
                                   .DeviceIdentifier(device.UDID)
                                   .StartApp();

                            iOS.SimulatorHelpers.DisableHardwareKeyboard();
                            iOS.SimulatorHelpers.ResetScale();

                            //This is harmless but forces the app to be present                        
                            app.DismissKeyboard();
                            simulatorTest(app, outputFile);
                            Console.WriteLine("Completed Simulation");
                            app = null;
                        }

                    } catch (Exception ex) {
                        Console.WriteLine(string.Format("Could not run test: {0}", ex.Message));
                        Console.WriteLine(ex.ToString());
                    }

                    if (Utilities.IsSet(Config.Global.iOS.SimulatorStatusMagicPath)) {
                        //Reset the statusbar
                        Console.WriteLine("Reinstallating StatusMagic");
                        StatusMagic.Install("iOS"); //Close the magic app by reinstalling
                        Console.WriteLine("Launching StatusMagic");
                        StatusMagic.Launch(false);
                    }

                }

                Console.WriteLine("Done");

            } else {
                Console.WriteLine(string.Format("An invalid iOS devices path was provided: {0}", Config.Global.iOS.DevicesPath));
            }

            iOS.SimulatorHelpers.ShutdownSimulator();

        }



    }
}
