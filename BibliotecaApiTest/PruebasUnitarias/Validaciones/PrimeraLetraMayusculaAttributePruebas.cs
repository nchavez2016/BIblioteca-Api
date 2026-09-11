using BibliotecaAPI.Validaciones;
using Microsoft.Extensions.Validation;
using Microsoft.OpenApi;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BibliotecaApiTest.PruebasUnitarias.Validaciones
{
    [TestClass]    
    public class PrimeraLetraMayusculaAttributePruebas
    {
        [TestMethod]
        [DataRow("")]
        [DataRow("  ")]
        [DataRow(null)]
        [DataRow("Felipe")]
        public void Isvalid_RetornaExitoso_SiValueNoTieneLaPrimeraMayuscula(string value)
        {
            //preparacion
            var primeraLetraMayusculaAttribute = new PrimeraLetraMayusculaAttribute();
            var validacionContext = new ValidationContext(new object());            
            //prueba
            var resultado=primeraLetraMayusculaAttribute.GetValidationResult(value,validacionContext);
            //verificacion            
            Assert.AreEqual(expected: ValidationResult.Success, actual: resultado);

        }

        [TestMethod]        
        [DataRow("felipe")]
        public void Isvalid_RetornaERror_SiValueTieneLaPrimeraMayuscula(string value)
        {
            //preparacion
            var primeraLetraMayusculaAttribute = new PrimeraLetraMayusculaAttribute();
            var validacionContext = new ValidationContext(new object());
            //prueba
            var resultado = primeraLetraMayusculaAttribute.GetValidationResult(value, validacionContext);
            //verificacion            
            Assert.AreEqual(expected: "Primera letra debe se mayuscula", 
                            actual: resultado!.ErrorMessage);

        }


    }

}
