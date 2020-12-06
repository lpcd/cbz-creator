using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace app
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InputClick(object sender, RoutedEventArgs e)
        {
            inputTextBox.Text = this.FolderDialog();
        }

        private async void SubmitClick(object sender, RoutedEventArgs e)
        {
            try
            {
                //DISPLAY SPINNER
                gridConfiguration.Visibility = Visibility.Hidden;
                stackPanelSpinner.Visibility = Visibility.Visible;

                //WORK
                var task = this.IsWorking();
                var items = await task;

                // HIDE SPINNER
                gridConfiguration.Visibility = Visibility.Visible;
                stackPanelSpinner.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string FolderDialog()
        {
            var dialog = new FolderBrowserDialog();
            
            DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
                return dialog.SelectedPath;
            else
                return string.Empty;
        }

        private async Task<bool> IsWorking()
        {
            labelInformation.Content = "Working started";

            string tempFolder = $"{ inputTextBox.Text }\\temp";
            List<string> directories = new List<string>(Directory.EnumerateDirectories(inputTextBox.Text));
            foreach (var directory in directories)
            {
                // create directory temp
                Directory.CreateDirectory(tempFolder);

                int counter = 0;
                foreach (string file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
                {
                    labelInformation.Content = file;

                    IEnumerable<string> imageExtensions = new List<string> { "jpg", "jpeg", "jpe", "bmp", "gif", "png" };
                    if (imageExtensions.Contains(file.Split('.').Last()))
                    {
                        using (Image img = this.GetImageByPath(file))
                        {
                            if (IsDoublePage.IsChecked.Value)
                            {
                                if (img.Width > img.Height)
                                {
                                    IEnumerable<Image> imgs = this.SplitDoublePage(img);
                                    foreach (Image splitedImg in imgs)
                                    {
                                        this.SaveSinglePage(splitedImg, this.DestinationName(tempFolder, counter));
                                        counter++;
                                    }
                                }
                                else
                                {
                                    this.SaveSinglePage(img, this.DestinationName(tempFolder, counter));
                                    counter++;
                                }
                            }
                            else
                            {
                                this.SaveSinglePage(img, this.DestinationName(tempFolder, counter));
                                counter++;
                            }
                        }
                        GC.Collect();
                    }
                }

                // Create CBZ
                ZipFile.CreateFromDirectory(tempFolder, $"{ inputTextBox.Text}\\{ mangaNameTextBox.Text } { directory.Split('\\').Last() }.cbz");
                
                // Clear unused data
                Directory.Delete(tempFolder, true);
                Directory.Delete(directory, true);
            }

            return true;
        }

        private string DestinationName(string tempFolder, int counter)
            => $"{ tempFolder }\\{ String.Format("{0:00000}", counter) }.jpg";

        private Image GetImageByPath(string inputPath)
            => Image.FromFile(inputPath);

        private byte[] ConvertToJPG(Image input)
        {
            Bitmap img = input as Bitmap;
            using (MemoryStream ms = new MemoryStream())
            {
                if (IsCompressed.IsChecked.Value)
                {
                    EncoderParameters myEncoderParameters = new EncoderParameters(1);
                    myEncoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 50L);
                
                    img.Save(ms, this.GetEncoder(ImageFormat.Jpeg), myEncoderParameters);
                }
                else
                {
                    img.Save(ms, ImageFormat.Jpeg);
                }
                return ms.ToArray();
            }
        }

        private void SaveImage(byte[] imagebuffer, string outputPath)
        {
            File.WriteAllBytes(outputPath, imagebuffer);
        }

        private void SaveSinglePage(Image img, string destinationPath)
        {
            byte[] imageBuffer = this.ConvertToJPG(img);
            this.SaveImage(imageBuffer, destinationPath);
        }

        private IEnumerable<Image> SplitDoublePage(Image inputImg)
        {
            var imgs = new List<Image>();
            int imgWidth = (inputImg.Width / 2);

            if (RightToLeft.IsChecked.Value)
            {
                imgs.Add(this.ResizeImage(inputImg, imgWidth, imgWidth));
                imgs.Add(this.ResizeImage(inputImg, 0, imgWidth));
            }
            else
            {
                imgs.Add(this.ResizeImage(inputImg, 0, imgWidth));
                imgs.Add(this.ResizeImage(inputImg, imgWidth, imgWidth));
            }

            return imgs;
        }

        private Image ResizeImage(Image imgToResize, int x, int width)
        {
            Rectangle cropRect = new Rectangle(x, 0, width, imgToResize.Height);
            Bitmap src = imgToResize as Bitmap;
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height),
                                 cropRect,
                                 GraphicsUnit.Pixel);
            }

            return target;
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}
