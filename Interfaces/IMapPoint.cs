using System;

namespace BarbarianPrince
{
    public interface IMapPoint
    {
        double X { get; set; }
        double Y { get; set; }
        String ToString();
    }
}
