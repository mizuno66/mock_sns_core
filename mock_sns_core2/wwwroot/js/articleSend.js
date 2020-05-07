"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/articleHub").build();

//Disable send button until connection is established
document.getElementById("sendButton").disabled = true;

connection.on("ReceiveMessage", function (id, userName, ApplicationUserName, message) {
    console.log("receivemessage");
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var divParent = document.createElement("div");
    divParent.className = "media";

    var a = document.createElement("a");
    a.className = "media-left";
    a.href = "#";

    var img = document.createElement("img");
    img.src = "/Home/UserPhoto/" + userName;
    img.width = 150;
    img.height = 150;

    var divBody = document.createElement("div");
    divBody.className = "media-body";
    divBody.id = id;

    var header = document.createElement("h4");
    header.className = "media-heading";
    header.textContent = ApplicationUserName;

    var divMsg = document.createElement("div");
    divMsg.textContent = msg;

    divParent.appendChild(a);
    a.appendChild(img);
    divParent.appendChild(divBody);
    divBody.appendChild(header);
    divBody.appendChild(divMsg);

    var list = document.getElementById("messagesList")
    list.insertBefore(divParent, list.firstChild);
});

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    var userName = document.getElementById("userInput").value;
    var message = document.getElementById("messageInput").value;
    connection.invoke("SendMessage", userName, message).catch(function (err) {
        return console.error(err.toString());
    });
    document.getElementById("messageInput").value = "";
    event.preventDefault();
});