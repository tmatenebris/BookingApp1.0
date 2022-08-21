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

namespace BookingApp1._0
{
    /// <summary>
    /// Logika interakcji dla klasy OfferScreen.xaml
    /// </summary>
    public partial class OfferScreen : Window
    {
       
        public OfferScreen(int offer_id)
        {
            InitializeComponent();

            string result = TCPConnection.TCPClient.ServerRequestWithResponse("GetOffer: <" + offer_id.ToString() + ">");

            if (result != "error")
            {
                OfferDTO curr_offer = XMLSerialize.Deserialize<OfferDTO>(result);

                HallName.Text = curr_offer.Name;
                HallLocation.Text = curr_offer.Location;
                HallPrice.Text = curr_offer.Price.ToString();
                HallCapacity.Text = curr_offer.Capacity.ToString();
                OwnerEmail.Text = curr_offer.Email;
                OwnerName.Text = curr_offer.FirstName;
                OwnerLastName.Text = curr_offer.LastName;
                OwnerPhoneNumber.Text = curr_offer.PhoneNumber;
                OfferThumbnail.Source = ByteToImage(curr_offer.Image);

                DocReader.Document = SetRTF(curr_offer.Description);
            }
            else MessageBox.Show("Unable to load offer");


        }

        private void Book(object sender, RoutedEventArgs e)
        {

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
    }
}
