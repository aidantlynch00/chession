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

        try
        {
            var json = await File.ReadAllTextAsync(_tokenFilePath);
            var data = JsonSerializer.Deserialize<TokenData>(json);
            return data;
        }
        catch
        {
            return null;
        }
    }

    public async Task<TokenStorageResult> StoreTokenAsync(string token)
    {
        try
        {
            var directory = Path.GetDirectoryName(_tokenFilePath)!;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var data = new TokenData(token, DateTimeOffset.UtcNow);
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_tokenFilePath, json);
            
            return new TokenStorageResult(true);
        }
        catch (UnauthorizedAccessException ex)
        {
            return new TokenStorageResult(false, $"Permission denied: {ex.Message}");
        }
        catch (IOException ex)
        {
            return new TokenStorageResult(false, $"IO error: {ex.Message}");
        }
    }

    public Task<TokenStorageResult> ClearTokenAsync()
    {
        try
        {
            if (File.Exists(_tokenFilePath))
            {
                File.Delete(_tokenFilePath);
            }
            return Task.FromResult(new TokenStorageResult(true));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Task.FromResult(new TokenStorageResult(false, $"Permission denied: {ex.Message}"));
        }
        catch (IOException ex)
        {
            return Task.FromResult(new TokenStorageResult(false, $"IO error: {ex.Message}"));
        }
    }

    private static string GetAppDataPath()
    {
        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(basePath, AppName);
    }
}
