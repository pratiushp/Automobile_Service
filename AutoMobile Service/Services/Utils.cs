using System.Security.Cryptography;

namespace AutoMobile_Service.Services;

public static class Utils
{
    private const char stringDelimeter = ':';

    public static string GetDirectoryPath()
    {
        return @"C:\Users\ASUS\source\repos\AutoMobile Service\AutoMobile Service\wwwroot\JSON";

    }

    public static string GetUsersFilePath()
    {
        return Path.Combine(GetDirectoryPath(), "Users.json");
    }

    public static string GetItemsFilePath()
    {
        return Path.Combine(GetDirectoryPath(), "Items.json");
    }
    public static string GetItemsHistoryFilePath()
    {
        return Path.Combine(GetDirectoryPath(), "Records.json");
    }

    public static string HashPassword(string password)
    {
        var saltSize = 16;
        int iterations = 100000;
        var keySize = 32;
        HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA256;

        byte[] salt = RandomNumberGenerator.GetBytes(saltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, hashAlgorithm, keySize);

        return string.Join(
            stringDelimeter,
            Convert.ToHexString(hash),
            Convert.ToHexString(salt),
            iterations,
            hashAlgorithm
        );
    }

    public static bool HashVerification(string password, string hashPassword)
    {
        string[] hashStrings = hashPassword.Split(stringDelimeter);
        byte[] hash = Convert.FromHexString(hashStrings[0]);
        byte[] salt = Convert.FromHexString(hashStrings[1]);
        int iterations = int.Parse(hashStrings[2]);
        HashAlgorithmName hashAlgorithm = new(hashStrings[3]);

        byte[] inputPasswordHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            hashAlgorithm,
            hash.Length
        );

        return CryptographicOperations.FixedTimeEquals(inputPasswordHash, hash);
    }
}
