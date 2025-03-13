$(function () {
    var l = abp.localization.getResource('DFApp');
    var statisticsService = dFApp.lottery.simulation.lotteryKL8Simulation;

    statisticsService.getStatistics()
        .then(function (result) {
            if (!result || !result.terms || result.terms.length === 0) {
                console.error('未获取到有效数据');
                return;
            }

            var ctx = document.getElementById('k8StatisticsChart');
            if (!ctx) {
                console.error('未找到图表容器元素');
                return;
            }
            
            ctx = ctx.getContext('2d');
            new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: result.terms,
                    datasets: [
                        {
                            label: l('LotteryK8:PurchaseAmount'),
                            data: result.purchaseAmounts,
                            backgroundColor: 'rgba(75, 192, 192, 0.5)',
                            borderColor: 'rgb(75, 192, 192)',
                            borderWidth: 1
                        },
                        {
                            label: l('LotteryK8:WinningAmount'),
                            data: result.winningAmounts,
                            backgroundColor: 'rgba(255, 99, 132, 0.5)',
                            borderColor: 'rgb(255, 99, 132)',
                            borderWidth: 1
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
        })
        .catch(function (error) {
            console.error('获取K8统计数据失败:', error);
        });
});