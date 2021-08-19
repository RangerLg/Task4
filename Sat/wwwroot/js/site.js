/// <reference path="../lib/jquery/dist/jquery.js" />

$('#theme').change(function () {
    console.log("Hi")
    var item = $("#theme option:selected").text();
    $.post("/Home/SetTheme",{
        data:item
    });
})