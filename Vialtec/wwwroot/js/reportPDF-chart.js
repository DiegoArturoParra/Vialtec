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
        url: '/Reflectivities/GetDataChart',
        data: filters,
        type: 'get'
    }).done(function (results) {
        if (results !== null) {
            if (results.length > 6) {
                $('#average-chart').removeClass('col-md-6 col-sm-12').addClass('col-12');
                $('#total-results-chart').removeClass('col-md-6 col-sm-12').addClass('col-12');
            }
            initChartAverage(results);
            initChartTotalResults(results);
        }
    Swal.close();
    }).fail(function (error) { console.error(error); });

    function number_format(val, decimals) {
        //Parse the value as a float value
        val = parseFloat(val);
        //Format the value w/ the specified number
        //of decimal places and return it.
        return val.toFixed(decimals);
    }

    function initChartAverage(results) {
        let dynamicBackgroundColors = [];
        let dynamicBorderColors = [];
        let data = [];
        let labels = [];
        let iterator = 0;
        results.forEach(x => {
            let average = number_format(x.averageMeasurement, 2);
            //console.log({ doubleNumber: x.averageMeasurement, average });
            labels.push(x.lineNumber.toString());
            data.push(average);
            // background and color
            dynamicBackgroundColors.push(backgroundColors[iterator]);
            dynamicBorderColors.push(borderColors[iterator]);
            iterator += 1;
            if (iterator > 5) {
                iterator = 0;
            }
        });
        //original canvas
        let ctx = document.getElementById('cool-canvas-average').getContext('2d');
        let myChart = new Chart(ctx, {
            type: 'bar',
            data: {
                //labels: ['Red', 'Blue', 'Yellow', 'Green', 'Purple', 'Orange'],
                labels,
                datasets: [{
                    label: `Promedio`,
                    //data: [12, 19, 3, 5, 2, 3],
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
                scales: {
                    yAxes: [{
                        display: true,
                        scaleLabel: {
                            display: true,
                            labelString: 'Retro-reflectividad',
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
                            labelString: 'Número de línea',
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

    function initChartTotalResults(results) {
        let dynamicBackgroundColors = [];
        let dynamicBorderColors = [];
        let data = [];
        let labels = [];
        let iterator = 0;
        results.forEach(x => {
            labels.push(x.lineNumber.toString());
            data.push(x.totalResults);
            // background and color
            dynamicBackgroundColors.push(backgroundColors[iterator]);
            dynamicBorderColors.push(borderColors[iterator]);
            iterator += 1;
            if (iterator > 5) {
                iterator = 0;
            }
        });
        //original canvas
        let ctx = document.getElementById('cool-canvas-total-results').getContext('2d');
        let myChart = new Chart(ctx, {
            type: 'bar',
            data: {
                //labels: ['Red', 'Blue', 'Yellow', 'Green', 'Purple', 'Orange'],
                labels,
                datasets: [{
                    label: 'Número de muestras',
                    //data: [12, 19, 3, 5, 2, 3],
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
                scales: {
                    yAxes: [{
                        display: true,
                        scaleLabel: {
                            display: true,
                            labelString: 'Número de muestras',
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
                            labelString: 'Número de línea',
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