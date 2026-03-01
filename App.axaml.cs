using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using chession.Services;
using chession.Views;

namespace chession;

/// <summary>
/// Application entry point and configuration.
/// </summary>
public class App : Application
{
    private readonly ITokenStorage _tokenStorage = new TokenStorage();

    /// <inheritdoc />
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <inheritdoc />
    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow();
            desktop.MainWindow = mainWindow;
            await mainWindow.InitializeAsync(_tokenStorage);
        }

        base.OnFrameworkInitializationCompleted();
    }
}
