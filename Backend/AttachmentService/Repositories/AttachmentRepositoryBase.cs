using AttachmentService.Models;

namespace AttachmentService.Repositories
{
    public abstract class AttachmentRepositoryBase:IAttachmentRepository
    {
        protected readonly AttachmentBankContext _context;

        protected AttachmentRepositoryBase(AttachmentBankContext context)
        {
            _context = context;
        }
        public abstract Task<IEnumerable<Attachment>> GetAllAttachmentsAsync();
        public abstract Task<Attachment?> GetAttachmentByIdAsync(int attachmentId);
        public abstract Task<IEnumerable<Attachment>> GetAttachmentsByCustomerIdAsync(int customerId);
        public abstract Task AddAttachmentAsync(Attachment attachment);
        public abstract Task UpdateAttachmentAsync(Attachment attachment);
        public abstract Task DeleteAttachmentAsync(int attachmentId);
    }
}
