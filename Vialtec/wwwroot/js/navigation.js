$(document).ready(function (e) {
    $('[data-toggle="tooltip"]').tooltip();
    $('.number-page').hover(function (e) {
        $(this).css('transform', 'translateY(-5px)');
        $(this).addClass('shadow');
        $(this).removeClass('text-muted');
    });
    $('.number-page').mouseleave(function (e) {
        $(this).css('transform', 'none');
        $(this).removeClass('shadow');
        if (!$(this).hasClass('badge-info')) {
            $(this).addClass('text-muted');
        }
    });
});