using System.Security.Cryptography;
using System.Text;

namespace Menu.Services;

public class PasswordService
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;
    private const string Prefix = "PBKDF2";

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("La contraseña no puede estar vacía.", nameof(password));

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
            return false;

        if (!hash.StartsWith($"{Prefix}$", StringComparison.Ordinal))
            return VerifyLegacySha256(password, hash);

        var parts = hash.Split('$');

        if (parts.Length != 4 || !int.TryParse(parts[1], out var iterations))
            return false;

        var salt = Convert.FromBase64String(parts[2]);
        var expectedKey = Convert.FromBase64String(parts[3]);
        var actualKey = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            expectedKey.Length);

        return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
    }

    public bool NeedsRehash(string hash)
    {
        return !hash.StartsWith($"{Prefix}${Iterations}$", StringComparison.Ordinal);
    }

    private static bool VerifyLegacySha256(string password, string hash)
    {
        using var sha256 = SHA256.Create();

        var bytes = Encoding.UTF8.GetBytes(password);
        var passwordHash = Convert.ToBase64String(sha256.ComputeHash(bytes));

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(passwordHash),
            Encoding.UTF8.GetBytes(hash));
    }
}
