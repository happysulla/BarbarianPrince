using System;
using System.Data.Common;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BarbarianPrince
{
   public enum LogEnum
   {
      LE_ERROR,
      LE_GAME_INIT,
      LE_GAME_INIT_VERSION,
      LE_WIT_AND_WILES_INIT,
      LE_USER_ACTION,
      LE_NEXT_ACTION,
      LE_UNDO_COMMAND,
      LE_GAME_PARTYMEMBER_COUNT,
      LE_PARTYMEMBER_ADD,
      LE_REMOVE_KIA,
      LE_PROCESS_MIA,
      LE_END_GAME,
      LE_END_GAME_CHECK,
      LE_MOVE_STACKING,
      LE_MOVE_COUNT,
      LE_COMBAT_RESULT,
      LE_COMBAT_STATE,
      LE_COMBAT_STATE_END,
      LE_COMBAT_STATE_ESCAPE,
      LE_COMBAT_STATE_ROUTE,
      LE_COMBAT_THREAD,
      LE_COMBAT_TROLL_HEAL,
      LE_COMBAT_WIZARD,
      LE_ENCOUNTER_ESCAPE,
      LE_RESET_ROLL_STATE,
      LE_GET_COIN,
      LE_BRIBE,
      LE_ADD_WEALTH_CODE,
      LE_FREE_LOAD,
      LE_ADD_FOOD,
      LE_ADD_COIN,
      LE_REDUCE_COIN,
      LE_ADD_COIN_AUTO,
      LE_GET_ITEM,
      LE_ADD_ITEM,
      LE_REMOVE_ITEM,
      LE_MANAGE_CACHE,
      LE_LODGING_COST,
      LE_MOUNT_CHANGE,
      LE_END_ENCOUNTER,
      LE_HEX_WITHIN_RANGE,
      LE_STARVATION_STATE_CHANGE,
      LE_MAPITEM_MOVING_COUNT,
      LE_MAPITEM_WOUND,
      LE_MAPITEM_POISION,
      LE_SERIALIZE_FEATS,
      LE_VIEW_APPEND_EVENT,
      LE_VIEW_SHOW_PARTY_DIALOG,
      LE_VIEW_DICE_MOVING,
      LE_VIEW_DICE_RESULT,
      LE_VIEW_DIALOG_PARTY,
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
      LE_VIEW_MAP_THUMBNAIL,
      LE_END_ENUM
   }

   public class Logger
   {
      const int NUM_LOG_LEVELS = (int)LogEnum.LE_END_ENUM;
      public static Boolean[] theLogLevel = new Boolean[NUM_LOG_LEVELS];
      public static string theLogDirectory = "";        
      private static string theFileName = "";
      private static bool theIsLogFileCreated = false;
      private static Mutex theMutex = new Mutex();
      //--------------------------------------------------
      static public bool SetInitial()
      {
         //---------------------------------------------------------------------
         try // create the file
         {
            if( false == Directory.Exists(theLogDirectory) )
                Directory.CreateDirectory(theLogDirectory);
            theFileName = theLogDirectory + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".txt";
            FileInfo f = new FileInfo(theFileName);
            f.Create();
            theIsLogFileCreated = true;
         }
         catch (DirectoryNotFoundException dirException)
         {
            Console.WriteLine("SetInitial(): create file\n" + dirException.ToString());
         }
         catch (FileNotFoundException fileException)
         {
            Console.WriteLine("SetInitial(): create file\n" + fileException.ToString());
         }
         catch (IOException ioException)
         {
            Console.WriteLine("SetInitial(): create file\n" + ioException.ToString());
         }
         catch (Exception ex)
         {
            Console.WriteLine("SetInitial(): create file\n" + ex.ToString());
         }
         //---------------------------------------------------------------------
         Logger.SetOn(LogEnum.LE_ERROR);
         //Logger.SetOn(LogEnum.LE_GAME_INIT);
         Logger.SetOn(LogEnum.LE_GAME_INIT_VERSION); 
         //Logger.SetOn(LogEnum.LE_WIT_AND_WILES_INIT);
         Logger.SetOn(LogEnum.LE_USER_ACTION);
         Logger.SetOn(LogEnum.LE_NEXT_ACTION);
         //Logger.SetOn(LogEnum.LE_UNDO_COMMAND);
         //Logger.SetOn(LogEnum.LE_GAME_PARTYMEMBER_COUNT);
         Logger.SetOn(LogEnum.LE_PARTYMEMBER_ADD);
         Logger.SetOn(LogEnum.LE_REMOVE_KIA);
         Logger.SetOn(LogEnum.LE_END_GAME);
         //Logger.SetOn(LogEnum.LE_END_GAME_CHECK);
         //Logger.SetOn(LogEnum.LE_MOVE_STACKING);
         //Logger.SetOn(LogEnum.LE_MOVE_COUNT);
         //Logger.SetOn(LogEnum.LE_RESET_ROLL_STATE);
         //Logger.SetOn(LogEnum.LE_GET_COIN);
         //Logger.SetOn(LogEnum.LE_BRIBE);
         //Logger.SetOn(LogEnum.LE_ADD_WEALTH_CODE);
         Logger.SetOn(LogEnum.LE_FREE_LOAD);
         //Logger.SetOn(LogEnum.LE_ADD_FOOD);
         Logger.SetOn(LogEnum.LE_ADD_COIN);
         //Logger.SetOn(LogEnum.LE_REDUCE_COIN);
         //Logger.SetOn(LogEnum.LE_MANAGE_CACHE);
         //Logger.SetOn(LogEnum.LE_ADD_COIN_AUTO);
         //Logger.SetOn(LogEnum.LE_GET_ITEM);
         Logger.SetOn(LogEnum.LE_ADD_ITEM);
         Logger.SetOn(LogEnum.LE_REMOVE_ITEM);
         //Logger.SetOn(LogEnum.LE_LODGING_COST);
         Logger.SetOn(LogEnum.LE_MOUNT_CHANGE);
         //Logger.SetOn(LogEnum.LE_COMBAT_STATE);
         Logger.SetOn(LogEnum.LE_COMBAT_STATE_END);
         //Logger.SetOn(LogEnum.LE_COMBAT_STATE_ESCAPE);
         //Logger.SetOn(LogEnum.LE_COMBAT_STATE_ROUTE);
         //Logger.SetOn(LogEnum.LE_COMBAT_RESULT);
         //Logger.SetOn(LogEnum.LE_COMBAT_TROLL_HEAL);
         //Logger.SetOn(LogEnum.LE_COMBAT_WIZARD);
         Logger.SetOn(LogEnum.LE_ENCOUNTER_ESCAPE);
         //Logger.SetOn(LogEnum.LE_MAPITEM_WOUND);
         //Logger.SetOn(LogEnum.LE_MAPITEM_POISION);
         Logger.SetOn(LogEnum.LE_END_ENCOUNTER);
         //Logger.SetOn(LogEnum.LE_HEX_WITHIN_RANGE);
         //Logger.SetOn(LogEnum.LE_STARVATION_STATE_CHANGE);
         Logger.SetOn(LogEnum.LE_SERIALIZE_FEATS);
         //Logger.SetOn(LogEnum.LE_RETURN_TO_START);
         //Logger.SetOn(LogEnum.LE_VIEW_UPDATE_WINDOW);
         //Logger.SetOn(LogEnum.LE_VIEW_SHOW_PARTY_DIALOG);
         //Logger.SetOn(LogEnum.LE_VIEW_UPDATE_MENU);
         //Logger.SetOn(LogEnum.LE_VIEW_UPDATE_STATUS_BAR);
         //Logger.SetOn(LogEnum.LE_VIEW_UPDATE_ACTION_PANEL);
         //Logger.SetOn(LogEnum.LE_VIEW_UPDATE_ACTION_PANEL_CLEAR);
         //Logger.SetOn(LogEnum.LE_VIEW_APPEND_EVENT);
         //Logger.SetOn(LogEnum.LE_VIEW_DIALOG_PARTY);
         //Logger.SetOn(LogEnum.LE_VIEW_DICE_MOVING);
         //Logger.SetOn(LogEnum.LE_VIEW_DICE_RESULT);
         //Logger.SetOn(LogEnum.LE_VIEW_RESET_BATTLE_GRID);
         //Logger.SetOn(LogEnum.LE_VIEW_DEC_COUNT_GRID);
         Logger.SetOn(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER);
         //Logger.SetOn(LogEnum.LE_VIEW_UPDATE_DAILY_ACTIONS);
         //Logger.SetOn(LogEnum.LE_VIEW_TRAVEL_CHECK);
         //Logger.SetOn(LogEnum.LE_VIEW_MIM);
         //Logger.SetOn(LogEnum.LE_VIEW_MIM_ADD);
         //Logger.SetOn(LogEnum.LE_VIEW_MIM_CLEAR);
         //Logger.SetOn(LogEnum.LE_VIEW_SHOW_LOADS);
         //Logger.SetOn(LogEnum.LE_VIEW_SHOW_HUNT);
         //Logger.SetOn(LogEnum.LE_VIEW_MAP_THUMBNAIL);
         return true;
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
      static public void Log(LogEnum logLevel, String description)
      {
         if (true == theLogLevel[(int)logLevel])
         {
            theMutex.WaitOne();
            Console.WriteLine("{0} {1}", logLevel.ToString(), description);
            if (false == theIsLogFileCreated)
            {
               theMutex.ReleaseMutex();
               return;
            }
            try
            {
               FileInfo file = new FileInfo(theFileName);
               if (true == File.Exists(theFileName))
               {
                  StreamWriter swriter = File.AppendText(theFileName);
                  swriter.Write(logLevel.ToString());
                  swriter.Write(" ");
                  swriter.Write(description);
                  swriter.Write("\n");
                  swriter.Close();
               }
            }
            catch (FileNotFoundException fileException)
            {
               Console.WriteLine("Log(): ll=" + logLevel.ToString() + "desc=" + description + "\n" + fileException.ToString());
            }
            catch (IOException ioException)
            {
               Console.WriteLine("Log(): ll=" + logLevel.ToString() + "desc=" + description + "\n" + ioException.ToString());
            }
            catch (Exception ex)
            {
               Console.WriteLine("Log(): ll=" + logLevel.ToString() + "desc=" + description + "\n" + ex.ToString());
            }
            theMutex.ReleaseMutex();
         }
      }
   }
}
