$(function () {
    var l = abp.localization.getResource('DFApp');
    var k8GenerateModal = new abp.ModalManager(abp.appPath + 'Lottery/Simulation/K8GenerateModal');
    var deleteModal = new abp.ModalManager(abp.appPath + 'Lottery/Simulation/DeleteByTermNumberModal');

    var dataTable = $('#K8SimulationTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[0, "desc"]],
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(dFApp.lottery.simulation.lotteryK8Simulation.getList),
            columnDefs: [
                {
                    title: l('LotteryK8:TermNumber'),
                    data: "termNumber",
                    className: 'text-center'
                },
                {
                    title: l('LotteryK8:Numbers'),
                    data: "redNumbers",
                    className: 'text-center'
                },
                {
                    title: l('LotteryK8:GameType'),
                    data: "gameType",
                    className: 'text-center',
                    render: function (data) {
                        return data === 0 ? '双色球' : '快乐8';
                    }
                },
                {
                    title: l('LotteryK8:GroupId'),
                    data: "groupId",
                    className: 'text-center'
                }
            ]
        })
    );

    k8GenerateModal.onResult(function () {
        dataTable.ajax.reload();
    });

    deleteModal.onResult(function () {
        dataTable.ajax.reload();
    });

    $('#K8GenerateButton').click(function (e) {
        e.preventDefault();
        k8GenerateModal.open();
    });

    $('#K8DeleteByTermNumberButton').click(function (e) {
        e.preventDefault();
        deleteModal.open();
    });

    $('#K8StatisticsButton').click(function (e) {
        e.preventDefault();
        window.location.href = '/Lottery/Simulation/K8Statistics';
    });
});
