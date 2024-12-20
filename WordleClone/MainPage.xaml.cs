﻿using System.Collections.ObjectModel;
using System.Text.Json;
using WordleClone.Services;

namespace WordleClone;

public partial class MainPage : ContentPage
{
    public MainPageViewModel ViewModel { get; set; }
    public ObservableCollection<PlayHistory> PlayHistoryList { get; set; } = [];

    private readonly WordListService wordListService;
    static int NUMBER_OF_GUESSES = 6;
    static int WORD_LETTER_COUNT;
    int guessesRemaining = NUMBER_OF_GUESSES;
    string rightGuessString;
    private Dictionary<char, Color> keyboardStates = new Dictionary<char, Color>(); //to track the color of the keyboard letters, idk why microsoft calls their hashmap dictionaries

    Color WordleGreen = Color.FromRgb(66, 113, 62);
    Color WordleYellow = Color.FromRgb(145, 127, 47);
    Color WordleGray = Color.FromRgb(44, 48, 50);
    Color WordleDarkGray = Color.FromRgb(14, 15, 16);
    Color WordleLightGray = Color.FromRgb(121, 112, 99);
    double gameBoardWidth = 0;
    bool isDarkMode;

    public MainPage()
    {
        InitializeComponent();
        ViewModel = new MainPageViewModel();
        BindingContext = ViewModel;
        //load preferences
        isDarkMode = Preferences.Get("IsDarkMode", true);
        WORD_LETTER_COUNT = Preferences.Get("letterCount", 5);
        ThemeSwitch.IsToggled = isDarkMode; 
        this.BackgroundColor = isDarkMode ? WordleDarkGray : Colors.Gray;

        ViewModel.CurrentGuess.CollectionChanged += (sender, args) => //stackoverflow code
        {
            UpdateBoardFromCurrentGuess();
        };
        wordListService = new WordListService();
        LoadWords();        
        InitBoard();
        InitKeyBoard();
        LoadHistoryList();
        //make the keyboard letters white because i dont wanna manually put textcolor=white in all of em
        foreach (var child in KeyboardGrid.Children)
        {
            if (child is Button button)
            {
                button.TextColor = Colors.White;
            }
        }
    }

