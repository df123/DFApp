$(function () {
    var l = abp.localization.getResource('DFApp');
    var linkModal = new abp.ModalManager(abp.appPath + 'TG/Media/ExternalLink/LinkModal');


    var dataTable = $('#ExternalLinkTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[1, "desc"]],
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(dFApp.media.externalLink.externalLink.getList),
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items:
                            [
                                {
                                    text: l('TG:Media:ExternalLink:Button:Link'),
                                    action: function (data) {
                                        console.log(data.record.linkContent);
                                        linkModal.open({ id: data.record.id });
                                    }
                                },
                                {
                                    text: l('TG:Media:ExternalLink:Button:Remove'),
                                    visible: abp.auth.isGranted('DFApp.Medias.Delete'),
                                    confirmMessage: function (data) {
                                        return l('TG:Media:ExternalLink:RemoveConfirmationMessage', data.record.name);
                                    },
                                    action: function (data) {
                                        dFApp.media.externalLink.externalLink
                                            .removeFile(data.record.id)
                                            .then(function () {
                                                abp.notify.info(l('TG:Media:ExternalLink:SuccessfullyDeleted'));
                                                dataTable.ajax.reload();
                                            });
                                    }
                                },
                                {
                                    text: l('TG:Media:ExternalLink:Button:Delete'),
                                    visible: abp.auth.isGranted('DFApp.Medias.Delete'),
                                    confirmMessage: function (data) {
                                        return l('TG:Media:ExternalLink:DeletionConfirmationMessage', data.record.name);
                                    },
                                    action: function (data) {
                                        dFApp.media.externalLink.externalLink
                                            .delete(data.record.id)
                                            .then(function () {
                                                abp.notify.info(l('TG:Media:ExternalLink:SuccessfullyDeleted'));
                                                dataTable.ajax.reload();
                                            });
                                    }
                                }
                            ]
                    }
                },
                {
                    title: l('TG:Media:ExternalLink:Column:ID'),
                    data: "id"
                },
                {
                    title: l('TG:Media:ExternalLink:Column:Name'),
                    data: "name"
                },
                {
                    title: l('TG:Media:ExternalLink:Column:Size'),
                    data: "size",
                    render: function (data, type) {
                        return convertBytes(data);
                    }
                },
                {
                    title: l('TG:Media:ExternalLink:Column:TimeConsumed'),
                    data: "timeConsumed"
                },
                {
                    title: l('TG:Media:ExternalLink:Column:IsRemove'),
                    data: "isRemove"
                },
                {
                    title: l('TG:Media:ExternalLink:Column:CreationTime'),
                    data: "creationTime",
                    dataFormat: "datetime"
                },
                {
                    title: l('TG:Media:ExternalLink:Column:lastModificationTime'),
                    data: "lastModificationTime",
                    dataFormat: "datetime"
                }
            ]
        })
    );

    $("#NewExternalLinkButton").click(function (e) {
        dFApp.media.externalLink.externalLink.getExternalLink();
        abp.notify.info(l('TG:Media:ExternalLink:SuccessfullyAdd'));
    })

});