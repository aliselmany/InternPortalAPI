using System;
using System.Collections.Generic;
using System.Text;

namespace InternPortal.Application.Dtos
{
    public class GetUserFilterDto
    {
        public Guid? UserId { get; set; }
        public Guid? RoleId { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
    }
}
