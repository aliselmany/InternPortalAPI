using System;
using System.Collections.Generic;
using System.Text;

namespace InternPortal.Application.Dtos;

public class ApplicationFilterQuery
{
    public string? Name { get; set; }
    public string? SchoolName { get; set; }
    public string? DepartmentOfStudy { get; set; }
    public int? InternshipType { get; set; } 
    public string? Status { get; set; }
}