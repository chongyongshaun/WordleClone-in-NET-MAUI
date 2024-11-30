namespace WordleClone.Services;

internal class DictionaryApiService
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
}
