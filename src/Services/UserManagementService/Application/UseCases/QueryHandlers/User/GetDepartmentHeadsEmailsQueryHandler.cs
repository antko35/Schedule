using MediatR;
using UserManagementService.Application.UseCases.Queries.User;
using UserManagementService.Domain.Abstractions.IRepository;

namespace UserManagementService.Application.UseCases.QueryHandlers.User;

public class GetDepartmentHeadsEmailsQueryHandler : IRequestHandler<GetDepartmentHeadsEmailsQuery, List<string>>
{
    private readonly IUserJobsRepository userJobsRepository;

    public GetDepartmentHeadsEmailsQueryHandler(IUserJobsRepository userJobsRepository)
    {
        this.userJobsRepository = userJobsRepository;
    }

    public async Task<List<string>> Handle(GetDepartmentHeadsEmailsQuery request, CancellationToken cancellationToken)
    {
        var emails = await userJobsRepository.GetHeadEmails(request.DepartmentsIds);

        return emails;
    }
}