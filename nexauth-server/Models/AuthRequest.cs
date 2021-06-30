using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nexauth_server.Models {
    public class AuthRequest {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Challenge { get; set; }
        public bool Completed { get; set;  }
    }
}
