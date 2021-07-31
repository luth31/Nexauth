using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nexauth_server.Models {
    public class AuthResponse {
        public long reqId { get; set; }
        public long userId { get; set; }
        public string challenge { get; set; }
        public string signedChallenge { get; set; }

    }
}
