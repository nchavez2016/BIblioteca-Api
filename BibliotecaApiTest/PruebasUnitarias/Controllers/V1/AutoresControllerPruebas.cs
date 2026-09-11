using Azure;
using BibliotecaAPI.Controllers.V1;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using BibliotecaAPI.Servicios.V1;
using BibliotecaApiTest.Utilidades;
using BibliotecaApiTest.Utilidades.Dobles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.CodeCoverage.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace BibliotecaApiTest.PruebasUnitarias.Controllers.V1
{
    [TestClass]
    public class AutoresControllerPruebas:BasePruebas
    {
        IAlmacenadorArchivos almacenadorArchivos = null!;
        ILogger<AutoresController> logger = null!;
        IOutputCacheStore outputCacheStore = null!;
        IServicioAutores servicioAutores = null!;
        private string nombreBD = Guid.NewGuid().ToString();
        private AutoresController controller = null!;

        [TestInitialize]
        public void Setup()
        {
            //fase preparacion 
            var context = ConstruirContext(nombreBD);
            var mapper = ConfiguraAutomaper();
            almacenadorArchivos = Substitute.For<IAlmacenadorArchivos>(); ;
            logger = Substitute.For<ILogger<AutoresController>>();
            outputCacheStore = Substitute.For<IOutputCacheStore>();
            servicioAutores = Substitute.For<IServicioAutores>();
            controller = new AutoresController(context, mapper, almacenadorArchivos, logger, outputCacheStore, servicioAutores);

        }



        [TestMethod]
        public async Task Get_Retorna404_CuandoAutorConIdNoExiste()
        {            
            //prueba
            var respuesta = await controller.Get(1);

            //verificacion
            var resultado = respuesta.Result as StatusCodeResult;
            Assert.AreEqual(expected: 404, actual: resultado!.StatusCode);

        }

        [TestMethod]
        public async Task Get_Retorna404_CuandoAutorConIdExiste()
        {
            //preparacion            
            var context = ConstruirContext(nombreBD);            
            context.Autores.Add(new Autor { Nombres = "NCh", Apellidos = "Chav" });
            context.Autores.Add(new Autor { Nombres = "Petrus", Apellidos = "Chanc" });
            await context.SaveChangesAsync();
            
            //prueba

            var respuesta = await controller.Get(1);

            //verificacion
            var resultado = respuesta.Value;
            Assert.AreEqual(expected: 1, actual: resultado!.Id);           

        }


        [TestMethod]
        public async Task Get_RetornaAutoresConLIbros_CuandoAutorTieneLibro()
        {
            //preparacion            
            var context = ConstruirContext(nombreBD);
            var libro1 = new Libro { Titulo = "Libro1" };
            var libro2 = new Libro { Titulo = "Libro2" };

            var autor = new Autor
            {
                Nombres = "Nombre 1",
                Apellidos = "Apellido",
                Libros = new List<AutorLibro>
                {
                    new AutorLibro{  Libro=libro1 },
                    new AutorLibro{ Libro= libro2 }
                }

            };
            context.Add(autor);
            await context.SaveChangesAsync();
            //prueba

            var respuesta = await controller.Get(1);

            //verificacion
            var resultado = respuesta.Value;
            Assert.AreEqual(expected: 1, actual: resultado!.Id);
            Assert.AreEqual(expected: 2, actual: resultado.Libros.Count);

        }



        [TestMethod]

        public async Task Get_DebeLllamarGetDelServicioAutore()
        {
            //preparacion            
            var paginacionDTO = new PaginacionDTO(2, 3);

            //prueba
            await controller.Get(paginacionDTO);

            //verificacion
            await servicioAutores.Received(1).Get(paginacionDTO);
        }


        [TestMethod]
        public async Task Post_DebeCrearAutor_CuandoEnviamosAutor()
        {
            //preparacion            
            var context = ConstruirContext(nombreBD);            
            var autorNuevo = new AutorCreacionDTO{ Nombres = "nuevos", Apellidos = "auor" };            
            
            //prueba
            var respuesta= await controller.Post(autorNuevo);

            //verificacion
            var resultado = respuesta as CreatedAtRouteResult;
            Assert.IsNotNull(resultado);//

            //validar en verdad se inserto
            var contexto2 = ConstruirContext(nombreBD);
            var cantidad = await contexto2.Autores.CountAsync();
            Assert.AreEqual(expected: 1, actual: cantidad); //valida qeu en verdad se inserto
        }

        [TestMethod]
        public async Task Put_Retorna404_CuandoAutorNoExiste()
        {
            //prueba
            var respuesta = await controller.Put(1, autorCreacionDTO: null!);
            ///verficacion
            var resultado = respuesta.Result as StatusCodeResult;
            Assert.AreEqual(404, resultado!.StatusCode);

        }

        private const string contenedor = "autore";

        private const string cache = "autores-obtener";

        [TestMethod]
        public async Task Put_ActualizaAutor_CuandEnviamosAutorSinFoto()
        {
            //preparacion

            var context = ConstruirContext(nombreBD);
            context.Autores.Add(new Autor { Nombres = "Nch", Apellidos = "Ch" ,Identificacion="id"});
            await context.SaveChangesAsync();

            var autorCreaciuonDTO = new AutorCreacionDTOconFoto 
            {
                Nombres = "Nch2",
                Apellidos = "Ch2",
                Identificacion = "id2"
            };

            //prueba
            var respuesta = await controller.Put(1, autorCreacionDTO: autorCreaciuonDTO);
            ///verficacion
            var resultado = respuesta.Result as StatusCodeResult;
            Assert.AreEqual(204, resultado!.StatusCode);


            var context3 = ConstruirContext(nombreBD);
            var autorActuaizado = await context3.Autores.SingleAsync();

            Assert.AreEqual(expected:"Nch2",actual: autorActuaizado.Nombres);
            Assert.AreEqual(expected: "Ch2", actual: autorActuaizado.Apellidos);
            Assert.AreEqual(expected: "id2", actual: autorActuaizado.Identificacion);

            await outputCacheStore.Received().EvictByTagAsync(cache,default);
            await almacenadorArchivos.DidNotReceiveWithAnyArgs().Editar(default,default!,default!);

        }


        [TestMethod]
        public async Task Put_ActualizaAutor_CuandEnviamosAutorConFoto()
        {
            //preparacion

            var context = ConstruirContext(nombreBD);

            var urlAnterior = "URL-1";
            var urlNueva = "URL-2";
            almacenadorArchivos.Editar(default,default,default).ReturnsForAnyArgs(urlNueva);//retorna el argumento a culaquiera

            context.Autores.Add(new Autor 
            { 
                Nombres = "Nch", Apellidos = "Ch", Identificacion = "id" ,
                Foto= urlAnterior

            });
            await context.SaveChangesAsync();

            var formFile = Substitute.For<IFormFile>();


            var autorCreaciuonDTO = new AutorCreacionDTOconFoto
            {
                Nombres = "Nch2",
                Apellidos = "Ch2",
                Identificacion = "id2",
                Foto=formFile
            };

            //prueba
            var respuesta = await controller.Put(1, autorCreacionDTO: autorCreaciuonDTO);
            ///verficacion
            var resultado = respuesta.Result as StatusCodeResult;
            Assert.AreEqual(204, resultado!.StatusCode);


            var context3 = ConstruirContext(nombreBD);
            var autorActualizado = await context3.Autores.SingleAsync();

            Assert.AreEqual(expected: "Nch2", actual: autorActualizado.Nombres);
            Assert.AreEqual(expected: "Ch2", actual: autorActualizado.Apellidos);
            Assert.AreEqual(expected: "id2", actual: autorActualizado.Identificacion);
            Assert.AreEqual(expected: urlNueva, actual: autorActualizado.Foto);

            await outputCacheStore.Received().EvictByTagAsync(cache, default);
            await almacenadorArchivos.Received(1).Editar(urlAnterior, contenedor, formFile);

        }

        [TestMethod]
        public async Task Patch_Retorna400_Cuando_PatchDoc_EsNulo()
        {
            //prueba
            var respuesta = await controller.Patch(1, null!);

            //verificacion
            var resultado=respuesta as StatusCodeResult;

            Assert.AreEqual(400, resultado!.StatusCode);
        }


        [TestMethod]
        public async Task Patch_Retorna404_CuandoAutorNoExiste()
        {
            //preparacion
            var patchDoc = new Microsoft.AspNetCore.JsonPatch.JsonPatchDocument<AutorPatchDTO>();
            
            //prueba
            var respuesta = await controller.Patch(1, patchDoc);

            //verificacion
            var resultado = respuesta as StatusCodeResult;

            Assert.AreEqual(404, resultado!.StatusCode);

        }


        [TestMethod]
        public async Task Patch_RetornaValidationProblem_CuandoHayErrorDeValidacion()
        {
            //preparacion
            var context = ConstruirContext(nombreBD);            
            context.Autores.Add(new Autor
            {
                Nombres = "Nch",
                Apellidos = "Ch",
                Identificacion = "id"
            });
            await context.SaveChangesAsync();

            var objectValidator = Substitute.For<IObjectModelValidator>();
            controller.ObjectValidator = objectValidator;

            var mensajeError = "mensaje de error";
            controller.ModelState.AddModelError("",mensajeError);

            var patchDoc = new Microsoft.AspNetCore.JsonPatch.JsonPatchDocument<AutorPatchDTO>();

            //prueba
            var respuesta = await controller.Patch(1, patchDoc);

            //verificacion
            var resultado = respuesta as ObjectResult;
            var problemDeatails = resultado.Value as ValidationProblemDetails;

            Assert.IsNotNull(problemDeatails);
            Assert.AreEqual(expected:1,actual: problemDeatails.Errors.Keys.Count);

            Assert.AreEqual(expected: mensajeError, actual: problemDeatails.Errors.Values.First().First());

        }

        [TestMethod]
        public async Task Patch_ActualizaUnCampo_CuandoSeLeEnviaUnaOperacion()
        {
            ////preparacion
            //var context = ContruirContext(nombreBD);
            //context.Autores.Add(new Autor
            //{
            //    Nombres = "Nch",
            //    Apellidos = "Ch",
            //    Identificacion = "id",
            //    Foto = "ur1"

            //});
            //await context.SaveChangesAsync();

            //var objectValidator = Substitute.For<IObjectModelValidator>();
            //controller.ObjectValidator = objectValidator;


            //var patchDoc = new Microsoft.AspNetCore.JsonPatch.JsonPatchDocument<AutorPatchDTO>();
            //patchDoc.Operations.Add(new Microsoft.AspNetCore.JsonPatch.Operations.Operation<AutorPatchDTO>("replace", "/nombres", null, "Felipe"));
            ////prueba
            //var respuesta = await controller.Patch(1, patchDoc);

            ////verificacion
            //var resultado = respuesta as StatusCodeResult;


            //Assert.AreEqual(expected: 204, actual: resultado!.StatusCode);
            //await outputCacheStore.Received(1).EvictByTagAsync(cache, default);

            //var contexto2 = ContruirContext(nombreBD);
            //var autorDB = await contexto2.Autores.SingleAsync();

            //Assert.AreEqual(expected: "Felite", actual: autorDB.Nombres);
            //Assert.AreEqual(expected: "Ch", actual: autorDB.Apellidos);
            //Assert.AreEqual(expected: "id", actual: autorDB.Identificacion);
            // Preparación

            var context = ConstruirContext(nombreBD);
            context.Autores.Add(new Autor
            {
                Nombres = "Felipe",
                Apellidos = "Gavilán",
                Identificacion = "123",
                Foto = "URL-1"
            });

            await context.SaveChangesAsync();

            var objectValidator = Substitute.For<IObjectModelValidator>();
            controller.ObjectValidator = objectValidator;

            var patchDoc = new JsonPatchDocument<AutorPatchDTO>();
            patchDoc.Replace(p => p.Nombres, "Felipe2");
            //patchDoc.Operations.Add(new Operation<AutorPatchDTO>("replace",
            //                                                     "/nombres",
            //                                                     null,
            //                                                     "Felipe2"));

            // Prueba
            var respuesta = await controller.Patch(1, patchDoc);

            // Verificación
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(expected: 204, resultado!.StatusCode);

            await outputCacheStore.Received(1).EvictByTagAsync(cache, default);

            var context2 = ConstruirContext(nombreBD);
            var autorBD = await context2.Autores.SingleAsync();

            Assert.AreEqual(expected: "Felipe2", autorBD.Nombres);
            Assert.AreEqual(expected: "Gavilán", autorBD.Apellidos);
            Assert.AreEqual(expected: "123", autorBD.Identificacion);
            Assert.AreEqual(expected: "URL-1", autorBD.Foto);
        }

        [TestMethod]
        public async Task Delete_Retornar404_CuandoAutorNoExiste()
        {
            // Prueba
            var respuesta = await controller.Delete(1);

            // Verificación
            var resultado = respuesta.Result as StatusCodeResult;
            Assert.AreEqual(404, resultado!.StatusCode);
        }

        [TestMethod]
        public async Task Delete_BorraAutor_CuandoAutorExiste()
        {
            // Preparación
            var urlFoto = "URL-1";

            var context = ConstruirContext(nombreBD);
                          

            context.Autores.Add(new Autor { Nombres = "Autor1", Apellidos = "Autor1", Foto = urlFoto });
            context.Autores.Add(new Autor { Nombres = "Autor2", Apellidos = "Autor2" });

            await context.SaveChangesAsync();

            // Prueba
            var respuesta = await controller.Delete(1);

            // Verificación
            var resultado = respuesta.Result as StatusCodeResult;
            Assert.AreEqual(204, resultado!.StatusCode);

            var context2 = ConstruirContext(nombreBD);
            var cantidadAutores = await context2.Autores.CountAsync();
            Assert.AreEqual(expected: 1, actual: cantidadAutores);

            var autor2Existe = await context2.Autores.AnyAsync(x => x.Nombres == "Autor2");
            Assert.IsTrue(autor2Existe);

            await outputCacheStore.Received(1).EvictByTagAsync(cache, default);
            await almacenadorArchivos.Received(1).Borrar(urlFoto, contenedor);
        }


    }
}
