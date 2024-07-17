"use strict";
var connection = null;
var loginButton = document.getElementById("SendLogin");
var logoutButton = document.getElementById("SendLogout");
var usernameInput = document.getElementById("Username");
var loginLogText = document.getElementById("LoginLog");
var CreateRoomButton = document.getElementById("CreateRoom");
var RoomCodeInput = document.getElementById("RoomCode");
var EnterRoomButton = document.getElementById("EnterRoom");
var LeaveRoomButton = document.getElementById("LeaveRoom");
var MessageInput = document.getElementById("Message");
var SendMessageButton = document.getElementById("SendMessage");
var ToUserInput = document.getElementById("ToUser");
var PrivateMessageInput = document.getElementById("PrivateMessage");
var SendPrivateMessageButton = document.getElementById("SendPrivateMessage");
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
};
function EventListeners() {
    loginButton === null || loginButton === void 0 ? void 0 : loginButton.addEventListener("click", () => {
        connection = new signalR.HubConnectionBuilder().withUrl(`/chatHub?username=${usernameInput === null || usernameInput === void 0 ? void 0 : usernameInput.value}`).build();
        connection.start().then(() => {
            if (loginLogText)
                loginLogText.innerText = `Conectado como [${usernameInput === null || usernameInput === void 0 ? void 0 : usernameInput.value}]`;
            Login();
            SocketListeners();
        }).catch(() => {
        });
    });
    logoutButton === null || logoutButton === void 0 ? void 0 : logoutButton.addEventListener("click", () => {
        connection === null || connection === void 0 ? void 0 : connection.stop().then(() => {
            if (loginLogText)
                loginLogText.innerText = `Desconectado.`;
            Logout();
        }).catch(() => {
        });
    });
    CreateRoomButton === null || CreateRoomButton === void 0 ? void 0 : CreateRoomButton.addEventListener("click", () => {
        connection === null || connection === void 0 ? void 0 : connection.invoke("CreateRoom").catch(function (err) {
            return console.error(err.toString());
        });
    });
    EnterRoomButton === null || EnterRoomButton === void 0 ? void 0 : EnterRoomButton.addEventListener("click", () => {
        if (RoomCodeInput == null)
            return;
        connection === null || connection === void 0 ? void 0 : connection.invoke("JoinRoom", RoomCodeInput.value).catch(function (err) {
            return console.error(err.toString());
        });
    });
    LeaveRoomButton === null || LeaveRoomButton === void 0 ? void 0 : LeaveRoomButton.addEventListener("click", () => {
        connection === null || connection === void 0 ? void 0 : connection.invoke("LeaveRoom").catch(function (err) {
            return console.error(err.toString());
        });
    });
    SendMessageButton === null || SendMessageButton === void 0 ? void 0 : SendMessageButton.addEventListener("click", () => {
        const message = MessageInput === null || MessageInput === void 0 ? void 0 : MessageInput.value;
        connection === null || connection === void 0 ? void 0 : connection.invoke("SendToGroup", message).then(() => {
            MessageInput.value = "";
        }).catch(function (err) {
            return console.error(err.toString());
        });
    });
    SendPrivateMessageButton === null || SendPrivateMessageButton === void 0 ? void 0 : SendPrivateMessageButton.addEventListener("click", () => {
        const message = PrivateMessageInput === null || PrivateMessageInput === void 0 ? void 0 : PrivateMessageInput.value;
        connection === null || connection === void 0 ? void 0 : connection.invoke("SendPrivate", message).then(() => {
            PrivateMessageInput.value = "";
        }).catch(function (err) {
            return console.error(err.toString());
        });
    });
}
;
function SocketListeners() {
    connection === null || connection === void 0 ? void 0 : connection.on("NewMessage", (message) => {
        var li = document.createElement("li");
        li.textContent = `${message.fromUser}: ${message.content}`;
        if (chatMessageList)
            chatMessageList.appendChild(li);
    });
    connection === null || connection === void 0 ? void 0 : connection.on("OnNewUsers", (userList) => {
        if (userLoggedList == null)
            return;
        while (userLoggedList.firstChild) {
            userLoggedList.removeChild(userLoggedList.firstChild);
        }
        userList.forEach(user => {
            var li = document.createElement("li");
            li.textContent = `${user}`;
            if (userLoggedList)
                userLoggedList.appendChild(li);
        });
    });
    connection === null || connection === void 0 ? void 0 : connection.on("OnCreateRoom", (roomCode) => {
        var li = document.createElement("li");
        li.textContent = `${usernameInput === null || usernameInput === void 0 ? void 0 : usernameInput.value} criou a sala ${roomCode}`;
        if (chatLogList)
            chatLogList.appendChild(li);
        if (RoomCodeInput)
            RoomCodeInput.value = roomCode;
        EnterRoom();
    });
    connection === null || connection === void 0 ? void 0 : connection.on("OnJoinRoom", (username) => {
        var li = document.createElement("li");
        li.textContent = `${username} entrou na sala`;
        if (chatLogList)
            chatLogList.appendChild(li);
    });
    connection === null || connection === void 0 ? void 0 : connection.on("OnLeaveRoom", (username) => {
        var li = document.createElement("li");
        li.textContent = `${username} saiu da sala`;
        if (chatLogList)
            chatLogList.appendChild(li);
    });
}
;
function Login() {
    if (usernameInput)
        usernameInput.disabled = true;
    if (loginButton)
        loginButton.disabled = true;
    if (logoutButton)
        logoutButton.disabled = false;
    if (CreateRoomButton)
        CreateRoomButton.disabled = false;
    if (EnterRoomButton)
        EnterRoomButton.disabled = false;
    if (LeaveRoomButton)
        LeaveRoomButton.disabled = false;
    if (RoomCodeInput)
        RoomCodeInput.disabled = false;
    if (MessageInput)
        MessageInput.disabled = false;
    if (SendMessageButton)
        SendMessageButton.disabled = false;
    if (ToUserInput)
        ToUserInput.disabled = false;
    if (PrivateMessageInput)
        PrivateMessageInput.disabled = false;
    if (SendPrivateMessageButton)
        SendPrivateMessageButton.disabled = false;
}
function Logout() {
    if (usernameInput)
        usernameInput.disabled = false;
    if (loginButton)
        loginButton.disabled = false;
    if (logoutButton)
        logoutButton.disabled = true;
    if (CreateRoomButton)
        CreateRoomButton.disabled = true;
    if (EnterRoomButton)
        EnterRoomButton.disabled = true;
    if (LeaveRoomButton)
        LeaveRoomButton.disabled = true;
    if (RoomCodeInput)
        RoomCodeInput.disabled = true;
    if (MessageInput)
        MessageInput.disabled = true;
    if (SendMessageButton)
        SendMessageButton.disabled = true;
    if (ToUserInput)
        ToUserInput.disabled = true;
    if (PrivateMessageInput)
        PrivateMessageInput.disabled = true;
    if (SendPrivateMessageButton)
        SendPrivateMessageButton.disabled = true;
}
function EnterRoom() {
    if (CreateRoomButton)
        CreateRoomButton.disabled = true;
    if (EnterRoomButton)
        EnterRoomButton.disabled = true;
    if (LeaveRoomButton)
        LeaveRoomButton.disabled = true;
    if (MessageInput)
        MessageInput.disabled = false;
    if (SendMessageButton)
        SendMessageButton.disabled = false;
    if (ToUserInput)
        ToUserInput.disabled = false;
    if (PrivateMessageInput)
        PrivateMessageInput.disabled = false;
    if (SendPrivateMessageButton)
        SendPrivateMessageButton.disabled = false;
}
function ExitRoom() {
    if (CreateRoomButton)
        CreateRoomButton.disabled = true;
    if (EnterRoomButton)
        EnterRoomButton.disabled = false;
    if (LeaveRoomButton)
        LeaveRoomButton.disabled = false;
    if (RoomCodeInput)
        RoomCodeInput.value = "";
    if (MessageInput)
        MessageInput.disabled = true;
    if (SendMessageButton)
        SendMessageButton.disabled = true;
    if (ToUserInput)
        ToUserInput.disabled = true;
    if (PrivateMessageInput)
        PrivateMessageInput.disabled = true;
    if (SendPrivateMessageButton)
        SendPrivateMessageButton.disabled = true;
}
