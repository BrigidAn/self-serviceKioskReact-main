using KioskAPI.Data;
using KioskAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/cart")]
[ApiController]
public class CartItemController : ControllerBase
{
    private readonly AppDbContext _context;

    public CartItemController(AppDbContext context)
    {
        _context = context;
    }

    private int GetUserId()
    {
        return HttpContext.Session.GetInt32("UserId") ?? 0;
    }

    // GET CART
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        int userId = GetUserId();
        if (userId == 0) return Unauthorized();

        await CleanupExpiredReservations();

        var items = await _context.CartItems
            .Where(c => c.UserId == userId && !c.IsCheckedOut)
            .Include(c => c.Product)
            .ToListAsync();

        return Ok(new { items });
    }

    // ADD TO CART
    [HttpPost("add")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest req)
    {
        int userId = GetUserId();
        if (userId == 0) return Unauthorized();

        var product = await _context.Products.FindAsync(req.ProductId);
        if (product == null) return NotFound();

        if (product.Quantity < req.Quantity)
            return BadRequest(new { message = "Not enough stock" });

        product.Quantity -= req.Quantity;

        var cartItem = new CartItem
        {
            UserId = userId,
            ProductId = req.ProductId,
            Product = product,
            Quantity = req.Quantity,
            ReservedUntil = DateTime.UtcNow.AddHours(18)
        };

        _context.CartItems.Add(cartItem);
        await _context.SaveChangesAsync();

        return Ok(new { expiresAt = cartItem.ReservedUntil });
    }

    // REMOVE ITEM FROM CART
    [HttpDelete("remove/{cartItemId}")]
    public async Task<IActionResult> Remove(int cartItemId)
    {
        int userId = GetUserId();
        if (userId == 0) return Unauthorized();

        var item = await _context.CartItems
            .Include(c => c.Product)
            .FirstOrDefaultAsync(c => c.CartItemId == cartItemId && c.UserId == userId);

        if (item == null) return NotFound();

        // restore stock
        item.Product.Quantity += item.Quantity;

        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Removed and stock restored" });
    }

    // CLEANUP EXPIRED
    [HttpPost("cleanup")]
    public async Task<IActionResult> Cleanup()
    {
        int restored = await CleanupExpiredReservations();
        return Ok(new { restored });
    }

    private async Task<int> CleanupExpiredReservations()
    {
        var now = DateTime.UtcNow;

        var expired = await _context.CartItems
            .Include(c => c.Product)
            .Where(c => !c.IsCheckedOut && c.ReservedUntil < now)
            .ToListAsync();

        foreach (var item in expired)
        {
            item.Product.Quantity += item.Quantity;
            _context.CartItems.Remove(item);
        }

        await _context.SaveChangesAsync();
        return expired.Count;
    }
}

public class AddToCartRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
