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
using System.Windows.Shapes;
using EbestTradeBot_V1.ViewModels;
using MahApps.Metro.Controls;

namespace EbestTradeBot_V1.Views
{
    /// <summary>
    /// LoadingView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoadingView : MetroWindow
    {
        private LoadingViewModel _viewModel;
        public LoadingView()
        {
            InitializeComponent();

            _viewModel = (LoadingViewModel)DataContext;
            _viewModel.InitCompleted += (sender, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();

                    Close();
                });
            };
        }

        private void LoadingView_OnLoaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                _viewModel.InitCommand.Execute();
            });
        }
    }
}
