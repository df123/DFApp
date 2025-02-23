using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace DFApp.Lottery.Validation
{
    public class TermNumberFormatAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return new ValidationResult("期号不能为空");

            var termNumber = value.ToString();
            
            // 验证格式为 yyyyxxx，其中 yyyy 为年份，xxx 为序号
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
