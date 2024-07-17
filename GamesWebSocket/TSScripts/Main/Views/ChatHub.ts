

interface newMessage {
    fromAvatar: string
    content: string
    fromNickname: string
    fromUser: string
    roomName: string
    timestamp: Date
}

var connection: signalR.HubConnection | null = null;

var loginButton = document.getElementById("SendLogin") as HTMLButtonElement | null;
var logoutButton = document.getElementById("SendLogout") as HTMLButtonElement | null;

var usernameInput = document.getElementById("Username") as HTMLInputElement | null;
var loginLogText = document.getElementById("LoginLog")

var CreateRoomButton = document.getElementById("CreateRoom") as HTMLButtonElement | null;

var RoomCodeInput = document.getElementById("RoomCode") as HTMLInputElement | null;
var EnterRoomButton = document.getElementById("EnterRoom") as HTMLButtonElement | null;
var LeaveRoomButton = document.getElementById("LeaveRoom") as HTMLButtonElement | null;

var MessageInput = document.getElementById("Message") as HTMLInputElement | null;
var SendMessageButton = document.getElementById("SendMessage") as HTMLButtonElement | null;
var ToUserInput = document.getElementById("ToUser") as HTMLInputElement | null;
var PrivateMessageInput = document.getElementById("PrivateMessage") as HTMLInputElement | null;
var SendPrivateMessageButton = document.getElementById("SendPrivateMessage") as HTMLButtonElement | null;

var userLoggedTitle = document.getElementById("userLoggedTitle");
var userLoggedList = document.getElementById("userLogged");
var chatLogTitle = document.getElementById("chatLogTitle");
var chatLogList = document.getElementById("chatLog");
var chatMessageTitle = document.getElementById("chatMessageTitle");
var chatMessageList = document.getElementById("chatMessage");


window.onload = function () {

    EventListeners();
    ExitRoom();
    Logout();
}

function EventListeners() {
    loginButton?.addEventListener("click", () => {
        connection = new signalR.HubConnectionBuilder().withUrl(`/chatHub?username=${usernameInput?.value}`).build();
        connection.start().then(() => {
            if (loginLogText) loginLogText.innerText = `Conectado como [${usernameInput?.value}]`;
            Login();
            SocketListeners();

        }).catch(() => {

        });
    });
    
    logoutButton?.addEventListener("click", () => {
        connection?.stop().then(() => {
            if (loginLogText) loginLogText.innerText = `Desconectado.`;
            Logout();

        }).catch(() => {

        });
    });
    
    CreateRoomButton?.addEventListener("click", () => {
        connection?.invoke("CreateRoom").catch(function (err) {
            return console.error(err.toString());
        })
    });

    EnterRoomButton?.addEventListener("click", () => {
        if (RoomCodeInput == null) return;

        connection?.invoke("JoinRoom", RoomCodeInput.value).catch(function (err) {
            return console.error(err.toString());
        })
    });
    
    LeaveRoomButton?.addEventListener("click", () => {
        connection?.invoke("LeaveRoom").catch(function (err) {
            return console.error(err.toString());
        })
    });

    SendMessageButton?.addEventListener("click", () => {
        const message = MessageInput?.value;
        connection?.invoke("SendToGroup", message).then(() => {
            MessageInput!.value = "";
        }).catch(function (err) {
            return console.error(err.toString());
        })
    });

    SendPrivateMessageButton?.addEventListener("click", () => {
        const message = PrivateMessageInput?.value;
        connection?.invoke("SendPrivate", message).then(() => {
            PrivateMessageInput!.value = "";
        }).catch(function (err) {
            return console.error(err.toString());
        })
    });


};
function SocketListeners() {
    connection?.on("NewMessage", (message: newMessage) => {
        var li = document.createElement("li");
        li.textContent = `${message.fromUser}: ${message.content}`;
        if (chatMessageList) chatMessageList.appendChild(li);
    });
    connection?.on("OnNewUsers", (userList: string[]) => {
        if (userLoggedList == null) return;
        while (userLoggedList.firstChild) {
            userLoggedList.removeChild(userLoggedList.firstChild);
        }
        userList.forEach(user => {
            var li = document.createElement("li");
            li.textContent = `${user}`;
            if (userLoggedList) userLoggedList.appendChild(li);
        })
    });

    connection?.on("OnCreateRoom", (roomCode: string) => {
        var li = document.createElement("li");
        li.textContent = `${usernameInput?.value} criou a sala ${roomCode}`;
        if (chatLogList) chatLogList.appendChild(li);
        if (RoomCodeInput) RoomCodeInput.value = roomCode;
        EnterRoom();
    });

    connection?.on("OnJoinRoom", (username: string) => {
        var li = document.createElement("li");
        li.textContent = `${username} entrou na sala`;
        if (chatLogList) chatLogList.appendChild(li);
    });
    connection?.on("OnLeaveRoom", (username: string) => {
        var li = document.createElement("li");
        li.textContent = `${username} saiu da sala`;
        if (chatLogList) chatLogList.appendChild(li);
    });
};

