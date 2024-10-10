$(function () {
    var l = abp.localization.getResource('DFApp');
    var editModal = new abp.ModalManager(abp.appPath + 'Configuration/EditModal');
    var createModal = new abp.ModalManager(abp.appPath + 'Configuration/CreateModal');

    var dataTable = $('#ConfigurationInfoTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[1, "desc"]],
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(dFApp.configuration.configurationInfo.getList),
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items:
                            [
                                {
                                    text: l('Edit'),
                                    visible: abp.auth.isGranted('DFApp.ConfigurationInfo.Edit'),
                                    action: function (data) {
                                        editModal.open({ id: data.record.id });
                                    }
                                },
                                {
                                    text: l('Delete'),
                                    visible: abp.auth.isGranted('DFApp.ConfigurationInfo.Delete'),
                                    confirmMessage: function (data) {
                                        return l('MediaDeletionConfirmationMessage', data.record.id);
                                    },
                                    action: function (data) {
                                        dFApp.configuration.configurationInfo
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
                    title: l('ConfigurationInfo:Column:Id'),
                    data: "id"
                },
                {
                    title: l('ConfigurationInfo:Column:MoudleName'),
                    data: "moduleName"
                },
                {
                    title: l('ConfigurationInfo:Column:ConfigurationName'),
                    data: "configurationName"
                },
                {
                    title: l('ConfigurationInfo:Column:ConfigurationValue'),
                    data: "configurationValue"
                },
                {
                    title: l('ConfigurationInfo:Column:Remark'),
                    data: "remark"
                },
                {
                    title: l('Common:Column:CreationTime'),
                    data: "creationTime",
                    dataFormat: "datetime"
                },
                {
                    title: l('Common:Column:ModificationTime'),
                    data: "lastModificationTime",
                    dataFormat: "datetime"
                }
            ]
        })
    );

    createModal.onResult(function () {
        dataTable.ajax.reload();
    });

    editModal.onResult(function () {
        dataTable.ajax.reload();
    });

    $('#NewBookButton').click(function (e) {
        e.preventDefault();
        createModal.open();
    });

});