namespace GamesWebSocket.Models.Hubs.ChatHub;

public class MessageViewModel
{
    public string Content { get; set; } = string.Empty;
    public string FromUser { get; set; } = string.Empty;
    public string FromNickname { get; set; } = string.Empty;
    public string FromAvatar { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
