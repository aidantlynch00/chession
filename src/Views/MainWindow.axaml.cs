using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using chession.Services;
using chession.ViewModels;
using LichessSharp.Exceptions;
using Microsoft.Extensions.Logging;

namespace chession.Views;

/// <summary>
/// Main window orchestrating navigation between AuthView and MainView.
/// </summary>
public partial class MainWindow : Window
{
    private static readonly ILoggerFactory LogFactory = LoggerFactory.Create(builder =>
    {
        builder
            .AddFilter("chession", LogLevel.Debug)
            .AddConsole();
    });

    private ITokenStorage _tokenStorage = null!;
    private AuthViewModel? _authViewModel;
    private MainViewModel? _mainViewModel;

    /// <summary>
    /// Initializes a new instance of the MainWindow class.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        Closed += OnClosed;
    }

    /// <summary>
    /// Handles the window closed event to dispose resources.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void OnClosed(object? sender, EventArgs e)
    {
        _mainViewModel?.Dispose();
    }

    /// <summary>
    /// Initializes the window by checking for a stored token and showing the appropriate view.
    /// </summary>
    /// <param name="tokenStorage">The token storage instance.</param>
    public async Task InitializeAsync(ITokenStorage tokenStorage)
    {
        _tokenStorage = tokenStorage;
        var tokenData = await _tokenStorage.GetTokenAsync();

        if (tokenData == null)
        {
            ShowAuthView();
        }
        else
        {
            await ShowMainViewAsync(tokenData.Token);
        }
    }

    /// <summary>
    /// Shows the authentication view for token input.
    /// </summary>
    /// <param name="errorMessage">Optional error message to display.</param>
    private void ShowAuthView(string? errorMessage = null)
    {
        _authViewModel = new AuthViewModel(LogFactory);
        _authViewModel.ErrorMessage = errorMessage;
        _authViewModel.TokenSubmitted += OnTokenSubmitted;
        MainContent.Content = new AuthView(_authViewModel);
    }

    /// <summary>
    /// Handles token submission and shows the main view.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="token">The submitted token.</param>
    private async void OnTokenSubmitted(object? sender, string token)
    {
        await ShowMainViewAsync(token);
    }

    /// <summary>
    /// Shows the main view with the Lichess service.
    /// </summary>
    /// <param name="token">The Lichess API token.</param>
    private async Task ShowMainViewAsync(string token)
    {
        _mainViewModel?.Dispose();
        
        var lichessService = new LichessService(token);
        _mainViewModel = new MainViewModel(lichessService, LogFactory);
        _mainViewModel.AuthenticationFailed += OnAuthenticationFailed;
        MainContent.Content = new MainView { DataContext = _mainViewModel };

        try
        {
            await _mainViewModel.InitializeAsync();
            await _tokenStorage.StoreTokenAsync(token);
        }
        catch (LichessAuthenticationException)
        {
            ShowAuthView("Invalid or expired token. Please enter a new one.");
        }
        catch (Exception ex)
        {
            ShowAuthView($"Failed to connect: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles authentication failure by clearing the token and showing the auth view.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void OnAuthenticationFailed(object? sender, EventArgs e)
    {
        _ = _tokenStorage.ClearTokenAsync();
        ShowAuthView("Session expired. Please enter your token again.");
    }
}
