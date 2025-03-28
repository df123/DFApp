$(function () {
    var l = abp.localization.getResource('DFApp');
    let monthlyChart = null;

    function initMonthlyChart() {
        const ctx = document.getElementById('monthlyChart').getContext('2d');
        monthlyChart = new Chart(ctx, {
            type: 'bar',
            options: {
                responsive: true,
                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });

        loadMonthlyData(getCurrentYear());
    }

    function getCurrentYear() {
        return $('#yearSelect').val();
    }

    function loadMonthlyData(year) {
        dFApp.bookkeeping.expenditure.bookkeepingExpenditure
            .getMonthlyExpenditure(year)
            .then(function (result) {
                updateMonthlyChart(result);
            });
    }

    function updateMonthlyChart(data) {
        monthlyChart.data = {
            labels: data.labels,
            datasets: [
                {
                    label: l('BookkeepingExpenditure:Chart:TotalExpenditure'),
                    data: data.totalData,
                    backgroundColor: 'rgba(54, 162, 235, 0.5)',
                    borderColor: 'rgba(54, 162, 235, 1)',
                    borderWidth: 1
                },
                {
                    label: l('BookkeepingExpenditure:Chart:SelfExpenditure'),
                    data: data.selfData,
                    backgroundColor: 'rgba(75, 192, 192, 0.5)',
                    borderColor: 'rgba(75, 192, 192, 1)',
                    borderWidth: 1
                },
                {
                    label: l('BookkeepingExpenditure:Chart:NonSelfExpenditure'),
                    data: data.nonSelfData,
                    backgroundColor: 'rgba(255, 99, 132, 0.5)',
                    borderColor: 'rgba(255, 99, 132, 1)',
                    borderWidth: 1
                },
                {
                    label: l('BookkeepingExpenditure:Chart:TotalAverage'),
                    data: Array(12).fill(data.totalAverage),
                    type: 'line',
                    borderColor: 'rgba(54, 162, 235, 1)',
                    borderWidth: 2,
                    borderDash: [5, 5],
                    fill: false
                },
                {
                    label: l('BookkeepingExpenditure:Chart:SelfAverage'),
                    data: Array(12).fill(data.selfAverage),
                    type: 'line',
                    borderColor: 'rgba(75, 192, 192, 1)',
                    borderWidth: 2,
                    borderDash: [5, 5],
                    fill: false
                },
                {
                    label: l('BookkeepingExpenditure:Chart:NonSelfAverage'),
                    data: Array(12).fill(data.nonSelfAverage),
                    type: 'line',
                    borderColor: 'rgba(255, 99, 132, 1)',
                    borderWidth: 2,
                    borderDash: [5, 5],
                    fill: false
                }
            ]
        };
        monthlyChart.update();
    }

    // 初始化Select2
    $('#yearSelect').select2({
        minimumResultsForSearch: Infinity, // 禁用搜索
        theme: 'default',
        width: '150px'
    });

    $('#yearSelect').on('change', function() {
        loadMonthlyData($(this).val());
    });

    initMonthlyChart();
});
