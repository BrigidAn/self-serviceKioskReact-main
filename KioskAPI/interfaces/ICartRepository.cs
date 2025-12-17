namespace KioskAPI.interfaces
{
  using System.Threading.Tasks;
  using KioskAPI.Models;

  public interface ICartRepository
  {
    Task<Cart> GetUserCart(int userId);
    Task<Cart> CreateCartForUser(int userId);
    Task<CartItem> AddToCart(int userId, int productId, int quantity);
    Task<bool> UpdateQuantity(int userId, int cartItemId, int quantity);
    Task<bool> RemoveCartItem(int userId, int cartItemId);

  }
}