using Database.Models;
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

namespace BookingApp1._0
{
    /// <summary>
    /// Logika interakcji dla klasy UserScreen.xaml
    /// </summary>
    public partial class UserScreen : Window
    {
        public User user_to_modify = new User();
        public int closing_mode = 0;
        public UserScreen(User clicked)
        {
            InitializeComponent();
            user_to_modify.UserId = clicked.UserId;
            Username.Text = clicked.Username;
            Username.IsReadOnly = true;
            FirstName.Text = clicked.FirstName;
            LastName.Text = clicked.LastName;
            PhoneNumber.Text = clicked.PhoneNumber;
            EMail.Text = clicked.Email;
        }

        private void UpdateUser(object sender, RoutedEventArgs e)
        {
            
            user_to_modify.Username = Username.Text;
            user_to_modify.FirstName = FirstName.Text;
            user_to_modify.LastName = LastName.Text;
            user_to_modify.PhoneNumber = PhoneNumber.Text;
            user_to_modify.Email = EMail.Text;
            closing_mode = 1;
            Close();
        }

        private void DeleteUser(object sender, RoutedEventArgs e)
        {
            user_to_modify.Username = Username.Text;
            user_to_modify.FirstName = FirstName.Text;
            user_to_modify.LastName = LastName.Text;
            user_to_modify.PhoneNumber = PhoneNumber.Text;
            user_to_modify.Email = EMail.Text;
            closing_mode = 2;
            Close();
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
