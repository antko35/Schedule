using AutoMapper;
using ScheduleService.Application.UseCases.Commands.Calendar;
using ScheduleService.Domain.Models;

namespace ScheduleService.Application.Mapping
{
    public class CalendarMapping : Profile
    {
        public CalendarMapping()
        {
            CreateMap<AddOfficialHolidayCommand, Calendar>()
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Holiday.Year))
                .ForMember(dest => dest.HolidayDayOfMonth, opt => opt.MapFrom(src => src.Holiday.Day))
                .ForMember(dest => dest.DayOfWeek, opt => opt.MapFrom(src => src.Holiday.DayOfWeek))
                .ForMember(dest => dest.HolidayDate, opt => opt.MapFrom(src => src.Holiday))
                .ForMember(dest => dest.MonthOfHoliday, opt => opt.MapFrom(src => src.Holiday.Month))
                .ForMember(dest => dest.MonthOfTransferDay, opt => opt.MapFrom(src => src.TransferDay.Month))
                .AfterMap((src, dest) =>
                {
                    if (src.TransferDay.Year == 0001)
                    {
                        dest.TransferDate = null;
                    }
                    else
                    {
                        dest.TransferDate = src.TransferDay;
                    }
                });
        }
    }
}