using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleService.Application.UseCases.Commands.Schedule
{
    public class AddWorkingSaturdayCommand
        : IRequest
    {
        public DateTime Saturday { get; set; }

        public DateTime DayOne { get; set; }

        public DateTime DayTwo { get; set; }

        public DateTime DayThree { get; set; }

        public DateTime DayFour { get; set; }

        public DateTime DayFive { get; set; }
    }
}
