using MediatR;
using ScheduleService.Application.UseCases.Commands.Schedule;
using ScheduleService.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleService.Application.UseCases.CommandHandlers.Schedule
{
    public class AddWorkingSaturdayCommandHandler
        : IRequestHandler<AddWorkingSaturdayCommand>
    {
        private readonly IScheduleRepository scheduleRepository;

        public AddWorkingSaturdayCommandHandler(IScheduleRepository scheduleRepository)
        {
            this.scheduleRepository = scheduleRepository;
        }

        public Task Handle(AddWorkingSaturdayCommand request, CancellationToken cancellationToken)
        {
            bool isAutoDistribution = IsPossibleAutoDistribution(request.Saturday.Day);

            if (isAutoDistribution)
            {
                
            }

            throw new NotImplementedException();
        }

        private bool IsPossibleAutoDistribution(int day)
        {
            if (day >= 6)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
