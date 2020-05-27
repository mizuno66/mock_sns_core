"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/articleHub").build();

//Disable send button until connection is established
document.getElementById("sendButton").disabled = true;

connection.on("ReceiveMessage", function (id, userName, ApplicationUserName, message, imageContents, videoContents) {
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var divParent = document.createElement("div");
    divParent.className = "media";

    var a = document.createElement("a");
    a.className = "media-left";
    a.href = "#";

    var img = document.createElement("img");
    img.src = "/Home/UserPhoto/" + userName;
    img.className = "userphoto";

    var divBody = document.createElement("div");
    divBody.className = "media-body";
    divBody.id = id;

    var header = document.createElement("h4");
    header.className = "media-heading";
    header.textContent = ApplicationUserName;

    var divMsg = document.createElement("div");
    divMsg.textContent = msg;

    var divContentsRow = document.createElement("div");
    divContentsRow.className = "row";

    var list = imageContents.split(',');
    for (let i in list)
    {
        if (list[i] != "") {
            var divContentCol = document.createElement("div");
            divContentCol.className = "col-md-6";

            var imgContent = document.createElement("img");
            imgContent.setAttribute("data-original", "/Contents/" + userName + "/" + list[i]);
            imgContent.className = "content-image-preview lazy";
            imgContent.setAttribute("onclick", "popImage(this)");
            imgContent.setAttribute("asp-append-version", "true");

            divContentCol.appendChild(imgContent);
            divContentsRow.appendChild(divContentCol);
        }
    }

    list = videoContents.split(',');
    for (let i in list) {
        if (list[i] != "") {
            var divContentCol = document.createElement("div");
            divContentCol.className = "col-md-6";

            var videoContent = document.createElement("video");
            videoContent.src = "/Contents/" + userName + "/" + list[i] + "/thumbnail.mp4" ;
            videoContent.className = "content-video-preview";
            videoContent.setAttribute("onclick", "popVideo(this)");
            videoContent.setAttribute("asp-append-version", "true");

            divContentCol.appendChild(videoContent);
            divContentsRow.appendChild(divContentCol);
        }
    }


    divMsg.appendChild(divContentsRow);

    divParent.appendChild(a);
    a.appendChild(img);
    divParent.appendChild(divBody);
    divBody.appendChild(header);
    divBody.appendChild(divMsg);

    var list = document.getElementById("messagesList")
    list.insertBefore(divParent, list.firstChild);
    $('img.lazy').lazyload();
});

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

function FetchSubmit(oFormElement) {
    const formData = new FormData(oFormElement);
    const xhr = new XMLHttpRequest();

    xhr.open(oFormElement.method, oFormElement.action);

    xhr.addEventListener('load', (evt) => {
        // 正常終了
        let response = JSON.parse(xhr.responseText);

        var userName = document.getElementById("userInput").value;
        var message = document.getElementById("messageInput").value;
        connection.invoke("SendMessage", response.art_Id.toString(), userName, message).catch(function (err)
        {
            setAlert("alert-danger", err.toString());
            return console.error(err.toString());
        });
        resetForm();
    });

    xhr.addEventListener("error", (evt) => {
        // リクエスト送信エラー
        setAlert("alert-danger", "Network Error");
    });

    xhr.upload.addEventListener("loadstart", (evt) => {
        // アップロード開始
        var pg = document.getElementById("pg");
        pg.style.visibility = "visible";
        var pgb = document.getElementById("pgb");
        pgb.innerText = "アップロード中"
    });

    xhr.upload.addEventListener("load", (evt) => {
        // アップロード正常終了
        var pgb = document.getElementById("pgb");
        pgb.innerText = "処理中";
    });

    xhr.upload.addEventListener('progress', (evt) => {
        // 進捗
        let percent = (evt.loaded / evt.total * 100).toFixed(1);
        var pgb = document.getElementById("pgb");
        pgb.style.width = `${percent}%`;
    });

    xhr.upload.addEventListener("abort", (evt) => {
        // アップロード中断
        setAlert("alert-danger", "Upload Abort");
    });

    xhr.upload.addEventListener("error", (evt) => {
        // アップロードエラー
        setAlert("alert-danger", "Upload Error");
    });

    xhr.upload.addEventListener("timeout", (evt) => {
        // アップロードタイムアウト
        setAlert("alert-danger", "Upload Timeout");
    });

    xhr.send(formData);
}

function resetForm() {
    document.getElementById("messageInput").value = "";
    document.getElementById("image-select").value = "";
    var preview = document.getElementById("imagepreview");
    while (preview.firstChild) {
        preview.removeChild(preview.firstChild);
    }
    preview = document.getElementById("videopreview");
    while (preview.firstChild) {
        preview.removeChild(preview.firstChild);
    }
    var pg = document.getElementById("pg");
    pg.style.visibility = "hidden";
    var pgb = document.getElementById("pgb");
    pgb.style.width = "0%";
    pgb.innerText = "";
}


