using LimpeAgoraApp.Views;

namespace LimpeAgoraApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("CadastroPage", typeof(CadastroPage));
    }
}
