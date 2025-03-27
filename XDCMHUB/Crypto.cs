using System.Security.Cryptography;
using System.Text;

namespace XDCMHUB;

public class Crypto
{
    //AES Encryption
    public static string EncryptString(string plainText, string keyToUse)
    {
        byte[] iv = new byte[16];
        byte[] array;

        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(Get128BitString(keyToUse));
            aes.IV = iv;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using MemoryStream memoryStream = new();
            using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
            using (StreamWriter streamWriter = new(cryptoStream))
            {
                streamWriter.Write(plainText);
            }

            array = memoryStream.ToArray();
        }

        return Convert.ToBase64String(array);
    }

    public static string DecryptString(string cipherText, string keyToUse)
    {
        byte[] iv = new byte[16];
        byte[] buffer = Convert.FromBase64String(cipherText);

        using Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(Get128BitString(keyToUse));
        aes.IV = iv;
        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using MemoryStream memoryStream = new(buffer);
        using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
        using StreamReader streamReader = new(cryptoStream);
        return streamReader.ReadToEnd();
    }

    public static string Get128BitString(string keyToConvert)
    {
        StringBuilder b = new StringBuilder();
        for (int i = 0; i < 16; i++)
        {
            b.Append(keyToConvert[i % keyToConvert.Length]);
        }
        keyToConvert = b.ToString();

        return keyToConvert;
    }
}
