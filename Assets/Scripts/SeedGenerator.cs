using System.Security.Cryptography;

public static class SeedGenerator
{
    private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static string GenerateSeed(int length = 5)
    {
        byte[] data = new byte[length];
        RandomNumberGenerator.Fill(data);

        char[] result = new char[length];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Chars[data[i] % Chars.Length];
        }

        string seed = new string(result);

        return seed;
    }
}
