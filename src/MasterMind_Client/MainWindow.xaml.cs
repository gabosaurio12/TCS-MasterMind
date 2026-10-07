using System;
using MasterMind_Client.ViewModels.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace MasterMind_Client
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : NavigationWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            this.WindowStyle = WindowStyle.None;
            this.WindowState = WindowState.Maximized;
        }

        private void NavigationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Navigated += MainWindow_Navigated;

            if (AppServices.NavigationService is NavigationServiceAdapter adapter)
            {
                adapter.SetNavigationService(this.NavigationService);
            }
        }

        private void MainWindow_Navigated(object sender, NavigationEventArgs e)
        {
            if (AppServices.NavigationService is NavigationServiceAdapter adapter)
            {
                adapter.SetNavigationService(this.NavigationService);
            }

            if (e.Content is Page page && !(page.Content is Viewbox))
            {
                var originalContent = page.Content;
                page.Content = null;

                var viewbox = new Viewbox { Stretch = Stretch.Fill };
                var container = new Grid { Width = 1920, Height = 1080 };

                container.Children.Add((UIElement)originalContent);
                viewbox.Child = container;
                page.Content = viewbox;
            }
        }
    }
}
