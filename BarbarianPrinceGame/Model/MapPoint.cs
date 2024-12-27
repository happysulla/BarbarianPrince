using System;
using System.Text;
using System.Windows;
namespace BarbarianPrince
{
   [Serializable]
   public class MapPoint : IMapPoint
   {
      private Double myX = 0.0; public Double X { get => myX; set => myX = value; }
      private Double myY = 0.0; public Double Y { get => myY; set => myY = value; }
      private Point myCenterPoint = new Point(); public Point CenterPoint { get => myCenterPoint; set => myCenterPoint = value; }
      public MapPoint() { }
      public MapPoint(Double x, Double y) { myX = x; myY = y; }
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
