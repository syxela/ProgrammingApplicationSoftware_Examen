using Ex.Databank;
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

namespace Ex.Windows
{
    /// <summary>
    /// Interaction logic for UserName.xaml
    /// </summary>
    public partial class UserName : Window
    {
        public UserName()
        {
            InitializeComponent();
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            var nameUser = txtUserName.Text;

            //Zet users naam in the database
            using (ScoreboardDbContext context = new ScoreboardDbContext())
            {
                var newUser = new User { Name = nameUser, Score = 0};
                context.Users.Add(newUser);
                context.SaveChanges();
            }
            StartQuiz();
            //sluit dit scherm
            this.Close();
        }

        public void StartQuiz()
        {
            //Opent scherm van de quiz
            MainWindow mainWindow = new MainWindow();
             
            mainWindow.Show();
        }
    }
}
