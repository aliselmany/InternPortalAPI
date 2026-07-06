using System;

namespace InternPortal.Domain.Entities
{
    public class TaskActivityLog
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public string UserId { get; set; }
        //TODO: UserName'ı User entity'sinden alabiliriz.
        public string UserName { get; set; }

        //TODO: ActionType için enum oluşturabiliriz.
        public string ActionType { get; set; }

        public string Details { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}