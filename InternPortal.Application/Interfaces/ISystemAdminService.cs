using System.Collections.Generic;
using System.Threading.Tasks;

namespace InternPortalAPI.Services.Interfaces
{
    public interface ISystemAdminService
    {
        Task<object> GetDashboardSummaryAsync();
        Task<IEnumerable<object>> GetCalendarEventsAsync();
        Task<IEnumerable<object>> GetGlobalLogsAsync();
    }
}