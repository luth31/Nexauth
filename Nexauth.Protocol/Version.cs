using System;
using System.Linq;

namespace Nexauth.Protocol {
    public static class Version {

        public bool IsSupported(string Version) {
            var versionArray = Version.Split('.').Select(int.Parse).ToArray();
            if (versionArray.Length == 3) {
                int major, minor, patch = versionArray;
                IsSupported(major, minor, patch);
            }
            return false;
        }

        public bool IsSupported(int Major, int Minor, int Patch) {
            if (this.Major != Major || this.Minor != Minor)
                return false;
            return true;
        }

        public int Major {
            get {
                return 0;
            }
        }

        public int Minor {
            get {
                return 1;
            }
        }

        public int Patch {
            get {
                return 0;
            }
        }

        public string VersionString {
            get {
                return String.Concat(Major, ".", Minor, ".", Hotfix);
            }
        }
    }
}