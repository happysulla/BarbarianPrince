using System;
namespace BarbarianPrince
{
   public enum LogEnum
   {
      LE_ERROR,
      LE_GAME_INIT,
      LE_USER_ACTION,
      LE_NEXT_ACTION,
      LE_UNDO_COMMAND,
      LE_GAME_PARTYMEMBER_COUNT,
      LE_PARTYMEMBER_ADD,
      LE_REMOVE_KILLED,
      LE_MOVE_STACKING,
      LE_MOVE_COUNT,
      LE_COMBAT_RESULT,
      LE_COMBAT_STATE,
      LE_COMBAT_STATE_ESCAPE,
      LE_COMBAT_STATE_ROUTE,
      LE_COMBAT_THREAD,
      LE_COMBAT_TROLL_HEAL,
      LE_COMBAT_WIZARD,
      LE_RESET_ROLL_STATE,
      LE_GET_COIN,
      LE_BRIBE,
      LE_ADD_WEALTH_CODE,
      LE_ADD_COIN,
      LE_ADD_COIN_AUTO,
      LE_ADD_FOOD,
      LE_LODGING_COST,
      LE_MOUNT_CHANGE,
      LE_END_ENCOUNTER,
      LE_HEX_WITHIN_RANGE,
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
      LE_VIEW_TRAVEL_CHECK,
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
      static public void SetInitial()
      {
         Logger.SetOn(LogEnum.LE_ERROR);
         Logger.SetOn(LogEnum.LE_GAME_INIT);
         Logger.SetOn(LogEnum.LE_USER_ACTION);
         Logger.SetOn(LogEnum.LE_NEXT_ACTION);
         Logger.SetOn(LogEnum.LE_UNDO_COMMAND);
         //Logger.SetOn(LogEnum.LE_GAME_PARTYMEMBER_COUNT);
         Logger.SetOn(LogEnum.LE_PARTYMEMBER_ADD);
         Logger.SetOn(LogEnum.LE_REMOVE_KILLED);
         //Logger.SetOn(LogEnum.LE_MOVE_STACKING);
         Logger.SetOn(LogEnum.LE_MOVE_COUNT);
         //Logger.SetOn(LogEnum.LE_RESET_ROLL_STATE);
         //Logger.SetOn(LogEnum.LE_GET_COIN);
         Logger.SetOn(LogEnum.LE_BRIBE);
         Logger.SetOn(LogEnum.LE_ADD_WEALTH_CODE);
         Logger.SetOn(LogEnum.LE_ADD_COIN);
         Logger.SetOn(LogEnum.LE_ADD_COIN_AUTO);
         //Logger.SetOn(LogEnum.LE_LODGING_COST);
         //Logger.SetOn(LogEnum.LE_MOUNT_CHANGE);
         Logger.SetOn(LogEnum.LE_COMBAT_STATE);
         //Logger.SetOn(LogEnum.LE_COMBAT_STATE_ESCAPE);
         //Logger.SetOn(LogEnum.LE_COMBAT_STATE_ROUTE);
         Logger.SetOn(LogEnum.LE_COMBAT_RESULT);
         //Logger.SetOn(LogEnum.LE_COMBAT_TROLL_HEAL);
         Logger.SetOn(LogEnum.LE_COMBAT_WIZARD);
         Logger.SetOn(LogEnum.LE_MAPITEM_WOUND);
         Logger.SetOn(LogEnum.LE_MAPITEM_POISION);
         Logger.SetOn(LogEnum.LE_END_ENCOUNTER);
         //Logger.SetOn(LogEnum.LE_HEX_WITHIN_RANGE);
         //Logger.SetOn(LogEnum.LE_STARVATION_STATE_CHANGE);
         //Logger.SetOn(LogEnum.LE_VIEW_UPDATE_WINDOW);
         //Logger.SetOn(LogEnum.LE_VIEW_UPDATE_MENU);
         //Logger.SetOn(LogEnum.LE_VIEW_UPDATE_STATUS_BAR);
         //Logger.SetOn(LogEnum.LE_VIEW_UPDATE_ACTION_PANEL);
         //Logger.SetOn(LogEnum.LE_VIEW_UPDATE_ACTION_PANEL_CLEAR);
         //Logger.SetOn(LogEnum.LE_RETURN_TO_START);
         //Logger.SetOn(LogEnum.LE_VIEW_DICE_MOVING);
         //Logger.SetOn(LogEnum.LE_VIEW_RESET_BATTLE_GRID);
         //Logger.SetOn(LogEnum.LE_VIEW_DEC_COUNT_GRID);
         Logger.SetOn(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER);
         //Logger.SetOn(LogEnum.LE_VIEW_UPDATE_DAILY_ACTIONS);
         Logger.SetOn(LogEnum.LE_VIEW_TRAVEL_CHECK);
         Logger.SetOn(LogEnum.LE_VIEW_MIM);
         Logger.SetOn(LogEnum.LE_VIEW_MIM_ADD);
         Logger.SetOn(LogEnum.LE_VIEW_MIM_CLEAR);
         //Logger.SetOn(LogEnum.LE_VIEW_SHOW_LOADS);
         //Logger.SetOn(LogEnum.LE_VIEW_SHOW_HUNT);
      }
   }
}
