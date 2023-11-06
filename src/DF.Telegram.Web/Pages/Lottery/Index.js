$(function () {
    var l = abp.localization.getResource('Telegram');
    var editModal = new abp.ModalManager(abp.appPath + 'Lottery/EditModal');

    var dataTable = $('#LotteryTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[1, "desc"]],
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(dF.telegram.lottery.lottery.getList),
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items:
                            [
                                {
                                    text: l('Edit'),
                                    visible: abp.auth.isGranted('Telegram.Lottery.Edit'),
                                    action: function (data) {
                                        editModal.open({ id: data.record.id });
                                    }
                                },
                                {
                                    text: l('Delete'),
                                    visible: abp.auth.isGranted('Telegram.Lottery.Delete'),
                                    confirmMessage: function (data) {
                                        return l('MediaDeletionConfirmationMessage', data.record.accessHash);
                                    },
                                    action: function (data) {
                                        dF.telegram.media.mediaInfo
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
                    title: l('LotteryNumber'),
                    data: "number"
                },
                {
                    title: l('LotteryColorType'),
                    data: "colorType"
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