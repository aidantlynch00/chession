using System;
using System.Windows.Input;
using Microsoft.Extensions.Logging;

namespace chession.ViewModels;

/// <summary>
/// ViewModel for the authentication view handling token input.
/// </summary>
public class AuthViewModel : ViewModelBase
{
    private readonly ILogger _logger;
    private string _token = string.Empty;
    private string? _errorMessage;

    private RelayCommand? _submitCommand;

    /// <summary>
    /// Initializes a new instance of the AuthViewModel class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory for creating loggers.</param>
    public AuthViewModel(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<AuthViewModel>();
    }

    /// <summary>
    /// Gets the command to submit the token.
    /// </summary>
    public ICommand SubmitCommand => _submitCommand ??= new RelayCommand(Submit, CanSubmit);

    /// <summary>
    /// Event raised when a token is submitted.
    /// </summary>
    public event EventHandler<string>? TokenSubmitted;

    /// <summary>
    /// Gets or sets the Lichess API token.
    /// </summary>
    public string Token
    {
        get => _token;
        set
        {
            if (SetProperty(ref _token, value))
            {
                _logger.LogDebug("Token length: {Length}", value?.Length ?? 0);
                _submitCommand?.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the error message to display.
    /// </summary>
    public string? ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (value != null) _logger.LogWarning("Error message set: {Error}", value);
            SetProperty(ref _errorMessage, value);
        }
    }

    /// <summary>
    /// Determines whether the token can be submitted.
    /// </summary>
    /// <returns>True if the token is not empty; otherwise, false.</returns>
    private bool CanSubmit()
    {
        return !string.IsNullOrWhiteSpace(Token);
    }

    /// <summary>
    /// Submits the token and raises the TokenSubmitted event.
    /// </summary>
    private void Submit()
    {
        ErrorMessage = null;
        _logger.LogInformation("Token submitted");
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
