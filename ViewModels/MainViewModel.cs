using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using chession.Models;
using chession.Services;
using LichessSharp.Exceptions;
using LichessSharp.Models.Users;

namespace chession.ViewModels;

public class MainViewModel : ViewModelBase, IDisposable
{
    private const int SlotCount = 10;

    private readonly ILichessService _lichessService;
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

    public event EventHandler? AuthenticationFailed;

    public MainViewModel(ILichessService lichessService)
    {
        _lichessService = lichessService;

        for (var i = 0; i < SlotCount; i++)
            DisplaySlots.Add(new GameSlot(null));

        CompletedGames.CollectionChanged += OnCompletedGamesChanged;
    }

    public UserExtended? Profile
    {
        get => _profile;
        private set => SetProperty(ref _profile, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public int Wins
    {
        get => _wins;
        private set
        {
            if (SetProperty(ref _wins, value))
                OnPropertyChanged(nameof(SessionScore));
        }
    }

    public int Losses
    {
        get => _losses;
        private set
        {
            if (SetProperty(ref _losses, value))
                OnPropertyChanged(nameof(SessionScore));
        }
    }

    public int Draws
    {
        get => _draws;
        private set
        {
            if (SetProperty(ref _draws, value))
                OnPropertyChanged(nameof(SessionScore));
        }
    }

    public string SessionScore
    {
        get
        {
            var score = Wins + (Draws * 0.5);
            var scoreStr = Draws % 2 == 0 ? $"{score:F0}" : $"{score:F1}";
            return $"{scoreStr} / {Wins + Losses + Draws}";
        }
    }

    public ObservableCollection<GameResult> CompletedGames { get; } = new();

    public ObservableCollection<GameSlot> DisplaySlots { get; } = new();

    public bool HasOverflow
    {
        get => _hasOverflow;
        private set => SetProperty(ref _hasOverflow, value);
    }

    public bool IsOverflowExpanded
    {
        get => _isOverflowExpanded;
        set => SetProperty(ref _isOverflowExpanded, value);
    }

    private void OnCompletedGamesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshDisplaySlots();
    }

    private void RefreshDisplaySlots()
    {
        for (var i = 0; i < SlotCount; i++)
            DisplaySlots[i] = new GameSlot(i < CompletedGames.Count ? CompletedGames[i] : null);

        HasOverflow = CompletedGames.Count > SlotCount;
    }

    public async Task InitializeAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        _cts = new CancellationTokenSource();

        try
        {
            Profile = await _lichessService.GetProfileAsync(_cts.Token);

            _gameTracker = new GameTracker(_lichessService, this);
            await _gameTracker.StartTrackingAsync(Profile.Id);
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

    public void OnAuthenticationFailed() => AuthenticationFailed?.Invoke(this, EventArgs.Empty);

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
