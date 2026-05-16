using DFApp.FileUploadDownload;
using DFApp.Web.DTOs.FileUploadDownload;
using Riok.Mapperly.Abstractions;

namespace DFApp.Web.Mapping;

/// <summary>
/// 文件上传下载映射器
/// </summary>
[Mapper]
public partial class FileUploadDownloadMapper
{
    /// <summary>
    /// FileUploadInfo → FileUploadInfoDto
    /// </summary>
    public partial FileUploadInfoDto MapToDto(FileUploadInfo entity);

    /// <summary>
    /// CreateUpdateFileUploadInfoDto → FileUploadInfo
    /// </summary>
    [MapperIgnoreTarget(nameof(FileUploadInfo.ConcurrencyStamp))]
    public partial FileUploadInfo MapToEntity(CreateUpdateFileUploadInfoDto dto);
}
