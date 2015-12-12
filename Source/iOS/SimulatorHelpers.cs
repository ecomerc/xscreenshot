using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using xscreenshot.Extensions;

namespace xscreenshot.iOS {
	internal class SimulatorHelpers {
		internal static IEnumerable<Simulator> LoadSimulators(string path, IEnumerable<string> includeDeviceTypes, IEnumerable<string> excludeDeviceTypes, string iOSVersion) {
			var all = new List<Simulator>();

			foreach (var file in Directory.EnumerateFiles(path, "device.plist", SearchOption.AllDirectories)) {
				var simulator = LoadSimulator(file);

				var addsim = true;
				if (iOSVersion == "latest") {
					addsim = !all.Any (s => s.iOSVersion.CompareTo(simulator.iOSVersion) >= 0 && s.Type == simulator.Type );
					all.RemoveAll (s => s.iOSVersion.CompareTo(simulator.iOSVersion) < 0  && s.Type == simulator.Type);
				} else if (iOSVersion != "all") {
					addsim = (iOSVersion == simulator.iOSVersion);
				}

				if (addsim)
					all.Add(simulator);
			}

			IEnumerable<Simulator> list = all;

			if (excludeDeviceTypes != null && excludeDeviceTypes.Count() > 0)
				list = list.Where(s => !s.Type.ContainsListAny(excludeDeviceTypes));


			if (includeDeviceTypes != null && includeDeviceTypes.Count() > 0)
				list = list.Where(s => s.Type.ContainsListAny(includeDeviceTypes));

			return list;
		}

		internal static Simulator LoadSimulator(string plistFile) {
			var plistDoc = new XmlDocument();
			plistDoc.LoadXml(File.ReadAllText(plistFile));
			var dictNodes = plistDoc.GetElementsByTagName("dict").Item(0).ChildNodes;

			var name = "";
			var udid = "";
			var type = "";
			var ios = "";

			for (var i = 0; i < dictNodes.Count; i++) {
				if (dictNodes[i].Name == "key" && dictNodes[i].InnerText == "name") {
					name = dictNodes[i + 1].InnerText;
				}
				if (dictNodes[i].Name == "key" && dictNodes[i].InnerText == "UDID") {
					udid = dictNodes[i + 1].InnerText;
				}
				if (dictNodes[i].Name == "key" && dictNodes[i].InnerText == "deviceType") {
					type = dictNodes[i + 1].InnerText;
					type = type.Split('.').Reverse().First();
				}
				if (dictNodes[i].Name == "key" && dictNodes[i].InnerText == "runtime") {
						ios = dictNodes[i + 1].InnerText;
						ios = ios.Split('.').Reverse().First();
				}
			}
			return new Simulator() {
				Name = name,
				UDID = udid,
				iOSVersion = ios,
				Type = type
			};
		}

        internal static void DisableHardwareKeyboard() {
            iOSHelpers.ExecuteApplescript(
 @"tell application ""System Events"" to tell process ""Simulator""
    tell menu item ""Keyboard"" of menu 1 of menu bar item ""Hardware"" of menu bar 1

        set v to(value of attribute ""AXMenuItemMarkChar"" of menu item ""Connect Hardware Keyboard"" of menu 1) as string

        if (v = """ + '\x2713' + @""") then
            click menu item ""Connect Hardware Keyboard"" of menu 1
        end if

    end tell
end tell");
        }

        internal static void ResetScale() {
            iOSHelpers.ExecuteApplescript("tell application \"System Events\" to tell process \"Simulator\" to click menu item 1 of menu 1 of menu item \"Scale\" of menu 1 of menu bar item \"Window\" of menu bar 1");
        }

        internal static void CleanSimulator(string udid) {
            Utilities.Run("xcrun", string.Format("simctl erase '{0}'", udid));
        }

        internal static void CleanAllSimulators(string udid) {
            Utilities.Run("xcrun", "simctl erase all");
        }

        internal static void ShutdownSimulator() {
            Utilities.Run("killall", "\"iOS Simulator\"");
            Utilities.Run("killall", "\"Simulator\"");
        }

        internal static void StartSimulator(string udid) {
            Utilities.Run("xcrun", string.Format("instruments -w {0}", udid));
        }
       

    }
	
}
