using System;
using System.Text;
namespace InternPortal.Domain.Entities 
{
    public class KanbanTask
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; }
        public int OrderIndex { get; set; }
        public Guid InternId { get; set; }
        public Guid StaffId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? DueDate { get; set; }
    }
}