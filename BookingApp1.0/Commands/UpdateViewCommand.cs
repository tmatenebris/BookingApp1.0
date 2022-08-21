using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BookingApp1._0.ViewModels;


namespace BookingApp1._0.Commands
{
    public class UpdateViewCommand : ICommand
    {
        private MainViewModel viewModel;

        public UpdateViewCommand(MainViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter.ToString() == "MyBookings")
            {
                viewModel.SelectedViewModel = new MyBookingsViewModel();
            }
            else if (parameter.ToString() == "MainList")
            {
                viewModel.SelectedViewModel = new MainListViewModel();
            }
            else if (parameter.ToString() == "PrivateList")
            {
                viewModel.SelectedViewModel = new PrivateListViewModel();
            }
            else if(parameter.ToString() == "AddHall")
            {
                viewModel.SelectedViewModel = new AddHallViewModel();
            }
            else if(parameter.ToString() == "Users")
            {
                viewModel.SelectedViewModel = new UsersViewModel();
            }
        }
    }
}
