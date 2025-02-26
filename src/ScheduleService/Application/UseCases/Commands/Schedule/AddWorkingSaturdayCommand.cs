using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScheduleService.Domain.Models;

namespace ScheduleService.Application.UseCases.Commands.Schedule
{
    public class AddWorkingSaturdayCommand
        : IRequest
    {
        public string ScheduleId { get; set; }

        public WorkDay Saturday { get; set; }

        public List<WorkDay> WorkingDays { get; set; } = new List<WorkDay>();

        public WorkDay DayOne { get; set; }

        public WorkDay DayTwo { get; set; }

        public WorkDay DayThree { get; set; }

        public WorkDay DayFour { get; set; }

        public WorkDay DayFive { get; set; }
    }
}
