using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Text;

using System.Security.Cryptography;

namespace Nexauth.Server {
    public class ECDSAProvider {
        public static bool VerifySignature(string key, string data, string signature) {
            byte[] signedData = Convert.FromBase64String(signature);
            byte[] originalData = Encoding.UTF8.GetBytes(data);
            byte[] pubKey = Convert.FromBase64String(key);

            ECDsa ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

            ecdsa.ImportSubjectPublicKeyInfo(pubKey, out _);
            return ecdsa.VerifyData(originalData, signedData, HashAlgorithmName.SHA256); ;
        }
    }
}
