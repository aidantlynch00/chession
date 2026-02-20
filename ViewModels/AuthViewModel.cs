using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using chession.Services;

namespace chession.ViewModels;

public class AuthViewModel : ViewModelBase
{
    private readonly ITokenStorage _tokenStorage;
    
    private string _token = string.Empty;
    private string? _errorMessage;
    private bool _isLoading;

    public string Token
    {
        get => _token;
        set => SetProperty(ref _token, value);
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

    public ICommand SubmitCommand { get; }
    public ICommand OpenTokenUrlCommand { get; }

    public event EventHandler? AuthenticationSucceeded;

    public AuthViewModel(ITokenStorage tokenStorage)
    {
        _tokenStorage = tokenStorage;
        SubmitCommand = new RelayCommand(Submit, CanSubmit);
        OpenTokenUrlCommand = new RelayCommand(OpenTokenUrl);
    }

    private void OpenTokenUrl()
    {
        const string url = "https://lichess.org/account/oauth/token/create";
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    private bool CanSubmit()
    {
        return !string.IsNullOrWhiteSpace(Token) && !IsLoading;
    }

    private async void Submit()
    {
        if (IsLoading)
            return;

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            await _tokenStorage.StoreTokenAsync(Token.Trim());
            AuthenticationSucceeded?.Invoke(this, EventArgs.Empty);
        }
        catch (UnauthorizedAccessException ex)
        {
            ErrorMessage = $"Permission denied: {ex.Message}";
        }
        catch (IOException ex)
        {
            ErrorMessage = $"IO error: {ex.Message}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to store token: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            ((RelayCommand)SubmitCommand).RaiseCanExecuteChanged();
        }
    }

    private class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => _execute();

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
