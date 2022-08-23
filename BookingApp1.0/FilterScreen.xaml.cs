using Database;
using Database.Models;
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

namespace BookingApp1._0
{
    /// <summary>
    /// Logika interakcji dla klasy FilterScreen.xaml
    /// </summary>
    public partial class FilterScreen : Window
    {
        public Filters Filters = new Filters();
        public int closing_mode = 0;
        public FilterScreen(Filters filter_info)
        {
            InitializeComponent();


            Locations.ItemsSource = filter_info.locations;
            MaterialDesignThemes.Wpf.HintAssist.SetHint(LowCapacity, "Min: "+filter_info.from_capacity);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(HighCapacity, "Max: " + filter_info.to_capacity);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(LowPrice, "Min: " + filter_info.from_price);
            MaterialDesignThemes.Wpf.HintAssist.SetHint(HighPrice, "Max: " + filter_info.to_price);
        }

        private void CloseFilterScreen(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ApplyFilters(object sender, RoutedEventArgs e)
        {
            closing_mode = 1;
            Filters.location = Locations.Text;
            Filters.from_date = LowDate.Text;
            Filters.to_date = HighDate.Text;
            Filters.from_capacity = int.Parse(LowCapacity.Text);
            Filters.to_capacity = int.Parse(HighCapacity.Text);
            Filters.from_price = int.Parse(LowPrice.Text);
            Filters.to_price = int.Parse(HighPrice.Text);
            Close();
        }
    }
}
