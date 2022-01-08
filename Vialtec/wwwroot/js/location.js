Date.prototype.toDateInputValue = (function () {
    var local = new Date(this);
    local.setMinutes(this.getMinutes() - this.getTimezoneOffset());
    return local.toJSON().slice(0, 10);
});

$(document).ready(function (e) {
    // MAPA DEFINCIÓN ----------------------------------------------------------------------------------------------
    var map = L.map('map').setView([4.7109745, -74.2121788], 6); // ([lat, lng], zoom)

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    // show the scale bar on the lower left corner
    L.control.scale().addTo(map);

    var mapMarkersSimples = [];
    var polyline;

    // -------------------------------------------------------------------------------------------------------
    // Inicializar input date y time
    $('input[type=date]').val(new Date().toDateInputValue());
    $('#timeInit').val('00:00');
    $('#timeFinal').val('23:59');
    // -------------------------------------------------------------------------------------------------------
    var filters;
    var equipmentsSelected = [];

    showEquipmentsSelected();
    // Equipments del EquipmentGroup inicial del select
    getEquipments($('#equipmentGroupId').val());

    // onchange projects select, obtener los equipments por equipmentGroup
    $('#equipmentGroupId').change(function (e) {
        getEquipments($(this).val());
    });

    // change reportType
    $('input[name=tipo]').change(function (e) {
        if ($(this).val() === 'ruta') {
            $('#date-time-filter').removeClass('d-none');
            equipmentsSelected = [];
            getEquipments($('#equipmentGroupId').val());
            showEquipmentsSelected();
        } else {
            $('#date-time-filter').addClass('d-none');
        }
    });

    // click agregar equipment al filtro
    $('#table-equipments').on('click', '.add-equipment', function (e) {

        var types = $('input[name=tipo]');
        var checkedType = types.filter(":checked").val();

        if (checkedType == 'ruta' && equipmentsSelected.length == 1) {
            Swal.fire({
                icon: 'warning',
                text: 'El reporte de ruta es sólo para grupos y activos viales individuales'
            });
        } else {
            let id = $(this).attr('id').split('-')[1];
            let equipmentAlias = $(this).closest('tr').find('.equipmentAlias').val();
            if (!existsEquipment(id)) {
                equipmentsSelected.push({ id, equipmentAlias });
                //// refresh equipments
                getEquipments($('#equipmentGroupId').val());
                //// refresh seleccionados
                showEquipmentsSelected();
            }
        }
    });

    // click eliminar equipment del filtro
    $('#table-seleccion').on('click', '.delete-equipment', function (e) {
        let id = $(this).attr('id').split('-')[1];
        equipmentsSelected = equipmentsSelected.filter(x => x.id != id);
        //// refresh equipments
        getEquipments($('#equipmentGroupId').val());
        //// refresh seleccionados
        showEquipmentsSelected();
    });

    // Validar y posteriormente filtrar
    $('#btn-filtrar').click(function (e) {
        if (equipmentsSelected.length === 0) {
            showMessageError('No has seleccionado ningún activo vial');
            return;
        }
        // obtener Date y Time: init y final
        var dateInit = $('#dateInit').val();
        var dateFinal = $('#dateFinal').val();
        var timeInit = $('#timeInit').val();
        var timeFinal = $('#timeFinal').val();

        var types = $('input[name=tipo]');
        var checkedType = types.filter(":checked").val();

        if (checkedType == 'ruta') {
            if (dateInit == '' || dateFinal == '' || timeInit == '' || timeFinal == '') {
                showMessageError('Es necesario completar todos los campos para aplicar el filtro por fecha y hora');
                return;
            } else {
                let date1 = Date.parse(`${dateInit} ${timeInit}`);
                let date2 = Date.parse(`${dateFinal} ${timeFinal}`);
                if (date1 > date2) {
                    showMessageError('La fecha inicial no debe ser mayor a la final');
                    return;
                }
            }
        }

        filters = {
            reportType: checkedType,
            equipments: JSON.stringify(equipmentsSelected),
            dateInitComplete: checkedType == 'ruta' ? `${dateInit} ${timeInit}` : null,
            dateFinalComplete: checkedType == 'ruta' ? `${dateFinal} ${timeFinal}` : null
        };
        // aplicar los filtros
        applyFilters();
    });

    function showMessageError(text) {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text
        });
    }

    // Aplicar los filtros y obtener los registros mediante AJAX
    function applyFilters() {
        // Loading
        Swal.fire({
            icon: 'info',
            title: 'Espere por favor',
            text: 'Cargando información',
            allowOutsideClick: false
        });
        Swal.showLoading();

        $.ajax({
            url: "Location/GetLocationEquipments",
            data: filters,
            type: "get"
        }).done(function (response) {
            if (response.results.length !== 0) {
                if (response.reportType == 'ubicacion') {
                    showLocationEquipments(response.results);
                } else if (response.reportType == 'ruta') {
                    showRouteReport(response.results);
                }
            } else {
                filters = null;
                Swal.fire({
                    icon: 'warning',
                    title: 'Mensaje',
                    text: 'No se encontraron resultados para la búsqueda'
                });
            }
        }).fail(function (xhr, status, error) {
            console.error(error);
        });
    }

    // Mostrar la ruta del equipment
    function showRouteReport(results) {

        // Remover el polyline
        if (polyline) {
            polyline.removeFrom(map);
        }

        let latLngs = [];

        results.forEach(x => {
            latLngs.push([x.latitude, x.longitude]);
        });

        createMarkersInitFinal(results[0], results[results.length - 1]);
        polyline = L.polyline(latLngs, { color: 'red' }).addTo(map);
        map.fitBounds(polyline.getBounds());
        // Cerrar loading y modal
        Swal.close();
        $('#modal-location').modal('hide');
    }

    function createMarkersInitFinal(init, final) {
        // Vaciar marcadores sin agrupar (no agrupamiento)
        clearSimpleMarkers();
        // Creando Marcador con tooltip permanente
        let markerInit = L.marker([init.latitude, init.longitude])
            .bindTooltip(`
                        <span style="display: block; text-align: center;">
                            Inicio</br>
                            <small>${init.deviceDt.replace('T', ' ').split('.')[0]}</small>
                        </span>
                    `, {
                    permanent: true,
                    direction: 'top',
                    offset: [0, -10],
                    className: 'leaflet-tooltip'
            });
        let markerFinal = L.marker([final.latitude, final.longitude])
            .bindTooltip(`
                        <span style="display: block; text-align: center;">
                            Fin</br>
                            <small>${final.deviceDt.replace('T', ' ').split('.')[0]}</small>
                        </span>
                    `, {
                    permanent: true,
                    direction: 'top',
                    offset: [0, -10],
                    className: 'leaflet-tooltip'
            });
        // agregar al array de markers y al mapa
        mapMarkersSimples.push(markerInit);
        mapMarkersSimples.push(markerFinal);
        markerInit.addTo(map);
        markerFinal.addTo(map);
    }

    // Mostrar los marcadores 
    function showLocationEquipments(results) {

        // Vaciar marcadores sin agrupar (no agrupamiento)
        clearSimpleMarkers();

        // Removar el polyline
        if (polyline) {
            polyline.removeFrom(map);
        }

        // Recorrer items results
        results.forEach(x => {
            // obtener lat y lng
            let lat = Number(x.lastLatitude);
            let lng = Number(x.lastLongitude);
            // Creando Marcador con tooltip permanente
            let marker = L.marker([lat, lng])
                .bindTooltip(`
                        ${x.equipmentAlias}
                        <br/>
                        <small style="display: block; text-align: center;">${x.lastPositionDt.replace('T', ' ').split('.')[0]}</small>
                    `, {
                    permanent: true,
                    direction: 'top',
                    offset: [0, -10],
                    className: 'leaflet-tooltip'
                });

            // Sin agrupar - agregar al mapa directamente
            mapMarkersSimples.push(marker);
            marker.addTo(map);
        });

        // fitBounds para centrar el mapa donde el grupo de markers esté
        var group = new L.featureGroup(mapMarkersSimples);
        map.fitBounds(group.getBounds());

        // Cerrar loading y modal
        Swal.close();
        $('#modal-location').modal('hide');
    }

    function clearSimpleMarkers() {
        for (var i = 0; i < mapMarkersSimples.length; i++) {
            map.removeLayer(mapMarkersSimples[i]);
        }
        mapMarkersSimples = [];
    }

    // Verificar si el ID está en los equipments seleccionados
    function existsEquipment(equipmentId) {
        var item = equipmentsSelected.find(x => x.id == equipmentId);
        return item !== undefined;
    }

    // función para recuperar los equipments del equipmentGroupId
    function getEquipments(equipmentGroupId) {
        $.ajax({
            url: "Location/GetEquipmentsByEquipmentGroupId",
            data: {
                equipmentGroupId
            },
            type: "get"
        }).done(function (equipments) {
            // limpiar actividades actuales
            $('#table-equipments tr').remove();
            if (equipments.length === 0) {
                let emptyMessage = `
                     <tr class="text-center">
                        <td colspan="2">No se encontraron activos viales</td>
                    </tr>
                `;
                $('#table-equipments').append(emptyMessage);
            }
            // Mostrar las actividades
            equipments.forEach(x => {
                let equipmentHTML = '';
                if (!existsEquipment(x.id)) {
                    equipmentHTML = `
                        <tr class="text-center">
                            <input type="hidden" value="${x.equipmentAlias}" class="equipmentAlias" />
                            <td>${x.equipmentAlias}</td>
                            <td>
                                <span class="badge badge-info add-equipment" style="cursor: pointer;" id="equipment-${x.id}">
                                    <i class="fas fa-plus"></i>
                                </span>
                            </td>
                        </tr>
                    `;
                } else {
                    equipmentHTML = `
                        <tr class="text-center">
                            <td>${x.equipmentAlias}</td>
                            <td>
                                <span class="badge badge-success" style="cursor: default;">
                                    <i class="fas fa-check"></i>
                                </span>
                            </td>
                        </tr>
                    `;
                }
                $('#table-equipments').append(equipmentHTML);
            });
        }).fail(function (xhr, status, error) {
            console.error(error);
        });
    }

    // Mostrar las actividades que han sido seleccionadas
    function showEquipmentsSelected() {
        $('#table-seleccion tr').remove();
        if (equipmentsSelected.length == 0) {
            let emptyMessage = `
                     <tr class="text-center">
                        <td colspan="2">Ninguno</td>
                    </tr>
                `;
            $('#table-seleccion').append(emptyMessage);
        }
        equipmentsSelected.forEach(x => {
            let equipmentHTML = `
                <tr class="text-center">
                    <td>${x.equipmentAlias}</td>
                    <td>
                        <span class="badge badge-danger delete-equipment" style="cursor: pointer;" id="delete-${x.id}">
                            <i class="fas fa-trash"></i>
                        </span>
                    </td>
                </tr>
            `;
            $('#table-seleccion').append(equipmentHTML);
        });
    }
});