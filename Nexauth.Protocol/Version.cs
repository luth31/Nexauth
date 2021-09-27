using System;
using System.Linq;

namespace Nexauth.Protocol {
    public static class Version {

        public static bool IsSupported(string Version) {
            try {
                int[] versionArray = Version.Split('.').Select(int.Parse).ToArray();
                if (versionArray.Length == 3) {
                    int major = versionArray[0];
                    int minor = versionArray[1];
                    int patch = versionArray[2];
                    return IsSupported(major, minor, patch);
                }
            } catch {
                return false;
            }
            return false;
        }

        public static bool IsSupported(int Major, int Minor, int Patch) {
            if (Version.Major != Major || Version.Minor != Minor)
                return false;
            return true;
        }

        public static int Major {
            get {
                return 0;
            }
        }

        public static int Minor {
            get {
                return 1;
            }
        }

        public static int Patch {
            get {
                return 0;
            }
        }

        public static string VersionString {
            get {
                return String.Concat(Major, ".", Minor, ".", Hotfix);
            }
        }
    }
}