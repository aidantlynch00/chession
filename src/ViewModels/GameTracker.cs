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

/// <summary>
/// Tracks ongoing Lichess games and records results when they complete.
/// </summary>
public class GameTracker : IDisposable
{
    private readonly ILichessService _lichessService;
    private readonly MainViewModel _mainViewModel;
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _activeGameStreams = new();
    private CancellationTokenSource? _pollingCts;
    private Timer? _pollTimer;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the GameTracker class.
    /// </summary>
    /// <param name="lichessService">The Lichess service for API interactions.</param>
    /// <param name="mainViewModel">The main ViewModel to record results to.</param>
    /// <param name="logger">The logger for tracking events.</param>
    public GameTracker(ILichessService lichessService, MainViewModel mainViewModel, ILogger logger)
    {
        _lichessService = lichessService;
        _mainViewModel = mainViewModel;
        _logger = logger;
    }

    /// <summary>
    /// Stops all active game streams and polling.
    /// </summary>
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

    /// <summary>
    /// Handles authentication failure by stopping tracking and notifying the main ViewModel.
    /// </summary>
    private void HandleAuthenticationFailure()
    {
        StopAll();
        _mainViewModel.OnAuthenticationFailed();
    }

    /// <summary>
    /// Starts tracking ongoing games with periodic polling.
    /// </summary>
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

    /// <summary>
    /// Refreshes the list of ongoing games and starts streaming new ones.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
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

    /// <summary>
    /// Streams moves for a game and records the result when it completes.
    /// </summary>
    /// <param name="gameId">The ID of the game to stream.</param>
    /// <param name="userColor">The color the user was playing.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
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

    /// <summary>
    /// Determines the game result based on the game data.
    /// </summary>
    /// <param name="gameId">The ID of the game.</param>
    /// <param name="userColor">The color the user was playing.</param>
    /// <returns>The game result, or null if undetermined.</returns>
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

    /// <summary>
    /// Disposes of the tracker and releases all resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        StopAll();
    }
}
