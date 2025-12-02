namespace KioskAPI.Controllers
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Security.Claims;
  using System.Threading.Tasks;
  using KioskAPI.Mappers;
  using KioskAPI.Dtos;
  using KioskAPI.interfaces;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using KioskAPI.Models;

  [ApiController]
  [Route("api/[controller]")]
  [Authorize] // Ensure user is authenticated
  public class AccountController : ControllerBase
  {
    private readonly IAccountRepository _accountRepo;

    public AccountController(IAccountRepository accountRepo)
    {
      this._accountRepo = accountRepo;
    }

    // Helper: Get the logged-in user's Identity Id
    private int GetIdentityUserId()
    {
      var claim = this.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
      return claim != null ? int.Parse(claim.Value) : 0;
    }

    // GET BALANCE
    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
      int userId = this.GetIdentityUserId();
      if (userId == 0)
      {
        return this.Unauthorized(new { message = "You are not logged in" });
      }

      var account = await this._accountRepo.GetAccountByUserIdAsync(userId).ConfigureAwait(true);

      if (account == null)
      {
        return this.NotFound(new { message = "Account not found" });
      }

      return this.Ok(AccountMapper.ToDto(account));
    }

    // TOP-UP onto ACCOUNT
    [HttpPost("topup")]
    public async Task<IActionResult> TopUp([FromBody] TopUpDto request)
    {
      int userId = this.GetIdentityUserId();
      if (userId == 0)
      {
        return this.Unauthorized(new { message = "You are not logged in" });
      }

      if (request.Amount <= 0)
      {
        return this.BadRequest(new { message = "Amount must be positive" });
      }

      // Check if user already has an account
      var account = await this._accountRepo.GetAccountByUserIdAsync(userId).ConfigureAwait(true);

      // If no account exists, create a new one
      account ??= await this._accountRepo.CreateAccountForUserAsync(userId).ConfigureAwait(true);

      // Update the balance
      await this._accountRepo.UpdateBalanceAsync(userId, request.Amount).ConfigureAwait(true);

      // Reload the account to return updated balance
      account = await this._accountRepo.GetAccountByUserIdAsync(userId).ConfigureAwait(true);

      return this.Ok(AccountMapper.ToDto(account));
    }

    // GET USER TRANSACTIONS
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions()
    {
      int userId = this.GetIdentityUserId();
      if (userId == 0)
      {
        return this.Unauthorized();
      }

      var transactions = await this._accountRepo.GetTransactionsAsync(userId).ConfigureAwait(true);
      return this.Ok(transactions.Select(TransactionMapper.ToDto));
    }

    // GET /api/Account/me
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
      int userId = this.GetIdentityUserId();
      if (userId == 0)
      {
        return this.Unauthorized(new { message = "You are not logged in" });
      }

      // Get account (create if it doesn't exist)
      var account = await this._accountRepo.GetAccountByUserIdAsync(userId).ConfigureAwait(false);
      account ??= await this._accountRepo.CreateAccountForUserAsync(userId).ConfigureAwait(false);

      // Get transactions
      var transactions = await this._accountRepo.GetTransactionsAsync(userId).ConfigureAwait(false);

      // Compose response
      var response = new
      {
        user = new
        {
          Id = userId,
          Name = this.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value,
          Email = this.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
        },
        account = new
        {
          account.AccountId,
          account.Balance
        },
        transactions = transactions.Select(t => new
        {
          t.TransactionId,
          t.Description,
          t.TotalAmount,
          t.Type,
          t.CreatedAt
        })
      };

      return this.Ok(response);
    }
  }
}