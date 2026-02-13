using System.Security.Cryptography;
using System.Text;

public static class TokenHashHelper
{
    public static string Hash(string token)
    {
        using var sha256 = SHA256.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(token);
        byte[] hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }
}
