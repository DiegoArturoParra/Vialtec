$(document).ready(function (e) {
    // -------------------------------------- MAP -----------------------------------------------------------------
    var map = L.map('map').setView([4.7109745, -74.2121788], 5.3); // ([lat, lng], zoom)
    var markersCluster;
    var mapMarkersSimples = [];
    var groupMarkersToFitBounds = [];

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    // show the scale bar on the lower left corner
    L.control.scale().addTo(map);
    // ------------------------------------- END MAP  ----------------------------------------------------------

    // Event Click Tab Map
    $('#nav-map-tab').click(function (e) {
        handleFilter();
    });

    // Filtrar
    const handleFilter = () => {
        // Filters object
        const filters = {
            muReportId: $('#muReportId').val()
        };

        // Aplicar filtros
        applyFilters(filters);
    }

    // Aplicar los filtros y obtener los registros mediante AJAX
    const applyFilters = (filters) => {
        // Loading
        Swal.fire({ icon: 'info', title: 'Espere por favor', text: 'Cargando información', allowOutsideClick: false });
        Swal.showLoading();

        $.ajax({
            url: "CoefficientFriction/GetDataByFilters",
            data: filters,
            type: "get"
        }).done(function (results) {
            if (results.length > 0) {
                // Mostrar los marcadores en el mapa
                try {
                    showMarkers(results);
                } catch (err) {
                    console.warn(err);
                }
            } else {
                filters = null;
                Swal.fire({ icon: 'warning', title: 'Mensaje', text: 'No se encontraron resultados para la búsqueda' });
            }
        }).fail(function (xhr, status, error) {
            console.error(error);
        });
    }

    // Mostrar cualquier mensaje de error
    function showMessageError(text) {
        Swal.fire({ icon: 'error', title: 'Error', text });
    }

    // Mostrar los marcadores que se obtuvieron de la consulta por filtros
    const showMarkers = (results) => {

        // setInterval(() => { console.log('Zoom', map.getZoom()); }, 1000);

        // Vaciar marcadores sin agrupar (no agrupamiento)
        clearSimpleMarkers();

        // Eliminar anterior Layer de markers (agrupamiento)
        if (markersCluster) {
            map.removeLayer(markersCluster);
        }

        // Vaciar fitBounds markers array
        groupMarkersToFitBounds = [];

        // determinar si agrupar o no los resultados
        //const clusterEnabled = $('#chk_agrupar').prop('checked');
        const clusterEnabled = true;

        // determinar si mostrar valores o no
        //var showValueEnabled = $('#mostrarValoresCheck')[0].checked;
        const showValueEnabled = true;

        // Cluster Markers
        markersCluster = new L.MarkerClusterGroup({ disableClusteringAtZoom: 16 });

        // minimo y tolerancia
        const minimo = $('#minimo').val();
        const tolerancia = $('#tolerancia').val();

        // Recorrer items results para mostrar los Modals con los detalles del item
        results.forEach(x => {
            // obtener lat y lng
            var lat = Number(x.latitude);
            var lng = Number(x.longitude);

            const iconOptions = {
                iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-blue.png',
                shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
                iconSize: [25, 41],
                iconAnchor: [12, 41],
                popupAnchor: [1, -34],
                shadowSize: [41, 41]
            };

            if (minimo && tolerancia) {
                iconOptions.iconUrl = getColorByMinimoTolerancia(minimo, tolerancia, x.mu)
            }

            var customIcon = new L.Icon(iconOptions);

            // Creando Marcador
            var marker = L.marker([lat, lng], { icon: customIcon })
                .bindTooltip(`${ x.mu }`, {
                    permanent: showValueEnabled, // mostrar valores ? permanent: true or false
                    direction: 'top',
                    offset: [0, -39],
                    className: 'leaflet-tooltip'
                });
            // clikc event para los marcadores
            marker.on('click', function (e) {
                // Loading
                Swal.fire({ icon: 'info', title: 'Espere por favor', text: 'Cargando información', allowOutsideClick: false });
                Swal.showLoading();

                $.ajax({
                    url: "CoefficientFriction/GetCoefficientFrictionById",
                    data: { id: x.id },
                    type: "get"
                }).done(function (result) {
                    Swal.fire({
                        showConfirmButton: false,
                        html: `
                        <b class="text-muted text-center d-block mb-1">DETALLES REGISTRO</b>
                        <table class="table table-sm table-bordered">
                            <tbody>
                                <tr>
                                    <td><b class="text-muted">Reporte</b></td>
                                    <td>${result.muReport.title}</td>
                                </tr>
                                <tr>
                                    <td><b class="text-muted">Mu</b></td>
                                    <td>${result.mu}</td>
                                </tr>
                                <tr>
                                    <td><b class="text-muted">Velocidad</b></td>
                                    <td>${result.speed}</td>
                                </tr>
                                <tr>
                                    <td><b class="text-muted">PR</b></td>
                                    <td>${result.prStr}</td>
                                </tr>
                                <tr>
                                    <td><b class="text-muted">Temperatura Ambiente</b></td>
                                    <td>${result.temperatureEnvironment}</td>
                                </tr>
                                <tr>
                                    <td><b class="text-muted">Temperatura Vial</b></td>
                                    <td>${result.temperatureVia}</td>
                                </tr>
                                <tr>
                                    <td><b class="text-muted">Fecha</b></td>
                                    <td>${result.date && result.date.replace('T', ' ').split('.')[0]}</td>
                                </tr>
                                <tr>
                                    <td><b class="text-muted">Latitud</b></td>
                                    <td>${result.latitude}</td>
                                </tr>
                                <tr>
                                    <td><b class="text-muted">Longitud</b></td>
                                    <td>${result.longitude}</td>
                                </tr>
                            </tbody>
                        </table>
                        <div style="position: absolute; top: 5px; right: 7px;">
                            <span class="text-muted" style="cursor: pointer;" onclick="Swal.close()">
                                <i class="fas fa-times"></i>
                            </span>
                        </div>
                        `
                    }).then(console.log);
                }).fail(function (xhr, status, error) {
                    console.error(error);
                });
            });

            // groupMarkersToFitBounds 
            groupMarkersToFitBounds.push(marker);

            if (clusterEnabled === true) {
                // Agrupamiento (Cluster) agregar al Layer
                markersCluster.addLayer(marker);
            } else {
                // Sin agrupar - agregar al mapa directamente
                mapMarkersSimples.push(marker);
                marker.addTo(map);
            }
        });
        if (clusterEnabled) {
            // agregar el layer de ClusterMarkers
            map.addLayer(markersCluster);
        }

        setTimeout(function () {
            map.invalidateSize();

            // fitBounds para centrar el mapa donde el grupo de markers esté
            var group = new L.featureGroup(groupMarkersToFitBounds);
            map.fitBounds(group.getBounds());
        }, 800);

        // Cerrar loading y modal
        Swal.close();
        $('#exampleModal').modal('hide');
    }

    // Obtener el color del marker por mínimo/tolerancia
    const getColorByMinimoTolerancia = (minimo, tolerancia, mu) => {
        if (mu < (minimo - tolerancia)) {
            return 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-red.png';
        } else if (mu >= (minimo - tolerancia) && mu < minimo) {
            return 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-yellow.png';
        } else if (mu >= minimo) {
            return 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-green.png';
        }
    }

    // Limpiar los marcadores del mapa
    function clearSimpleMarkers() {
        for (var i = 0; i < mapMarkersSimples.length; i++) {
            map.removeLayer(mapMarkersSimples[i]);
        }
        mapMarkersSimples = [];
    }

    // Export Excel
    $('#btn-export-excel').click(function (e) {
        $('#muReportId_xls').val($('#muReportId').val());
        $('#form-export-excel').submit();
    });

    // Export Kml
    $('#btn-export-kml').click(function (e) {
        $('#muReportId_kml').val($('#muReportId').val());
        $('#form-export-kml').submit();
    });
});