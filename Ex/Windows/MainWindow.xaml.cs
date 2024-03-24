using System.ComponentModel;
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
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        //private string _buttonClicked;
        private string correctCharacterName; // Variabele om de naam van het correcte karakter bij te houden
        string apiUrl = "https://api.gameofthronesquotes.xyz/v1/characters";
        string apiUrl2 = "https://thronesapi.com/api/v2/Characters/";
        public string selectedChar;
        private DispatcherTimer timer;
        
        private string _username;
        private int _userId;
        public MainWindow(string username, int userId, ScoreboardDbContext context)
        {
            _username = username;
            InitializeComponent();
            InitializeTimer();
            DataContext = this;
            _userId = userId;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GetQuote();
        }
        string GuessCharacter;
        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1); 
            timer.Tick += TimerTick;
        }

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

        private void optionsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(optionsListView.SelectedItems.Count > 0)
            {
                selectedChar = optionsListView.SelectedItems[0] as string;
                ShowCharacter(selectedChar);
            }
        }
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
                //string path = "Ex/images/No-Image.png"; 
                //BitmapImage bitmapImage = new BitmapImage(new Uri(path));
                imgChar.Source = null; 
                
            }
        }

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

        public int Score
        {
            get { return _score; }
            set
            {
                if (_score != value)
                {
                    _score = value;
                    OnPropertyChanged(nameof(Score));
                }
            }
        }
        int Tries = 0;
        public void GetScore(bool isJuist)
        {

            Tries++;
            if (isJuist)
            {
                Score++;
            }
            if (Tries >= 5)
            {
                UpdateUser(_userId, Score);
                MessageBox.Show("You have reached the maximum number of tries. You got " + Score + " out of 5.");
            }
               
        }
        private void NewTry()
        {
            imgChar.Source = null;
            GetQuote();
        }
        private void TimerTick(object sender, EventArgs e)
        {
            timer.Stop(); 
            NewTry();
        }

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
        }
    }
}
