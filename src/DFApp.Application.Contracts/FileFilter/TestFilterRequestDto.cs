using System.ComponentModel.DataAnnotations;

namespace DFApp.FileFilter
{
    /// <summary>
    /// 测试过滤请求DTO
    /// </summary>
    public class TestFilterRequestDto
    {
        /// <summary>
        /// 要测试的文件名
        /// </summary>
        [Required(ErrorMessage = "文件名不能为空")]
        public required string FileName { get; set; }
    }
}
