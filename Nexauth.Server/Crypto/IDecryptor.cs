namespace Nexauth.Server.Crypto {
    public interface IDecryptor {
        byte[] Decrypt(byte[] data);
    }
}