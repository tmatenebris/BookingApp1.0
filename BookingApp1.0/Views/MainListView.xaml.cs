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
using Database.Models;
using Database;
using TCPConnection;
using System.Diagnostics;
using System.Linq;
using PagedList;

namespace BookingApp1._0.Views
{
    /// <summary>
    /// Logika interakcji dla klasy MainListView.xaml
    /// </summary>
    public partial class MainListView : UserControl
    {

        private IPagedList<HallDTO> _cview;
        private List<HallDTO> _filtered;
        private List<HallDTO> _halls;
        private List<HallDTO> _sorted;
        private string current_view = "normal";
        private int pageNumber = 1;
        private int numOfPages = 0;
      

        
        public MainListView()
        {
            InitializeComponent();

            Loaded += MainListView_Load;
            if(App.appuser.Role == "admin")
            {
                AdminPanel.Visibility = Visibility.Visible;
            }
            SearchBy.Items.Add("Name");
            SearchBy.Items.Add("Location");
            Order_By.Items.Add("Name");
            Order_By.Items.Add("Location");
            Order_By.Items.Add("Price");
            Order_By.Items.Add("Capacity");
            Direction.Items.Add("ASCENDING");
            Direction.Items.Add("DESCENDIG");

        }

        private async Task<IPagedList<HallDTO>> GetPagedListAsync(int pageNumber = 1, int pageSize = 7)
        {
            return await Task.Factory.StartNew(() =>
            {
                if (current_view == "normal")
                {
                    numOfPages = (_halls.Count() + pageSize - 1) / pageSize;
                    return _halls.ToPagedList(pageNumber, pageSize);
                }
                else if (current_view == "filtered")
                {
                    numOfPages = (_filtered.Count() + pageSize - 1) / pageSize;
                    return _filtered.ToPagedList(pageNumber, pageSize);
                }
                else return _sorted.ToPagedList(pageNumber, pageSize);
                
                
            });

        }



        private async Task<List<HallDTO>> GetHallsAsync()
        {
            return await Task.Factory.StartNew(() =>
            {
                string response = TCPClient.ServerRequestWithResponse("[(GET_HALLS)]");
                List<HallDTO> halls = new List<HallDTO>();
                halls = XMLSerialize.Deserialize<List<HallDTO>>(response);
                return halls;
            });
        }

        private async Task<List<HallDTO>> GetFiltredHallsAsync(Filters filtersToApply)
        {
            return await Task.Factory.StartNew(() =>
            {
                string request = XMLSerialize.Serialize<Filters>(filtersToApply);
                string response = TCPClient.ServerRequestWithResponse("[(GET_FILTERED_DATA)]"+request);
                List<HallDTO> halls = new List<HallDTO>();
                halls = XMLSerialize.Deserialize<List<HallDTO>>(response);
                return halls;
            });
        }

        private async void MainListView_Load(object sender, EventArgs e)
        {
            ProgressBar.IsIndeterminate = true;
            _halls = await GetHallsAsync();
            _cview = await GetPagedListAsync();
            _sorted = _halls;
            MaxNumber.Text = numOfPages.ToString();
           


            PrevPage.IsEnabled = _cview.HasPreviousPage;
            NextPage.IsEnabled = _cview.HasNextPage;
            hallDataGrid.DataContext = _cview.ToList();
            ProgressBar.IsIndeterminate = false;

        }
        private async void OnPreviousClicked(object sender, RoutedEventArgs e)
        {
            if (_cview.HasPreviousPage)
            {
                _cview = await GetPagedListAsync(--pageNumber);
                PrevPage.IsEnabled = _cview.HasPreviousPage;
                NextPage.IsEnabled = _cview.HasNextPage;
                hallDataGrid.DataContext = _cview.ToList();
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
                hallDataGrid.DataContext = _cview.ToList();
                Number.Text = pageNumber.ToString();
            }
        }

