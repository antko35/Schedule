using System.Globalization;
using System.Reflection;
using MediatR;
using OfficeOpenXml;
using ScheduleService.Application.UseCases.Commands.Schedule;
using ScheduleService.DataAccess.Repository;
using ScheduleService.Domain.Abstractions;
using DomainModels = ScheduleService.Domain.Models;

namespace ScheduleService.Application.UseCases.CommandHandlers.Schedule;

public class ExportScheduleCommandHandler : IRequestHandler<ExportScheduleCommand, byte[]>
{
    private const int FirstEmployeeRow = 9;
    private const int RowsPerEmployee = 3;
    private const int TemplateEmployeeSlots = 1;
    private const int FirstDayColumn = 2;   // column B
    private const int LastDayColumn = 32;   // column AF (day 31)
    private const int DaysCountColumn = 33; // column AG
    private const int HoursTotalColumn = 34; // column AH

    private static readonly string[] RussianMonths =
    [
        "январь", "февраль", "март", "апрель", "май", "июнь",
        "июль", "август", "сентябрь", "октябрь", "ноябрь", "декабрь",
    ];

    private readonly IScheduleRepository scheduleRepository;
    private readonly IUserRuleRepository userRuleRepository;

    public ExportScheduleCommandHandler(
        IScheduleRepository scheduleRepository,
        IUserRuleRepository userRuleRepository)
    {
        this.scheduleRepository = scheduleRepository;
        this.userRuleRepository = userRuleRepository;
    }

    public async Task<byte[]> Handle(ExportScheduleCommand request, CancellationToken cancellationToken)
    {
        var scheduleMap = await this.BuildScheduleMap(request);

        var templateBytes = GetTemplateBytes();
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var ms = new MemoryStream(templateBytes);
        using var package = new ExcelPackage(ms);
        var ws = package.Workbook.Worksheets[0];

        this.FillHeaders(ws, request);

        int userCount = request.Users.Count;
        int extraRowsAdded = this.EnsureEmployeeRows(ws, userCount);

        for (int i = 0; i < userCount; i++)
        {
            var user = request.Users[i];
            int nameRow = FirstEmployeeRow + (i * RowsPerEmployee);
            int startRow = nameRow + 1;
            int endRow = nameRow + 2;

            scheduleMap.TryGetValue(user.UserId, out var schedule);
            this.FillEmployeeBlock(ws, nameRow, startRow, endRow, user.FullName, schedule);
        }

        this.FillFooter(ws, extraRowsAdded, request.Year);

        return await package.GetAsByteArrayAsync(cancellationToken);
    }

    private async Task<Dictionary<string, DomainModels.Schedule>> BuildScheduleMap(ExportScheduleCommand request)
    {
        var monthName = new DateOnly(request.Year, request.Month, 1)
            .ToString("MMMM", CultureInfo.InvariantCulture)
            .ToLower();

        IEnumerable<DomainModels.UserScheduleRules> usersRules;
        try
        {
            usersRules = await this.userRuleRepository.GetUsersRulesByDepartment(
                request.DepartmentId, monthName, request.Year);
        }
        catch (InvalidOperationException)
        {
            return new Dictionary<string, DomainModels.Schedule>();
        }

        var map = new Dictionary<string, DomainModels.Schedule>();
        foreach (var rule in usersRules)
        {
            var schedule = await this.scheduleRepository.GetMonthSchedule(rule.ScheduleId);
            if (schedule is not null)
                map[rule.UserId] = schedule;
        }

        return map;
    }

    private void FillHeaders(ExcelWorksheet ws, ExportScheduleCommand request)
    {
        ws.Cells[4, 1].Value = request.DepartmentName;

        var titleCell = ws.Cells[5, 1];
        titleCell.Value = titleCell.Text
            .Replace("{MONTH}", RussianMonths[request.Month - 1])
            .Replace("{YEAR}", request.Year.ToString());
    }

    private int EnsureEmployeeRows(ExcelWorksheet ws, int userCount)
    {
        if (userCount <= TemplateEmployeeSlots)
            return 0;

        int extraEmployees = userCount - TemplateEmployeeSlots;
        int insertBase = FirstEmployeeRow + (TemplateEmployeeSlots * RowsPerEmployee);

        for (int e = 0; e < extraEmployees; e++)
        {
            int insertAt = insertBase + (e * RowsPerEmployee);
            ws.InsertRow(insertAt, 1, FirstEmployeeRow);
            ws.InsertRow(insertAt + 1, 1, FirstEmployeeRow + 1);
            ws.InsertRow(insertAt + 2, 1, FirstEmployeeRow + 2);
        }

        return extraEmployees * RowsPerEmployee;
    }

    private void FillEmployeeBlock(
        ExcelWorksheet ws,
        int nameRow,
        int startRow,
        int endRow,
        string fullName,
        DomainModels.Schedule? schedule)
    {
        ws.Cells[nameRow, 1].Value = fullName;
        ws.Cells[startRow, 1].Value = string.Empty;
        ws.Cells[endRow, 1].Value = "1.0 ст.";

        for (int col = FirstDayColumn; col <= LastDayColumn; col++)
        {
            ws.Cells[nameRow, col].Value = null;
            ws.Cells[startRow, col].Value = null;
            ws.Cells[endRow, col].Value = null;
        }

        ws.Cells[nameRow, DaysCountColumn].Value = null;
        ws.Cells[nameRow, HoursTotalColumn].Value = null;

        if (schedule is null)
            return;

        double totalDays = 0;
        double totalHours = 0;

        foreach (var workDay in schedule.WorkDays)
        {
            int col = workDay.Day + 1;
            if (col < FirstDayColumn || col > LastDayColumn)
                continue;

            double startFraction = workDay.StartTime.TimeOfDay.TotalDays;
            double endFraction = workDay.EndTime.TimeOfDay.TotalDays;
            double hoursFraction = endFraction - startFraction;

            ws.Cells[nameRow, col].Value = startFraction;
            ws.Cells[startRow, col].Value = endFraction;
            ws.Cells[endRow, col].Value = hoursFraction * 24;

            totalDays++;
            totalHours += hoursFraction * 24;
        }

        ws.Cells[nameRow, DaysCountColumn].Value = totalDays;
        ws.Cells[nameRow, HoursTotalColumn].Value = totalHours;
    }

    private void FillFooter(ExcelWorksheet ws, int rowOffset, int year)
    {
        int footerRow21 = 15 + rowOffset;
        int footerRow29 = 23 + rowOffset;

        var createdByCell = ws.Cells[footerRow21, 1];
        if (createdByCell.Text.Contains("{WhoCreate}"))
            createdByCell.Value = createdByCell.Text.Replace("{WhoCreate}", string.Empty);

        var dateCell = ws.Cells[footerRow29, 1];
        if (dateCell.Text.Contains("{year}"))
            dateCell.Value = dateCell.Text.Replace("{year}", year.ToString());
    }

    private static byte[] GetTemplateBytes()
    {
        var assembly = Assembly.GetExecutingAssembly();
        const string ResourceName = "ScheduleService.Application.Resources.scheduleTemplate.xlsx";
        using var stream = assembly.GetManifestResourceStream(ResourceName)
            ?? throw new FileNotFoundException("Excel template not found.", ResourceName);
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
}
