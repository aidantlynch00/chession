using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using chession.Models;
using LichessSharp;
using LichessSharp.Models.Games;
using LichessSharp.Models.Users;

namespace chession.Services;

/// <summary>
/// Implementation of the Lichess service using LichessSharp client.
/// </summary>
public class LichessService : ILichessService
{
    private readonly LichessClient _client;
    private readonly HttpClient _httpClient;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the LichessService class.
    /// </summary>
    /// <param name="token">The Lichess API token.</param>
    public LichessService(string token)
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        var options = new LichessClientOptions { AccessToken = token };
        _client = new LichessClient(_httpClient, options);
    }

    /// <inheritdoc />
    public Task<UserExtended> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        return _client.Account.GetProfileAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CurrentGame>> GetCurrentGamesAsync(
        int count = 9,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"/api/account/playing?nb={count}", cancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize(json, ChessionJsonContext.Default.CurrentGamesResponse);
        return result?.NowPlaying ?? [];
    }

    /// <inheritdoc />
    public IAsyncEnumerable<MoveStreamEvent> StreamGameMovesAsync(
        string gameId,
        CancellationToken cancellationToken = default)
    {
        return _client.Games.StreamGameMovesAsync(gameId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Game> GetGameAsync(string gameId, CancellationToken cancellationToken = default)
    {
        return await _client.Games.ExportAsync(gameId, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _client.Dispose();
        _httpClient.Dispose();
    }
}
