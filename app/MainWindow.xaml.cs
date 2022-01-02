using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace app
{
    public partial class MainWindow : Window
    {
        private Services.CbzService _cbzService;

        public MainWindow()
        {
            InitializeComponent();
            _cbzService = new Services.CbzService();
        }

        private void InputClick(object sender, RoutedEventArgs e)
        {
            inputTextBox.Text = FolderDialog();
        }

        private string FolderDialog()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();

            return result == System.Windows.Forms.DialogResult.OK 
                ? dialog.SelectedPath 
                : string.Empty;
        }

        private async void SubmitClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button submitButton = (System.Windows.Controls.Button)sender;
            System.Windows.Controls.StackPanel formStackpanel = (System.Windows.Controls.StackPanel)submitButton.Parent;
            System.Windows.Controls.Grid mainGrid = (System.Windows.Controls.Grid)formStackpanel.Parent;
            System.Windows.Controls.StackPanel loaderStackpanel = (System.Windows.Controls.StackPanel)mainGrid.Children[2];

            formStackpanel.Visibility = Visibility.Hidden;
            loaderStackpanel.Visibility = Visibility.Visible;

            _cbzService.InitializeService(inputTextBox.Text, mangaNameTextBox.Text, !IsDoublePage.IsChecked.Value, IsCompressed.IsChecked.Value, RightToLeft.IsChecked.GetValueOrDefault());
            Task task = Task.Run(() => _cbzService.CreateFile());
            await task;

            formStackpanel.Visibility = Visibility.Visible;
            loaderStackpanel.Visibility = Visibility.Hidden;
        }

        private async void RenameClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button submitButton = (System.Windows.Controls.Button)sender;
            System.Windows.Controls.Grid gridButtons = (System.Windows.Controls.Grid)submitButton.Parent;
            System.Windows.Controls.StackPanel formStackpanel = (System.Windows.Controls.StackPanel)gridButtons.Parent;
            System.Windows.Controls.StackPanel mainStackPanel = (System.Windows.Controls.StackPanel)formStackpanel.Parent;
            System.Windows.Controls.StackPanel loaderStackpanel = (System.Windows.Controls.StackPanel)mainStackPanel.Children[1];

            formStackpanel.Visibility = Visibility.Hidden;
            loaderStackpanel.Visibility = Visibility.Visible;

            _cbzService.InitializeService(inputTextBox.Text, mangaNameTextBox.Text, !IsDoublePage.IsChecked.Value, IsCompressed.IsChecked.Value, RightToLeft.IsChecked.GetValueOrDefault());
            Task task = Task.Run(() => _cbzService.RenameFolder());
            await task;

            formStackpanel.Visibility = Visibility.Visible;
            loaderStackpanel.Visibility = Visibility.Hidden;
        }
    }
}
