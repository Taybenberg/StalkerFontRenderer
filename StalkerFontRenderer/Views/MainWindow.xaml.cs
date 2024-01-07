using StalkerFontRenderer.ViewModels;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace StalkerFontRenderer.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : FluentWindow
{
    public MainWindow()
    {
        SystemThemeWatcher.Watch(this);

        var service = new SnackbarService();
        DataContext = new MainWindowViewModel(service);

        InitializeComponent();

        service.SetSnackbarPresenter(SnackbarPresenter);
    }
}