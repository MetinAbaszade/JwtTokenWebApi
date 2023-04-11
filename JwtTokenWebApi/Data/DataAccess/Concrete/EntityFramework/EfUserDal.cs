using JwtTokenWebApi.Data.Core.Concrete.EntityFramework;
using JwtTokenWebApi.Data.DataAccess.Abstract;
using JwtTokenWebApi.Entities.Concrete;

namespace JwtTokenWebApi.Data.DataAccess.Concrete.EntityFramework
{
    public class EfUserDal : EfEntityRepositoryBase<User, AppDbContext>, IUserDal
    {
    }
}
