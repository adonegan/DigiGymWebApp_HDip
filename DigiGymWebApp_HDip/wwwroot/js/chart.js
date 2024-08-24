document.addEventListener('DOMContentLoaded', (event) => {

    Chart.register(ChartDataLabels);

    // store html element
    var ctx = document.getElementById('weightChart').getContext('2d');

    // get weight and timestamp data
    var weightData = JSON.parse(document.getElementById('weightData').textContent);
    var dateData = JSON.parse(document.getElementById('dateData').textContent);

    // convert dateData (strings) to Date objects for proper time scale, Chart.js functionality
    var dates = dateData.map(date => new Date(date));

    // create min and max points for x and y axis
    var maxWeight = Math.max(...weightData);
    var yAxisMax = maxWeight + 10;

    var minWeight = Math.min(...weightData);
    var yAxisMin = minWeight - 10;

    // initialise chart
    var chart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: dates,
            datasets: [{
                label: 'Weight',
                data: weightData,
                borderColor: 'rgba(75, 192, 192, 1)',
                backgroundColor: 'rgba(75, 192, 192, 0.2)',
                fill: true
            }]
        },
        options: {
            responsive: true,
            scales: {
                x: {
                    type: 'time',
                    time: {
                        unit: 'month',
                        tooltipFormat: 'MMM YYYY'
                    },
                    title: {
                        display: true,
                        text: 'Date'
                    }
                },
                y: {
                    beginAtZero: false,
                    min: yAxisMin,
                    max: yAxisMax,
                    title: {
                        display: true,
                        text: 'Weight'
                    }
                }
            },
            plugins: {
                datalabels: {
                    display: false  
                }
            }
        }
    });
});
