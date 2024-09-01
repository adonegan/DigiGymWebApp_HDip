document.addEventListener('DOMContentLoaded', (event) => {

    Chart.register(ChartDataLabels);

    // get canvas element
    var ctx = document.getElementById('macroPie').getContext('2d');

    // get macro data
    var proteinData = document.getElementById('proteinData').textContent;
    var carbData = document.getElementById('carbData').textContent;
    var fatData = document.getElementById('fatData').textContent;

    // convert strings to numbers
    var protein = parseFloat(proteinData);
    var carbs = parseFloat(carbData);
    var fat = parseFloat(fatData);

    // initialise chart
    var macroPieChart = new Chart(ctx, {
        type: 'pie',
        data: {
            labels: [
                'Protein',
                'Carbs',
                'Fat'
            ],
            datasets: [{
                label: 'Total Macros for Today',
                data: [protein, carbs, fat],
                backgroundColor: [
                    '#008080',
                    '#6c757d',
                    '#7fbe9f'
                ],
                hoverOffset: 4
            }]
        },
        options: {
            responsive: true, 
            plugins: {
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            // tooltip content
                            let label = context.label || '';
                            let value = context.raw || 0;
                            return `${label}: ${value}g`;
                        }
                    }
                },
                legend: {
                    display: true, 
                },
                datalabels: {
                    display: true,
                    color: 'black',
                    formatter: (value) => {
                        return `${value}g`;
                    }
                }
            }
        }
    });
});