using WordleClone.Services;

namespace WordleClone;

public partial class MainPage : ContentPage
{
    public MainPageViewModel ViewModel { get; set; }    

    private readonly WordListService wordListService;
    static int NUMBER_OF_GUESSES = 6;
    static int WORD_LETTER_COUNT = 5;
    int guessesRemaining = NUMBER_OF_GUESSES;
    string rightGuessString;
    private Dictionary<char, Color> keyboardStates = new Dictionary<char, Color>(); //to track the color of the keyboard letters, idk why microsoft calls their hashmap dictionaries

    Color WordleGreen = Color.FromRgb(66, 113, 62);
    Color WordleYellow = Color.FromRgb(145, 127, 47);
    Color WordleGray = Color.FromRgb(44, 48, 50);
    Color WordleDarkGray = Color.FromRgb(14, 15, 16);
    Color WordleLightGray = Color.FromRgb(121, 112, 99);

    public MainPage()
    {
        InitializeComponent();
        ViewModel = new MainPageViewModel();
        BindingContext = ViewModel;
        ViewModel.CurrentGuess.CollectionChanged += (sender, args) =>
        {
            UpdateBoardFromCurrentGuess();
        };
        wordListService = new WordListService();
        LoadWords();        
        InitBoard();
        InitKeyBoard();   
        //make the keyboard letters white because i dont wanna manually put textcolor=white in all of em
        foreach (var child in KeyboardGrid.Children)
        {
            if (child is Button button)
            {
                button.TextColor = Colors.White;
            }
        }
        ThemeSwitch.IsToggled = true;
        UserInput.Focus(); //focus the mouse on the userinput entry when init
    }
    private void OnKeyboardButtonClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button != null)
        {
            char letter = button.Text[0];
            ViewModel.UserInput += letter;
            UpdateBoardFromCurrentGuess();
        }
        UserInput.Focus();
    }
    private void ButtonDel_Clicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (button != null)
        {
            ViewModel.UserInput = ViewModel.UserInput.Remove(ViewModel.UserInput.Length - 1);   
            UpdateBoardFromCurrentGuess();
        }
        UserInput.Focus();
    }

    private void UpdateBoardFromCurrentGuess()
    {
        for (int i = 0; i < WORD_LETTER_COUNT; i++)
        {
            int index = (NUMBER_OF_GUESSES - guessesRemaining) * WORD_LETTER_COUNT + i;
            var box = (Frame)GameBoard.Children[index];
            var label = box.Content as Label;

            if (label != null)
            {
                label.Text = i < ViewModel.CurrentGuess.Count ? ViewModel.CurrentGuess[i].ToString().ToUpper() : string.Empty;
            }
        }
    }

    private void InitKeyBoard()
    {
        foreach (char c in "ABCDEFGHIJKLMNOPQRSTUVWXYZ")
        {
            keyboardStates[c] = WordleDarkGray;
            Button button = FindKeyboardButton(c);
            if (button != null)
            {
                button.BackgroundColor = WordleDarkGray;
            }
        }
        var enterButton = this.FindByName<Button>("ButtonEnter");
        if (enterButton != null)
            enterButton.BackgroundColor = WordleDarkGray;
        var deleteButton = this.FindByName<Button>("ButtonDel");      
        if (deleteButton != null)
            deleteButton.BackgroundColor = WordleDarkGray;
    }

    private void InitBoard()
    {
        //clear the board in case we call initboard again
        GameBoard.Children.Clear();
        GameBoard.RowDefinitions.Clear();
        GameBoard.ColumnDefinitions.Clear();

        GameBoard.RowSpacing = 10;
        GameBoard.ColumnSpacing = 10;

        for (int i = 0; i < NUMBER_OF_GUESSES; i++)
        {
            GameBoard.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (int j = 0; j < WORD_LETTER_COUNT; j++)
            {
                if (i == 0) //edge case for NUMBER_OF_GUESSES = 0
                {
                    GameBoard.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                }
                //create box for each letter
                int boxSize = 75;
                var box = new Frame
                {
                    BorderColor = WordleLightGray,                    
                    HeightRequest = boxSize,
                    WidthRequest = boxSize,
                    BackgroundColor = WordleDarkGray,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Content = new Label
                    {
                        Text = string.Empty,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        FontSize = boxSize/3,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center
                    }
                };         
                //add box to grid, nice and simple
                GameBoard.SetRow(box, i);
                GameBoard.SetColumn(box, j);
                GameBoard.Children.Add(box);
            }
        }
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

    
    private void ResetGame()
    {
        guessesRemaining = NUMBER_OF_GUESSES;
        ViewModel.CurrentGuess.Clear();
        LoadWords();
        InitBoard();
        foreach (char key in keyboardStates.Keys)
        {
            keyboardStates[key] = WordleDarkGray;
        }
        UpdateKeyboardColors();
        UserInput.Text = string.Empty;
    }
    private async void CheckGuessAsync()
    {
        string guess = new(ViewModel.CurrentGuess.ToArray());

        //check win
        if (guess == rightGuessString)
        {
            for (int i = 0; i < WORD_LETTER_COUNT; i++)
            {
                int index = (NUMBER_OF_GUESSES - guessesRemaining) * WORD_LETTER_COUNT + i; //check row index for the grid.children
                var box = (Frame)GameBoard.Children[index];
                box.BackgroundColor = WordleGreen;
            }

            await DisplayAlert("Congratulations!", "You guessed the word!", "OK");
            ResetGame();
            return;
        }

        //give feedback for each letter
        //track count of correct letter in the correct word, basically hashmap
        //i cant believe i'm actually using hashmaps for something wtf
        Dictionary<char, int> letterCounts = new();
        foreach (char letter in rightGuessString)
        {
            if (letterCounts.ContainsKey(letter))
                letterCounts[letter]++;
            else
                letterCounts[letter] = 1;
        }

        //first pass, mark greens and reduce count for that letter in the hashmap
        for (int i = 0; i < WORD_LETTER_COUNT; i++)
        {
            int index = (NUMBER_OF_GUESSES - guessesRemaining) * WORD_LETTER_COUNT + i;
            var box = (Frame)GameBoard.Children[index];
            char letter = ViewModel.CurrentGuess[i];

            if (letter == rightGuessString[i])
            {
                box.BackgroundColor = WordleGreen;
                keyboardStates[letter] = WordleGreen;
                letterCounts[letter]--;
            }
        }
        //second pass, mark yellows and grays
        for (int i = 0; i < WORD_LETTER_COUNT; i++)
        {
            int index = (NUMBER_OF_GUESSES - guessesRemaining) * WORD_LETTER_COUNT + i; 
            var box = (Frame)GameBoard.Children[index];
            char letter = ViewModel.CurrentGuess[i];

            if (box.BackgroundColor == WordleGreen)
                continue; // skip alr marked greens

            if (rightGuessString.Contains(letter) && letterCounts[letter] > 0)
            {
                box.BackgroundColor = WordleYellow;//right ltr wrong post
                keyboardStates[letter] = WordleYellow;
                letterCounts[letter]--;
            }
            else
            {
                box.BackgroundColor = WordleGray;//wrong ltr
                //check if that key already has a color yellow or green
                if (keyboardStates[char.ToUpper(letter)] == WordleYellow || keyboardStates[char.ToUpper(letter)] == WordleGreen) continue;
                keyboardStates[letter] = WordleGray;
            }
        }

        UpdateKeyboardColors();
        guessesRemaining--;

        if (guessesRemaining == 0)
        {
            await DisplayAlert("Game Over", $"The correct word was: {rightGuessString}", "OK");
            ResetGame();
            return;
        }

        // Reset for the next guess
        ViewModel.CurrentGuess.Clear(); // Clear the current guess
        UserInput.Text = string.Empty; // Clear the Entry
    }

    private void UpdateKeyboardColors()
    {
        foreach (var key in keyboardStates)
        {
            Button button = FindKeyboardButton(key.Key);
            if (button != null)
            {
                button.BackgroundColor = key.Value;
            }
        }
    }    
    private Button FindKeyboardButton(char letter)
    {
        string buttonName = $"Button{char.ToUpper(letter)}";
        return this.FindByName<Button>(buttonName);
    }

    private async void UserInput_Completed(object sender, EventArgs e)
    {        
        if (ViewModel.CurrentGuess.Count != WORD_LETTER_COUNT)
        {
            await DisplayAlert("Invalid Input", "Please enter a 5-letter word.", "OK");
        }
        else if (!await DictionaryApiService.DoesWordExistAsync(ViewModel.UserInput))
        {
            await DisplayAlert("Invalid Input", "The word does not exist in the dictionary.", "OK");
        }
        else
        {            
            ViewModel.UserInput = ViewModel.UserInput.ToLower();
            CheckGuessAsync();
        }
        UserInput.Focus();
    }
    private void UserInput_TextChanged(object sender, TextChangedEventArgs e)
    {
        //copied code from stackoverflow
        if (!string.IsNullOrEmpty(e.NewTextValue))
        {
            var textFiltered = new string(e.NewTextValue.Where(char.IsLetter).ToArray());
            ((Entry)sender).Text = textFiltered;
        }
    }

    private void ThemeSwitch_Toggled(object sender, ToggledEventArgs e)
    {
        if (e.Value) this.BackgroundColor = WordleDarkGray; //if switch is on, dark mode
        else this.BackgroundColor = Colors.Gray;
        UserInput.Focus();
    }
}
