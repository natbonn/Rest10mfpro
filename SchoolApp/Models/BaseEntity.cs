namespace SchoolApp.Models
{
    public class BaseEntity
    {
        public DateTime InsertedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public bool IsDeleted { get; set; } = false; // αν και ειναι default
        public DateTime DeletedAt { get; set; }   // soft delete both 

    }
}
