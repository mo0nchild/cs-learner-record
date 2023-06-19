using System.ComponentModel.DataAnnotations;

namespace ClientWeb.Models
{
    public partial class User
    {
        [Key]
        public int IdUser { get; set; } = default!;

        public string Login { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string Role { get; set; } = null!;

        public string Id { get; set; } = null!;
    }
}
