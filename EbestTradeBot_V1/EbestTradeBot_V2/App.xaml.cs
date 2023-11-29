using EbestTradeBot.Core;
using EbestTradeBot_V2.Views;
using Prism.Ioc;
using System.Windows;
using ControlzEx.Theming;
using EbestTradeBot_V2.ViewModels;

namespace EbestTradeBot_V2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            SetTheme("Red");
            return Container.Resolve<LoadingView>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<MainWindowViewModel>();
            containerRegistry.Register<ConfigurationViewModel>();
            containerRegistry.Register<LoadingViewModel>();
        }

        public void SetTheme(string color)
        {
            EbestTradeBot.Core.Models.Theme theme = AppSettings.Instance.Theme;
            if (theme == EbestTradeBot.Core.Models.Theme.Dark)
            {
                SetDarkMode(color);
            }
            else
            {
                SetLightMode(color);
            }
        }

        private void SetLightMode(string color)
        {
            ThemeManager.Current.ChangeTheme(this, $"Light.{color}");
        }

        private void SetDarkMode(string color)
        {
            ThemeManager.Current.ChangeTheme(this, $"Dark.{color}");
        }
    }
}
