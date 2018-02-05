using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TodoAPI.Controllers.Resources
{
    public class ValidationErrorResource
    {
        public string Message { get; set; }
        public IEnumerable<string> Errors { get; set; }

        public ValidationErrorResource(ModelStateDictionary modelState)
        {
            Message = "Validation Failed";
            Errors = modelState.Keys
                .SelectMany(key => modelState[key].Errors.Select(x => x.ErrorMessage))
                .ToList();
        }
    }
}