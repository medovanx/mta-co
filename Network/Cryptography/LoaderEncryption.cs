using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NightMare.Network.Cryptography
{
    public class LoaderEncryption
    {
        private static byte[] Key = { 12, 12, 215, 10, 20, 11, 60, 193, 11, 96, 53, 157, 71, 37, 150, 225, 86, 224, 178, 184, 230, 147, 79, 194, 160, 0, 99, 239, 218, 134, 179, 13, 247, 155, 237, 245, 165, 245, 128, 144 };
        public static void Encrypt(byte[] arr)
        {
            int length = Key.Length;
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] ^= Key[i % length];
                arr[i] ^= Key[(i + 1) % length];

            }
        }
        public static void Decrypt(byte[] arr, int size)
        {
            int length = Key.Length;
            for (int i = 0; i < size; i++)
            {
                arr[i] ^= Key[(i + 1) % length];
                arr[i] ^= Key[i % length];

            }
        }
    }
}