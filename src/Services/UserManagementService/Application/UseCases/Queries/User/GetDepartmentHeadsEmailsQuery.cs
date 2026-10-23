using MediatR;

namespace UserManagementService.Application.UseCases.Queries.User;

public record GetDepartmentHeadsEmailsQuery(IEnumerable<string> DepartmentsIds)
    : IRequest<List<string>>;