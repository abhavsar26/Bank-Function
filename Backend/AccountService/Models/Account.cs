using CustomerService.Models;
using System;
using System.Collections.Generic;

namespace AccountService.Models;

public partial class Account
{
    public int AccountId { get; set; }

    public int CustomerId { get; set; }

    public string? AccountType { get; set; }

    public string AccountNumber { get; set; } = null!;

    public string? Category { get; set; }

    public string? JointAccountHolderName { get; set; }

    public string? Status { get; set; }

    public DateTime DateOpened { get; set; }

    public double? InterestRate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public decimal? Balance { get; set; }
    public Customer Customer { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
