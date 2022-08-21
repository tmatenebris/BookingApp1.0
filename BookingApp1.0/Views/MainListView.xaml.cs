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

namespace BookingApp1._0.Views
{
    /// <summary>
    /// Logika interakcji dla klasy MainListView.xaml
    /// </summary>
    public partial class MainListView : UserControl
    {

        private  PagingCollectionView<HallDTO> _cview;
        private PagingCollectionView<HallDTO> _cviewFilterable;
        private PagingCollectionView<HallDTO> _nameFiltered;
        private string prevDataContext;
        private string currentDataContext;
        private string filterString;
        public MainListView()
        {
            InitializeComponent();
            string response = TCPClient.ServerRequestWithResponse("GetHalls");
            Debug.WriteLine(response);
            List<HallDTO> halls = new List<HallDTO>();

            halls = XMLSerialize.Deserialize<List<HallDTO>>(response);

            this._cview = new PagingCollectionView<HallDTO>(halls, 7);
            this.DataContext = this._cview;
            currentDataContext = "cview";
            //Task.Run(() => FillGridBase()).ConfigureAwait(false);
        }

     
        private void FillGridBase()
        {
            string response;
            while (true)
            {
                string request = "GetHallsNotIn: ";
                foreach (HallDTO hall in this._cview)
                {
                    if (request == "GetHallsNotIn: ") request += hall.HallId.ToString();
                    else request += ", " + hall.HallId.ToString();
                }

               
                List<HallDTO> tmphalls = new List<HallDTO>();

                response = TCPClient.ServerRequestWithResponse(request);
                if (response == "empty") break;
                tmphalls = XMLSerialize.Deserialize<List<HallDTO>>(response);

                this._cview.AddRange(tmphalls);
            }
        }
        
        private void FillGridFilter(Filters filtersToApply)
        {
            string request, response;
            while (true)
            {
                filtersToApply.idsstring = "";
                foreach (HallDTO hall in this._cviewFilterable)
                {
                    if (filtersToApply.idsstring == "") filtersToApply.idsstring += hall.HallId.ToString();
                    else filtersToApply.idsstring += ", " + hall.HallId.ToString();
                }

                List<HallDTO> tmphalls = new List<HallDTO>();
                request = XMLSerialize.Serialize<Filters>(filtersToApply);
                response = TCPClient.ServerRequestWithResponse(request);
                if (response == "empty") break;
                tmphalls = XMLSerialize.Deserialize<List<HallDTO>>(response);

                this._cviewFilterable.AddRange(tmphalls);
            }
        }
        private void OnPreviousClicked(object sender, RoutedEventArgs e)
        {
            if (currentDataContext == "cview") this._cview.MoveToPreviousPage();
            else if (currentDataContext == "filter") this._cviewFilterable.MoveToPreviousPage();
            else if (currentDataContext == "tmp_paging") this._nameFiltered.MoveToPreviousPage();
        }

        private void OnNextClicked(object sender, RoutedEventArgs e)
        {
            if (currentDataContext == "cview") this._cview.MoveToNextPage();
            else if (currentDataContext == "filter") this._cviewFilterable.MoveToNextPage();
            else if (currentDataContext == "tmp_paging") this._nameFiltered.MoveToNextPage();
        }

        private void OpenFiltersWindow(object sender, RoutedEventArgs e)
        {
            FilterScreen win = new FilterScreen();
            if(win.ShowDialog() == false)
            {
                Filters filtersToApply = new Filters();
                filtersToApply.location = win.Filters.location;
                filtersToApply.from_price = win.Filters.from_price;
                filtersToApply.to_price = win.Filters.to_price;
                filtersToApply.from_capacity = win.Filters.from_capacity;
                filtersToApply.to_capacity = win.Filters.to_capacity;
                filtersToApply.from_date = win.Filters.from_date;
                filtersToApply.to_date = win.Filters.to_date;

                string request = XMLSerialize.Serialize<Filters>(filtersToApply);
                string response = TCPClient.ServerRequestWithResponse(request);

            
               List<HallDTO> halls = new List<HallDTO>();
               if(response != "empty")halls = XMLSerialize.Deserialize<List<HallDTO>>(response);
               this._cviewFilterable = new PagingCollectionView<HallDTO>(halls, 7);
               this.DataContext = this._cviewFilterable;
                currentDataContext = "filter";
                Task.Run(() => FillGridFilter(filtersToApply)).ConfigureAwait(false);

            }
        }

        private void ShowOffer(object sender, MouseButtonEventArgs e)
        {
           var clicked = hallDataGrid.SelectedItem as HallDTO;

            //Txt.Text = clicked.HallId.ToString();
            
            OfferScreen win = new OfferScreen(clicked.HallId);
            win.ShowDialog();
        }

        private void BackToBasic(object sender, RoutedEventArgs e)
        {
            this.DataContext = _cview;
            currentDataContext = "cview";
        }

        private bool Comp(object obj)
        {
            HallDTO s = obj as HallDTO;
            return s.Name.Contains(filterString);
            
           
        }
        private void FilterByName(object sender, TextChangedEventArgs e)
        {
            TextBox senderTextBox = sender as TextBox;
            filterString = senderTextBox.Text;
            List<HallDTO> list = null;
            if (currentDataContext == "cview")
            {
                list = new List<HallDTO>(this._cview.GetInner());
                prevDataContext = "cview";

            }
            else if(currentDataContext == "filter")
            {
                list = new List<HallDTO>(this._cviewFilterable.GetInner());
                prevDataContext = "filter";
            }
            else
            {
                if(prevDataContext == "cview")
                {
                    list = new List<HallDTO>(this._cview.GetInner());
                }
                else
                {
                    list = new List<HallDTO>(this._cviewFilterable.GetInner());
                }
            }
            


            List<HallDTO> filtred = list.Where(x => x.Name.Contains(filterString) == true).ToList();
            _nameFiltered = new PagingCollectionView<HallDTO>(filtred, 7);
            
            if(filterString == String.Empty)
            {
                
                if(currentDataContext == "cview")
                {
                    this.DataContext = _cview;
                    currentDataContext = "cview";
                }
                else if (currentDataContext == "filter")
                {
                    this.DataContext = _cviewFilterable;
                    currentDataContext = "filter";
                }
                else
                {
                    if (prevDataContext == "cview")
                    {
                        this.DataContext = _cview;
                        currentDataContext = "cview";
                    }
                    else
                    {
                        this.DataContext = _cviewFilterable;
                        currentDataContext = "filter";
                    }
                }
            }
            else
            {
                this.DataContext = _nameFiltered;
                currentDataContext = "tmp_paging";
            }
        }

    }
}
