using System.Windows;
using EbestTradeBot_V2.ViewModels;
using MahApps.Metro.Controls;

namespace EbestTradeBot_V2.Views
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
