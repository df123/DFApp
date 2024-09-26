$(function () {
    
    $("#UploadFileDto_File").change(function () {
        var fileName = $(this)[0].files[0].name;

        $("#UploadFileDto_Name").val(fileName);
    });

});