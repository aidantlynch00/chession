using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using chession.Models;
using chession.Services;
using LichessSharp.Exceptions;
using LichessSharp.Models.Games;
using LichessSharp.Models.Enums;
using Microsoft.Extensions.Logging;

namespace chession.ViewModels;

public class GameTracker : IDisposable
{
    private readonly ILichessService _lichessService;
    private readonly MainViewModel _mainViewModel;
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _activeGameStreams = new();
    private CancellationTokenSource? _pollingCts;
    private Timer? _pollTimer;
    private bool _disposed;

    public GameTracker(ILichessService lichessService, MainViewModel mainViewModel, ILogger logger)
    {
        _lichessService = lichessService;
        _mainViewModel = mainViewModel;
        _logger = logger;
    }

    private void StopAll()
    {
        _pollTimer?.Dispose();
        _pollTimer = null;

        _pollingCts?.Cancel();
        _pollingCts?.Dispose();
        _pollingCts = null;

        foreach (var cts in _activeGameStreams.Values)
        {
            cts.Cancel();
            cts.Dispose();
        }
        _activeGameStreams.Clear();
    }

    private void HandleAuthenticationFailure()
    {
        StopAll();
        _mainViewModel.OnAuthenticationFailed();
    }

    public async Task StartTrackingAsync()
    {
        _pollingCts = new CancellationTokenSource();

        await RefreshOngoingGamesAsync(_pollingCts.Token);

        _pollTimer = new Timer(
            async _ => await RefreshOngoingGamesAsync(_pollingCts.Token),
            null,
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(10));
    }

    private async Task RefreshOngoingGamesAsync(CancellationToken ct)
    {
        try
        {
            var games = await _lichessService.GetCurrentGamesAsync(9, ct);

            foreach (var game in games)
            {
                if (game.GameId == null) continue;

                CancellationTokenSource source = CancellationTokenSource.CreateLinkedTokenSource(ct);
                if (_activeGameStreams.TryAdd(game.GameId, source))
                {
                    var color = game.Color;
                    _ = StreamGameAsync(game.GameId, color, source.Token);
                }
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
        catch (LichessAuthenticationException)
        {
            _logger.LogError("Authentication failed during polling");
            HandleAuthenticationFailure();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing games");
        }
    }

    private async Task StreamGameAsync(string gameId, Color userColor, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Starting stream for game {GameId}", gameId);
        try
        {
            await foreach (var evt in _lichessService.StreamGameMovesAsync(gameId, cancellationToken))
            {
                if (evt.Lm != null)
                {
                    _logger.LogDebug("Game {GameId} move: {LastMove}", gameId, evt.Lm);
                }
            }

            _logger.LogDebug("Stream for game {GameId} completed", gameId);
            var result = await DetermineGameResultAsync(gameId, userColor);
            if (result.HasValue)
            {
                _logger.LogInformation("Game {GameId} finished with result: {Result}", gameId, result);
                _mainViewModel.RecordGameResult(result.Value);
            }
            else
            {
                _logger.LogWarning("Game {GameId} finished with no result", gameId);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Stream cancelled for game {GameId}", gameId);
        }
        catch (LichessAuthenticationException)
        {
            _logger.LogError("Authentication failed while streaming game {GameId}", gameId);
            HandleAuthenticationFailure();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stream error for game {GameId}", gameId);
        }
        finally
        {
            _activeGameStreams.TryRemove(gameId, out _);
        }
    }

    private async Task<GameResult?> DetermineGameResultAsync(string gameId, Color userColor)
    {
        var game = await _lichessService.GetGameAsync(gameId);

        if (game.Winner.HasValue)
        {
            if (game.Winner.Value == userColor)
            {
                return GameResult.Win;
            }
            else
            {
                return GameResult.Loss;
            }
        }

        if (game.Status == GameStatus.Draw ||
            game.Status == GameStatus.Stalemate ||
            game.Status == GameStatus.Outoftime ||
            game.Status == GameStatus.Timeout)
            return GameResult.Draw;

        return null;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        StopAll();
    }
}
