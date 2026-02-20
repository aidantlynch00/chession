using System.Threading.Tasks;
using Avalonia.Controls;
using chession.Services;
using chession.ViewModels;

namespace chession.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public async Task InitializeAsync(ITokenStorage tokenStorage)
    {
        var token = await tokenStorage.GetTokenAsync();
        
        if (token == null)
        {
            ShowAuthView(tokenStorage);
        }
        else
        {
            ShowMainView();
        }
    }

    private void ShowAuthView(ITokenStorage tokenStorage)
    {
        var viewModel = new AuthViewModel(tokenStorage);
        viewModel.AuthenticationSucceeded += (s, e) => ShowMainView();
        MainContent.Content = new AuthView(viewModel);
    }

    private void ShowMainView()
    {
        MainContent.Content = new MainView();
    }
}
