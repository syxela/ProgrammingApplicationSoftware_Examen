﻿using Ex.Databank;
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
        private ScoreboardDbContext _context; // Field to store ScoreboardDbContext

        public UserName()
        {
            InitializeComponent();
            
        }


        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            var nameUser = txtUserName.Text;
            int userId;
            //Zet users naam in the database
            using (ScoreboardDbContext context = new ScoreboardDbContext())
            {
                
                var newUser = new User { Name = nameUser};
                userId = newUser.Id;
                context.Users.Add(newUser);
                context.SaveChanges();
            }
            StartQuiz(nameUser, userId);
            //sluit dit scherm
            this.Close();
        }

        public void StartQuiz(string username, int userId)
        {
            //Opent scherm van de quiz
            MainWindow mainWindow = new MainWindow(username, userId, _context);
             
            mainWindow.Show();
        }
    }
}
