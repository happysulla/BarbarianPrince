using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Win32;
using static System.Windows.Forms.Design.AxImporter;

namespace BarbarianPrince
{
#nullable enable
   internal class GameLoadMgr
   {
      public static string theGamesDirectory = "";
      public static bool theIsCheckFileExist = false;
      private IMapItems myMapItems = new MapItems();  // used to store all mapitems when reading from XML file
      //--------------------------------------------------
      public GameLoadMgr() { }
      //--------------------------------------------------
      public IGameInstance? OpenGame()
      {
         try
         {
            if (false == Directory.Exists(theGamesDirectory)) // create directory if does not exists
               Directory.CreateDirectory(theGamesDirectory);
            string filename = theGamesDirectory + "Checkpoint.bpg";
            IGameInstance? gi = ReadXml(filename);
            if( null == gi )
            {
               Logger.Log(LogEnum.LE_ERROR, "OpenGame(): ReadXml() returned null for " + filename);
               return null;
            }
            Logger.Log(LogEnum.LE_GAME_INIT, "OpenGame(): gi=" + gi.ToString());
            return gi;
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenGame(): path=" + theGamesDirectory + " e =" + e.ToString());
            return new GameInstance();
         }
      }
      //--------------------------------------------------
      public bool SaveGameToFile(IGameInstance gi)
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
         try
         {
            string filename = theGamesDirectory + "Checkpoint.bpg";
            XmlDocument? aXmlDocument = CreateXml(gi); // create a new XML document 
            if (null == aXmlDocument)
            {
               Logger.Log(LogEnum.LE_ERROR, "SaveGameToFile(): CreateXml() returned null for path=" + theGamesDirectory);
               return false;
            }
            using (FileStream writer = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write))
            {
               XmlWriterSettings settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true, NewLineOnAttributes = false };
               using (XmlWriter xmlWriter = XmlWriter.Create(writer, settings)) // For XmlWriter, it uses the stream that was created: writer.
               {
                  aXmlDocument.Save(xmlWriter);
               }
            }
            theIsCheckFileExist = true;
            return true;
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "SaveGameToFile(): path=" + theGamesDirectory + " e =" + ex.ToString());
            System.Diagnostics.Debug.WriteLine(ex.ToString());
            return false;
         }
      }
      //--------------------------------------------------
      public IGameInstance? OpenGameFromFile()
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
         try
         {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = theGamesDirectory;
            dlg.RestoreDirectory = true;
            dlg.Filter = "Barbarin Prince Games|*.bpg";
            if (true == dlg.ShowDialog())
            {
               IGameInstance? gi = ReadXml(dlg.FileName);
               if (null == gi)
               {
                  Directory.SetCurrentDirectory(MainWindow.theAssemblyDirectory);
                  Logger.Log(LogEnum.LE_ERROR, "OpenGame(): ReadXml(=) returned null for " + dlg.FileName);
                  return null;
               }
               Logger.Log(LogEnum.LE_GAME_INIT, "OpenGameFromFile(): gi=" + gi.ToString());
               string? gamePath = Path.GetDirectoryName(dlg.FileName); // save off the directory user chosen
               if( null == gamePath)
               {
                  Directory.SetCurrentDirectory(MainWindow.theAssemblyDirectory);
                  Logger.Log(LogEnum.LE_ERROR, "OpenGameFromFile(): Path.GetDirectoryName() returned null for fn=" + dlg.FileName);
                  return null;
               }
               theGamesDirectory = gamePath;
               theGamesDirectory += "\\";
               Directory.SetCurrentDirectory(MainWindow.theAssemblyDirectory);
               return gi;
            }
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenGameFromFile(): path=" + theGamesDirectory + " e =" + e.ToString());
         }
         Directory.SetCurrentDirectory(MainWindow.theAssemblyDirectory);
         return null;
      }
      //--------------------------------------------------
      public bool SaveGameAsToFile(IGameInstance gi)
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
         try
         {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            string filename = GetFileName(gi);
            dlg.FileName = filename;
            dlg.InitialDirectory = theGamesDirectory;
            dlg.RestoreDirectory = true;
            if (true == dlg.ShowDialog())
            {
               XmlDocument? aXmlDocument = CreateXml(gi); // create a new XML document 
               if( null == aXmlDocument)
               {
                  Logger.Log(LogEnum.LE_ERROR, "SaveGameAsToFile(): CreateXml() returned null for path=" + theGamesDirectory );
                  return false;
               }
               using (FileStream writer = new FileStream(dlg.FileName, FileMode.OpenOrCreate, FileAccess.Write))
               {
                  XmlWriterSettings settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true, NewLineOnAttributes = false };
                  using (XmlWriter xmlWriter = XmlWriter.Create(writer, settings)) // For XmlWriter, it uses the stream that was created: writer.
                  {
                     aXmlDocument.Save(xmlWriter);
                  }
               }
               string? gamePath = Path.GetDirectoryName(dlg.FileName); // save off the directory user chosen
               if (null == gamePath)
               {
                  Logger.Log(LogEnum.LE_ERROR, "SaveGameAsToFile(): Path.GetDirectoryName() returned null for fn=" + dlg.FileName);
                  return false;
               }
               theGamesDirectory = gamePath; // save off the directory user chosen
               theGamesDirectory += "\\";
            }
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "SaveGameAsToFile(): path=" + theGamesDirectory + " e =" + ex.ToString());
            return false;
         }
         return true;
      }
      //--------------------------------------------------
      private string GetFileName(IGameInstance gi)
      {
         StringBuilder sb = new StringBuilder();
         sb.Append(DateTime.Now.ToString("yyyyMMdd-HHmmss"));
         sb.Append("-D");
         int days = gi.Days + 1;
         if (days < 100)
            sb.Append("0");
         if ( days < 10 )
            sb.Append("0");
         sb.Append(days.ToString());
         sb.Append("-F");
         int food = gi.GetFoods();
         if (food < 100)
            sb.Append("0");
         if ( food < 10 )
            sb.Append("0");
         sb.Append(food.ToString());
         sb.Append("-C");
         int coin = gi.GetCoins();
         if (coin < 100)
            sb.Append("0");
         if (coin < 10)
            sb.Append("0");
         sb.Append(coin.ToString());
         sb.Append(".bpg");
         return sb.ToString();
      }
      //--------------------------------------------------
      private int GetMajorVersion()
      {
         Assembly assembly = Assembly.GetExecutingAssembly();
         if (null == assembly)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): Assembly.GetExecutingAssembly()=null");
            return -1;
         }
         Version? versionRunning = assembly.GetName().Version;
         if (null == versionRunning)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml():  assembly.GetName().Version=null");
            return -1;
         }
         return versionRunning.Major;  
      }
      //--------------------------------------------------
      private IGameInstance? ReadXml(string filename)
      {
         IGameInstance gi = new GameInstance();
         IMapItems mapItems1 = new MapItems();
         ITerritories territories = new Territories();
         XmlTextReader? reader = null;
         try
         {
            // Load the reader with the data file and ignore all white space nodes.
            reader = new XmlTextReader(filename) { WhitespaceHandling = WhitespaceHandling.None };
            reader.Read();
            if( true == reader.IsStartElement())
            {
               if (reader.Name != "GameInstance")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): first node is not GameInstance");
                  return null;
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "Version")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sVersion = reader.GetAttribute("value");
                  if (null == sVersion)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): version=null");
                     return null;
                  }
                  int version = Int32.Parse(sVersion);
                  if ( version != GetMajorVersion() )
                  {
                     MessageBox.Show("Unable to open due to version mismatch. File v" + version + " does not match running v" + GetMajorVersion() + ".");
                     return null;
                  }
               }
            }
            //----------------------------------------------
            if( false == ReadXmlOptions(reader, gi.Options))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlOptions() returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlGameStat(reader, gi.Statistic))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlGameStat() returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlGamePartyMembers(reader, gi))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlGamePartyMembers() returned false");
               return null;
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "WitAndWile")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(WitAndWile)=null");
                     return null;
                  }
                  gi.WitAndWile = Int32.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "WitAndWileInitial")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(WitAndWileInitial)=null");
                     return null;
                  }
                  gi.WitAndWileInitial = Int32.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "Days")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(Days)=null");
                     return null;
                  }
                  gi.Days = Int32.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "EventActive")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  gi.EventActive = reader.GetAttribute("value");
                  if (null == gi.EventActive)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(EventActive)=null");
                     return null;
                  }
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "EventDisplayed")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  gi.EventDisplayed = reader.GetAttribute("value");
                  if (null == gi.EventActive)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(EventDisplayed)=null");
                     return null;
                  }
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "EventStart")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  gi.EventStart = reader.GetAttribute("value");
                  if (null == gi.EventStart)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(EventStart)=null");
                     return null;
                  }
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "GameTurn")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(GameTurn)=null");
                     return null;
                  }
                  gi.GameTurn = Int32.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsMarkOfCain")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsMarkOfCain)=null");
                     return null;
                  }
                  gi.IsMarkOfCain = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsEnslaved")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsEnslaved)=null");
                     return null;
                  }
                  gi.IsEnslaved = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsSpellBound")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsSpellBound)=null");
                     return null;
                  }
                  gi.IsSpellBound = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "WanderingDayCount")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(WanderingDayCount)=null");
                     return null;
                  }
                  gi.WanderingDayCount = Int32.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsBlessed")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsBlessed)=null");
                     return null;
                  }
                  gi.IsBlessed = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsArchTravelKnown")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsArchTravelKnown)=null");
                     return null;
                  }
                  gi.IsArchTravelKnown = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsMerchantWithParty")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsMerchantWithParty)=null");
                     return null;
                  }
                  gi.IsMerchantWithParty = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsJailed")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsJailed)=null");
                     return null;
                  }
                  gi.IsJailed = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsDungeon")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsDungeon)=null");
                     return null;
                  }
                  gi.IsDungeon = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "NightsInDungeon")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(NightsInDungeon)=null");
                     return null;
                  }
                  gi.NightsInDungeon = Int32.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsWoundedWarriorRest")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsWoundedWarriorRest)=null");
                     return null;
                  }
                  gi.IsWoundedWarriorRest = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "NumMembersBeingFollowed")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(NumMembersBeingFollowed)=null");
                     return null;
                  }
                  gi.NumMembersBeingFollowed = Int32.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsHighPass")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsHighPass)=null");
                     return null;
                  }
                  gi.IsHighPass = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "EventAfterRedistribute")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  gi.EventAfterRedistribute = reader.GetAttribute("value");
                  if (null == gi.EventAfterRedistribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(EventAfterRedistribute)=null");
                     return null;
                  }
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsImpassable")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsImpassable)=null");
                     return null;
                  }
                  gi.IsImpassable = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsFlood")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsFlood)=null");
                     return null;
                  }
                  gi.IsFlood = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsFloodContinue")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsFloodContinue)=null");
                     return null;
                  }
                  gi.IsFloodContinue = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsMountsSick")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsMountsSick)=null");
                     return null;
                  }
                  gi.IsMountsSick = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsFalconFed")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsFalconFed)=null");
                     return null;
                  }
                  gi.IsFalconFed = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsEagleHunt")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsEagleHunt)=null");
                     return null;
                  }
                  gi.IsEagleHunt = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsExhausted")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsExhausted)=null");
                     return null;
                  }
                  gi.IsExhausted = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "RaftState")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(RaftState)=null");
                     return null;
                  }
                  switch(sAttribute)
                  {
                     case "RE_NO_RAFT": gi.RaftState = RaftEnum.RE_NO_RAFT; break;
                     case "RE_RAFT_SHOWN": gi.RaftState = RaftEnum.RE_RAFT_SHOWN; break;
                     case "RE_RAFT_CHOSEN": gi.RaftState = RaftEnum.RE_RAFT_CHOSEN; break;
                     case "RE_RAFT_ENDS_TODAY": gi.RaftState = RaftEnum.RE_RAFT_ENDS_TODAY; break;
                     default:
                        Logger.Log(LogEnum.LE_ERROR, "ReadXml(): reached default rs=" + sAttribute);
                        return null;
                  }
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsWoundedBlackKnightRest")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsWoundedBlackKnightRest)=null");
                     return null;
                  }
                  gi.IsWoundedBlackKnightRest = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsTrainHorse")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsTrainHorse)=null");
                     return null;
                  }
                  gi.IsTrainHorse = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsBadGoing")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsBadGoing)=null");
                     return null;
                  }
                  gi.IsBadGoing = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsHeavyRain")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsHeavyRain)=null");
                     return null;
                  }
                  gi.IsHeavyRain = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsHeavyRainNextDay")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsHeavyRainNextDay)=null");
                     return null;
                  }
                  gi.IsHeavyRainNextDay = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsHeavyRainContinue")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsHeavyRainContinue)=null");
                     return null;
                  }
                  gi.IsHeavyRainContinue = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsHeavyRainDismount")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsHeavyRainDismount)=null");
                     return null;
                  }
                  gi.IsHeavyRainDismount = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "HydraTeethCount")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(HydraTeethCount)=null");
                     return null;
                  }
                  gi.HydraTeethCount = Int32.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsHuldraHeirKilled")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsHuldraHeirKilled)=null");
                     return null;
                  }
                  gi.IsHuldraHeirKilled = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "DayOfLastOffering")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(DayOfLastOffering)=null");
                     return null;
                  }
                  gi.DayOfLastOffering = Int32.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsPartyContinuouslyLodged")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsPartyContinuouslyLodged)=null");
                     return null;
                  }
                  gi.IsPartyContinuouslyLodged = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsTrueLoveHeartBroken")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsTrueLoveHeartBroken)=null");
                     return null;
                  }
                  gi.IsTrueLoveHeartBroken = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsMustLeaveHex")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsMustLeaveHex)=null");
                     return null;
                  }
                  gi.IsMustLeaveHex = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "NumMonsterKill")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(NumMonsterKill)=null");
                     return null;
                  }
                  gi.NumMonsterKill = Int32.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsOmenModifier")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsOmenModifier)=null");
                     return null;
                  }
                  gi.IsOmenModifier = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsInfluenceModifier")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsInfluenceModifier)=null");
                     return null;
                  }
                  gi.IsInfluenceModifier = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsSecretTempleKnown")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsSecretTempleKnown)=null");
                     return null;
                  }
                  gi.IsSecretTempleKnown = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "ChagaDrugCount")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(ChagaDrugCount)=null");
                     return null;
                  }
                  gi.ChagaDrugCount = Int32.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsChagaDrugProvided")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsChagaDrugProvided)=null");
                     return null;
                  }
                  gi.IsChagaDrugProvided = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsSecretBaronHuldra")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsSecretBaronHuldra)=null");
                     return null;
                  }
                  gi.IsSecretBaronHuldra = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsSecretLadyAeravir")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsSecretLadyAeravir)=null");
                     return null;
                  }
                  gi.IsSecretLadyAeravir = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            reader.Read();
            if (reader.IsStartElement())
            {
               if (reader.Name != "IsSecretCountDrogat")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml(): node=" + reader.Name);
                  return null;
               }
               else
               {
                  string? sAttribute = reader.GetAttribute("value");
                  if (null == sAttribute)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml(): GetAttribute(IsSecretCountDrogat)=null");
                     return null;
                  }
                  gi.IsSecretCountDrogat = Boolean.Parse(sAttribute);
               }
            }
            //----------------------------------------------
            if (false == ReadXmlAtRiskMounts(reader, gi))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlAtRiskMounts() returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlLostTrueLoves(reader, gi))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlLostTrueLoves() returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlForbiddenAudiences(reader, gi))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlForbiddenAudiences() returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlCaches(reader, gi.Caches))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlCaches() returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlTerritories(reader, gi.DwarfAdviceLocations, "DwarfAdviceLocations"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlTerritories(DwarfAdviceLocations) returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlTerritories(reader, gi.WizardAdviceLocations, "WizardAdviceLocations"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlTerritories(WizardAdviceLocations) returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlTerritories(reader, gi.Arches, "Arches"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlTerritories(Arches) returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlTerritories(reader, gi.VisitedLocations, "VisitedLocations"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlTerritories(VisitedLocations) returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlTerritories(reader, gi.EscapedLocations, "EscapedLocations"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlTerritories(EscapedLocations) returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlTerritories(reader, gi.GoblinKeeps, "GoblinKeeps"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlTerritories(GoblinKeeps) returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlTerritories(reader, gi.DwarvenMines, "DwarvenMines"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlTerritories(DwarvenMines) returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlTerritories(reader, gi.OrcTowers, "OrcTowers"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlTerritories(OrcTowers) returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlTerritories(reader, gi.WizardTowers, "WizardTowers"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlTerritories(WizardTowers) returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlTerritories(reader, gi.PixieAdviceLocations, "PixieAdviceLocations"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlTerritories(PixieAdviceLocations) returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlTerritories(reader, gi.HalflingTowns, "HalflingTowns"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlTerritories(HalflingTowns) returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlTerritories(reader, gi.RuinsUnstable, "RuinsUnstable"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlTerritories(RuinsUnstable) returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlTerritories(reader, gi.HiddenRuins, "HiddenRuins"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlTerritories(HiddenRuins) returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlTerritories(reader, gi.HiddenTowns, "HiddenTowns"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlTerritories(HiddenTowns) returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlTerritories(reader, gi.HiddenTemples, "HiddenTemples"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlTerritories(HiddenTemples) returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlTerritories(reader, gi.KilledLocations, "KilledLocations"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlTerritories(KilledLocations) returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlTerritories(reader, gi.EagleLairs, "EagleLairs"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlTerritories(EagleLairs) returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlTerritories(reader, gi.SecretClues, "SecretClues"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlTerritories(SecretClues) returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlTerritories(reader, gi.LetterOfRecommendations, "LetterOfRecommendations"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlTerritories(LetterOfRecommendations) returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlTerritories(reader, gi.Purifications, "Purifications"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlTerritories(Purifications) returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlTerritories(reader, gi.ElfTowns, "ElfTowns"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlTerritories(ElfTowns) returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlTerritories(reader, gi.ElfCastles, "ElfCastles"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlTerritories(ElfCastles) returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlTerritories(reader, gi.FeelAtHomes, "FeelAtHomes"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlTerritories(FeelAtHomes) returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlTerritories(reader, gi.SecretRites, "SecretRites"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlTerritories(SecretRites) returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlTerritories(reader, gi.CheapLodgings, "CheapLodgings"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlTerritories(CheapLodgings) returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlTerritories(reader, gi.ForbiddenHexes, "ForbiddenHexes"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlTerritories(ForbiddenHexes) returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlTerritories(reader, gi.AbandonedTemples, "AbandonedTemples"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlTerritories(AbandonedTemples) returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlTerritories(reader, gi.ForbiddenHires, "ForbiddenHires"))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlTerritories(ForbiddenHires) returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlEnteredHexes(reader, gi.EnteredHexes))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml(): ReadXmlEnteredHexes() returned false");
               return null;
            }
            //----------------------------------------------
            return gi;
         } // try
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXml():\n" + e.ToString());
            return null;
         }
         finally
         {
            if (reader != null)
               reader.Close();
         }
      }
      private bool ReadXmlOptions(XmlReader reader, Options options)
      {
         options.Clear();
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): reader.IsStartElement() = false");
            return false;
         }
         if (reader.Name != "Options")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): Options != (node=" + reader.Name + ")");
            return false;
         }
         string? sCount = reader.GetAttribute("count");
         if( null == sCount)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): Count=null");
            return false;
         }
         //-------------------------------------
         int count = Int32.Parse(sCount);
         for (int i = 0; i < count; ++i)
         {
            reader.Read();
            if (true == reader.IsStartElement())
            {
               if (reader.Name != "Option")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): Option != " + reader.Name);
                  return false;
               }
               string? name = reader.GetAttribute("Name");
               if (name == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): Name=null");
                  return false;
               }
               string? sEnabled = reader.GetAttribute("IsEnabled");
               if (sEnabled == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): IsEnabled=null");
                  return false;
               }
               bool isEnabled = Boolean.Parse(sEnabled);
               Option option = new Option(name, isEnabled);
               options.Add(option);
            }
         }
         reader.Read(); // get past </Options>
         return true;
      }
      private bool ReadXmlGameStat(XmlReader reader, GameStat statistic)
      {
         statistic.Clear();
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): reader.IsStartElement()=false");
            return false;
         }
         if (reader.Name != "GameStat")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): GameStat != (node=" + reader.Name + ")");
            return false;
         }
         //---------------------------------------------
         reader.Read();
         if (true == reader.IsStartElement())
         {
            if (reader.Name != "myDaysLost")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): myDaysLost != (node=" + reader.Name + ")");
               return false;
            }
            else
            {
               string? value = reader.GetAttribute("value");
               if (value == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): value=null");
                  return false;
               }
               statistic.myDaysLost = Int32.Parse(value);
            }
         }
         //---------------------------------------------
         reader.Read();
         if (true == reader.IsStartElement())
         {
            if (reader.Name != "myNumEncounters")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): myNumEncounters != (node=" + reader.Name + ")");
               return false;
            }
            else
            {
               string? value = reader.GetAttribute("value");
               if (value == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): value=null");
                  return false;
               }
               statistic.myNumEncounters = Int32.Parse(value);
            }
         }
         //---------------------------------------------
         reader.Read();
         if (true == reader.IsStartElement())
         {
            if (reader.Name != "myNumOfRestDays")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): myNumOfRestDays != (node=" + reader.Name + ")");
               return false;
            }
            else
            {
               string? value = reader.GetAttribute("value");
               if (value == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): value=null");
                  return false;
               }
               statistic.myNumOfRestDays = Int32.Parse(value);
            }
         }
         //---------------------------------------------
         reader.Read();
         if (true == reader.IsStartElement())
         {
            if (reader.Name != "myNumOfAudienceAttempt")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): myNumOfAudienceAttempt != (node=" + reader.Name + ")");
               return false;
            }
            else
            {
               string? value = reader.GetAttribute("value");
               if (value == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): value=null");
                  return false;
               }
               statistic.myNumOfAudienceAttempt = Int32.Parse(value);
            }
         }
         //---------------------------------------------
         reader.Read();
         if (true == reader.IsStartElement())
         {
            if (reader.Name != "myNumOfAudience")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): myNumOfAudience != (node=" + reader.Name + ")");
               return false;
            }
            else
            {
               string? value = reader.GetAttribute("value");
               if (value == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): value=null");
                  return false;
               }
               statistic.myNumOfAudience = Int32.Parse(value);
            }
         }
         //---------------------------------------------
         reader.Read();
         if (true == reader.IsStartElement())
         {
            if (reader.Name != "myNumOfOffering")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): myNumOfOffering != (node=" + reader.Name + ")");
               return false;
            }
            else
            {
               string? value = reader.GetAttribute("value");
               if (value == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): value=null");
                  return false;
               }
               statistic.myNumOfOffering = Int32.Parse(value);
            }
         }
         //---------------------------------------------
         reader.Read();
         if (true == reader.IsStartElement())
         {
            if (reader.Name != "myDaysInJailorDungeon")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): myDaysInJailorDungeon != (node=" + reader.Name + ")");
               return false;
            }
            else
            {
               string? value = reader.GetAttribute("value");
               if (value == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): value=null");
                  return false;
               }
               statistic.myDaysInJailorDungeon = Int32.Parse(value);
            }
         }
         //---------------------------------------------
         reader.Read();
         if (true == reader.IsStartElement())
         {
            if (reader.Name != "myNumRiverCrossingSuccess")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): myNumRiverCrossingSuccess != (node=" + reader.Name + ")");
               return false;
            }
            else
            {
               string? value = reader.GetAttribute("value");
               if (value == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): value=null");
                  return false;
               }
               statistic.myNumRiverCrossingSuccess = Int32.Parse(value);
            }
         }
         //---------------------------------------------
         reader.Read();
         if (true == reader.IsStartElement())
         {
            if (reader.Name != "myNumRiverCrossingFailure")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): myNumRiverCrossingFailure != (node=" + reader.Name + ")");
               return false;
            }
            else
            {
               string? value = reader.GetAttribute("value");
               if (value == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): value=null");
                  return false;
               }
               statistic.myNumRiverCrossingFailure = Int32.Parse(value);
            }
         }
         //---------------------------------------------
         reader.Read();
         if (true == reader.IsStartElement())
         {
            if (reader.Name != "myNumDaysOnRaft")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): myNumDaysOnRaft != (node=" + reader.Name + ")");
               return false;
            }
            else
            {
               string? value = reader.GetAttribute("value");
               if (value == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): value=null");
                  return false;
               }
               statistic.myNumDaysOnRaft = Int32.Parse(value);
            }
         }
         //---------------------------------------------
         reader.Read();
         if (true == reader.IsStartElement())
         {
            if (reader.Name != "myNumDaysAirborne")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): myNumDaysAirborne != (node=" + reader.Name + ")");
               return false;
            }
            else
            {
               string? value = reader.GetAttribute("value");
               if (value == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): value=null");
                  return false;
               }
               statistic.myNumDaysAirborne = Int32.Parse(value);
            }
         }
         //---------------------------------------------
         reader.Read();
         if (true == reader.IsStartElement())
         {
            if (reader.Name != "myNumDaysArchTravel")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): myNumDaysArchTravel != (node=" + reader.Name + ")");
               return false;
            }
            else
            {
               string? value = reader.GetAttribute("value");
               if (value == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): value=null");
                  return false;
               }
               statistic.myNumDaysArchTravel = Int32.Parse(value);
            }
         }
         //---------------------------------------------
         reader.Read();
         if (true == reader.IsStartElement())
         {
            if (reader.Name != "myMaxPartySize")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): myMaxPartySize != (node=" + reader.Name + ")");
               return false;
            }
            else
            {
               string? value = reader.GetAttribute("value");
               if (value == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): value=null");
                  return false;
               }
               statistic.myMaxPartySize = Int32.Parse(value);
            }
         }
         //---------------------------------------------
         reader.Read();
         if (true == reader.IsStartElement())
         {
            if (reader.Name != "myMaxPartyEndurance")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): myMaxPartyEndurance != (node=" + reader.Name + ")");
               return false;
            }
            else
            {
               string? value = reader.GetAttribute("value");
               if (value == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): value=null");
                  return false;
               }
               statistic.myMaxPartyEndurance = Int32.Parse(value);
            }
         }
         //---------------------------------------------
         reader.Read();
         if (true == reader.IsStartElement())
         {
            if (reader.Name != "myMaxPartyCombat")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): myMaxPartyCombat != (node=" + reader.Name + ")");
               return false;
            }
            else
            {
               string? value = reader.GetAttribute("value");
               if (value == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): value=null");
                  return false;
               }
               statistic.myMaxPartyCombat = Int32.Parse(value);
            }
         }
         //---------------------------------------------
         reader.Read();
         if (true == reader.IsStartElement())
         {
            if (reader.Name != "myNumOfPartyKilled")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): myNumOfPartyKilled != (node=" + reader.Name + ")");
               return false;
            }
            else
            {
               string? value = reader.GetAttribute("value");
               if (value == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): value=null");
                  return false;
               }
               statistic.myNumOfPartyKilled = Int32.Parse(value);
            }
         }
         //---------------------------------------------
         reader.Read();
         if (true == reader.IsStartElement())
         {
            if (reader.Name != "myNumOfPartyHeal")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): myNumOfPartyHeal != (node=" + reader.Name + ")");
               return false;
            }
            else
            {
               string? value = reader.GetAttribute("value");
               if (value == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): value=null");
                  return false;
               }
               statistic.myNumOfPartyHeal = Int32.Parse(value);
            }
         }
         //---------------------------------------------
         reader.Read();
         if (true == reader.IsStartElement())
         {
            if (reader.Name != "myNumOfPartyKill")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): myNumOfPartyKill != (node=" + reader.Name + ")");
               return false;
            }
            else
            {
               string? value = reader.GetAttribute("value");
               if (value == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): value=null");
                  return false;
               }
               statistic.myNumOfPartyKill = Int32.Parse(value);
            }
         }
         //---------------------------------------------
         reader.Read();
         if (true == reader.IsStartElement())
         {
            if (reader.Name != "myNumOfPartyKillEndurance")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): myNumOfPartyKillEndurance != (node=" + reader.Name + ")");
               return false;
            }
            else
            {
               string? value = reader.GetAttribute("value");
               if (value == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): value=null");
                  return false;
               }
               statistic.myNumOfPartyKillEndurance = Int32.Parse(value);
            }
         }
         //---------------------------------------------
         reader.Read();
         if (true == reader.IsStartElement())
         {
            if (reader.Name != "myNumOfPartyKillCombat")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): myNumOfPartyKillCombat != (node=" + reader.Name + ")");
               return false;
            }
            else
            {
               string? value = reader.GetAttribute("value");
               if (value == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): value=null");
                  return false;
               }
               statistic.myNumOfPartyKillCombat = Int32.Parse(value);
            }
         }
         //---------------------------------------------
         reader.Read();
         if (true == reader.IsStartElement())
         {
            if (reader.Name != "myNumOfPrinceKill")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): myNumOfPrinceKill != (node=" + reader.Name + ")");
               return false;
            }
            else
            {
               string? value = reader.GetAttribute("value");
               if (value == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): value=null");
                  return false;
               }
               statistic.myNumOfPrinceKill = Int32.Parse(value);
            }
         }
         //---------------------------------------------
         reader.Read();
         if (true == reader.IsStartElement())
         {
            if (reader.Name != "myNumOfPrinceHeal")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): myNumOfPrinceHeal != (node=" + reader.Name + ")");
               return false;
            }
            else
            {
               string? value = reader.GetAttribute("value");
               if (value == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): value=null");
                  return false;
               }
               statistic.myNumOfPrinceHeal = Int32.Parse(value);
            }
         }
         //---------------------------------------------
         reader.Read();
         if (true == reader.IsStartElement())
         {
            if (reader.Name != "myNumOfPrinceStarveDays")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): myNumOfPrinceStarveDays != (node=" + reader.Name + ")");
               return false;
            }
            else
            {
               string? value = reader.GetAttribute("value");
               if (value == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): value=null");
                  return false;
               }
               statistic.myNumOfPrinceStarveDays = Int32.Parse(value);
            }
         }
         //---------------------------------------------
         reader.Read();
         if (true == reader.IsStartElement())
         {
            if (reader.Name != "myNumOfPrinceUncounscious")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): myNumOfPrinceUncounscious != (node=" + reader.Name + ")");
               return false;
            }
            else
            {
               string? value = reader.GetAttribute("value");
               if (value == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): value=null");
                  return false;
               }
               statistic.myNumOfPrinceUncounscious = Int32.Parse(value);
            }
         }
         //---------------------------------------------
         reader.Read();
         if (true == reader.IsStartElement())
         {
            if (reader.Name != "myNumOfPrinceResurrection")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): myNumOfPrinceResurrection != (node=" + reader.Name + ")");
               return false;
            }
            else
            {
               string? value = reader.GetAttribute("value");
               if (value == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): value=null");
                  return false;
               }
               statistic.myNumOfPrinceResurrection = Int32.Parse(value);
            }
         }
         //---------------------------------------------
         reader.Read();
         if (true == reader.IsStartElement())
         {
            if (reader.Name != "myNumOfPrinceAxeDeath")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): myNumOfPrinceAxeDeath != (node=" + reader.Name + ")");
               return false;
            }
            else
            {
               string? value = reader.GetAttribute("value");
               if (value == null)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): value=null");
                  return false;
               }
               statistic.myNumOfPrinceAxeDeath = Int32.Parse(value);
            }
         }
         reader.Read(); // get past </GameStat> tag
         return true;
      }
      private bool ReadXmlGamePartyMembers(XmlReader reader, IGameInstance gi)
      {
         gi.PartyMembers.Clear(); // clear out all party members
         reader.Read();
         if (reader.IsStartElement())
         {
            if (reader.Name != "PartyMembers")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): PartyMembers != (node=" + reader.Name + ")");
               return false;
            }
            string? sCount = reader.GetAttribute("count");
            if (null == sCount)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): Count=null");
               return false;
            }
            int count = Int32.Parse(sCount);
            Dictionary<string, string> flyersWithRiders = new Dictionary<string, string>();  // flyer name, rider name
            for (int i = 0; i < count; ++i)
            {
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "MapItem")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): MapItem != (node=" + reader.Name + ")");
                     return false;
                  }
               }
               //--------------------------------------
               string? miName = "";
               string? topMapImage = "";
               int endurance = 0;
               int combat = 0;
               int wealthCode = 0;
               ITerritory? territory = null;
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "Name")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): Name != (node=" + reader.Name + ")");
                     return false;
                  }
                  miName = reader.GetAttribute("value");
                  if (null == miName)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(miName) returned null");
                     return false;
                  }
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "TopImageName")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): TopImageName != (node=" + reader.Name + ")");
                     return false;
                  }
                  topMapImage = reader.GetAttribute("value");
                  if (null == topMapImage)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(topMapImage) returned null");
                     return false;
                  }
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "Endurance")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): Endurance != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sEndurance = reader.GetAttribute("value");
                  if (null == sEndurance)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(Endurance) returned null");
                     return false;
                  }
                  endurance = Int32.Parse(sEndurance);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "Combat")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): Combat != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sCombat = reader.GetAttribute("value");
                  if (null == sCombat)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(Combat) returned null");
                     return false;
                  }
                  combat = Int32.Parse(sCombat);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "WealthCode")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): WealthCode != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sWealthCode = reader.GetAttribute("value");
                  if (null == sWealthCode)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(WealthCode) returned null");
                     return false;
                  }
                  wealthCode = Int32.Parse(sWealthCode);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "Territory")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): Territory != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? tName = reader.GetAttribute("value");
                  if (null == tName)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(Territory) returned null");
                     return false;
                  }
                  territory = Territory.theTerritories.Find(tName);
                  if( null == territory)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers():  Territory.theTerritories.Find() returned null for tName=" + tName);
                     return false;
                  }
               }
               IMapItem mapItem = new MapItem(miName, 1.0, false, false, topMapImage, topMapImage, territory, endurance, combat, wealthCode);
               if (true == miName.Contains("Prince"))
                  gi.Prince = mapItem;
               //=========================================================================
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "OverlayImageName")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): OverlayImageName != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? overlayImageName = reader.GetAttribute("value");
                  if (null == overlayImageName)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(OverlayImageName) returned null");
                     return false;
                  }
                  mapItem.OverlayImageName = overlayImageName;
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "Movement")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): Movement != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sMovement = reader.GetAttribute("value");
                  if (null == sMovement)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(sMovement) returned null");
                     return false;
                  }
                  mapItem.Movement = Int32.Parse(sMovement);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "Wound")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): Wound != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sWound = reader.GetAttribute("value");
                  if (null == sWound)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(Wound) returned null");
                     return false;
                  }
                  mapItem.Wound = Int32.Parse(sWound);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "Poison")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): Poison != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sPoison = reader.GetAttribute("value");
                  if (null == sPoison)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(Poison) returned null");
                     return false;
                  }
                  mapItem.Poison = Int32.Parse(sPoison);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "Coin")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): Coin != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sCoin = reader.GetAttribute("value");
                  if (null == sCoin)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(Coin) returned null");
                     return false;
                  }
                  mapItem.Coin = Int32.Parse(sCoin);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "Food")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): Food != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sFood = reader.GetAttribute("value");
                  if (null == sFood)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(Food) returned null");
                     return false;
                  }
                  mapItem.Food = Int32.Parse(sFood);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "StarveDayNum")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): StarveDayNum != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sStarveDayNum = reader.GetAttribute("value");
                  if (null == sStarveDayNum)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(StarveDayNum) returned null");
                     return false;
                  }
                  mapItem.StarveDayNum = Int32.Parse(sStarveDayNum);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "StarveDayNumOld")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): StarveDayNumOld != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sStarveDayNumOld = reader.GetAttribute("value");
                  if (null == sStarveDayNumOld)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(StarveDayNumOld) returned null");
                     return false;
                  }
                  mapItem.StarveDayNumOld = Int32.Parse(sStarveDayNumOld);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "MovementUsed")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): MovementUsed != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sMovementUsed = reader.GetAttribute("value");
                  if (null == sMovementUsed)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(MovementUsed) returned null");
                     return false;
                  }
                  mapItem.MovementUsed = Int32.Parse(sMovementUsed);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "IsGuide")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): IsGuide != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sIsGuide = reader.GetAttribute("value");
                  if (null == sIsGuide)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(IsGuide) returned null");
                     return false;
                  }
                  mapItem.IsGuide = Boolean.Parse(sIsGuide);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "IsKilled")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): IsKilled != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sIsKilled = reader.GetAttribute("value");
                  if (null == sIsKilled)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(IsKilled) returned null");
                     return false;
                  }
                  mapItem.IsKilled = Boolean.Parse(sIsKilled);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "IsUnconscious")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): IsUnconscious != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sIsUnconscious = reader.GetAttribute("value");
                  if (null == sIsUnconscious)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(IsUnconscious) returned null");
                     return false;
                  }
                  mapItem.IsUnconscious = Boolean.Parse(sIsUnconscious);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "IsExhausted")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): IsExhausted != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sIsExhausted = reader.GetAttribute("value");
                  if (null == sIsExhausted)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(IsExhausted) returned null");
                     return false;
                  }
                  mapItem.IsExhausted = Boolean.Parse(sIsExhausted);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "IsSunStroke")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): IsSunStroke != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sIsSunStroke = reader.GetAttribute("value");
                  if (null == sIsSunStroke)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(IsSunStroke) returned null");
                     return false;
                  }
                  mapItem.IsSunStroke = Boolean.Parse(sIsSunStroke);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "IsPlagued")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): IsPlagued != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sIsPlagued = reader.GetAttribute("value");
                  if (null == sIsPlagued)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(IsPlagued) returned null");
                     return false;
                  }
                  mapItem.IsPlagued = Boolean.Parse(sIsPlagued);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "PlagueDustWound")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): PlagueDustWound != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sPlagueDustWound = reader.GetAttribute("value");
                  if (null == sPlagueDustWound)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(PlagueDustWound) returned null");
                     return false;
                  }
                  mapItem.PlagueDustWound = Int32.Parse(sPlagueDustWound);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "IsPlayedMusic")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): IsPlayedMusic != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sIsPlayedMusic = reader.GetAttribute("value");
                  if (null == sIsPlayedMusic)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(IsPlayedMusic) returned null");
                     return false;
                  }
                  mapItem.IsPlayedMusic = Boolean.Parse(sIsPlayedMusic);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "IsCatchCold")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): IsCatchCold != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sIsCatchCold = reader.GetAttribute("value");
                  if (null == sIsCatchCold)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(IsCatchCold) returned null");
                     return false;
                  }
                  mapItem.IsCatchCold = Boolean.Parse(sIsCatchCold);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "IsRiding")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): IsRiding != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sIsRiding = reader.GetAttribute("value");
                  if (null == sIsRiding)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(IsRiding) returned null");
                     return false;
                  }
                  mapItem.IsRiding = Boolean.Parse(sIsRiding);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "IsFlying")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): IsFlying != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sIsFlying = reader.GetAttribute("value");
                  if (null == sIsFlying)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(IsFlying) returned null");
                     return false;
                  }
                  mapItem.IsFlying = Boolean.Parse(sIsFlying);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "IsSecretGatewayToDarknessKnown")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): IsSecretGatewayToDarknessKnown != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sIsSecretGatewayToDarknessKnown = reader.GetAttribute("value");
                  if (null == sIsSecretGatewayToDarknessKnown)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(IsSecretGatewayToDarknessKnown) returned null");
                     return false;
                  }
                  mapItem.IsSecretGatewayToDarknessKnown = Boolean.Parse(sIsSecretGatewayToDarknessKnown);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "IsFugitive")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): IsFugitive != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sIsFugitive = reader.GetAttribute("value");
                  if (null == sIsFugitive)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(IsFugitive) returned null");
                     return false;
                  }
                  mapItem.IsFugitive = Boolean.Parse(sIsFugitive);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "IsResurrected")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): IsResurrected != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sIsResurrected = reader.GetAttribute("value");
                  if (null == sIsResurrected)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(IsResurrected) returned null");
                     return false;
                  }
                  mapItem.IsResurrected = Boolean.Parse(sIsResurrected);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "IsTrueLove")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): IsTrueLove != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sIsTrueLove = reader.GetAttribute("value");
                  if (null == sIsTrueLove)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(IsTrueLove) returned null");
                     return false;
                  }
                  mapItem.IsTrueLove = Boolean.Parse(sIsTrueLove);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "IsFickle")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): IsFickle != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sIsFickle = reader.GetAttribute("value");
                  if (null == sIsFickle)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(IsFickle) returned null");
                     return false;
                  }
                  mapItem.IsFickle = Boolean.Parse(sIsFickle);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "GroupNum")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GroupNum != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sGroupNum = reader.GetAttribute("value");
                  if (null == sGroupNum)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(GroupNum) returned null");
                     return false;
                  }
                  mapItem.GroupNum = Int32.Parse(sGroupNum);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "PayDay")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): PayDay != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sPayDay = reader.GetAttribute("value");
                  if (null == sPayDay)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(PayDay) returned null");
                     return false;
                  }
                  mapItem.PayDay = Int32.Parse(sPayDay);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "Wages")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): Wages != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sWages = reader.GetAttribute("value");
                  if (null == sWages)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(Wages) returned null");
                     return false;
                  }
                  mapItem.Wages = Int32.Parse(sWages);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "IsAlly")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): IsAlly != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sIsAlly = reader.GetAttribute("value");
                  if (null == sIsAlly)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(IsAlly) returned null");
                     return false;
                  }
                  mapItem.IsAlly = Boolean.Parse(sIsAlly);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "IsLooter")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): IsLooter != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sIsLooter = reader.GetAttribute("value");
                  if (null == sIsLooter)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(IsLooter) returned null");
                     return false;
                  }
                  mapItem.IsLooter = Boolean.Parse(sIsLooter);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "IsTownCastleTempleLeave")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): IsTownCastleTempleLeave != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sIsTownCastleTempleLeave = reader.GetAttribute("value");
                  if (null == sIsTownCastleTempleLeave)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(IsTownCastleTempleLeave) returned null");
                     return false;
                  }
                  mapItem.IsTownCastleTempleLeave = Boolean.Parse(sIsTownCastleTempleLeave);
               }
               //--------------------------------------
               if( false == ReadXmlGamePartyMembersTerritories(reader, mapItem.GuideTerritories, "GuideTerritories"))
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): ReadXmlGamePartyMembersTerritories(GuideTerritories) returned false");
                  return false;
               }
               //--------------------------------------
               if (false == ReadXmlGamePartyMembersSpecialEnums(reader, mapItem, false))
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): ReadXmlGamePartyMembersPossessions(SpecialKeeps) returned false mi=" + mapItem.Name);
                  return false;
               }
               //--------------------------------------
               if (false == ReadXmlGamePartyMembersSpecialEnums(reader, mapItem, true))
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): ReadXmlGamePartyMembersPossessions(SpecialShares) returned false for mi=" + mapItem.Name);
                  return false;
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "Rider")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): Rider != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sRider = reader.GetAttribute("value");
                  if (null == sRider)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute(IsAlly) returned null");
                     return false;
                  }
                  if ("None" != sRider)
                     flyersWithRiders[miName] = sRider;
               }
               if (false == ReadXmlGamePartyMembersMounts(reader, mapItem))
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): ReadXmlGamePartyMembersMounts() returned false");
                  return false;
               }
               //-----------------------------------------------------------
               gi.PartyMembers.Add(mapItem);
               reader.Read(); // get past </MapItem> tag
            } // end for()
            reader.Read(); // get past </PartyMember> tag
            //-----------------------------------------------------------
            foreach ( KeyValuePair<string, string> kvp in flyersWithRiders) // assign riders to flyers
            {
               IMapItem? rider = gi.PartyMembers.Find(kvp.Value);
               if (null == rider)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers():  partyMembers.Find(rider) returned null for " + kvp.Value);
                  return false;
               }
               IMapItem flyer  = gi.PartyMembers.Find(kvp.Key);
               if (null == flyer)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers():  partyMembers.Find(flyer) returned null for " + kvp.Key);
                  return false;
               }
               flyer.Rider = rider;
               rider.Mounts.Add(flyer);
            }
         }
         return true;
      }
      private bool ReadXmlGamePartyMembersTerritories(XmlReader reader, ITerritories territories, string nodeName)
      {
         reader.Read();
         if (reader.IsStartElement())
         {
            if (reader.Name != nodeName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): " + nodeName + " != (node=" + reader.Name + ")");
               return false;
            }
            string? sCount = reader.GetAttribute("count");
            if (null == sCount)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): Count=null");
               return false;
            }
            int count = Int32.Parse(sCount);
            for (int i = 0; i < count; ++i)
            {
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "Territory")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): MapItem != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? tName = reader.GetAttribute("value");
                  if( null == tName )
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): GetAttribute() returned false");
                     return false;
                  }
                  ITerritory territory = Territory.theTerritories.Find(tName);
                  if( null == territory )
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembers(): Find() returned null for tName=" + tName);
                     return false;
                  }
                  territories.Add(territory);   
               }
            }
            if( 0 < count )
               reader.Read(); // get past end tag
         }
         return true;
      }
      private bool ReadXmlGamePartyMembersSpecialEnums(XmlReader reader, IMapItem mi, bool isShare)
      {
         List<SpecialEnum> specialEnums = new List<SpecialEnum>();
         reader.Read();
         if (reader.IsStartElement())
         {
            string matchingNode = "SpecialKeeps";
            if(true == isShare )
               matchingNode = "SpecialShares";
            if (reader.Name != matchingNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembersPossessions(): " + matchingNode + " != (node=" + reader.Name + ")");
               return false;
            }
            string? sCount = reader.GetAttribute("count");
            if (null == sCount)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembersPossessions(): Count=null");
               return false;
            }
            int count = Int32.Parse(sCount);
            for (int i = 0; i < count; ++i)
            {
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "Possession")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembersPossessions(): Possession != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? pName = reader.GetAttribute("value");
                  if (null == pName)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembersPossessions(): GetAttribute() returned false");
                     return false;
                  }
                  switch (pName)
                  {
                     case "HealingPoition": specialEnums.Add(SpecialEnum.HealingPoition); break;
                     case "CurePoisonVial": specialEnums.Add(SpecialEnum.CurePoisonVial); break;
                     case "GiftOfCharm": specialEnums.Add(SpecialEnum.GiftOfCharm); break;
                     case "EnduranceSash": specialEnums.Add(SpecialEnum.EnduranceSash); break;
                     case "ResistanceTalisman": specialEnums.Add(SpecialEnum.ResistanceTalisman); break;
                     case "PoisonDrug": specialEnums.Add(SpecialEnum.PoisonDrug); break;
                     case "MagicSword": specialEnums.Add(SpecialEnum.MagicSword); break;
                     case "AntiPoisonAmulet": specialEnums.Add(SpecialEnum.AntiPoisonAmulet); break;
                     case "PegasusMount": specialEnums.Add(SpecialEnum.PegasusMount); break;
                     case "PegasusMountTalisman": specialEnums.Add(SpecialEnum.PegasusMountTalisman); break;
                     case "CharismaTalisman": specialEnums.Add(SpecialEnum.CharismaTalisman); break;
                     case "NerveGasBomb": specialEnums.Add(SpecialEnum.NerveGasBomb); break;
                     case "ResistanceRing": specialEnums.Add(SpecialEnum.ResistanceRing); break;
                     case "ResurrectionNecklace": specialEnums.Add(SpecialEnum.ResurrectionNecklace); break;
                     case "ShieldOfLight": specialEnums.Add(SpecialEnum.ShieldOfLight); break;
                     case "RoyalHelmOfNorthlands": specialEnums.Add(SpecialEnum.RoyalHelmOfNorthlands); break;
                     case "TrollSkin": specialEnums.Add(SpecialEnum.TrollSkin); break;
                     case "DragonEye": specialEnums.Add(SpecialEnum.DragonEye); break;
                     case "RocBeak": specialEnums.Add(SpecialEnum.RocBeak); break;
                     case "GriffonClaws": specialEnums.Add(SpecialEnum.GriffonClaws); break;
                     case "Foulbane": specialEnums.Add(SpecialEnum.Foulbane); break;
                     case "MagicBox": specialEnums.Add(SpecialEnum.MagicBox); break;
                     case "HydraTeeth": specialEnums.Add(SpecialEnum.HydraTeeth); break;
                     case "StaffOfCommand": specialEnums.Add(SpecialEnum.StaffOfCommand); break;
                     default:
                        Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembersPossessions(): reached default pName=" + pName);
                        return false;
                  }
               }
            }
            if (0 < count)
               reader.Read(); // get past </SpecialXXX> tag
         }
         foreach (SpecialEnum possession in specialEnums)
         {
            if (true == isShare)
               mi.SpecialShares.Add(possession);
            else
               mi.SpecialKeeps.Add(possession);
         }
         return true;
      }
      private bool ReadXmlGamePartyMembersMounts(XmlReader reader, IMapItem mi)
      {
         reader.Read();
         if (reader.IsStartElement())
         {
            if (reader.Name != "Mounts")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembersMounts(): Mounts != (node=" + reader.Name + ")");
               return false;
            }
            string? sCount = reader.GetAttribute("count");
            if (null == sCount)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembersMounts(): Count=null");
               return false;
            }
            int count = Int32.Parse(sCount);
            for (int i = 0; i < count; ++i)
            {
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "Mount")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembersMounts(): Mount != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? name = reader.GetAttribute("Name");
                  if (null == name)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembersMounts(): GetAttribute() returned false");
                     return false;
                  }
                  IMapItem? mount = null;
                  if( true == name.Contains("Pegasus"))
                  {
                     mount = new MapItem(name, 1.0, false, false, "MPegasus", "MPegasus", mi.Territory, 0, 0, 0);
                  }
                  else if (true == name.Contains("Horse"))
                  {
                     mount = new MapItem(name, 1.0, false, false, "MHorse", "MHorse", mi.Territory, 0, 0, 0);
                  }
                  else if ( (true == name.Contains("Harpy")) || (true == name.Contains("Griffon")) )
                  {
                     continue; // griffon and harpy are matched up with riders at completion of reading all PartyMembers
                  }
                  else
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembersMounts(): reached default name=" + name);
                     return false;
                  }
                  //---------------------------
                  string? sStarveDayNum = reader.GetAttribute("StarveDayNum");
                  if (null == sStarveDayNum)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembersMounts(): GetAttribute() returned false");
                     return false;
                  }
                  mount.StarveDayNum = Int32.Parse(sStarveDayNum);
                  //---------------------------
                  string? sStarveDayNumOld = reader.GetAttribute("StarveDayNumOld");
                  if (null == sStarveDayNumOld)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembersMounts(): GetAttribute() returned false");
                     return false;
                  }
                  mount.StarveDayNumOld = Int32.Parse(sStarveDayNumOld);
                  //---------------------------
                  string? sIsMountSick = reader.GetAttribute("IsMountSick");
                  if (null == sIsMountSick)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembersMounts(): GetAttribute() returned false");
                     return false;
                  }
                  mount.IsMountSick = Boolean.Parse(sIsMountSick);
                  //---------------------------
                  string? sIsExhausted = reader.GetAttribute("IsExhausted");
                  if (null == sIsExhausted)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembersMounts(): GetAttribute() returned false");
                     return false;
                  }
                  mount.IsExhausted = Boolean.Parse(sIsExhausted);
                  //---------------------------
                  string? sIsSunStroke = reader.GetAttribute("IsSunStroke");
                  if (null == sIsSunStroke)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlGamePartyMembersMounts(): GetAttribute() returned false");
                     return false;
                  }
                  mount.IsSunStroke = Boolean.Parse(sIsSunStroke);
                  //---------------------------
                  mi.Mounts.Add(mount);
               }
            }
            if (0 < count)
               reader.Read(); // get past </Mounts> tag
         } // if (reader.IsStartElement())
         return true;
      }
      private bool ReadXmlAtRiskMounts(XmlReader reader, IGameInstance gi)
      {
         reader.Read();
         if (reader.IsStartElement())
         {
            if (reader.Name != "AtRiskMounts")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlAtRiskMounts(): AtRiskMounts != (node=" + reader.Name + ")");
               return false;
            }
            string? sCount = reader.GetAttribute("count");
            if (null == sCount)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlAtRiskMounts(): Count=null");
               return false;
            }
            int count = Int32.Parse(sCount);
            for (int i = 0; i < count; ++i)
            {
               reader.Read();
               if (reader.IsStartElement())
               {
                  if (reader.Name != "MapItem")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlAtRiskMounts(): MapItem != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? mountName = reader.GetAttribute("Name");
                  if (mountName == null)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlAtRiskMounts(): Name=null");
                     return false;
                  }
                  foreach (IMapItem partyMember in gi.PartyMembers)
                  {
                     foreach (IMapItem mount in partyMember.Mounts)
                     {
                        if (mount.Name == mountName)
                        {
                           gi.AtRiskMounts.Add(mount);
                           break;
                        }
                     }
                  }
               }
            }
            if (0 < count)
               reader.Read(); // get past </AtRiskMounts> tag
         }
         return true;
      }
      private bool ReadXmlLostTrueLoves(XmlReader reader, IGameInstance gi)
      {
         reader.Read();
         if (reader.IsStartElement())
         {
            if (reader.Name != "LostTrueLoves")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlLostTrueLoves(): LostTrueLoves != (node=" + reader.Name + ")");
               return false;
            }
            string? sCount = reader.GetAttribute("count");
            if (null == sCount)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlLostTrueLoves(): count=null");
               return false;
            }
            int count = Int32.Parse(sCount);
            for (int i = 0; i < count; ++i)
            {
               reader.Read();
               if (reader.IsStartElement())
               {
                  if (reader.Name != "MapItem")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlLostTrueLoves(): MapItem != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? loverName = reader.GetAttribute("Name");
                  if (loverName == null)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlLostTrueLoves(): loverName=null");
                     return false;
                  }
                  IMapItem lover = gi.PartyMembers.Find(loverName);
                  if( lover != null)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlLostTrueLoves(): lover=null for mi=" + loverName);
                     return false;
                  }
                  gi.LostTrueLoves.Add(lover);
               }
            }
            if( 0 < count )
               reader.Read(); // get past ending tag
         }
         return true;
      }
      private bool ReadXmlForbiddenAudiences(XmlReader reader, IGameInstance gi)
      {
         reader.Read();
         if (reader.IsStartElement())
         {
            if (reader.Name != "ForbiddenAudiences")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlForbiddenAudiences(): ForbiddenAudiences != (node=" + reader.Name + ")");
               return false;
            }
            string? sCount = reader.GetAttribute("count");
            if (null == sCount)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlForbiddenAudiences(): count=null");
               return false;
            }
            int count = Int32.Parse(sCount);
            for (int i = 0; i < count; ++i)
            {
               reader.Read();
               if (reader.IsStartElement())
               {
                  if (reader.Name != "ForbiddenAudience")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlForbiddenAudiences(): ForbiddenAudience != (node=" + reader.Name + ")");
                     return false;
                  }
                  //--------------------------------------
                  string? typeOfForbidden = null;
                  reader.Read();
                  if (reader.IsStartElement())
                  {
                     if (reader.Name != "Constraint")
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ReadXmlForbiddenAudiences(): Constraint != (node=" + reader.Name + ")");
                        return false;
                     }
                     typeOfForbidden = reader.GetAttribute("value");
                  }
                  if (typeOfForbidden == null)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlForbiddenAudiences(): typeOfForbidden=null");
                     return false;
                  }
                  //--------------------------------------
                  ITerritory? tForbidden = null;
                  reader.Read();
                  if (reader.IsStartElement())
                  {
                     if (reader.Name != "ForbiddenTerritory")
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ReadXmlForbiddenAudiences(): ForbiddenTerritory != (node=" + reader.Name + ")");
                        return false;
                     }
                     string? sForbidden = reader.GetAttribute("value");
                     if (null == sForbidden)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ReadXmlForbiddenAudiences(): sForbidden=null");
                        return false;
                     }
                     tForbidden = Territory.theTerritories.Find(sForbidden);
                  }
                  if (tForbidden == null)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlForbiddenAudiences(): tForbidden=null");
                     return false;
                  }
                  //--------------------------------------
                  ITerritory? tTarget = null;
                  string? sTarget = "unknown";
                  reader.Read();
                  if (reader.IsStartElement())
                  {
                     if (reader.Name != "TargetTerritory")
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ReadXmlForbiddenAudiences(): TargetTerritory != (node=" + reader.Name + ")");
                        return false;
                     }
                     sTarget = reader.GetAttribute("value");
                     if( null == sTarget )
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ReadXmlForbiddenAudiences(): sTarget=null");
                        return false;
                     }
                     tTarget = Territory.theTerritories.Find(sTarget);
                  }
                  if (tTarget == null)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlForbiddenAudiences(): tTarget=null for t=" + sTarget);
                     return false;
                  }
                  //--------------------------------------
                  IMapItem? assistant = null;
                  reader.Read();
                  if (reader.IsStartElement())
                  {
                     if (reader.Name != "Assistant")
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ReadXmlForbiddenAudiences(): TargetAssistantTerritory != (node=" + reader.Name + ")");
                        return false;
                     }
                     string? sAssistant = reader.GetAttribute("value");
                     if ("null" != sAssistant)
                     {
                        assistant = gi.PartyMembers.Find(sAssistant);
                        if (assistant == null)
                        {
                           Logger.Log(LogEnum.LE_ERROR, "ReadXmlForbiddenAudiences(): assistant=null");
                           return false;
                        }
                     }
                  }
                  //--------------------------------------
                  int days = -1000;
                  reader.Read();
                  if (reader.IsStartElement())
                  {
                     if (reader.Name != "Day")
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ReadXmlForbiddenAudiences(): Day != (node=" + reader.Name + ")");
                        return false;
                     }
                     string? sDays = reader.GetAttribute("value");
                     if (null == sDays)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ReadXmlForbiddenAudiences(): sDays=null");
                        return false;
                     }
                     days = Int32.Parse(sDays);
                  }
                  if( -1000 == days )
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlForbiddenAudiences(): days=-1000");
                     return false;
                  }
                  //--------------------------------------
                  switch (typeOfForbidden)
                  {
                     case "PURIFICATION":
                        gi.ForbiddenAudiences.AddPurifyConstaint(tForbidden);
                        break;
                     case "OFFERING":
                        gi.ForbiddenAudiences.AddOfferingConstaint(tForbidden, days);
                        break;
                     case "LETTER":
                        gi.ForbiddenAudiences.AddLetterConstraint(tForbidden, tTarget);
                        break;
                     case "ASSISTANT_OR_LETTER":
                        gi.ForbiddenAudiences.AddAssistantConstraint(tForbidden, assistant, tTarget);
                        break;
                     case "DAY":
                        gi.ForbiddenAudiences.AddTimeConstraint(tForbidden, days);
                        break;
                     case "CLOTHES":
                        gi.ForbiddenAudiences.AddClothesConstraint(tForbidden);
                        break;
                     case "RELIGION":
                        gi.ForbiddenAudiences.AddReligiousConstraint(tForbidden);
                        break;
                     case "MONSTER_KILL":
                        gi.ForbiddenAudiences.AddMonsterKillConstraint(tForbidden);
                        break;
                     default:
                        Logger.Log(LogEnum.LE_ERROR, "ReadXmlForbiddenAudiences(): reached default t=" + typeOfForbidden);
                        return false;
                  }
               }
               reader.Read(); // get past </ForbiddenAudience>
            }
            if( 0 < count )
               reader.Read(); // get past </ForbiddenAudiences> tag
         }
         return true;
      }
      private bool ReadXmlCaches(XmlReader reader, ICaches caches)
      {
         reader.Read();
         if (reader.IsStartElement())
         {
            if (reader.Name != "Caches")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlCaches(): Caches != (node=" + reader.Name + ")");
               return false;
            }
            string? sCount = reader.GetAttribute("count");
            if (null == sCount)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlCaches(): count=null");
               return false;
            }
            int count = Int32.Parse(sCount);
            for (int i = 0; i < count; ++i)
            {
               reader.Read();
               if (reader.IsStartElement())
               {
                  if (reader.Name != "Cache")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlCaches(): Cache != (node=" + reader.Name + ")");
                     return false;
                  }
                  //--------------------------------------
                  ITerritory? tCache = null;
                  reader.Read();
                  if (reader.IsStartElement())
                  {
                     string? sCache = reader.GetAttribute("value");
                     if (null == sCache)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ReadXmlForbiddenAudiences(): sCache=null");
                        return false;
                     }
                     tCache = Territory.theTerritories.Find(sCache);
                  }
                  if (tCache == null)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlForbiddenAudiences(): tCache=null");
                     return false;
                  }
                  //--------------------------------------
                  int coin = -1000;
                  reader.Read();
                  if (reader.IsStartElement())
                  {
                     string? sCoin = reader.GetAttribute("value");
                     if (null == sCoin)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ReadXmlForbiddenAudiences(): sCoin=null");
                        return false;
                     }
                     coin = Int32.Parse(sCoin);
                  }
                  if (-1000 == coin)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlForbiddenAudiences(): coin=-1000");
                     return false;
                  }
                  //--------------------------------------
                  caches.Add(tCache, coin);
                  reader.Read(); // get past </Cache> tag
               }
            }
            if (0 < count)
               reader.Read(); // get past </Caches> tag
         }
         return true;
      }
      private bool ReadXmlTerritories(XmlReader reader, ITerritories territories, string nodeName)
      {
         reader.Read();
         if (reader.IsStartElement())
         {
            if (reader.Name != "Territories")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): Territories != (node=" + reader.Name + ")");
               return false;
            }
            string? attribute = reader.GetAttribute("value");
            if (nodeName != attribute)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): attribute=null for nodeName=" + nodeName);
               return false;
            }
            string? sCount = reader.GetAttribute("count");
            if (null == sCount)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): Count=null");
               return false;
            }
            int count = Int32.Parse(sCount);
            for (int i = 0; i < count; ++i)
            {
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "Territory")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): Territory != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? tName = reader.GetAttribute("value");
                  if (null == tName)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): GetAttribute() returned false");
                     return false;
                  }
                  ITerritory territory = Territory.theTerritories.Find(tName);
                  if (null == territory)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlTerritories(): Find() returned null for tName=" + tName);
                     return false;
                  }
                  territories.Add(territory);
               }
            }
            if (0 < count)
               reader.Read(); // get past </Territories> tag
         }
         return true;
      }
      private bool ReadXmlEnteredHexes(XmlReader reader, List<EnteredHex> enteredHexes)
      {
         reader.Read();
         if (reader.IsStartElement())
         {
            if (reader.Name != "EnteredHexes")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): EnteredHexes != (node=" + reader.Name + ")");
               return false;
            }
            string? sTheIdentifier = reader.GetAttribute("ID");
            if (null == sTheIdentifier)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): sIdentifier=null");
               return false;
            }
            EnteredHex.theId = Int32.Parse(sTheIdentifier);
            string? sCount = reader.GetAttribute("count");
            if (null == sCount)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): Count=null");
               return false;
            }
            int countEnterHex = Int32.Parse(sCount);
            for (int i = 0; i < countEnterHex; ++i)
            {
               string? sIdentifier = null;
               int day = 0;
               string? sHexName = null;
               bool isEncounter = false;
               int position = 0;
               ColorActionEnum colorAction = ColorActionEnum.CAE_START;
               List<string> eventNames = new List<string>();
               List<string> partyNames = new List<string>();   
               //----------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "EnteredHex")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): EnteredHex != (node=" + reader.Name + ")");
                     return false;
                  }
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "Identifier")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): Identifier != (node=" + reader.Name + ")");
                     return false;
                  }
                  sIdentifier = reader.GetAttribute("value");
                  if (null == sIdentifier)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): GetAttribute(sIdentifier) returned null");
                     return false;
                  }
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "Day")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): Day != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sDay = reader.GetAttribute("value");
                  if (null == sDay)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): GetAttribute(sDay) returned null");
                     return false;
                  }
                  day = Int32.Parse(sDay);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "HexName")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): HexName != (node=" + reader.Name + ")");
                     return false;
                  }
                  sHexName = reader.GetAttribute("value");
                  if (null == sHexName)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): GetAttribute(sHexName) returned null");
                     return false;
                  }
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "IsEncounter")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): IsEncounter != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sIsEncounter = reader.GetAttribute("value");
                  if (null == sIsEncounter)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): GetAttribute(sIsEncounter) returned null");
                     return false;
                  }
                  isEncounter = Boolean.Parse(sIsEncounter);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "Position")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): Position != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sPosition = reader.GetAttribute("value");
                  if (null == sPosition)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): GetAttribute(sPosition) returned null");
                     return false;
                  }
                  position = Int32.Parse(sPosition);
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "ColorAction")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): ColorAction != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sColorAction = reader.GetAttribute("value");
                  if (null == sColorAction)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): GetAttribute(sColorAction) returned null");
                     return false;
                  }
                  switch (sColorAction)
                  {
                     case "CAE_START": colorAction = ColorActionEnum.CAE_START; break;
                     case "CAE_LOST": colorAction = ColorActionEnum.CAE_LOST; break;
                     case "CAE_REST": colorAction = ColorActionEnum.CAE_REST; break;
                     case "CAE_JAIL": colorAction = ColorActionEnum.CAE_JAIL; break;
                     case "CAE_TRAVEL": colorAction = ColorActionEnum.CAE_TRAVEL; break;
                     case "CAE_TRAVEL_AIR": colorAction = ColorActionEnum.CAE_TRAVEL_AIR; break;
                     case "CAE_TRAVEL_RAFT": colorAction = ColorActionEnum.CAE_TRAVEL_RAFT; break;
                     case "CAE_TRAVEL_DOWNRIVER": colorAction = ColorActionEnum.CAE_TRAVEL_DOWNRIVER; break;
                     case "CAE_ESCAPE": colorAction = ColorActionEnum.CAE_ESCAPE; break;
                     case "CAE_FOLLOW": colorAction = ColorActionEnum.CAE_FOLLOW; break;
                     case "CAE_SEARCH": colorAction = ColorActionEnum.CAE_SEARCH; break;
                     case "CAE_SEARCH_RUINS": colorAction = ColorActionEnum.CAE_SEARCH_RUINS; break;
                     case "CAE_SEEK_NEWS": colorAction = ColorActionEnum.CAE_SEEK_NEWS; break;
                     case "CAE_HIRE": colorAction = ColorActionEnum.CAE_HIRE; break;
                     case "CAE_AUDIENCE": colorAction = ColorActionEnum.CAE_AUDIENCE; break;
                     case "CAE_OFFERING": colorAction = ColorActionEnum.CAE_OFFERING; break;
                     default:
                        Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): reached default with sColorAction=" + sColorAction);
                        return false;
                  }
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "EventNames")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): EventNames != (node=" + reader.Name + ")");
                     return false;
                  }
                  sCount = reader.GetAttribute("count");
                  if (null == sCount)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): GetAttribute(sCount) returned null");
                     return false;
                  }
                  int countEventName = Int32.Parse(sCount);
                  for(int k=0; k< countEventName; ++k )
                  {
                     reader.Read();
                     if (reader.Name != "EventName")
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): EventName != (node=" + reader.Name + ")");
                        return false;
                     }
                     string? sEventName= reader.GetAttribute("value");
                     if (null == sEventName)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): GetAttribute(sEventName) returned null");
                        return false;
                     }
                     eventNames.Add(sEventName);
                  }
                  if (0 < countEventName)
                     reader.Read(); // get past </EventNames> tag
               }
               //--------------------------------------
               reader.Read();
               if (true == reader.IsStartElement())
               {
                  if (reader.Name != "PartyNames")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): PartyNames != (node=" + reader.Name + ")");
                     return false;
                  }
                  sCount = reader.GetAttribute("count");
                  if (null == sCount)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): GetAttribute(sCount) returned null");
                     return false;
                  }
                  int countPartyName = Int32.Parse(sCount);
                  for (int k = 0; k < countPartyName; ++k)
                  {
                     reader.Read();
                     if (reader.Name != "PartyName")
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): PartyName != (node=" + reader.Name + ")");
                        return false;
                     }
                     string? sPartyName = reader.GetAttribute("value");
                     if (null == sPartyName)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): GetAttribute(sPartyName) returned null");
                        return false;
                     }
                     partyNames.Add(sPartyName);
                  }
                  if( 0 < countPartyName)
                     reader.Read(); // get past </PartyNames> tag
               }
               //--------------------------------------
               EnteredHex enteredHex = new EnteredHex(sIdentifier, day, sHexName, isEncounter, position, colorAction, eventNames, partyNames);
               enteredHexes.Add(enteredHex);
               reader.Read(); // get past </EnteredHex> tag
            }
            if (0 < countEnterHex)
               reader.Read(); // get past </EnteredHexes> tag
         }
         return true;
      }
      //--------------------------------------------------
      private XmlDocument? CreateXml(IGameInstance gi)
      {
         XmlDocument aXmlDocument = new XmlDocument();
         aXmlDocument.LoadXml("<GameInstance></GameInstance>");
         if (null == aXmlDocument.DocumentElement)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): aXmlDocument.DocumentElement=null");
            return null;
         }
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): root is null");
            return null;
         }
         //------------------------------------------
         XmlElement? versionElem = aXmlDocument.CreateElement("Version"); 
         if( null == versionElem )
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): aXmlDocument.DocumentElement.LastChild=null");
            return null;
         }
         int majorVersion = GetMajorVersion();
         if (majorVersion < 0 )
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml():  0 > majorVersion=" + majorVersion.ToString());
            return null;
         }
         versionElem.SetAttribute("value", majorVersion.ToString());
         XmlNode? versionNode = root.AppendChild(versionElem);
         if (null == versionNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(versionNode) returned null");
            return null;
         }
         //------------------------------------------
         if ( false == CreateXmlGameOptions(aXmlDocument, gi.Options))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlOptions() returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlGameStat(aXmlDocument, gi.Statistic))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlGameStat() returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlPartyMembers(aXmlDocument, gi.PartyMembers))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlGameStat() returned false");
            return null;
         }
         //------------------------------------------
         XmlElement? elem = aXmlDocument.CreateElement("WitAndWile");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(WitAndWile) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.WitAndWile.ToString());
         XmlNode? node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(WitAndWile) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("WitAndWileInitial");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(WitAndWileInitial) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.WitAndWileInitial.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(WitAndWile) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("Days");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Days) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.Days.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(Days) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("EventActive");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(EventActive) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.EventActive);
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(EventActive) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("EventDisplayed");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(EventDisplayed) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.EventDisplayed);
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(EventDisplayed) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("EventStart");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(EventStart) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.EventStart);
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(EventStart) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("GameTurn");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(GameTurn) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.GameTurn.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(GameTurn) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsMarkOfCain");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsMarkOfCain) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsMarkOfCain.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsMarkOfCain) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsEnslaved");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsEnslaved) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsEnslaved.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsEnslaved) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsSpellBound");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsSpellBound) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsSpellBound.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsSpellBound) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("WanderingDayCount");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(WanderingDayCount) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.WanderingDayCount.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(WanderingDayCount) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsBlessed");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsBlessed) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsBlessed.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsBlessed) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsArchTravelKnown");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsArchTravelKnown) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsArchTravelKnown.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsArchTravelKnown) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsMerchantWithParty");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsMerchantWithParty) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsMerchantWithParty.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsMerchantWithParty) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsJailed");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsJailed) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsJailed.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsJailed) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsDungeon");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsDungeon) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsDungeon.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsDungeon) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("NightsInDungeon");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(NightsInDungeon) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.NightsInDungeon.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(NightsInDungeon) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsWoundedWarriorRest");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsWoundedWarriorRest) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsWoundedWarriorRest.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsWoundedWarriorRest) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("NumMembersBeingFollowed");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(NumMembersBeingFollowed) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.NumMembersBeingFollowed.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(NumMembersBeingFollowed) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsHighPass");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsHighPass) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsHighPass.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsHighPass) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("EventAfterRedistribute");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(EventAfterRedistribute) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.EventAfterRedistribute);
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(EventAfterRedistribute) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsImpassable");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsImpassable) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsImpassable.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsImpassable) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsFlood");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsFlood) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsFlood.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsFlood) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsFloodContinue");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsFloodContinue) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsFloodContinue.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsFloodContinue) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsMountsSick");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsMountsSick) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsMountsSick.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsMountsSick) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsFalconFed");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsFalconFed) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsFalconFed.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsFalconFed) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsEagleHunt");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsEagleHunt) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsEagleHunt.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsEagleHunt) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsExhausted");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsExhausted) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsExhausted.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsExhausted) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("RaftState");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(RaftState) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.RaftState.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(RaftState) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsWoundedBlackKnightRest");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsWoundedBlackKnightRest) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsWoundedBlackKnightRest.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsWoundedBlackKnightRest) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsTrainHorse");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsTrainHorse) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsTrainHorse.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsTrainHorse) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsBadGoing");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsBadGoing) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsBadGoing.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsBadGoing) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsHeavyRain");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsHeavyRain) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsHeavyRain.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsHeavyRain) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsHeavyRainNextDay");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsHeavyRainNextDay) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsHeavyRainNextDay.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsHeavyRainNextDay) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsHeavyRainContinue");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsHeavyRainContinue) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsHeavyRainContinue.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsHeavyRainContinue) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsHeavyRainDismount");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsHeavyRainDismount) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsHeavyRainDismount.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsHeavyRainDismount) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("HydraTeethCount");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(HydraTeethCount) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.HydraTeethCount.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(HydraTeethCount) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsHuldraHeirKilled");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsHuldraHeirKilled) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsHuldraHeirKilled.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsHuldraHeirKilled) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("DayOfLastOffering");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(DayOfLastOffering) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.DayOfLastOffering.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(DayOfLastOffering) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsPartyContinuouslyLodged");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsPartyContinuouslyLodged) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsPartyContinuouslyLodged.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsPartyContinuouslyLodged) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsTrueLoveHeartBroken");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsTrueLoveHeartBroken) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsTrueLoveHeartBroken.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsTrueLoveHeartBroken) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsMustLeaveHex");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsMustLeaveHex) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsMustLeaveHex.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsMustLeaveHex) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("NumMonsterKill");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(NumMonsterKill) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.NumMonsterKill.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(NumMonsterKill) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsOmenModifier");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsOmenModifier) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsOmenModifier.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsOmenModifier) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsInfluenceModifier");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsInfluenceModifier) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsInfluenceModifier.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsInfluenceModifier) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsSecretTempleKnown");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsSecretTempleKnown) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsSecretTempleKnown.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsSecretTempleKnown) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("ChagaDrugCount");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(ChagaDrugCount) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.ChagaDrugCount.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(ChagaDrugCount) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsChagaDrugProvided");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsChagaDrugProvided) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsChagaDrugProvided.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsChagaDrugProvided) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsSecretBaronHuldra");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsSecretBaronHuldra) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsSecretBaronHuldra.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsSecretBaronHuldra) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsSecretLadyAeravir");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsSecretLadyAeravir) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsSecretLadyAeravir.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsSecretLadyAeravir) returned null");
            return null;
         }
         //------------------------------------------
         elem = aXmlDocument.CreateElement("IsSecretCountDrogat");
         if (null == elem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsSecretCountDrogat) returned null");
            return null;
         }
         elem.SetAttribute("value", gi.IsSecretCountDrogat.ToString());
         node = root.AppendChild(elem);
         if (null == node)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(IsSecretCountDrogat) returned null");
            return null;
         }
         //------------------------------------------
         XmlElement? atRiskMountsElem = aXmlDocument.CreateElement("AtRiskMounts");
         if (null == atRiskMountsElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(AtRiskMounts) returned null");
            return null;
         }
         atRiskMountsElem.SetAttribute("count", gi.AtRiskMounts.Count.ToString());
         XmlNode? atRiskMountsNode = root.AppendChild(atRiskMountsElem);
         if (null == atRiskMountsNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(atRiskMountsNode) returned null");
            return null;
         }
         foreach (IMapItem mi in gi.AtRiskMounts)
         {
            XmlElement? atRiskMountElem = aXmlDocument.CreateElement("MapItem");
            if (null == atRiskMountElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(atRiskMountElem) returned null");
               return null;
            }
            versionElem.SetAttribute("value", mi.Name.ToString());
            XmlNode? atRiskMountNode = atRiskMountsNode.AppendChild(atRiskMountElem);
            if (null == atRiskMountNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(atRiskMountNode) returned null");
               return null;
            }
         }
         //------------------------------------------
         XmlElement? lostTrueLovesElem = aXmlDocument.CreateElement("LostTrueLoves");
         if (null == lostTrueLovesElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(LostTrueLoves) returned null");
            return null;
         }
         lostTrueLovesElem.SetAttribute("count", gi.LostTrueLoves.Count.ToString());
         XmlNode? lostTrueLovesNode = root.AppendChild(lostTrueLovesElem);
         if (null == lostTrueLovesNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(lostTrueLovesNode) returned null");
            return null;
         }
         foreach (IMapItem mi in gi.LostTrueLoves)
         {
            XmlElement? lostTrueLoveElem = aXmlDocument.CreateElement("MapItem");
            if (null == lostTrueLoveElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(lostTrueLoveElem) returned null");
               return null;
            }
            versionElem.SetAttribute("value", mi.Name.ToString());
            XmlNode? lostTrueLoveNode = lostTrueLovesNode.AppendChild(lostTrueLoveElem);
            if (null == lostTrueLoveNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(lostTrueLoveNode) returned null");
               return null;
            }
         }
         //------------------------------------------
         if (false == CreateXmlForbiddenAudiences(aXmlDocument, gi.ForbiddenAudiences))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlForbiddenAudiences() returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlCaches(aXmlDocument, gi.Caches))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlCaches() returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTerritories(aXmlDocument, "DwarfAdviceLocations", gi.DwarfAdviceLocations))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlTerritories(DwarfAdviceLocations) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTerritories(aXmlDocument, "WizardAdviceLocations", gi.WizardAdviceLocations))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlTerritories(WizardAdviceLocations) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTerritories(aXmlDocument, "Arches", gi.Arches))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlTerritories(Arches) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTerritories(aXmlDocument, "VisitedLocations", gi.VisitedLocations))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlTerritories(VisitedLocations) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTerritories(aXmlDocument, "EscapedLocations", gi.EscapedLocations))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlTerritories(EscapedLocations) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTerritories(aXmlDocument, "GoblinKeeps", gi.GoblinKeeps))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlTerritories(GoblinKeeps) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTerritories(aXmlDocument, "DwarvenMines", gi.DwarvenMines))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlTerritories(DwarvenMines) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTerritories(aXmlDocument, "OrcTowers", gi.OrcTowers))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlTerritories(OrcTowers) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTerritories(aXmlDocument, "WizardTowers", gi.WizardTowers))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlTerritories(WizardTowers) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTerritories(aXmlDocument, "PixieAdviceLocations", gi.PixieAdviceLocations))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlTerritories(PixieAdviceLocations) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTerritories(aXmlDocument, "HalflingTowns", gi.HalflingTowns))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlTerritories(HalflingTowns) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTerritories(aXmlDocument, "RuinsUnstable", gi.RuinsUnstable))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlTerritories(RuinsUnstable) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTerritories(aXmlDocument, "HiddenRuins", gi.HiddenRuins))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlTerritories(HiddenRuins) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTerritories(aXmlDocument, "HiddenTowns", gi.HiddenTowns))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlTerritories(HiddenTowns) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTerritories(aXmlDocument, "HiddenTemples", gi.HiddenTemples))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlTerritories(HiddenTemples) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTerritories(aXmlDocument, "KilledLocations", gi.KilledLocations))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlTerritories(KilledLocations) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTerritories(aXmlDocument, "EagleLairs", gi.EagleLairs))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlTerritories(EagleLairs) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTerritories(aXmlDocument, "SecretClues", gi.SecretClues))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlTerritories(SecretClues) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTerritories(aXmlDocument, "LetterOfRecommendations", gi.LetterOfRecommendations))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlTerritories(LetterOfRecommendations) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTerritories(aXmlDocument, "Purifications", gi.Purifications))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlTerritories(Purifications) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTerritories(aXmlDocument, "ElfTowns", gi.ElfTowns))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlTerritories(ElfTowns) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTerritories(aXmlDocument, "ElfCastles", gi.ElfCastles))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlTerritories(ElfCastles) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTerritories(aXmlDocument, "FeelAtHomes", gi.FeelAtHomes))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlTerritories(FeelAtHomes) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTerritories(aXmlDocument, "SecretRites", gi.SecretRites))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlTerritories(SecretRites) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTerritories(aXmlDocument, "CheapLodgings", gi.CheapLodgings))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlTerritories(CheapLodgings) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTerritories(aXmlDocument, "ForbiddenHexes", gi.ForbiddenHexes))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlTerritories(ForbiddenHexes) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTerritories(aXmlDocument, "AbandonedTemples", gi.AbandonedTemples))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlTerritories(AbandonedTemples) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlTerritories(aXmlDocument, "ForbiddenHires", gi.ForbiddenHires))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlTerritories(ForbiddenHires) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlEnteredHexes(aXmlDocument, gi.EnteredHexes))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateXmlEnteredHexes() returned false");
            return null;
         }
         return aXmlDocument;
      }
      private bool CreateXmlGameOptions(XmlDocument aXmlDocument, Options options)
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): root is null");
            return false;
         }
         XmlElement? optionsElem = aXmlDocument.CreateElement("Options");
         if (null == optionsElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Options) returned null");
            return false;
         }
         optionsElem.SetAttribute("count", options.Count.ToString());
         XmlNode? optionsNode = root.AppendChild(optionsElem);
         if (null == optionsNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(optionsNode) returned null");
            return false;
         }
         //--------------------------------
         foreach (Option option in options)
         {
            XmlElement? optionElem = aXmlDocument.CreateElement("Option");  
            if( null == optionElem )
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Option) returned null");
               return false;
            }
            optionElem.SetAttribute("Name", option.Name);
            optionElem.SetAttribute("IsEnabled", option.IsEnabled.ToString());
            XmlNode? optionNode = optionsNode.AppendChild(optionElem);
            if (null == optionNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(optionNode) returned null");
               return false;
            }
         }
         return true;
      }
      private bool CreateXmlGameStat(XmlDocument aXmlDocument, GameStat stat)
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): root is null");
            return false;
         }
         XmlElement? gameStatElem = aXmlDocument.CreateElement("GameStat");
         if (null == gameStatElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(gameStatElem) returned null");
            return false;
         }
         XmlNode? gameStatNode = root.AppendChild(gameStatElem);
         if (null == gameStatNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(gameStatNode) returned null");
            return false;
         }
         //--------------------------------
         XmlElement? statElem = aXmlDocument.CreateElement("myDaysLost");
         if (null == statElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(myDaysLost) returned null");
            return false;
         }
         statElem.SetAttribute("value", stat.myDaysLost.ToString());
         XmlNode? statNode = gameStatNode.AppendChild(statElem);
         if (null == statNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(statNode) returned null");
            return false;
         }
         //--------------------------------
         statElem = aXmlDocument.CreateElement("myNumEncounters");
         if (null == statElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(myNumEncounters) returned null");
            return false;
         }
         statElem.SetAttribute("value", stat.myNumEncounters.ToString());
         statNode = gameStatNode.AppendChild(statElem);
         if (null == statNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(statNode) returned null");
            return false;
         }
         //--------------------------------
         statElem = aXmlDocument.CreateElement("myNumOfRestDays");
         if (null == statElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(myNumOfRestDays) returned null");
            return false;
         }
         statElem.SetAttribute("value", stat.myNumOfRestDays.ToString());
         statNode = gameStatNode.AppendChild(statElem);
         if (null == statNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(statNode) returned null");
            return false;
         }
         //--------------------------------
         statElem = aXmlDocument.CreateElement("myNumOfAudienceAttempt");
         if (null == statElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(myNumOfAudienceAttempt) returned null");
            return false;
         }
         statElem.SetAttribute("value", stat.myNumOfAudienceAttempt.ToString());
         statNode = gameStatNode.AppendChild(statElem);
         if (null == statNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(statNode) returned null");
            return false;
         }
         //--------------------------------
         statElem = aXmlDocument.CreateElement("myNumOfAudience");
         if (null == statElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(myNumOfAudience) returned null");
            return false;
         }
         statElem.SetAttribute("value", stat.myNumOfAudience.ToString());
         statNode = gameStatNode.AppendChild(statElem);
         if (null == statNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(statNode) returned null");
            return false;
         }
         //--------------------------------
         statElem = aXmlDocument.CreateElement("myNumOfOffering");
         if (null == statElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(myNumOfOffering) returned null");
            return false;
         }
         statElem.SetAttribute("value", stat.myNumOfOffering.ToString());
         statNode = gameStatNode.AppendChild(statElem);
         if (null == statNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(statNode) returned null");
            return false;
         }
         //--------------------------------
         statElem = aXmlDocument.CreateElement("myDaysInJailorDungeon");
         if (null == statElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(myDaysInJailorDungeon) returned null");
            return false;
         }
         statElem.SetAttribute("value", stat.myDaysInJailorDungeon.ToString());
         statNode = gameStatNode.AppendChild(statElem);
         if (null == statNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(statNode) returned null");
            return false;
         }
         //--------------------------------
         statElem = aXmlDocument.CreateElement("myNumRiverCrossingSuccess");
         if (null == statElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(myNumRiverCrossingSuccess) returned null");
            return false;
         }
         statElem.SetAttribute("value", stat.myNumRiverCrossingSuccess.ToString());
         statNode = gameStatNode.AppendChild(statElem);
         if (null == statNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(statNode) returned null");
            return false;
         }
         //--------------------------------
         statElem = aXmlDocument.CreateElement("myNumRiverCrossingFailure");
         if (null == statElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(myNumRiverCrossingFailure) returned null");
            return false;
         }
         statElem.SetAttribute("value", stat.myNumRiverCrossingFailure.ToString());
         statNode = gameStatNode.AppendChild(statElem);
         if (null == statNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(statNode) returned null");
            return false;
         }
         //--------------------------------
         statElem = aXmlDocument.CreateElement("myNumDaysOnRaft");
         if (null == statElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(myNumDaysOnRaft) returned null");
            return false;
         }
         statElem.SetAttribute("value", stat.myNumDaysOnRaft.ToString());
         statNode = gameStatNode.AppendChild(statElem);
         if (null == statNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(statNode) returned null");
            return false;
         }
         //--------------------------------
         statElem = aXmlDocument.CreateElement("myNumDaysAirborne");
         if (null == statElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(myNumDaysAirborne) returned null");
            return false;
         }
         statElem.SetAttribute("value", stat.myNumDaysAirborne.ToString());
         statNode = gameStatNode.AppendChild(statElem);
         if (null == statNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(statNode) returned null");
            return false;
         }
         //--------------------------------
         statElem = aXmlDocument.CreateElement("myNumDaysArchTravel");
         if (null == statElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(myNumDaysArchTravel) returned null");
            return false;
         }
         statElem.SetAttribute("value", stat.myNumDaysArchTravel.ToString());
         statNode = gameStatNode.AppendChild(statElem);
         if (null == statNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(statNode) returned null");
            return false;
         }
         //--------------------------------
         statElem = aXmlDocument.CreateElement("myMaxPartySize");
         if (null == statElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(myMaxPartySize) returned null");
            return false;
         }
         statElem.SetAttribute("value", stat.myMaxPartySize.ToString());
         statNode = gameStatNode.AppendChild(statElem);
         if (null == statNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(statNode) returned null");
            return false;
         }
         //--------------------------------
         statElem = aXmlDocument.CreateElement("myMaxPartyEndurance");
         if (null == statElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(myMaxPartyEndurance) returned null");
            return false;
         }
         statElem.SetAttribute("value", stat.myMaxPartyEndurance.ToString());
         statNode = gameStatNode.AppendChild(statElem);
         if (null == statNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(statNode) returned null");
            return false;
         }
         //--------------------------------
         statElem = aXmlDocument.CreateElement("myMaxPartyCombat");
         if (null == statElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(myMaxPartyCombat) returned null");
            return false;
         }
         statElem.SetAttribute("value", stat.myMaxPartyCombat.ToString());
         statNode = gameStatNode.AppendChild(statElem);
         if (null == statNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(statNode) returned null");
            return false;
         }
         //--------------------------------
         statElem = aXmlDocument.CreateElement("myNumOfPartyKilled");
         if (null == statElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(myNumOfPartyKilled) returned null");
            return false;
         }
         statElem.SetAttribute("value", stat.myNumOfPartyKilled.ToString());
         statNode = gameStatNode.AppendChild(statElem);
         if (null == statNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(statNode) returned null");
            return false;
         }
         //--------------------------------
         statElem = aXmlDocument.CreateElement("myNumOfPartyHeal");
         if (null == statElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(myNumOfPartyHeal) returned null");
            return false;
         }
         statElem.SetAttribute("value", stat.myNumOfPartyHeal.ToString());
         statNode = gameStatNode.AppendChild(statElem);
         if (null == statNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(statNode) returned null");
            return false;
         }
         //--------------------------------
         statElem = aXmlDocument.CreateElement("myNumOfPartyKill");
         if (null == statElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(myNumOfPartyKill) returned null");
            return false;
         }
         statElem.SetAttribute("value", stat.myNumOfPartyKill.ToString());
         statNode = gameStatNode.AppendChild(statElem);
         if (null == statNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(statNode) returned null");
            return false;
         }
         //--------------------------------
         statElem = aXmlDocument.CreateElement("myNumOfPartyKillEndurance");
         if (null == statElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(myNumOfPartyKillEndurance) returned null");
            return false;
         }
         statElem.SetAttribute("value", stat.myNumOfPartyKillEndurance.ToString());
         statNode = gameStatNode.AppendChild(statElem);
         if (null == statNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(statNode) returned null");
            return false;
         }
         //--------------------------------
         statElem = aXmlDocument.CreateElement("myNumOfPartyKillCombat");
         if (null == statElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(myNumOfPartyKillCombat) returned null");
            return false;
         }
         statElem.SetAttribute("value", stat.myNumOfPartyKillCombat.ToString());
         statNode = gameStatNode.AppendChild(statElem);
         if (null == statNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(statNode) returned null");
            return false;
         }
         //--------------------------------
         statElem = aXmlDocument.CreateElement("myNumOfPrinceKill");
         if (null == statElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(myNumOfPrinceKill) returned null");
            return false;
         }
         statElem.SetAttribute("value", stat.myNumOfPrinceKill.ToString());
         statNode = gameStatNode.AppendChild(statElem);
         if (null == statNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(statNode) returned null");
            return false;
         }
         //--------------------------------
         statElem = aXmlDocument.CreateElement("myNumOfPrinceHeal");
         if (null == statElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(myNumOfPrinceHeal) returned null");
            return false;
         }
         statElem.SetAttribute("value", stat.myNumOfPrinceHeal.ToString());
         statNode = gameStatNode.AppendChild(statElem);
         if (null == statNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(statNode) returned null");
            return false;
         }
         //--------------------------------
         statElem = aXmlDocument.CreateElement("myNumOfPrinceStarveDays");
         if (null == statElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(myNumOfPrinceStarveDays) returned null");
            return false;
         }
         statElem.SetAttribute("value", stat.myNumOfPrinceStarveDays.ToString());
         statNode = gameStatNode.AppendChild(statElem);
         if (null == statNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(statNode) returned null");
            return false;
         }
         //--------------------------------
         statElem = aXmlDocument.CreateElement("myNumOfPrinceUncounscious");
         if (null == statElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(myNumOfPrinceUncounscious) returned null");
            return false;
         }
         statElem.SetAttribute("value", stat.myNumOfPrinceUncounscious.ToString());
         statNode = gameStatNode.AppendChild(statElem);
         if (null == statNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(statNode) returned null");
            return false;
         }
         //--------------------------------
         statElem = aXmlDocument.CreateElement("myNumOfPrinceResurrection");
         if (null == statElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(myNumOfPrinceResurrection) returned null");
            return false;
         }
         statElem.SetAttribute("value", stat.myNumOfPrinceResurrection.ToString());
         statNode = gameStatNode.AppendChild(statElem);
         if (null == statNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(statNode) returned null");
            return false;
         }
         //--------------------------------
         statElem = aXmlDocument.CreateElement("myNumOfPrinceAxeDeath");
         if (null == statElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(myNumOfPrinceAxeDeath) returned null");
            return false;
         }
         statElem.SetAttribute("value", stat.myNumOfPrinceAxeDeath.ToString());
         statNode = gameStatNode.AppendChild(statElem);
         if (null == statNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(statNode) returned null");
            return false;
         }
         return true;
      }
      private bool CreateXmlPartyMembers(XmlDocument aXmlDocument, IMapItems partyMembers)
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): root is null");
            return false;
         }
         XmlElement? partyMembersElem = aXmlDocument.CreateElement("PartyMembers");
         if (null == partyMembersElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(partyMembersElem) returned null");
            return false;
         }
         partyMembersElem.SetAttribute("count", partyMembers.Count.ToString());
         XmlNode? partyMembersNode = root.AppendChild(partyMembersElem);
         if (null == partyMembersNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(partyMembersNode) returned null");
            return false;
         }
         //--------------------------------
         foreach (IMapItem mi in partyMembers)
         {
            XmlElement? miElem = aXmlDocument.CreateElement("MapItem");
            if (null == miElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(miElem) returned null");
               return false;
            }
            XmlNode? miNode = partyMembersNode.AppendChild(miElem);
            if (null == miNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(miNode) returned null");
               return false;
            }
            //--------------------------------
            XmlElement? elem = aXmlDocument.CreateElement("Name");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Name) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.Name);
            XmlNode? node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("TopImageName");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(TopImageName) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.TopImageName);
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("Endurance");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Endurance) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.Endurance.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("Combat");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Combat) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.Combat.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("WealthCode");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(WealthCode) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.WealthCode.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            XmlElement? terrElem = aXmlDocument.CreateElement("Territory");  // name of territory
            if (null == terrElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(terrElem) returned null");
               return false;
            }
            terrElem.SetAttribute("value", mi.Territory.Name);
            XmlNode? territoryNode = miNode.AppendChild(terrElem);
            if (null == territoryNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(territoryNode) returned null");
               return false;
            }
            //=========================================================================
            elem = aXmlDocument.CreateElement("OverlayImageName");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(OverlayImageName) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.OverlayImageName);
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("Movement");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Movement) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.Movement.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("Wound");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Wound) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.Wound.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("Poison");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Poison) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.Poison.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("Coin");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Coin) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.Coin.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("Food");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Food) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.Food.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("StarveDayNum");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(StarveDayNum) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.StarveDayNum.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("StarveDayNumOld");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(StarveDayNumOld) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.StarveDayNumOld.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("MovementUsed");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(MovementUsed) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.MovementUsed.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsGuide");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsGuide) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsGuide.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsKilled");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsKilled) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsKilled.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsUnconscious");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsUnconscious) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsUnconscious.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsExhausted");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsExhausted) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsExhausted.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsSunStroke");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsSunStroke) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsSunStroke.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsPlagued");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsPlagued) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsPlagued.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("PlagueDustWound");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(PlagueDustWound) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.PlagueDustWound.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsPlayedMusic");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsPlayedMusic) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsPlayedMusic.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsCatchCold");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsCatchCold) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsCatchCold.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsRiding");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsRiding) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsRiding.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsFlying");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsFlying) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsFlying.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsSecretGatewayToDarknessKnown");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsSecretGatewayToDarknessKnown) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsSecretGatewayToDarknessKnown.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsFugitive");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsFugitive) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsFugitive.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsResurrected");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsResurrected) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsResurrected.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsTrueLove");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsTrueLove) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsTrueLove.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsFickle");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsFickle) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsFickle.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("GroupNum");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(GroupNum) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.GroupNum.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("PayDay");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(PayDay) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.PayDay.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("Wages");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Wages) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.Wages.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsAlly");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsAlly) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsAlly.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsLooter");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsLooter) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsLooter.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsTownCastleTempleLeave");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsTownCastleTempleLeave) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsTownCastleTempleLeave.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            XmlElement? guideTerrElem = aXmlDocument.CreateElement("GuideTerritories");
            if (null == guideTerrElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(guideTerrElem) returned null");
               return false;
            }
            guideTerrElem.SetAttribute("count", mi.GuideTerritories.Count.ToString());
            XmlNode? guideTerrNode = miNode.AppendChild(guideTerrElem);
            if (null == guideTerrNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(guideTerrNode) returned null");
               return false;
            }
            foreach (ITerritory t in mi.GuideTerritories)
            {
               terrElem = aXmlDocument.CreateElement("Territory");  // name of territory
               if (null == terrElem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(terrElem) returned null");
                  return false;
               }
               terrElem.SetAttribute("value", t.Name);
               territoryNode = guideTerrNode.AppendChild(terrElem);
               if (null == territoryNode)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(territoryNode) returned null");
                  return false;
               }
            }
            //--------------------------------
            XmlElement? specialKeepsElem = aXmlDocument.CreateElement("SpecialKeeps");
            if (null == specialKeepsElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(SpecialKeeps) returned null");
               return false;
            }
            specialKeepsElem.SetAttribute("count", mi.SpecialKeeps.Count.ToString());
            XmlNode? specialKeepsNode = miNode.AppendChild(specialKeepsElem);
            if (null == specialKeepsNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(specialKeepsNode) returned null");
               return false;
            }
            foreach (SpecialEnum keep in mi.SpecialKeeps)
            {
               XmlElement? keepsElem = aXmlDocument.CreateElement("Possession");
               if (null == keepsElem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(keepsElem) returned null");
                  return false;
               }
               keepsElem.SetAttribute("value", keep.ToString());
               XmlNode? keepsNode = specialKeepsNode.AppendChild(keepsElem);
               if (null == keepsNode)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(keepsNode) returned null");
                  return false;
               }
            }
            //--------------------------------
            XmlElement? specialShareElem = aXmlDocument.CreateElement("SpecialShares");
            if (null == specialShareElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(SpecialKeeps) returned null");
               return false;
            }
            specialShareElem.SetAttribute("count", mi.SpecialShares.Count.ToString());
            XmlNode? specialShareNode = miNode.AppendChild(specialShareElem);
            if (null == specialShareNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(specialShareNode) returned null");
               return false;
            }
            foreach (SpecialEnum share in mi.SpecialShares)
            {
               XmlElement? sharesElem = aXmlDocument.CreateElement("Possession");
               if (null == sharesElem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(sharesElem) returned null");
                  return false;
               }
               sharesElem.SetAttribute("value", share.ToString());
               XmlNode? sharesNode = specialShareNode.AppendChild(sharesElem);
               if (null == sharesNode)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(sharesNode) returned null");
                  return false;
               }
            }
            //--------------------------------+++++++++++++++++++++++++++++++++++++++++++++++
            elem = aXmlDocument.CreateElement("Rider");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Rider) returned null");
               return false;
            }
            if( null == mi.Rider )
               elem.SetAttribute("value", "None");
            else
               elem.SetAttribute("value", mi.Rider.Name);
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            XmlElement? mountsElem = aXmlDocument.CreateElement("Mounts");
            if (null == mountsElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Mounts) returned null");
               return false;
            }
            mountsElem.SetAttribute("count", mi.Mounts.Count.ToString());
            XmlNode? mountsNode = miNode.AppendChild(mountsElem);
            if (null == mountsNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(mountsNode) returned null");
               return false;
            }
            foreach (IMapItem mount in mi.Mounts) // only save off name of mounts
            {
               XmlElement? mountElem = aXmlDocument.CreateElement("Mount");
               if (null == mountElem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Mount) returned null");
                  return false;
               }
               mountElem.SetAttribute("Name", mount.Name);
               mountElem.SetAttribute("StarveDayNum", mi.StarveDayNum.ToString());
               mountElem.SetAttribute("StarveDayNumOld", mi.StarveDayNumOld.ToString());
               mountElem.SetAttribute("IsMountSick", mi.IsMountSick.ToString());
               mountElem.SetAttribute("IsExhausted", mi.IsExhausted.ToString());
               mountElem.SetAttribute("IsSunStroke", mi.IsSunStroke.ToString());
               XmlNode? mountNode = mountsNode.AppendChild(mountElem);
               if (null == mountNode)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(mountNode) returned null");
                  return false;
               }
            }
         }
         return true;
      }
      private bool CreateXmlTerritories(XmlDocument aXmlDocument, string attribute, ITerritories territories)
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): root is null");
            return false;
         }
         XmlElement? territoriesElem = aXmlDocument.CreateElement("Territories");
         if (null == territoriesElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Territories) returned null");
            return false;
         }
         territoriesElem.SetAttribute("value", attribute);
         territoriesElem.SetAttribute("count", territories.Count.ToString());
         XmlNode? territoriesNode = root.AppendChild(territoriesElem);
         if (null == territoriesNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(territoriesNode) returned null");
            return false;
         }
         //--------------------------------
         foreach (Territory t in territories)
         {
            XmlElement? terrElem = aXmlDocument.CreateElement("Territory");  // name of territory
            if (null == terrElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(terrElem) returned null");
               return false;
            }
            terrElem.SetAttribute("value", t.Name);
            XmlNode? territoryNode = territoriesNode.AppendChild(terrElem);
            if (null == territoryNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(territoryNode) returned null");
               return false;
            }
         }
         return true;
      }
      private bool CreateXmlEnteredHexes(XmlDocument aXmlDocument, List<EnteredHex> hexes)
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): root is null");
            return false;
         }
         //--------------------------------------------------------------------
         XmlElement? enteredHexesElem = aXmlDocument.CreateElement("EnteredHexes");
         if (null == enteredHexesElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Territories) returned null");
            return false;
         }
         enteredHexesElem.SetAttribute("ID", EnteredHex.theId.ToString());
         enteredHexesElem.SetAttribute("count", hexes.Count.ToString());
         XmlNode? enteredHexesNode = root.AppendChild(enteredHexesElem);
         if (null == enteredHexesNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(enteredHexesNode) returned null");
            return false;
         }
         //--------------------------------------------------------------------
         foreach (EnteredHex hex in hexes)
         {
            XmlElement? hexElem = aXmlDocument.CreateElement("EnteredHex");  
            if (null == hexElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(hexElem) returned null");
               return false;
            }
            XmlNode? hexNode = enteredHexesNode.AppendChild(hexElem);
            if (null == hexNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(hexNode) returned null");
               return false;
            }
            //-------------------------------------------------
            XmlElement? elem = aXmlDocument.CreateElement("Identifier");  
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Identifier) returned null");
               return false;
            }
            elem.SetAttribute("value", hex.Identifer.ToString());
            XmlNode? node = hexNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //-------------------------------------------------
            elem = aXmlDocument.CreateElement("Day");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Day) returned null");
               return false;
            }
            elem.SetAttribute("value", hex.Day.ToString());
            node = hexNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //-------------------------------------------------
            elem = aXmlDocument.CreateElement("HexName");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(HexName) returned null");
               return false;
            }
            elem.SetAttribute("value", hex.HexName);
            node = hexNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //-------------------------------------------------
            elem = aXmlDocument.CreateElement("IsEncounter");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsEncounter) returned null");
               return false;
            }
            elem.SetAttribute("value", hex.IsEncounter.ToString());
            node = hexNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //-------------------------------------------------
            elem = aXmlDocument.CreateElement("Position");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Position) returned null");
               return false;
            }
            elem.SetAttribute("value", hex.Position.ToString());
            node = hexNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //-------------------------------------------------
            elem = aXmlDocument.CreateElement("ColorAction");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(ColorAction) returned null");
               return false;
            }
            elem.SetAttribute("value", hex.ColorAction.ToString());
            node = hexNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //-------------------------------------------------
            XmlElement? eventNamesElem = aXmlDocument.CreateElement("EventNames");
            if (null == eventNamesElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(EventNames) returned null");
               return false;
            }
            eventNamesElem.SetAttribute("count", hex.EventNames.Count.ToString());
            XmlNode? eventNamesNode = hexNode.AppendChild(eventNamesElem);
            if (null == eventNamesNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(eventNamesNode) returned null");
               return false;
            }
            foreach( string sEventName in hex.EventNames )
            {
               elem = aXmlDocument.CreateElement("EventName");
               if (null == elem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(EventName) returned null");
                  return false;
               }
               elem.SetAttribute("value", sEventName);
               node = eventNamesNode.AppendChild(elem);
               if (null == node)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
                  return false;
               }
            }
            //-------------------------------------------------
            XmlElement? partyNamesElem = aXmlDocument.CreateElement("PartyNames");
            if (null == partyNamesElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(PartyNames) returned null");
               return false;
            }
            partyNamesElem.SetAttribute("count", hex.Party.Count.ToString());
            XmlNode? partyNamesNode = hexNode.AppendChild(partyNamesElem);
            if (null == partyNamesNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(partyNamesNode) returned null");
               return false;
            }
            foreach (string sPartyName in hex.Party)
            {
               elem = aXmlDocument.CreateElement("PartyName");
               if (null == elem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(PartyName) returned null");
                  return false;
               }
               elem.SetAttribute("value", sPartyName);
               node = partyNamesNode.AppendChild(elem);
               if (null == node)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
                  return false;
               }
            }
         }
         return true;
      }
      private bool CreateXmlForbiddenAudiences(XmlDocument aXmlDocument, IForbiddenAudiences audiences)
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): root is null");
            return false;
         }
         XmlElement? audiencesElem = aXmlDocument.CreateElement("ForbiddenAudiences");
         if (null == audiencesElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Territories) returned null");
            return false;
         }
         audiencesElem.SetAttribute("count", audiences.Count.ToString());
         XmlNode? audiencesNode = root.AppendChild(audiencesElem);
         if (null == audiencesNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(audiencesNode) returned null");
            return false;
         }
         //--------------------------------
         foreach (IForbiddenAudience audience in audiences)
         {
            XmlElement? audienceElem = aXmlDocument.CreateElement("ForbiddenAudience");
            if (null == audienceElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(ForbiddenAudience) returned null");
               return false;
            }
            XmlNode? audienceNode = audiencesNode.AppendChild(audienceElem);
            if (null == audienceNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(audienceNode) returned null");
               return false;
            }
            //-------------------------------------------------
            XmlElement? elem = aXmlDocument.CreateElement("Constraint");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Constraint) returned null");
               return false;
            }
            elem.SetAttribute("value", audience.Constraint.ToString());
            XmlNode? node = audienceNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //-------------------------------------------------
            elem = aXmlDocument.CreateElement("ForbiddenTerritory");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(ForbiddenTerritory) returned null");
               return false;
            }
            if (null == audience.ForbiddenTerritory)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): audience.ForbiddenTerritory=null");
               return false;
            }
            elem.SetAttribute("value", audience.ForbiddenTerritory.Name);
            node = audienceNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //-------------------------------------------------
            elem = aXmlDocument.CreateElement("TargetTerritory");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(TargetTerritory) returned null");
               return false;
            }
            if (null == audience.TargetTerritory)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): audience.TargetTerritory=null for " + audience.ForbiddenTerritory.Name);
               return false;
            }
            elem.SetAttribute("value", audience.TargetTerritory.Name);
            node = audienceNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //-------------------------------------------------
            elem = aXmlDocument.CreateElement("Assistant");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Assistant) returned null");
               return false;
            }
            if (null == audience.Assistant)
               elem.SetAttribute("value", "null");
            else
               elem.SetAttribute("value", audience.Assistant.Name);
            node = audienceNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //-------------------------------------------------
            elem = aXmlDocument.CreateElement("Day");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Day) returned null");
               return false;
            }
            elem.SetAttribute("value", audience.Day.ToString());
            node = audienceNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
         }
         return true;
      }
      private bool CreateXmlCaches(XmlDocument aXmlDocument, ICaches caches)
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): root is null");
            return false;
         }
         XmlElement? cachesElem = aXmlDocument.CreateElement("Caches");
         if (null == cachesElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Caches) returned null");
            return false;
         }
         cachesElem.SetAttribute("count", caches.Count.ToString());
         XmlNode? cachesNode = root.AppendChild(cachesElem);
         if (null == cachesNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(cachesNode) returned null");
            return false;
         }
         //--------------------------------
         foreach (ICache cache in caches)
         {
            XmlElement? cacheElem = aXmlDocument.CreateElement("Cache");
            if (null == cacheElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Cache) returned null");
               return false;
            }
            XmlNode? cacheNode = cachesNode.AppendChild(cacheElem);
            if (null == cacheNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(cacheNode) returned null");
               return false;
            }
            //-------------------------------------------------
            XmlElement? elem = aXmlDocument.CreateElement("TargetTerritory");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(TargetTerritory) returned null");
               return false;
            }
            elem.SetAttribute("value", cache.CacheTerritory.Name);
            XmlNode? node = cacheNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //-------------------------------------------------
            elem = aXmlDocument.CreateElement("Coin");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Coin) returned null");
               return false;
            }
            elem.SetAttribute("value", cache.Coin.ToString());
            node = cacheNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
         }
         return true;
      }
   }
#nullable disable
}
