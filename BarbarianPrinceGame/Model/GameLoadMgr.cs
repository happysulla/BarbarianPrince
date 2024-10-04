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
      public static string theGamesDirectory = "";
      public static bool theIsCheckFileExist = false;
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
         FileStream fileStream = null;
         try
         {
            if (false == Directory.Exists(theGamesDirectory)) // create directory if does not exists
               Directory.CreateDirectory(theGamesDirectory);
            string filename = theGamesDirectory + "Checkpoint.bpg";
            fileStream = new FileStream(filename, FileMode.Open);
            BinaryFormatter formatter = new BinaryFormatter();
            IGameInstance gi = (GameInstance)formatter.Deserialize(fileStream);
            Logger.Log(LogEnum.LE_GAME_INIT, "OpenGame(): gi=" + gi.ToString());
            fileStream.Close();
            return gi;
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenGame(): path=" + theGamesDirectory + " e =" + e.ToString());
            if (null != fileStream)
               fileStream.Close();
            return null;
         }
      }
      //--------------------------------------------------
      public static bool SaveGameToFile(IGameInstance gi)
      {
         try
         {
            if (false == Directory.Exists(theGamesDirectory)) // create directory if does not exists
               Directory.CreateDirectory(theGamesDirectory);
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "SaveGameToFile(): path=" + theGamesDirectory + " e=" + e.ToString());
            return false;
         }
         FileStream fileStream = null;
         try
         {
            string filename = theGamesDirectory + "Checkpoint.bpg";
            fileStream = File.OpenWrite(filename);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(fileStream, gi);
            fileStream.Close();
            theIsCheckFileExist = true;
            return true;
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "SaveGameToFile(): path=" + theGamesDirectory + " e =" + ex.ToString());
            Console.WriteLine(ex.ToString());
            if (null != fileStream)
               fileStream.Close();
            return false;
         }
      }
      //--------------------------------------------------
      public static IGameInstance OpenGameFromFile()
      {
         try
         {
            if (false == Directory.Exists(theGamesDirectory)) // create directory if does not exists
               Directory.CreateDirectory(theGamesDirectory);
            Directory.SetCurrentDirectory(theGamesDirectory);
         }
         catch(Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenGameFromFile(): path=" + theGamesDirectory + " e=" + e.ToString());
            return null;
         }
         FileStream fileStream = null;
         try
         {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = theGamesDirectory;
            dlg.RestoreDirectory = true;
            dlg.Filter = "Barbarin Prince Games|*.bpg";
            if (true == dlg.ShowDialog())
            {
               fileStream = new FileStream(dlg.FileName, FileMode.Open);
               BinaryFormatter formatter = new BinaryFormatter();
               IGameInstance gi = (GameInstance)formatter.Deserialize(fileStream);
               Logger.Log(LogEnum.LE_GAME_INIT, "OpenGameFromFile(): gi=" + gi.ToString());
               fileStream.Close();
               theGamesDirectory = Path.GetDirectoryName(dlg.FileName); // save off the directory user chosen
               theGamesDirectory += "\\";
               Directory.SetCurrentDirectory(AssemblyDirectory);
               return gi;
            }
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenGameFromFile(): path=" + theGamesDirectory + " e =" + e.ToString());
            if (null != fileStream)
               fileStream.Close();
         }
         Directory.SetCurrentDirectory(AssemblyDirectory);
         return null;
      }
      //--------------------------------------------------
      public static bool SaveGameAsToFile(IGameInstance gi)
      {
         try
         {
            if (false == Directory.Exists(theGamesDirectory)) // create directory if does not exists
               Directory.CreateDirectory(theGamesDirectory);
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "SaveGameAsToFile(): path=" + theGamesDirectory + " e=" + e.ToString());
            return false;
         }
         FileStream fileStream = null;
         try
         {
            SaveFileDialog dlg = new SaveFileDialog();
            string filename = GetFileName(gi);
            dlg.FileName = filename;
            dlg.InitialDirectory = theGamesDirectory;
            dlg.RestoreDirectory = true;
            if (true == dlg.ShowDialog())
            {
               fileStream = File.OpenWrite(theGamesDirectory + filename);
               BinaryFormatter formatter = new BinaryFormatter();
               formatter.Serialize(fileStream, gi);
               fileStream.Close();
               theGamesDirectory = Path.GetDirectoryName(dlg.FileName); // save off the directory user chosen
               theGamesDirectory += "\\";
            }
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "SaveGameAsToFile(): path=" + theGamesDirectory + " e =" + ex.ToString());
            Console.WriteLine(ex.ToString());
            if (null != fileStream)
               fileStream.Close();
            return false;
         }
         return true;
      }
      //--------------------------------------------------
      private static string GetFileName(IGameInstance gi)
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
