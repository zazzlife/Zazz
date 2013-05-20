namespace Zazz.Core.Models.Data
{
    public class ApiApp : BaseEntity
    {
        public string Name { get; set; }

        public byte[] PasswordSigningKey { get; set; }

        public byte[] RequestSigningKey { get; set; } 
    }
}