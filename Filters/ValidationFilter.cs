using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TodoAPI.Controllers.Resources;

namespace SimpleTodoServer.Filters
{
    public class ValidationFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if(!context.ModelState.IsValid) {
                context.Result = new BadRequestObjectResult(new ValidationErrorResource(context.ModelState));
            }
        }
    }
}