namespace UserManagementService.API.Extensions
{
    using System.ComponentModel;
    using System.Net;
    using System.Text.Json;
    using UserManagementService.Application.Extensions.Validation;

    public class ExeptionHadlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ExeptionHadlingMiddleware> logger;

        public ExeptionHadlingMiddleware(RequestDelegate next, ILogger<ExeptionHadlingMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Request failed; committed database changes are not automatically rolled back after external publication failures.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            HttpStatusCode status;
            string message;
            string stackTrace = null;
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

                case ValidationException validationException:
                    status = HttpStatusCode.BadRequest;
                    message = validationException.Message;
                    errors = (Dictionary<string, string[]>)validationException.Errors;
                    break;


                default:
                    status = HttpStatusCode.InternalServerError;
                    message = exception.Message;
                    stackTrace = exception.StackTrace;
                    break;
            }

            var result = JsonSerializer.Serialize(new
            {
                error = message,
                errors,
                stackTrace,
            });
            context.Response.StatusCode = (int)status;

            return context.Response.WriteAsync(result);
        }
    }
}
