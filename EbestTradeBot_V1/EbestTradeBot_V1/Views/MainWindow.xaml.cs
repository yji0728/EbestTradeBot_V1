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
using EbestTradeBot_V1.ViewModels;
using EbestTradeBot_V1.Views;
using MahApps.Metro.Controls;

namespace EbestTradeBot_V1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private MainWindowViewModel _vm;
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainWindowViewModel();
            _vm = (MainWindowViewModel)DataContext;
        }

        private void Cofiguration_Clicked(object sender, RoutedEventArgs e)
        {
            if (_vm.IsRun)
            {
                MessageBox.Show("매매 진행중엔 설정 창을 열 수 없습니다", "오류");
                return;
            }
            ConfigurationWindow configurationWindow = new ConfigurationWindow();
            configurationWindow.ShowDialog();
        }
    }
}