    private void LoadHistoryList()
    {
        string historyJson = Preferences.Get("PlayHistoryList", null);
        if (!string.IsNullOrEmpty(historyJson))
        {
            var loadedList = JsonSerializer.Deserialize<List<PlayHistory>>(historyJson);
            if (loadedList != null)
            {
                foreach (var item in loadedList)
                {
                    PlayHistoryList.Add(item);
                }
            }
        }
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
        if (ViewModel.UserInput.Length < 1)
        {
            UserInput.Focus();
            return;
        }
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
                button.TextColor = Colors.White;
            }
        }
        var enterButton = this.FindByName<Button>("ButtonEnter");
        if (enterButton != null) {
            enterButton.BackgroundColor = WordleDarkGray;
            enterButton.TextColor = Colors.White;
        }          
        
        var deleteButton = this.FindByName<Button>("ButtonDel");
        if (deleteButton != null) {
            deleteButton.BackgroundColor = WordleDarkGray;
            deleteButton.TextColor = Colors.White;
        }
            
    }

    private void InitBoard()
    {
        //clear the board in case we call initboard again
        GameBoard.Children.Clear();
        GameBoard.RowDefinitions.Clear();
        GameBoard.ColumnDefinitions.Clear();

        double boxSize = 75;
        GameBoard.RowSpacing = boxSize/75*10;
        GameBoard.ColumnSpacing = boxSize / 75 * 10;

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
                        FontSize = boxSize / 3,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                        TextColor = Colors.White,
                    }
                };
                var tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += Frame_Tapped; // Attach the event handler
                box.GestureRecognizers.Add(tapGestureRecognizer);
                //add box to grid, nice and simple
                GameBoard.SetRow(box, i);
                GameBoard.SetColumn(box, j);
                GameBoard.Children.Add(box);
            }
        }
    }

    private void Frame_Tapped(object? sender, TappedEventArgs e)
    {
        UserInput.Focus();
    }

    private async void LoadWords()
    {
        if (WORD_LETTER_COUNT == 5)
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
        } else
        {
            rightGuessString = await DictionaryApiService.GetNLetterWordAsync(WORD_LETTER_COUNT);
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
    private async Task FrameAnimationAsync(Frame frame, Color newColor)
    {
        // scale it down, change color and make it big again, for a pop effect
        await frame.ScaleTo(0, 75, Easing.CubicIn); //easing graphs, same stuff as javascript
        frame.BackgroundColor = newColor;
        await frame.ScaleTo(1, 75, Easing.CubicOut);
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
                await FrameAnimationAsync(box, WordleGreen);
            }
            PlayHistoryList.Add(new PlayHistory
            {
                Timestamp = DateTime.Now,
                CorrectWord = rightGuessString,
                GuessesTaken = NUMBER_OF_GUESSES - guessesRemaining,
                WinState = "Win"
            });
            SaveHistoryList();
            await DisplayAlert("Congratulations!", "You guessed the word!", "OK");
            ResetGame();
            return;
        }

        //give feedback for each letter
        //track count of correct letter in the correct word, basically hashmap
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
                await FrameAnimationAsync(box, WordleGreen);
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
                await FrameAnimationAsync(box, WordleYellow);//right ltr wrong post
                keyboardStates[letter] = WordleYellow;
                letterCounts[letter]--;
            }
            else
            {
                await FrameAnimationAsync(box, WordleGray);//wrong ltr
                //check if that key already has a color yellow or green
                if (keyboardStates[char.ToUpper(letter)] == WordleYellow || keyboardStates[char.ToUpper(letter)] == WordleGreen) continue;
                keyboardStates[letter] = WordleGray;
            }
        }

        UpdateKeyboardColors();
        guessesRemaining--;

        if (guessesRemaining == 0) //lose logic
        {
            PlayHistoryList.Add(new PlayHistory
            {
                Timestamp = DateTime.Now,
                CorrectWord = rightGuessString,
                GuessesTaken = NUMBER_OF_GUESSES,
                WinState = "Loss"
            });
            SaveHistoryList();
            UserInput.Unfocus();
            await DisplayAlert("Game Over", $"The correct word was: {rightGuessString}", "OK");
            ResetGame();
            return;
        }

        //clear everything for the next game
        ViewModel.CurrentGuess.Clear();
        UserInput.Text = string.Empty; 
    }

    private void SaveHistoryList()
    {
        string historyJson = JsonSerializer.Serialize(PlayHistoryList);
        Preferences.Set("PlayHistoryList", historyJson);
    }

    private void UpdateKeyboardColors()
    {
        foreach (var key in keyboardStates)
        {
            Button button = FindKeyboardButton(key.Key);
            if (button != null)
            {
                if (button.BackgroundColor == WordleDarkGray) button.BackgroundColor = key.Value;
                if (button.BackgroundColor == WordleYellow && key.Value == WordleGreen) button.BackgroundColor = key.Value;
                if (button.BackgroundColor == WordleGray && key.Value == WordleGreen) button.BackgroundColor = key.Value;
                if (button.BackgroundColor == WordleGray && key.Value == WordleYellow) button.BackgroundColor = key.Value;
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
        UserInput.IsEnabled = false;
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
        await Task.Delay(1000); //keep the input disable for 2 seconds to avoid user spam clicking enter key bug
        UserInput.IsEnabled = true;
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
        isDarkMode = e.Value; //update isDarkMode bool and save to pref
        Preferences.Set("IsDarkMode", isDarkMode);
        if (isDarkMode)
        {
            this.BackgroundColor = WordleDarkGray;
        }
        else
        {
            this.BackgroundColor = Colors.Gray;
        }
        UserInput.Focus();
    }

    private void LetterCountPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (LetterCountPicker.SelectedItem != null)
        {
            WORD_LETTER_COUNT = (int)LetterCountPicker.SelectedItem;
            UserInput.MaxLength = WORD_LETTER_COUNT;
            switch (WORD_LETTER_COUNT) {
                case 4: case 5: 
                    NUMBER_OF_GUESSES = 6;
                    break;
                case 6: case 7: case 8: 
                    NUMBER_OF_GUESSES = 7;
                    break;
                default:
                    NUMBER_OF_GUESSES = WORD_LETTER_COUNT + 1;
                    break;
            }
        }
    }

    private void LetterCountBtn_Clicked(object sender, EventArgs e)
    {
        ResetGame();
        InitBoard();
        ResizeGameBoard(gameBoardWidth, WORD_LETTER_COUNT);
        InitKeyBoard();
        Preferences.Set("letterCount", WORD_LETTER_COUNT);
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        UserInput.Focus(); //refocus onto the entry whenever i click smwhere else
        SettingsSidebar.IsVisible = false; //make the sidebar go away when u click smwhr else
    }

    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        UserInput.Focus();//focus after the app fully loades
    }

    private void SettingsBtn_Clicked(object sender, EventArgs e)
    {
        SettingsSidebar.IsVisible = !SettingsSidebar.IsVisible;
    }
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        //outputLbl.Text = "width: " + this.Width + "height: " + this.Height;
        gameBoardWidth = width;
        ResizeGameBoard(width, WORD_LETTER_COUNT);
        ResizeKeyboard(width);
    }

    private void ResizeKeyboard(double width)
    {
        foreach (var item in KeyboardGrid.Children)
        {
            if (item.GetType() == typeof(Button))
            {
                Button btn = (Button)item;
                btn.Scale = 1 - btn.Width / width;
            }
        }
        KeyboardGrid.Scale = 1 - ((KeyboardGrid.Width / 4) / width);
    }

    private void ResizeGameBoard(double width, int letterCount)
    {
        foreach (var item in GameBoard.Children)
        {
            if (item.GetType() == typeof(Frame))
            {
                Frame frame = (Frame)item;
                frame.Scale = 1 - frame.Width/ width;
                if (letterCount >= 7) frame.Scale = 0.8;
            }
        }
        GameBoard.Scale = 1 - ((GameBoard.Width/3) / width);
    }

    private void HistoryPageBtn_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new HistoryPage(PlayHistoryList));
    }
}
