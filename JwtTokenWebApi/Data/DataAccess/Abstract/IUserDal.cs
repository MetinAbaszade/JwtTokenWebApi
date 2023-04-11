using JwtTokenWebApi.Data.Core.Abstract;
using JwtTokenWebApi.Entities.Concrete;

namespace JwtTokenWebApi.Data.DataAccess.Abstract
{
    public interface IUserDal : IEntityRepository<User>
    {
    }
}
