using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace nexauth {
    public class AESProvider {
        public enum AES_KEY_SIZE {
            AES_KEY_128 = 4,
            AES_KEY_192 = 6,
            AES_KEY_256 = 8
        };

        [DllImport("litecrypt")]
        private static extern IntPtr AES_CTR_Init(AES_KEY_SIZE size, IntPtr key, ulong nonce);
        [DllImport("litecrypt")]
        private static extern void AES_GenCtrBlock(IntPtr ctx, IntPtr output);
        public AESProvider(AES_KEY_SIZE size, byte[] key, byte[] nonce) {
            Init(size, key, nonce);
        }

        private void Init(AES_KEY_SIZE size, byte[] key, byte[] nonce) {
            IntPtr key_ptr = Marshal.AllocHGlobal(16);
            Marshal.Copy(key, 0, key_ptr, 16);
            ulong lnonce = BitConverter.ToUInt64(nonce);
            AES_CTR_Ctx = AES_CTR_Init(size, key_ptr, lnonce);
            Marshal.FreeHGlobal(key_ptr);
            keystream = new byte[4096];
            GenKeystream(4096);
        }

        public byte[] Encrypt(byte[] data) {
            byte[] xor_data = new byte[data.Length];
            if (data.Length > idx) {
                GenKeystream(data.Length - idx);
            }
            for (int i = 0; i < data.Length; ++i) {
                xor_data[i] = (byte)(data[i] ^ keystream[i]);
            }
            if (idx - data.Length > 0) {
                Array.Copy(keystream, data.Length, keystream, 0, idx - data.Length);
                idx -= data.Length;
            }
            else {
                idx = 0;
            }
            return xor_data;
        }

        public byte[] Decrypt(byte[] data) {
            return Encrypt(data);
        }

        private void GenKeystream(int size) {
            IntPtr output = Marshal.AllocHGlobal(16);
            if (size < 16 || size + idx > 4096)
                size = 16;
            if (size % 16 != 0)
                size = size + (16 - (size % 16));
            if (size + idx > 4096)
                throw new Exception("Keystream overflow!");
            for (int i = 0; i < size / 16; ++i) {
                AES_GenCtrBlock(AES_CTR_Ctx, output);
                Marshal.Copy(output, keystream, idx, 16);
                idx += 16;
            }
        }
        int idx;
        byte[] keystream;
        IntPtr AES_CTR_Ctx;
    }
}
