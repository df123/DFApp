$(function () {
    var l = abp.localization.getResource('DFApp');
    var editModal = new abp.ModalManager(abp.appPath + 'Bookkeeping/Expenditure/EditModal');
    var createModal = new abp.ModalManager(abp.appPath + 'Bookkeeping/Expenditure/CreateModal');

    var dataTable = $('#ExpenditureTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[1, "desc"]],
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(dFApp.bookkeeping.expenditure.bookkeepingExpenditure.getList),
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items:
                            [
                                {
                                    text: l('Edit'),
                                    visible: abp.auth.isGranted('DFApp.Bookkeeping.Expenditure.Edit'),
                                    action: function (data) {
                                        editModal.open({ id: data.record.id });
                                    }
                                },
                                {
                                    text: l('Delete'),
                                    visible: abp.auth.isGranted('DFApp.Bookkeeping.Expenditure.Delete'),
                                    confirmMessage: function (data) {
                                        return l('MediaDeletionConfirmationMessage', data.record.id);
                                    },
                                    action: function (data) {
                                        dFApp.bookkeeping.expenditure.bookkeepingExpenditure
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
                    title: l('BookkeepingExpenditure:Column:Id'),
                    data: "id"
                },
                {
                    title: l('BookkeepingExpenditure:Column:ExpenditureDate'),
                    data: "expenditureDate",
                    dataFormat: "date"
                },
                {
                    title: l('BookkeepingExpenditure:Column:Expenditure'),
                    data: "expenditure"
                },
                {
                    title: l('BookkeepingExpenditure:Column:Category'),
                    data: "category"
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

    createModal.onResult(function () {
        dataTable.ajax.reload();
    });

    $('#NewExpenditureButton').click(function (e) {
        e.preventDefault();
        createModal.open();
    })

});