        private async void OpenFiltersWindow(object sender, RoutedEventArgs e)
        {
            string result = TCPConnection.TCPClient.ServerRequestWithResponse("[(GET_FILTERS_INITIAL)]");

            Filters filter_info = XMLSerialize.Deserialize<Filters>(result);

            FilterScreen win = new FilterScreen(filter_info);
            if (win.ShowDialog() == false)
            {
                if (win.closing_mode == 1)
                {
                    Filters filtersToApply = new Filters();
                    filtersToApply.userid = -1;
                    filtersToApply.location = win.Filters.location;
                    filtersToApply.from_price = win.Filters.from_price;
                    filtersToApply.to_price = win.Filters.to_price;
                    filtersToApply.from_capacity = win.Filters.from_capacity;
                    filtersToApply.to_capacity = win.Filters.to_capacity;
                    filtersToApply.from_date = win.Filters.from_date;
                    filtersToApply.to_date = win.Filters.to_date;

                    
                    ProgressBar.IsIndeterminate = true;
                    _filtered = await GetFiltredHallsAsync(filtersToApply);
                    _sorted = _filtered;
                    current_view = "filtered";
                    _cview = await GetPagedListAsync(1);
                    MaxNumber.Text = numOfPages.ToString();
                    PrevPage.IsEnabled = _cview.HasPreviousPage;
                    NextPage.IsEnabled = _cview.HasNextPage;
                    ProgressBar.IsIndeterminate = false;
                    hallDataGrid.DataContext = _cview.ToList();

                    
                }
            }
        }

        private async void ShowOffer(object sender, MouseButtonEventArgs e)
        {
            var clicked = hallDataGrid.SelectedItem as HallDTO;
            OfferScreen win = new OfferScreen(clicked);
 
            if(win.ShowDialog() == false)
            {
                if(win.closing_mode == 1)
                {
                    if (_halls != null)
                    {
                        try
                        {
                            _halls.Remove(_halls.Where(s => s.HallId == clicked.HallId).First());
                            _halls.Add(win.offerHall);
                        }
                        catch { }
                    }

                    if (_filtered != null)
                    {
                        try
                        {
                            _filtered.Remove(_filtered.Where(s => s.HallId == clicked.HallId).First());
                            _filtered.Add(win.offerHall);
                        }
                        catch { }
                    }

                    if (_sorted != null)
                    {
                        try
                        {
                            _sorted.Remove(_sorted.Where(s => s.HallId == clicked.HallId).First());
                            _sorted.Add(win.offerHall);
                        }
                        catch { }
                    }
                    _cview = await GetPagedListAsync(pageNumber);
                    MaxNumber.Text = numOfPages.ToString();
                    PrevPage.IsEnabled = _cview.HasPreviousPage;
                    NextPage.IsEnabled = _cview.HasNextPage;
                    hallDataGrid.DataContext = _cview.ToList();
                    Number.Text = pageNumber.ToString();
                }
            }
        }


