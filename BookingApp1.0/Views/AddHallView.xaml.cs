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
using BookingApp1._0.Helpers;
using System.Text.RegularExpressions;

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
                thumb50x50 = ImageProcessing.ToBitmapImage(res);
                BitmapImage img = ImageProcessing.ToBitmapImage(resized);
              
                Thumbnail.Source = img;
            }
        }

        private void UploadOffer(object sender, RoutedEventArgs e)
        {
            if (NameSet.Text != String.Empty && LocationSet.Text != String.Empty && PriceSet.Text != String.Empty && CapacitySet.Text != String.Empty)
            {
                HallDTO new_hall = new HallDTO();


                new_hall.OwnerId = App.appuser.UserId;
                new_hall.Name = NameSet.Text;
                new_hall.Location = LocationSet.Text;
                new_hall.Price = int.Parse(PriceSet.Text);
                new_hall.Capacity = int.Parse(CapacitySet.Text);
                new_hall.Description = DocumentsProcessing.GetXaml(DescRTB);


                BitmapImage image = Thumbnail.Source as BitmapImage;

                if (image == null)
                {
                    image = new BitmapImage(LocalFilesProcessing.GetAbsoluteUrlForLocalFile(Directory.GetCurrentDirectory() + "/Assets/image-placeholder.png"));
                    thumb50x50 = new BitmapImage(LocalFilesProcessing.GetAbsoluteUrlForLocalFile(Directory.GetCurrentDirectory() + "/Assets/rsz_image-placeholder.png"));
                }
                new_hall.Image = ImageProcessing.GetJPGFromImageControl(image);
                new_hall.ThumbnailImage = ImageProcessing.GetJPGFromImageControl(thumb50x50);
                string response = TCPConnection.TCPClient.ServerRequestWithResponse("[(ADD_HALL)]" + XMLSerialize.Serialize<HallDTO>(new_hall));
                if (response == "error") MessageBox.Show("Error Occured While Adding Offer");
                else MessageBox.Show("Succeed");
            }
            else MessageBox.Show("You must fill out all the gaps");
        }

        private void NumberValid(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]{1,8}");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
