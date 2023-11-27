using ControlzEx.Theming;
using EbestTradeBot_V1.ViewModels;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using EbestTradeBot.Core;
using EbestTradeBot_V1.Views;

namespace EbestTradeBot_V1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            SetTheme();

            return Container.Resolve<LoadingView>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<MainWindowViewModel>();
            containerRegistry.Register<ConfigurationViewModel>();
            containerRegistry.Register<LoadingViewModel>();
        }

        private void SetTheme()
        {
            EbestTradeBot.Core.Models.Theme theme = AppSettings.Instance.Theme;
            if (theme == EbestTradeBot.Core.Models.Theme.Dark)
            {
                SetDarkMode();
            }
            else
            {
                SetLightMode();
            }
        }

        private void SetLightMode()
        {
            ThemeManager.Current.ChangeTheme(this, "Light.Olive");
        }

        private void SetDarkMode()
        {
            ThemeManager.Current.ChangeTheme(this, "Dark.Olive");
        }
    }
}
