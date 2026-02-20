using System.Threading.Tasks;
using chession.Models;

namespace chession.Services;

public readonly record struct TokenStorageResult(bool Success, string? ErrorMessage = null);

public interface ITokenStorage
{
    Task<TokenData?> GetTokenAsync();
    Task<TokenStorageResult> StoreTokenAsync(string token);
    Task<TokenStorageResult> ClearTokenAsync();
}
