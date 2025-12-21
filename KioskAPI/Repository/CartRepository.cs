namespace KioskAPI.Repository
{
  using System.Linq;
  using System.Threading.Tasks;
  using KioskAPI.Data;
  using KioskAPI.interfaces;
  using KioskAPI.Models;
  using Microsoft.EntityFrameworkCore;

  public class CartRepository : ICartRepository
  {
    private readonly AppDbContext _context;

    public CartRepository(AppDbContext context)
    {
      this._context = context;
    }

    public async Task<Cart> GetUserCart(int userId)
    {
      return await this._context.Carts
          .Include(c => c.CartItems)
          .ThenInclude(i => i.Product)
          .FirstOrDefaultAsync(c => c.UserId == userId).ConfigureAwait(true);
    }

    public async Task<Cart> CreateCartForUser(int userId)
    {
      var cart = new Cart { UserId = userId };
      this._context.Carts.Add(cart);
      await this._context.SaveChangesAsync().ConfigureAwait(true);
      return cart;
    }

    public async Task<CartItem> AddToCart(int userId, int productId, int quantity)
    {
      var cart = await this.GetUserCart(userId).ConfigureAwait(true);
      if (cart == null)
      {
        cart = await this.CreateCartForUser(userId).ConfigureAwait(true);
      }

      var existingItem = cart.CartItems.FirstOrDefault(i => i.ProductId == productId);

      if (existingItem != null)
      {
        existingItem.Quantity += quantity;
      }
      else
      {
        var newItem = new CartItem
        {
          CartId = cart.CartId,
          ProductId = productId,
          Quantity = quantity
        };

        cart.CartItems.Add(newItem);
      }

      await this._context.SaveChangesAsync().ConfigureAwait(true);
      return cart.CartItems.First(i => i.ProductId == productId);
    }

    public async Task<bool> UpdateQuantity(int userId, int cartItemId, int quantity)
    {
      var cart = await this.GetUserCart(userId).ConfigureAwait(true);
      if (cart == null)
      {
        return false;
      }

      var item = cart.CartItems.FirstOrDefault(i => i.CartItemId == cartItemId);
      if (item == null)
      {
        return false;
      }

      item.Quantity = quantity;
      await this._context.SaveChangesAsync().ConfigureAwait(true);
      return true;
    }

    public async Task<bool> RemoveCartItem(int userId, int cartItemId)
    {
      var cart = await this.GetUserCart(userId).ConfigureAwait(true);
      if (cart == null)
      {
        return false;
      }

      var item = cart.CartItems.FirstOrDefault(i => i.CartItemId == cartItemId);
      if (item == null)
      {
        return false;
      }

      this._context.CartItems.Remove(item);
      await this._context.SaveChangesAsync().ConfigureAwait(true);
      return true;
    }
  }
}
