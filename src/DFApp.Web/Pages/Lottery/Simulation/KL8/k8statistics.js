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
            var colors = [
                { backgroundColor: 'rgba(255, 99, 132, 0.5)', borderColor: 'rgb(255, 99, 132)' },    // 红色
                { backgroundColor: 'rgba(54, 162, 235, 0.5)', borderColor: 'rgb(54, 162, 235)' },    // 蓝色
                { backgroundColor: 'rgba(255, 206, 86, 0.5)', borderColor: 'rgb(255, 206, 86)' },    // 黄色
                { backgroundColor: 'rgba(75, 192, 192, 0.5)', borderColor: 'rgb(75, 192, 192)' },    // 青色
                { backgroundColor: 'rgba(153, 102, 255, 0.5)', borderColor: 'rgb(153, 102, 255)' },  // 紫色
                { backgroundColor: 'rgba(255, 159, 64, 0.5)', borderColor: 'rgb(255, 159, 64)' },    // 橙色
                { backgroundColor: 'rgba(199, 199, 199, 0.5)', borderColor: 'rgb(199, 199, 199)' },  // 灰色
                { backgroundColor: 'rgba(83, 102, 255, 0.5)', borderColor: 'rgb(83, 102, 255)' },    // 靛青
                { backgroundColor: 'rgba(255, 99, 255, 0.5)', borderColor: 'rgb(255, 99, 255)' },    // 粉红
                { backgroundColor: 'rgba(159, 255, 64, 0.5)', borderColor: 'rgb(159, 255, 64)' },    // 浅绿
                { backgroundColor: 'rgba(255, 0, 0, 0.5)', borderColor: 'rgb(255, 0, 0)' },          // 深红
                { backgroundColor: 'rgba(0, 255, 0, 0.5)', borderColor: 'rgb(0, 255, 0)' },          // 亮绿
                { backgroundColor: 'rgba(0, 0, 255, 0.5)', borderColor: 'rgb(0, 0, 255)' },          // 深蓝
                { backgroundColor: 'rgba(128, 0, 128, 0.5)', borderColor: 'rgb(128, 0, 128)' },      // 深紫
                { backgroundColor: 'rgba(128, 128, 0, 0.5)', borderColor: 'rgb(128, 128, 0)' },      // 橄榄
                { backgroundColor: 'rgba(0, 128, 128, 0.5)', borderColor: 'rgb(0, 128, 128)' },      // 蓝绿
                { backgroundColor: 'rgba(255, 128, 0, 0.5)', borderColor: 'rgb(255, 128, 0)' },      // 深橙
                { backgroundColor: 'rgba(255, 0, 128, 0.5)', borderColor: 'rgb(255, 0, 128)' },      // 玫红
                { backgroundColor: 'rgba(128, 0, 255, 0.5)', borderColor: 'rgb(128, 0, 255)' },      // 紫罗兰
                { backgroundColor: 'rgba(0, 255, 128, 0.5)', borderColor: 'rgb(0, 255, 128)' }       // 春绿
            ];

            // 为每种选号类型创建数据集
            Object.keys(result.purchaseAmountsByType).forEach(function(type, index) {
                datasets.push({
                    label: l('LotteryK8:PurchaseAmount') + '-' + l('LotteryK8:PlayType:' + type),
                    data: result.purchaseAmountsByType[type],
                    backgroundColor: colors[index * 2].backgroundColor,
                    borderColor: colors[index * 2].borderColor,
                    borderWidth: 1
                });
                
                datasets.push({
                    label: l('LotteryK8:WinningAmount') + '-' + l('LotteryK8:PlayType:' + type),
                    data: result.winningAmountsByType[type],
                    backgroundColor: colors[index * 2 + 1].backgroundColor,
                    borderColor: colors[index * 2 + 1].borderColor,
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