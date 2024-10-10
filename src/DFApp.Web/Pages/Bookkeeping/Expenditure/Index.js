$(function () {
    var l = abp.localization.getResource('DFApp');
    var editModal = new abp.ModalManager(abp.appPath + 'Bookkeeping/Expenditure/EditModal');
    var createModal = new abp.ModalManager(abp.appPath + 'Bookkeeping/Expenditure/CreateModal');

    var dataTable = $('#ExpenditureTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            searching: true,
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
                    title: l('BookkeepingExpenditure:Column:ExpenditureDate'),
                    data: "expenditureDate",
                    dataFormat: "date"
                },
                {
                    title: l('BookkeepingExpenditure:Column:Expenditure'),
                    data: "expenditure"
                },
                {
                    title: l('BookkeepingExpenditure:Column:IsBelongToSelf'),
                    data: "isBelongToSelf",
                    render: function (data, type, row) {
                        return '<input type="checkbox" ' + (data ? 'checked' : '') + '>';
                    }
                },
                {
                    title: l('BookkeepingExpenditure:Column:Category'),
                    data: "category.category"
                },
                {
                    title: l('BookkeepingExpenditure:Column:Remark'),
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
            ],
            footerCallback: function (row, data, start, end, display) {

                let api = this.api();


                // Remove the formatting to get integer data for summation
                let intVal = function (i) {
                    return typeof i === 'string'
                        ? i.replace(/[\$,]/g, '') * 1
                        : typeof i === 'number'
                            ? i
                            : 0;
                };

                // Total over all pages
                total = api
                    .column(2)
                    .data()
                    .reduce((a, b) => intVal(a) + intVal(b), 0);

                // Total over this page
                pageTotal = api
                    .column(2, { page: 'current' })
                    .data()
                    .reduce((a, b) => intVal(a) + intVal(b), 0);

                // Update footer

                api.column(2).footer().innerHTML =
                    '￥' + pageTotal.toFixed(2) + ' ( ￥' + total.toFixed(2) + ' total)';

            },
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
