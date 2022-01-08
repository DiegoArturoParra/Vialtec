$(document).ready(function (e) {
    // Lanzar modal inicial cuando no hay registros de transmisión todavía
    if ($('#totalResults').val() == 0 && $('#customerModelEventIds').val() == '') {
        $('#exampleModal').modal('show');
    }

    // INICIALIZAR MAPA
    var map = L.map("map").setView([4.7109745, -74.2121788], 7);
    var marker;

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    // show the scale bar on the lower left corner
    L.control.scale().addTo(map);
    //_------------------------------------------------------------------------------------

    // Click para empezar a mostrar la transmisión en un mini-mapa
    $('.btn-map').click(function (e) {
        const transmissionID = $(this).attr('id');
        const eventAlias = $(this).closest('tr').find('input[type=hidden]').val();
        $.ajax({
            url: "/TransmissionsInfo/GetTransmissionById",
            data: {
                transmissionID,
                eventAlias
            },
            type: "get"
        }).done(function (transmission) {
            showMapTransmission(transmission);
        }).fail(console.error);
    });

    // Mostrar transmisión en un mini-mapa
    showMapTransmission = (transmission) => {
        const { eventAlias, deviceDt, latitude, longitude } = transmission;
        const { equipmentAlias } = transmission.equipment;
        if (marker) {
            map.removeLayer(marker);
        }
        map.setView([latitude, longitude], 15);
        // Crear marcador
        console.log(transmission);
        marker = L.marker([latitude, longitude])
                    .bindTooltip(`
                                <span style="display: block; text-align: center;">
                                    ${ equipmentAlias }</br>
                                    ${ eventAlias }</br>
                                    <small>${deviceDt.replace('T', ' ').split('.')[0]}</small>
                                </span>
                            `, {
                            permanent: true,
                            direction: 'top',
                            offset: [0, 0],
                            className: 'leaflet-tooltip'
                        });
        // Agregar marcador
        marker.addTo(map);

        // Mostrar modal contenedor del map
        $('#mapModal').modal('show');
    }
    // --------------------------------------------------------------------------

    // Solución al problema de Leaflet y Modals de bootstrap (mapa gris)
    $('#mapModal').on('show.bs.modal', function () {
        setTimeout(function () {
            map.invalidateSize();
        }, 200);
    });

    // Filtrar las transmisiones
    $('#btn-filtrar').click(function (e) {
        // obtener Date y Time: init y final
        var datetime_inicial = $('#datetime-inicial').val();
        var datetime_final = $('#datetime-final').val();

        if (datetime_inicial == '' || datetime_final == '') {
            Swal.fire('Error', 'Es necesario completar todos los campos para aplicar el filtro por fecha y hora', 'error');
            return;
        } else {
            let datetime_inicial_format = Date.parse(datetime_inicial);
            let datetime_final_format = Date.parse(datetime_final);
            if (datetime_inicial_format > datetime_final_format) {
                Swal.fire('Error', 'La fecha inicial no debe ser mayor a la final', 'error');
                return;
            }
        }
        const customerModelEventIdsStr = getCustomerModelEventIdsStr();
        if (customerModelEventIdsStr == '') {
            Swal.fire('Error', 'No se ha seleccionado ningún evento', 'error');
            return;
        } else {
            $('#customerModelEventIds').val(customerModelEventIdsStr);
            $('#formFilter').submit();
        }
    });

    // Obtener un string con el formato 3#4#5... con los ids de los customerModelEvents que se quiren consultar
    getCustomerModelEventIdsStr = () => {
        var checkEventsGroups = $('#check-events .input-group');
        if (checkEventsGroups.find('input[type=checkbox]').length === 0) {
            return '';
        }
        let customerModelEventIds = [];
        checkEventsGroups.each(function (e) {
            if ($(this).find('input[type=checkbox]')[0].checked) {
                customerModelEventIds.push($(this).find('input[type=text]').attr('id'));
            }
        });
        return customerModelEventIds.join('#');
    }

    // --------------------------------------------------------------------------------
    // Evento change del select de equipmentGroups
    $('#equipmentGroupId').change(function (e) {
        getEquipmentsByEquipmentGroupId($(this).val());
    });
    // Obtener los equipments por equipmentGroupId
    getEquipmentsByEquipmentGroupId = (equipmentGroupId) => {
        $.ajax({
            url: "/TransmissionsInfo/GetEquipmentsByEquipmentGroupId",
            data: { equipmentGroupId },
            type: "get"
        }).done(function (results) {
            $('#equipmentId option').remove();
            if (results.length == 0) {
                const emptyOption = `<option value="-1">Sin registros</option>`;
                $('#equipmentId').append(emptyOption);
                getCustomerModelEventByEquipmentId(-1);
            } else {
                results.forEach(item => {
                    const option = `<option value="${item.id}">${item.equipmentAlias}</option>`;
                    $('#equipmentId').append(option);
                });
                // Obtener los eventos por el primer equipment traido de la petición ajax
                getCustomerModelEventByEquipmentId(results[0].id);
            }
        }).fail(console.log);
    }
    getEquipmentsByEquipmentGroupId($('#equipmentGroupId').val());
    // ---------------------------------------------------------------------------------

    // ---------------------------------------------------------------------------------
    $('#equipmentId').change(function (e) {
        getCustomerModelEventByEquipmentId($(this).val());
    });
    // Obtener los customer model events por equipment
    getCustomerModelEventByEquipmentId = (equipmentId) => {
        $.ajax({
            url: "/TransmissionsInfo/GetCustomerModelEventByEquipmentId",
            data: { equipmentId },
            type: "get"
        }).done(function (results) {
            $('#check-events .input-group').remove();
            if (results.length == 0) {
                const emptyCheck = `
                            <div class="input-group input-group-sm">
                              <div class="input-group-prepend">
                                <span class="input-group-text" id="inputGroup-sizing-sm"></span>
                              </div>
                              <input type="text" class="form-control disabled" value="No hay eventos" >
                            </div>
                        `;
                $('#check-events').append(emptyCheck);
            } else {
                results.forEach(item => {
                    const checkEvent = `
                                <div class="input-group input-group-sm mb-1">
                                  <div class="input-group-prepend">
                                    <div class="input-group-text">
                                      <input type="checkbox" checked>
                                    </div>
                                  </div>
                                  <input type="text" class="form-control" readonly
                                    id="${ item.id }" value="${ item.title }">
                                </div>
                            `;
                    $('#check-events').append(checkEvent);
                });

                // Solo cuando se recarga la página
                if ($('#customerModelEventIds').val() != '') {
                    const customerModelEventsList = $('#customerModelEventIds').val().split('#');
                    $('#check-events .input-group').each(function (e) {
                        const id = $(this).find('input[type=text]').attr('id');
                        if (!customerModelEventsList.includes(id)) {
                            $(this).find('input[type=checkbox]')[0].checked = false;
                        }
                    });
                    $('#customerModelEventIds').val('');
                }
            }
        }).fail(console.log);
    }
    // ---------------------------------------------------------------------------------
});