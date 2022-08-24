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
        private IPagedList<User> _cview;
        public int pageNumber = 1;
        private int numOfPages = 0;
        private List<User> _users;
        public UsersView()
        {
            InitializeComponent();

            Loaded += UsersView_Load;
            SearchBy.Items.Add("Username");
            SearchBy.Items.Add("First Name");
            SearchBy.Items.Add("Last Name");
            SearchBy.Items.Add("EMail");

            Order_By.Items.Add("First Name");
            Order_By.Items.Add("Last Name");
            Order_By.Items.Add("First+Last Name");
            Order_By.Items.Add("EMail");
            Direction.Items.Add("ASCENDING");
            Direction.Items.Add("DESCENDING");

        }

        private async Task<IPagedList<User>> GetPagedListAsync(int pageNumber = 1, int pageSize = 11)
        {
            return await Task.Factory.StartNew(() =>
            {
                numOfPages = (_users.Count() + pageSize - 1) / pageSize;
                return _users.ToPagedList(pageNumber, pageSize);
            });


        }

        private async Task<List<User>> GetUsersAsync()
        {
            return await Task.Factory.StartNew(() =>
            {
                string response = "error";
                response = TCPClient.ServerRequestWithResponse("[(GET_USERS)]");
                List<User> bookings = new List<User>();
                bookings = XMLSerialize.Deserialize<List<User>>(response);
                return bookings;
            });
        }

        private async void UsersView_Load(object sender, EventArgs e)
        {

            ProgressBar.IsIndeterminate = true;
            _users = await GetUsersAsync();
            _cview = await GetPagedListAsync();
            Number.Text = pageNumber.ToString();
            MaxNumber.Text = numOfPages.ToString();
            PrevPage.IsEnabled = _cview.HasPreviousPage;
            NextPage.IsEnabled = _cview.HasNextPage;
            UsersGrid.DataContext = _cview.ToList();
            ProgressBar.IsIndeterminate = false;

        }
        private async void OnPreviousClicked(object sender, RoutedEventArgs e)
        {
            if (_cview.HasPreviousPage)
            {
                _cview = await GetPagedListAsync(--pageNumber);
                PrevPage.IsEnabled = _cview.HasPreviousPage;
                NextPage.IsEnabled = _cview.HasNextPage;
                UsersGrid.DataContext = _cview.ToList();
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
                UsersGrid.DataContext = _cview.ToList();
                Number.Text = pageNumber.ToString();
            }
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
                string response = TCPClient.ServerRequestWithResponse("[(UPDATE_USER)]"+XMLSerialize.Serialize<User>(to_up));
                return response;
            });
        }

        private async void UpdateUser(User to_up)
        {
            string response = await UpdateRequest(to_up);
            if (response == "error") MessageBox.Show("Unable to Update User");
        }

        private async void DeleteUser(object sender, RoutedEventArgs e)
        {
            var clicked = UsersGrid.SelectedItem as User;


            string response = await DeleteRequest(clicked.UserId);

            if (response == "error") MessageBox.Show("Unable to delete User");
            else
            {
                if (_users != null) _users.Remove(clicked);
                _cview = await GetPagedListAsync(pageNumber);
                MaxNumber.Text = numOfPages.ToString();
                PrevPage.IsEnabled = _cview.HasPreviousPage;
                NextPage.IsEnabled = _cview.HasNextPage;
                UsersGrid.DataContext = _cview.ToList();
                Number.Text = pageNumber.ToString();
            }

        }


        private async Task<IPagedList<User>> GetPagedListAsyncFirstNameFiltered(string firstname, int pageNumber = 1, int pageSize = 11)
        {
            return await Task.Factory.StartNew(() =>
            {
                numOfPages = (_users.Where(s => s.FirstName.Contains(firstname, StringComparison.OrdinalIgnoreCase)).ToList().Count() + pageSize - 1) / pageSize;
                return _users.Where(s => s.FirstName.Contains(firstname, StringComparison.OrdinalIgnoreCase)).ToList().ToPagedList(pageNumber, pageSize);
            });


        }

        private async Task<IPagedList<User>> GetPagedListAsyncLastNameFiltered(string lastname, int pageNumber = 1, int pageSize = 11)
        {
            return await Task.Factory.StartNew(() =>
            {
                numOfPages = (_users.Where(s => s.LastName.Contains(lastname, StringComparison.OrdinalIgnoreCase)).ToList().Count() + pageSize - 1) / pageSize;
                return _users.Where(s => s.LastName.Contains(lastname, StringComparison.OrdinalIgnoreCase)).ToList().ToPagedList(pageNumber, pageSize);
            });


        }

        private async Task<IPagedList<User>> GetPagedListAsyncUsernameFiltered(string username, int pageNumber = 1, int pageSize = 11)
        {
            return await Task.Factory.StartNew(() =>
            {
                numOfPages = (_users.Where(s => s.Username.Contains(username, StringComparison.OrdinalIgnoreCase)).ToList().Count() + pageSize - 1) / pageSize;
                return _users.Where(s => s.Username.Contains(username, StringComparison.OrdinalIgnoreCase)).ToList().ToPagedList(pageNumber, pageSize);
            });


        }

        private async Task<IPagedList<User>> GetPagedListAsyncEMailFiltered(string email, int pageNumber = 1, int pageSize = 11)
        {
            return await Task.Factory.StartNew(() =>
            {
                numOfPages = (_users.Where(s => s.Email.Contains(email, StringComparison.OrdinalIgnoreCase)).ToList().Count() + pageSize - 1) / pageSize;
                return _users.Where(s => s.Email.Contains(email, StringComparison.OrdinalIgnoreCase)).ToList().ToPagedList(pageNumber, pageSize);
            });


        }

        private async void FilterBy(object sender, TextChangedEventArgs e)
        {
            if (txtFilter.Text != String.Empty)
            {
                OrderButton.IsEnabled = false;
                SearchBy.IsEnabled = false;
                if (SearchBy.Text == "Username")
                {
                    _cview = await GetPagedListAsyncUsernameFiltered(txtFilter.Text, 1);
                    MaxNumber.Text = numOfPages.ToString();
                    PrevPage.IsEnabled = _cview.HasPreviousPage;
                    NextPage.IsEnabled = _cview.HasNextPage;
                    UsersGrid.DataContext = _cview.ToList();
                    pageNumber = 1;
                    Number.Text = pageNumber.ToString();
                }
                else if (SearchBy.Text == "First Name")
                {
                    _cview = await GetPagedListAsyncFirstNameFiltered(txtFilter.Text, 1);
                    MaxNumber.Text = numOfPages.ToString();
                    PrevPage.IsEnabled = _cview.HasPreviousPage;
                    NextPage.IsEnabled = _cview.HasNextPage;
                    UsersGrid.DataContext = _cview.ToList();
                    pageNumber = 1;
                    Number.Text = pageNumber.ToString();
                }
                else if (SearchBy.Text == "Last Name")
                {
                    _cview = await GetPagedListAsyncLastNameFiltered(txtFilter.Text, 1);
                    MaxNumber.Text = numOfPages.ToString();
                    PrevPage.IsEnabled = _cview.HasPreviousPage;
                    NextPage.IsEnabled = _cview.HasNextPage;
                    UsersGrid.DataContext = _cview.ToList();
                    pageNumber = 1;
                    Number.Text = pageNumber.ToString();
                }
                else if (SearchBy.Text == "EMail")
                {
                    _cview = await GetPagedListAsyncEMailFiltered(txtFilter.Text, 1);
                    MaxNumber.Text = numOfPages.ToString();
                    PrevPage.IsEnabled = _cview.HasPreviousPage;
                    NextPage.IsEnabled = _cview.HasNextPage;
                    UsersGrid.DataContext = _cview.ToList();
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
                UsersGrid.DataContext = _cview.ToList();
                PrevPage.IsEnabled = _cview.HasPreviousPage;
                NextPage.IsEnabled = _cview.HasNextPage;
                pageNumber = 1;
                Number.Text = pageNumber.ToString();
            }
        }

        private async void OrderBy(object sender, RoutedEventArgs e)
        {
            if (Order_By.Text == "Username")
            {
                if (Direction.Text == "ASCENDING")
                {
                    _users = _users.OrderBy(s => s.Username).ToList();
                }
                else
                {
                    _users = _users.OrderByDescending(s => s.Username).ToList();
                }

                _cview = await GetPagedListAsync(1);
                MaxNumber.Text = numOfPages.ToString();
                PrevPage.IsEnabled = _cview.HasPreviousPage;
                NextPage.IsEnabled = _cview.HasNextPage;
                UsersGrid.DataContext = _cview.ToList();
                pageNumber = 1;
                Number.Text = pageNumber.ToString();

            }
            else if (Order_By.Text == "First Name")
            {
                if (Direction.Text == "ASCENDING")
                {
                    _users = _users.OrderBy(s => s.FirstName).ToList();
                }
                else
                {
                    _users = _users.OrderByDescending(s => s.FirstName).ToList();
                }

                _cview = await GetPagedListAsync(1);
                MaxNumber.Text = numOfPages.ToString();
                PrevPage.IsEnabled = _cview.HasPreviousPage;
                NextPage.IsEnabled = _cview.HasNextPage;
                UsersGrid.DataContext = _cview.ToList();
                pageNumber = 1;
                Number.Text = pageNumber.ToString();
            }
            else if (Order_By.Text == "Last Name")
            {
                if (Direction.Text == "ASCENDING")
                {
                    _users = _users.OrderBy(s => s.LastName).ToList();
                }
                else
                {
                    _users = _users.OrderByDescending(s => s.LastName).ToList();
                }

                _cview = await GetPagedListAsync(1);
                MaxNumber.Text = numOfPages.ToString();
                PrevPage.IsEnabled = _cview.HasPreviousPage;
                NextPage.IsEnabled = _cview.HasNextPage;
                UsersGrid.DataContext = _cview.ToList();
                pageNumber = 1;
                Number.Text = pageNumber.ToString();
            }
            else if (Order_By.Text == "First+Last Name")
            {
                if (Direction.Text == "ASCENDING")
                {
                    _users = _users.OrderBy(s => s.FirstName).ThenBy(s => s.LastName).ToList();
                }
                else
                {
                    _users = _users.OrderByDescending(s => s.FirstName).ThenByDescending(s => s.LastName).ToList();
                }

                _cview = await GetPagedListAsync(1);
                MaxNumber.Text = numOfPages.ToString();
                PrevPage.IsEnabled = _cview.HasPreviousPage;
                NextPage.IsEnabled = _cview.HasNextPage;
                UsersGrid.DataContext = _cview.ToList();
                pageNumber = 1;
                Number.Text = pageNumber.ToString();
            }
            else if (Order_By.Text == "EMail")
            {
                if (Direction.Text == "ASCENDING")
                {
                    _users = _users.OrderBy(s => s.Email).ToList();
                }
                else
                {
                    _users = _users.OrderByDescending(s => s.Email).ToList();
                }

                _cview = await GetPagedListAsync(1);
                MaxNumber.Text = numOfPages.ToString();
                PrevPage.IsEnabled = _cview.HasPreviousPage;
                NextPage.IsEnabled = _cview.HasNextPage;
                UsersGrid.DataContext = _cview.ToList();
                pageNumber = 1;
                Number.Text = pageNumber.ToString();
            }
        }

        private async void EditUser(object sender, RoutedEventArgs e)
        {
            var clicked = UsersGrid.SelectedItem as User;

            UserScreen win = new UserScreen(clicked);

            if(win.ShowDialog() == false)
            {
                if(win.closing_mode == 1)
                {

                    UpdateUser(win.user_to_modify);
                    if (_users != null)
                    {
                        _users.Remove(clicked);
                        _users.Add(win.user_to_modify);
                    }

                    _cview = await GetPagedListAsync(pageNumber);
                    MaxNumber.Text = numOfPages.ToString();
                    PrevPage.IsEnabled = _cview.HasPreviousPage;
                    NextPage.IsEnabled = _cview.HasNextPage;
                    UsersGrid.DataContext = _cview.ToList();
                    Number.Text = pageNumber.ToString();

                }
                if (win.closing_mode == 2)
                {
                    DeleteUser(sender, e);
                }
              
            }
        }
    }
}
