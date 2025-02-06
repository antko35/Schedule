using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleService.Application.Extensions
{
    public class ValidationException : Exception
    {
        public IReadOnlyDictionary<string, string[]> Errors { get; }

        public ValidationException(IReadOnlyDictionary<string, string[]> errors)
            : base("One or more validation errors occurred")
        {
            Errors = errors;
        }
    }
}
