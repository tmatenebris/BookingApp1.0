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

namespace BookingApp1._0
{
    /// <summary>
    /// Logika interakcji dla klasy OfferScreen.xaml
    /// </summary>
    public partial class OfferScreen : Window
    {
        private static HallDTO offerHall;
        private static BitmapImage thumb50x50;
        public OfferScreen(HallDTO current)
        {
            InitializeComponent();
            offerHall = current;
            if((offerHall.OwnerId == App.appuser.UserId) || (App.appuser.Role == "admin"))
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
           
            string result = TCPConnection.TCPClient.ServerRequestWithResponse("GetOffer: <" + offerHall.HallId.ToString() + ">");

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
                OfferThumbnail.Source = ByteToImage(curr_offer.Image);

                DocReader.Document = SetRTF(curr_offer.Description);
                
                
            }
            else MessageBox.Show("Unable to load offer");

            
        }

        private void Book(object sender, RoutedEventArgs e)
        {
            Booking new_booking = new Booking();

            new_booking.UserId = App.appuser.UserId;
            new_booking.HallId = offerHall.HallId;
            new_booking.OwnerId = offerHall.OwnerId;
            new_booking.FromDate = new DateOnly(FromDate.SelectedDate.Value.Year, FromDate.SelectedDate.Value.Month, FromDate.SelectedDate.Value.Day);
            new_booking.ToDate = new DateOnly(ToDate.SelectedDate.Value.Year, ToDate.SelectedDate.Value.Month, ToDate.SelectedDate.Value.Day);
            var date = ToDate.SelectedDate.Value - FromDate.SelectedDate.Value;
            new_booking.TotalPrice = date.Days * offerHall.Price;


            string response = TCPConnection.TCPClient.ServerRequestWithResponse("[(ADD_BOOKING)]"+XMLSerialize.Serialize<Booking>(new_booking));
            if (response == "error") MessageBox.Show("Error");
            else MessageBox.Show("Succeed");
        }

        private static FlowDocument SetRTF(string xamlString)
        {
            StringReader stringReader = new StringReader(xamlString);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            Section sec = XamlReader.Load(xmlReader) as Section;
            FlowDocument doc = new FlowDocument();
            while (sec.Blocks.Count > 0)
                doc.Blocks.Add(sec.Blocks.FirstBlock);
            return doc;
        }
        public static ImageSource ByteToImage(byte[] imageData)
        {
            BitmapImage biImg = new BitmapImage();
            MemoryStream ms = new MemoryStream(imageData);
            biImg.BeginInit();
            biImg.StreamSource = ms;
            biImg.EndInit();

            ImageSource imgSrc = biImg as ImageSource;

            return imgSrc;
        }

        private void CloseOfferWindow(object sender, RoutedEventArgs e)
        {
            Close();
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
        private void UpdateOffer(object sender, RoutedEventArgs e)
        {
            HallDTO new_hall = new HallDTO();

            new_hall.HallId = offerHall.HallId;
            new_hall.OwnerId = App.appuser.UserId;
            new_hall.Name = HallName.Text;
            new_hall.Location = HallLocation.Text;
            new_hall.Price = int.Parse(HallPrice.Text);
            new_hall.Capacity = int.Parse(HallCapacity.Text);
            new_hall.Description = GetXaml(DocReader);
            thumb50x50 = (BitmapImage)ByteToImage(offerHall.ThumbnailImage);

            BitmapImage image = OfferThumbnail.Source as BitmapImage;
            if (image == null)
            {
                image = new BitmapImage(GetAbsoluteUrlForLocalFile(Directory.GetCurrentDirectory() + "/Assets/image-placeholder.png"));
                thumb50x50 = new BitmapImage(GetAbsoluteUrlForLocalFile(Directory.GetCurrentDirectory() + "/Assets/rsz_image-placeholder.png"));
            }
            new_hall.Image = getJPGFromImageControl(image);
            new_hall.ThumbnailImage = getJPGFromImageControl(thumb50x50);
            string response = TCPConnection.TCPClient.ServerRequestWithResponse("[(UPDATE_HALL)]" + XMLSerialize.Serialize<HallDTO>(new_hall));
            if (response == "error") MessageBox.Show("Error Occured While Adding Offer");
            else MessageBox.Show("Succeed");
        }

        private void UploadThumbnail(object sender, RoutedEventArgs e)
        {

        }
    }
}
