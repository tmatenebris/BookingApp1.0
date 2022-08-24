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
        private int numOfPages = 0;
        private List<BookingView> _bookings;
        public MyBookingsView()
        {
            InitializeComponent();

            Loaded += PrivateBookingsView_Load;

            if (App.appuser.Role == "admin")
            {
                Label.Text = "List Of Bookings:";
                UserCol.Visibility = Visibility.Visible;
                SearchBy.Items.Add("Username");
                Order_By.Items.Add("Username");

            }
            SearchBy.Items.Add("Hall Name");
            SearchBy.Items.Add("Owner");
            Order_By.Items.Add("From Date");
            Order_By.Items.Add("Total Price");
            Order_By.Items.Add("Owner");
            Order_By.Items.Add("To Date");
            Order_By.Items.Add("Hall Name");
            Direction.Items.Add("ASCENDING");
            Direction.Items.Add("DESCENDING");

        }

        private async Task<IPagedList<BookingView>> GetPagedListAsync(int pageNumber = 1, int pageSize = 7)
        {
            return await Task.Factory.StartNew(() =>
            {
                numOfPages = (_bookings.Count() + pageSize - 1) / pageSize;
                return _bookings.ToPagedList(pageNumber, pageSize);
            });


        }

        private async Task<List<BookingView>> GetMyBookingsAsync()
        {
            return await Task.Factory.StartNew(() =>
            {
                string response = "error";
                if (App.appuser.Role == "user")
                {
                    response = TCPClient.ServerRequestWithResponse("[(GET_MY_BOOKINGS)]");
                }
                else
                {
                    response = TCPClient.ServerRequestWithResponse("[(GET_ALL_BOOKINGS)]");
                }
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
            Number.Text = pageNumber.ToString();
            MaxNumber.Text = numOfPages.ToString();
            PrevPage.IsEnabled = _cview.HasPreviousPage;
            NextPage.IsEnabled = _cview.HasNextPage;
            bookingList.DataContext = _cview.ToList();
            ProgressBar.IsIndeterminate = false;

        }
        private async void OnPreviousClicked(object sender, RoutedEventArgs e)
        {
            if (_cview.HasPreviousPage)
            {
                _cview = await GetPagedListAsync(--pageNumber);
                PrevPage.IsEnabled = _cview.HasPreviousPage;
                NextPage.IsEnabled = _cview.HasNextPage;
                bookingList.DataContext = _cview.ToList();
                Number.Text = pageNumber.ToString();
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
                Number.Text = pageNumber.ToString();
            }
        }


        private async Task<string> DeleteRequest(int? booking_id)
        {
            return await Task.Factory.StartNew(() =>
            {
                string response = TCPClient.ServerRequestWithResponse("[(DELETE_BOOKING)]:(" + booking_id.ToString() + ")");
                return response;
            });
        }


        private async void DeleteBooking(object sender, RoutedEventArgs e)
        {
            var clicked = bookingList.SelectedItem as BookingView;


            string response = await DeleteRequest(clicked.BookingId);

            if (response == "error") MessageBox.Show("Unable to delete Booking");
            else
            {
                if (_bookings != null) _bookings.Remove(clicked);
                _cview = await GetPagedListAsync(pageNumber);
                MaxNumber.Text = numOfPages.ToString();
                PrevPage.IsEnabled = _cview.HasPreviousPage;
                NextPage.IsEnabled = _cview.HasNextPage;
                bookingList.DataContext = _cview.ToList();
                Number.Text = pageNumber.ToString();
            }

        }


        private async Task<IPagedList<BookingView>> GetPagedListAsyncHallNameFiltered(string hallname, int pageNumber = 1, int pageSize = 7)
        {
            return await Task.Factory.StartNew(() =>
            {
                numOfPages = (_bookings.Where(s => s.Name.Contains(hallname, StringComparison.OrdinalIgnoreCase)).ToList().Count() + pageSize - 1) / pageSize;
                return _bookings.Where(s => s.Name.Contains(hallname, StringComparison.OrdinalIgnoreCase)).ToList().ToPagedList(pageNumber, pageSize);
            });


        }

        private async Task<IPagedList<BookingView>> GetPagedListAsyncOwnerFiltered(string owner, int pageNumber = 1, int pageSize = 7)
        {
            return await Task.Factory.StartNew(() =>
            {
                numOfPages = (_bookings.Where(s => s.Owner.Contains(owner, StringComparison.OrdinalIgnoreCase)).ToList().Count() + pageSize - 1) / pageSize;
                return _bookings.Where(s => s.Owner.Contains(owner, StringComparison.OrdinalIgnoreCase)).ToList().ToPagedList(pageNumber, pageSize);
            });


        }

        private async Task<IPagedList<BookingView>> GetPagedListAsyncUserFiltered(string username, int pageNumber = 1, int pageSize = 7)
        {
            return await Task.Factory.StartNew(() =>
            {
                numOfPages = (_bookings.Where(s => s.Username.Contains(username, StringComparison.OrdinalIgnoreCase)).ToList().Count() + pageSize - 1) / pageSize;
                return _bookings.Where(s => s.Username.Contains(username, StringComparison.OrdinalIgnoreCase)).ToList().ToPagedList(pageNumber, pageSize);
            });


        }

        private async void FilterBy(object sender, TextChangedEventArgs e)
        {
            if (txtFilter.Text != String.Empty)
            {
                OrderButton.IsEnabled = false;
                SearchBy.IsEnabled = false;
                if (SearchBy.Text == "Hall Name")
                {
                    _cview = await GetPagedListAsyncHallNameFiltered(txtFilter.Text, 1);
                    MaxNumber.Text = numOfPages.ToString();
                    PrevPage.IsEnabled = _cview.HasPreviousPage;
                    NextPage.IsEnabled = _cview.HasNextPage;
                    bookingList.DataContext = _cview.ToList();
                    pageNumber = 1;
                    Number.Text = pageNumber.ToString();
                }
                else if (SearchBy.Text == "Owner")
                {
                    _cview = await GetPagedListAsyncOwnerFiltered(txtFilter.Text, 1);
                    MaxNumber.Text = numOfPages.ToString();
                    PrevPage.IsEnabled = _cview.HasPreviousPage;
                    NextPage.IsEnabled = _cview.HasNextPage;
                    bookingList.DataContext = _cview.ToList();
                    pageNumber = 1;
                    Number.Text = pageNumber.ToString();
                }
                else if (SearchBy.Text == "Username")
                {
                    _cview = await GetPagedListAsyncUserFiltered(txtFilter.Text, 1);
                    MaxNumber.Text = numOfPages.ToString();
                    PrevPage.IsEnabled = _cview.HasPreviousPage;
                    NextPage.IsEnabled = _cview.HasNextPage;
                    bookingList.DataContext = _cview.ToList();
                    pageNumber = 1;
                    Number.Text = pageNumber.ToString();
                }


            }
            else
            {
                SearchBy.IsEnabled = true;
                OrderButton.IsEnabled = true;
                _cview = await GetPagedListAsync(1);
                MaxNumber.Text = numOfPages.ToString();
                bookingList.DataContext = _cview.ToList();
                PrevPage.IsEnabled = _cview.HasPreviousPage;
                NextPage.IsEnabled = _cview.HasNextPage;
                pageNumber = 1;
                Number.Text = pageNumber.ToString();
            }
        }

        private async void OrderBy(object sender, RoutedEventArgs e)
        {
            if(Order_By.Text == "Hall Name")
            {
                if(Direction.Text == "ASCENDING")
                {
                    _bookings = _bookings.OrderBy(s => s.Name).ToList();
                }
                else
                {
                    _bookings = _bookings.OrderByDescending(s => s.Name).ToList();
                }

                _cview = await GetPagedListAsync(1);
                MaxNumber.Text = numOfPages.ToString();
                PrevPage.IsEnabled = _cview.HasPreviousPage;
                NextPage.IsEnabled = _cview.HasNextPage;
                bookingList.DataContext = _cview.ToList();
                pageNumber = 1;
                Number.Text = pageNumber.ToString();

            }
            else if (Order_By.Text == "Owner")
            {
                if (Direction.Text == "ASCENDING")
                {
                    _bookings = _bookings.OrderBy(s => s.Owner).ToList();
                }
                else
                {
                    _bookings = _bookings.OrderByDescending(s => s.Owner).ToList();
                }

                _cview = await GetPagedListAsync(1);
                MaxNumber.Text = numOfPages.ToString();
                PrevPage.IsEnabled = _cview.HasPreviousPage;
                NextPage.IsEnabled = _cview.HasNextPage;
                bookingList.DataContext = _cview.ToList();
                pageNumber = 1;
                Number.Text = pageNumber.ToString();
            }
            else if (Order_By.Text == "From Date")
            {
                if (Direction.Text == "ASCENDING")
                {
                    _bookings = _bookings.OrderBy(s => s.FromDate).ToList();
                }
                else
                {
                    _bookings = _bookings.OrderByDescending(s => s.FromDate).ToList();
                }

                _cview = await GetPagedListAsync(1);
                MaxNumber.Text = numOfPages.ToString();
                PrevPage.IsEnabled = _cview.HasPreviousPage;
                NextPage.IsEnabled = _cview.HasNextPage;
                bookingList.DataContext = _cview.ToList();
                pageNumber = 1;
                Number.Text = pageNumber.ToString();
            }
            else if (Order_By.Text == "To Date")
            {
                if (Direction.Text == "ASCENDING")
                {
                    _bookings = _bookings.OrderBy(s => s.ToDate).ToList();
                }
                else
                {
                    _bookings = _bookings.OrderByDescending(s => s.ToDate).ToList();
                }

                _cview = await GetPagedListAsync(1);
                MaxNumber.Text = numOfPages.ToString();
                PrevPage.IsEnabled = _cview.HasPreviousPage;
                NextPage.IsEnabled = _cview.HasNextPage;
                bookingList.DataContext = _cview.ToList();
                pageNumber = 1;
                Number.Text = pageNumber.ToString();
            }
            else if (Order_By.Text == "Total Price")
            {
                if (Direction.Text == "ASCENDING")
                {
                    _bookings = _bookings.OrderBy(s => s.TotalPrice).ToList();
                }
                else
                {
                    _bookings = _bookings.OrderByDescending(s => s.TotalPrice).ToList();
                }

                _cview = await GetPagedListAsync(1);
                MaxNumber.Text = numOfPages.ToString();
                PrevPage.IsEnabled = _cview.HasPreviousPage;
                NextPage.IsEnabled = _cview.HasNextPage;
                bookingList.DataContext = _cview.ToList();
                pageNumber = 1;
                Number.Text = pageNumber.ToString();
            }
        }
    }
}
