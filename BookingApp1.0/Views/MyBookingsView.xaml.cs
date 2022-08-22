using Database;
using Database.Models;
using PagedList;
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

namespace BookingApp1._0.Views
{
    /// <summary>
    /// Logika interakcji dla klasy MyBookingsView.xaml
    /// </summary>
    public partial class MyBookingsView : UserControl
    {
        private IPagedList<BookingView> _cview;
        public int pageNumber = 1;

        private List<BookingView> _bookings;
        public MyBookingsView()
        {
            InitializeComponent();

            Loaded += PrivateBookingsView_Load;
     
        }

        private async Task<IPagedList<BookingView>> GetPagedListAsync(int pageNumber = 1, int pageSize = 11)
        {
            return await Task.Factory.StartNew(() =>
            {
                return _bookings.ToPagedList(pageNumber, pageSize);
            });


        }

        private async Task<List<BookingView>> GetMyBookingsAsync()
        {
            return await Task.Factory.StartNew(() =>
            {
                string response = TCPClient.ServerRequestWithResponse("GetMyBookings");
                List<BookingView> bookings = new List<BookingView>();
                bookings = XMLSerialize.Deserialize<List<BookingView>>(response);
                return bookings;
            });
        }

        private async void PrivateBookingsView_Load(object sender, EventArgs e)
        {
            ProgressBar.IsIndeterminate = true;
            _bookings = await GetMyBookingsAsync();
            _cview = await GetPagedListAsync();
            ProgressBar.IsIndeterminate = false;
            PrevPage.IsEnabled = _cview.HasPreviousPage;
            NextPage.IsEnabled = _cview.HasNextPage;
            bookingList.DataContext = _cview.ToList();

        }
        private async void OnPreviousClicked(object sender, RoutedEventArgs e)
        {
            if (_cview.HasPreviousPage)
            {
                _cview = await GetPagedListAsync(--pageNumber);
                PrevPage.IsEnabled = _cview.HasPreviousPage;
                NextPage.IsEnabled = _cview.HasNextPage;
                bookingList.DataContext = _cview.ToList();
            }
        }

        private async void OnNextClicked(object sender, RoutedEventArgs e)
        {
            if (_cview.HasNextPage)
            {
                _cview = await GetPagedListAsync(++pageNumber);
                PrevPage.IsEnabled = _cview.HasPreviousPage;
                NextPage.IsEnabled = _cview.HasNextPage;
                bookingList.DataContext = _cview.ToList();
            }
        }

        private void OpenFiltersWindow(object sender, RoutedEventArgs e)
        {
            FilterScreen win = new FilterScreen();
            win.ShowDialog();
        }
    }
}
