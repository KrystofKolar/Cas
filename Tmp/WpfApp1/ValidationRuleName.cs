using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows.Controls;

namespace val
{
    public class ValidationRuleName : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return new ValidationResult(false, "null value");
            }

            string name = (string)value;

            if (name.Length < 3)
            {
                Debug.WriteLine("Check NOK");
                return new ValidationResult(false, "Too short");

            }

            Debug.WriteLine("Check OK");
            return new ValidationResult(true, null);

        }
    }
}
