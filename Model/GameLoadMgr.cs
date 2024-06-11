using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace BarbarianPrince
{
   internal class GameLoadMgr
   {
      public static string theCurrentFilename = "*.bpg";
      //--------------------------------------------------
      public static IGameInstance OpenGame()
      {
         FileStream fileStream = null;
         try
         {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.FileName = theCurrentFilename;
            if (true == dlg.ShowDialog())
            {
               theCurrentFilename = dlg.FileName;
               fileStream = File.OpenRead(dlg.FileName);
               BinaryFormatter formatter = new BinaryFormatter();
               IGameInstance gi = (GameInstance)formatter.Deserialize(fileStream);
               Logger.Log(LogEnum.LE_GAME_INIT, "OpenGame(): gi=" + gi.ToString());
               fileStream.Close();
               return gi;
            }
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenGame(): e=" + ex.ToString());
            if (null != fileStream)
               fileStream.Close();
         }
         return null;
      }
      //--------------------------------------------------
      public static bool SaveGame(IGameInstance gi)
      {
         FileStream fileStream = null;
         try
         {
            fileStream = File.OpenWrite(theCurrentFilename);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(fileStream, gi);
            fileStream.Close();
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItemSave_Click(): e=" + ex.ToString());
            if (null != fileStream)
               fileStream.Close();
            return false;
         }
         return true;
      }
      //--------------------------------------------------
      public static bool SaveGameAs(IGameInstance gi)
      {
         FileStream fileStream = null;
         try
         {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = getCurrentTimeDate();
            if (true == dlg.ShowDialog())
            {
               theCurrentFilename = dlg.FileName;
               fileStream = File.OpenRead(dlg.FileName);
               BinaryFormatter formatter = new BinaryFormatter();
               formatter.Serialize(fileStream, gi);
               fileStream.Close();
            }
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "SaveGameAs(): e=" + ex.ToString());
            if (null != fileStream)
               fileStream.Close();
            return false;
         }
         return true;
      }
      //--------------------------------------------------
      private static string getCurrentTimeDate()
      {
         string fileName = "";
         return fileName;
      }
   }
}
