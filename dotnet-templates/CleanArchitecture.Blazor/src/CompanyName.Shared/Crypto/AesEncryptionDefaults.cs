namespace CompanyName.Shared.Crypto;

public static class AesEncryptionDefaults
{
    public const string DefaultKeyAppSetting = "App:Key";
    public const int KeySizeBytes = 32;
    public const int IvSizeBytes = 16;
    public const int AesBlockSizeBytes = 16;
}
