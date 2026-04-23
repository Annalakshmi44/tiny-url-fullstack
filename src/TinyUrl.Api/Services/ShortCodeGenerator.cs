using System.Security.Cryptography;

namespace TinyUrl.Api.Services;

public class ShortCodeGenerator : IShortCodeGenerator
{
    private const string Characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public string Generate(int length = 6)
    {
        return RandomNumberGenerator.GetString(Characters, length);
    }
}
