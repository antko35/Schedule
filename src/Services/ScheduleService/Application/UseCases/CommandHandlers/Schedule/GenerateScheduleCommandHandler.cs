// <copyright file="GenerateScheduleCommandHandler.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ScheduleService.Application.UseCases.CommandHandlers.Schedule;

using MediatR;
using Extensions;
using Extensions.Validation;
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
        EnsureRulesCanGenerate(userRules);

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

    private static void EnsureRulesCanGenerate(Domain.Models.UserScheduleRules rules)
    {
        var errors = new Dictionary<string, string[]>();
        if (rules.HoursPerMonth <= 0)
        {
            errors[nameof(rules.HoursPerMonth)] = new[] { "Hours per month must be greater than zero." };
        }

        if (rules.MaxHoursPerDay <= 0)
        {
            errors[nameof(rules.MaxHoursPerDay)] = new[] { "Hours per day must be greater than zero." };
        }

        if (rules.HoursPerMonth > 0 && rules.MaxHoursPerDay > 0
            && rules.HoursPerMonth / rules.MaxHoursPerDay is < 15 or > 23)
        {
            errors["WorkDays"] = new[] { "The generated work day count must be between 15 and 23." };
        }

        if (!rules.OnlyFirstShift && !rules.OnlySecondShift && !rules.EvenDOW
            && !rules.UnEvenDOW && !rules.EvenDOM && !rules.UnEvenDOM)
        {
            errors["Periodicity"] = new[] { "An automatic schedule periodicity must be selected." };
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }
}
