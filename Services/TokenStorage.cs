using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using chession.Models;

namespace chession.Services;

public class TokenStorage : ITokenStorage
{
    private static readonly string AppName = "chession";
    private static readonly string TokenFileName = "token.json";
    
    private readonly string _tokenFilePath;

    public TokenStorage()
    {
        var appDataPath = GetAppDataPath();
        _tokenFilePath = Path.Combine(appDataPath, TokenFileName);
    }

    public async Task<TokenData?> GetTokenAsync()
    {
        if (!File.Exists(_tokenFilePath))
            return null;

        var json = await File.ReadAllTextAsync(_tokenFilePath);
        return JsonSerializer.Deserialize<TokenData>(json);
    }

    public async Task StoreTokenAsync(string token)
    {
        var directory = Path.GetDirectoryName(_tokenFilePath)!;
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var data = new TokenData(token, DateTimeOffset.UtcNow);
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_tokenFilePath, json);
    }

    public Task ClearTokenAsync()
    {
        if (File.Exists(_tokenFilePath))
        {
            File.Delete(_tokenFilePath);
        }
        return Task.CompletedTask;
    }

    private static string GetAppDataPath()
    {
        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(basePath, AppName);
    }
}
