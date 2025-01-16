namespace UserManagementService.Application.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UserManagementService.Domain.Models;

    public class AllUserInfo : User
    {
        public IEnumerable<UserJob> Jobs { get; set; } = new List<UserJob>();
    }
}
