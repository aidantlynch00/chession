using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using chession.Models;
using chession.Services;
using LichessSharp.Exceptions;
using LichessSharp.Models.Users;
using Microsoft.Extensions.Logging;

namespace chession.ViewModels;

/// <summary>
/// ViewModel for the main dashboard displaying session game statistics.
/// </summary>
public class MainViewModel : ViewModelBase, IDisposable
{
    private const int SlotCount = 10;

    private readonly ILichessService _lichessService;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;
    private CancellationTokenSource? _cts;
    private GameTracker? _gameTracker;
    private bool _disposed;

    private UserExtended? _profile;
    private string? _errorMessage;
    private bool _isLoading = true;
    private int _wins;
    private int _losses;
    private int _draws;
    private bool _hasOverflow;
    private bool _isOverflowExpanded;

    /// <summary>
    /// Event raised when authentication fails.
    /// </summary>
    public event EventHandler? AuthenticationFailed;

    /// <summary>
    /// Initializes a new instance of the MainViewModel class.
    /// </summary>
    /// <param name="lichessService">The Lichess service for API interactions.</param>
    /// <param name="loggerFactory">The logger factory for creating loggers.</param>
    public MainViewModel(ILichessService lichessService, ILoggerFactory loggerFactory)
    {
        _lichessService = lichessService;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<MainViewModel>();

        for (var i = 0; i < SlotCount; i++)
            DisplaySlots.Add(new GameSlot(null));

        CompletedGames.CollectionChanged += OnCompletedGamesChanged;
    }

    /// <summary>
    /// Gets the authenticated user's profile information.
    /// </summary>
    public UserExtended? Profile
    {
        get => _profile;
        private set => SetProperty(ref _profile, value);
    }

    /// <summary>
    /// Gets or sets the error message to display.
    /// </summary>
    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    /// <summary>
    /// Gets or sets whether the view is loading.
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    /// <summary>
    /// Gets the number of games won in the session.
    /// </summary>
    public int Wins
    {
        get => _wins;
        private set
        {
            if (SetProperty(ref _wins, value))
                OnPropertyChanged(nameof(SessionScore));
        }
    }

    /// <summary>
    /// Gets the number of games lost in the session.
    /// </summary>
    public int Losses
    {
        get => _losses;
        private set
        {
            if (SetProperty(ref _losses, value))
                OnPropertyChanged(nameof(SessionScore));
        }
    }

    /// <summary>
    /// Gets the number of games drawn in the session.
    /// </summary>
    public int Draws
    {
        get => _draws;
        private set
        {
            if (SetProperty(ref _draws, value))
                OnPropertyChanged(nameof(SessionScore));
        }
    }

    /// <summary>
    /// Gets the session score formatted as "points / total games".
    /// </summary>
    public string SessionScore
    {
        get
        {
            var score = Wins + (Draws * 0.5);
            var scoreStr = Draws % 2 == 0 ? $"{score:F0}" : $"{score:F1}";
            return $"{scoreStr} / {Wins + Losses + Draws}";
        }
    }

    /// <summary>
    /// Gets the collection of completed games for the session.
    /// </summary>
    public ObservableCollection<GameResult> CompletedGames { get; } = new();

    /// <summary>
    /// Gets the collection of game slots for display.
    /// </summary>
    public ObservableCollection<GameSlot> DisplaySlots { get; } = new();

    /// <summary>
    /// Gets whether there are more games than display slots.
    /// </summary>
    public bool HasOverflow
    {
        get => _hasOverflow;
        private set => SetProperty(ref _hasOverflow, value);
    }

    /// <summary>
    /// Gets or sets whether the overflow section is expanded.
    /// </summary>
    public bool IsOverflowExpanded
    {
        get => _isOverflowExpanded;
        set => SetProperty(ref _isOverflowExpanded, value);
    }

    private void OnCompletedGamesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshDisplaySlots();
    }

    /// <summary>
    /// Refreshes the display slots based on completed games.
    /// </summary>
    private void RefreshDisplaySlots()
    {
        for (var i = 0; i < SlotCount; i++)
            DisplaySlots[i] = new GameSlot(i < CompletedGames.Count ? CompletedGames[i] : null);

        HasOverflow = CompletedGames.Count > SlotCount;
    }

    /// <summary>
    /// Initializes the ViewModel by loading the user profile and starting game tracking.
    /// </summary>
    public async Task InitializeAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        _cts = new CancellationTokenSource();

        try
        {
            Profile = await _lichessService.GetProfileAsync(_cts.Token);

            _gameTracker = new GameTracker(_lichessService, this, _loggerFactory.CreateLogger<GameTracker>());
            await _gameTracker.StartTrackingAsync();
        }
        catch (LichessAuthenticationException)
        {
            OnAuthenticationFailed();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load profile: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Raises the AuthenticationFailed event.
    /// </summary>
    public void OnAuthenticationFailed() => AuthenticationFailed?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Records a game result and updates session statistics.
    /// </summary>
    /// <param name="result">The result of the completed game.</param>
    public void RecordGameResult(GameResult result)
    {
        CompletedGames.Insert(0, result);

        switch (result)
        {
            case GameResult.Win:
                Wins++;
                break;
            case GameResult.Loss:
                Losses++;
                break;
            case GameResult.Draw:
                Draws++;
                break;
        }
    }

    /// <summary>
    /// Disposes of the ViewModel and releases resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _cts?.Cancel();
        _cts?.Dispose();
        _gameTracker?.Dispose();
        _lichessService.Dispose();
    }
}
