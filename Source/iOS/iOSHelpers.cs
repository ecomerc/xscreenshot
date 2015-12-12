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

        internal static void ExecuteApplescript(string script) {

            Utilities.Run("osascript", string.Format("-e '{0}' 0", script));

        }


        internal static void RunSimulatorsForAction(Action<Xamarin.UITest.IApp, string> simulatorTest) {
            string path = ((string)Config.Global.iOS.DevicesPath).ExpandPath();
            if (Directory.Exists(path)) {
                Console.WriteLine(string.Format("Loading devices from: {0}", path));


                IEnumerable<string> excludedDevices = null; ;
                if (Utilities.IsSet(Config.Global.iOS.ExcludeDevices) && Config.Global.iOS.ExcludeDevices.Length > 0) {
                    excludedDevices = Config.Global.iOS.ExcludeDevices;
                }
                IEnumerable<string> includeDevices = null;
                if (Utilities.IsSet(Config.Global.iOS.IncludeDevices) && Config.Global.iOS.IncludeDevices.Length > 0) {
                    includeDevices = Config.Global.iOS.IncludeDevices;
                }

                iOS.SimulatorHelpers.ShutdownSimulator();

                var devices = iOS.SimulatorHelpers.LoadSimulators(path, includeDevices, excludedDevices, Config.Global.iOS.OSVersion);


                foreach (Simulator device in devices) {
                    Console.WriteLine(string.Format("Beginning screenshots for device: {0} {1} {2}", device.Name, device.UDID, device.iOSVersion));


                    iOS.SimulatorHelpers.CleanSimulator(device.UDID);

                    if (Utilities.IsSet(Config.Global.iOS.SimulatorStatusMagicPath)) {
                        var statusMagic = StatusMagic.Build(Config.Global.iOS.SimulatorStatusMagicPath, device.UDID);
                        SimulatorHelpers.StartSimulator(device.UDID);
                        StatusMagic.Install();
                        StatusMagic.Launch();
                    }

                    string outputPath = ((string)Config.Global.iOS.OutputPath).ExpandPath();
                    string outputFile = Path.Combine(outputPath, device.Name + "." + device.iOSVersion);
                    string appPath = ((string)Config.Global.iOS.AppPath).ExpandPath();

                    Directory.CreateDirectory(outputPath);

                    try {


                        Xamarin.UITest.IApp app = Xamarin.UITest.ConfigureApp.iOS.EnableLocalScreenshots()
                               .AppBundle(appPath)
                               .DeviceIdentifier(device.UDID)
                           .StartApp();

                        iOS.SimulatorHelpers.DisableHardwareKeyboard();
                        iOS.SimulatorHelpers.ResetScale();

                        simulatorTest(app, outputPath);

                        app = null;

                    } catch (Exception ex) {
                        Console.WriteLine(string.Format("Could not run test: {0}", ex.Message));
                    }

                    if (Utilities.IsSet(Config.Global.iOS.SimulatorStatusMagicPath)) {
                        //Reset the statusbar
                        StatusMagic.Install();
                        StatusMagic.Launch();
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
