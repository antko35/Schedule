using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagementService.Domain.Models;

namespace UserManagementService.Application.Extensions
{
    public static class EntityExtensions
    {
        public static void EnsureExists<T>(this T? entity, string errorMessage) where T : Entity
        {
            if (entity is null)
            {
                throw new KeyNotFoundException(errorMessage);
            }
        }
    }
}
