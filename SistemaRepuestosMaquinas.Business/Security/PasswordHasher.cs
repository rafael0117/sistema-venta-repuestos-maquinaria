using System.Security.Cryptography;

namespace SistemaRepuestosMaquinas.Business.Security;

public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;
    private const string FormatPrefix = "PBKDF2";

    public static string Hash(string rawPassword)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(rawPassword, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
        return $"{FormatPrefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
    }

    public static bool Verify(string rawPassword, string storedHash, out bool requiresRehash)
    {
        requiresRehash = false;
        if (string.IsNullOrWhiteSpace(storedHash))
            return false;

        var parts = storedHash.Split('$');
        if (parts.Length != 4 || !string.Equals(parts[0], FormatPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!int.TryParse(parts[1], out var iterations) || iterations <= 0)
            return false;

        var salt = Convert.FromBase64String(parts[2]);
        var expectedKey = Convert.FromBase64String(parts[3]);
        var actualKey = Rfc2898DeriveBytes.Pbkdf2(rawPassword, salt, iterations, HashAlgorithmName.SHA256, expectedKey.Length);
        requiresRehash = iterations < Iterations;
        return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
    }
}
