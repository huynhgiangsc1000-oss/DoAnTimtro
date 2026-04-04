using DoAnCk.Models.Entities;

namespace DoAnCk.Models.Entities
{
    public class RoomAmenity
    {
        public int RoomId { get; set; }
        public virtual Room Room { get; set; }

        public int AmenityId { get; set; }
        public virtual Amenity Amenity { get; set; }
    }
}