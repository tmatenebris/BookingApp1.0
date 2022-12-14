using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Database;
using Database.Models;

namespace BookingApp1._0
{
    /// <summary>
    /// Logika interakcji dla klasy RegisterScreen.xaml
    /// </summary>
    public partial class RegisterScreen : Window
    {
        public RegisterScreen()
        {
            InitializeComponent();
        }

        private void RegisterAccount(object sender, RoutedEventArgs e)
        {
            if (Username.Text != String.Empty && FirstName.Text != String.Empty && LastName.Text != String.Empty && EMail.Text != String.Empty && PhoneNumber.Text != String.Empty && Password.Password != String.Empty)
            {
                User user = new User();
                user.Username = Username.Text;
                user.FirstName = FirstName.Text;
                user.LastName = LastName.Text;
                user.Email = EMail.Text;
                user.PhoneNumber = PhoneNumber.Text;
                user.Password = Password.Password;


                string response = TCPConnection.TCPClient.ServerRequestWithResponse("[(ADD_USER)]" + XMLSerialize.Serialize<User>(user));

                if (response == "succeed")
                {
                    MessageBox.Show("Registered!");
                    Close();
                }
                else
                {
                    MessageBox.Show("User with that username exists!");
                }
            }
            else MessageBox.Show("You must fill out the gapes!");
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PhoneNumberValid(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]{1,9}");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
