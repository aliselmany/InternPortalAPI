using System;

namespace InternPortal.Domain.Entities
{
    public class TaskActivityLog
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }

        public string ActionType { get; set; }

        public string Details { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}