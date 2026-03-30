using System;
using System.Collections.Generic;
using System.Text;

namespace InternPortal.Application.Dtos;

public class AssignMentorRequest
{
    public Guid InternId { get; set; }
    public Guid MentorId { get; set; }
}