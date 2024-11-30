using System.Text.Json;

namespace WordleClone.Services;

public class DictionaryApiService
{
    private static readonly HttpClient httpClient = new HttpClient();

    public static async Task<bool> DoesWordExistAsync(string word)
    {
        try
        {
            string apiUrl = $"https://api.dictionaryapi.dev/api/v2/entries/en/{word}";
            HttpResponseMessage response = await httpClient.GetAsync(apiUrl);
            if (!response.IsSuccessStatusCode) //check if response is successful, just in case
                return false;
            string responseBody = await response.Content.ReadAsStringAsync();
            //the api returns this json if word doesn't exist:
            //{"title":"No Definitions Found","message":"Sorry pal, we couldn't find definitions for the word you were looking for.","resolution":"You can try the search again at later time or head to the web instead."}
            if (responseBody.Contains("\"title\":\"No Definitions Found\""))
            {
                return false;  
            }
            return true; 
        }
        catch
        {
            return false;
        }
    }

    public static async Task<string> GetNLetterWordAsync(int numOfLetters)
    {
        string word = string.Empty;
        do
        {
            try
            {
                string apiUrl = $"https://random-word-api.herokuapp.com/word?length={numOfLetters}";
                HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var apiOut = JsonSerializer.Deserialize<string[]>(jsonResponse); //im parsing the api output which is an array of words in JSON
                    if (apiOut != null && apiOut.Length > 0)
                    {
                        word = apiOut[0]; //get the first word in the array
                    }
                    else
                    {
                        Console.WriteLine("No words found in the API response.");
                        continue; //no valid word found skip to next loop
                    }
                }
                else
                {
                    Console.WriteLine($"Failed to fetch word. Status code: {response.StatusCode}");
                    continue; //skip to next iteration if the get req fails
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching random word: {ex.Message}");
                continue; //same thing
            }
        }
        while (!await DoesWordExistAsync(word));
        return word;
    }
}
