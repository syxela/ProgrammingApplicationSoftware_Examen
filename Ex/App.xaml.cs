using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks; 
using System.Windows;
using Ex.Databank;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ex
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //databank file creëren bij opstart
        protected override void OnStartup(StartupEventArgs e)
        {
            DatabaseFacade facade = new DatabaseFacade(new ScoreboardDbContext()); ;
            //verzekerd het creëren van de databank
            facade.EnsureCreated();
        }
    }

}
