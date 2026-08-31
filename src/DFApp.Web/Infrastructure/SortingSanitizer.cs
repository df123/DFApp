using System;
using System.Linq;
using System.Reflection;

namespace DFApp.Web.Infrastructure;

/// <summary>
/// 排序参数净化器：将用户输入的排序串限制为实体属性白名单 + asc/desc 方向，
/// 防止排序参数被拼接进 SQL（SqlSugar 的 OrderBy(string) 会直接拼入 ORDER BY 子句）
/// </summary>
public static class SortingSanitizer
{
    /// <summary>
    /// 解析用户排序输入；字段必须在实体简单类型属性集合内，方向只允许 asc/desc，
    /// 不满足时回退默认排序
    /// </summary>
    /// <typeparam name="TEntity">目标实体类型</typeparam>
    /// <param name="sorting">用户输入的排序串，如 "creationTime desc"</param>
    /// <param name="defaultSorting">非法输入时使用的默认排序</param>
    public static string Sanitize<TEntity>(string? sorting, string defaultSorting)
    {
        var allowedFields = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(IsSimpleType)
            .Select(p => p.Name)
            .ToArray();

        return Sanitize(sorting, allowedFields, defaultSorting);
    }

    /// <summary>
    /// 显式白名单版本：仅当无法提供实体类型时使用
    /// </summary>
    public static string Sanitize(string? sorting, string[] allowedFields, string defaultSorting)
    {
        if (string.IsNullOrWhiteSpace(sorting))
        {
            return defaultSorting;
        }

        var parts = sorting.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0 || parts.Length > 2)
        {
            return defaultSorting;
        }

        var field = allowedFields.FirstOrDefault(f =>
            string.Equals(f, parts[0].Trim(), StringComparison.OrdinalIgnoreCase));
        if (field is null)
        {
            return defaultSorting;
        }

        var direction = "asc";
        if (parts.Length == 2)
        {
            var raw = parts[1].Trim().ToLowerInvariant();
            if (raw is not ("asc" or "desc"))
            {
                return defaultSorting;
            }

            direction = raw;
        }

        return $"{field} {direction}";
    }

    /// <summary>
    /// 只允许排序映射到单列的简单类型属性，排除导航属性与复杂类型
    /// </summary>
    private static bool IsSimpleType(PropertyInfo property)
    {
        var type = property.PropertyType;
        return type.IsPrimitive ||
               type.IsEnum ||
               type == typeof(string) ||
               type == typeof(decimal) ||
               type == typeof(DateTime) ||
               type == typeof(DateTimeOffset) ||
               type == typeof(TimeSpan) ||
               type == typeof(Guid);
    }
}
