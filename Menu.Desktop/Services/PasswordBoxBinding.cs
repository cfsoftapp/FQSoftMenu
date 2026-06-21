using System.Windows;
using System.Windows.Controls;

namespace Menu.Desktop.Services;

public static class PasswordBoxBinding
{
    public static readonly DependencyProperty BoundPasswordProperty =
        DependencyProperty.RegisterAttached(
            "BoundPassword",
            typeof(string),
            typeof(PasswordBoxBinding),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBoundPasswordChanged));

    private static readonly DependencyProperty IsUpdatingProperty =
        DependencyProperty.RegisterAttached(
            "IsUpdating",
            typeof(bool),
            typeof(PasswordBoxBinding),
            new PropertyMetadata(false));

    public static readonly DependencyProperty BindPasswordProperty =
        DependencyProperty.RegisterAttached(
            "BindPassword",
            typeof(bool),
            typeof(PasswordBoxBinding),
            new PropertyMetadata(false, OnBindPasswordChanged));

    public static string GetBoundPassword(DependencyObject obj)
    {
        return (string)obj.GetValue(BoundPasswordProperty);
    }

    public static void SetBoundPassword(DependencyObject obj, string value)
    {
        obj.SetValue(BoundPasswordProperty, value);
    }

    public static bool GetBindPassword(DependencyObject obj)
    {
        return (bool)obj.GetValue(BindPasswordProperty);
    }

    public static void SetBindPassword(DependencyObject obj, bool value)
    {
        obj.SetValue(BindPasswordProperty, value);
    }

    private static bool GetIsUpdating(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsUpdatingProperty);
    }

    private static void SetIsUpdating(DependencyObject obj, bool value)
    {
        obj.SetValue(IsUpdatingProperty, value);
    }

    private static void OnBindPasswordChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is not PasswordBox passwordBox)
            return;

        if ((bool)e.OldValue)
            passwordBox.PasswordChanged -= OnPasswordChanged;

        if ((bool)e.NewValue)
            passwordBox.PasswordChanged += OnPasswordChanged;
    }

    private static void OnBoundPasswordChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is not PasswordBox passwordBox)
            return;

        if (!GetIsUpdating(passwordBox))
            passwordBox.Password = e.NewValue?.ToString() ?? string.Empty;
    }

    private static void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not PasswordBox passwordBox)
            return;

        SetIsUpdating(passwordBox, true);
        passwordBox.SetCurrentValue(BoundPasswordProperty, passwordBox.Password);
        SetIsUpdating(passwordBox, false);
    }
}
