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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TCPConnection;
using Database.Models;
using Database;

namespace BookingApp1._0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.Close();
        }

        private void Login(object sender, RoutedEventArgs e)
        {
            string response = TCPClient.ServerRequestWithResponse("[(LOGIN)]: <" + LoginBox.Text + "> " + "[(Password)]: <" + PasswordBox.Password + ">");
            if (response == "error")
            {
                MessageBox.Show("Invalid login or password");
            }
            else
            {
                
                User user = new User();
                user = XMLSerialize.Deserialize<User>(response);
                UserInterface win = new UserInterface(user);
                App.appuser = user;
                //TCPClient.SetUserId(user.UserId);
                win.Show();
                Close();
            } 
        }

        private void Register(object sender, RoutedEventArgs e)
        {
            RegisterScreen win = new RegisterScreen();
            win.ShowDialog();

        }
    }
}
