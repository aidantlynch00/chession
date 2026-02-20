using System;
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

    public event EventHandler? AuthenticationSucceeded;

    public AuthViewModel(ITokenStorage tokenStorage)
    {
        _tokenStorage = tokenStorage;
        SubmitCommand = new RelayCommand(Submit, CanSubmit);
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
            var result = await _tokenStorage.StoreTokenAsync(Token.Trim());
            
            if (result.Success)
            {
                AuthenticationSucceeded?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Failed to store token.";
            }
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
