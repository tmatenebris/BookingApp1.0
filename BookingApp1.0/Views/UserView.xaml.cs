
using Database;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TCPConnection;

namespace BookingApp1._0.Views
{
    /// <summary>
    /// Logika interakcji dla klasy UserView.xaml
    /// </summary>
    public partial class UserView : UserControl
    {
  
        public UserView()
        {
            InitializeComponent();
            Username.Text = App.appuser.Username;
            FirstName.Text = App.appuser.FirstName;
            LastName.Text = App.appuser.LastName;
            PhoneNumber.Text = App.appuser.PhoneNumber;
            EMail.Text = App.appuser.Email;
        }

        private async Task<string> DeleteRequest(int user_id)
        {
            return await Task.Factory.StartNew(() =>
            {
                string response = TCPClient.ServerRequestWithResponse("[(DELETE_USER)]:(" + user_id.ToString() + ")");
                return response;
            });
        }

        private async Task<string> UpdateRequest(User to_up)
        {
            return await Task.Factory.StartNew(() =>
            {
                string response = TCPClient.ServerRequestWithResponse("[(UPDATE_USER)]" + XMLSerialize.Serialize<User>(to_up));
                return response;
            });
        }


        private async void UpdateUser(object sender, RoutedEventArgs e)
        {
             App.appuser.FirstName = FirstName.Text;
             App.appuser.LastName = LastName.Text;
             App.appuser.PhoneNumber = PhoneNumber.Text;
             App.appuser.Email = EMail.Text;
             string response = await UpdateRequest(App.appuser);
             if (response == "error") MessageBox.Show("Unable to Update User");
        }

        private async void DeleteUser(object sender, RoutedEventArgs e)
        {
            string response = await DeleteRequest(App.appuser.UserId);

            if (response == "error") MessageBox.Show("Unable to delete User");
            else
            {
                App.Current.Shutdown();

            }
        }

        private void PhoneNumberValid(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]{1,9}");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
