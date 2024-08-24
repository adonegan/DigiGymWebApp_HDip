document.addEventListener('DOMContentLoaded', (event) => {

    Chart.register(ChartDataLabels);

    // retrieve the JSON data from hidden elements
    const totalIntake = JSON.parse(document.getElementById('waterData').textContent);

    // define daily goal, might make dynamic later
    const dailyGoal = 2000; 

    // calculate remaining amount
    const remaining = dailyGoal - totalIntake;

    // calculate percentage of goal achieved
    const percentage = Math.round((totalIntake / dailyGoal) * 100);

    const data = {
        datasets: [{
            data: [totalIntake, remaining],
            backgroundColor: ['#345760', '#e0e0e0'], 
            borderWidth: 0
        }],
        labels: ['Intake', 'Remaining'] 
    };

    // text in the center
    const centerTextPlugin = {
        id: 'centerTextPlugin',
        beforeDraw: (chart) => {
            const { width, height, ctx } = chart;
            ctx.save();
            const percentText = `${percentage}%`;
            ctx.font = 'bold 50px Arial';
            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';
            ctx.fillStyle = '#345760';
            ctx.fillText(percentText, width / 2, height / 2);
            ctx.restore();
        }
    };

    const options = {
        responsive: true,  
        plugins: {
            tooltip: {
                enabled: false 
            },
            legend: {
                display: false 
            },
            datalabels: {
                display: false
            }
        },
        cutout: '50%'
    };

    new Chart(document.getElementById('waterChart').getContext('2d'), {
        type: 'doughnut',
        data: data,
        options: options,
        plugins: [centerTextPlugin]
    });
});