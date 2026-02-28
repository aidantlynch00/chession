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

public class LichessService : ILichessService
{
    private readonly LichessClient _client;
    private readonly HttpClient _httpClient;
    private bool _disposed;

    public LichessService(string token)
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        var options = new LichessClientOptions { AccessToken = token };
        _client = new LichessClient(_httpClient, options);
    }

    public Task<UserExtended> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        return _client.Account.GetProfileAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CurrentGame>> GetCurrentGamesAsync(
        int count = 9,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"/api/account/playing?nb={count}", cancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<CurrentGamesResponse>(json);
        return result?.NowPlaying ?? [];
    }

    public IAsyncEnumerable<MoveStreamEvent> StreamGameMovesAsync(
        string gameId,
        CancellationToken cancellationToken = default)
    {
        return _client.Games.StreamGameMovesAsync(gameId, cancellationToken);
    }

    public async Task<Game> GetGameAsync(string gameId, CancellationToken cancellationToken = default)
    {
        return await _client.Games.ExportAsync(gameId, cancellationToken: cancellationToken);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _client.Dispose();
        _httpClient.Dispose();
    }
}
