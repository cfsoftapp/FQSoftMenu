namespace Menu.Desktop.ViewModels;

public sealed class DashboardMetricViewModel
{
    public DashboardMetricViewModel(string title, string value, string accent)
    {
        Title = title;
        Value = value;
        Accent = accent;
    }

    public string Title { get; }

    public string Value { get; }

    public string Accent { get; }
}
