using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        }

        private void MainWindow_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Content is Page page && !(page.Content is Viewbox))
            {
                var originalContent = page.Content;
                page.Content = null;

                var viewbox = new Viewbox { Stretch = Stretch.Uniform };
                var container = new Grid { Width = 1920, Height = 1080 };

                container.Children.Add((UIElement)originalContent);
                viewbox.Child = container;
                page.Content = viewbox;
            }
        }
    }
}
