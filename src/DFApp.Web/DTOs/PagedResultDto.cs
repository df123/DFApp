using System.Collections.Generic;

namespace DFApp.Web.DTOs;

/// <summary>
/// 分页结果 DTO
/// </summary>
/// <typeparam name="TItem">项目类型</typeparam>
public class PagedResultDto<TItem>
{
    /// <summary>
    /// 总记录数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 项目列表
    /// </summary>
    public List<TItem> Items { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    public PagedResultDto()
    {
        Items = new List<TItem>();
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="totalCount">总记录数</param>
    /// <param name="items">项目列表</param>
    public PagedResultDto(int totalCount, List<TItem> items)
    {
        TotalCount = totalCount;
        Items = items;
    }
}
