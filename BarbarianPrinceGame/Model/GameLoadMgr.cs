using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Win32;

namespace BarbarianPrince
{
#nullable enable
   internal class GameLoadMgr
   {
      public static string theGamesDirectory = "";
      public static bool theIsCheckFileExist = false;
      //--------------------------------------------------
      public GameLoadMgr() { }
      //--------------------------------------------------
      public IGameInstance OpenGame()
      {
         try
         {
            if (false == Directory.Exists(theGamesDirectory)) // create directory if does not exists
               Directory.CreateDirectory(theGamesDirectory);
            string filename = theGamesDirectory + "Checkpoint.bpg";
            IGameInstance gi = ReadXml(filename);
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
            Console.WriteLine(ex.ToString());
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
               IGameInstance gi = ReadXml(dlg.FileName);
               Logger.Log(LogEnum.LE_GAME_INIT, "OpenGameFromFile(): gi=" + gi.ToString());
               string? gamePath = Path.GetDirectoryName(dlg.FileName); // save off the directory user chosen
               if( null == gamePath)
               {
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
               using (FileStream writer = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write))
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
      //--------------------------------------------------
      private IGameInstance ReadXml(string filename)
      {
         IGameInstance gi = new GameInstance();
         return gi;
      }
      //--------------------------------------------------
      private XmlDocument? CreateXml(IGameInstance gi)
      {
         XmlDocument aXmlDocument = new XmlDocument();
         if (null == aXmlDocument.DocumentElement)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): aXmlDocument.DocumentElement=null");
            return null;
         }
         aXmlDocument.LoadXml("<GameInstance></GameInstance>");
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
         Assembly assembly = Assembly.GetExecutingAssembly();
         if( null == assembly )
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): Assembly.GetExecutingAssembly()=null");
            return null;
         }
         Version? version = assembly.GetName().Version;
         if (null == version)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml():  assembly.GetName().Version=null");
            return null;
         }
         versionElem.SetAttribute("value", version.ToString());
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
         XmlElement? atRiskMountsElem = aXmlDocument.CreateElement("AtRiskMounts");
         if (null == atRiskMountsElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(AtRiskMounts) returned null");
            return null;
         }
         XmlNode? atRiskMountsNode = root.AppendChild(atRiskMountsElem);
         if (null == atRiskMountsNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(atRiskMountsNode) returned null");
            return null;
         }
         foreach(IMapItem mi in gi.AtRiskMounts)
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
         XmlNode? lostTrueLovesNode = root.AppendChild(lostTrueLovesElem);
         if (null == lostTrueLovesNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(lostTrueLovesNode) returned null");
            return null;
         }
         foreach (IMapItem mi in gi.AtRiskMounts)
         {
            XmlElement? lostTrueLoveElem = aXmlDocument.CreateElement("MapItem");
            if (null == lostTrueLoveElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(atRiskMountElem) returned null");
               return null;
            }
            versionElem.SetAttribute("value", mi.Name.ToString());
            XmlNode? lostTrueLoveNode = lostTrueLovesNode.AppendChild(lostTrueLoveElem);
            if (null == lostTrueLoveNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(lostTrueLoveNode) returned null");
               return null;
            }
            //------------------------------------------
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
            elem = aXmlDocument.CreateElement("BottomImageName");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(BottomImageName) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.BottomImageName);
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
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
            elem = aXmlDocument.CreateElement("Zoom");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Zoom) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.Zoom.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsHidden");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsHidden) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsHidden.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsExposedToUser");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsExposedToUser) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsExposedToUser.ToString());
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
            elem = aXmlDocument.CreateElement("IsRunAway");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsRunAway) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsRunAway.ToString());
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
            elem = aXmlDocument.CreateElement("IsMountSick");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsMountSick) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsMountSick.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsShowFireball");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsShowFireball) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsShowFireball.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(node) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsDisappear");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsDisappear) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsDisappear.ToString());
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
            elem = aXmlDocument.CreateElement("IsPoisonApplied");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsPoisonApplied) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsPoisonApplied.ToString());
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
            elem = aXmlDocument.CreateElement("IsShieldApplied");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(IsShieldApplied) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsShieldApplied.ToString());
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
            elem = aXmlDocument.CreateElement("Rider"); // only save off name of rider
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(Rider) returned null");
               return false;
            }
            if (null == mi.Rider)
               elem.SetAttribute("value", "null");
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
               mountElem.SetAttribute("value", mount.Name);
               XmlNode? mountNode = mountsNode.AppendChild(mountElem);
               if (null == mountNode)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(mountNode) returned null");
                  return false;
               }
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
            //--------------------------------
            terrElem = aXmlDocument.CreateElement("TerritoryStarting");  // name of territory
            if (null == terrElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(terrElem) returned null");
               return false;
            }
            terrElem.SetAttribute("value", mi.TerritoryStarting.Name);
            territoryNode = miNode.AppendChild(terrElem);
            if (null == territoryNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): AppendChild(territoryNode) returned null");
               return false;
            }
            //--------------------------------
            XmlElement? guideTerrElem = aXmlDocument.CreateElement("GuideTerritories");
            if (null == guideTerrElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml(): CreateElement(guideTerrElem) returned null");
               return false;
            }
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
         territoriesElem.SetAttribute("Name", attribute);
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
   }
#nullable disable
}
