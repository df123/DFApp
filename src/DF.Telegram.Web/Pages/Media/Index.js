$(function () {
    var l = abp.localization.getResource('Telegram');
    var editModal = new abp.ModalManager(abp.appPath + 'Media/EditModal');

    var dataTable = $('#MediasTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[1, "desc"]],
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(dF.telegram.media.mediaInfo.getList),
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items:
                            [
                                {
                                    text: l('Edit'),
                                    visible: abp.auth.isGranted('Telegram.Medias.Edit'),
                                    action: function (data) {
                                        editModal.open({ id: data.record.id });
                                    }
                                },
                                {
                                    text: l('Delete'),
                                    visible: abp.auth.isGranted('Telegram.Medias.Delete'),
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
                                },
                                {
                                    text: l('Button:Download'),
                                    visible: abp.auth.isGranted('Telegram.Medias.Download'),
                                    action: function (data) {
                                        window.open('api/FileDownload?id=' + data.record.id, '_blank');
                                    }
                                },
                            ]
                    }
                },
                {
                    title: l('MediaId'),
                    data: "id"
                },
                {
                    title: l('MediaAccessHash'),
                    data: "accessHash"
                },
                {
                    title: l('MediaTId'),
                    data: "tid"
                },
                {
                    title: l('MediaSize'),
                    data: "size"
                },
                {
                    title: l('MediaIsDownload'),
                    data: "isDownload"
                },
                {
                    title: l('MediaIsReturn'),
                    data: "isReturn"
                },
                {
                    title: l('MediaTaskComplete'),
                    data: "taskComplete",
                    dataFormat: "datetime"
                },
                {
                    title: l('MediaSavePath'),
                    data: "savePath"
                },
                {
                    title: l('MediaValueSHA1'),
                    data: "valueSHA1"
                },
                {
                    title: l('MediaCreationTime'),
                    data: "creationTime",
                    dataFormat: "datetime"
                },
                {
                    title: l('MediaLastModificationTime'),
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