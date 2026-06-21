namespace Menu.Desktop.ViewModels;

public sealed class PlaceholderPageViewModel : ObservableObject
{
    public PlaceholderPageViewModel(string title, string message)
    {
        Title = title;
        Message = message;
    }

    public string Title { get; }

    public string Message { get; }
}
