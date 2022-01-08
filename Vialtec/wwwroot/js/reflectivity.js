$(document).ready(function (e) {

    // MAPA DEFINCIÓN ----------------------------------------------------------------------------------------------
    var map = L.map('map').setView([4.7109745, -74.2121788], 6); // ([lat, lng], zoom)
    var markersCluster;
    var mapMarkersSimples = [];
    var groupMarkersToFitBounds = [];

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    // show the scale bar on the lower left corner
    L.control.scale().addTo(map);

    // -------------------------------------------------------------------------------------------------------
    //Generar checkbox para reporte PDF
    var lineNumbersSelected = [];
    for (let i = 1; i <= 20; i++) {
        let chkLineNumber = `
            <div class="form-check">
                <input type="checkbox" class="form-check-input" id="check-${i}" style="cursor: pointer;">
                <b class="text-muted"><label class="form-check-label" for="check-${i}" style="cursor: pointer;">${i}</label></b>
            </div>
        `;
        let columnNumberSelector = '';
        if (i <= 5) {
            columnNumberSelector = '#column-1';
        } else if (i > 5 && i <= 10) {
            columnNumberSelector = '#column-2';
        } else if (i > 10 && i <= 15) {
            columnNumberSelector = '#column-3';
        } else if (i > 15 && i <= 20) {
            columnNumberSelector = '#column-4';
        }
        $(columnNumberSelector).append(chkLineNumber);
    }

    // FILTROS -----------------------------------------------------------------------------------------------
    var filters; // global filters
    // aquí se almacenarán los id´s de las actividades seleccionadas
    var activitiesSelected = [];
    showActivitiesSelected();

    // Actividades del proyecto inicial del select
    getActivities($('#projectId').val());
    // onchange projects select, obtener las actividades por proyecto
    $('#projectId').change(function (e) {
        getActivities($(this).val());
    });

    // type=number validations
    $('input[type=number]').keypress(function (e) {
        if (isNaN(e.key)) { e.preventDefault(); }
        if ($(this).val().length === 5) {
            e.preventDefault();
        }
    });

    // click agregar actividad al filtro
    $('#table-activities').on('click', '.add-activity', function (e) {
        let id = $(this).attr('id').split('-')[1];
        let title = $(this).closest('tr').find('.title').val();
        if (!existsActivity(id)) {
            activitiesSelected.push({ id, title });
            // refresh actividades
            getActivities($('#projectId').val());
            // refresh seleccionadas
            showActivitiesSelected();
        }
    });

    // click eliminar actividad del filtro
    $('#table-seleccion').on('click', '.delete-activity', function (e) {
        let id = $(this).attr('id').split('-')[1];
        activitiesSelected = activitiesSelected.filter(x => x.id != id);
        //// refresh actividades
        getActivities($('#projectId').val());
        //// refresh seleccionadas
        showActivitiesSelected();
    });

    // Validar y posteriormente filtrar
    $('#btn-filtrar').click(function (e) {

        if (activitiesSelected.length === 0) {
            showMessageError('No has seleccionado ninguna actividad');
            return;
        }

        var geometryId = getValueFilter('#geometryId');
        var lineColorId = getValueFilter('#lineColorId');
        var lineNumber = getValueFilter('#lineNumber');
        var dateInit = getValueFilter('#dateInit');
        var dateFinal = getValueFilter('#dateFinal');
        var minimo = getValueFilter('#minimo');
        var tolerancia = getValueFilter('#tolerancia');

        // obtener valores pr init y final para el filtro
        var pr_init1 = getValueFilter('#pr_init1');
        var pr_final1 = getValueFilter('#pr_final1');
        var pr_init2 = getValueFilter('#pr_init2');
        var pr_final2 = getValueFilter('#pr_final2');
        var pr_init = null;
        var pr_final = null;

        // Validaciones
        var valid = true;
        // 1. Fechas
        if (dateInit !== null) {
            if (dateInit === '' || dateFinal === '') {
                showMessageError('La fecha inicial y la fecha final deben ser definidas');
                valid = false;
            } else  {
                var init = new Date(dateInit);
                var final = new Date(dateFinal);
                if (init.getTime() > final.getTime()) {
                    showMessageError('La fecha inicial no puede ser mayor a la final');
                    valid = false;
                }
            }
        }
        // 2. Minimo/Tolerancia
        if (valid == true) {
            if (minimo !== null) {
                if (tolerancia === '') {
                    $('#tolerancia').val('0');
                    tolerancia = '0';
                }
                if (minimo === '') {
                    showMessageError('Es necesario definir un mínimo');
                    valid = false;
                } else {
                    if (Number(minimo) < 0 || Number(tolerancia) < 0) {
                        showMessageError('Mínimo y tolerancia no pueden ser negativos');
                        valid = false;
                    }
                }
            }
        }
        // 3. PR init y final
        if (valid == true) {
            if (pr_init1 !== null) {
                if (pr_init1 === '' || pr_final1 === '' || pr_init2 === '' || pr_final2 === '') {
                    showMessageError('Debes completar todos los campos PR para aplicar el filtro');
                    valid = false;
                } else {
                    if (Number(pr_init1) < 0 || Number(pr_final1) < 0 || Number(pr_init2) < 0 || Number(pr_final2) < 0) {
                        showMessageError('Los campos PR no pueden ser negativos');
                        valid = false;
                    } else {
                        // valid PR
                        pr_init = `${pr_init1}+${pr_final1}`;
                        pr_final = `${pr_init2}+${pr_final2}`;
                    }
                }
            }
        }

        // Si valid es false es porque ocurrió un error en el filtrado
        if (valid == true) {
            filters = {
                activities: JSON.stringify(activitiesSelected),
                geometryId,
                lineColorId,
                lineNumber,
                dateInit,
                dateFinal,
                //minimo,
                //tolerancia,
                pr_init,
                pr_final
            };
            // aplicar los filtros
            applyFilters();
        }
    });

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
            url: "Reflectivities/GetReflectivities",
            data: filters,
            type: "get"
        }).done(function (results) {
            // console.log(results);
            //Swal.close();
            //$('#exampleModal').modal('hide');
            if (results.length !== 0) {
                // eliminar clase si la tiene aún
                if ($('#fab-container').hasClass('d-none')) {
                    $('#fab-container').removeClass('d-none');
                }

                // solo copiar filtros a forms para exportar (Kml, Excel y PDF) cuando los resultados no esten vacíos
                copyFiltersForms('kml');
                copyFiltersForms('xls');
                copyFiltersForms('pdf');

                // Mostrar los marcadores en el mapa
                showMarkers(results);
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

    // Mostrar cualquier mensaje de error
    function showMessageError(text) {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text
        });
    }

    // función para extraer el valor el input (dinamicamente) y verificar el checkbox
    function getValueFilter(selector_id) {
        var input = $(selector_id);
        var checkbox = input.closest('.form-group').find('input[type=checkbox]')[0];
        return checkbox.checked ? input.val() : null;
    }

    // función para recuperar las actividades del projectId
    function getActivities(projectId) {
        $.ajax({
            url: "Reflectivities/GetActivitiesByProjectId",
            data: {
                projectId
            },
            type: "get"
        }).done(function (activities) {
            // limpiar actividades actuales
            $('#table-activities tr').remove();
            if (activities.length === 0) {
                let emptyMessage = `
                     <tr class="text-center">
                        <td colspan="2">No se encontraron actividades</td>
                    </tr>
                `;
                $('#table-activities').append(emptyMessage);
            }
            // Mostrar las actividades
            activities.forEach(x => {
                let activityHTML = '';
                if (!existsActivity(x.id)) {
                    activityHTML = `
                        <tr class="text-center">
                            <input type="hidden" value="${x.title}" class="title" />
                            <td>${x.title}</td>
                            <td>
                                <span class="badge badge-info add-activity" style="cursor: pointer;" id="activity-${x.id}">
                                    <i class="fas fa-plus"></i>
                                </span>
                            </td>
                        </tr>
                    `;
                } else {
                    activityHTML = `
                        <tr class="text-center">
                            <td>${x.title}</td>
                            <td>
                                <span class="badge badge-success" style="cursor: default;">
                                    <i class="fas fa-check"></i>
                                </span>
                            </td>
                        </tr>
                    `;
                }
                $('#table-activities').append(activityHTML);
            });
        }).fail(function (xhr, status, error) {
            console.error(error);
        });
    }

    // Mostrar las actividades que han sido seleccionadas
    function showActivitiesSelected() {
        $('#table-seleccion tr').remove();
        if (activitiesSelected.length == 0) {
            let emptyMessage = `
                     <tr class="text-center">
                        <td colspan="2">Ninguna</td>
                    </tr>
                `;
            $('#table-seleccion').append(emptyMessage);
        }
        activitiesSelected.forEach(x => {
            let activityHTML = `
                <tr class="text-center">
                    <td>${x.title}</td>
                    <td>
                        <span class="badge badge-danger delete-activity" style="cursor: pointer;" id="delete-${x.id}">
                            <i class="fas fa-trash"></i>
                        </span>
                    </td>
                </tr>
            `;
            $('#table-seleccion').append(activityHTML);
        });
    }

    // Verificar si el ID está en las actividades seleccionadas
    function existsActivity(activityId) {
        var item = activitiesSelected.find(x => x.id == activityId);
        return item !== undefined;
    }

    // Map Functions -----------------------------------------------------------------------------------------------
    // Mostrar los marcadores que se obtuvieron de la consulta por filtros
    function showMarkers(results) {

        // Vaciar marcadores sin agrupar (no agrupamiento)
        clearSimpleMarkers();

        // Eliminar anterior Layer de markers (agrupamiento)
        if (markersCluster) {
            map.removeLayer(markersCluster);
        }

        // Vaciar fitBounds markers array
        groupMarkersToFitBounds = [];

        // determinar si agrupar o no los resultados
        var clusterEnabled = $('#agruparCheck')[0].checked;
        // determinar si mostrar valores o no
        var showValueEnabled = $('#mostrarValoresCheck')[0].checked;

        // Cluster Markers
        markersCluster = new L.MarkerClusterGroup();

        // minimo y tolerancia
        let minimo = getValueFilter('#minimo');
        let tolerancia = getValueFilter('#tolerancia');

        // Recorrer items results para mostrar los Modals con los detalles del item
        results.forEach(x => {
            // iconUrl
            let iconUrl = '/img/map-icons/';
            if (minimo == null) { // no se definió minimo/tolerancia
                iconUrl += x.hasPicture ? 'camera-blue.png' : 'hor-azul.png';
            } else {
                iconUrl += getIconByMinimoTolerancia(Number(minimo), Number(tolerancia), Number(x.measurement), x.hasPicture);
            }
            // obtener lat y lng
            var lat = Number(x.latitude);
            var lng = Number(x.longitude);

            // Icono
            var iconCar = L.icon({
                iconUrl,
                iconSize: x.hasPicture ? [32, 25] : [35, 30] // [width, height] size of the icon
            });

            // Creando Marcador
            var marker = L.marker([lat, lng], { icon: iconCar })
                .bindTooltip(x.measurement.toString(), {
                    permanent: (showValueEnabled === true), // mostrar valores ? permanent: true or false
                    direction: 'top',
                    offset: [0, -10],
                    className: 'leaflet-tooltip'
                });
            // clikc event para los marcadores
            marker.on('click', function (e) {
                // Loading
                Swal.fire({
                    icon: 'info',
                    title: 'Espere por favor',
                    text: 'Cargando información',
                    allowOutsideClick: false
                });
                Swal.showLoading();

                $.ajax({
                    url: "Reflectivities/GetReflecitivityById",
                    data: {
                        reflectivityId: x.id
                    },
                    type: "get"
                }).done(function (result) {
                    Swal.close(); // cerrar loading
                    var pictureHTML = '';
                    if (result.picture !== null) {
                        pictureHTML = `
                        <tr>
                            <td colspan="2" class="text-center">
                                <img alt="img-picture" width="250" height="300" src="data:image/jpg;base64, ${result.picture}" />
                            </td>
                        </tr>
                    `;
                    }
                    Swal.fire({
                        //title: 'Detalles de la medición',
                        showConfirmButton: false,
                        html: `
                        <b class="text-muted text-center d-block mb-1">DETALLES DE MEDICIÓN</b>
                        <table class="table table-sm table-bordered">
                            <tbody>
                                <tr>
                                    <td><b class="text-muted">Reflectividad</b></td>
                                    <td>${result.measurement}</td>
                                </tr>
                                <tr>
                                    <td><b class="text-muted">Geometría</b></td>
                                    <td>${result.geometry}</td>
                                </tr>
                                <tr>
                                    <td><b class="text-muted">Línea</b></td>
                                    <td>${result.lineNumber}</td>
                                </tr>
                                <tr>
                                    <td><b class="text-muted">Color</b></td>
                                    <td>${result.lineColor}</td>
                                </tr>
                                <tr>
                                    <td><b class="text-muted">PR</b></td>
                                    <td>${result.prStr}</td>
                                </tr>
                                <tr>
                                    <td><b class="text-muted">Modelo Equipo</b></td>
                                    <td>${result.model}</td>
                                </tr>
                                <tr>
                                    <td><b class="text-muted">Serial Equipo</b></td>
                                    <td>${result.deviceSerial}</td>
                                </tr>
                                <tr>
                                    <td><b class="text-muted">Fecha Medición</b></td>
                                    <td>${result.deviceDt.replace('T', ' ').split('.')[0]}</td>
                                </tr>
                                <tr>
                                    <td><b class="text-muted">Fecha Transmisión</b></td>
                                    <td>${result.servetDt.replace('T', ' ').split('.')[0]}</td>
                                </tr>
                                <tr>
                                    <td><b class="text-muted">Latitud</b></td>
                                    <td>${result.latitude}</td>
                                </tr>
                                <tr>
                                    <td><b class="text-muted">Longitud</b></td>
                                    <td>${result.longitude}</td>
                                </tr>
                                ${ pictureHTML}
                            </tbody>
                        </table>
                        <div style="position: absolute; top: 5px; right: 7px;">
                            <span class="text-muted" style="cursor: pointer;" onclick="Swal.close()">
                                <i class="fas fa-times"></i>
                            </span>
                        </div>
                        `,
                        showConfirmButton: true,
                        confirmButtonText: 'Eliminar',
                        confirmButtonColor: '#d33'
                    }).then(resp => {
                        if (resp.isConfirmed) {
                            Swal.fire({
                                title: '¿Estás seguro?',
                                text: "No podrás revertir los cambios",
                                icon: 'warning',
                                showCancelButton: true,
                                confirmButtonColor: '#d33',
                                cancelButtonColor: '#3085d6',
                                confirmButtonText: 'Eliminar',
                                cancelButtonText: 'Cancelar'
                            }).then((resp2) => {
                                if (resp2.isConfirmed) {
                                    $.ajax({
                                        url: "/Reflectivities/DeleteReflectivity",
                                        data: { id: result.id },
                                        type: "delete"
                                    }).done(deleted => {
                                        if (deleted === true) {
                                            // Refrescar marcadores
                                            applyFilters();
                                        } else {
                                            alert('Ha ocurrido un error');
                                        }
                                    });
                                }
                            });
                        }
                    });

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

        // fitBounds para centrar el mapa donde el grupo de markers esté
        var group = new L.featureGroup(groupMarkersToFitBounds);
        map.fitBounds(group.getBounds());

        // Reestablecer View y Zoom normal
        // map.setView([4.7109745, -74.2121788], 6); -> con el fitBounds ya no es necesario

        // Cerrar loading y modal
        Swal.close();
        $('#exampleModal').modal('hide');
    }

    // Limpiar los marcadores del mapa
    function clearSimpleMarkers() {
        for (var i = 0; i < mapMarkersSimples.length; i++) {
            map.removeLayer(mapMarkersSimples[i]);
        }
        mapMarkersSimples = [];
    }

    // Obtener el icono por el minimo de tolerancia
    function getIconByMinimoTolerancia(minimo, tolerancia, measurement, hasPicture) {
        if (measurement < (minimo - tolerancia)) {
            return hasPicture ? 'camera-red.png' : 'hor-rojo.png';
        } else if (measurement >= (minimo - tolerancia) && measurement < minimo) {
            return hasPicture ? 'camera-yellow.png' : 'hor-amarillo.png';
        } else if (measurement >= minimo) {
            return hasPicture ? 'camera-green.png' : 'hor-verde.png';
        }
    }

    // Copiar los filtros en los forms para los reportes
    function copyFiltersForms(type) {
        var str_ids = '';
        activitiesSelected.forEach(x => {
            str_ids += x.id + ',';
        });
        $('#activities_' + type).val(str_ids);

        $('#geometryId_' + type).val(filters.geometryId);
        $('#lineColorId_' + type).val(filters.lineColorId);
        $('#lineNumber_' + type).val(filters.lineNumber);
        $('#dateInit_' + type).val(filters.dateInit);
        $('#dateFinal_' + type).val(filters.dateFinal);
        $('#pr_init_' + type).val(filters.pr_init);
        $('#pr_final_' + type).val(filters.pr_final);
        //$('#minimo_' + type).val(filters.minimo);
        //$('#tolerancia_' + type).val(filters.tolerancia);
    }

    // Reportes KML, XLS, PDF --------------------------------------------------------------
    $('#btn-kml').click(function (e) {
        $('#form-export-kml').submit();
    });
    $('#btn-xls').click(async function (e) {
        const { dismiss, isConfirmed } = await mensajeConfirmacion('¿Desea incluir las imágenes en el reporte?');
        
        if (dismiss !== 'backdrop') {
            $('#include_image_xls').val(isConfirmed);
            $('#form-export-xls').submit();
        }
    });
    $('#btn-pdf').click(function (e) {
        $('#modalReportPDF').modal('show');
    });
    $('#btn-report-pdf').click(function (e) {
        // obtener los checkbox seleccionados para las líneas
        $('#report-checkbox input[type=checkbox]').each(function (e) {
            if ($(this).prop('checked') == true) {
                let lineNumber = $(this).attr('id').split('-')[1];
                lineNumbersSelected.push(lineNumber);
            }
        });
        if (lineNumbersSelected.length == 0) {
            Swal.fire({ icon: 'error', text: 'Es necesario seleccionar al menos una línea' });
        } else {
            let lineNumbersIds = lineNumbersSelected.join(',');
            $('#lineNumbers_pdf').val(lineNumbersIds);
            $('#form-report-pdf').submit();
        }
    });

    // Mensaje de confirmación
    mensajeConfirmacion = (text) => {
        return Swal.fire({
            text,
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Si',
            cancelButtonText: 'No'
        });
    }
});