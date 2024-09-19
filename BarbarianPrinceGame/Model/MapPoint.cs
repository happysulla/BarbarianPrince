using System;
using System.Text;
using System.Windows;
namespace BarbarianPrince
{
   [Serializable]
   public class MapPoint : IMapPoint
   {
      private double myX = 0.0; public double X { get => myX; set => myX = value; }
      private double myY = 0.0; public double Y { get => myY; set => myY = value; }
      private Point myCenterPoint = new Point(); public Point CenterPoint { get => myCenterPoint; set => myCenterPoint = value; }
      public MapPoint() { }
      public MapPoint(double x, double y) { myX = x; myY = y; }
      public override String ToString()
      {
         StringBuilder sb = new StringBuilder("(");
         sb.Append(this.myX.ToString("####."));
         sb.Append(",");
         sb.Append(this.myY.ToString("####."));
         sb.Append(")");
         return sb.ToString();
      }
   }
}
