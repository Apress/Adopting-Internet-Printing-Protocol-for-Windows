using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace IppCheck
{
    public class MinLengthValidationRule : ValidationRule
    {
        public int MinLength { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string input = value as string;
            if (input != null && input.Length >= MinLength)
            {
                return ValidationResult.ValidResult;
            }
            return new ValidationResult(false, $"Input must be at least {MinLength} characters long.");
        }
    }
}
