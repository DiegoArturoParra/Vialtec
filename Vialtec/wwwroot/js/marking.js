
class Reporte {
    constructor(fechaInicial, fechaFinal, leftPaint, centerPaint, rightPaint, totalMetros, tiempo) {
        this.fechaInicial = fechaInicial;
        this.fechaFinal = fechaFinal;
        this.tiempo = tiempo;
        this.leftPaint = leftPaint;
        this.centerPaint = centerPaint;
        this.rightPaint = rightPaint;
        this.totalMetros = totalMetros;
    }
}

$(document).ready(function (e) {
    $('#btnExportarExcel').prop("disabled", true);
    // MAPA DEFINCIÓN ----------------------------------------------------------------------------------------------
    var map = L.map('map').setView([4.7109745, -74.2121788], 6);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    // show the scale bar on the lower left corner
    L.control.scale().addTo(map);

    // Polylines
    var polylineTotal; // Traslado
    var polylinesMarking = []; // Demarcación
    // Markers
    var markers = [];
    // Arreglo de Promesas que contendran los resumenes de las tramas (demarcación)
    summaryPromises = [];

    // Creando iconos: inicial y final
    var iconInit = L.icon({
        iconUrl: '/img/map-icons/green-circle.png',
        iconSize: [20, 20] // [width, height]
    });

    var iconFinal = L.icon({
        iconUrl: '/img/map-icons/red-square.png',
        iconSize: [20, 20] // [width, height]
    });
    // -------------------------------------------------------------------------------------------------------

    // EQUIPMENT_GROUPS Y EQUIPMENTS -------------------------------------------------------------------------------
    var filters;

    // Equipments del EquipmentGroup inicial del select
    getEquipments($('#equipmentGroupId').val());

    // onchange projects select, obtener los equipments por equipmentGroup
    $('#equipmentGroupId').change(function (e) {
        getEquipments($(this).val());
    });

    // función para recuperar los equipments del equipmentGroupId
    function getEquipments(equipmentGroupId) {
        $.ajax({
            url: "Marking/GetEquipmentsByEquipmentGroupId",
            data: {
                equipmentGroupId
            },
            type: "get"
        }).done(function (equipments) {
            // Limpiar equipments actuales
            $('#equipmentId option').remove();
            if (equipments.length === 0) {
                let emptyOption = `<option value="-1">No hay dispositivos</option>`;
                $('#equipmentId').append(emptyOption);
            } else {
                // Mostrar los dispositivos
                equipments.forEach(equipment => {
                    let option = `<option value="${equipment.id}">${equipment.equipmentAlias}</option>`;
                    $('#equipmentId').append(option);
                });
            }
        }).fail(function (xhr, status, error) {
            console.error(error);
        });
    }

    // Validar y posteriormente filtrar
    $('#btn-filtrar').click(function (e) {
        const equipmentId = $('#equipmentId').val();
        if (equipmentId == '-1') {
            Swal.fire('Error', 'No has seleccionado el dispositivo', 'error');
            return;
        }
        // Obtener fecha y hora: inicial y final
        var datetime_inicial = $('#datetime-inicial').val();
        var datetime_final = $('#datetime-final').val();

        if (!datetime_inicial || !datetime_final) {
            Swal.fire('Error', 'Es necesario completar todos los campos para aplicar el filtro por fecha y hora', 'error');
            return;
        } else {
            let datetime_inicial_format = Date.parse(datetime_inicial);
            let datetime_final_format = Date.parse(datetime_final);

            // Verficiar formatos de fecha, más que todo para el navegador firefox donde se produce este error
            if (isNaN(datetime_inicial_format)) {
                Swal.fire('Error', 'El formato de la fecha inicial es inválido', 'error');
                return;
            }

            if (isNaN(datetime_final_format)) {
                Swal.fire('Error', 'El formato de la fecha final es inválido', 'error');
                return;
            }

            if (datetime_inicial_format > datetime_final_format) {
                Swal.fire('Error', 'La fecha inicial no debe ser mayor a la final', 'error');
                return;
            }
        }

        var radioButtonsOptions = $('input[name=exampleRadios]');
        var typeSelected = radioButtonsOptions.filter(":checked").val(); //traslado | demarcado | ambos

        // Definir los filtros
        filters = {
            equipmentId,
            dateInitComplete: datetime_inicial,
            dateFinalComplete: datetime_final
        };

        $('#checkAll').prop('checked', false);
        // Mostrar los registros dependiendo del tipo de solicitud (traslado, demarcado)
        if (typeSelected === 'traslado') {
            loadTransmissionsInfo();
        } else if (typeSelected === 'demarcado') {
            loadMarkingResults();
        }
    });

    // Change Event para el radio button de tipo reporte
    $('input[name=exampleRadios]').change(function (e) {
        const value = $(this).val();
        if (value == 'demarcado') {
            $('#col-min-meters').removeClass('d-none');
        } else if (value == 'traslado') {
            $('#col-min-meters').addClass('d-none');
        }
    });

    // Eliminar los marcadores colocados
    function clearMarkers() {
        for (var i = 0; i < markers.length; i++) {
            map.removeLayer(markers[i]);
        }
        markers = [];
    }

    // Limpiar mapa
    function clearMap() {
        // Eliminar marcadores
        clearMarkers();
        // Remover el actual polyline de traslado
        if (polylineTotal) {
            polylineTotal.removeFrom(map);
        }
        // Remover el actual polyline de demarcado
        if (polylinesMarking.length > 0) {
            polylinesMarking.forEach(x => {
                x.removeFrom(map);
            });
        }
    }

    // Cargar los registros del reporte de traslado
    function loadTransmissionsInfo() {
        Swal.fire({
            icon: 'info',
            title: 'Espere por favor',
            text: 'Cargando información',
            allowOutsideClick: false
        });
        Swal.showLoading();

        $.ajax({
            url: "Marking/GetTransmissionsInfo",
            data: filters,
            dataType: "json",
            type: "POST"
        }).done(function (results) {
            if (results.length > 0) {
                showTransmissionsInfo(results);
            } else {
                Swal.fire('Warning', 'No se encontraron resultados para la búsqueda', 'warning');
            }
        }).fail(function (xhr, status, error) {
            console.error(error);
        });
    }

    // Mostrar el reporte de traslado
    showTransmissionsInfo = (transmissionsInfo) => {
        // Ocultar fab buttons si es necesario
        if (!$('#fab-container').hasClass('d-none')) {
            $('#fab-container').addClass('d-none');
        }

        // Ocultar Tramas draggable si es necesario
        if (!$('#card-draggable').hasClass('d-none')) {
            $('#card-draggable').addClass('d-none');
        }

        // Limpiar mapa
        clearMap();

        // Array de latitudes y longitudes
        let latLngs = [];

        transmissionsInfo.forEach(x => {
            if (x.latitude && x.longitude) {
                latLngs.push([x.latitude, x.longitude]);
            }
        });

        // Registrar los marcadores inicial y final
        createMarkersInitFinal(transmissionsInfo[0], transmissionsInfo[transmissionsInfo.length - 1]);

        // Mostrar polyline en el mapa
        polylineTotal = L.polyline(latLngs, { color: 'red' }).addTo(map);
        // Zoom the map to the polyline de traslado
        map.fitBounds(polylineTotal.getBounds());

        // Cerrar loading y modal
        Swal.close();
        $('#exampleModal').modal('hide');
    }

    // Crear el marcador inicial y final
    function createMarkersInitFinal(init, final) {
        // Creando Marcador con tooltip permanente
        let markerInit = L.marker([init.latitude, init.longitude], { icon: iconInit })
            .bindTooltip(`
                        <span style="display: block; text-align: center;">
                            Inicio</br>
                            <small>${init.deviceDt.replace('T', ' ').split('.')[0]}</small>
                        </span>
                    `, {
                permanent: false,
                direction: 'left',
                offset: [0, 0],
                className: 'leaflet-tooltip'
            });

        let markerFinal = L.marker([final.latitude, final.longitude], { icon: iconFinal })
            .bindTooltip(`
                        <span style="display: block; text-align: center;">
                            Fin</br>
                            <small>${final.deviceDt.replace('T', ' ').split('.')[0]}</small>
                        </span>
                    `, {
                permanent: false,
                direction: 'right',
                offset: [0, 0],
                className: 'leaflet-tooltip'
            });
        // Agregar al array de marcadores
        markers.push(markerInit);
        markers.push(markerFinal);
        // Mostrar marcadores en el mapa
        markerInit.addTo(map);
        markerFinal.addTo(map);
    }

    // Crear el marcador inicial y final personalizado
    function createMarkersInitFinalCustom(init, final) {
        // Creando Marcador con tooltip permanente
        let markerInit = L.marker([init.latitude, init.longitude], { icon: iconInit })
            .bindTooltip(`
                        <span style="display: block; text-align: center;">
                            Inicio</br>
                            <small>${init.dateInitStr}</small>
                        </span>
                    `, {
                permanent: false,
                direction: 'left',
                offset: [0, 0],
                className: 'leaflet-tooltip'
            });

        let markerFinal = L.marker([final.latitude, final.longitude], { icon: iconFinal })
            .bindTooltip(`
                        <span style="display: block; text-align: center;">
                            Fin</br>
                            <small>${final.dateFinalStr}</small>
                        </span>
                    `, {
                permanent: false,
                direction: 'right',
                offset: [0, 0],
                className: 'leaflet-tooltip'
            });
        // Agregar al array de marcadores
        markers.push(markerInit);
        markers.push(markerFinal);
        // Mostrar marcadores en el mapa
        markerInit.addTo(map);
        markerFinal.addTo(map);
    }

    // Cargar los registros del reporte de demarcación
    function loadMarkingResults() {

        Swal.fire({
            icon: 'info',
            title: 'Espere por favor',
            text: 'Cargando información',
            allowOutsideClick: false
        });
        Swal.showLoading();

        $.ajax({
            url: "Marking/GetMarkingResults",
            data: filters,
            dataType: "json",
            type: "POST"
        }).done(function (results) {
            if (results.valido) {
                let markingsListado = [];
                let totales;
                if (results.markings.length > 0) {
                    markingsListado = results.markings;
                }
                if (results.totales != null) {
                    totales = results.totales;
                }
                if (markingsListado.length > 0) {
                    showReportMarkings(markingsListado, totales);
                }
            }
            else {
                Swal.fire('Warning', 'No se encontraron resultados para la búsqueda', 'warning');
            }
        }).fail(function (xhr, status, error) {
            Swal.fire('Error', error, 'error' + status);
            console.error(error);
        });
    }


    showReportMarkings = (markings, totales) => {
        // Limpiar mapa IMPORTANTE: Limpiar antes de vaciar polylinesMarking
        clearMap();
        // Vaciar array de polylines
        polylinesMarking.length = 0;
        // Limpiar tabla de tramas
        $('#tbody-tramas tr').remove();
        markings.forEach((x, index) => {
            let row = `
            <tr id="trama-row-${index}">
                <td class="trama-fecha-inicial">${x.initialDate}</td>
                <td class="trama-fecha-final">${x.finalDate}</td>
                <td class="trama-pintura-izquierda">${x.sumLeftPaintMeters}</td>
                <td class="trama-pintura-Centro">${x.sumCenterPaintMeters}</td>
                <td class="trama-pintura-Derecha">${x.sumRightPaintMeters}</td>
                <td class="trama-total-metros">${x.totalMeters}</td>
                <td class="trama-total-tiempo">${x.totalMinutes}</td>
                <td><input type="checkbox" id="trama-${index}" class="btn-trama" style="cursor: pointer;"/></td>
            </tr> `;
            $('#tbody-tramas').append(row);

            let datos = { InitialDate: x.initialDate, FinalDate: x.finalDate, TrackNumber: x.trackNumber }
            $.ajax({
                url: "Marking/GetMarkingByTrackNumber",
                data: datos,
                dataType: "json",
                type: "POST"
            }).done(function (results) {
                if (results.length > 0) {
                    showMarkingInMap(results);
                } else {
                    Swal.fire('Warning', 'No se encontraron resultados para la búsqueda', 'warning');
                }
            }).fail(function (xhr, status, error) {
                Swal.fire('Error', error, 'error' + status);
                console.error(error);
            });
        })

        let rowDescription = `
            <tr id="description">
                <td><i class="fa fa-calculator mr-2" aria-hidden="true"></i><strong>TOTALES</strong></td>
            </tr> `;
        $('#tbody-tramas').append(rowDescription);

        let rowTotales = `
            <tr id="trama-row-totales">
                <td class="total-trama-fecha-inicial">${totales.initialDateRoute}</td>
                <td class="total-trama-fecha-final">${totales.finalDateRoute}</td>
                <td class="total-trama-pintura-izquierda">${totales.totalLeftPaintMeters}</td>
                <td class="total-trama-pintura-Centro">${totales.totalCenterPaintMeters}</td>
                <td class="total-trama-pintura-Derecha">${totales.totalRightPaintMeters}</td>
                <td class="total-trama-total-metros">${totales.totalPaintMetersRoute}</td>
                <td class="total-trama-total-tiempo">${totales.totalMinutesRoute}</td>
            </tr> `;
        $('#tbody-tramas').append(rowTotales);

        Swal.close();
        $('#exampleModal').modal('hide');

        // Mostrar Fab buttons
        if ($('#fab-container').hasClass('d-none')) {
            $('#fab-container').removeClass('d-none');
        }

        // Mostrar Tramas Draggable
        if ($('#card-draggable').hasClass('d-none')) {
            $('#card-draggable').removeClass('d-none');
        }
        $('#btnExportarExcel').prop("disabled", false);
    }

    // Mostrar el reporte de demarcación
    showMarkingInMap = (markings) => {
        // Colors
        let colorIndex = 0;                    // gris             // naranja // violeta
        let colors = ['blue', 'red', 'green', '#636363', 'purple', '#fa8628', '#ff47af'];

        // Array de latitudes y longitudes
        let latLngs = [];
        latLngs.length = 0;
        // For each
        markings.forEach(async x => {
            latLngs.push([x.latitude, x.longitude]);
            // Guardar el tramo actual y limpiar todo para el nuevo tramo
            // Se almacena el polyline del tramo
            // Almacenar el último polyline
            // Cambiar de color
            colorIndex++;
            if (colorIndex > 6) colorIndex = 0;
        });
        polylinesMarking.push(L.polyline(latLngs, { color: colors[colorIndex], weight: 5 }));
        // Recorrer todos los polylines (tramas) creados para asignar eventos
        polylinesMarking.forEach((polyline, index) => {
            polyline.on('mouseover', function () {
                polyline.setStyle({ weight: 8 });
            });
            polyline.on('mouseout', function () {
                polyline.setStyle({ weight: 5 });
            });
            polyline.on('click', function () {
                // console.log({ polyline, dateInfo: datesInitAndFinal[index] });
                const equipmentId = $('#equipmentId').val();
                // Cargar el resumen para la trama seleccionada
                // loadSummaryQuery(equipmentId, datesInitAndFinal[index].dateInit, datesInitAndFinal[index].dateFinal);
            });
        });

        // Zoom the map to the polylines de demarcado
        var group = new L.featureGroup(polylinesMarking);
        map.fitBounds(group.getBounds());
    }

    // Agregar nueva fila a la tabla de tramas
    agregarTrama = async (init, final, index) => {
        const equipmentId = $('#equipmentId').val();
        const fechaInicial = init.deviceDt.replace('T', ' ').split('.')[0];
        const fechaFinal = final.deviceDt.replace('T', ' ').split('.')[0];

        // Obtener la promesa de resumen de la trama (total metros y promedio velocidad)
        summaryPromises.push(obtenerResumenTrama(equipmentId, fechaInicial, fechaFinal));

        let row = `
            < tr id = "trama-row-${index}" >
                <td class="trama-fecha-inicial">${fechaInicial}</td>
                <td class="trama-fecha-final">${fechaFinal}</td>
                <td class="trama-tiempo">Loading...</td>
                <td class="trama-total-metros">Loading...</td>
                <td class="trama-promedio-velocidad">Loading...</td>
                <td><input type="checkbox" id="trama-${index}" class="btn-trama" style="cursor: pointer;"/></td>
            </tr >
            `;
        $('#tbody-tramas').append(row);
    }

    // Obtener el total de metros y el promedio de velocidad
    obtenerResumenTrama = (equipmentId, dateInit, dateFinal) => {
        return $.ajax({
            url: "/Marking/GetSummaryQuery",
            data: {
                equipmentId,
                dateInitComplete: dateInit,
                dateFinalComplete: dateFinal
            },
            dataType: "json",
            type: "POST"
        });
    }
    // ***

    // Mostrar el resumen en el modal
    $('#btn-resumen').click(function (e) {
        const equipmentId = $('#equipmentId').val();
        const dateInit = $('#datetime-inicial').val();
        const dateFinal = $('#datetime-final').val();

        loadSummaryQuery(equipmentId, dateInit, dateFinal);
    });

    // Realizar la consulta para el resumen de demarcación
    loadSummaryQuery = (equipmentId, dateInit, dateFinal) => {
        Swal.fire({
            icon: 'info',
            title: 'Espere por favor',
            text: 'Cargando información',
            allowOutsideClick: false
        });
        Swal.showLoading();

        $.ajax({
            url: "/Marking/GetSummaryQuery",
            data: {
                equipmentId,
                dateInitComplete: dateInit,
                dateFinalComplete: dateFinal
            },
            dataType: "json",
            type: "POST"
        }).then(summary => {
            // Cerrar loading
            Swal.close();
            // Asignar los valores a los campos correspondientes del resumen
            Object.keys(summary).forEach(key => {
                const value = number_format(summary[key], 2);
                $(`#${key} `).html(value);
            });

            // Mostrar modal
            $('#modalSummary').modal('show');
        }).catch(error => {
            Swal.fire('Error', 'Internal Server Error', 'error');
            console.error(error);
        });
    }

    // Exportar Excel
    $('#btnExportarExcel').click(function () {
        let url = "/Marking/GenerateExcel";
        let data = [];
        let lista = [];
        let item;
        var tab = document.getElementById("TableMarking");
        let filas = tab.rows;
        for (var i = 1; i < filas.length - 2; i++) {// recorre las filas de la tabla
            for (var j = 0; j < filas[i].cells.length - 1; j++) {
                // atraviesa las columnas de cada fila
                item = filas[i].cells[j].innerHTML;
                data.push(item);
            }
            let objeto = new Reporte(data[0], data[1], data[2], data[3], data[4], data[5], data[6]);
            data.length = 0;
            lista.push(objeto);
        }
        let reporte = JSON.stringify(lista);
        $.ajax({
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            type: 'POST',
            url: url,
            data: reporte,
            success: function (data) {
                if (data.redirect) {
                    // we got a url for the pdf file
                    window.location.replace(data.redirect); // downloads without affecting DOM
                }
            },
            failure: function (response) {
                $('#result').html(response);
            }
        });
    });

    // Mostrar el card draggable de tramas
    $('#btn-tramas').click(function (e) {
        // Mostrar Tramas Draggables
        if ($('#card-draggable').hasClass('d-none')) {
            $('#card-draggable').removeClass('d-none');
        }
    });

    //Mostra la trama y los marcadores de inicio y final
    $('#tbody-tramas').on('change', '.btn-trama', function (e) {
        // Obtener datos para los marcadores de fechas
        const fechaInicial = $(this).closest('tr').find('.trama-fecha-inicial').html();
        const fechaFinal = $(this).closest('tr').find('.trama-fecha-final').html();

        // Obtener el index del botón que sería la posición del polyline
        const index = $(this).attr('id').split('-')[1];

        // Obtener si la trama es seleccionada o no
        const checked = $(this).prop('checked');

        // Obtener latitudes y longitudes para los marcadores
        const latlngInit = polylinesMarking[index]._latlngs[0];
        const latlngFinal = polylinesMarking[index]._latlngs[polylinesMarking[index]._latlngs.length - 1];

        if (checked) {
            // Mostrar el polyline
            polylinesMarking[index].addTo(map);
            const init = {
                latitude: latlngInit.lat,
                longitude: latlngInit.lng,
                dateInitStr: fechaInicial
            };
            const final = {
                latitude: latlngFinal.lat,
                longitude: latlngFinal.lng,
                dateFinalStr: fechaFinal
            };
            // Crear los marcadores inicial y final para la trama
            createMarkersInitFinalCustom(init, final);
        } else {
            // Remover la trama del mapa
            polylinesMarking[index].removeFrom(map);
            // Remover el marcador inicial y final de la trama
            markers.forEach(marker => {
                const { lat, lng } = marker._latlng;
                if (latlngInit.lat == lat && latlngInit.lng == lng || latlngFinal.lat == lat && latlngFinal.lng == lng) {
                    map.removeLayer(marker);
                }
            });
        }
    });



    // Seleccionar todas las tramas
    $('#checkAll').click(function (e) {
        const checked = $(this).prop('checked');
        $('.btn-trama').each(function (e) {
            $(this).prop('checked', checked);
            $(this).trigger('change');
        });
    });

    function number_format(val, decimals) {
        //Parse the value as a float value
        val = parseFloat(val);
        //Format the value w/ the specified number
        //of decimal places and return it.
        return val.toFixed(decimals);
    }

    // Método encargado de retornar la distancia por latitud y longitud
    function distance(lat1, lon1, lat2, lon2, unit) {
        if ((lat1 == lat2) && (lon1 == lon2)) {
            return 0;
        }
        else {
            var radlat1 = Math.PI * lat1 / 180;
            var radlat2 = Math.PI * lat2 / 180;
            var theta = lon1 - lon2;
            var radtheta = Math.PI * theta / 180;
            var dist = Math.sin(radlat1) * Math.sin(radlat2) + Math.cos(radlat1) * Math.cos(radlat2) * Math.cos(radtheta);
            if (dist > 1) {
                dist = 1;
            }
            dist = Math.acos(dist);
            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515;
            if (unit == "K") { dist = dist * 1.609344 }
            if (unit == "N") { dist = dist * 0.8684 }
            return dist;
        }
    }
});

