using System;
using System.Collections.Generic;
using System.Text;

namespace InternPortal.Domain.Entities
{
    public class Role
    {
        public Guid Id { get; set; }
        // TODO: Roller enum olacak, namei kaldır.
        public string Name { get; set; } = string.Empty; 
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}