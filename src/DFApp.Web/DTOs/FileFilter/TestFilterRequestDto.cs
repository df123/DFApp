namespace DFApp.Web.DTOs.FileFilter
{
    /// <summary>
    /// 测试过滤请求 DTO
    /// </summary>
    public class TestFilterRequestDto
    {
        /// <summary>
        /// 无参构造函数
        /// </summary>
        public TestFilterRequestDto()
        {
            FileName = string.Empty;
        }

        /// <summary>
        /// 待测试的文件名
        /// </summary>
        public string FileName { get; set; }
    }
}
