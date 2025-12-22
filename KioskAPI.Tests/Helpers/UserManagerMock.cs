namespace KioskAPI.Tests.Helpers
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Identity;
  using Moq;

  public class UserManagerMock
  {
    public static Mock<UserManager<TUser>> Create<TUser>() where TUser : class
    {
      var store = new Mock<IUserStore<TUser>>();

      return new Mock<UserManager<TUser>>(
          store.Object,
          null,
          null,
          null,
          null,
          null,
          null,
          null,
          null
      );
    }
  }
}