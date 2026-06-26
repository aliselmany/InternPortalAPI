using System;

namespace InternPortal.Application.Dtos
{
    public class CreateKanbanTaskDto
    {
        public string Title { get; set; }
        public Guid InternId { get; set; }
        public Guid StaffId { get; set; }
    }

    public class MoveKanbanTaskDto
    {
        public int TaskId { get; set; }
        public string NewStatus { get; set; }
        public int NewOrderIndex { get; set; }
    }

    public class UpdateKanbanTaskDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }
}