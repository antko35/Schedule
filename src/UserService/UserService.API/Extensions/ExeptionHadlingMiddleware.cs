namespace UserService.API.Extensions
{
    using System.Net;
    using System.Text.Json;

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
                await this.next(context);
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

                default:
                    status = HttpStatusCode.InternalServerError;
                    message = exception.Message;
                    break;
            }

            var result = JsonSerializer.Serialize(new { error = message });
            context.Response.StatusCode = (int)status;

            return context.Response.WriteAsync(result);
        }
    }
}