function Login() {
    if (usernameInput) usernameInput.disabled = true;
    if (loginButton) loginButton.disabled = true;
    if (logoutButton) logoutButton.disabled = false;

    if (CreateRoomButton) CreateRoomButton.disabled = false;
    if (EnterRoomButton) EnterRoomButton.disabled = false;
    if (LeaveRoomButton) LeaveRoomButton.disabled = false;
    if (RoomCodeInput) RoomCodeInput.disabled = false;

    if (MessageInput) MessageInput.disabled = false;
    if (SendMessageButton) SendMessageButton.disabled = false;
    if (ToUserInput) ToUserInput.disabled = false;
    if (PrivateMessageInput) PrivateMessageInput.disabled = false;
    if (SendPrivateMessageButton) SendPrivateMessageButton.disabled = false;

}
function Logout() {
    if (usernameInput) usernameInput.disabled = false;
    if (loginButton) loginButton.disabled = false;
    if (logoutButton) logoutButton.disabled = true;

    if (CreateRoomButton) CreateRoomButton.disabled = true;
    if (EnterRoomButton) EnterRoomButton.disabled = true;
    if (LeaveRoomButton) LeaveRoomButton.disabled = true;
    if (RoomCodeInput) RoomCodeInput.disabled = true;

    if (MessageInput) MessageInput.disabled = true;
    if (SendMessageButton) SendMessageButton.disabled = true;
    if (ToUserInput) ToUserInput.disabled = true;
    if (PrivateMessageInput) PrivateMessageInput.disabled = true;
    if (SendPrivateMessageButton) SendPrivateMessageButton.disabled = true;
}
function EnterRoom() {
    if (CreateRoomButton) CreateRoomButton.disabled = true;
    if (EnterRoomButton) EnterRoomButton.disabled = true;
    if (LeaveRoomButton) LeaveRoomButton.disabled = true;

    if (MessageInput) MessageInput.disabled = false;
    if (SendMessageButton) SendMessageButton.disabled = false;
    if (ToUserInput) ToUserInput.disabled = false;
    if (PrivateMessageInput) PrivateMessageInput.disabled = false;
    if (SendPrivateMessageButton) SendPrivateMessageButton.disabled = false;
}
function ExitRoom() {
    if (CreateRoomButton) CreateRoomButton.disabled = true;
    if (EnterRoomButton) EnterRoomButton.disabled = false;
    if (LeaveRoomButton) LeaveRoomButton.disabled = false;
    if (RoomCodeInput) RoomCodeInput.value = "";

    if (MessageInput) MessageInput.disabled = true;
    if (SendMessageButton) SendMessageButton.disabled = true;
    if (ToUserInput) ToUserInput.disabled = true;
    if (PrivateMessageInput) PrivateMessageInput.disabled = true;
    if (SendPrivateMessageButton) SendPrivateMessageButton.disabled = true;
}