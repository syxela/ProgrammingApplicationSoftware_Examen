﻿using System.ComponentModel;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Ex.Databank;
using Ex.Klassen;
using Ex.Windows;
using static System.Net.WebRequestMethods;


namespace Ex
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    // Declaratie van de MainWindow-klasse die de interactie en logica voor het hoofdvenster van de WPF-toepassing bevat

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // Declaratie van variabelen, eigenschappen en API-URL's
        private string correctCharacterName; // Variabele om de naam van het correcte karakter bij te houden
        string apiUrl = "https://api.gameofthronesquotes.xyz/v1/characters";
        string apiUrl2 = "https://thronesapi.com/api/v2/Characters/";
        public string selectedChar;
        private DispatcherTimer timer;
        
        private string _username;
        private int _userId;

        // Constructor van MainWindow
        public MainWindow(string username, int userId, ScoreboardDbContext context)
        {
            _username = username;
            InitializeComponent();
            InitializeTimer();
            DataContext = this;
            _userId = userId;
        }

        // Event handler voor de startknop
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            GetQuote();
        }


        // Methode om de timer te initialiseren
        string GuessCharacter;
        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1); 
            timer.Tick += TimerTick;
        }


        // Methode om een citaat van een personage op te halen
        public async Task GetQuote()
        {
            try
            {
                List<Character> characters = await GetCharactersWithQuotes();

                if (characters != null && characters.Any())
                {
                    Random random = new Random();
                    Character randomCharacter = characters[random.Next(characters.Count)];
                    string randomQuote = randomCharacter.quotes[random.Next(randomCharacter.quotes.Count)];
                    txtQuote.Text = "Quote: " + randomQuote;

                    correctCharacterName = randomCharacter.name.Trim(); // Trim whitespace

                    switch (correctCharacterName)
                    {
                        case "Eddard \\\"Ned\\\" Stark":
                            correctCharacterName = "Ned Stark";
                            break;
                        case "Jaime Lannister":
                            correctCharacterName = "Jamie Lannister";
                            break;
                        case "Bran Stark":
                            correctCharacterName = "Brandon Stark";
                            break;
                        case "Lord Varys":
                            correctCharacterName = "Varys";
                            break;
                        case "Ramsay Bolton":
                            correctCharacterName = "Ramsey Bolton";
                            break;
                            // No default case needed since there's nothing to do for other names
                    }

                    await ShowOptions(correctCharacterName, characters);
                }
                else
                {
                    txtQuote.Text = "Er zijn geen personages gevonden.";
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately, such as logging or displaying an error message.
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        // Methode om personages met citaten op te halen van een externe API
        public async Task<List<Character>> GetCharactersWithQuotes()
        {
            List<Character> characters = new List<Character>();
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    // Deserialiseer de JSON-response naar een lijst van characters
                    characters = JsonSerializer.Deserialize<List<Character>>(responseBody);
                }
                catch (HttpRequestException ex)
                {
                    // Fout bij het maken van de HTTP-request
                    MessageBox.Show($"Fout bij het maken van de HTTP-request: {ex.Message}");
                }
                catch (JsonException ex)
                {
                    // Fout bij het deserialiseren van de JSON
                    MessageBox.Show($"Fout bij het deserialiseren van de JSON: {ex.Message}");
                }
            }
            return characters;
        }


        // Event handler voor wanneer een optie in de lijst wordt geselecteerd
        private void optionsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(optionsListView.SelectedItems.Count > 0)
            {
                selectedChar = optionsListView.SelectedItems[0] as string;
                ShowCharacter(selectedChar);
            }
        }

        // Event handler voor wanneer de gebruiker een gok invoert
        private void btnEnterGuess_Click(object sender, RoutedEventArgs e)
        {
            GuessCharacter = selectedChar;

            if (GuessCharacter == correctCharacterName)
            {
                
                MessageBox.Show("Right Answer!");
                timer.Start();
                GetScore(true); 
            }
            else
            {
                
                MessageBox.Show("You guessed wrong.");
                timer.Start();
                GetScore(false); 
                
            }
        }

        // Methode om het beeld van een personage weer te geven
        public async void ShowCharacter(string _character)
        {

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(apiUrl2);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            List<CharacterImage> characterImages = JsonSerializer.Deserialize<List<CharacterImage>>(responseBody);

            // Filter the characterImages list based on the correct character name
            List<CharacterImage> imagesToShow = characterImages.Where
                (characterImage => characterImage.fullName == _character).ToList();

            if (imagesToShow.Any())
            {
                CharacterImage firstImage = imagesToShow.First();
                string imagePath = firstImage.imageUrl; 

                ImageSource imageSource = new BitmapImage(new Uri(imagePath));

                imgChar.Source = imageSource;

            }
            else
            {
                string path = "pack://application:,,,/Ex;component/images/No-Image.png";
                BitmapImage bitmapImage = new BitmapImage(new Uri(path));
                imgChar.Source = bitmapImage;
            }
        }

        // Methode om opties weer te geven voor het raden van het juiste personage
        private async Task ShowOptions(string correctCharacterName, List<Character> characters)
        {
            List<string> Options = new List<string>();

            // Add the correct character
            Options.Add(correctCharacterName);

            // Add two more random characters
            Random random = new Random();
            while (Options.Count < 3)
            {
                Character randomCharacter = characters[random.Next(characters.Count)];
                if (!Options.Contains(randomCharacter.name))
                {
                    Options.Add(randomCharacter.name);
                }
            }

            // Shuffle the options
            Options = Options.OrderBy(x => random.Next()).ToList();

            
            optionsListView.ItemsSource = Options;
        }


        // Event handler voor wanneer de gebruiker een nieuw citaat wil
        private void btnNewQuote_Click(object sender, RoutedEventArgs e)
        {
            NewTry(); 

        }

        private int _score;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        // Eigenschap om de score van de quiz bij te houden
        public int QuizScore
        {
            get { return _score; }
            set
            {
                if (_score != value)
                {
                    _score = value;
                    OnPropertyChanged(nameof(QuizScore));
                }
            }
        }
        int Tries = 0;


        // Methode om de score bij te houden en te controleren op maximale pogingen
        public void GetScore(bool isJuist)
        {

            Tries++;
            if (isJuist)
            {
                QuizScore++;
            }
            if (Tries >= 5)
            {
                MessageBox.Show("You have reached the maximum number of tries. You got " + QuizScore + " out of 5.");
                UpdateUser(_userId, QuizScore);
                
                
            }
        }

        // Methode om een nieuwe poging te starten
        private void NewTry()
        {
            imgChar.Source = null;
            GetQuote();
        }

        // Event handler voor wanneer de timer afloopt
        private void TimerTick(object sender, EventArgs e)
        {
            timer.Stop(); 
            NewTry();
        }


        // Methode om de gebruikersscore in de database bij te werken
        private void UpdateUser(int userId, int newScore)
        {
            using (ScoreboardDbContext context = new ScoreboardDbContext())
            {
                //Haal user's id op
                var updatedUser = context.Users.FirstOrDefault(u => u.Id == userId);

                if (updatedUser != null)
                {
                    //user's score updaten
                    updatedUser.Score = newScore;

                    // Opslaan in Database
                    context.SaveChanges();
                }
            }
            EndGame();
        }


        // Methode om het einde van het spel aan te kondigen en de UI te resetten
        public void EndGame()
        {
            txtQuote.Text = string.Empty;
            optionsListView.ItemsSource = null;
            imgChar.Source = null;

            
            Tries = 0;
            QuizScore = 0;
        }


    }
}
