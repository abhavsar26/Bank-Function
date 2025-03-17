using AttachmentService.Models;
using Microsoft.EntityFrameworkCore;

namespace AttachmentService.Repositories
{
    public class AttachmentRepository:AttachmentRepositoryBase
    {
        public AttachmentRepository(AttachmentBankContext context) : base(context)
        {
        }
        public override async Task<IEnumerable<Attachment>> GetAllAttachmentsAsync()
        {
            return await _context.Attachments.ToListAsync();
        }

        public override async Task<Attachment?> GetAttachmentByIdAsync(int attachmentId)
        {
            return await _context.Attachments.FindAsync(attachmentId);
        }

        public override async Task<IEnumerable<Attachment>> GetAttachmentsByCustomerIdAsync(int customerId)
        {
            return await _context.Attachments
                                 .Where(a => a.CustomerId == customerId)
                                 .ToListAsync();
        }

        public override async Task AddAttachmentAsync(Attachment attachment)
        {
            await _context.Attachments.AddAsync(attachment);
            await _context.SaveChangesAsync();
        }

        public override async Task UpdateAttachmentAsync(Attachment attachment)
        {
            _context.Attachments.Update(attachment);
            await _context.SaveChangesAsync();
        }

        public override async Task DeleteAttachmentAsync(int attachmentId)
        {
            var attachment = await GetAttachmentByIdAsync(attachmentId);
            if (attachment != null)
            {
                _context.Attachments.Remove(attachment);
                await _context.SaveChangesAsync();
            }
        }
    }
}
