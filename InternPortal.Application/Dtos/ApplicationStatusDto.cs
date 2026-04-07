
using InternPortal.Domain.Enums;
using System.Text.Json.Serialization;

namespace InternPortal.Application.Dtos
{
    public class ApplicationStatusDto
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ApplicationStatus Status { get; set; }
    }
}