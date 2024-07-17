namespace GamesWebSocket.Models.Hubs.ChatHub;

public class RoomViewModel
{
    public string RoomName { get; set; } = string.Empty;
    public string RoomCode { get; set; } = string.Empty;
    public int Limit { get; set; } = 0;
}
