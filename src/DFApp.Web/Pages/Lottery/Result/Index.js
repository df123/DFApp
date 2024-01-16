$(function () {
    var l = abp.localization.getResource('DFApp');

    var dataTable = $('#LotteryResultTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[2, "desc"]],
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(dFApp.lottery.lotteryResult.getList),
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items:
                            [
                                {
                                    text: l('Delete'),
                                    visible: abp.auth.isGranted('DFApp.Lottery.Delete'),
                                    confirmMessage: function (data) {
                                        return l('MediaDeletionConfirmationMessage', data.record.id);
                                    },
                                    action: function (data) {
                                        dFApp.lottery.lotteryResult
                                            .delete(data.record.id)
                                            .then(function () {
                                                abp.notify.info(l('SuccessfullyDeleted'));
                                                dataTable.ajax.reload();
                                            });
                                    }
                                }
                            ]
                    }
                },
                {
                    title: "Id",
                    data: "id"
                },
                {
                    title: l('Lottery:LotteryPeriod'),
                    data: "code"
                },
                {
                    title: l('Lottery:LotteryName'),
                    data: "name"
                },
                {
                    title: l('Lottery:LotteryBlue'),
                    data: "blue"
                },
                
                {
                    title: l('Lottery:LotteryRed'),
                    data: "red"
                },
                {
                    title: l('Lottery:LotteryCode'),
                    data: "date"
                },
                {
                    title: l('LotteryCreationTime'),
                    data: "creationTime",
                    dataFormat: "datetime"
                }
            ]
        })
    );

});