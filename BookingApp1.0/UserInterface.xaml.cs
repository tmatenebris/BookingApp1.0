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
using BookingApp1._0.ViewModels;
using Database.Models;

namespace BookingApp1._0
{
    /// <summary>
    /// Logika interakcji dla klasy UserInterface.xaml
    /// </summary>
    public partial class UserInterface : Window
    {
        
        public UserInterface(User user)
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            UserBox.Text = user.FirstName + " " + user.LastName;
            UserInitials.Text = user.FirstName.Substring(0, 1).ToUpper() + user.LastName.Substring(0, 1).ToUpper(); 
            if(user.Role == "user")
            {
                UsersButton.Visibility = Visibility.Collapsed;
            }
        }

        private void CloseApp(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }
    }
}
