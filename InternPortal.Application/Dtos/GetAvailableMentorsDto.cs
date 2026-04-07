using System;
using System.Collections.Generic;
using System.Text;

namespace InternPortal.Application.Dtos
{
    public class GetAvailableMentorsDto
    {
        public Guid? UserId { get; set; } 
        public string? FullName { get; set; }
        public string? Expertise { get; set; } 
    }
}
