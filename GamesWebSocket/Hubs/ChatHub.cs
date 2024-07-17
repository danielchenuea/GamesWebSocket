using GamesWebSocket.Models.Hubs.ChatHub;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace GamesWebSocket.Hubs;

public class ChatHub: Hub
{
    public readonly static List<RoomViewModel> _RoomList = new List<RoomViewModel>();
    public readonly static List<UserViewModel> _Connections = new List<UserViewModel>();

    private readonly static ConcurrentDictionary<string, string> _UsersMap = new ConcurrentDictionary<string, string>();
    private readonly static ConcurrentDictionary<string, UserViewModel> _ConnectionsMap = new ConcurrentDictionary<string, UserViewModel>();
    public async Task SendPrivate(string senderName, string receiverName, string message)
    {
        try
        {
            _UsersMap.TryGetValue(senderName, out string? senderId);
            _UsersMap.TryGetValue(receiverName, out string? receiverId);
            _ConnectionsMap.TryGetValue(senderName, out UserViewModel? senderCredentials);
            if (!string.IsNullOrEmpty(message.Trim()) && senderCredentials!.CurrentRoom != null)
            {
                MessageViewModel _message = new()
                {
                    FromAvatar = senderCredentials.Avatar,
                    Content = message,
                    FromNickname = senderCredentials.Nickname,
                    FromUser = senderCredentials.Username,
                    RoomName = senderCredentials.CurrentRoom.RoomName,
                    Timestamp = DateTime.Now,
                };

                await Clients.Caller.SendAsync("NewMessage", _message);
                await Clients.Client(receiverId!).SendAsync("NewMessage", _message);
            }
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("OnError", $"Failed on sending the message to this person. {ex.Message}");
        }
    }

    public async Task SendToGroup(string message)
    {
        try
        {
            if (_ConnectionsMap.TryGetValue(Context.ConnectionId, out UserViewModel? userCredentials))
            {
                if (!string.IsNullOrEmpty(message.Trim()) && userCredentials.CurrentRoom != null)
                {
                    MessageViewModel _message = new()
                    {
                        FromAvatar = userCredentials.Avatar,
                        Content = message,
                        FromNickname = userCredentials.Nickname,
                        FromUser = userCredentials.Username,
                        RoomName = userCredentials.CurrentRoom.RoomName,
                        Timestamp = DateTime.Now,
                    };

                    await Clients.Caller.SendAsync("NewMessage", _message);
                    await Clients.OthersInGroup(userCredentials.CurrentRoom.RoomCode).SendAsync("NewMessage", _message);
                }
            }
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("OnError", $"Failed on sending the message to the Group. {ex.Message}");
        }
    }

    public async Task CreateRoom()
    {
        string code;
        while (true)
        {
            code = GenerateRandomRoomCode();
            if (!_RoomList.Any(el => el.RoomName == code)) break;
        }

        RoomViewModel newRoom = new()
        {
            RoomCode = code,
            RoomName = $"Room {_RoomList.Count + 1}",
            Limit = 4,
        };

        _RoomList.Add(newRoom);
        await Clients.Caller.SendAsync("OnCreateRoom", code);

        await JoinRoom(code);
    }

    public async Task JoinRoom(string roomCode)
    {
        try
        {
            if (_ConnectionsMap.TryGetValue(Context.ConnectionId, out UserViewModel? userCredentials))
            {
                await LeaveRoom();
                await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
                userCredentials.CurrentRoom = _RoomList.First(el => el.RoomCode == roomCode);

                await Clients.Group(roomCode).SendAsync("OnJoinRoom", userCredentials.Username);
                await Clients.Group(roomCode).SendAsync("OnNewUsers", 
                    _ConnectionsMap
                        .Where(el => {
                            if (el.Value.CurrentRoom == null) return false;
                            return el.Value.CurrentRoom.RoomCode == roomCode;
                        })
                        .Select(el => el.Value.Username)
                );
            }
        }
        catch(Exception ex)
        {
            await Clients.Caller.SendAsync("OnError", $"Failed entering the room. {ex.Message}");
        }
    }
    public async Task LeaveRoom()
    {
        try
        {
            if (_ConnectionsMap.TryGetValue(Context.ConnectionId, out UserViewModel? userCredentials)) {
                if (userCredentials.CurrentRoom != null)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, userCredentials.CurrentRoom.RoomCode);

                    await Clients.OthersInGroup(userCredentials.CurrentRoom.RoomCode).SendAsync("OnLeaveRoom", userCredentials.Username);
                    
                    if(_ConnectionsMap.Any(el => el.Value.CurrentRoom!.RoomCode == userCredentials.CurrentRoom.RoomCode))
                        _RoomList.RemoveAll(el => el.RoomCode == userCredentials.CurrentRoom.RoomCode);
                    
                    userCredentials.CurrentRoom = null;
                }
            }
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("OnError", $"Failed leaving the room. {ex.Message}");
        }
    }

    public override Task OnConnectedAsync()
    {
        try
        {
            var username = Context.GetHttpContext()?.Request.Query["username"];
            if (string.IsNullOrEmpty(username)) throw new Exception("Nome de usuário não está válido.");

            _UsersMap.TryAdd(username, Context.ConnectionId);
            _ConnectionsMap.TryAdd(Context.ConnectionId, new UserViewModel()
            {
                Username = username,
                Nickname = username,
                Avatar = username,
                CurrentRoom = null,
                Device = "",
            });
        }
        catch (Exception ex)
        {
            Clients.Caller.SendAsync("OnError", $"Connection Error: {ex.Message}");
        }
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            _ = LeaveRoom();
            if (!_ConnectionsMap.TryRemove(Context.ConnectionId, out UserViewModel? userView)) throw new Exception();
            if (!_UsersMap.TryRemove(userView.Username, out _)) throw new Exception();
        }
        catch (Exception ex)
        {
            Clients.Caller.SendAsync("OnError", $"Connection Error: {ex.Message}");
        }
        return base.OnDisconnectedAsync(exception);
    }

    private string GenerateRandomRoomCode()
    {
        string possibleLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        Random rd = new();
        string code = "";
        for (int i = 0; i < 4; i++)
        {
            code += possibleLetters[rd.Next(possibleLetters.Length)];
        }
        return code;
    }
}
