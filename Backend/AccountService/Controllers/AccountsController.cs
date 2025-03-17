using AccountService.Dto;
using AccountService.Models;
using AccountService.Repositories;
using CustomerService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;


namespace AccountService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IHttpClientFactory _httpClientFactory; // Use IHttpClientFactory
        private readonly string _customerServiceBaseUrl;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IMoneyRequestRepository _moneyRequestRepository;

        public AccountsController(IAccountRepository accountRepository, IHttpClientFactory httpClientFactory, IConfiguration configuration, ITransactionRepository transactionRepository,IMoneyRequestRepository moneyRequestRepository)
        {
            _accountRepository = accountRepository;
            _httpClientFactory = httpClientFactory; // Initialize IHttpClientFactory
            _customerServiceBaseUrl = configuration.GetValue<string>("CustomerServiceBaseUrl");
            _transactionRepository = transactionRepository;
            _moneyRequestRepository = moneyRequestRepository;
        }

        // GET: api/accounts/{id}
        //[Authorize] // Protects this endpoint, requires JWT token
        [HttpGet("{id}")]                   
        public async Task<ActionResult<Account>> GetAccountById(int id)
        {
            var account = await _accountRepository.GetAccountByAccountNumberAsync(id);
            if (account == null) return NotFound("Account not found.");

            var httpClient = _httpClientFactory.CreateClient(); // Create HttpClient
            HttpResponseMessage customerResponse;
            try
            {
                // Make an HTTP request to the customer service to get the customer by their ID
                customerResponse = await httpClient.GetAsync($"{_customerServiceBaseUrl}/api/Customers/{account.CustomerId}");
            }
            catch (HttpRequestException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving customer data.");
            }

            if (!customerResponse.IsSuccessStatusCode)
            {
                return NotFound("Customer not found.");
            }

            Customer customer;
            try
            {
                // Deserialize the customer data from the response
                customer = await customerResponse.Content.ReadFromJsonAsync<Customer>();
            }
            catch (JsonException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error processing customer data.");
            }

            // Attach the customer information to the account object
            account.Customer = customer;

            // Return the enriched account with the customer information
            return Ok(account);
        }

        // GET: api/accounts/customer/{customerId}
        //[Authorize] // Protect this endpoint
        [HttpGet("customer/{customerId}/details")]
        public async Task<ActionResult<IEnumerable<Account>>> GetAccountsByCustomerId(int customerId)
        {
            var httpClient = _httpClientFactory.CreateClient(); // Create HttpClient
            HttpResponseMessage customerResponse;
            try
            {
                // Make an HTTP request to the customer service to get the customer by their ID
                customerResponse = await httpClient.GetAsync($"{_customerServiceBaseUrl}/api/Customers/{customerId}");
            }
            catch (HttpRequestException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving customer data.");
            }

            if (!customerResponse.IsSuccessStatusCode)
            {
                return NotFound("Customer not found.");
            }

            Customer customer;
            try
            {
                // Deserialize the customer data from the response
                customer = await customerResponse.Content.ReadFromJsonAsync<Customer>();
            }
            catch (JsonException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error processing customer data.");
            }

            // Fetch the accounts associated with the customer from the repository
            var accounts = await _accountRepository.GetAccountsByCustomerIdAsync(customerId);

            // If no accounts are found, return NotFound
            if (accounts == null || !accounts.Any())
            {
                return NotFound("Accounts not found for the customer.");
            }

            // Attach the customer data to each account
            foreach (var account in accounts)
            {
                account.Customer = customer;
            }

            // Return the accounts with customer data
            return Ok(accounts);
        }

        // POST: api/accounts
        //[Authorize] // Protect this endpoint
        // POST: api/accounts
        [HttpPost]
        public async Task<ActionResult> AddAccount(AccountDto accountDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Generate a unique account number based on the account type
            var random = new Random();
            string accountNumberPrefix = string.Empty;

            // Set prefix based on AccountType
            if (accountDto.AccountType.ToLower() == "current")
            {
                accountNumberPrefix = "CA"; // Current Account
            }
            else if (accountDto.AccountType.ToLower() == "saving")
            {
                accountNumberPrefix = "SB"; // Saving Account
            }

            // Generate the rest of the account number (10-digit random number)
            string generatedAccountNumber = accountNumberPrefix + random.Next(100000, 999999).ToString() + random.Next(100000, 999999).ToString();

            // Map AccountDto to Account
            var account = new Account
            {
                AccountId = accountDto.AccountId,
                CustomerId = accountDto.CustomerId,
                AccountType = accountDto.AccountType,
                AccountNumber = generatedAccountNumber, // Assign the generated account number
                Category = accountDto.Category,
                JointAccountHolderName = accountDto.JointAccountHolderName,
                Status = "Active", // Set default status to "Active"
                DateOpened = accountDto.DateOpened,
                InterestRate = accountDto.InterestRate,
                CreatedAt = accountDto.CreatedAt ?? DateTime.UtcNow,
                Balance = accountDto.Balance,
                UpdatedAt = accountDto.UpdatedAt ?? DateTime.UtcNow
            };

            await _accountRepository.AddAccountAsync(account);

            return CreatedAtAction(nameof(GetAccountById), new { id = account.AccountId }, accountDto);
        }




        // PUT: api/accounts/{id}
        //[Authorize] // Protect this endpoint
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAccount(int id, AccountDto accountDto)
        {
            if (id != accountDto.AccountId)
            {
                return BadRequest("Account ID mismatch.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingAccount = await _accountRepository.GetAccountByAccountNumberAsync(id);
            if (existingAccount == null)
            {
                return NotFound();
            }

            // Map AccountDto to Account
            var account = new Account
            {
                AccountId = accountDto.AccountId,
                CustomerId = accountDto.CustomerId,
                AccountType = accountDto.AccountType,
                AccountNumber = accountDto.AccountNumber,
                Category = accountDto.Category,
                JointAccountHolderName = accountDto.JointAccountHolderName,
                Status = accountDto.Status,
                DateOpened = accountDto.DateOpened,
                InterestRate = accountDto.InterestRate,
                CreatedAt = existingAccount.CreatedAt, // Preserve original creation date
                UpdatedAt = DateTime.UtcNow,
                Balance = accountDto.Balance,
            };

            await _accountRepository.UpdateAccountAsync(account);
            return NoContent();
        }

        // DELETE: api/accounts/{id}
        [Authorize] // Protect this endpoint
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAccount(int id)
        {
            var existingAccount = await _accountRepository.GetAccountByAccountNumberAsync(id);
            if (existingAccount == null)
            {
                return NotFound();
            }

            await _accountRepository.DeleteAccountAsync(id);
            return Ok("Account deleted successfully");
        }
        // GET: api/accounts/customer/{customerId}/summary
        //[Authorize] // Protect this endpoint
        [HttpGet("customer/{customerId}/summary")]
        public async Task<ActionResult<IEnumerable<AccountSummaryDto>>> GetAccountSummaryByCustomerId(int customerId)
        {
            var httpClient = _httpClientFactory.CreateClient(); // Create HttpClient
            HttpResponseMessage customerResponse;

            // Make an HTTP request to the customer service to get the customer by their ID
            try
            {
                customerResponse = await httpClient.GetAsync($"{_customerServiceBaseUrl}/api/Customers/{customerId}");
            }
            catch (HttpRequestException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving customer data.");
            }

            if (!customerResponse.IsSuccessStatusCode)
            {
                return NotFound("Customer not found.");
            }

            Customer customer;
            try
            {
                // Deserialize the customer data from the response
                customer = await customerResponse.Content.ReadFromJsonAsync<Customer>();
            }
            catch (JsonException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error processing customer data.");
            }

            // Fetch accounts associated with the customer from the repository
            var accounts = await _accountRepository.GetAccountsByCustomerIdAsync(customerId);

            // If no accounts are found, return NotFound
            if (accounts == null || !accounts.Any())
            {
                return NotFound("Accounts not found for the customer.");
            }

            // Create a summary list of accounts with customer name
            var accountSummaries = accounts.Select(account => new AccountSummaryDto
            {
                AccountType = account.AccountType,
                AccountNumber = account.AccountNumber,
                Category = account.Category,
                JointAccountHolderName = account.JointAccountHolderName,
                Status = account.Status,
                Balance = account.Balance,
                CustomerName = $"{customer.FirstName} {customer.LastName}" // Combine first and last name
            }).ToList();

            // Return the list of account summaries
            return Ok(accountSummaries);
        }
        // PUT: api/accounts/add-money/{accountNumber}
        //[Authorize] // Protect this endpoint
        // PUT: api/accounts/add-money/{accountNumber}
        //[Authorize] // Protect this endpoint
        [HttpPut("add-money/{accountNumber}")]
        public async Task<ActionResult> AddMoneyToAccount(string accountNumber, [FromBody] decimal amount)
        {
            if (amount <= 0)
            {
                return BadRequest("Amount must be greater than zero.");
            }

            // Retrieve the account by the account number
            var account = await _accountRepository.GetAccountByAccountNumberAsync(accountNumber);
            if (account == null)
            {
                return NotFound("Account not found.");
            }

            // Initialize balance if it's null
            if (account.Balance == null)
            {
                account.Balance = 0;
            }

            // Add the amount to the balance
            account.Balance += amount;

            // Update the account with the new balance
            await _accountRepository.UpdateAccountAsync(account);

            // Add a transaction record for the money added
            var transaction = new Transaction
            {
                AccountId = account.AccountId,
                Amount = amount,
                TransactionDate = DateTime.UtcNow,
                TransactionType = "Credit",
                Description = "Money added to account"
            };
            await _transactionRepository.AddTransactionAsync(transaction);

            return Ok($"Amount of {amount} added to account {accountNumber}. New balance: {account.Balance}");
        }

        //[Authorize] // Protect this endpoint
        [HttpPut("update-balance/{accountNumber}")]
        public async Task<ActionResult> UpdateBalance(string accountNumber, [FromBody] decimal newBalance)
        {
            if (newBalance < 0)
            {
                return BadRequest("Balance cannot be negative.");
            }

            // Retrieve the account by account number
            var account = await _accountRepository.GetAccountByAccountNumberAsync(accountNumber);
            if (account == null)
            {
                return NotFound("Account not found.");
            }

            // Set the new balance
            account.Balance = newBalance;

            // Update the account with the new balance
            await _accountRepository.UpdateAccountAsync(account);

            return Ok($"Balance for account number {accountNumber} updated to {newBalance}.");
        }
        // POST: api/accounts/transfer
        //[Authorize] // Protect this endpoint
        //[HttpPost("transfer")]
        //public async Task<ActionResult> TransferMoney([FromBody] TransferDto transferDto)
        //{
        //    // Validate the model
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    // Retrieve source account
        //    var sourceAccount = await _accountRepository.GetAccountByAccountNumberAsync(transferDto.SourceAccountNumber);
        //    if (sourceAccount == null)
        //    {
        //        return NotFound($"Source account {transferDto.SourceAccountNumber} not found.");
        //    }

        //    // Retrieve destination account
        //    var destinationAccount = await _accountRepository.GetAccountByAccountNumberAsync(transferDto.DestinationAccountNumber);
        //    if (destinationAccount == null)
        //    {
        //        return NotFound($"Destination account {transferDto.DestinationAccountNumber} not found.");
        //    }

        //    // Check if the source account has enough balance
        //    if (sourceAccount.Balance < transferDto.Amount)
        //    {
        //        return BadRequest("Insufficient funds in the source account.");
        //    }

        //    // Perform the transfer
        //    sourceAccount.Balance -= transferDto.Amount; // Deduct from source
        //    destinationAccount.Balance += transferDto.Amount; // Add to destination

        //    // Update both accounts
        //    await _accountRepository.UpdateAccountAsync(sourceAccount);
        //    await _accountRepository.UpdateAccountAsync(destinationAccount);

        //    // Add transaction record for the source account (debit)
        //    var debitTransaction = new Transaction
        //    {
        //        AccountId = sourceAccount.AccountId,
        //        Amount = -transferDto.Amount,
        //        TransactionDate = DateTime.UtcNow,
        //        TransactionType = "Debit",
        //        Description = $"Transfer to account {transferDto.DestinationAccountNumber}"
        //    };
        //    await _transactionRepository.AddTransactionAsync(debitTransaction);

        //    // Add transaction record for the destination account (credit)
        //    var creditTransaction = new Transaction
        //    {
        //        AccountId = destinationAccount.AccountId,
        //        Amount = transferDto.Amount,
        //        TransactionDate = DateTime.UtcNow,
        //        TransactionType = "Credit",
        //        Description = $"Transfer from account {transferDto.SourceAccountNumber}"
        //    };
        //    await _transactionRepository.AddTransactionAsync(creditTransaction);

        //    return Ok($"Transferred {transferDto.Amount} from account {transferDto.SourceAccountNumber} to {transferDto.DestinationAccountNumber}.");
        //}

        [HttpGet("{accountId}/transactions")]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetTransactionHistory(int accountId)
        {
            // Fetch the account to ensure it exists using the accountId
            var account = await _accountRepository.GetAccountByIdAsync(accountId);
            if (account == null)
            {
                return NotFound("Account not found.");
            }

            // Fetch the transaction history for the account
            var transactions = await _transactionRepository.GetTransactionsByAccountIdAsync(accountId);

            // Map transactions to DTOs and include the account number
            var transactionDtos = transactions.Select(t => new TransactionDto
            {
                TransactionId = t.TransactionId,
                Amount = t.Amount,
                TransactionDate = t.TransactionDate,
                TransactionType = t.TransactionType,
                Description = t.Description,
                AccountNumber = account.AccountNumber // Assuming AccountNumber is a property of Account
            }).ToList();

            // Return the transaction history
            return Ok(transactionDtos);
        }
        [HttpGet("by-account-number/{accountNumber}")]
        public async Task<IActionResult> GetAccountByAccountNumber(string accountNumber)
        {
            var account = await _accountRepository.GetAccountByAccountNumberAsync(accountNumber);

            if (account == null)
            {
                return NotFound(new { Message = "Account not found" });
            }

            return Ok(account);
        }
        [HttpPost("transfer")]
public async Task<ActionResult> TransferMoney([FromBody] TransferDto transferDto)
{
    // Validate the model
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    // Retrieve source account
    var sourceAccount = await _accountRepository.GetAccountByAccountNumberAsync(transferDto.SourceAccountNumber);
    if (sourceAccount == null)
    {
        return NotFound($"Source account {transferDto.SourceAccountNumber} not found.");
    }

    // Retrieve destination account
    var destinationAccount = await _accountRepository.GetAccountByAccountNumberAsync(transferDto.DestinationAccountNumber);
    if (destinationAccount == null)
    {
        return NotFound($"Destination account {transferDto.DestinationAccountNumber} not found.");
    }

    // Check if the source account has enough balance
    if (sourceAccount.Balance < transferDto.Amount)
    {
        return BadRequest("Insufficient funds in the source account.");
    }

    // Perform the transfer
    sourceAccount.Balance -= transferDto.Amount; // Deduct from source
    destinationAccount.Balance += transferDto.Amount; // Add to destination

    // Update both accounts
    await _accountRepository.UpdateAccountAsync(sourceAccount);
    await _accountRepository.UpdateAccountAsync(destinationAccount);

    // Add transaction record for the source account (debit)
    var debitTransaction = new Transaction
    {
        AccountId = sourceAccount.AccountId,
        Amount = -transferDto.Amount,
        TransactionDate = DateTime.UtcNow,
        TransactionType = "Debit",
        Description = $"Transfer to account {transferDto.DestinationAccountNumber}"
    };
    await _transactionRepository.AddTransactionAsync(debitTransaction);

    // Add transaction record for the destination account (credit)
    var creditTransaction = new Transaction
    {
        AccountId = destinationAccount.AccountId,
        Amount = transferDto.Amount,
        TransactionDate = DateTime.UtcNow,
        TransactionType = "Credit",
        Description = $"Transfer from account {transferDto.SourceAccountNumber}"
    };
    await _transactionRepository.AddTransactionAsync(creditTransaction);

    // If the transfer was related to a money request, log that too (optional)
    var requestTransaction = await _transactionRepository.GetLatestTransactionByDescriptionAsync(
        destinationAccount.AccountId, 
        $"Money request from {transferDto.SourceAccountNumber}");

    if (requestTransaction != null)
    {
        requestTransaction.TransactionType = "Money Request Fulfilled";
        await _transactionRepository.UpdateTransactionAsync(requestTransaction);
    }

    return Ok($"Transferred {transferDto.Amount} from account {transferDto.SourceAccountNumber} to {transferDto.DestinationAccountNumber}.");
}

        // POST: api/accounts/request-money
        //[Authorize] // Protect this endpoint
        [HttpPost("request-money")]
        public async Task<ActionResult> RequestMoney([FromBody] MoneyRequestDto moneyRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate that the from account exists
            var fromAccount = await _accountRepository.GetAccountByAccountNumberAsync(moneyRequestDto.FromAccountNumber);
            if (fromAccount == null)
            {
                return NotFound($"From account {moneyRequestDto.FromAccountNumber} not found.");
            }

            // Validate that the to account exists
            var toAccount = await _accountRepository.GetAccountByAccountNumberAsync(moneyRequestDto.ToAccountNumber);
            if (toAccount == null)
            {
                return NotFound($"To account {moneyRequestDto.ToAccountNumber} not found.");
            }

            // Create the money request
            var moneyRequest = new MoneyRequest
            {
                FromAccountNumber = moneyRequestDto.FromAccountNumber,
                ToAccountNumber = moneyRequestDto.ToAccountNumber,
                Amount = moneyRequestDto.Amount,
                RequestDate = DateTime.UtcNow,
                Status = "Pending" // Default status
            };

            await _moneyRequestRepository.AddMoneyRequestAsync(moneyRequest);

            return CreatedAtAction(nameof(RequestMoney), new { id = moneyRequest.MoneyRequestId }, moneyRequestDto);
        }
        [HttpGet("pending-requests/{customerId}")]
        public async Task<ActionResult<IEnumerable<RequestMoneyDto>>> GetPendingRequestsByCustomer(int customerId)
        {
            // Retrieve customer accounts from the repository
            var customerAccounts = await _accountRepository.GetAccountsByCustomerIdAsync(customerId);

            if (customerAccounts == null || !customerAccounts.Any())
            {
                return NotFound("Customer does not have any accounts.");
            }

            // Extract account numbers
            var accountNumbers = customerAccounts.Select(a => a.AccountNumber).ToList();

            // Get pending requests for these account numbers
            var pendingRequests = await _moneyRequestRepository.GetPendingRequestsByAccountNumbersAsync(accountNumbers);

            if (pendingRequests == null || !pendingRequests.Any())
            {
                return NotFound("No pending requests found for the customer.");
            }

            // Map the money requests to DTOs
            var requestDtos = pendingRequests.Select(m => new RequestMoneyDto
            {
                MoneyRequestId = m.MoneyRequestId,
                FromAccountNumber = m.FromAccountNumber,
                ToAccountNumber = m.ToAccountNumber,
                Amount = m.Amount,
                RequestDate = m.RequestDate,
                Status = m.Status
            }).ToList();

            return Ok(requestDtos);
        }

        [HttpPost("accept-request/{requestId}")]
        public async Task<IActionResult> AcceptMoneyRequest(int requestId)
        {
            // Retrieve the money request from the repository
            var moneyRequest = await _moneyRequestRepository.GetMoneyRequestByIdAsync(requestId);
            if (moneyRequest == null)
            {
                return NotFound("Money request not found.");
            }

            if (moneyRequest.Status != "Pending")
            {
                return BadRequest("This request has already been processed.");
            }

            // Retrieve accounts from the repository
            var fromAccount = await _accountRepository.GetAccountByAccountNumberAsync(moneyRequest.FromAccountNumber);
            var toAccount = await _accountRepository.GetAccountByAccountNumberAsync(moneyRequest.ToAccountNumber);

            if (fromAccount == null || toAccount == null)
            {
                return NotFound("One of the accounts is invalid.");
            }

            if (fromAccount.Balance < moneyRequest.Amount)
            {
                return BadRequest("Insufficient funds in the source account.");
            }

            // Perform the transfer
            fromAccount.Balance += moneyRequest.Amount;
            toAccount.Balance -= moneyRequest.Amount;

            // Update the accounts in the repository
            await _accountRepository.UpdateAccountAsync(fromAccount);
            await _accountRepository.UpdateAccountAsync(toAccount);

            // Mark the money request as completed
            moneyRequest.Status = "Completed";
            await _moneyRequestRepository.UpdateMoneyRequestAsync(moneyRequest);

            // Add transaction records for both accounts
            var debitTransaction = new Transaction
            {
                AccountId = fromAccount.AccountId,
                Amount = moneyRequest.Amount,
                TransactionDate = DateTime.UtcNow,
                TransactionType = "Credit",
                Description = $"Money request accepted. Transferred to account {moneyRequest.ToAccountNumber}."
            };
            var creditTransaction = new Transaction
            {
                AccountId = toAccount.AccountId,
                Amount = -moneyRequest.Amount,
                TransactionDate = DateTime.UtcNow,
                TransactionType = "Debit",
                Description = $"Money request accepted. Received from account {moneyRequest.FromAccountNumber}."
            };

            await _transactionRepository.AddTransactionAsync(debitTransaction);
            await _transactionRepository.AddTransactionAsync(creditTransaction);

            return Ok("Money request accepted and transaction completed.");
        }

    }
}
