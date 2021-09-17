namespace Nexauth.Server.Crypto {
    public interface IEncryptor {
        byte[] Encrypt(byte[] data);
    }
}