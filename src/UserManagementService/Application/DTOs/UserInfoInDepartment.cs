namespace UserManagementService.Application.DTOs;

using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagementService.Domain.Models;

public class UserInfoInDepartment : User
{
    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public string? Role { get; set; }

    public string? Status { get; set; }

    public UserInfoInDepartment(User user, UserJob userJob)
    {
        Id = user.Id;
        FirstName = user.FirstName;
        LastName = user.LastName;
        Patronymic = user.Patronymic;
        Role = userJob.Role;
        Gender = user.Gender;
        Age = user.Age;
        PhoneNumber = userJob.PhoneNumber;
        Email = userJob.Email;
        Status = userJob.Status;
    }
}