        private async Task<IPagedList<HallDTO>> GetPagedListAsyncNameFiltered(string name, int pageNumber = 1, int pageSize = 7)
        {
            return await Task.Factory.StartNew(() =>
            {
                if (current_view == "normal")
                {
                    numOfPages = (_halls.Where(s => s.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList().Count() + pageSize - 1) / pageSize;
                    return _halls.Where(s => s.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToPagedList(pageNumber, pageSize);
                }
                else if (current_view == "filtered")
                {
                    numOfPages = (_filtered.Where(s => s.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList().Count() + pageSize - 1) / pageSize;
                    return _filtered.Where(s => s.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToPagedList(pageNumber, pageSize);
                }
                else
                {
                    numOfPages = (_sorted.Where(s => s.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList().Count() + pageSize - 1) / pageSize;
                    return _sorted.Where(s => s.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToPagedList(pageNumber, pageSize);
                }
            });


        }



        private async Task<IPagedList<HallDTO>> GetPagedListAsyncLocationFiltered(string location, int pageNumber = 1, int pageSize = 7)
        {
            return await Task.Factory.StartNew(() =>
            {
                if (current_view == "normal")
                {
                    numOfPages = (_halls.Where(s => s.Location.Contains(location, StringComparison.OrdinalIgnoreCase)).ToList().Count() + pageSize - 1) / pageSize;
                    return _halls.Where(s => s.Location.Contains(location, StringComparison.OrdinalIgnoreCase)).ToPagedList(pageNumber, pageSize);
                }
                else if (current_view == "filtered")
                {
                    numOfPages = (_filtered.Where(s => s.Location.Contains(location, StringComparison.OrdinalIgnoreCase)).ToList().Count() + pageSize - 1) / pageSize;
                    return _filtered.Where(s => s.Location.Contains(location, StringComparison.OrdinalIgnoreCase)).ToPagedList(pageNumber, pageSize);
                }
                else
                {
                    numOfPages = (_sorted.Where(s => s.Location.Contains(location, StringComparison.OrdinalIgnoreCase)).ToList().Count() + pageSize - 1) / pageSize;
                    return _sorted.Where(s => s.Location.Contains(location, StringComparison.OrdinalIgnoreCase)).ToPagedList(pageNumber, pageSize);
                }
            });

        }


        private async void FilterBy(object sender, TextChangedEventArgs e)
        {
            if (txtFilter.Text != String.Empty)
            {
                OrderButton.IsEnabled = false;
                SearchBy.IsEnabled = false;
                if (SearchBy.Text == "Name")
                {
                    _cview = await GetPagedListAsyncNameFiltered(txtFilter.Text, 1);
                    MaxNumber.Text = numOfPages.ToString();
                    PrevPage.IsEnabled = _cview.HasPreviousPage;
                    NextPage.IsEnabled = _cview.HasNextPage;
                    hallDataGrid.DataContext = _cview.ToList();
                    pageNumber = 1;
                    Number.Text = pageNumber.ToString();
                }
                else if (SearchBy.Text == "Location")
                {
                    _cview = await GetPagedListAsyncLocationFiltered(txtFilter.Text, 1);
                    MaxNumber.Text = numOfPages.ToString();
                    PrevPage.IsEnabled = _cview.HasPreviousPage;
                    NextPage.IsEnabled = _cview.HasNextPage;
                    hallDataGrid.DataContext = _cview.ToList();
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
                hallDataGrid.DataContext = _cview.ToList();
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
            hallDataGrid.DataContext = _cview.ToList();
            _sorted = _halls;
            pageNumber = 1;
            Number.Text = pageNumber.ToString();
            Order_By.SelectedIndex = -1;
            Direction.SelectedIndex = -1;
        }

        //sortowanie
        private async void OrderBy(object sender, RoutedEventArgs e)
        {
            current_view = "sorted";
            if (current_view == "normal")
            {
              
                if(Order_By.Text== "Name")
                {
                    
                    if(Direction.Text == "ASCENDING")
                    {
                        _sorted = _halls.OrderBy(s => s.Name).ToList();
                    }
                    else
                    {
                        _sorted = _halls.OrderByDescending(s => s.Name).ToList();
                    }
                }
                else if(Order_By.Text== "Location")
                {
                    if (Direction.Text == "ASCENDING")
                    {
                        _sorted = _halls.OrderBy(s => s.Location).ToList();
                    }
                    else
                    {
                        _sorted = _halls.OrderByDescending(s => s.Location).ToList();
                    }
                }
                else if(Order_By.Text == "Price")
                {
                    if (Direction.Text == "ASCENDING")
                    {
                        _sorted = _halls.OrderBy(s => s.Price).ToList();
                    }
                    else
                    {
                        _sorted = _halls.OrderByDescending(s => s.Price).ToList();
                    }
                }
                else if(Order_By.Text == "Capacity")
                {
                    if (Direction.Text == "ASCENDING")
                    {
                        _sorted = _halls.OrderBy(s => s.Capacity).ToList();
                    }
                    else
                    {
                        _sorted = _halls.OrderByDescending(s => s.Capacity).ToList();
                    }
                }

                _cview = await GetPagedListAsync(1);
                MaxNumber.Text = numOfPages.ToString();
                PrevPage.IsEnabled = _cview.HasPreviousPage;
                NextPage.IsEnabled = _cview.HasNextPage;
                hallDataGrid.DataContext = _cview.ToList();
                pageNumber = 1;
                Number.Text = pageNumber.ToString();
            }
            else if (current_view == "filtered")
            {
          
                if (Order_By.Text == "Name")
                {
                    if (Direction.Text == "ASCENDING")
                    {
                        _sorted = _filtered.OrderBy(s => s.Name).ToList();
                    }
                    else
                    {
                        _sorted = _filtered.OrderByDescending(s => s.Name).ToList();
                    }
                }
                else if (Order_By.Text == "Location")
                {
                    if (Direction.Text == "ASCENDING")
                    {
                        _sorted = _filtered.OrderBy(s => s.Location).ToList();
                    }
                    else
                    {
                        _sorted = _filtered.OrderByDescending(s => s.Location).ToList();
                    }
                }
                else if (Order_By.Text == "Price")
                {
                    if (Direction.Text == "ASCENDING")
                    {
                        _sorted = _filtered.OrderBy(s => s.Price).ToList();
                    }
                    else
                    {
                        _sorted = _filtered.OrderByDescending(s => s.Price).ToList();
                    }
                }
                else if (Order_By.Text == "Capacity")
                {
                    if (Direction.Text == "ASCENDING")
                    {
                        _sorted = _filtered.OrderBy(s => s.Capacity).ToList();
                    }
                    else
                    {
                        _sorted = _filtered.OrderByDescending(s => s.Capacity).ToList();
                    }
                }

                _cview = await GetPagedListAsync(1);
                MaxNumber.Text = numOfPages.ToString();
                PrevPage.IsEnabled = _cview.HasPreviousPage;
                NextPage.IsEnabled = _cview.HasNextPage;
                hallDataGrid.DataContext = _cview.ToList();
                pageNumber = 1;
                Number.Text = pageNumber.ToString();
            }
            else if (current_view == "sorted")
            {
          
                if (Order_By.Text == "Name")
                {
                    if (Direction.Text == "ASCENDING")
                    {
                        _sorted = _sorted.OrderBy(s => s.Name).ToList();
                    }
                    else
                    {
                        _sorted = _sorted.OrderByDescending(s => s.Name).ToList();
                    }
                }
                else if (Order_By.Text == "Location")
                {
                    if (Direction.Text == "ASCENDING")
                    {
                        _sorted = _sorted.OrderBy(s => s.Location).ToList();
                    }
                    else
                    {
                        _sorted = _sorted.OrderByDescending(s => s.Location).ToList();
                    }
                }
                else if (Order_By.Text == "Price")
                {
                    if (Direction.Text == "ASCENDING")
                    {
                        _sorted = _sorted.OrderBy(s => s.Price).ToList();
                    }
                    else
                    {
                        _sorted = _sorted.OrderByDescending(s => s.Price).ToList();
                    }
                }
                else if (Order_By.Text == "Capacity")
                {
                    if (Direction.Text == "ASCENDING")
                    {
                        _sorted = _sorted.OrderBy(s => s.Capacity).ToList();
                    }
                    else
                    {
                        _sorted = _sorted.OrderByDescending(s => s.Capacity).ToList();
                    }
                }

                _cview = await GetPagedListAsync(1);
                MaxNumber.Text = numOfPages.ToString();
                PrevPage.IsEnabled = _cview.HasPreviousPage;
                NextPage.IsEnabled = _cview.HasNextPage;
                hallDataGrid.DataContext = _cview.ToList();
                pageNumber = 1;
                Number.Text = pageNumber.ToString();
            }

        }


        private async Task<string> DeleteRequest(int hall_id)
        {
            return await Task.Factory.StartNew(() =>
            {
                string response = TCPClient.ServerRequestWithResponse("[(DELETE_HALL)]:("+hall_id.ToString()+")");
                return response;
            });
        }


        private async void DeleteHall(object sender, RoutedEventArgs e)
        {
            var clicked = hallDataGrid.SelectedItem as HallDTO;


            string response = await DeleteRequest(clicked.HallId);

            if (response == "error") MessageBox.Show("Unable to delete Hall");
            else
            {
                if (_halls != null)
                {
                    try
                    {
                        _halls.Remove(_halls.Where(s => s.HallId == clicked.HallId).First());
                    }
                    catch { }
                }
                
                if (_filtered != null)
                {
                    try
                    {
                        _filtered.Remove(_filtered.Where(s => s.HallId == clicked.HallId).First());
                    }
                    catch { }
                }
                
                if (_sorted != null)
                {
                    try
                    {
                        _sorted.Remove(_sorted.Where(s => s.HallId == clicked.HallId).First());
                    }
                    catch { }
                }
                
                _cview = await GetPagedListAsync(pageNumber);
                MaxNumber.Text = numOfPages.ToString();
                PrevPage.IsEnabled = _cview.HasPreviousPage;
                NextPage.IsEnabled = _cview.HasNextPage;
                hallDataGrid.DataContext = _cview.ToList();
                Number.Text = pageNumber.ToString();
                txtFilter.Text = "";
            }
            
        }
        
        
    }
}
