using System;
using System.ComponentModel.DataAnnotations;

namespace BookSellerWebAPI.Validation
{
    /// <summary>
    /// Validate if value is at least the same as <paramref name="minValue"/>.
    /// </summary>
    /// <exception cref="FormatException"><paramref name="minValue"/> is not a valid Decimal.</exception>
    /// <exception cref="OverflowException"><paramref name="minValue"/> is either too large or too small for a Decimal.</exception>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class MinAttribute : ValidationAttribute
    {
        public decimal MinValue { get; }

        public MinAttribute(string minValue)
        {
            this.MinValue = Convert.ToDecimal(minValue);
            this.ErrorMessage = $"Value must be greater than or equals to {minValue}";
        }

        public MinAttribute(string minValue, string errorMessage)
        {
            this.MinValue = Convert.ToDecimal(minValue);
            this.ErrorMessage = errorMessage;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            => (decimal)value >= this.MinValue ? ValidationResult.Success : new ValidationResult(this.ErrorMessage);
    }
}
