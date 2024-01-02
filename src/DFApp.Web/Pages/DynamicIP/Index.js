$(function () {
    var l = abp.localization.getResource('DFApp');

    var dataTable = $('#DynamicIPTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[3, "desc"]],
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(dFApp.iP.dynamicIP.getList),
            columnDefs: [
                {
                    title: l('DynamicIPIPAddress'),
                    render: (data, type, row) => row.ip + ":" + row.port
                },
                {
                    title: l('DynamicIPIP'),
                    data: "ip"
                },
                {
                    title: l('DynamicIPPort'),
                    data: "port"
                },
                {
                    title: l('DynamicIPCreateTime'),
                    data: "creationTime",
                    dataFormat: "datetime"
                }
            ]
        })
    );

})