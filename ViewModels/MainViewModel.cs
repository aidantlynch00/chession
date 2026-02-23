using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using chession.Models;
using chession.Services;
using LichessSharp.Exceptions;
using LichessSharp.Models.Users;

namespace chession.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly ILichessService _lichessService;
    private User? _profile;
    private string? _errorMessage;
    private bool _isLoading = true;
    private int _wins;
    private int _losses;
    private int _draws;

    public event EventHandler? AuthenticationFailed;

    public MainViewModel(ILichessService lichessService)
    {
        _lichessService = lichessService;
    }

    public User? Profile
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

    public string SessionScore => $"{Wins + (Draws * 0.5):F1} / {Wins + Losses + Draws}";

    public ObservableCollection<GameResult> CompletedGames { get; } = new();

    public async Task InitializeAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            Profile = await _lichessService.GetProfileAsync();
        }
        catch (LichessAuthenticationException)
        {
            AuthenticationFailed?.Invoke(this, EventArgs.Empty);
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
}
