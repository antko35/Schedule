using MediatR;

namespace ScheduleService.Application.UseCases.Commands.Schedule;

public class ExportScheduleCommand(Guid DepartmentId) : IRequest<byte[]>;