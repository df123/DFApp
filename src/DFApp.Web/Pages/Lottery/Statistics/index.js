$(function () {
    let chart;

    function initializeEvents() {
        $('#lotteryType').change(updateChart);
        $('#purchasedPeriod').change(handlePeriodChange);
        $('#winningPeriod').change(handlePeriodChange);
    }

    function handlePeriodChange() {
        let purchasedPeriod = $('#purchasedPeriod').val();
        let winningPeriod = $('#winningPeriod').val();

        if (isValidString(purchasedPeriod) && !isValidString(winningPeriod)) {
            $('#winningPeriod').val(purchasedPeriod);
        }

        if (isValidString(winningPeriod) && !isValidString(purchasedPeriod)) {
            $('#purchasedPeriod').val(winningPeriod);
        }

        updateChart();
    }

    async function updateChart() {
        const purchasedPeriod = $('#purchasedPeriod').val();
        const winningPeriod = $('#winningPeriod').val();
        const lotteryType = $('#lotteryType option:selected').text();

        if (chart) {
            chart.destroy();
        }

        const response = await $.get(`/Lottery/Statistics/Index?handler=StatisticsData&purchasedPeriod=${purchasedPeriod}&winningPeriod=${winningPeriod}&lotteryType=${lotteryType}`);
        
        if (response && response.length > 0) {
            const labels = response.map(item => item.code);
            const buyData = response.map(item => item.buyAmount);
            const winData = response.map(item => item.winAmount);

            chart = new Chart('statisticsChart', {
                type: 'bar',
                data: {
                    labels: labels,
                    datasets: [{
                        label: '购买金额',
                        data: buyData,
                        borderWidth: 1
                    }, {
                        label: '中奖金额',
                        data: winData,
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
        }
    }

    function isValidString(str) {
        return str !== null && str !== undefined && str.trim() !== '';
    }

    initializeEvents();
    updateChart();
});
