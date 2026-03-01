using Avalonia.Controls;
using chession.ViewModels;

namespace chession.Views;

/// <summary>
/// Authentication view for entering the Lichess API token.
/// </summary>
public partial class AuthView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the AuthView class.
    /// </summary>
    public AuthView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance of the AuthView class with a ViewModel.
    /// </summary>
    /// <param name="viewModel">The authentication ViewModel.</param>
    public AuthView(AuthViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
