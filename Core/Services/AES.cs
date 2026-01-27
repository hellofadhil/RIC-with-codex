using System.Security.Cryptography;
using System.Text;

namespace Core.Services;

public static class AES
{
    public static string Encrypt(string data, string key)
    {
        using MemoryStream Memory = new();

        using Aes aes = Aes.Create();

        byte[] plainBytes = Encoding.UTF8.GetBytes(data);
        byte[] bufferKey = new byte[32];
        Array.Copy(Encoding.UTF8.GetBytes(key.PadRight(bufferKey.Length)), bufferKey, bufferKey.Length);

        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.PKCS7;
        aes.KeySize = 256;
        aes.Key = bufferKey;
        using CryptoStream cryptoStream = new(Memory, aes.CreateEncryptor(), CryptoStreamMode.Write);

        try
        {
            cryptoStream.Write(plainBytes, 0, plainBytes.Length);
            cryptoStream.FlushFinalBlock();
            return Convert.ToBase64String(Memory.ToArray());
        }
        catch (Exception)
        {
            return "";
        }
    }
    public static string Decrypt(string data, string key)
    {
        byte[] encryptedBytes = Convert.FromBase64String(data);
        byte[] bufferKey = new byte[32];
        Array.Copy(Encoding.UTF8.GetBytes(key.PadRight(bufferKey.Length)), bufferKey, bufferKey.Length);

        using MemoryStream Memory = new(encryptedBytes);
        using Aes aes = Aes.Create();
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.PKCS7;
        aes.KeySize = 256;
        aes.Key = bufferKey;
        using CryptoStream cryptoStream = new(Memory, aes.CreateDecryptor(), CryptoStreamMode.Read);

        string response = "";
        try
        {
            using var plainTextReader = new StreamReader(cryptoStream);
            response = plainTextReader.ReadToEnd();
        }
        catch (Exception e)
        {
            response = e.Message;
        }
        return response;
    }
}
