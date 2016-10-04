using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
                    addsim = !all.Any(s => s.iOSVersion.CompareTo(simulator.iOSVersion) >= 0 && s.Type == simulator.Type);
                    all.RemoveAll(s => s.iOSVersion.CompareTo(simulator.iOSVersion) < 0 && s.Type == simulator.Type);
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

        internal static void TypeToSimulator(string text) {
            iOSHelpers.ExecuteApplescript("tell application \"Simulator\" to activate (windows where name contains \"iOS\")");
            System.Threading.Thread.Sleep(1000);
            iOSHelpers.ExecuteApplescript("tell application \"System Events\" to keystroke \"" + text + "\"");
            System.Threading.Thread.Sleep(1500);
        }

        internal static void DisableHardwareKeyboard() {
            iOSHelpers.ExecuteApplescript(
 @"tell application ""System Events"" to tell process ""Simulator""
    tell menu item ""Keyboard"" of menu 1 of menu bar item ""Hardware"" of menu bar 1

        set v to(value of attribute ""AXMenuItemMarkChar"" of menu item ""Connect Hardware Keyboard"" of menu 1) as string

        if (v is not missing value) then
            click menu item ""Connect Hardware Keyboard"" of menu 1
        end if

    end tell
end tell");
        }

        internal static void EnableHardwareKeyboard() {
            iOSHelpers.ExecuteApplescript(
 @"tell application ""System Events"" to tell process ""Simulator""
    tell menu item ""Keyboard"" of menu 1 of menu bar item ""Hardware"" of menu bar 1

        set v to(value of attribute ""AXMenuItemMarkChar"" of menu item ""Connect Hardware Keyboard"" of menu 1) as string

        if (v is missing value) then
            click menu item ""Connect Hardware Keyboard"" of menu 1
        end if

    end tell
end tell");
        }

        internal static void ResetScale() {
            iOSHelpers.ExecuteApplescript("tell application \"System Events\" to tell process \"Simulator\" to click menu item 1 of menu 1 of menu item \"Scale\" of menu 1 of menu bar item \"Window\" of menu bar 1");
            System.Threading.Thread.Sleep(500);

            iOSHelpers.ExecuteApplescript("tell application \"Simulator\" to activate (windows where name contains \"iOS\")");
            System.Threading.Thread.Sleep(500);
        }

        internal static void CleanSimulator(string udid) {
            Utilities.Run("xcrun", string.Format("simctl erase '{0}'", udid));
        }

        internal static void DisableAutocorrect(string udid) {
            WriteDefaults(udid, "KeyboardAutocapitalization -bool NO");
            WriteDefaults(udid, "KeyboardCheckSpelling -bool NO");
            WriteDefaults(udid, "KeyboardAutocorrection -bool NO");
            WriteDefaults(udid, "KeyboardPrediction -bool NO");
            WriteDefaults(udid, "KeyboardShowPredictionBar -bool NO");
        }

        internal static void WriteDefaults(string udid, string setting) {
            var path = "~/Library/Developer/CoreSimulator/Devices/{0}/data/Library/Preferences/com.apple.Preferences.plist".ExpandPath();
            var args = string.Format("write " + path + " {1}", udid, setting);
            Console.WriteLine("Set Defaults: " + setting);
            var output = Utilities.RunWithOutput("defaults", args);

        }
        internal static void Install(string appPath) {
            Utilities.Run("xcrun", string.Format("simctl install booted {0}", appPath));
            System.Threading.Thread.Sleep(2000);
        }

        internal static void Uninstall(string bundleid) {
            Utilities.Run("xcrun", string.Format("simctl uninstall booted {0}", bundleid));
            System.Threading.Thread.Sleep(2000);
        }

        internal static void Launch(string bundleid) {
            Launch(bundleid, "", "", null);
        }
        internal static void Launch(string bundleid, Dictionary<string, string> environmentVariables) {
            Launch(bundleid, "", "", environmentVariables);
        }

        internal static void Launch(string bundleid, string languageId) {
            Launch(bundleid, languageId, "", null);
        }

        internal static void Launch(string bundleid, string languageId, Dictionary<string, string> environmentVariables) {
            Launch(bundleid, languageId, "", environmentVariables);
        }
        internal static void Launch(string bundleid, string languageId, string additionalArguments) {
            Launch(bundleid, languageId, additionalArguments, null);
        }
        internal static void Launch(string bundleid, string languageId, string additionalArguments, Dictionary<string, string> environmentVariables) {

            var lang = string.IsNullOrWhiteSpace(languageId) ? "" : string.Format(" -AppleLanguages \"({0})\"", languageId);

            Utilities.Run("xcrun", string.Format("simctl launch booted {0}{1}{2}", bundleid, lang, additionalArguments), null, false, environmentVariables);
            System.Threading.Thread.Sleep(2000);
        }

        internal static void CleanAllSimulators(string udid) {
            Utilities.Run("xcrun", "simctl erase all");
        }

        internal static string GetBundleIdentifierFromApp(string appPath) {
            return iOSHelpers.ExecuteApplescriptWithOutput("id of app \"" + appPath + "\"");
        }

        internal static void ShutdownSimulator() {
            Utilities.Run("xcrun", string.Format("simctl shutdown booted"));
            System.Threading.Thread.Sleep(2000);
            Utilities.Run("killall", "\"iOS Simulator\"");
            Utilities.Run("killall", "\"Simulator\"");
        }

        internal static void StartSimulator(string udid) {
            Utilities.Run("xcrun", string.Format("instruments -w {0}", udid));
        }

        internal static void SetLanguageAndRegion(string locale) {
            /*        
                "en_US" => {"AppleLanguages" => "en", "AppleLocale" => "en_US"},
                global_pref_path=sim_path+"/data/Library/Preferences/.GlobalPreferences.plist"
                `/usr/libexec/PlistBuddy #{global_pref_path} -c "Add :AppleLanguages:0 string '#{LANG_HASH["#{locale}"]["AppleLanguages"]}'"`
                `/usr/libexec/PlistBuddy #{global_pref_path} -c "Set :AppleLocale '#{LANG_HASH["#{locale}"]["AppleLocale"]}'"`
            */
        }

    }

}
