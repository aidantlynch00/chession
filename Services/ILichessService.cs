using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using chession.Models;
using LichessSharp;
using LichessSharp.Models.Games;
using LichessSharp.Models.Users;

namespace chession.Services;

/// <summary>
/// Interface for interacting with the Lichess API.
/// </summary>
public interface ILichessService : IDisposable
{
    /// <summary>
    /// Gets the authenticated user's profile.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user's profile information.</returns>
    Task<UserExtended> GetProfileAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the currently ongoing games.
    /// </summary>
    /// <param name="count">The maximum number of games to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of current games.</returns>
    Task<IReadOnlyList<CurrentGame>> GetCurrentGamesAsync(
        int count = 9,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams moves for a specific game.
    /// </summary>
    /// <param name="gameId">The ID of the game to stream.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An async enumerable of move events.</returns>
    IAsyncEnumerable<MoveStreamEvent> StreamGameMovesAsync(
        string gameId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the details of a completed game.
    /// </summary>
    /// <param name="gameId">The ID of the game.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The game details.</returns>
    Task<Game> GetGameAsync(string gameId, CancellationToken cancellationToken = default);
}
