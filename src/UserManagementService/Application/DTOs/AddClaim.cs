using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace UserManagementService.Application.DTOs
{
    public class AddClaim
    {
        public string Email { get; set; }

        public string Clinic { get; set; }

        public string Permission {get; set; }

    }
}
