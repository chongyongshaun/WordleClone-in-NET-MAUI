using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace WordleClone.Services;

public class WordListService
{
    private const string WordListUrl = "https://raw.githubusercontent.com/DonH-ITS/jsonfiles/main/words.txt";
    private readonly string localFilePath;

    public WordListService()
    {        
        string localFolder = FileSystem.AppDataDirectory; //get local file path for getting the words
        localFilePath = Path.Combine(localFolder, "words.txt");
    }

    public async Task EnsureWordsFileExistsAsync()
    {
        if (!File.Exists(localFilePath))
        {
            await DownloadAndSaveWordsAsync();
        }
    }

    private async Task DownloadAndSaveWordsAsync()
    {
        try
        {
            HttpClient client = new();
            string wordList = await client.GetStringAsync(WordListUrl);

            File.WriteAllText(localFilePath, wordList); //save the file locally
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading word list: {ex.Message}");
            throw;
        }
    }

    public string[] GetWords()
    {
        if (File.Exists(localFilePath))
        {
            string wordList = File.ReadAllText(localFilePath);
            return wordList.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }
        else
        {
            throw new FileNotFoundException("Word list file not found.");
        }
    }
}
