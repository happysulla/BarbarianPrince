using System;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace BarbarianPrince
{
   internal class GameLoadMgr
   {
      public static string theDirectoryName = "";
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
            Logger.Log(LogEnum.LE_ERROR, "OpenGame(): path=" + theDirectoryName + " e=" + e.ToString());
            return null;
         }
         FileStream fileStream = null;
         try
         {
            string filename = theDirectoryName + @"\Checkpoint.bpg";
            fileStream = new FileStream(filename, FileMode.Open);
            BinaryFormatter formatter = new BinaryFormatter();
            IGameInstance gi = (GameInstance)formatter.Deserialize(fileStream);
            Logger.Log(LogEnum.LE_GAME_INIT, "OpenGame(): gi=" + gi.ToString());
            fileStream.Close();
            Directory.SetCurrentDirectory(AssemblyDirectory);
            return gi;
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenGame(): path=" + theDirectoryName + " e =" + e.ToString());
            if (null != fileStream)
               fileStream.Close();
            Directory.SetCurrentDirectory(AssemblyDirectory);
            return null;
         }

      }
      //--------------------------------------------------
      public static bool SaveGameToFile(IGameInstance gi)
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
            Logger.Log(LogEnum.LE_ERROR, "SaveGameToFile(): path=" + theDirectoryName + " e=" + e.ToString());
            Directory.SetCurrentDirectory(AssemblyDirectory);
            return false;
         }
         FileStream fileStream = null;
         try
         {
            string filename = theDirectoryName + @"\Checkpoint.bpg";
            fileStream = File.OpenWrite(filename);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(fileStream, gi);
            fileStream.Close();
            Directory.SetCurrentDirectory(AssemblyDirectory);
            theIsCheckFileExist = true;
            return true;
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "SaveGameToFile(): path=" + theDirectoryName + " e =" + ex.ToString());
            if (null != fileStream)
               fileStream.Close();
            Directory.SetCurrentDirectory(AssemblyDirectory);
            return false;
         }
      }
      //--------------------------------------------------
      public static IGameInstance OpenGameFromFile()
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
            Logger.Log(LogEnum.LE_ERROR, "OpenGameFromFile(): path=" + theDirectoryName + " e=" + e.ToString());
            Directory.SetCurrentDirectory(AssemblyDirectory);
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
               fileStream = new FileStream(dlg.FileName, FileMode.Open);
               BinaryFormatter formatter = new BinaryFormatter();
               IGameInstance gi = (GameInstance)formatter.Deserialize(fileStream);
               Logger.Log(LogEnum.LE_GAME_INIT, "OpenGameFromFile(): gi=" + gi.ToString());
               fileStream.Close();
               theDirectoryName = Path.GetDirectoryName(dlg.FileName); // save off the directory user chosen
               Directory.SetCurrentDirectory(AssemblyDirectory);
               return gi;
            }
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenGameFromFile(): path=" + theDirectoryName + " e =" + e.ToString());
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
            if (true == string.IsNullOrEmpty(theDirectoryName)) // use the directory name as the place to load games. If none exists, create directory name
               theDirectoryName = AssemblyDirectory + @"\Games";
            if (false == Directory.Exists(theDirectoryName)) // create directory if does not exists
               Directory.CreateDirectory(theDirectoryName);
            Directory.SetCurrentDirectory(theDirectoryName);
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "SaveGameAsToFile(): path=" + theDirectoryName + " e=" + e.ToString());
            Directory.SetCurrentDirectory(AssemblyDirectory);
            return false;
         }
         FileStream fileStream = null;
         try
         {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = getCurrentTimeDate(gi);
            if (true == dlg.ShowDialog())
            {
               fileStream = File.OpenWrite(dlg.FileName);
               BinaryFormatter formatter = new BinaryFormatter();
               formatter.Serialize(fileStream, gi);
               fileStream.Close();
               theDirectoryName = Path.GetDirectoryName(dlg.FileName); // save off the directory user chosen
            }
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "SaveGameAsToFile(): path=" + theDirectoryName + " e =" + ex.ToString());
            if (null != fileStream)
               fileStream.Close();
            Directory.SetCurrentDirectory(AssemblyDirectory);
            return false;
         }
         Directory.SetCurrentDirectory(AssemblyDirectory);
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
