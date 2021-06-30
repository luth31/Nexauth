using System.ComponentModel.DataAnnotations;

namespace nexauth_server.Models {
    public class User {
        public long Id { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Key { get; set; }
        [Required]
        public string Token { get; set; }
    }
}
