using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace app.Services
{
    public class CbzService
    {
        public string InputFolder { get; set; }

        public string Prefix { get; set; }

        public bool IsSinglePage { get; set; }

        public bool DoCompress { get; set; }

        public bool RightToLeft { get; set; }

        public void InitializeService(string inputFolder, string prefix, bool isSinglePage, bool doCompress, bool rightToLeft)
        {
            InputFolder = inputFolder;
            Prefix = prefix;
            IsSinglePage = isSinglePage;
            DoCompress = doCompress;
            RightToLeft = rightToLeft;
        }

        public void CreateFile()
        {
            Directory.EnumerateDirectories(InputFolder)
                .ToList()
                .ForEach(directory =>
                {
                    BrowseDirectory(directory);
                });
        }

        public void RenameFolder()
        {
            InputFolder = "H:\\Desktop\\Nouveau dossier";

            Dictionary<string, double> directories = Directory.EnumerateDirectories(InputFolder)
                .ToDictionary(x => x, x => double.Parse(x.Substring(InputFolder.Length).Replace("\\", string.Empty).Trim().Split(' ')[0], CultureInfo.InvariantCulture));

            int highDirectory = (int)directories.OrderBy(x => x.Value).LastOrDefault().Value;
            int totalWidth = highDirectory.ToString().Length + 2;
            foreach (KeyValuePair<string, double> sourceDirectory in directories)
            {
                string newFilename = sourceDirectory.Value.ToString("F1", CultureInfo.InvariantCulture)
                    .PadLeft(totalWidth, '0')
                    .Replace(".0", string.Empty);

                string destinationDirectory = $"{InputFolder}\\{newFilename}";
                if (sourceDirectory.Key != destinationDirectory)
                {
                    Directory.Move(sourceDirectory.Key, destinationDirectory);
                }
            }
        }

        #region Private

        private static IEnumerable<string> ImageExtensions = new List<string> { "jpg", "jpeg", "jpe", "bmp", "gif", "png" };

        private string TempFolderPath => $"{InputFolder}\\temp";

        private string DestinationName(string tempFolder, int counter) => $"{ tempFolder }\\{ string.Format("{0:00000}", counter) }.jpg";

        private Image GetImageByPath(string inputPath) => Image.FromFile(inputPath);

        private void BrowseDirectory(string directory)
        {
            Directory.CreateDirectory(TempFolderPath);

            int counter = 0;
            Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories)
                .ToList()
                .ForEach(file =>
                {
                    BrowseFile(file, ref counter);
                });

            ZipFile.CreateFromDirectory(TempFolderPath, $"{ InputFolder}\\{ Prefix } { directory.Split('\\').Last() }.cbz");

            Directory.Delete(TempFolderPath, true);
            Directory.Delete(directory, true);
        }

        private void BrowseFile(string file, ref int counter)
        {
            if (ImageExtensions.Contains(file.Split('.').Last()))
            {
                using (Image image = GetImageByPath(file))
                {
                    if (!IsSinglePage)
                    {
                        if (image.Width > image.Height)
                        {
                            IEnumerable<Image> imgs = SplitDoublePage(image);
                            foreach (Image splitedImg in imgs)
                            {
                                SaveSinglePage(splitedImg, DestinationName(TempFolderPath, counter));
                                counter++;
                            }
                        }
                        else
                        {
                            SaveSinglePage(image, DestinationName(TempFolderPath, counter));
                            counter++;
                        }
                    }
                    else
                    {
                        SaveSinglePage(image, DestinationName(TempFolderPath, counter));
                        counter++;
                    }
                }
            }
        }

        private byte[] ConvertToJPG(Image input)
        {
            Bitmap image = input as Bitmap;
            using (MemoryStream ms = new MemoryStream())
            {
                if (DoCompress)
                {
                    EncoderParameters myEncoderParameters = new EncoderParameters(1);
                    myEncoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 50L);

                    image.Save(ms, GetEncoder(ImageFormat.Jpeg), myEncoderParameters);
                }
                else
                {
                    image.Save(ms, ImageFormat.Jpeg);
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
            byte[] imageBuffer = ConvertToJPG(img);
            SaveImage(imageBuffer, destinationPath);
        }

        private IEnumerable<Image> SplitDoublePage(Image inputImg)
        {
            var imgs = new List<Image>();
            int imgWidth = (inputImg.Width / 2);

            if (RightToLeft)
            {
                imgs.Add(ResizeImage(inputImg, imgWidth, imgWidth));
                imgs.Add(ResizeImage(inputImg, 0, imgWidth));
            }
            else
            {
                imgs.Add(ResizeImage(inputImg, 0, imgWidth));
                imgs.Add(ResizeImage(inputImg, imgWidth, imgWidth));
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
                g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height), cropRect, GraphicsUnit.Pixel);
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

        #endregion Private
    }
}
