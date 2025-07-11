﻿using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using UserManagementService.Application.Extensions.Mapping;
using UserManagementService.Application.Extensions.Validation;

namespace UserManagementService.Application.Extensions;
public static class DependencyInjection
{
    public static IServiceCollection AddApplicatiobLayerDependencis(this IServiceCollection services)
    {
        services.AddMediatR(cnf => cnf.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddAutoMapper(typeof(UserProfile).Assembly);

        return services;
    }
}
