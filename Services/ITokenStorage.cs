using System.Threading.Tasks;
using chession.Models;

namespace chession.Services;

public interface ITokenStorage
{
    Task<TokenData?> GetTokenAsync();
    Task StoreTokenAsync(string token);
    Task ClearTokenAsync();
}
