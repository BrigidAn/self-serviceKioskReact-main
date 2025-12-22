namespace KioskAPI.Controllers
{
  using System.Linq;
  using System.Security.Claims;
  using System.Threading.Tasks;
  using KioskAPI.Mappers;
  using KioskAPI.Dtos;
  using KioskAPI.interfaces;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;

  /// <summary>
  /// Controller that handles user accounts
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  [Authorize]
  public class AccountController : ControllerBase
  {
    private readonly IAccountRepository _accountRepo;

    /// <summary>
    ///Initializes a new instance of the <see cref="AccountController"/> class.
    /// </summary>
    /// <param name="accountRepo">Injected the account repository for data access</param>
    public AccountController(IAccountRepository accountRepo)
    {
      this._accountRepo = accountRepo;
    }

    /// <summary>
    /// Retrieves the ID of the currently authenticated user
    /// </summary>
    /// <returns>The user's id as an integer, or 0 if not found</returns>
    private int GetIdentityUserId()
    {
      var claim = this.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
      return claim != null ? int.Parse(claim.Value) : 0;
    }

    /// <summary>
    ///Retrieves current user's account balance
    /// </summary>
    /// <returns> 200 OK with the account data if found,
    /// 401, Unauthorized if the user is not logged in
    /// 404 Not found if the account is not found</returns>
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

    /// <summary>
    /// Tops up current user's account with specified amount
    /// </summary>
    /// <param name="request">containing the top-up amount.</param>
    /// <returns>200 OK updated account data
    /// 400 request exceeds the amount of 1500
    /// 401 an unauthorized user not logged in </returns>
    [HttpPost("topup")]
    public async Task<IActionResult> TopUp([FromBody] TopUpDto request)
    {
      int userId = this.GetIdentityUserId();
      if (userId == 0)
      {
        return this.Unauthorized(new { message = "You are not logged in" });
      }

      if (request.Amount > 1500)
      {
        return this.BadRequest(new { message = "Maximum amount to deposit is R1500" });
      }

      var account = await this._accountRepo.GetAccountByUserIdAsync(userId).ConfigureAwait(true);

      account ??= await this._accountRepo.CreateAccountForUserAsync(userId).ConfigureAwait(true);

      await this._accountRepo.UpdateBalanceAsync(userId, request.Amount).ConfigureAwait(true);

      account = await this._accountRepo.GetAccountByUserIdAsync(userId).ConfigureAwait(true);

      return this.Ok(AccountMapper.ToDto(account));
    }

    /// <summary>
    /// Get transaction history from current user
    /// </summary>
    /// <returns>200 OK returns list transactions
    /// 401 unauthorized user not logged in</returns>
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

    /// <summary>
    /// Retrieves the current user's account information, balance, and transaction history.
    /// </summary>
    /// <returns>
    /// 200 OK with a object containing user info, account info, and transactions,
    /// 401 Unauthorized if the user is not logged in.
    /// </returns>
    [HttpGet("myaccount")]
    public async Task<IActionResult> GetCurrentUser()
    {
      int userId = this.GetIdentityUserId();
      if (userId == 0)
      {
        return this.Unauthorized(new { message = "You are not logged in" });
      }

      var account = await this._accountRepo.GetAccountByUserIdAsync(userId).ConfigureAwait(false);
      account ??= await this._accountRepo.CreateAccountForUserAsync(userId).ConfigureAwait(false);

      var transactions = await this._accountRepo.GetTransactionsAsync(userId).ConfigureAwait(false);

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