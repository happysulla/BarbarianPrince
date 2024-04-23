using System;
namespace BarbarianPrince
{
   public enum LogEnum
   {
      LE_ERROR,
      LE_GAME_INIT,
      LE_GAME_PARTYMEMBER_COUNT,
      LE_REMOVE_KILLED,
      LE_MOVE_STACKING,
      LE_MOVE_COUNT,
      LE_COMBAT_RESULT,
      LE_COMBAT_STATE,
      LE_COMBAT_STATE_ESCAPE,
      LE_COMBAT_STATE_ROUTE,
      LE_COMBAT_THREAD,
      LE_COMBAT_TROLL_HEAL,
      LE_NEXT_ACTION,
      LE_RESET_ROLL_STATE,
      LE_GET_COIN,
      LE_MOUNT_CHANGE,
      LE_END_ENCOUNTER,
      LE_STARVATION_STATE_CHANGE,
      LE_MAPITEM_MOVING_COUNT,
      LE_MAPITEM_WOUND,
      LE_MAPITEM_POISION,
      LE_VIEW_DICE_MOVING,
      LE_VIEW_RESET_BATTLE_GRID,
      LE_VIEW_DEC_COUNT_GRID,
      LE_VIEW_UPDATE_MENU,
      LE_VIEW_UPDATE_STATUS_BAR,
      LE_VIEW_UPDATE_EVENTVIEWER,
      LE_VIEW_UPDATE_DAILY_ACTIONS,
      LE_VIEW_MIM,
      LE_VIEW_MIM_ADD,
      LE_VIEW_MIM_CLEAR,
      LE_VIEW_SHOW_LOADS,
      LE_VIEW_SHOW_HUNT,
      LE_END_ENUM
   }

   public class Logger
   {
      const int NUM_LOG_LEVELS = (int)LogEnum.LE_END_ENUM;
      public static Boolean[] theLogLevel = new Boolean[NUM_LOG_LEVELS];
      static public void Log(LogEnum logLevel, String description)
      {
         if (true == theLogLevel[(int)logLevel])
            Console.WriteLine("{0} {1}", logLevel.ToString(), description);
      }
      static public void SetOn(LogEnum logLevel)
      {
         if ((int)logLevel < NUM_LOG_LEVELS)
            theLogLevel[(int)logLevel] = true;
      }
      static public void SetOff(LogEnum logLevel)
      {
         if ((int)logLevel < NUM_LOG_LEVELS)
            theLogLevel[(int)logLevel] = false;
      }
   }
}
