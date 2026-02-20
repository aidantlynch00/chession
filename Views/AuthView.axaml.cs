using Avalonia.Controls;
using chession.ViewModels;

namespace chession.Views;

public partial class AuthView : UserControl
{
    public AuthView()
    {
        InitializeComponent();
    }

    public AuthView(AuthViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
