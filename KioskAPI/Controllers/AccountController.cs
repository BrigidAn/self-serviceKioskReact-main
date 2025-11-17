using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using KioskAPI.Data;
using KioskAPI.Dtos;
using KioskAPI.interfaces;
using KioskAPI.Models;
using KioskAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KioskAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
    private readonly IAccountRepository _accountRepo;
    private readonly IMapper _mapper;

    public AccountController(IAccountRepository accountRepo, IMapper mapper)
    {
        _accountRepo = accountRepo;
        _mapper = mapper;
    }

 // GET BALANCE (SESSION-BASED)
    [HttpGet("balance")]
      public async Task<IActionResult> GetBalance()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return Unauthorized();

        var account = await _accountRepo.GetAccountByUserIdAsync(userId.Value);
        if (account == null) return NotFound();

        var dto = _mapper.Map<AccountDto>(account);
        return Ok(dto);
    }
      
// TOP-UP onto ACCOUNT session-based
    [HttpPost("topup")]
       public async Task<IActionResult> TopUp([FromBody] TopUpDto request)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return Unauthorized();

        if (request.Amount <= 0)
            return BadRequest(new { message = "Amount must be positive" });

        await _accountRepo.UpdateBalanceAsync(userId.Value, request.Amount);

        var account = await _accountRepo.GetAccountByUserIdAsync(userId.Value);

        return Ok(_mapper.Map<AccountDto>(account));
    }


         // GET USER TRANSACTIONS
  [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return Unauthorized();

        var transactions = await _accountRepo.GetTransactionsAsync(userId.Value);

        var dtos = _mapper.Map<IEnumerable<TransactionDto>>(transactions);

        return Ok(dtos);
    }

        private int GetUserId()
        {
          return HttpContext.Session.GetInt32("UserId") ?? 0;
        }
    }
}