using MasterMind_Client.TempData.Enum;

namespace MasterMind_Client.TempData.DTO
{
    public class PlayerReportDto
    {
        public int ReportId { get; set; }
        public string ReportedPlayerUsername { get; set; }
        public string ReportedByPlayerUsername { get; set; }
        public ReportReasonEnum Reason { get; set; }
    }
}
