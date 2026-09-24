using log4net;
using log4net.Config;
using MasterMind_Client.TempData;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace MasterMind_Client
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(App));

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            CultureInfo cultureToUse = DetermineStartupCulture();

            System.Threading.Thread.CurrentThread.CurrentUICulture = cultureToUse;
            CultureInfo.DefaultThreadCurrentUICulture = cultureToUse;

            XmlConfigurator.Configure();

            log.Info("Starting seeding in the sandbox");

            var testuser = new TempPlayer
            {
                Username = "testuser",
                Password = "password123",
                Email = "testuser@example.com",
                Avatar = new BitmapImage(new Uri("Images/Avatars/Link.jpg", UriKind.Relative))
            };

            var johndoe = new TempPlayer
            {
                Username = "johndoe",
                Password = "securepass",
                Email = "johndoe@example.com",
                Avatar = new BitmapImage(new Uri("Images/Avatars/MasterChief.jpg", UriKind.Relative))
            };

            var gabosaurio = new TempPlayer
            {
                Username = "gabosaurio",
                Password = "gabo",
                Email = "gabosaurio@gmail.com",
                Avatar = new BitmapImage(new Uri("Images/Avatars/Patronus HD.jpg", UriKind.Relative))
            };

            var noobmaster = new TempPlayer
            {
                Username = "noobmaster69",
                Password = "noob",
                Email = "noobmaster@gmail.com",
                Avatar = new BitmapImage(new Uri("Images/Avatars/Link con fondo.jpg", UriKind.Relative))
            };

            TempAuthService.MockRegisterPlayer(gabosaurio);
            TempAuthService.MockRegisterPlayer(testuser);
            TempAuthService.MockRegisterPlayer(johndoe);
            TempAuthService.MockRegisterPlayer(noobmaster);

            FriendshipService.SendFrienshipRequest(testuser.Id, gabosaurio.Id);
            FriendshipService.SendFrienshipRequest(johndoe.Id, gabosaurio.Id);

            log.Info("Seeding finished");
        }

        private static CultureInfo DetermineStartupCulture()
        {
            var systemCulture = CultureInfo.CurrentUICulture;

            if (systemCulture.TwoLetterISOLanguageName == "es")
            {
                return new CultureInfo("es-ES");
            }
            else
            {
                return new CultureInfo("en-US");
            }
        }
    }
}
