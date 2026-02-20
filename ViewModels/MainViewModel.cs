using System;
using System.Threading.Tasks;
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
