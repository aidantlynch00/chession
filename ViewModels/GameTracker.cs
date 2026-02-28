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

                if (_activeGameStreams.TryAdd(game.GameId, CancellationTokenSource.CreateLinkedTokenSource(ct)))
                {
                    var color = game.Color.ToString().ToLower();
                    _ = StreamGameAsync(game.GameId, color, _activeGameStreams[game.GameId].Token);
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

    private async Task StreamGameAsync(string gameId, string userColor, CancellationToken cancellationToken)
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

    private async Task<GameResult?> DetermineGameResultAsync(string gameId, string userColor)
    {
        var game = await _lichessService.GetGameAsync(gameId);
        var status = game.Status;

        if (status == GameStatus.Aborted ||
            status == GameStatus.NoStart ||
            status == GameStatus.UnknownFinish ||
            status == GameStatus.Started)
            return null;

        if (status == GameStatus.Draw ||
            status == GameStatus.Stalemate ||
            status == GameStatus.Outoftime)
            return GameResult.Draw;

        var winner = game.Winner;
        if (!winner.HasValue)
            return null;

        var userIsWhite = userColor.Equals("white", StringComparison.OrdinalIgnoreCase);
        var whiteWon = winner.Value == Color.White;

        if (userIsWhite == whiteWon)
            return GameResult.Win;

        return GameResult.Loss;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        StopAll();
    }
}
