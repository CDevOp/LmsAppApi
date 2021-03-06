using System;
using System.Security.Claims;
using System.Threading.Tasks;
using LmsApp.API.Data;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace LmsApp.API.Helpers
{
    public class LogUsersActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();

            var userId = int.Parse(resultContext.HttpContext.User
                .FindFirst(ClaimTypes.NameIdentifier).Value);

            var repo = resultContext.HttpContext.RequestServices.GetService<ILmsRepository>();

            var user = await repo.GetUser(userId, true);

            user.LastActive = DateTime.Now;

            await repo.SaveAll();
        }
    }
}