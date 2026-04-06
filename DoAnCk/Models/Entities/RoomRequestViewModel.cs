namespace DoAnCk.Models.Entities
{
    public class RoomRequestViewModel
    {
        public int Id { get; set; }
        public string? RoomTitle { get; set; } // Thêm dấu ?
        public string? SenderName { get; set; }
        public string? Message { get; set; }
        public DateTime RequestDate { get; set; }
        public int Status { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
