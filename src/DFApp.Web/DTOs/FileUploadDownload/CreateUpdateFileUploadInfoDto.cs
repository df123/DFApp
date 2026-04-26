namespace DFApp.Web.DTOs.FileUploadDownload;

/// <summary>
/// 创建/更新文件上传信息 DTO
/// </summary>
public class CreateUpdateFileUploadInfoDto
{
    public string FileName { get; set; } = default!;
    public string Path { get; set; } = default!;
    public string Sha1 { get; set; } = default!;
    public long FileSize { get; set; }
}
