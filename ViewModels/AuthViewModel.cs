using System;
using System.Windows.Input;

namespace chession.ViewModels;

public class AuthViewModel : ViewModelBase
{
    private string _token = string.Empty;
    private string? _errorMessage;

    private RelayCommand? _submitCommand;

    public ICommand SubmitCommand => _submitCommand ??= new RelayCommand(Submit, CanSubmit);

    public event EventHandler<string>? TokenSubmitted;

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

    private bool CanSubmit()
    {
        return !string.IsNullOrWhiteSpace(Token);
    }

    private void Submit()
    {
        ErrorMessage = null;
        TokenSubmitted?.Invoke(this, Token.Trim());
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
