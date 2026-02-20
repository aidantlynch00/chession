using System;
using System.Threading;
using System.Threading.Tasks;
using LichessSharp;
using LichessSharp.Models.Users;

namespace chession.Services;

public class LichessService : ILichessService
{
    private readonly LichessClient _client;

    public LichessService(string token)
    {
        _client = new LichessClient(token);
    }

    public async Task<User> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        return await _client.Account.GetProfileAsync(cancellationToken);
    }
}
