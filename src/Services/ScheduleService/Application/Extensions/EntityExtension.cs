using ScheduleService.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleService.Application.Extensions
{
    public static class EntityExtension
    {
        public static void EnsureExists<T>(this T? entity, string errorMessage)
            where T : Entity
        {
            if (entity is null)
            {
                throw new KeyNotFoundException(errorMessage);
            }
        }
    }
}
