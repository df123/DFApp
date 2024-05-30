$(function () {
    var l = abp.localization.getResource('DFApp');
    var linkModal = new abp.ModalManager(abp.appPath + 'Aria2/LinkModal');


    var dataTable = $('#Aria2Table').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[1, "desc"]],
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(dFApp.aria2.aria2.getList),
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items:
                            [
                                {
                                    text: l('Aria2:Button:Link'),
                                    action: function (data) {
                                        linkModal.open({ id: data.record.id });
                                    }
                                },
                                {
                                    text: l('Aria2:Button:Delete'),
                                    visible: abp.auth.isGranted('DFApp.Aria2.Delete'),
                                    confirmMessage: function (data) {
                                        return l('Aria2:DeletionConfirmationMessage', data.record.gID);
                                    },
                                    action: function (data) {
                                        dFApp.aria2.aria2
                                            .delete(data.record.id)
                                            .then(function () {
                                                abp.notify.info(l('Aria2:SuccessfullyDeleted'));
                                                dataTable.ajax.reload();
                                            });
                                    }
                                }
                            ]
                    }
                },
                {
                    title: l('Aria2:Column:ID'),
                    data: "id"
                },
                {
                    title: l('Aria2:Column:GID'),
                    data: "gid"
                },
                {
                    title: l('Aria2:Column:Status'),
                    data: "status"
                },
                {
                    title: l('Aria2:Column:TotalLength'),
                    data: "totalLength",
                    render: function (data, type) {
                        return convertBytes(data);
                    }
                },
                {
                    title: l('Aria2:Column:TotalLength'),
                    data: "files",
                    render: function (data, type) {
                        return data[0].path;
                    }
                },
                {
                    title: l('Aria2:Column:CreationTime'),
                    data: "creationTime",
                    dataFormat: "datetime"
                }
            ]
        })
    );

    $('#DeleteAllButton').click(function (e) {

        var result = confirm("Are you sure you want to proceed?");
        if (result) {
            dFApp.aria2.aria2.deleteAll();
        }

    })

});