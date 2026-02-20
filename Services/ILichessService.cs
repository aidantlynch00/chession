using System.Threading;
using System.Threading.Tasks;
using LichessSharp.Models.Users;

namespace chession.Services;

public interface ILichessService
{
    Task<User> GetProfileAsync(CancellationToken cancellationToken = default);
}
