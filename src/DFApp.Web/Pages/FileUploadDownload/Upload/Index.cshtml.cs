using DFApp.Helper;
using DFApp.Web.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.IO;

namespace DFApp.Web.Pages.FileUploadDownload.Upload
{
    public class IndexModel : DFAppPageModel
    {

        private readonly long _fileSizeLimit;
        //private readonly string[] _permittedExtensions = { ".txt" };
        //private readonly string _targetFilePath;

        //public IndexModel()
        //{
        //    _fileSizeLimit = 10240;
        //    _targetFilePath = ".";
        //}

        //[BindProperty]
        //public BufferedSingleFileUploadPhysical FileUpload { get; set; }

        //public string Result { get; private set; }

        //public void OnGet()
        //{
        //}

        //public async Task<IActionResult> OnPostUploadAsync()
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        Result = "Please correct the form.";

        //        return Page();
        //    }
        //    var formFileContent =
        //        await FileHelpers.ProcessFormFile<BufferedSingleFileUploadPhysical>(
        //            FileUpload.FormFile, ModelState, _permittedExtensions,
        //            _fileSizeLimit);

        //    if (!ModelState.IsValid)
        //    {
        //        Result = "Please correct the form.";

        //        return Page();
        //    }

        //    // For the file name of the uploaded file stored
        //    // server-side, use Path.GetRandomFileName to generate a safe
        //    // random file name.
        //    var trustedFileNameForFileStorage = Path.GetRandomFileName();
        //    var filePath = Path.Combine(
        //        _targetFilePath, trustedFileNameForFileStorage);

        //    // **WARNING!**
        //    // In the following example, the file is saved without
        //    // scanning the file's contents. In most production
        //    // scenarios, an anti-virus/anti-malware scanner API
        //    // is used on the file before making the file available
        //    // for download or for use by other systems. 
        //    // For more information, see the topic that accompanies 
        //    // this sample.

        //    using (var fileStream = System.IO.File.Create(filePath))
        //    {
        //        await fileStream.WriteAsync(formFileContent);

        //        // To work directly with a FormFile, use the following
        //        // instead:
        //        //await FileUpload.FormFile.CopyToAsync(fileStream);
        //    }

        //    return RedirectToPage("./Index");
        //}

        [BindProperty]
        public UploadFileDto UploadFileDto { get; set; }

        //private readonly IFileAppService _fileAppService;

        public bool Uploaded { get; set; } = false;

        public IndexModel()
        {
            _fileSizeLimit = 10 * 1024 * 1024;
        }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostAsync()
        {

            if (UploadFileDto.File.Length > _fileSizeLimit)
            {
                TempData["MsgFail"] = "大小超过限制";
                return Page();
            }

            using (var memoryStream = new MemoryStream())
            {
                await UploadFileDto.File.CopyToAsync(memoryStream);

                System.IO.File.WriteAllBytes($"{AppsettingsHelper.app("RunConfig", "SaveUplouadFilePath")}/{UploadFileDto.Name}", memoryStream.ToArray());
            }

            TempData["Msg"] = "上传成功";

            return Page();
        }

    }

    public class BufferedSingleFileUploadPhysical
    {
        [Required]
        [Display(Name = "File")]
        public IFormFile FormFile { get; set; }

        [Display(Name = "Note")]
        [StringLength(50, MinimumLength = 0)]
        public string Note { get; set; }
    }

    public class UploadFileDto
    {
        [Required]
        [Display(Name = "File")]
        public IFormFile File { get; set; }

        [Required]
        [Display(Name = "Filename")]
        public string Name { get; set; }
    }


}
