namespace Nexauth.Server.Models {
    public class AuthResponse {
        public long reqId { get; set; }
        public long userId { get; set; }
        public string challenge { get; set; }
        public string signedChallenge { get; set; }

    }
}
