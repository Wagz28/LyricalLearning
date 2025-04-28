using LyricalLearning.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LyricalLearning.data {
    public class UsersDbContext : IdentityDbContext<Users> {
        public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options) {
        }
    }
}