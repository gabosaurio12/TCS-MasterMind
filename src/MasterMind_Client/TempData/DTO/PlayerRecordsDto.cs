
namespace MasterMind_Client.TempData.DTO
{
    public class PlayerRecordsDto
    {
        public string PlayerUsername { get; set; }
        public int[] PlayerTriesRecords { get; set; }
        public int[] PlayerTimeTrialRecords { get; set; }
    }
}
