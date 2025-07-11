﻿namespace UserManagementService.Application.UseCases.Commands.Department;

using MediatR;
using UserManagementService.Domain.Models;

public record CreateDepartmentCommand(
    string DeartmentName,
    string? ClinicId)
    : IRequest<Department>;
