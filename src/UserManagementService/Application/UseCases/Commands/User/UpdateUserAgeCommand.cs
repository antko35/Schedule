﻿using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagementService.Application.UseCases.Commands.User
{
    public record UpdateUserAgeCommand : IRequest
    {
    }
}
