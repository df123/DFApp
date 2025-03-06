$(function () {
    var l = abp.localization.getResource('DFApp');
    var statisticsService = dFApp.lottery.simulation.lotteryK8Simulation;

    statisticsService.getStatistics().then(function (result) {
        var ctx = document.getElementById('k8StatisticsChart').getContext('2d');
        new Chart(ctx, {
            type: 'line',
            data: {
                labels: result.terms,
                datasets: [
                    {
                        label: l('LotteryK8:PurchaseAmount'),
                        data: result.purchaseAmounts,
                        borderColor: 'rgb(75, 192, 192)',
                        tension: 0.1
                    },
                    {
                        label: l('LotteryK8:WinningAmount'),
                        data: result.winningAmounts,
                        borderColor: 'rgb(255, 99, 132)',
                        tension: 0.1
                    }
                ]
            },
            options: {
                responsive: true,
                plugins: {
                    title: {
                        display: true,
                        text: l('LotteryK8:Statistics')
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
});
