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
using Database.Models;
using Database;
using System.IO;
using System.Xml;
using System.Windows.Markup;
using System.Drawing;
using System.Drawing.Imaging;
using BookingApp1._0.Helpers;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace BookingApp1._0
{
    /// <summary>
    /// Logika interakcji dla klasy OfferScreen.xaml
    /// </summary>
    public partial class OfferScreen : Window
    {
        public  HallDTO offerHall;
        public int closing_mode = 0;
        private static BitmapImage thumb50x50;
        public OfferScreen(HallDTO current)
        {
            InitializeComponent();
            offerHall = current;

            Loaded += OfferScreen_Load;
        }

        private async void OfferScreen_Load(object sender, EventArgs e)
        {
            try
            {
                if ((offerHall.OwnerId == App.appuser.UserId) || (App.appuser.Role == "admin"))
                {
                    BookButton.Visibility = Visibility.Hidden;
                    UpdateButton.Visibility = Visibility.Visible;
                    mainToolBar.Visibility = Visibility.Visible;
                    DocReader.IsReadOnly = false;
                    FromDate.Visibility = Visibility.Hidden;
                    ToDate.Visibility = Visibility.Hidden;
                    HallName.IsReadOnly = false;
                    HallPrice.IsReadOnly = false;
                    HallLocation.IsReadOnly = false;
                    HallCapacity.IsReadOnly = false;
                    UploadButton.Visibility = Visibility.Visible;
                    Block.Visibility = Visibility.Hidden;
                }

                string result = TCPConnection.TCPClient.ServerRequestWithResponse("[(GET_OFFER)]: (" + offerHall.HallId.ToString() + ")");

                if (result != "error")
                {
                    OfferDTO curr_offer = XMLSerialize.Deserialize<OfferDTO>(result);

                    HallName.Text = curr_offer.Name;
                    HallLocation.Text = curr_offer.Location;
                    HallPrice.Text = curr_offer.Price.ToString();
                    HallCapacity.Text = curr_offer.Capacity.ToString();
                    OwnerEmail.Text = curr_offer.Email;
                    OwnerName.Text = curr_offer.FirstName;
                    OwnerSurname.Text = curr_offer.LastName;
                    OwnerPhone.Text = curr_offer.PhoneNumber;
                    OfferThumbnail.Source = ImageProcessing.ByteToImage(curr_offer.Image);

                    DocReader.Document = DocumentsProcessing.SetRTF(curr_offer.Description);


                }
                else MessageBox.Show("Unable to load offer");
            }
            catch { MessageBox.Show("Error"); }
        }

        private void Book(object sender, RoutedEventArgs e)
        {
            if (FromDate.SelectedDate.HasValue && ToDate.SelectedDate.HasValue)
            {
                Booking new_booking = new Booking();

                new_booking.UserId = App.appuser.UserId;
                new_booking.HallId = offerHall.HallId;
                new_booking.OwnerId = offerHall.OwnerId;
                new_booking.FromDate = new DateOnly(FromDate.SelectedDate.Value.Year, FromDate.SelectedDate.Value.Month, FromDate.SelectedDate.Value.Day);
                new_booking.ToDate = new DateOnly(ToDate.SelectedDate.Value.Year, ToDate.SelectedDate.Value.Month, ToDate.SelectedDate.Value.Day);
                var date = ToDate.SelectedDate.Value - FromDate.SelectedDate.Value;
                new_booking.TotalPrice = date.Days * offerHall.Price;


                string response = TCPConnection.TCPClient.ServerRequestWithResponse("[(ADD_BOOKING)]" + XMLSerialize.Serialize<Booking>(new_booking));
                if (response == "error") MessageBox.Show("Error");
                else MessageBox.Show("Succeed");
            }
            else MessageBox.Show("You must pick dates!");
        }




        private void CloseOfferWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void UpdateOffer(object sender, RoutedEventArgs e)
        {
            if (HallName.Text != String.Empty && HallLocation.Text != String.Empty && HallPrice.Text != String.Empty && HallCapacity.Text != String.Empty)
            {
                HallDTO new_hall = new HallDTO();

                new_hall.HallId = offerHall.HallId;
                new_hall.OwnerId = App.appuser.UserId;
                new_hall.Name = HallName.Text;
                new_hall.Location = HallLocation.Text;
                new_hall.Price = int.Parse(HallPrice.Text);
                new_hall.Capacity = int.Parse(HallCapacity.Text);
                new_hall.Description = DocumentsProcessing.GetXaml(DocReader);
                if (thumb50x50 == null) thumb50x50 = (BitmapImage)ImageProcessing.ByteToImage(offerHall.ThumbnailImage);

                BitmapImage image = OfferThumbnail.Source as BitmapImage;
                if (image == null)
                {
                    image = new BitmapImage(LocalFilesProcessing.GetAbsoluteUrlForLocalFile(Directory.GetCurrentDirectory() + "/Assets/image-placeholder.png"));

                }
                new_hall.Image = ImageProcessing.GetJPGFromImageControl(image);
                new_hall.ThumbnailImage = ImageProcessing.GetJPGFromImageControl(thumb50x50);
                string response = TCPConnection.TCPClient.ServerRequestWithResponse("[(UPDATE_HALL)]" + XMLSerialize.Serialize<HallDTO>(new_hall));
                if (response == "error") MessageBox.Show("Error Occured While Adding Offer");
                else MessageBox.Show("Succeed");

                offerHall.Name = new_hall.Name;
                offerHall.Location = new_hall.Location;
                offerHall.Price = new_hall.Price;
                offerHall.Capacity = new_hall.Capacity;
                offerHall.Description = new_hall.Description;
                offerHall.ThumbnailImage = new_hall.ThumbnailImage;
                closing_mode = 1;
            }
            else MessageBox.Show("You must fill out the gapes!");
        }

        private void UploadThumbnail(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Image files|*.bmp;*.jpg;*.png";
            fileDialog.FilterIndex = 1;

            if (fileDialog.ShowDialog() == true)
            {
                Bitmap orig = (Bitmap)System.Drawing.Image.FromFile(fileDialog.FileName);
                Bitmap resized = new Bitmap(orig, new System.Drawing.Size(450, 300));
                Bitmap res = new Bitmap(orig, new System.Drawing.Size(50, 50));
                thumb50x50 = ImageProcessing.ToBitmapImage(res);
                BitmapImage img = ImageProcessing.ToBitmapImage(resized);

                OfferThumbnail.Source = img;
            }
        }


        private void NumberValid(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]{1,8}");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
