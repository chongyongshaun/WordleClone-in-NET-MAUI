using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WordleClone;

public class MainPageViewModel : INotifyPropertyChanged
{
    public ObservableCollection<char> CurrentGuess { get; set; } = new ObservableCollection<char>();

    public event PropertyChangedEventHandler PropertyChanged;

    private string userInput;
    public string UserInput
    {
        get => userInput;
        set
        {
            if (userInput != value)
            {
                userInput = value;
                OnPropertyChanged();

                UpdateCurrentGuess(value);
            }
        }
    }

    private void UpdateCurrentGuess(string input)
    {
        CurrentGuess.Clear();
        foreach (var c in input.ToCharArray())
        {
            CurrentGuess.Add(c);
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
