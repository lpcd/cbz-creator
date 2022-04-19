using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace app
{
    public partial class MainWindow : Window
    {
        private readonly Services.CbzService _cbzService;

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

        private void OkDialog(string message, string caption)
        {
            DialogResult result = System.Windows.Forms.MessageBox.Show(message, caption, MessageBoxButtons.OK);
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Close();
            }
        }

        private async void SubmitClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button submitButton = (System.Windows.Controls.Button)sender;
            System.Windows.Controls.Grid gridButtons = (System.Windows.Controls.Grid)submitButton.Parent;
            System.Windows.Controls.StackPanel formStackpanel = (System.Windows.Controls.StackPanel)gridButtons.Parent;
            System.Windows.Controls.StackPanel mainStackPanel = (System.Windows.Controls.StackPanel)formStackpanel.Parent;
            System.Windows.Controls.StackPanel loaderStackpanel = (System.Windows.Controls.StackPanel)mainStackPanel.Children[1];

            (string inputFolder, string prefix, bool isSinglePage, bool doCompress, bool rightToLeft) = InitializeService();

            if (!string.IsNullOrWhiteSpace(inputFolder))
            {
                ChangeVisibility(formStackpanel, loaderStackpanel);

                Task task = Task.Run(() => _cbzService.CreateFile());
                await task;

                ChangeVisibility(formStackpanel, loaderStackpanel);
            }
            else
            {
                OkDialog("Input directory cannot be empty.", "Seed input directory !");
            }
        }

        private async void RenameClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button submitButton = (System.Windows.Controls.Button)sender;
            System.Windows.Controls.Grid gridButtons = (System.Windows.Controls.Grid)submitButton.Parent;
            System.Windows.Controls.StackPanel formStackpanel = (System.Windows.Controls.StackPanel)gridButtons.Parent;
            System.Windows.Controls.StackPanel mainStackPanel = (System.Windows.Controls.StackPanel)formStackpanel.Parent;
            System.Windows.Controls.StackPanel loaderStackpanel = (System.Windows.Controls.StackPanel)mainStackPanel.Children[1];

            (string inputFolder, string prefix, bool isSinglePage, bool doCompress, bool rightToLeft) = InitializeService();

            if (!string.IsNullOrWhiteSpace(inputFolder))
            {
                ChangeVisibility(formStackpanel, loaderStackpanel);

                Task task = Task.Run(() => _cbzService.RenameFolder());
                await task;

                ChangeVisibility(formStackpanel, loaderStackpanel);
            }
            else
            {
                OkDialog("Input directory cannot be empty.", "Seed input directory !");
            }
        }

        private (string inputFolder, string prefix, bool isSinglePage, bool doCompress, bool rightToLeft) InitializeService()
        {
            string inputFolder = inputTextBox.Text;

            string prefix = string.IsNullOrWhiteSpace(mangaNameTextBox.Text)
                ? inputTextBox.Text.Split('\\', '/').AsEnumerable().LastOrDefault()
                : mangaNameTextBox.Text;

            if (IsVolume.IsChecked.Value)
            {
                prefix += " " + IsVolume.Content;
            }
            else if (IsChapter.IsChecked.Value)
            {
                prefix += " " + IsChapter.Content;
            }

            bool isSinglePage = !IsDoublePage.IsChecked.Value;

            bool doCompress = IsCompressed.IsChecked.Value;

            bool rightToLeft = RightToLeft.IsChecked.GetValueOrDefault();

            _cbzService.InitializeService(inputFolder, prefix, isSinglePage, doCompress, rightToLeft);

            return (inputFolder, prefix, isSinglePage, doCompress, rightToLeft);
        }

        private void ChangeVisibility(UIElement uiElement, UIElement loader)
        {
            uiElement.Visibility = uiElement.Visibility == Visibility.Hidden
                ? Visibility.Visible
                : Visibility.Hidden;

            loader.Visibility = loader.Visibility == Visibility.Hidden
                ? Visibility.Visible
                : Visibility.Hidden;
        }
    }
}
