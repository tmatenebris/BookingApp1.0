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
using Database;
using Database.Models;
using BookingApp1._0.Commands;
using PagedList;

namespace BookingApp1._0.Views
{
    /// <summary>
    /// Logika interakcji dla klasy UsersView.xaml
    /// </summary>
    public partial class UsersView : UserControl
    {
        //private readonly PagingCollectionView<User> _cview;
        private IPagedList<User> _cview;
        public int pageNumber = 1;

        private List<User> _users;
        public UsersView()
        {
            InitializeComponent();

            Loaded += UserView_Load;
            UsersGrid.CanUserSortColumns = true;
        }

        private async Task<IPagedList<User>> GetPagedListAsync(int pageNumber = 1, int pageSize = 11)
        {
            return await Task.Factory.StartNew(() =>
            {
                return _users.ToPagedList(pageNumber, pageSize);
            });

            
        }

        private async Task<List<User>> GetUsersAsync()
        {
            return await Task.Factory.StartNew(() =>
            {
                string response = TCPClient.ServerRequestWithResponse("GetUsers");
                List<User> users = new List<User>();
                users = XMLSerialize.Deserialize<List<User>>(response);
                return users;
            });
        }

        private async void UserView_Load(object sender, EventArgs e)
        {
            _users = await GetUsersAsync();
            _cview = await GetPagedListAsync();
            PrevPage.IsEnabled = _cview.HasPreviousPage;
            NextPage.IsEnabled = _cview.HasNextPage;
            UsersGrid.DataContext = _cview.ToList();
            
        }
        private async void OnPreviousClicked(object sender, RoutedEventArgs e)
        {
           if(_cview.HasPreviousPage)
            {
                _cview = await GetPagedListAsync(--pageNumber);
                PrevPage.IsEnabled = _cview.HasPreviousPage;
                NextPage.IsEnabled = _cview.HasNextPage;
                UsersGrid.DataContext = _cview.ToList();
            }
        }

        private async void OnNextClicked(object sender, RoutedEventArgs e)
        {
            if (_cview.HasNextPage)
            {
                _cview = await GetPagedListAsync(++pageNumber);
                PrevPage.IsEnabled = _cview.HasPreviousPage;
                NextPage.IsEnabled = _cview.HasNextPage;
                UsersGrid.DataContext = _cview.ToList();
            }
        }
    }
}
