$(function () {
    var l = abp.localization.getResource('DFApp');

    var dataTable = $('#FileListTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[1, "desc"]],
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(dFApp.fileUploadDownload.fileUploadInfo.getList),
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items:
                            [
                                {
                                    text: l('Delete'),
                                    visible: abp.auth.isGranted('DFApp.FileUploadDownload.Delete'),
                                    confirmMessage: function (data) {
                                        return l('MediaDeletionConfirmationMessage', data.record.fileName);
                                    },
                                    action: function (data) {
                                        dFApp.fileUploadDownload.fileUploadInfo
                                            .delete(data.record.id)
                                            .then(function () {
                                                abp.notify.info(l('SuccessfullyDeleted'));
                                                dataTable.ajax.reload();
                                            });
                                    }
                                },
                                {
                                    text: l('Button:Download'),
                                    visible: abp.auth.isGranted('DFApp.FileUploadDownload.Download'),
                                    action: function (data) {
                                        console.log()
                                        window.open(`${window.location.origin}/api/FileUploadInfo?id=${data.record.id}`, '_blank');
                                    }
                                },
                            ]
                    }
                },
                {
                    title: l('FileUploadDownload:Column:Id'),
                    data: "id"
                },
                {
                    title: l('FileUploadDownload:Column:FileName'),
                    data: "fileName"
                },
                {
                    title: l('FileUploadDownload:Column:Path'),
                    data: "path"
                },
                {
                    title: l('FileUploadDownload:Column:Sha1'),
                    data: "sha1"
                },
                {
                    title: l('FileUploadDownload:Column:FileSize'),
                    data: "fileSize",
                    render: function (data, type) {
                        return convertBytes(data);
                    }
                },
                {
                    title: l('Common:Column:CreationTime'),
                    data: "creationTime",
                    dataFormat: "datetime"
                }
            ]
        })
    );
});