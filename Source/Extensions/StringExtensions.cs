using System;
using System.Collections.Generic;

namespace xscreenshot.Extensions {
    public static class StringExtensions {

        public static bool ContainsListAny(this string @string, IEnumerable<string> list) {
            if (string.IsNullOrEmpty(@string))
                return false;

            foreach (var item in list) {
                if (@string.Contains(item)) {
                    return true;
                }
            }
            return false;
        }


        public static string ExpandPath(this string path) {

            if (path.StartsWith("~/"))
                path = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + path.Substring(1);

            return path;
        }

        public static string GetHome() {

            
            string homePath = (Environment.OSVersion.Platform == PlatformID.Unix ||
                   Environment.OSVersion.Platform == PlatformID.MacOSX)
                            ? Environment.GetEnvironmentVariable("HOME")
                            : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
            return homePath;
        }

    }
}
