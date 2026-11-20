using System.Security.Cryptography;
using System.Text;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Shared.Configurations;
using CompanyName.Shared.Crypto;
using Microsoft.Extensions.Options;

namespace CompanyName.Infrastructure.Services;

internal sealed class AesEncryptionService : IEncryptionService
{
    private readonly Func<AppSettings> _getSettings;

    public AesEncryptionService(IOptionsMonitor<AppSettings> appSettings)
    {
        _getSettings = () => appSettings.CurrentValue;
    }

    public AesEncryptionService(AppSettings appSettings)
    {
        _getSettings = () => appSettings;
    }

    public string Encrypt(string plainText) =>
        AesEncryptionCore.Encrypt(plainText, GetAppKey());

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
        {
            throw new ArgumentException("Ciphertext is required.", nameof(cipherText));
        }

        byte[] payload;
        try
        {
            payload = Convert.FromBase64String(cipherText);
        }
        catch (FormatException ex)
        {
            throw new CryptographicException("Ciphertext is not valid Base64.", ex);
        }

        return Encoding.UTF8.GetString(Decrypt(payload));
    }

    public byte[] Encrypt(byte[] plainText)
    {
        byte[] keyBytes = AesEncryptionCore.GetKeyBytes(GetAppKey());
        return AesEncryptionCore.Encrypt(plainText, keyBytes);
    }

    public byte[] Decrypt(byte[] payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        byte[] keyBytes = AesEncryptionCore.GetKeyBytes(GetAppKey());

        try
        {
            return AesEncryptionCore.Decrypt(payload, keyBytes);
        }
        catch (Exception ex) when (ex is CryptographicException or ArgumentException)
        {
            if (TryDecryptLegacy(payload, out byte[]? legacyPlain))
            {
                return legacyPlain;
            }

            throw new CryptographicException("Unable to decrypt ciphertext with AES or legacy encoding.", ex);
        }
    }

    private string GetAppKey()
    {
        var key = _getSettings().AppKey;
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("AppSettings:AppKey is required for encryption.");
        }

        return key;
    }

    private static bool TryDecryptLegacy(byte[] payload, out byte[] plainText)
    {
        plainText = Array.Empty<byte>();
        try
        {
            string decoded = Encoding.UTF8.GetString(payload);
            if (string.IsNullOrEmpty(decoded))
            {
                return false;
            }

            plainText = payload;
            return true;
        }
        catch (DecoderFallbackException)
        {
            return false;
        }
    }
}
