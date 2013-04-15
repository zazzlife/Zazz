namespace Zazz.Web.Models
{
    public class CropViewModel : BaseLayoutViewModel
    {
        public string PhotoUrl { get; set; }

        public bool IsFacebookPhoto { get; set; }

        public double Ratio { get; set; }

        public double? X { get; set; }

        public double? X2 { get; set; }

        public double? Y { get; set; }

        public double? Y2 { get; set; }

        public double? W { get; set; }

        public double? H { get; set; }
    }
}