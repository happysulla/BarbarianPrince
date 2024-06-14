using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Microsoft.Win32;

namespace BarbarianPrince
{
   internal class GameLoadMgr
   {
      public static string theCurrentFilename = "*.bpg";
      public static string theLastSavedFilename = "*.bpg";
      public static string theDirectoryName = "";
      //--------------------------------------------------
      public static string AssemblyDirectory
      {
         get
         {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return System.IO.Path.GetDirectoryName(path);
         }
      }
      //--------------------------------------------------
      public static IGameInstance OpenGame()
      {
         try
         {
            if (true == string.IsNullOrEmpty(theDirectoryName)) // use the directory name as the place to load games. If none exists, create directory name
               theDirectoryName = AssemblyDirectory + @"\Games";
            if (false == Directory.Exists(theDirectoryName)) // create directory if does not exists
               Directory.CreateDirectory(theDirectoryName);
            Directory.SetCurrentDirectory(theDirectoryName);
         }
         catch(Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenGame(): path=" + theDirectoryName + " e=" + e.ToString());
            return null;
         }
         FileStream fileStream = null;
         try
         {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = theDirectoryName;
            dlg.Filter = "Barbarin Prince Games|*.bpg";
            if (true == dlg.ShowDialog())
            {
               theCurrentFilename = dlg.FileName;
               fileStream = File.OpenRead(dlg.FileName);
               BinaryFormatter formatter = new BinaryFormatter();
               IGameInstance gi = (GameInstance)formatter.Deserialize(fileStream);
               Logger.Log(LogEnum.LE_GAME_INIT, "OpenGame(): gi=" + gi.ToString());
               fileStream.Close();
               theDirectoryName = Path.GetDirectoryName(theCurrentFilename); // save off the directory user chosen
               return gi;
            }
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenGame(): path=" + theDirectoryName+ " fn=" + theCurrentFilename + " e =" + e.ToString());
            if (null != fileStream)
               fileStream.Close();
         }
         return null;
      }
      //--------------------------------------------------
      public static bool SaveGameAs(IGameInstance gi)
      {
         try
         {
            if (true == string.IsNullOrEmpty(theDirectoryName)) // use the directory name as the place to load games. If none exists, create directory name
               theDirectoryName = AssemblyDirectory + @"\Games";
            if (false == Directory.Exists(theDirectoryName)) // create directory if does not exists
               Directory.CreateDirectory(theDirectoryName);
            Directory.SetCurrentDirectory(theDirectoryName);
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "SaveGameAs(): path=" + theDirectoryName + " e=" + e.ToString());
            return false;
         }
         FileStream fileStream = null;
         try
         {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = getCurrentTimeDate(gi);
            if (true == dlg.ShowDialog())
            {
               theCurrentFilename = dlg.FileName;
               fileStream = File.OpenWrite(dlg.FileName);
               BinaryFormatter formatter = new BinaryFormatter();
               formatter.Serialize(fileStream, gi);
               fileStream.Close();
               theDirectoryName = Path.GetDirectoryName(theCurrentFilename); // save off the directory user chosen
            }
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "SaveGameAs(): path=" + theDirectoryName + " fn=" + theCurrentFilename + " e =" + ex.ToString());
            if (null != fileStream)
               fileStream.Close();
            return false;
         }
         return true;
      }
      //--------------------------------------------------
      private static string getCurrentTimeDate(IGameInstance gi)
      {
         StringBuilder sb = new StringBuilder();
         sb.Append(DateTime.Now.ToString("yyyyMMdd-HHmmss"));
         sb.Append("-D");
         int days = gi.Days + 1;
         if( days < 10 )
            sb.Append("0");
         sb.Append(days.ToString());
         sb.Append("-F");
         int food = gi.GetFoods();
         if ( food < 10 )
            sb.Append("0");
         sb.Append(food.ToString());
         sb.Append("-C");
         int coin = gi.GetCoins();
         if (coin < 10)
            sb.Append("0");
         sb.Append(coin.ToString());
         sb.Append(".bpg");
         return sb.ToString();
      }
   }
}
