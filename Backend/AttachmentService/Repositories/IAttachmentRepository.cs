using AttachmentService.Models;

namespace AttachmentService.Repositories
{
    public interface IAttachmentRepository
    {
        Task<IEnumerable<Attachment>> GetAllAttachmentsAsync();
        Task<Attachment?> GetAttachmentByIdAsync(int attachmentId);
        Task<IEnumerable<Attachment>> GetAttachmentsByCustomerIdAsync(int customerId);
        Task AddAttachmentAsync(Attachment attachment);
        Task UpdateAttachmentAsync(Attachment attachment);
        Task DeleteAttachmentAsync(int attachmentId);
    }
}
