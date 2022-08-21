using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TCPConnection;
namespace BookingApp1._0
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
 
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            TCPClient.StartClient();
            
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            TCPClient.StopClient();
   
        }
    }
}
