using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace xscreenshot {
    public class SimulatorHelpers {
        public static IEnumerable<Simulator> LoadSimulators(string path, IEnumerable<string> includeDeviceTypes, IEnumerable<string> excludeDeviceTypes, string iOSVersion) {
            var all = new List<Simulator>();

            foreach (var file in Directory.EnumerateFiles(path, "device.plist", SearchOption.AllDirectories)) {
                var simulator = LoadSimulator(file);
                Console.WriteLine(string.Format("Found simulator: {0}, type {1}, ios {2}, udid {3}", simulator.Name , simulator.Type, simulator.iOSVersion , simulator.UDID));
                
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
                list = list.Where(s => !StringContainsList(s.Type, excludeDeviceTypes));


            if (includeDeviceTypes != null && includeDeviceTypes.Count() > 0)
                list = list.Where(s => StringContainsList(s.Type, includeDeviceTypes));

            return list;
        }

        public static Simulator LoadSimulator(string plistFile) {
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

        public static bool StringContainsList(string @string, IEnumerable<string> list) {
            if (string.IsNullOrEmpty(@string))
                return false;

            foreach (var item in list) {
                if (@string.Contains(item)) {
                    return true;
                }
            }
            return false;
        }

    }

    public class Simulator {
        public string Name { get; set; }
        public string UDID { get; set; }
        public string Type { get; set; }
        public string iOSVersion { get; set; }
    }
}
