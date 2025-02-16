$(function () {
    var l = abp.localization.getResource('DFApp');
    var statisticsService = dFApp.lottery.simulation.lotterySimulation.getStatistics;
    
    function initChart() {
        statisticsService().then(function (result) {
            var ctx = document.getElementById('statisticsChart').getContext('2d');
            new Chart(ctx, {
                type: 'line',
                data: {
                    labels: result.terms,
                    datasets: [{
                        label: l('LotterySimulation:PurchaseAmount'),
                        data: result.purchaseAmounts,
                        borderColor: 'rgb(75, 192, 192)',
                        fill: false
                    }, {
                        label: l('LotterySimulation:WinningAmount'),
                        data: result.winningAmounts,
                        borderColor: 'rgb(255, 99, 132)',
                        fill: false
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        title: {
                            display: true,
                            text: l('LotterySimulation:Statistics')
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true
                        }
                    }
                }
            });
        });
    }

    initChart();
});
