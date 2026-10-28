namespace ScheduleService.Domain.Models;

public sealed class CalendarException : Entity
{
    public CalendarException(string id, DateOnly holidayDate, DateOnly? transferDate = null)
        : base(id)
    {
        HolidayDate = holidayDate;
        TransferDate = transferDate;
    }

    private CalendarException()
    {
    }

    public DateOnly HolidayDate { get; private set; }

    public DateOnly? TransferDate { get; private set; }

    public void SetTransferDate(DateOnly? transferDate)
    {
        TransferDate = transferDate;
    }
}
