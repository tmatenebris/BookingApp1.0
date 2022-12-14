using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BookingApp1._0.Commands;
using BookingApp1._0.Views;
using Database.Models;

namespace BookingApp1._0.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private BaseViewModel _selectedViewModel = new SplashScreenViewModel();

        

        public BaseViewModel SelectedViewModel
        {
            get { return _selectedViewModel; }
            set
            {
                _selectedViewModel = value;
                OnPropertyChanged(nameof(SelectedViewModel));
            }
        }

        public ICommand UpdateViewCommand { get; set; }


        public MainViewModel()
        {
            UpdateViewCommand = new UpdateViewCommand(this);
            
        }
    }
}
