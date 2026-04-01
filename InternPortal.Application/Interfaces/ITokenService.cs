using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace InternPortal.Application.Interfaces
{
    public interface ITokenService
    {
       string GenerateToken(Domain.Entities.User user);
    }
}
