$(function () {
    var l = abp.localization.getResource('Telegram');

    const ctx = document.getElementById('meidaChart');
    dF.telegram.media.mediaInfo.getChartData()
        .then(result => {
            new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: result.labels,
                    datasets: [{
                        label: '# of Votes',
                        data: result.datas,
                        borderWidth: 1
                    }]
                },
                options: {
                    scales: {
                        y: {
                            beginAtZero: true
                        }
                    }
                }
            });
        })
        .catch(err => {
            console.log(err);
        })
});