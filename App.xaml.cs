using System.Globalization;
using System.Windows;

namespace FuzzyGameEngine
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var culture = new CultureInfo("uk-UA");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            base.OnStartup(e);
        }
    }
}
