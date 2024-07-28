// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


document.addEventListener('DOMContentLoaded', (event) => {

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
                    'rgb(255, 99, 132)',
                    'rgb(54, 162, 235)',
                    'rgb(255, 205, 86)'
                ],
                hoverOffset: 4
            }]
        }
    });
});