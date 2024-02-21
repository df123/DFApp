$(function () {
    var l = abp.localization.getResource('DFApp');

    var dataTable = $('#ManagementBackgroundTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: false,
            paging: true,
            order: [[1, "desc"]],
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(dFApp.management.managementBackground.getBackgroundStatus),
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items:
                            [
                                {
                                    text: l('Management:Background:Button:Stop'),
                                    visible: abp.auth.isGranted('DFApp.ManagementBackground.Stop'),
                                    confirmMessage: function (data) {
                                        return l('Management:Background:Msg:Stop', data.record.id);
                                    },
                                    action: function (data) {
                                        dFApp.management.managementBackground.restartService(data.record.moduleName)
                                            .then(function () {
                                                abp.notify.info(l('Management:Background:Msg:StopSuccess'));
                                                dataTable.ajax.reload();
                                            });
                                    }
                                },
                                {
                                    text: l('Management:Background:Button:Restart'),
                                    visible: abp.auth.isGranted('DFApp.ManagementBackground.Restart'),
                                    confirmMessage: function (data) {
                                        return l('Management:Background:Msg:Restart', data.record.id);
                                    },
                                    action: function (data) {
                                        dFApp.management.managementBackground.stopService(data.record.moduleName)                                            
                                            .then(function () {
                                                abp.notify.info(l('Management:Background:Msg:RestartSuccess'));
                                                dataTable.ajax.reload();
                                            });
                                    }
                                }
                            ]
                    }
                },
                {
                    title: l('Management:Background:Column:RunStatus'),
                    data: "runStatus"
                },
                {
                    title: l('Management:Background:Column:ModuleName'),
                    data: "moduleName"
                },
                {
                    title: l('Management:Background:Column:StartTime'),
                    data: "startTime",
                    dataFormat: "datetime"
                },
                {
                    title: l('Management:Background:Column:RestartTime'),
                    data: "restartTime",
                    dataFormat: "datetime"
                },
                {
                    title: l('Management:Background:Column:RestartCount'),
                    data: "restartCount",
                },
                {
                    title: l('Management:Background:Column:HasError'),
                    data: "hasError"
                },
                {
                    title: l('Management:Background:Column:ErrorCount'),
                    data: "errorCount"
                },
                {
                    title: l('Management:Background:Column:ErrorDescription'),
                    data: "errorDescription"
                }
            ]
        })
    );
});