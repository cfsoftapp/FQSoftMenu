using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Menu.Desktop.ViewModels;

namespace Menu.Desktop.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
    }

    private void LoginView_Loaded(object sender, RoutedEventArgs e)
    {
        UsuarioTextBox.Focus();
        Keyboard.Focus(UsuarioTextBox);
        UsuarioTextBox.SelectAll();
    }

    private void LoginInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        if (DataContext is LoginViewModel viewModel &&
            viewModel.LoginCommand.CanExecute(null))
        {
            viewModel.LoginCommand.Execute(null);
            e.Handled = true;
        }
    }
}
