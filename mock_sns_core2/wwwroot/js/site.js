// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function setAlert(typeName, message) {

    var alert = document.createElement("div");
    alert.className = "alert " + typeName + " alert-dismissible";
    alert.setAttribute("role", "alert");

    var button = document.createElement("button");
    button.className = "close";
    button.type = "button";
    button.setAttribute("data-dismiss", "alert");
    button.setAttribute("aria-label", "閉じる");
    button.innerHTML = "<span aria-hidden='true'>x</span>";

    alert.innerText = message;

    alert.appendChild(button);
    document.getElementById("alertMessage").appendChild(alert);
}