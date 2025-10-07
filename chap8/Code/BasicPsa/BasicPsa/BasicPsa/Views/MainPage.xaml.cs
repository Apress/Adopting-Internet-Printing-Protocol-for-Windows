using Windows.Graphics.Printing.PrintSupport;
using Windows.Graphics.Printing.PrintTicket;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel;
using System;
using Windows.System;
using Windows.Storage;

namespace BasicPsa;

/// <summary>
/// An empty page that can be used on its own or navigated to within a <see cref="Frame">.
/// </summary>
public sealed partial class MainPage : Page
{
    public static MainPage Current;
    public MainPage()
    {
        InitializeComponent();
        Current = this;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        var package = Package.Current;
        var version = package.Id.Version;
        var ststus = package.Description;
        string sVersionString = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        string sInstalledDate = package.InstalledDate.ToLocalTime().ToString();
        string sPublisherDisplayName = package.PublisherDisplayName;
        string sInstalledPath = package.InstalledPath.ToString();
        string sFamilyName = package.Id.FamilyName;
        InstalledDate.Text = sInstalledDate;
        Publisher.Text = sPublisherDisplayName;
        InstalledPath.Text = sInstalledPath;
        FamilyName.Text = sFamilyName;
        AppDeployPath.Text = Windows.Storage.ApplicationData.Current.LocalFolder.DisplayName.ToString();
    }

    private async void Printers_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
    {
        var uri = new Uri("ms-settings:printers");
        await Windows.System.Launcher.LaunchUriAsync(uri);
    }

    private async void Devices_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
    {
        var uri = new Uri("ms-settings:devices");
        await Windows.System.Launcher.LaunchUriAsync(uri);
    }
}
