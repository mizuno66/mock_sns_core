document.getElementById('image-select').addEventListener('change', function (e) {
    var list = e.target.files;

    var preview = document.getElementById("preview");
    preview.textContent = null;

    for (let i = 0; i < list.length; i++) {
        var file = e.target.files[i];

        var blobUrl = window.URL.createObjectURL(file);

        var img = document.createElement("img");
        img.src = blobUrl;
        img.width = 100;
        img.height = 100;

        preview.appendChild(img);
    }
});