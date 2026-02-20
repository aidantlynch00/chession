using System;
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

    private RelayCommand? _submitCommand;

    public ICommand SubmitCommand => _submitCommand ??= new RelayCommand(Submit, CanSubmit);

    public event EventHandler? AuthenticationSucceeded;

    public AuthViewModel(ITokenStorage tokenStorage)
    {
        _tokenStorage = tokenStorage;
    }

    public string Token
    {
        get => _token;
        set
        {
            if (SetProperty(ref _token, value))
            {
                _submitCommand?.RaiseCanExecuteChanged();
            }
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (SetProperty(ref _isLoading, value))
            {
                _submitCommand?.RaiseCanExecuteChanged();
            }
        }
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
            _submitCommand?.RaiseCanExecuteChanged();
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
