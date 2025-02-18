namespace UserManagementService.DataAccess.Repository;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagementService.DataAccess.Database;
using UserManagementService.Domain.Abstractions.IRepository;
using UserManagementService.Domain.Models;

public class ClinicRepository : GenericRepository<Clinic>, IClinicRepository
{
    public ClinicRepository(DbContext context, BaseDbOptions options) : base(context, options.ClinicsCollectionName)
    {
    }
}
