namespace CompanyName.Application.Common.Interfaces;

public interface IEncryptionService
{
    string Encrypt(string plainText);

    string Decrypt(string cipherText);

    byte[] Encrypt(byte[] plainText);

    byte[] Decrypt(byte[] payload);
}
