using System.Collections.ObjectModel;

namespace WordleClone;

public partial class HistoryPage : ContentPage
{
    public ObservableCollection<PlayHistory> PlayHistoryList { get; set; }
    Color WordleDarkGray = Color.FromRgb(14, 15, 16);
    public HistoryPage(ObservableCollection<PlayHistory> playHistoryList)
	{
		InitializeComponent();
        PlayHistoryList = playHistoryList;
        BindingContext = this;
        bool isDarkMode = Preferences.Get("IsDarkMode", true);
        this.BackgroundColor = isDarkMode ? WordleDarkGray : Colors.Gray;
    }
}