using System;
using System.Collections.Generic;
using System.IO;
using xscreenshot.Extensions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xscreenshot.iOS {
    class StatusMagic {
        

        internal static bool Build(string path, string deviceUdid) {
            path = path.ExpandPath();

            if (string.IsNullOrEmpty(path)) {
                return false;
            } else {

                string xcodebuild = string.Format("-scheme SimulatorStatusMagic -destination 'platform=iOS Simulator,id={0}' -derivedDataPath '/tmp/xscreenshot_derived/' clean build", deviceUdid);
                string defaultPipe = "| tee '/tmp/xcodelogs.log' | xcpretty";
                string prefix = "set -o pipefail && ";

                Utilities.Run("xcodebuild", xcodebuild, path);

                //Utilities.Run("gym", string.Format("-s 'SimulatorStatusMagic' -o '{0}' -n '{1}'", "./", DefaultIpaName), sourcePath);
                return true;
            }
        }


        internal static void Install() { 
            Utilities.Run("xcrun", string.Format("simctl install booted {0}", "/tmp/xscreenshot_derived/Build/Products/Debug-iphonesimulator/SimulatorStatusMagic.app"));
       
        }

        internal static void Launch() {     //TODO: launch magic app
            Utilities.Run("xcrun", string.Format("simctl launch booted {0}", "com.shinydevelopment.SimulatorStatusMagic"));

        }


    }
}
