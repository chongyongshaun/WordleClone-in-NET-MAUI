using Microsoft.Maui.Controls.Compatibility;
using WordleClone.Services;

namespace WordleClone;

public partial class MainPage : ContentPage
{
    private readonly WordListService wordListService;
    static int NUMBER_OF_GUESSES = 6;
    int guessesRemaining = NUMBER_OF_GUESSES;
    char[] currentGuess = [];
    int nextLetter = 0;
    string rightGuessString;

    public MainPage()
    {
        InitializeComponent();
        wordListService = new WordListService();
        LoadWords();
    }
    private async void LoadWords()
    {
        try
        {
            await wordListService.EnsureWordsFileExistsAsync();
            string[] words = wordListService.GetWords();
            Random rnd = new();
            rightGuessString = words[rnd.Next(words.Length)];

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading words: {ex.Message}");
        }
    }

}
