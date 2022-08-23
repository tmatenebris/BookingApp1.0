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
        private static IPagedList<User> _cview;
        private static List<User> _users;
        private static List<User> _sorted;
        private string current_view = "normal";
        private int pageNumber = 1;
        private int numOfPages = 0;



        public UsersView()
        {
            InitializeComponent();

            Loaded += UsersView_Load;


        }

        private async Task<IPagedList<User>> GetPagedListAsync(int pageNumber = 1, int pageSize = 7)
        {
            return await Task.Factory.StartNew(() =>
            {
                if (current_view == "normal")
                {
                    numOfPages = (_users.Count() + pageSize - 1) / pageSize;
                    return _users.ToPagedList(pageNumber, pageSize);
                }
                else return _sorted.ToPagedList(pageNumber, pageSize);


            });

        }



        private async Task<List<User>> GetUsersAsync()
        {
            return await Task.Factory.StartNew(() =>
            {
                string response = TCPClient.ServerRequestWithResponse("[(GetUsers)]");
                List<User> users = new List<User>();
                users = XMLSerialize.Deserialize<List<User>>(response);
                return users;
            });
        }

        private async void UsersView_Load(object sender, EventArgs e)
        {
            ProgressBar.IsIndeterminate = true;
            _users = await GetUsersAsync();
            _cview = await GetPagedListAsync();
            _sorted = _users;
            MaxNumber.Text = numOfPages.ToString();
            ProgressBar.IsIndeterminate = false;
            SearchBy.Items.Add("First Name");
            SearchBy.Items.Add("Last Name");
            SearchBy.Items.Add("EMail");
            Order_By.Items.Add("Fist Name");
            Order_By.Items.Add("Last Name");
            Order_By.Items.Add("EMail");
            Direction.Items.Add("ASCENDING");
            Direction.Items.Add("DESCENDIG");

            PrevPage.IsEnabled = _cview.HasPreviousPage;
            NextPage.IsEnabled = _cview.HasNextPage;
            UsersGrid.DataContext = _cview.ToList();


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





        private async Task<IPagedList<User>> GetPagedListAsyncFirstNameFiltered(string name, int pageNumber = 1, int pageSize = 7)
        {
            return await Task.Factory.StartNew(() =>
            {
                if (current_view == "normal")
                {
                    numOfPages = (_users.Where(s => s.FirstName.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList().Count() + pageSize - 1) / pageSize;
                    return _users.Where(s => s.FirstName.Contains(name, StringComparison.OrdinalIgnoreCase)).ToPagedList(pageNumber, pageSize);
                }
                else
                {
                    numOfPages = (_sorted.Where(s => s.FirstName.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList().Count() + pageSize - 1) / pageSize;
                    return _sorted.Where(s => s.FirstName.Contains(name, StringComparison.OrdinalIgnoreCase)).ToPagedList(pageNumber, pageSize);
                }
            });


        }




        private async Task<IPagedList<User>>GetPagedListAsyncLastNameFiltered(string last_name, int pageNumber = 1, int pageSize = 7)
        {
            return await Task.Factory.StartNew(() =>
            {
                if (current_view == "normal")
                {
                    numOfPages = (_users.Where(s => s.LastName.Contains(last_name, StringComparison.OrdinalIgnoreCase)).ToList().Count() + pageSize - 1) / pageSize;
                    return _users.Where(s => s.LastName.Contains(last_name, StringComparison.OrdinalIgnoreCase)).ToPagedList(pageNumber, pageSize);
                }
                else
                {
                    numOfPages = (_sorted.Where(s => s.LastName.Contains(last_name, StringComparison.OrdinalIgnoreCase)).ToList().Count() + pageSize - 1) / pageSize;
                    return _sorted.Where(s => s.LastName.Contains(last_name, StringComparison.OrdinalIgnoreCase)).ToPagedList(pageNumber, pageSize);
                }
            });

        }

        private async Task<IPagedList<User>> GetPagedListAsyncEMailFiltered(string email, int pageNumber = 1, int pageSize = 7)
        {
            return await Task.Factory.StartNew(() =>
            {
                if (current_view == "normal")
                {
                    numOfPages = (_users.Where(s => s.Email.Contains(email, StringComparison.OrdinalIgnoreCase)).ToList().Count() + pageSize - 1) / pageSize;
                    return _users.Where(s => s.Email.Contains(email, StringComparison.OrdinalIgnoreCase)).ToPagedList(pageNumber, pageSize);
                }
                else
                {
                    numOfPages = (_sorted.Where(s => s.Email.Contains(email, StringComparison.OrdinalIgnoreCase)).ToList().Count() + pageSize - 1) / pageSize;
                    return _sorted.Where(s => s.Email.Contains(email, StringComparison.OrdinalIgnoreCase)).ToPagedList(pageNumber, pageSize);
                }
            });

        }


        private async void FilterBy(object sender, TextChangedEventArgs e)
        {
            if (txtFilter.Text != String.Empty)
            {
                OrderButton.IsEnabled = false;
                SearchBy.IsEnabled = false;
                if (SearchBy.Text == "First Name")
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

        private async void BackToBasic(object sender, RoutedEventArgs e)
        {
            current_view = "normal";
            _cview = await GetPagedListAsync(1);
            MaxNumber.Text = numOfPages.ToString();
            PrevPage.IsEnabled = _cview.HasPreviousPage;
            NextPage.IsEnabled = _cview.HasNextPage;
            UsersGrid.DataContext = _cview.ToList();
            _sorted = _cview.ToList();
            pageNumber = 1;
            Number.Text = pageNumber.ToString();
        }

        private async void OrderBy(object sender, RoutedEventArgs e)
        {
            current_view = "sorted";
            if (current_view == "normal")
            {

                if (Order_By.Text == "Last Name")
                {

                    if (Direction.Text == "ASCENDING")
                    {
                        _sorted = _users.OrderBy(s => s.LastName).ToList();
                    }
                    else
                    {
                        _sorted = _users.OrderByDescending(s => s.LastName).ToList();
                    }
                }
                else if (Order_By.Text == "First Name")
                {
                    if (Direction.Text == "ASCENDING")
                    {
                        _sorted = _users.OrderBy(s => s.FirstName).ToList();
                    }
                    else
                    {
                        _sorted = _users.OrderByDescending(s => s.FirstName).ToList();
                    }
                }
                else if (Order_By.Text == "EMail")
                {
                    if (Direction.Text == "ASCENDING")
                    {
                        _sorted = _users.OrderBy(s => s.Email).ToList();
                    }
                    else
                    {
                        _sorted = _users.OrderByDescending(s => s.Email).ToList();
                    }
                }


                _cview = await GetPagedListAsync(1);
                MaxNumber.Text = numOfPages.ToString();
                PrevPage.IsEnabled = _cview.HasPreviousPage;
                NextPage.IsEnabled = _cview.HasNextPage;
                UsersGrid.DataContext = _cview.ToList();
                pageNumber = 1;
                Number.Text = pageNumber.ToString();
            }
            else
            {

                if (Order_By.Text == "Last Name")
                {
                    if (Direction.Text == "ASCENDING")
                    {
                        _sorted = _sorted.OrderBy(s => s.LastName).ToList();
                    }
                    else
                    {
                        _sorted = _sorted.OrderByDescending(s => s.LastName).ToList();
                    }
                }
                else if (Order_By.Text == "First Name")
                {
                    if (Direction.Text == "ASCENDING")
                    {
                        _sorted = _sorted.OrderBy(s => s.FirstName).ToList();
                    }
                    else
                    {
                        _sorted = _sorted.OrderByDescending(s => s.FirstName).ToList();
                    }
                }
                else if (Order_By.Text == "EMail")
                {
                    if (Direction.Text == "ASCENDING")
                    {
                        _sorted = _sorted.OrderBy(s => s.Email).ToList();
                    }
                    else
                    {
                        _sorted = _sorted.OrderByDescending(s => s.Email).ToList();
                    }
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


        private async Task<string> ServerRequest(int hall_id)
        {
            return await Task.Factory.StartNew(() =>
            {
                string response = TCPClient.ServerRequestWithResponse("[(DELETE_USER)]:(" + hall_id.ToString() + ")");
                return response;
            });
        }


        private async void DeleteHall(object sender, RoutedEventArgs e)
        {
            var clicked = UsersGrid.SelectedItem as User;


            string response = await ServerRequest(clicked.UserId);

            if (response == "error") MessageBox.Show("Unable to delete Hall");
            else
            {
                if (_users != null) _users.Remove(clicked);
                if (_sorted != null) _sorted.Remove(clicked);
                _cview = await GetPagedListAsync(pageNumber);
                MaxNumber.Text = numOfPages.ToString();
                PrevPage.IsEnabled = _cview.HasPreviousPage;
                NextPage.IsEnabled = _cview.HasNextPage;
                UsersGrid.DataContext = _cview.ToList();
                Number.Text = pageNumber.ToString();
            }

        }
    }
}
