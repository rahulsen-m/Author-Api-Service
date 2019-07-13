using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Library.API.Helpers
{
    public class UnProcessableEntityObjectResult : ObjectResult
    {
        // get the model state result and SerializableError helps to make key value pair of 
        public UnProcessableEntityObjectResult(ModelStateDictionary modelState) : base(new SerializableError(modelState))
        {
            if (modelState == null)
            {
                throw new ArgumentNullException(nameof(modelState));
            }
            StatusCode = 422;
        }
    }
}