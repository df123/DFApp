$(function () {
    var l = abp.localization.getResource('DFApp');
    var statisticsService = dFApp.lottery.simulation.lotterySSQSimulation.getStatistics;
    
    function initChart() {
        statisticsService().then(function (result) {
            var ctx = document.getElementById('statisticsChart').getContext('2d');
            new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: result.terms,
                    datasets: [{
                        label: l('LotterySimulation:PurchaseAmount'),
                        data: result.purchaseAmounts,
                        backgroundColor: 'rgba(75, 192, 192, 0.5)',
                        borderColor: 'rgb(75, 192, 192)',
                        borderWidth: 1
                    }, {
                        label: l('LotterySimulation:WinningAmount'),
                        data: result.winningAmounts,
                        backgroundColor: 'rgba(255, 99, 132, 0.5)',
                        borderColor: 'rgb(255, 99, 132)',
                        borderWidth: 1
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
