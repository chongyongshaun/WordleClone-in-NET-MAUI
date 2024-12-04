namespace WordleClone;

public class PlayHistory
{
    public DateTime Timestamp { get; set; }
    public string? CorrectWord { get; set; }
    public int GuessesTaken { get; set; }
    public string? WinState {  get; set; }  
}
