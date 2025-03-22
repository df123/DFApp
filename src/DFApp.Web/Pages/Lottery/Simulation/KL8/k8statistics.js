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

            var datasets = [];
            var colors = {
                purchase: {
                    backgroundColor: 'rgba(75, 192, 192, 0.5)',
                    borderColor: 'rgb(75, 192, 192)'
                },
                winning: {
                    backgroundColor: 'rgba(255, 99, 132, 0.5)',
                    borderColor: 'rgb(255, 99, 132)'
                }
            };

            // 为每种选号类型创建数据集
            Object.keys(result.purchaseAmountsByType).forEach(function(type) {
                datasets.push({
                    label: l('LotteryK8:PurchaseAmount') + '-' + type,
                    data: result.purchaseAmountsByType[type],
                    backgroundColor: colors.purchase.backgroundColor,
                    borderColor: colors.purchase.borderColor,
                    borderWidth: 1
                });
                
                datasets.push({
                    label: l('LotteryK8:WinningAmount') + '-' + type,
                    data: result.winningAmountsByType[type],
                    backgroundColor: colors.winning.backgroundColor,
                    borderColor: colors.winning.borderColor,
                    borderWidth: 1
                });
            });
            
            ctx = ctx.getContext('2d');
            new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: result.terms,
                    datasets: datasets
                },
                options: {
                    responsive: true,
                    plugins: {
                        title: {
                            display: true,
                            text: l('LotteryK8:Statistics')
                        },
                        legend: {
                            position: 'right',
                            labels: {
                                boxWidth: 20
                            }
                        }
                    },
                    scales: {
                        x: {
                            title: {
                                display: true,
                                text: l('LotteryK8:Term')
                            }
                        },
                        y: {
                            beginAtZero: true,
                            title: {
                                display: true,
                                text: l('LotteryK8:Amount')
                            }
                        }
                    }
                }
            });
        })
        .catch(function (error) {
            console.error('获取K8统计数据失败:', error);
        });
});