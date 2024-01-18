$(function () {
    var l = abp.localization.getResource('DFApp');
    var editModal = new abp.ModalManager(abp.appPath + 'Lottery/EditModal');

    var dataTable = $('#LotteryTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[1, "desc"]],
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(dFApp.lottery.lottery.getList),
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items:
                            [
                                {
                                    text: l('Edit'),
                                    visible: abp.auth.isGranted('DFApp.Lottery.Edit'),
                                    action: function (data) {
                                        editModal.open({ id: data.record.id });
                                    }
                                },
                                {
                                    text: l('Delete'),
                                    visible: abp.auth.isGranted('DFApp.Lottery.Delete'),
                                    confirmMessage: function (data) {
                                        return l('MediaDeletionConfirmationMessage', data.record.id);
                                    },
                                    action: function (data) {
                                        dFApp.lottery.lottery
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
                    title: l('LotteryId'),
                    data: "id"
                },
                {
                    title: l('LotteryIndexNo'),
                    data: "indexNo"
                },
                {
                    title: l('LotteryType'),
                    data: "lotteryType"
                },
                {
                    title: l('LotteryNumber'),
                    data: "number"
                },
                {
                    title: l('LotteryColorType'),
                    data: "colorType",
                    render: function (data, type) {
                        if (data == '0') {
                            return '红球';
                        }
                        else {
                            return '蓝球';
                        }
                        return data;
                    }
                },
                {
                    title: l('LotteryGroupId'),
                    data: "groupId"
                },
                {
                    title: l('LotteryCreationTime'),
                    data: "creationTime",
                    dataFormat: "datetime"
                },
                {
                    title: l('LotteryLastModificationTime'),
                    data: "lastModificationTime",
                    dataFormat: "datetime"
                }
            ]
        })
    );

    editModal.onResult(function () {
        dataTable.ajax.reload();
    });

});