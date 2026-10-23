namespace ScheduleService.Application.Extensions
{
    using ScheduleService.Domain.Models;

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
