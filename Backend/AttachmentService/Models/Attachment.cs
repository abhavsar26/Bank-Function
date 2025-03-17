using CustomerService.Models;
using System;
using System.Collections.Generic;

namespace AttachmentService.Models;

public partial class Attachment
{
    public int AttachmentId { get; set; }

    public int CustomerId { get; set; }

    public string? AttachmentType { get; set; }

    public string? FilePath { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public Customer Customer { get; set; }
}
