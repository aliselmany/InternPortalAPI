using InternPortal.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace InternPortal.Application.Dtos
{
    public class ApplicationDto
    {
        [Required(ErrorMessage = "School name is required.")]
        [Display(Name = "School Name")]
        public string SchoolName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select your grade.")]
        public StudentGrade Grade { get; set; } 

        [Required(ErrorMessage = "Please select a department.")]
        public Department SelectedDepartment { get; set; } 

        [Required(ErrorMessage = "Please select internship type.")]
        public InternshipType InternshipType { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^5\d{9}$", ErrorMessage = "Phone number must be 10 digits and start with 5.")]
        [MaxLength(10), MinLength(10)] 
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start date is required.")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } 

        [Required(ErrorMessage = "End date is required.")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } 

        public string? Description { get; set; }

        [Required(ErrorMessage = "Please upload your CV file.")]
        public required IFormFile CvFile { get; set; } 


    }
}