using WordleClone.Services;

namespace WordleClone;

public partial class MainPage : ContentPage
{
    private readonly WordListService wordListService;

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
                        
            // debug: display the first 5 words in lbl            
            foreach (var word in words)
            {
                lbl.Text += word + " ";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading words: {ex.Message}");
        }
    }

}
