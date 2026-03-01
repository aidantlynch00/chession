using System.Threading.Tasks;
using chession.Models;

namespace chession.Services;

/// <summary>
/// Interface for persisting the Lichess API token.
/// </summary>
public interface ITokenStorage
{
    /// <summary>
    /// Gets the stored token, if it exists.
    /// </summary>
    /// <returns>The stored token data, or null if not found.</returns>
    Task<TokenData?> GetTokenAsync();

    /// <summary>
    /// Stores the token securely.
    /// </summary>
    /// <param name="token">The token to store.</param>
    Task StoreTokenAsync(string token);

    /// <summary>
    /// Clears the stored token.
    /// </summary>
    Task ClearTokenAsync();
}
