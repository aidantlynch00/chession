using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using chession.Models;
using chession.Services;
using LichessSharp.Models.Games;
using LichessSharp.Models.Enums;

namespace chession.ViewModels;

public class GameTracker : IDisposable
{
    private readonly ILichessService _lichessService;
    private readonly MainViewModel _mainViewModel;
    private readonly Dictionary<string, CancellationTokenSource> _activeGameStreams = new();
    private CancellationTokenSource? _pollingCts;
    private Timer? _pollTimer;
    private string? _userId;
    private bool _disposed;

    public GameTracker(ILichessService lichessService, MainViewModel mainViewModel)
    {
        _lichessService = lichessService;
        _mainViewModel = mainViewModel;
    }

    public async Task StartTrackingAsync(string userId)
    {
        _userId = userId;
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
            var currentGameIds = games.Select(g => g.GameId).ToHashSet();

            foreach (var game in games)
            {
                if (!_activeGameStreams.ContainsKey(game.GameId))
                {
                    var gameCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    _activeGameStreams[game.GameId] = gameCts;
                    var color = game.Color.ToString().ToLower();
                    _ = StreamGameAsync(game.GameId, color, gameCts.Token);
                }
            }

            var toRemove = _activeGameStreams.Keys.Except(currentGameIds).ToList();
            foreach (var gameId in toRemove)
            {
                _activeGameStreams[gameId].Cancel();
                _activeGameStreams.Remove(gameId);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameTracker] Error refreshing games: {ex.Message}");
        }
    }

    private async Task StreamGameAsync(string gameId, string userColor, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[GameTracker] Starting stream for game {gameId}");
        try
        {
            await foreach (var evt in _lichessService.StreamGameMovesAsync(gameId, cancellationToken))
            {
                Console.WriteLine($"[GameTracker] Game {gameId} event: status={evt.Status?.ToString() ?? "null"}, lastMove={evt.Lm ?? "null"}, turns={evt.Turns}");
            }

            Console.WriteLine($"[GameTracker] Stream for game {gameId} completed normally");
            var result = await DetermineResultAsync(gameId, userColor);
            if (result.HasValue)
            {
                Console.WriteLine($"[GameTracker] Game {gameId} finished with result: {result}");
                _mainViewModel.RecordGameResult(result.Value);
            }
            else
            {
                Console.WriteLine($"[GameTracker] Game {gameId} finished with no result");
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"[GameTracker] Stream cancelled for game {gameId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameTracker] Stream error for game {gameId}: {ex.Message}");
        }
        finally
        {
            lock (_activeGameStreams)
            {
                _activeGameStreams.Remove(gameId);
            }
        }
    }

    private async Task<GameResult?> DetermineResultAsync(string gameId, string userColor)
    {
        try
        {
            var game = await _lichessService.GetGameAsync(gameId);
            return DetermineResult(game, userColor);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameTracker] Error fetching game {gameId}: {ex.Message}");
            return null;
        }
    }

    private GameResult? DetermineResult(Game game, string userColor)
    {
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

        _pollTimer?.Dispose();
        _pollingCts?.Cancel();
        _pollingCts?.Dispose();

        foreach (var cts in _activeGameStreams.Values)
        {
            cts.Cancel();
            cts.Dispose();
        }
        _activeGameStreams.Clear();
    }
}
