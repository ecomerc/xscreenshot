using System;
using System.Collections.Generic;

namespace xscreenshot.Extensions {
    public static class StringExtensions {

        public static bool ContainsListAny(this string @string, IEnumerable<string> list, bool ignoreCase = true) {
            if (string.IsNullOrEmpty(@string))
                return false;

            foreach (var item in list) {
                if (ignoreCase && @string.ToLowerInvariant().Contains(item.ToLowerInvariant())) {
                    return true;
                } else if (@string.Contains(item)) {
                    return true;
                }
            }
            return false;
        }


        public static string ExpandPath(this string path) {
            return ExpandPath(path, "");
        }
        public static string ExpandPath(this string path, string currentPath) {

            if (path.StartsWith("~/"))
                return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), path.Substring(2));

            if (!string.IsNullOrWhiteSpace(currentPath)) {
                if (path.StartsWith("./"))
                    path = path.Substring(2);

                return Path.GetFullPath(System.IO.Path.Combine(currentPath, path));
            }

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
