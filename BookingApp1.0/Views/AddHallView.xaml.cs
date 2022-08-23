using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
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
using Database.Models;
using Database;
using Database;

namespace BookingApp1._0.Views
{
    /// <summary>
    /// Logika interakcji dla klasy AddHallView.xaml
    /// </summary>
    public partial class AddHallView : UserControl
    {

        private BitmapImage thumb50x50;
        public AddHallView()
        {
            InitializeComponent();
        }

        private void UploadThumbnail(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Image files|*.bmp;*.jpg;*.png";
            fileDialog.FilterIndex = 1;

            if(fileDialog.ShowDialog() == true)
            {
                Bitmap orig = (Bitmap)System.Drawing.Image.FromFile(fileDialog.FileName);
                Bitmap resized = new Bitmap(orig, new System.Drawing.Size(450, 300));
                Bitmap res = new Bitmap(orig, new System.Drawing.Size(50, 50));
                thumb50x50 = ToBitmapImage(res);
                BitmapImage img = ToBitmapImage(resized);
              
                Thumbnail.Source = img;
            }
        }

        private void UploadOffer(object sender, RoutedEventArgs e)
        {
            HallDTO new_hall = new HallDTO();
     

            new_hall.OwnerId = App.appuser.UserId;
            new_hall.Name = NameSet.Text;
            new_hall.Location = LocationSet.Text;
            new_hall.Price = int.Parse(PriceSet.Text);
            new_hall.Capacity = int.Parse(CapacitySet.Text);
            new_hall.Description = GetXaml(DescRTB);
            

            BitmapImage image = Thumbnail.Source as BitmapImage;
            if (image == null)
            {
                image = new BitmapImage(GetAbsoluteUrlForLocalFile(Directory.GetCurrentDirectory() + "/Assets/image-placeholder.png"));
                thumb50x50 = new BitmapImage(GetAbsoluteUrlForLocalFile(Directory.GetCurrentDirectory() + "/Assets/rsz_image-placeholder.png"));
            }
            new_hall.Image = getJPGFromImageControl(image);
            new_hall.ThumbnailImage = getJPGFromImageControl(thumb50x50);
            string response = TCPConnection.TCPClient.ServerRequestWithResponse("[(ADD_HALL)]" + XMLSerialize.Serialize<HallDTO>(new_hall));
            if (response == "error") MessageBox.Show("Error Occured While Adding Offer");
            else MessageBox.Show("Succeed");
        }

        public static Uri GetAbsoluteUrlForLocalFile(string path)
        {
            var fileUri = new Uri(path, UriKind.RelativeOrAbsolute);

            if (fileUri.IsAbsoluteUri)
            {
                return fileUri;
            }
            else
            {
                var baseUri = new Uri(Directory.GetCurrentDirectory() + System.IO.Path.DirectorySeparatorChar);

                return new Uri(baseUri, fileUri);
            }
        }

        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        public byte[] getJPGFromImageControl(BitmapImage imageC)
        {
            MemoryStream memStream = new MemoryStream();
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(imageC));
            encoder.Save(memStream);
            return memStream.ToArray();
        }
        private static string GetXaml(RichTextBox rt)
        {
            TextRange range = new TextRange(rt.Document.ContentStart, rt.Document.ContentEnd);
            MemoryStream stream = new MemoryStream();
            range.Save(stream, DataFormats.Xaml);
            string xamlText = Encoding.UTF8.GetString(stream.ToArray());
            return xamlText;
        }

    }
}
