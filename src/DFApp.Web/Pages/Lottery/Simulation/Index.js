$(function () {
    var l = abp.localization.getResource('DFApp');
    var generateModal = new abp.ModalManager(abp.appPath + 'Lottery/Simulation/GenerateRandomNumbersModal');
    var deleteModal = new abp.ModalManager(abp.appPath + 'Lottery/Simulation/DeleteByTermNumberModal');
    var calculateModal = new abp.ModalManager(abp.appPath + 'Lottery/Simulation/CalculateWinningModal');

    var dataTable = $('#SimulationTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[0, "desc"]],
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(dFApp.lottery.simulation.lotterySimulation.getList),
            columnDefs: [
                {
                    title: l('LotterySimulation:TermNumber'),
                    data: "termNumber",
                    className: 'text-center'
                },
                {
                    title: l('LotterySimulation:RedNumbers'),
                    data: "redNumbers",
                    className: 'text-center'
                },
                {
                    title: l('LotterySimulation:BlueNumber'),
                    data: "blueNumber",
                    className: 'text-center'
                },
                {
                    title: l('LotterySimulation:GameType'),
                    data: "gameType",
                    className: 'text-center',
                    render: function (data) {
                        return data === 0 ? '双色球' : '快乐8';
                    }
                },
                {
                    title: l('LotterySimulation:GroupId'),
                    data: "groupId",
                    className: 'text-center'
                }
            ]
        })
    );

    generateModal.onResult(function () {
        dataTable.ajax.reload();
    });

    deleteModal.onResult(function () {
        dataTable.ajax.reload();
    });

    $('#GenerateRandomNumbersButton').click(function (e) {
        e.preventDefault();
        generateModal.open();
    });

    $('#DeleteByTermNumberButton').click(function (e) {
        e.preventDefault();
        deleteModal.open();
    });

    $('#CalculateWinningButton').click(function (e) {
        e.preventDefault();
        calculateModal.open();
    });

    $('#StatisticsButton').click(function (e) {
        e.preventDefault();
        window.location.href = '/Lottery/Simulation/Statistics';
    });
});
