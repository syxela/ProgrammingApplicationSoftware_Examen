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
using static System.Net.WebRequestMethods;

namespace Ex
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private string _buttonClicked;
        private string correctCharacterName; // Variabele om de naam van het correcte karakter bij te houden
        string apiUrl = "https://api.gameofthronesquotes.xyz/v1/characters";
        string apiUrl2 = "https://thronesapi.com/api/v2/Characters/";
        public string selectedChar;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GetQuote();
        }
        string GuessCharacter; 

        public async void GetQuote()
        {
            // Haal de lijst met personages en hun quotes op
            List<Character> characters = await GetCharactersWithQuotes();
            // Controleer of er personages zijn opgehaald
            if (characters != null && characters.Any())
            {
                // Kies een willekeurig personage uit de lijst
                Random random = new Random();
                Character randomCharacter = characters[random.Next(characters.Count)];
                // Kies een willekeurige quote uit de quotes van het gekozen personage
                string randomQuote = randomCharacter.quotes[random.Next(randomCharacter.quotes.Count)];
                // Toon de gekozen quote in de txtQuote
                txtQuote.Text = "Quote: " + randomQuote;

                correctCharacterName = randomCharacter.name;
                
                //Indien correctCharacterName = Ed Stark aanpassen zodat het in verdere methodes nog gebruikt kan worden
                if (correctCharacterName == "Eddard \\\"Ned\\\" Stark")
                {
                    correctCharacterName = "Ned Stark";
                }

                if (correctCharacterName == "Jaime Lannister")
                {
                    correctCharacterName = "Jamie Lannister";
                }

                if (correctCharacterName == "Bran Stark")
                {
                    correctCharacterName = "Brandon Stark";
                }
                if (correctCharacterName == "Lord Varys")
                {
                    correctCharacterName = "Varys";
                }
            }
            else
            {
                // Geen personages gevonden
                txtQuote.Text = "Er zijn geen personages gevonden.";
            }

            await ShowOptions(correctCharacterName, characters);
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
                Score(); 
            }
            else
            {
                MessageBox.Show("You guessed wrong."); 
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
                string imagePath = firstImage.imageUrl; // Assuming CharacterImage has a property called ImageUrl

                ImageSource imageSource = new BitmapImage(new Uri(imagePath));

                imgChar.Source = imageSource;

            }
            else
            {
                MessageBox.Show($"No image found for character: {_character}");
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
            imgChar.Source = null;
            GetQuote(); 

        }

        public int Score()
        {
            int i = 0;
            while(i>11)
            {
                i++;
            }
            return i;
        }

    }
    }
