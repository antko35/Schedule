using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleService.Application.Extensions
{
    public sealed class ValidationBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : class
    {
        private readonly IEnumerable<IValidator<TRequest>> validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            this.validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (!validators.Any())
            {
                return await next();
            }

            var context = new ValidationContext<TRequest>(request);
            var errorsDictionary = validators
                .Select(x => x.Validate(context))
                .SelectMany(x => x.Errors)
                .Where(x => x != null)
                .GroupBy(
                    x => x.PropertyName,
                    x => x.ErrorMessage,
                    (propertyName, errorMessages) => new
                    {
                        Key = propertyName,
                        Values = errorMessages.Distinct().ToArray()
                    })
                .ToDictionary(x => x.Key, x => x.Values);

            if (errorsDictionary.Any())
            {
                throw new ValidationException(errorsDictionary);
            }

            return await next();
        }
    }
}
