using System;
using System.Collections.Generic;
using System.Text;

namespace InternPortal.Domain.Entities
{
    public class Role
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty; 

        public ICollection<UserRoleMapping> UserRoles { get; set; } = new List<UserRoleMapping>();
    }
}