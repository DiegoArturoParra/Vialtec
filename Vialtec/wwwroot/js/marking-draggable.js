$(document).ready(function (e) {
    $('#card-draggable').draggable();
});

// Ocultar el card draggable de tramas
$('#close-tramas').click(function (e) {
    console.log(e);
    $('#card-draggable').addClass('d-none');
});