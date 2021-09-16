using System.Net;

namespace Nexauth.Networking {
    public static class Util {
        public static bool IsIPv4Valid(string Address) {
            // Special case, listen on all interfaces
            if (Address == "0.0.0.0")
                return true;
            string[] groups = Address.Split(".", 4);
            // 0.0.0.0/8 is used for source hosts, cant listen on IPs from this block
            if (groups[0] == "0")
                return false;
            if (IPAddress.TryParse(Address, out _))
                return true;
            return false;
        }
    }
}
