using System;
using System.Collections.Generic;
using System.Text;

namespace InternPortal.Application.Dtos
{
    public class RoleResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
