using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using chession.Models;
using LichessSharp;
using LichessSharp.Models.Games;
using LichessSharp.Models.Users;

namespace chession.Services;

public interface ILichessService : IDisposable
{
    Task<UserExtended> GetProfileAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CurrentGame>> GetCurrentGamesAsync(
        int count = 9,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<MoveStreamEvent> StreamGameMovesAsync(
        string gameId,
        CancellationToken cancellationToken = default);

    Task<Game> GetGameAsync(string gameId, CancellationToken cancellationToken = default);
}
