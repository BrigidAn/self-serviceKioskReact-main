namespace KioskAPI.Repository
{
  using System.Linq;
  using System.Threading.Tasks;
  using KioskAPI.Data;
  using KioskAPI.interfaces;
  using KioskAPI.Models;
  using Microsoft.EntityFrameworkCore;

  /// <summary>
  /// Implements <see cref="ICartRepository"/> to manage user carts and their items.
  /// </summary>
  public class CartRepository : ICartRepository
  {
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="CartRepository"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public CartRepository(AppDbContext context)
    {
      this._context = context;
    }

    /// <summary>
    /// Retrieves the cart for a specific user, including its items and associated products.
    /// </summary>
    /// <param name="userId">The ID of the user whose cart is retrieved.</param>
    /// <returns>The <see cref="Cart"/> object if found; otherwise, <c>null</c>.</returns>
    public async Task<Cart> GetUserCart(int userId)
    {
      return await this._context.Carts
          .Include(c => c.CartItems)
          .ThenInclude(i => i.Product)
          .FirstOrDefaultAsync(c => c.UserId == userId).ConfigureAwait(true);
    }

    /// <summary>
    /// Creates a new cart for the user.
    /// </summary>
    /// <param name="userId">The ID of the user for whom the cart is created.</param>
    /// <returns>The newly created <see cref="Cart"/>.</returns>
    public async Task<Cart> CreateCartForUser(int userId)
    {
      var cart = new Cart { UserId = userId };
      this._context.Carts.Add(cart);
      await this._context.SaveChangesAsync().ConfigureAwait(true);
      return cart;
    }

    /// <summary>
    /// Adds a product to the user's cart or updates quantity if it already exists.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="productId">The ID of the product to add.</param>
    /// <param name="quantity">The quantity to add.</param>
    /// <returns>The <see cref="CartItem"/> representing the added or updated item.</returns>
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

    /// <summary>
    /// Updates the quantity of a specific cart item for a user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="cartItemId">The ID of the cart item to update.</param>
    /// <param name="quantity">The new quantity.</param>
    /// <returns><c>true</c> if the update succeeded; otherwise, <c>false</c>.</returns>
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

    /// <summary>
    /// Removes a cart item from the user's cart.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="cartItemId">The ID of the cart item to remove.</param>
    /// <returns><c>true</c> if removal succeeded; otherwise, <c>false</c>.</returns>
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
