using MediatR;

namespace ScheduleService.Application.UseCases.Commands.Schedule;

public class ExportScheduleCommand : IRequest<byte[]>
{
    public string DepartmentId { get; set; } = string.Empty;

    public string DepartmentName { get; set; } = string.Empty;

    public int Year { get; set; }

    public int Month { get; set; }

    public List<ExportUserInfo> Users { get; set; } = new();
}

public class ExportUserInfo
{
    public string UserId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;
}
