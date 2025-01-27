namespace ScheduleService.API.Extensions
{
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class ExeptionHadlingMiddleware
    {
        private readonly RequestDelegate next;

        public ExeptionHadlingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            HttpStatusCode status;
            string message;
            Dictionary<string, string[]>? errors = null;

            switch (exception)
            {
                case ArgumentNullException argumentNull:
                    status = HttpStatusCode.NotFound;
                    message = argumentNull.Message;
                    break;

                case InvalidOperationException invalidOperation:
                    status = HttpStatusCode.NotFound;
                    message = invalidOperation.Message;
                    break;

                case KeyNotFoundException keyNotFound:
                    status = HttpStatusCode.NotFound;
                    message = keyNotFound.Message;
                    break;

                //case ValidationException validationException:
                //    status = HttpStatusCode.BadRequest;
                //    message = validationException.Message;
                //    errors = (Dictionary<string, string[]>)validationException.Errors;
                //    break;

                default:
                    status = HttpStatusCode.InternalServerError;
                    message = exception.Message;
                    break;
            }

            var result = JsonSerializer.Serialize(new
            {
                error = message,
                errors,
            });
            context.Response.StatusCode = (int)status;

            return context.Response.WriteAsync(result);
        }
    }
}
