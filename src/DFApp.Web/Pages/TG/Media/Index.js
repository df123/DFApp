$(function () {
    var l = abp.localization.getResource('DFApp');
    var editModal = new abp.ModalManager(abp.appPath + 'Media/EditModal');

    var dataTable = $('#MediasTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[1, "desc"]],
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(dFApp.media.mediaInfo.getList),
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items:
                            [
                                {
                                    text: l('Delete'),
                                    visible: abp.auth.isGranted('DFApp.Medias.Delete'),
                                    confirmMessage: function (data) {
                                        return l('MediaDeletionConfirmationMessage', data.record.accessHash);
                                    },
                                    action: function (data) {
                                        dFApp.media.mediaInfo
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
                    title: l('MediaId'),
                    data: "chatId"
                },
                {
                    title: l('chatTitle'),
                    data: "chatTitle"
                },
                {
                    title: l('IsExternalLinkGenerated'),
                    data: "isExternalLinkGenerated"
                },
                {
                    title: l('MediaSize'),
                    data: "size",
                    render: function (data, type) {
                        return convertBytes(data);
                    }
                },
                {
                    title: l('MediaSavePath'),
                    data: "savePath"
                },
                {
                    title: l('MediaValueSHA1'),
                    data: "mD5"
                },
                {
                    title: l('MediaMimeType'),
                    data: "mimeType"
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
                },
                {
                    title: l('MediaChatTitle'),
                    data: "message"
                }
            ]
        })
    );

    editModal.onResult(function () {
        dataTable.ajax.reload();
    });

});