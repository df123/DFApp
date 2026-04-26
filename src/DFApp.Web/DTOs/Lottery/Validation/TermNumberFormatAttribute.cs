using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace DFApp.Web.DTOs.Lottery.Validation
{
    /// <summary>
    /// 期号格式验证属性
    /// 验证期号格式为 yyyyxxx（7位数字，前4位为年份2000-2100，后3位为序号）
    /// </summary>
    public class TermNumberFormatAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return new ValidationResult("期号不能为空");

            var termNumber = value.ToString();

            // 验证格式为 7 位数字，前 4 位为有效年份
            if (!Regex.IsMatch(termNumber!, @"^\d{7}$") ||
                !int.TryParse(termNumber!.Substring(0, 4), out var year) ||
                year < 2000 || year > 2100)
            {
                return new ValidationResult("期号格式必须为 yyyyxxx，例如：2023001");
            }

            return ValidationResult.Success;
        }
    }
}
