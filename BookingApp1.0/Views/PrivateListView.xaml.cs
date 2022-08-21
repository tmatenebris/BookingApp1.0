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
using BookingApp1._0.Commands;
using Database;
using Database.Models;
using PagedList;
using TCPConnection;

namespace BookingApp1._0.Views
{
    /// <summary>
    /// Logika interakcji dla klasy PrivateListView.xaml
    /// </summary>
    public partial class PrivateListView : UserControl
    {

        private IPagedList<HallDTO> _cview;
        public int pageNumber = 1;

        private List<HallDTO> _users;
        public PrivateListView()
        {
            InitializeComponent();

            Loaded += PrivateListView_Load;
            MyHallsGrid.CanUserSortColumns = true;
        }

        private async Task<IPagedList<HallDTO>> GetPagedListAsync(int pageNumber = 1, int pageSize = 11)
        {
            return await Task.Factory.StartNew(() =>
            {
                return _users.ToPagedList(pageNumber, pageSize);
            });


        }

        private async Task<List<HallDTO>> GetMyHallsAsync()
        {
            return await Task.Factory.StartNew(() =>
            {
                string response = TCPClient.ServerRequestWithResponse("GetMyHalls");
                List<HallDTO> users = new List<HallDTO>();
                users = XMLSerialize.Deserialize<List<HallDTO>>(response);
                return users;
            });
        }

        private async void PrivateListView_Load(object sender, EventArgs e)
        {
            _users = await GetMyHallsAsync();
            _cview = await GetPagedListAsync();
            PrevPage.IsEnabled = _cview.HasPreviousPage;
            NextPage.IsEnabled = _cview.HasNextPage;
            MyHallsGrid.DataContext = _cview.ToList();

        }
        private async void OnPreviousClicked(object sender, RoutedEventArgs e)
        {
            if (_cview.HasPreviousPage)
            {
                _cview = await GetPagedListAsync(--pageNumber);
                PrevPage.IsEnabled = _cview.HasPreviousPage;
                NextPage.IsEnabled = _cview.HasNextPage;
                MyHallsGrid.DataContext = _cview.ToList();
            }
        }

        private async void OnNextClicked(object sender, RoutedEventArgs e)
        {
            if (_cview.HasNextPage)
            {
                _cview = await GetPagedListAsync(++pageNumber);
                PrevPage.IsEnabled = _cview.HasPreviousPage;
                NextPage.IsEnabled = _cview.HasNextPage;
                MyHallsGrid.DataContext = _cview.ToList();
            }
        }

        private void OpenFiltersWindow(object sender, RoutedEventArgs e)
        {
            FilterScreen win = new FilterScreen();
            win.ShowDialog();
        }
    }
}
