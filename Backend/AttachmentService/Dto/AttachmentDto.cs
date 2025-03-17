using System.ComponentModel.DataAnnotations;

namespace AttachmentService.Dto
{
    public class AttachmentDto
    {
        public int AttachmentId { get; set; }

        public int CustomerId { get; set; }

        public string? AttachmentType { get; set; }
        [Required]
        public string? FilePath { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
