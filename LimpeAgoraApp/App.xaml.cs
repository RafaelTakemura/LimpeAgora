namespace LimpeAgoraApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell())
        {
            Title = "LimpeAgora - App Cliente",
            Width = 420,
            Height = 820,
            MinimumWidth = 380,
            MinimumHeight = 600,
            MaximumWidth = 500,
            MaximumHeight = 920
        };

        return window;
    }
}