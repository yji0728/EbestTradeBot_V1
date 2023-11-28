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
