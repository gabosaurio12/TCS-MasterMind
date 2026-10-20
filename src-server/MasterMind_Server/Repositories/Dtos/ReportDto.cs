using MasterMind_Server.Repositories.Enums;

namespace MasterMind_Server.Repositories.Dtos
{
    public class ReportDto
    {
        public int ReportId { get; set; }
        public string ReportedPlayerUsername { get; set; }
        public string ReportedByPlayerUsername { get; set; }
        public string Reason { get; set; }
    }
}
