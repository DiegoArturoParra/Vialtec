$(document).ready(function (e) {

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

    // Filtrar
    $('#btn-filter').click(function (e) {
        const dateInit = $('#dateInit').val();
        const dateFinal = $('#dateFinal').val();

        if (!dateInit || !dateFinal) {
            Swal.fire('Error', 'El rango de fechas no ha sido definido', 'error');
            return;
        }

        const dateInit_format = new Date(dateInit);
        const dateFinal_format = new Date(dateFinal);
        console.log({ dateInit_format, dateFinal_format });
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

        $('#form-filter').submit();
    });

    var results;

    // Click Tab Chart
    $('#nav-profile-tab').click(async function (e) {
        const dateInit = $('#dateInit').val();
        const dateFinal = $('#dateFinal').val();
        const equipmentId = $('#equipmentId').val();
        $('#loading-chart').removeClass('d-none');

        try {
            results = await getDataSpeedStatReportByHours(equipmentId, dateInit, dateFinal);
            // Mostrar los canvas
            showChartsAverages();
        } catch (err) {
            $('#loading-chart').addClass('d-none');
            $('#nav-home-tab').click();
            Swal.fire('Error', 'Ha ocurrido un error', 'error');
        }
    });

    // Get Data Speed Stat Report By Hours
    getDataSpeedStatReportByHours = (equipmentId, dateInit, dateFinal) => {
        return $.ajax({
            url: '/SpeedStatReports/GetDataSpeedStatReportByHours',
            data: { equipmentId, dateInit, dateFinal },
            type: 'GET'
        });
    }

    var dynamicBackgroundColors;
    var dynamicBorderColors;
    var labels;

    // Show Charts
    function showChartsAverages() {
        // Background y Colors
        dynamicBackgroundColors = [];
        dynamicBorderColors = [];
        // Labels
        labels = [];
        // Data
        let data_avg_speed = [];
        let data_last_speed = [];
        let data_peak_speed = [];

        let iterator = 0;

        results.forEach(x => {
            // Labels
            labels.push(x.hour);

            // Data
            data_avg_speed.push( number_format(x.averageAvgSpeed, 1) );
            data_last_speed.push( number_format(x.averageLastSpeed, 1) );
            data_peak_speed.push( number_format(x.averagePeakSpeed, 1) );

            // Background and Color
            dynamicBackgroundColors.push(backgroundColors[iterator]);
            dynamicBorderColors.push(borderColors[iterator]);

            iterator += 1;
            if (iterator > 5) iterator = 0;
        });

        // Configure Canvas
        configureCanvas('canvas-average-avg-speeds-hours', data_avg_speed);
        configureCanvas('canvas-average-last-speeds-hours', data_last_speed);
        configureCanvas('canvas-average-peak-speeds-hours', data_peak_speed);

        $('#loading-chart').addClass('d-none');
    }

    // Configure Canvas
    configureCanvas = (canvas_selector, data) => {
        // Canvas
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
                            return `Cantidad: ${item.totalResults}`; //return a string that you wish to append
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

    function number_format(val, decimals) {
        //Parse the value as a float value
        val = parseFloat(val);
        //Format the value w/ the specified number
        //of decimal places and return it.
        return val.toFixed(decimals);
    }
});