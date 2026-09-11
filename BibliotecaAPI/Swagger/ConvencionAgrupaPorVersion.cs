using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace BibliotecaAPI.Swagger
{
    public class ConvencionAgrupaPorVersion : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            //Ejemplo 1:"Controle V1"
            var nameSpaceDelControlador = controller.ControllerType.Namespace;
            var version=nameSpaceDelControlador!.Split(".").Last().ToLower();//obtiene el ultimo de la cadena (V?)
            controller.ApiExplorer.GroupName = version;

        }
    }
}
