$(function () {
    var l = abp.localization.getResource('DFApp');

    var getFilter = function () {
        return {
            purchasedPeriod: undefined,
            winningPeriod: undefined,
            lotteryType: '快乐8'
        };
    };

    var dataTable = $('#LotteryStatisticsItemTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: false,
            paging: true,
            order: [[1, "desc"]],
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(dFApp.lottery.lottery.getStatisticsWinItemInputDto, getFilter),
            columnDefs: [
                {
                    title: l('Lottery:LotteryStatisticsItem:Code'),
                    data: "code"
                },
                {
                    title: l('Lottery:LotteryStatisticsItem:WinCode'),
                    data: "winCode"
                },
                {
                    title: l('Lottery:LotteryStatisticsItem:BuyNumber'),
                    data: "buyLotteryString"
                },
                {
                    title: l('Lottery:LotteryStatisticsItem:WinNumber'),
                    data: "winLotteryString"
                },
                {
                    title: l('Lottery:LotteryStatisticsItem:WinAmout'),
                    data: "winAmount"
                },
            ]
        })
    );

});