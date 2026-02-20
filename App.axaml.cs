using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using chession.Services;
using chession.Views;

namespace chession;

public class App : Application
{
    private readonly ITokenStorage _tokenStorage = new TokenStorage();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

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
