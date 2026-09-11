using Microsoft.AspNetCore.Authorization;

namespace BibliotecaApiTest.Utilidades
{
    public class AllowAnonymousHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            foreach (var requeriment in context.PendingRequirements)
            {
                context.Succeed(requeriment);
            }
            return Task.CompletedTask;
        }
    }
}
