using FluentValidation;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagementService.Application.Extensions.Validation;
using UserManagementService.Application.UseCases.Queries.User;

namespace UserManagementService.Application.UseCases.Validators.Queries.User
{
    public sealed class GetFullUserInfoQueryValidator
        : AbstractValidator<GetFullUserInfoQuery>
    {
        public GetFullUserInfoQueryValidator()
        {
            RuleFor(x => x.UserId)
                .MustBeValidObjectId();
        }
    }
}
