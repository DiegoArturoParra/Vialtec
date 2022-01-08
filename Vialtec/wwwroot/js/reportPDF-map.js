$(document).ready(function (e) {
    // MAPA DEFINCIÓN ----------------------------------------------------------------------------------------------
    var map = L.map('map').setView([4.7109745, -74.2121788], 6); // ([lat, lng], zoom)

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    // show the scale bar on the lower left corner
    L.control.scale().addTo(map);

    var markers = [];

    //-------------------------------------------------------------------------------------------------------------
    var filters = {
        activities: $('#activities').val(),
        geometryId: $('#geometryId').val(),
        lineColorId: $('#lineColorId').val(),
        lineNumber: $('#lineNumber').val(),
        dateInit: $('#dateInit').val(),
        dateFinal: $('#dateFinal').val(),
        pr_init: $('#pr_init').val(),
        pr_final: $('#pr_final').val(),
        lineNumbers: $('#lineNumbers').val()
    };

    // Loading
    Swal.fire({
        icon: 'info',
        title: 'Espere por favor',
        text: 'Cargando información',
        allowOutsideClick: false
    });
    Swal.showLoading();

    $.ajax({
        url: '/Reflectivities/GetCoordinatesByFilters',
        data: filters,
        type: 'get'
    }).done(function (results) {
        showCircles(results);
    }).fail(function (error) { console.error(error); });

    function showCircles(results) {
        results.forEach(x => {
            var marker = L.marker([x.latitude, x.longitude]);
            markers.push(marker);
            marker.addTo(map);
        });

        // fitBounds para centrar el mapa donde el grupo de markers esté
        var group = new L.featureGroup(markers);
        map.fitBounds(group.getBounds());
        Swal.close();
    }

    $('#btn-download-map').click(function (e) {
        Swal.fire({
            icon: 'info',
            title: 'Espere por favor',
            text: 'Cargando información',
            allowOutsideClick: false
        });
        Swal.showLoading();
        leafletImage(map, downloadMap);
    });

    function downloadMap(err, canvas) {
        var imgData = canvas.toDataURL("image/svg+xml", 1.0);
        var dimensions = map.getSize();

        var pdf = new jsPDF('l', 'pt', 'letter');
        pdf.addImage(imgData, 'PNG', 110, 40, dimensions.x * 0.5, dimensions.y * 0.5);
        let now = new Date();
        let dateString = now.toISOString();
        let fileName = `map_${dateString}.pdf`;
        pdf.save(fileName);
        Swal.close();
    };
});