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
using BookingApp1._0.Views;
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
            if(user.Role == "user")
            {
                
                UsersButton.Visibility = Visibility.Collapsed;
                Sep1.Visibility = Visibility.Collapsed;
            }
            else
            {
                UserButton.Visibility = Visibility.Collapsed;
                Sep2.Visibility = Visibility.Collapsed;
            }

        }

        private void CloseApp(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }
    }
}
