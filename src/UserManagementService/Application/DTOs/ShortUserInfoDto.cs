using MediatR;

namespace UserManagementService.Application.DTOs;

public class ShortUserInfoDto : IRequest
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Patronymic { get; set; }
}