$(document).ready(function (e) {
    // Lanzar modal inicial cuando no hay registros de transmisión todavía y el alert de consulta demasiado grande no haya sido lanzado
    if ($('#totalResults').val() == 0 && $('#queryLimitAlert').length == 0) {
        $('#exampleModal').modal('show');
    }

    function number_format(val, decimals) {
        //Parse the value as a float value
        val = parseFloat(val);
        //Format the value w/ the specified number
        //of decimal places and return it.
        return val.toFixed(decimals);
    }

    // Filtrar los registros
    $('#btn-filtrar').click(function () {

        Swal.fire({
            icon: 'info',
            title: 'Espere por favor',
            text: 'Cargando información',
            allowOutsideClick: false
        });
        Swal.showLoading();

        // Obtener ambas fechas
        var dateInit= $('#dateInit').val();
        var dateFinal = $('#dateFinal').val();

        if (dateInit == '' || dateFinal == '') {
            Swal.fire('Error', 'Es necesario completar todos los campos para aplicar el filtro por fecha', 'error');
            return;
        } else {
            let dateInit_format = new Date(dateInit);
            let dateFinal_format = new Date(dateFinal);
            if (dateInit_format > dateFinal_format) {
                Swal.fire('Error', 'La fecha inicial no debe ser mayor a la final', 'error');
                return;
            } else {
                const diffTime = dateFinal_format.getTime() - dateInit_format.getTime();
                const diffDays = diffTime / (1000 * 3600 * 24);
                if (diffDays > 10) {
                    Swal.fire('Error', 'El rango de fechas es demasiado alto (max: 10 días)', 'error');
                    return;
                }
            }
        }

        // Si las fechas son válidas
        $('#formFilter').submit();
    });

    // Descarga el archivo excel
    $('#btn-excel').click(function () {
        // Copiar los filtros
        $('#equipmentId_xls').val($('#equipmentId').val());
        $('#dateInit_xls').val($('#dateInit').val());
        $('#dateFinal_xls').val($('#dateFinal').val());
        $('#subprojectId_xls').val($('#subprojectId').val());
        $('#speedReportId_xls').val($('#speedReportId').val());

        $('#form-export-xls').submit();
    });

    // Mostrar Chart
    $('#nav-profile-tab').click(function (e) {
        loadDataByHours();
    });

    function loadDataByHours() {
        $.ajax({
            url: "/RadarSpeeds/GetDataStationarySpeedRadarByHours",
            data: {
                equipmentId: $('#equipmentId').val(),
                vehicleTypeId: $('#vehicleTypeId').val(),
                dateInit: $('#dateInit').val(),
                dateFinal: $('#dateFinal').val()
            },
            type: "get"
        }).then(results => {
            if (!results) {
                Swal.fire('Error', 'Ha ocurrido un error obteniendo los datos', 'error');
                return;
            }
            const canvas_selector = `canvas-radar-average-speeds-hours`;
            // Inicializar la gráfica con los resultados obtenidos
            initChartAverage(results, canvas_selector);
        }).catch((err) => {
            console.log(err);
            Swal.fire('Error', 'Ha ocurrido un error obteniendo los datos', 'error');
        });
    }

    var backgroundColors = [
        'rgba(255, 99, 132, 0.2)',
        'rgba(54, 162, 235, 0.2)',
        'rgba(255, 206, 86, 0.2)',
        'rgba(75, 192, 192, 0.2)',
        'rgba(153, 102, 255, 0.2)',
        'rgba(255, 159, 64, 0.2)'
    ];
    var borderColors = [
        'rgba(255, 99, 132, 1)',
        'rgba(54, 162, 235, 1)',
        'rgba(255, 206, 86, 1)',
        'rgba(75, 192, 192, 1)',
        'rgba(153, 102, 255, 1)',
        'rgba(255, 159, 64, 1)'
    ];

    function initChartAverage(results, canvas_selector) {
        let dynamicBackgroundColors = [];
        let dynamicBorderColors = [];
        let data = [];
        let labels = [];
        let iterator = 0;
        results.forEach(x => {
            let average = number_format(x.averageSpeed, 1);
            labels.push(x.hour);
            data.push(average);
            // background and color
            dynamicBackgroundColors.push(backgroundColors[iterator]);
            dynamicBorderColors.push(borderColors[iterator]);
            iterator += 1;
            if (iterator > 5) iterator = 0;
        });
        //original canvas
        let ctx = document.getElementById(canvas_selector).getContext('2d');
        let myChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels,
                datasets: [{
                    label: 'Promedio',
                    data,
                    backgroundColor: dynamicBackgroundColors,
                    borderColor: dynamicBorderColors,
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                legend: {
                    display: false
                },
                tooltips: {
                    callbacks: {
                        afterBody: function (t, d) {
                            let item = results[t[0].index];
                            return `Cantidad: ${ item.totalResults }`; //return a string that you wish to append
                        }
                    }
                },
                scales: {
                    yAxes: [{
                        display: true,
                        scaleLabel: {
                            display: true,
                            labelString: 'Velocidad (Km/h)',
                            fontSize: 17
                        },
                        ticks: {
                            beginAtZero: true
                        }
                    }],
                    xAxes: [{
                        display: true,
                        scaleLabel: {
                            display: true,
                            labelString: 'Horario',
                            fontSize: 17
                        }
                    }]
                },
                // PARA MOSTRAR EL VALOR DE LA BARRA SIN TENER QUE PASAR LE MOUSE
                animation: {
                    duration: 0.5,
                    onComplete: function () {
                        var chartInstance = this.chart,
                            ctx = chartInstance.ctx;

                        ctx.font = Chart.helpers.fontString(Chart.defaults.global.defaultFontSize, Chart.defaults.global.defaultFontStyle, Chart.defaults.global.defaultFontFamily);
                        ctx.textAlign = 'center';
                        ctx.textBaseline = 'bottom';

                        this.data.datasets.forEach(function (dataset, i) {
                            var meta = chartInstance.controller.getDatasetMeta(i);
                            meta.data.forEach(function (bar, index) {
                                var data = dataset.data[index];
                                // PONER CONSOLE LOGS PARA ENTENDER EL FUNCIONAMIENTO
                                if (data !== 0) {
                                    ctx.fillText(data, bar._model.x, bar._model.y - 5);
                                }
                            });
                        });
                    }
                },
            }
        });
    }
});