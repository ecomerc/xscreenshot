
using System;
using System.Collections.Generic;
using System.IO;
using xscreenshot.Extensions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xscreenshot.iOS {
    class StatusMagic {


        internal static bool Build(string path, string type, IEnumerable<string> destinations) {
            path = path.ExpandPath();

            if (string.IsNullOrEmpty(path)) {
                return false;
            } else {

                var destination = " -destination 'platform=iOS Simulator,id=" +
                    string.Join("' -destination 'platform=iOS Simulator,id=", destinations) + "'";

                string xcodebuild = string.Format("-scheme SimulatorStatusMagic {0} -derivedDataPath '/tmp/xscreenshot_derived/" + type + "/' clean build", destination);
                //string defaultPipe = "| tee '/tmp/xcodelogs.log' | xcpretty";
                //string prefix = "set -o pipefail && ";

                var buildoutput = Utilities.RunWithOutput("xcodebuild", xcodebuild, path);
                File.WriteAllText("statusmagicbuild.log", buildoutput);

                //Utilities.Run("gym", string.Format("-s 'SimulatorStatusMagic' -o '{0}' -n '{1}'", "./", DefaultIpaName), sourcePath);
                return true;
            }
        }


        internal static void Install(string type) {
            SimulatorHelpers.Install( "/tmp/xscreenshot_derived/" + type + "/Build/Products/Debug-iphonesimulator/SimulatorStatusMagic.app");
         
        }

        internal static void Uninstall() {
            SimulatorHelpers.Uninstall("com.shinydevelopment.SimulatorStatusMagic");
        }


        internal static void Launch(bool enableModifications) {
            Environment.SetEnvironmentVariable("SIMCTL_CHILD_SIMULATOR_STATUS_MAGIC_OVERRIDES", enableModifications ? "enable" : "disable");
            SimulatorHelpers.Launch( "com.shinydevelopment.SimulatorStatusMagic", "");
        }


    }
}
