﻿namespace ScheduleService.API.Extensions
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
            string source;
            Dictionary<string, string[]>? errors = null;

            switch (exception)
            {
                case ArgumentNullException argumentNull:
                    status = HttpStatusCode.NotFound;
                    message = argumentNull.Message;
                    source = exception.Message;
                    break;

                case InvalidOperationException invalidOperation:
                    status = HttpStatusCode.NotFound;
                    message = invalidOperation.Message;
                    source = invalidOperation.Source;
                    break;

                case KeyNotFoundException keyNotFound:
                    status = HttpStatusCode.NotFound;
                    message = keyNotFound.Message;
                    source = keyNotFound.Source;
                    break;

                case Application.Extensions.Validation.ValidationException validationException:
                    status = HttpStatusCode.BadRequest;
                    message = validationException.Message;
                    errors = (Dictionary<string, string[]>)validationException.Errors;
                    source = validationException.Source;
                    break;

                default:
                    status = HttpStatusCode.InternalServerError;
                    message = exception.Message;
                    source = exception.Source;
                    break;
            }

            var result = JsonSerializer.Serialize(new
            {
                error = message,
                errors,
                source = source,
            });
            context.Response.StatusCode = (int)status;

            return context.Response.WriteAsync(result);
        }
    }
}
