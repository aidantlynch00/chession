using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using chession.Services;
using chession.ViewModels;
using LichessSharp.Exceptions;
using Microsoft.Extensions.Logging;

namespace chession.Views;

public partial class MainWindow : Window
{
    private static readonly ILoggerFactory LogFactory = LoggerFactory.Create(builder =>
    {
        builder.AddConsole();
    });

    private ITokenStorage _tokenStorage = null!;
    private AuthViewModel? _authViewModel;
    private MainViewModel? _mainViewModel;

    public MainWindow()
    {
        InitializeComponent();
        Closed += OnClosed;
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _mainViewModel?.Dispose();
    }

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

    private void ShowAuthView(string? errorMessage = null)
    {
        _authViewModel = new AuthViewModel(LogFactory);
        _authViewModel.ErrorMessage = errorMessage;
        _authViewModel.TokenSubmitted += OnTokenSubmitted;
        MainContent.Content = new AuthView(_authViewModel);
    }

    private async void OnTokenSubmitted(object? sender, string token)
    {
        await ShowMainViewAsync(token);
    }

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

    private void OnAuthenticationFailed(object? sender, EventArgs e)
    {
        _ = _tokenStorage.ClearTokenAsync();
        ShowAuthView("Session expired. Please enter your token again.");
    }
}
