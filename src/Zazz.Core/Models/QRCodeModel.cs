namespace Zazz.Core.Models
{
    public class QRCodeModel
    {
        public QRCodeType Type { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }

        public string Photo { get; set; }

        public string Token { get; set; }
    }
}