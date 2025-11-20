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

    // GET BALANCE (SESSION-BASED)
    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
      var userId = this.HttpContext.Session.GetInt32("UserId");
      if (userId == null)
      {
        return this.Unauthorized();
      }

      var account = await this._accountRepo.GetAccountByUserIdAsync(userId.Value).ConfigureAwait(true);
      if (account == null)
      {
        return this.NotFound();
      }

      var dto = this._mapper.Map<AccountDto>(account);
      return this.Ok(dto);
    }

    // TOP-UP onto ACCOUNT session-based
    [HttpPost("topup")]
    public async Task<IActionResult> TopUp([FromBody] TopUpDto request)
    {
      var userId = this.HttpContext.Session.GetInt32("UserId");
      if (userId == null)
      {
        return this.Unauthorized();
      }

      if (request.Amount <= 0)
      {
        return this.BadRequest(new { message = "Amount must be positive" });
      }

      await this._accountRepo.UpdateBalanceAsync(userId.Value, request.Amount).ConfigureAwait(true);

      var account = await this._accountRepo.GetAccountByUserIdAsync(userId.Value).ConfigureAwait(true);

      return this.Ok(this._mapper.Map<AccountDto>(account));
    }

    // GET USER TRANSACTIONS
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions()
    {
      var userId = this.HttpContext.Session.GetInt32("UserId");
      if (userId == null)
      {
        return this.Unauthorized();
      }

      var transactions = await this._accountRepo.GetTransactionsAsync(userId.Value).ConfigureAwait(true);

      var dtos = this._mapper.Map<IEnumerable<TransactionDto>>(transactions);

      return this.Ok(dtos);
    }

    private int GetUserId()
    {
      return this.HttpContext.Session.GetInt32("UserId") ?? 0;
    }
  }
}