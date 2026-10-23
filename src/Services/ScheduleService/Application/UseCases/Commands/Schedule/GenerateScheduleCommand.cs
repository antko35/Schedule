// <copyright file="GenerateScheduleCommand.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;
using MediatR;

namespace ScheduleService.Application.UseCases.Commands.Schedule;

public record GenerateScheduleCommand(
    string DepartmentId,
    int Year,
    int Month)
    : IRequest
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;
}
