using MediatR;
//using OfficeOpenXml;
using ScheduleService.Application.UseCases.Commands.Schedule;
using ScheduleService.Domain.Abstractions;

namespace ScheduleService.Application.UseCases.CommandHandlers.Schedule;

public class ExportScheduleCommandHandler : IRequestHandler<ExportScheduleCommand, byte[]>
{
    public ExportScheduleCommandHandler(IScheduleRepository scheduleRepository)
    {
        
    }

    public async Task<byte[]> Handle(ExportScheduleCommand request, CancellationToken cancellationToken)
    {
        /*using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Sheet1");
            worksheet.Cells[1, 1].Value = "Hello";
            worksheet.Cells[1, 2].Value = "World";

            return await package.GetAsByteArrayAsync(cancellationToken);
        }*/
        return Array.Empty<byte>(); 
    }
}