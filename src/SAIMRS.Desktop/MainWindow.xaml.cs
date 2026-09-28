using System.Windows;

namespace SAIMRS.Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        // The BlazorWebView reads its services from this resource (see MainWindow.xaml).
        Resources.Add("services", App.Services);
        InitializeComponent();
    }
}
