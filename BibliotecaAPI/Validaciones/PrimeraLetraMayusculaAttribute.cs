using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.Validaciones
{
    public class PrimeraLetraMayusculaAttribute: ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null || string.IsNullOrEmpty(value.ToString()))
            {
                return ValidationResult.Success;
            }
            //return base.IsValid(value, validationContext);
            var valueString = value.ToString()!;
            var primeraLetra = valueString[0].ToString();
            if (primeraLetra != primeraLetra.ToUpper())
            {
                return new ValidationResult("Primera letra debe se mayuscula");
            }
            return ValidationResult.Success;
        }
    }
}
