$(function () {
    var l = abp.localization.getResource('Telegram');

    $("#btnExternalLink").click(function () {
        dF.telegram.media.mediaInfo.getExternalLinkDownload()
            .then(result => {
                $('#textExternalLink')[0].innerHTML = result;
            })
            .catch(err => {
                console.log(err);
            })
    });

    $("#btnCopy").click(function () {
        navigator.clipboard.writeText($('#textExternalLink')[0].innerHTML);
        alert(l('Media:ExternalLinkTitle:CopySuccessMesage'));
    });

    $("#btnMove").click(function () {
        console.log('123');
        dF.telegram.media.mediaInfo.moveDownloaded()
            .then(result => {
                $('#textExternalLink')[0].innerHTML = result;
            })
            .catch(err => {
                console.log(err);
            })
    });

});