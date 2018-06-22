using System.Globalization;
using System.Windows.Controls;

namespace Validation
{
    public class EmptyTextValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string Text = value as string;

            return new ValidationResult(Text != null && Text.Length > 0, "(Enter text)");
        }
    }
}
