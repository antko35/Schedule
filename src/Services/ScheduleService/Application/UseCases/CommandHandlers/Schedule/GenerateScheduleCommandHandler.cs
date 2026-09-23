// <copyright file="GenerateScheduleCommandHandler.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ScheduleService.Application.UseCases.CommandHandlers.Schedule;

using MediatR;
using Extensions;
using Services;
using ScheduleService.Application.UseCases.Commands.Schedule;
using DataAccess.Repository;
using Domain.Abstractions;

public class GenerateScheduleCommandHandler : IRequestHandler<GenerateScheduleCommand>
{
    private readonly IUserRuleRepository userRuleRepository;
    private readonly IScheduleRepository scheduleRepository;
    private readonly ICalendarRepository calendarRepository;

    public GenerateScheduleCommandHandler(
        IUserRuleRepository userRuleRepository,
        IScheduleRepository scheduleRepository,
        ICalendarRepository calendarRepository)
    {
        this.userRuleRepository = userRuleRepository;
        this.scheduleRepository = scheduleRepository;
        this.calendarRepository = calendarRepository;
    }

    public async Task Handle(GenerateScheduleCommand request, CancellationToken cancellationToken)
    {
        var monthName = new DateTime(request.Year, request.Month, 1)
            .ToString("MMMM")
            .ToLower();

        var getRulesTask = userRuleRepository.GetMonthScheduleRules(
            request.UserId,
            request.DepartmentId,
            monthName,
            request.Year);
        var getHolidaysTask = calendarRepository.GetMonthHolidays(request.Year, request.Month);
        var getTransferDaysTask = calendarRepository.GetMonthTransferDays(request.Year, request.Month);

        await Task.WhenAll(getRulesTask, getHolidaysTask, getTransferDaysTask);

        var userRules = await getRulesTask;
        userRules.EnsureExists("Schedule rules for this user not found");

        var generatedDays = ScheduleGenerator.GenerateWorkDaysForUser(
            userRules,
            request.Year,
            request.Month,
            await getHolidaysTask,
            await getTransferDaysTask);

        await scheduleRepository.ClearMonthSchedule(userRules.ScheduleId);

        foreach (var workDay in generatedDays)
        {
            await scheduleRepository.AddWorkDayAsync(userRules.ScheduleId, workDay);
        }
    }
}
