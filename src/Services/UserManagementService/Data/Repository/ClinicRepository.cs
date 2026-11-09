namespace UserManagementService.DataAccess.Repository;

using Microsoft.EntityFrameworkCore;
using UserManagementService.DataAccess.Database;
using UserManagementService.Domain.Abstractions.IRepository;
using UserManagementService.Domain.Models;

public class ClinicRepository(UserManagementDbContext context)
    : GenericRepository<Clinic>(context), IClinicRepository;
