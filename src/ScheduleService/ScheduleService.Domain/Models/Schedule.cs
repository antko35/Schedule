using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleService.Domain.Models
{
        public class Schedule : Entity
        {
            public List<WorkDay> WorkDays = new List<WorkDay>();
        }
}
