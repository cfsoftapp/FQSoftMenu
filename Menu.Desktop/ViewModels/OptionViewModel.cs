namespace Menu.Desktop.ViewModels;

public sealed class OptionViewModel<T>
{
    public OptionViewModel(T value, string text)
    {
        Value = value;
        Text = text;
    }

    public T Value { get; }

    public string Text { get; }
}
