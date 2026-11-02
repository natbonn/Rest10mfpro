using Microsoft.EntityFrameworkCore;
using SchoolApp.Core;
using SchoolApp.Data;
using SchoolApp.Models;
using System.Linq.Expressions;

namespace SchoolApp.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(SchoolRest10Context context) : base(context)
        {
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Role)       // eager loading JOIN = Include
                .ThenInclude(r => r.Capabilities)
                .FirstOrDefaultAsync(u => u.Username == username || u.Email == username);
        }

        public async Task<PaginatedResult<User>> GetUsersAsync(
            int pageNumber, 
            int pageSize, 
            List<Expression<Func<User, bool>>> predicates)
        {
            IQueryable<User> query = _context.Users;    // σαν να λέμε SELECT * FROM User - δεν εκτελείται ακόμα

            if (predicates is not null && predicates.Count > 0)     // έλεγχος αν υπάρχουν φίλτρα
            {
                foreach (var predicate in predicates)
                {
                    query = query.Where(predicate);    // υπονοείται το AND
                }
            }

            int totalRecords = await query.CountAsync();
            int skip = (pageNumber - 1) * pageSize;     // υπολογίζει πόσες εγγραφές θα προσπεράσουμε για pagination

            var data = await query
                .OrderBy(u => u.Id)     // paginated πάντα με OrderBy πριν το skip
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();       // η οποία εκτελεί τελικά το query

            return new PaginatedResult<User>(data, totalRecords, pageNumber, pageSize);
        }
    }
}
