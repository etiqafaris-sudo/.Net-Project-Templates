using System.Security.Cryptography;
using System.Text;

namespace CompanyName.Shared.Crypto;

internal static class AesEncryptionCore
{
    public static string Encrypt(string plainText, string key)
    {
        ArgumentNullException.ThrowIfNull(plainText);

        byte[] keyBytes = GetKeyBytes(key);
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] payload = Encrypt(plainBytes, keyBytes);
        return Convert.ToBase64String(payload);
    }

    public static string Decrypt(string cipherTextBase64, string key)
    {
        if (string.IsNullOrEmpty(cipherTextBase64))
        {
            throw new ArgumentException("Ciphertext is required.", nameof(cipherTextBase64));
        }

        byte[] keyBytes = GetKeyBytes(key);
        byte[] payload = Convert.FromBase64String(cipherTextBase64);
        byte[] plainBytes = Decrypt(payload, keyBytes);
        return Encoding.UTF8.GetString(plainBytes);
    }

    public static byte[] Encrypt(byte[] plainText, byte[] key)
    {
        ArgumentNullException.ThrowIfNull(plainText);
        ValidateKeyBytes(key);

        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = key;

        byte[] iv = new byte[AesEncryptionDefaults.IvSizeBytes];
        RandomNumberGenerator.Fill(iv);
        aes.IV = iv;

        using ICryptoTransform encryptor = aes.CreateEncryptor();
        byte[] cipherBytes = encryptor.TransformFinalBlock(plainText, 0, plainText.Length);
        byte[] result = new byte[AesEncryptionDefaults.IvSizeBytes + cipherBytes.Length];
        Buffer.BlockCopy(iv, 0, result, 0, AesEncryptionDefaults.IvSizeBytes);
        Buffer.BlockCopy(cipherBytes, 0, result, AesEncryptionDefaults.IvSizeBytes, cipherBytes.Length);
        return result;
    }

    public static byte[] Decrypt(byte[] payload, byte[] key)
    {
        ArgumentNullException.ThrowIfNull(payload);
        ValidateKeyBytes(key);

        int minimumLength = AesEncryptionDefaults.IvSizeBytes + AesEncryptionDefaults.AesBlockSizeBytes;
        if (payload.Length < minimumLength)
        {
            throw new ArgumentException(
                $"Ciphertext must be at least {minimumLength} bytes (IV plus one AES block).",
                nameof(payload));
        }

        byte[] iv = new byte[AesEncryptionDefaults.IvSizeBytes];
        Buffer.BlockCopy(payload, 0, iv, 0, AesEncryptionDefaults.IvSizeBytes);

        int cipherLength = payload.Length - AesEncryptionDefaults.IvSizeBytes;
        byte[] cipherBytes = new byte[cipherLength];
        Buffer.BlockCopy(payload, AesEncryptionDefaults.IvSizeBytes, cipherBytes, 0, cipherLength);

        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = key;
        aes.IV = iv;

        using ICryptoTransform decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
    }

    /// <summary>
    /// Resolves a 32-byte AES key from a passphrase (SHA-256) or, if the value decodes to exactly 32 bytes, raw Base64 key material.
    /// </summary>
    public static byte[] GetKeyBytes(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Encryption key is required.", nameof(key));
        }

        string trimmed = key.Trim();
        if (TryDecodeRawKey(trimmed, out byte[]? rawKey) && rawKey is not null)
        {
            return rawKey;
        }

        return SHA256.HashData(Encoding.UTF8.GetBytes(trimmed));
    }

    private static bool TryDecodeRawKey(string value, out byte[]? key)
    {
        key = null;
        try
        {
            byte[] decoded = Convert.FromBase64String(value);
            if (decoded.Length == AesEncryptionDefaults.KeySizeBytes)
            {
                key = decoded;
                return true;
            }
        }
        catch (FormatException)
        {
        }

        return false;
    }

    private static void ValidateKeyBytes(byte[] key)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (key.Length != AesEncryptionDefaults.KeySizeBytes)
        {
            throw new ArgumentException(
                $"Encryption key must be {AesEncryptionDefaults.KeySizeBytes} bytes ({AesEncryptionDefaults.KeySizeBytes * 8}-bit AES).",
                nameof(key));
        }
    }
}
