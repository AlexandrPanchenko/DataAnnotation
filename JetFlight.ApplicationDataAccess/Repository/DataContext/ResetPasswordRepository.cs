using JetFlight.ApplicationDataAccess.Entities.DataContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.ApplicationDataAccess.Repository.DataContext
{
    public interface IResetPasswordRepository : IGenericDataRepository<ResetPassword>
    {
        public IQueryable<ResetPassword> GetResetPasswords(string code);
    }
    public class ResetPasswordRepository : DataGenericRepository<ResetPassword>, IResetPasswordRepository
    {
        public readonly ApplicationDataAccess.ApplicationDataContext _dbContext;

        public ResetPasswordRepository(ApplicationDataAccess.ApplicationDataContext context) : base(context)
        {
            _dbContext = context;
        }

        public IQueryable<ResetPassword> GetResetPasswords(string code)
        {
            return _dbContext.ResetPassword.Include(r => r.Admin).Where(x => x.AuthCode == code);
        }
    }
}
