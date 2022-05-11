namespace Vialtec.Models
{
    public class MarkingFilter
    {
        public int EquipmentId { get; set; }
        public string DateInitComplete { get; set; }
        public string DateFinalComplete { get; set; }
    }

    public class MarkingMapFilter
    {
        public int TrackNumber { get; set; }
        public string InitialDate { get; set; }
        public string FinalDate { get; set; }
    }
}
