namespace GamesWebSocket.Models.Hubs.ChatHub;

public class UserViewModel
{
    public string Username { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public string Device { get; set; } = string.Empty;
    public RoomViewModel? CurrentRoom { get; set; }
}
