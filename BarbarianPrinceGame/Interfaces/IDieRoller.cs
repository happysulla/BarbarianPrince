using System.Threading;
using System.Windows.Controls;

namespace BarbarianPrince
{
   public delegate void RollEndCallback(int dieRoll);
   public delegate void LoadEndCallback();
   public interface IDieRoller
   {
      bool CtorError { get; }
      Mutex DieMutex { get; }
      void HideDie();
      int RollStationaryDie(Canvas c, RollEndCallback cb);
      int RollStationaryDice(Canvas c, RollEndCallback cb);
      int RollMovingDie(Canvas c, RollEndCallback cb);
      int RollMovingDice(Canvas c, RollEndCallback cb);
      int Roll3MovingDice(Canvas c, RollEndCallback cb);
   }
}
