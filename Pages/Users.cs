using Microsoft.AspNetCore.Identity;

namespace LyricalLearning.Models {
    public class Users : IdentityUser {
        public required string FullName {get; set;} // Adds an extra property to default user object
    }
}

