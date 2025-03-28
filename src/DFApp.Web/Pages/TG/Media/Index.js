$(function () {
    var l = abp.localization.getResource('DFApp');
    var editModal = new abp.ModalManager(abp.appPath + 'Media/EditModal');

    var dataTable = $('#MediasTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[6, "desc"]],
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
                                        return l('MediaDeletionConfirmationMessage', data.record.mediaId);
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
                    title: l('Media:Column:MediaId'),
                    data: "mediaId"
                },
                {
                    title: l('Media:Column:IsDownloadCompleted'),
                    data: "isDownloadCompleted",
                    render: function (data, type, row) {
                        return '<input type="checkbox" ' + (data ? 'checked' : '') + '>';
                    }
                },
                {
                    title: l('Media:Column:IsExternalLinkGenerated'),
                    data: "isExternalLinkGenerated",
                    render: function (data, type, row) {
                        return '<input type="checkbox" ' + (data ? 'checked' : '') + '>';
                    }
                },
                {
                    title: l('Media:Column:MediaSize'),
                    data: "size",
                    render: function (data, type) {
                        return convertBytes(data);
                    }
                },
                {
                    title: l('Media:Column:MediaMimeType'),
                    data: "mimeType"
                },
                {
                    title: l('Common:Column:CreationTime'),
                    data: "creationTime",
                    dataFormat: "datetime"
                },
                {
                    title: l('Media:Column:ChatId'),
                    data: "chatId"
                },
                {
                    title: l('Media:Column:ChatTitle'),
                    data: "chatTitle"
                },
                {
                    title: l('Media:Column:Message'),
                    data: "message"
                }
            ]
        })
    );

    editModal.onResult(function () {
        dataTable.ajax.reload();
    });

    $('#deleteInvalidItemsButton').click(function (e) {
        dFApp.media.mediaInfo.deleteInvalidItems().then(function () {
            abp.notify.info(l('Media:Message:InvalidItemsDeletedSuccessfully'));
        });
    })

});