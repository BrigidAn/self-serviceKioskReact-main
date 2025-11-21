namespace KioskAPI.Controllers
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using AutoMapper;
  using KioskAPI.Dtos;
  using KioskAPI.interfaces;
  using Microsoft.AspNetCore.Mvc;

  [ApiController]
  [Route("api/[controller]")]
  public class AccountController : ControllerBase
  {
    private readonly IAccountRepository _accountRepo;
    private readonly IMapper _mapper;

    public AccountController(IAccountRepository accountRepo, IMapper mapper)
    {
      this._accountRepo = accountRepo;
      this._mapper = mapper;
    }

    private int GetUserIdFromSession()
    {
      return this.HttpContext.Session.GetInt32("UserId") ?? 0;
    }

    // GET BALANCE
    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
      int userId = this.GetUserIdFromSession();
      if (userId == 0)
      {
        return this.Unauthorized(new { message = "You have not logged in" });
      }

      var account = await this._accountRepo.GetAccountByUserIdAsync(userId).ConfigureAwait(true);
      if (account == null)
      {
        return this.NotFound(new { message = "Account not found" });
      }

      return this.Ok(this._mapper.Map<AccountDto>(account));
    }

    // TOP-UP onto ACCOUNT session-based
    [HttpPost("topup")]
    public async Task<IActionResult> TopUp([FromBody] TopUpDto request)
    {
      int userId = this.GetUserIdFromSession();
      if (userId == 0)
      {
        return this.Unauthorized(new { message = "You are not logged in" });
      }

      if (request.Amount <= 0)
      {
        return this.BadRequest(new { message = "Amount must be positive" });
      }

      await this._accountRepo.UpdateBalanceAsync(userId, request.Amount).ConfigureAwait(true);

      var account = await this._accountRepo.GetAccountByUserIdAsync(userId).ConfigureAwait(true);

      return this.Ok(this._mapper.Map<AccountDto>(account));
    }

    // GET USER TRANSACTIONS
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions()
    {
      int userId = this.GetUserIdFromSession();
      if (userId == 0)
      {
        return this.Unauthorized();
      }

      var transactions = await this._accountRepo.GetTransactionsAsync(userId).ConfigureAwait(true);

      return this.Ok(this._mapper.Map<IEnumerable<TransactionDto>>(transactions));
    }
  }
}