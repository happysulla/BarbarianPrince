﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using WpfAnimatedGif;
using Button = System.Windows.Controls.Button;
using Point = System.Windows.Point;

namespace BarbarianPrince
{
   public class EventViewer : IView
   {
      static double theSmallElfImageHeight = 42;
      static double theSmallElfImageWidth = 21;
      public bool CtorError { get; } = false;
      private IGameEngine myGameEngine = null;
      private IGameInstance myGameInstance = null;
      private ITerritories myTerritories = null;
      //--------------------------------------------------------------------
      private IDieRoller myDieRoller = null;
      public int DieRoll { set; get; } = 0;
      //--------------------------------------------------------------------
      public RuleDialogViewer myRulesMgr = null;
      //--------------------------------------------------------------------
      private ScrollViewer myScrollViewerTextBlock = null;
      private StackPanel myStackPanel = null;
      private Canvas myCanvas = null;
      private TextBlock myTextBlock = null;
      //--------------------------------------------------------------------
      private readonly FontFamily myFontFam1 = new FontFamily("Courier New");
      //--------------------------------------------------------------------
      public EventViewer(IGameEngine ge, IGameInstance gi, Canvas c, ScrollViewer sv, StackPanel sp, ITerritories territories, IDieRoller dr)
      {
         myDieRoller = dr;
         if (null == ge)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventPanelViewer(): c=null");
            CtorError = true;
            return;
         }
         myGameEngine = ge;
         if (null == gi)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventPanelViewer(): c=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         if (null == c)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventPanelViewer(): c=null");
            CtorError = true;
            return;
         }
         myCanvas = c;
         if (null == territories)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventPanelViewer(): territories=null");
            CtorError = true;
            return;
         }
         myTerritories = territories;
         if (null == sv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventPanelViewer(): sv=null");
            CtorError = true;
            return;
         }
         myScrollViewerTextBlock = sv;
         if (null == sp)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventPanelViewer(): sp=null");
            CtorError = true;
            return;
         }
         myStackPanel = sp;
         //--------------------------------------------------------
         if (myScrollViewerTextBlock.Content is TextBlock)
            myTextBlock = (TextBlock)myScrollViewerTextBlock.Content;  // Find the TextBox in the visual tree
         if (null == myTextBlock)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventPanelViewer(): myTextBlock=null");
            CtorError = true;
            return;
         }
         //--------------------------------------------------------
         myRulesMgr = new RuleDialogViewer(myGameInstance, myGameEngine);
         if (true == myRulesMgr.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventPanelViewer(): RuleDialogViewer.CtorError=true");
            CtorError = true;
            return;
         }
         //--------------------------------------------------------
         if (false == CreateEvents(gi))
         {
            Logger.Log(LogEnum.LE_ERROR, "EventPanelViewer(): CreateEvents() returned false");
            CtorError = true;
            return;
         }
         if (null == myRulesMgr.Events)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventPanelViewer(): myRulesMgr.Events=null");
            CtorError = true;
            return;
         }
      }
      private bool CreateEvents(IGameInstance gi)
      {
         try
         {
            string filename = ConfigFileReader.theConfigDirectory + "Events.txt";
            ConfigFileReader cfr = new ConfigFileReader(filename);
            if (true == cfr.CtorError)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateEvents(): cfr.CtorError=true");
               return false;
            }
            myRulesMgr.Events = cfr.Entries;
            if (0 == myRulesMgr.Events.Count)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateEvents(): myRulesMgr.Events.Count=0");
               return false;
            }
            // For each event, create a dictionary entry. Assume no more than three die rolls per event
            foreach (string key in myRulesMgr.Events.Keys)
               gi.DieResults[key] = new int[3] { Utilities.NO_RESULT, Utilities.NO_RESULT, Utilities.NO_RESULT };
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateEvents(): e=" + e.ToString());
            return false;
         }
         return true;
      }
      //--------------------------------------------------------------------
      public void UpdateView(ref IGameInstance gi, GameAction action)
      {
         Logger.Log(LogEnum.LE_GAME_PARTYMEMBER_COUNT, "UpdateView() c=" + gi.PartyMembers.Count.ToString());
         gi.IsGridActive = true;
         switch (action)
         {
            case GameAction.UnitTestCommand:
            case GameAction.UnitTestNext:
               break;
            case GameAction.UpdateGameOptions:
            case GameAction.ShowAllRivers:
            case GameAction.ShowPartyPath:
               break;
            case GameAction.UpdateUndo:
               myScrollViewerTextBlock.Cursor = Cursors.Arrow;
               gi.IsGridActive = false;
               if (false == OpenEvent(gi, gi.EventActive))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): OpenEvent() returned false ae=" + myGameInstance.EventActive + " a=" + action.ToString());
               break;
            case GameAction.UpdateNewGame:
            case GameAction.UpdateLoadingGame:
               myGameInstance = gi;
               myRulesMgr.GameInstance = gi;
               gi.IsGridActive = false;
               myScrollViewerTextBlock.Cursor = Cursors.Arrow;
               try // resync the gi.DieResults[] to initial conditions
               {
                  foreach (string key in myRulesMgr.Events.Keys)
                     gi.DieResults[key] = new int[3] { Utilities.NO_RESULT, Utilities.NO_RESULT, Utilities.NO_RESULT };
               }
               catch (Exception e)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateEvents(): e=" + e.ToString());
                  return;
               }
               if (false == OpenEvent(gi, gi.EventActive))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): OpenEvent() returned false ae=" + myGameInstance.EventActive + " a=" + action.ToString());
               break;
            case GameAction.ShowReportErrorDialog:
               ShowReportErrorDialog dialogReportError = new ShowReportErrorDialog();
               dialogReportError.Show();
               break;
            case GameAction.ShowAboutDialog:
               ShowAboutDialog dialogAbout = new ShowAboutDialog();
               dialogAbout.Show();
               break;
            case GameAction.ShowInventory:
               InventoryDisplayDialog dialogInventory = new InventoryDisplayDialog(myGameInstance, myRulesMgr); // Set Options in Game
               if (true == dialogInventory.CtorError)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): InventoryDisplayDialog CtorError=true");
                  return;
               }
               if (true == dialogInventory.ShowDialog())
               {
               }
               break;
            case GameAction.ShowGameFeats:
               FeatDisplayDialog dialogShowFeats = new FeatDisplayDialog(myRulesMgr);
               if (true == dialogShowFeats.CtorError)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): FeatDisplayDialog CtorError=true");
                  return;
               }
               if (true == dialogShowFeats.ShowDialog())
               {
               }
               break;
            case GameAction.ShowRuleListing:
               RuleListingDialog dialogRuleListing = new RuleListingDialog(myRulesMgr);
               if (true == dialogRuleListing.CtorError)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): RuleListingDialog CtorError=true");
                  return;
               }
               dialogRuleListing.Show();
               break;
            case GameAction.ShowCharacterDescription:
               ShowCounterHelpDialog dialogShowCharDesc = new ShowCounterHelpDialog(myRulesMgr);
               if (true == dialogShowCharDesc.CtorError)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): dialogShowCharDesc CtorError=true");
                  return;
               }
               dialogShowCharDesc.Show();
               break;
            case GameAction.ShowEventListing:
               RuleListingDialog dialogEventListing = new RuleListingDialog(myRulesMgr, true);
               if (true == dialogEventListing.CtorError)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): RuleListingDialog CtorError=true");
                  return;
               }
               dialogEventListing.Show();
               break;
            //-------------------------------------
            case GameAction.TravelLostCheck:
               EventViewerTravelTable aTravelTableViewer = new EventViewerTravelTable(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aTravelTableViewer.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aTravelTableViewer.CtorError=true");
               else if (false == aTravelTableViewer.PerformTravel(ShowResultsTravel))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): TravelLostCheck PerformTravel() returned false");
               break;
            //-------------------------------------
            case GameAction.EncounterLootStart:
            case GameAction.EncounterLoot:
               if (0 < gi.CapturedWealthCodes.Count)
               {
                  int wealthCode = gi.CapturedWealthCodes[0];
                  gi.CapturedWealthCodes.RemoveAt(0);
                  EventViewerTreasureTable aTreasureTableViewer = new EventViewerTreasureTable(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
                  if (true == aTreasureTableViewer.CtorError)
                     Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aTreasureTableViewer.CtorError=true");
                  else if (false == aTreasureTableViewer.GetTreasure(ShowResultsTreasure, wealthCode, gi.ActiveMember, gi.PegasusTreasure))
                     Logger.Log(LogEnum.LE_ERROR, "UpdateView(): GetTreasure() returned false wc=" + wealthCode.ToString());
               }
               else
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): Invalid state with gi.CapturedWealthCodes.Count=0 a=" + action.ToString());
               }
               break;
            //-------------------------------------
            case GameAction.EncounterCombat:
               gi.IsReaverClanTrade = false; // only for e015a - no trading occurs if there is combat
               if ("e016a" == gi.EventStart) // only for e016a - no gift occurs if there is combat
                  gi.IsMagicianProvideGift = false;
               EventViewerCombatMgr aCombatMgrViewer = new EventViewerCombatMgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller, myStackPanel);
               if (true == aCombatMgrViewer.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "EventPanelViewer(): aCombatMgrViewer.CtorError=true");
               else if (false == aCombatMgrViewer.PerformCombat(ShowResultsCombat))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): PerformCombat() returned false");
               break;
            //-------------------------------------
            case GameAction.E031LootedTomb:
               EventViewerE031Mgr aE031MgrViewer = new EventViewerE031Mgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aE031MgrViewer.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aE031MgrViewer.CtorError=true");
               else if (false == aE031MgrViewer.OpenTomb(ShowTombOpeningResult))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): OpenTomb() returned false");
               break;
            //-------------------------------------
            case GameAction.E018MarkOfCain:
               if (false == myGameInstance.IsMarkOfCain)
               {
                  EventViewerE018Mgr aE018MgrViewer = new EventViewerE018Mgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
                  if (true == aE018MgrViewer.CtorError)
                     Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aE018MgrViewer.CtorError=true");
                  else if (false == aE018MgrViewer.MarkOfCainCheck(ShowMarkOfCainResult))
                     Logger.Log(LogEnum.LE_ERROR, "UpdateView(): MarkOfCainCheck() returned false");
               }
               else
               {
                  gi.IsGridActive = false;  // GameAction.E018MarkOfCain
                  gi.EventDisplayed = gi.EventActive = "e018d";
                  if (false == OpenEvent(gi, gi.EventActive))
                     Logger.Log(LogEnum.LE_ERROR, "UpdateView(): OpenEvent() returned false ae=" + myGameInstance.EventActive + " a=" + action.ToString());
               }
               break;
            //-------------------------------------
            case GameAction.E039TreasureChest:
               EventViewerE039Mgr aE039MgrViewer = new EventViewerE039Mgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aE039MgrViewer.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aE039MgrViewer.CtorError=true");
               else if (false == aE039MgrViewer.OpenChest(ShowE039ChestOpeningResult))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): OpenChest() returned false");
               break;
            //-------------------------------------
            case GameAction.E040TreasureChest:
               EventViewerE040Mgr aE040MgrViewer = new EventViewerE040Mgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aE040MgrViewer.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aE040MgrViewer.CtorError=true");
               else if (false == aE040MgrViewer.OpenChest(ShowE040ChestOpeningResult))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): OpenChest() returned false");
               break;
            //-------------------------------------
            case GameAction.E043SmallAltar:
               EventViewerE043Mgr aE043MgrViewer = new EventViewerE043Mgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aE043MgrViewer.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aE043MgrViewer.CtorError=true");
               else if (false == aE043MgrViewer.CheckRetreival(ShowRetreivalResult))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): CheckRetreival() returned false");
               break;
            //-------------------------------------
            case GameAction.E044HighAltar:
               EventViewerE044HighAltarMgr aE044MgrViewer = new EventViewerE044HighAltarMgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aE044MgrViewer.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aE044MgrViewer.CtorError=true");
               else if (false == aE044MgrViewer.CheckInvocation(ShowE044InvocationResult))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): CheckInvocation() returned false");
               break;
            //-------------------------------------
            case GameAction.E045ArchOfTravelEnd:
               if (null != myScrollViewerTextBlock.Cursor)
               {
                  myScrollViewerTextBlock.Cursor.Dispose();
                  myScrollViewerTextBlock.Cursor = Cursors.Arrow;
               }
               EventViewerTravelTable aTravelTableViewer1 = new EventViewerTravelTable(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aTravelTableViewer1.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aTravelTableViewer.CtorError=true");
               else if (false == aTravelTableViewer1.PerformTravel(ShowResultsTravelThroughArch))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(E045ArchOfTravelEnd): PerformTravel() returned false");
               break;
            //-------------------------------------
            case GameAction.E060JailOvernight:
               EventViewerE060Mgr aE060MgrViewer = new EventViewerE060Mgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aE060MgrViewer.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aE060MgrViewer.CtorError=true");
               else if (false == aE060MgrViewer.PaymentCheck(ShowE060ReleasePrisonerResult))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): PaymentCheck() returned false");
               break;
            //-------------------------------------
            case GameAction.E073WitchMeet:
               EventViewerE073FrogMgr aE073FrogMgr = new EventViewerE073FrogMgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aE073FrogMgr.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aE073FrogMgr.CtorError=true");
               else if (false == aE073FrogMgr.WitchCurseCheck(ShowE073WitchSpellResult))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): WitchCurseCheck() returned false");
               break;
            //-------------------------------------
            case GameAction.E079HeavyRains:
            case GameAction.E079HeavyRainsStartDayCheck:
            case GameAction.E079HeavyRainsStartDayCheckInAir:
               EventViewerE079HeavyRainMgr aE079HeavyRainMgr = new EventViewerE079HeavyRainMgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aE079HeavyRainMgr.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aE079HeavyRainMgr.CtorError=true");
               else if (false == aE079HeavyRainMgr.ColdCheck(ShowE079ColdCheckResult))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): ShowE079ColdCheckResult() returned false");
               break;
            //-------------------------------------
            case GameAction.E082SpectreMagic:
            case GameAction.E091PoisonSnake:
               EventViewerE343Mgr aE343MgrViewer = new EventViewerE343Mgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aE343MgrViewer.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aE343MgrViewer.CtorError=true");
               else if (false == aE343MgrViewer.FindVictim(ShowFindVictimResults))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): ShowFindVictimResults() returned false");
               break;
            //-------------------------------------
            case GameAction.E069WoundedWarriorRedistribute:
            case GameAction.E078BadGoingRedistribute:
            case GameAction.E079HeavyRainsRedistribute:
            case GameAction.E086HighPassRedistribute:
            case GameAction.E095MountAtRiskEnd:
            case GameAction.E121SunStrokeEnd:
            case GameAction.E126RaftInCurrentRedistribute:
            case GameAction.TravelAirRedistribute:
               EventViewerTransportMgr aTransportMgrViewer1 = new EventViewerTransportMgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr);
               if (true == aTransportMgrViewer1.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aTransportMgrViewer.CtorError=true");
               else if (false == aTransportMgrViewer1.TransportLoad(ShowTransportAfterRedistribute))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): TransportLoad() returned false");
               break;
            //-------------------------------------
            case GameAction.E085Falling:
               EventViewerE085LedgeMgr aE085MgrViewer = new EventViewerE085LedgeMgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aE085MgrViewer.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aE085MgrViewer.CtorError=true");
               else if (false == aE085MgrViewer.FallCheck(ShowE085FallingResult))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): FallCheck() returned false");
               break;
            //-------------------------------------
            case GameAction.E088FallingRocks:
               EventViewerE088RockMgr aE088MgrViewer = new EventViewerE088RockMgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aE088MgrViewer.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aE088MgrViewer.CtorError=true");
               else if (false == aE088MgrViewer.FallingRockCheck(ShowE088FallingRocksResult))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): FallingRockCheck(() returned false");
               break;
            //-------------------------------------
            case GameAction.E090Quicksand:
               EventViewerE090QuicksandMgr aE090MgrViewer = new EventViewerE090QuicksandMgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aE090MgrViewer.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aE090MgrViewer.CtorError=true");
               else if (false == aE090MgrViewer.QuicksandCheck(ShowE090QuicksandResult))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): QuicksandCheck(() returned false");
               break;
            //-------------------------------------
            case GameAction.E095MountAtRisk:
               EventViewerE095TiredMountMgr aE095TiredMountMgr = new EventViewerE095TiredMountMgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aE095TiredMountMgr.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aE095TiredMountMgr.CtorError=true");
               else if (false == aE095TiredMountMgr.TiredMountCheck(ShowE095MountCheckResult))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): TiredMountCheck() returned false");
               break;
            //-------------------------------------
            case GameAction.E097FleshRot:
               EventViewerE097FleshRotMgr aE097FleshRotMgr = new EventViewerE097FleshRotMgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aE097FleshRotMgr.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aE097FleshRotMgr.CtorError=true");
               else if (false == aE097FleshRotMgr.FleshRotCheck(ShowE097FleshRotResult))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): WitchCurseCheck() returned false");
               break;
            //-------------------------------------
            case GameAction.E106OvercastLost:
               EventViewerE106OvercastMgr aE106MgrViewer = new EventViewerE106OvercastMgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aE106MgrViewer.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aE106MgrViewer.CtorError=true");
               else if (false == aE106MgrViewer.OvercastLostCheck(ShowE106OvercastLostResult))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): OvercastLostCheck() returned false");
               break;
            //-------------------------------------
            case GameAction.E109PegasusCapture:
               EventViewerE109PegasusMgr aE109MgrViewer = new EventViewerE109PegasusMgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aE109MgrViewer.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aE109MgrViewer.CtorError=true");
               else if (false == aE109MgrViewer.PegasusCaptureCheck(ShowE109PegasusCapture))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): PegasusCaptureCheck() returned false");
               break;
            //-------------------------------------
            case GameAction.E111StormDemonRepel:
               EventViewerE111DemonMgr aE111MgrViewer = new EventViewerE111DemonMgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aE111MgrViewer.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aE111MgrViewer.CtorError=true");
               else if (false == aE111MgrViewer.StormDemonRepelCheck(ShowE111StormDemonRepelResults))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): StormDemonRepelCheck() returned false");
               break;
            //-------------------------------------
            case GameAction.E121SunStroke:
               EventViewerE121SunStrokeMgr aE121SunStrokeMgr = new EventViewerE121SunStrokeMgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aE121SunStrokeMgr.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aE121SunStrokeMgr.CtorError=true");
               else if (false == aE121SunStrokeMgr.SunstrokeCheck(ShowE121SunStrokeCheckResult))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): SunstrokeCheck() returned false");
               break;
            //-------------------------------------
            case GameAction.E123BlackKnightRefuse:
               EventViewerE123RefuseFightMgr aE123MgrViewer = new EventViewerE123RefuseFightMgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aE123MgrViewer.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aE123MgrViewer.CtorError=true");
               else if (false == aE123MgrViewer.DisgustCheck(ShowE123DisgustCheck))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): DisgustCheck(ShowE123DisgustCheck) returned false");
               break;
            //-------------------------------------
            case GameAction.E126RaftInCurrent:
               EventViewerE126RaftInCurrentMgr aE126RaftInCurrentMgr = new EventViewerE126RaftInCurrentMgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aE126RaftInCurrentMgr.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aE126RaftInCurrentMgr.CtorError=true");
               else if (false == aE126RaftInCurrentMgr.RaftInCurrentCheck(ShowE126RaftInCurrentResult))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): RaftInCurrentCheck() returned false");
               break;
            //-------------------------------------
            case GameAction.E133Plague:
               EventViewerE133Mgr aE133MgrViewer = new EventViewerE133Mgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aE133MgrViewer.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aE133MgrViewer.CtorError=true");
               else if (false == aE133MgrViewer.CheckPlague(ShowSearchRuinsPlague))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): CheckPlague() returned false");
               break;
            //-------------------------------------
            case GameAction.E134ShakyWalls:
               EventViewerE134Mgr aE134MgrViewer = new EventViewerE134Mgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aE134MgrViewer.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aE134MgrViewer.CtorError=true");
               else if (false == aE134MgrViewer.CheckRubbleDamage(ShowSearchRuinsShakey))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): CheckRubbleDamage() returned false");
               break;
            //-------------------------------------
            case GameAction.E203EscapeFromPrison:
               EventViewerE203Mgr aE203MgrViewer = new EventViewerE203Mgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aE203MgrViewer.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aE203MgrViewer.CtorError=true");
               else if (false == aE203MgrViewer.CheckPrisonBreak(ShowPrisonBreakResults))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): CheckPrisonBreak() returned false");
               break;
            //-------------------------------------
            case GameAction.E212TempleCurse:
               EventViewerE212CurseMgr aE212TempleCurse = new EventViewerE212CurseMgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aE212TempleCurse.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aE212TempleCurse.CtorError=true");
               else if (false == aE212TempleCurse.CurseCheck(ShowE212TempleCurse))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): DisgustCheck() returned false");
               break;
            //-------------------------------------
            case GameAction.Hunt:
               EventViewerHuntMgr aHuntMgrViewer = new EventViewerHuntMgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aHuntMgrViewer.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aHuntMgrViewer.CtorError=true");
               else if (false == aHuntMgrViewer.PerformHunt(ShowResultsOfHunt))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): PerformHunt() returned false");
               break;
            //-------------------------------------
            case GameAction.CampfirePlagueDust:
               EventViewerPlagueDustMgr aPlagueDustMgrViewer = new EventViewerPlagueDustMgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aPlagueDustMgrViewer.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aPlagueDustMgrViewer.CtorError=true");
               else if (false == aPlagueDustMgrViewer.ApplyPlague(ShowPlagueDust))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): ApplyPlague() returned false");
               break;
            //-------------------------------------
            case GameAction.CampfireTalismanDestroy:
               EventViewerE189Mgr aE189MgrViewer = new EventViewerE189Mgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aE189MgrViewer.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aE189MgrViewer.CtorError=true");
               else if (false == aE189MgrViewer.CheckTalismanDestruction(ShowCharismaTalismanDestroy))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): CheckTalismanDestruction() returned false");
               break;
            //-------------------------------------
            case GameAction.CampfireAntipoisionAmuletDestroy:
               EventViewerE187Mgr aE187MgrViewer = new EventViewerE187Mgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aE187MgrViewer.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aE189MgrViewer.CtorError=true");
               else if (false == aE187MgrViewer.CheckAntipoisonAmuletDestruction(ShowAntipoisonAmuletDestroy))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): CheckAntipoisonAmuletDestruction() returned false");
               break;
            //-------------------------------------
            case GameAction.CampfireMountDieCheck:
               EventViewerE096MountsDieMgr aE096MountsDieMgr = new EventViewerE096MountsDieMgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aE096MountsDieMgr.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aE096MountsDieMgr.CtorError=true");
               else if (false == aE096MountsDieMgr.MountDieCheck(ShowE096MountDieCheckResult))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): MountDieCheck() returned false");
               break;
            //-------------------------------------
            case GameAction.CampfireFalconCheck:
               EventViewerE107FalconMgr aE107MgrViewer = new EventViewerE107FalconMgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aE107MgrViewer.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aE107MgrViewer.CtorError=true");
               else if (false == aE107MgrViewer.FalconLeaveCheck(ShowE107FalconCheckResult))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): OvercastLostCheck() returned false");
               break;
            //-------------------------------------
            case GameAction.CampfireStarvationCheck:
               EventViewerStarvationMgr aStarvationMgrViewer = new EventViewerStarvationMgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aStarvationMgrViewer.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aStarvationMgrViewer.CtorError=true");
               else if (false == aStarvationMgrViewer.FeedParty(ShowResultsFeeding))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): FeedParty() returned false");
               break;
            //-------------------------------------
            case GameAction.CampfireLodgingCheck:
               EventViewerLodgingMgr aLodgingMgrViewer = new EventViewerLodgingMgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aLodgingMgrViewer.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "EventPanelViewer(): aLodgingMgrViewer.CtorError=true");
               else if (false == aLodgingMgrViewer.LodgeParty(ShowResultsLodging))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): LodgeParty() returned false");
               break;
            //-------------------------------------
            case GameAction.CampfireTrueLoveCheck:
               EventViewerE228Mgr aE228MgrViewer = new EventViewerE228Mgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aE228MgrViewer.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "EventPanelViewer(): aE228MgrViewer.CtorError=true");
               if (false == aE228MgrViewer.CheckTrueLoveReturn(ShowTrueLoveCheck))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): CheckTrueLoveReturn() returned false");
               break;
            //-------------------------------------
            case GameAction.CampfireLoadTransport:
               EventViewerTransportMgr aTransportMgrViewer = new EventViewerTransportMgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr);
               if (true == aTransportMgrViewer.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aTransportMgrViewer.CtorError=true");
               else if (false == aTransportMgrViewer.TransportLoad(ShowTransport))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): TransportLoad() returned false");
               break;
            //-------------------------------------
            case GameAction.CampfireDisgustCheck:
               EventViewerE010Mgr aE010MgrViewer = new EventViewerE010Mgr(myGameInstance, myCanvas, myScrollViewerTextBlock, myRulesMgr, myDieRoller);
               if (true == aE010MgrViewer.CtorError)
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): aE010MgrViewer.CtorError=true");
               else if (false == aE010MgrViewer.DisgustCheck(ShowE010DisgustCheck))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): DisgustCheck() returned false");
               break;
            //-------------------------------------
            case GameAction.UpdateEventViewerDisplay:

               gi.IsGridActive = false;
               if (false == OpenEvent(gi, gi.EventDisplayed))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): OpenEvent() returned false ae=" + myGameInstance.EventActive + " a=" + action.ToString());
               break;
            case GameAction.Travel:
               if (false == OpenEvent(gi, gi.EventActive))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): OpenEvent() returned false ae=" + myGameInstance.EventActive + " a=" + action.ToString());
               break;
            case GameAction.EncounterStart:
            case GameAction.EncounterSurrender:
            case GameAction.E045ArchOfTravel:
            case GameAction.CampfireWakeup:
            case GameAction.EndGameLost:
            case GameAction.EndGameWin:
            default:
               gi.IsGridActive = false;
               if (false == OpenEvent(gi, gi.EventActive))
                  Logger.Log(LogEnum.LE_ERROR, "UpdateView(): OpenEvent() returned false ae=" + myGameInstance.EventActive + " a=" + action.ToString());
               break;
         }
      }
      public bool OpenEvent(IGameInstance gi, string key)
      {
         if (null == myTextBlock)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): myTextBlock=null");
            return false;
         }
         if (null == myRulesMgr.Events)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): myRulesMgr.Events=null");
            return false;
         }
         //------------------------------------
         try
         {
            foreach (Inline inline in myTextBlock.Inlines) // Clean up resources from old link before adding new one
            {
               if (inline is InlineUIContainer)
               {
                  InlineUIContainer ui = (InlineUIContainer)inline;
                  if (ui.Child is Button b)
                     b.Click -= Button_Click;
               }
            }
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): for key=" + key + " e=" + e.ToString());
            return false;
         }
         //------------------------------------
         try
         {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"<TextBlock xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' Name='myTextBlockDisplay' xml:space='preserve' Width='600' Background='#17000000' FontFamily='Georgia' FontSize='18' TextWrapping='WrapWithOverflow' IsHyphenationEnabled='true' LineStackingStrategy='BlockLineHeight' Margin='3,0,3,0'>");
            sb.Append(myRulesMgr.Events[key]);
            sb.Append(@"</TextBlock>");
            StringReader sr = new StringReader(sb.ToString());
            XmlTextReader xr = new XmlTextReader(sr);
            myTextBlock = (TextBlock)XamlReader.Load(xr);
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): for key=" + key + " e=" + e.ToString());
            return false;
         }
         //------------------------------------
         myScrollViewerTextBlock.Content = myTextBlock;
         myTextBlock.MouseDown += TextBlock_MouseDown;
         //--------------------------------------------------
         int dieNumIndex = 0;
         bool isModified = true;
         bool[] isDieShown = new bool[4] { true, false, false, false };
         int[] eventDieRolls = null;
         try
         {
            eventDieRolls = gi.DieResults[key];
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): for key=" + key + " e=" + e.ToString());
            return false;
         }
         //------------------------------------
         while (dieNumIndex < 3 && true == isModified) // substitute die rolls that have occurred when multiple die rolls are in myTextBlock
         {
            int dieCount = 0;
            isModified = false;
            foreach (Inline inline in myTextBlock.Inlines)
            {
               if (inline is InlineUIContainer)
               {
                  InlineUIContainer ui = (InlineUIContainer)inline;
                  if (ui.Child is Button b)
                  {
                     if (false == SetButtonState(gi, key, b))
                        Logger.Log(LogEnum.LE_ERROR, "OpenEvent(): SetButtonState() returned false");
                  }
                  else if (ui.Child is Image img)
                  {
                     ImageBehavior.SetAnimatedSource(img, img.Source);
                     if ((true == img.Name.Contains("DieRoll")) || (true == img.Name.Contains("DiceRoll")) || (true == img.Name.Contains("Die3Roll")))
                     {
                        if (true == isDieShown[dieCount])
                        {
                           if (Utilities.NO_RESULT == eventDieRolls[dieNumIndex]) // if true, perform a one time insert b/c dieNumIndex increments by one
                           {
                              if ((true == myGameInstance.IsGiftCharmActive) || (true == myGameInstance.IsSlaveGirlActive) || (true == gi.IsLadyAeravirRerollActive))
                                 img.Visibility = Visibility.Visible;
                           }
                           else
                           {
                              img.Visibility = Visibility.Hidden;
                              if ((false == myGameInstance.IsGiftCharmActive) && (false == myGameInstance.IsSlaveGirlActive) && (false == gi.IsLadyAeravirRerollActive))
                              {
                                 Run newInline = new Run(eventDieRolls[dieNumIndex].ToString());  // Insert the die roll number result
                                 myTextBlock.Inlines.InsertBefore(inline, newInline); // If modified, need to start again
                              }
                              else
                              {
                                 Button b1 = new Button() { Content = eventDieRolls[dieNumIndex].ToString(), FontFamily = myFontFam1, FontSize = 12, Height = 16, Width = 48 };
                                 myTextBlock.Inlines.InsertAfter(inline, new InlineUIContainer(b1));
                                 b1.Click += Button_Click;
                              }
                              isModified = true;
                              ++dieNumIndex;
                              isDieShown[dieCount] = false;
                              isDieShown[++dieCount] = true;
                              break;
                           }
                        }
                        else
                        {
                           if ((false == myGameInstance.IsGiftCharmActive) && (false == gi.IsLadyAeravirRerollActive))
                              img.Visibility = Visibility.Hidden;
                        }
                        ++dieCount;
                     }
                     else if ((gi.GamePhase == GamePhase.Encounter) && (Utilities.NO_RESULT < gi.DieResults["e045b"][0])) // Arch Travel with number of lost days already rolled
                     {
                        img.Visibility = Visibility.Hidden;
                        double sizeCursor = Utilities.ZoomCanvas * Utilities.ZOOM * Utilities.theMapItemSize;
                        System.Windows.Point hotPoint = new System.Windows.Point(Utilities.theMapItemOffset, sizeCursor * 0.5); // set the center of the MapItem as the hot point for the cursor
                        Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Target"), Width = sizeCursor, Height = sizeCursor };
                        myScrollViewerTextBlock.Cursor = Utilities.ConvertToCursor(img1, hotPoint);
                     }
                     else if (("e016d" == gi.EventActive) && (Utilities.NO_RESULT == gi.DieResults["e016d"][0]))
                     {
                        img.Visibility = Visibility.Hidden;
                     }
                  }
               }
            }// end foreach
         } // end while
           //--------------------------------------------------
         myDieRoller.DieMutex.WaitOne();
         AppendAtEnd(gi, key);
         myDieRoller.DieMutex.ReleaseMutex();
         //--------------------------------------------------
         if (myGameInstance.EventDisplayed == myGameInstance.EventActive)
            myScrollViewerTextBlock.Background = Utilities.theBrushScrollViewerActive;
         else
            myScrollViewerTextBlock.Background = Utilities.theBrushScrollViewerInActive;
         return true;
      }
      public bool ShowRule(string key)
      {
         if (false == myRulesMgr.ShowRule(key))
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowRule() key=" + key);
            return false;
         }
         return true;
      }
      public bool ShowTable(string key)
      {
         if (false == myRulesMgr.ShowTable(key))
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowTable() key=" + key);
            return false;
         }
         return true;
      }
      public bool ShowRegion(string key)
      {
         // Remove any existing UI elements from the Canvas
         List<UIElement> results = new List<UIElement>();
         foreach (UIElement ui in myCanvas.Children)
         {
            if (ui is Polygon)
               results.Add(ui);
         }
         foreach (UIElement ui1 in results)
            myCanvas.Children.Remove(ui1);
         //--------------------------------
         if ("Tragoth River" == key) // show several hexes for Tragoth
         {
            ITerritory tAnchor = myTerritories.Find("1101");
            if (null == tAnchor)
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowRegion(): Unable to find name=1101");
               return false;
            }
            if (false == SetThumbnailState(myCanvas, tAnchor))
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowRegion(): SetThumbnailState returned false name=" + key);
               return false;
            }
            foreach (string s in Utilities.theNorthOfTragothHexes)
            {
               ITerritory t1 = myTerritories.Find(s);
               if (null == t1)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ShowRegion(): Unable to find name=" + s);
                  return false;
               }
               PointCollection points1 = new PointCollection();
               foreach (IMapPoint mp2 in t1.Points)
                  points1.Add(new System.Windows.Point(mp2.X, mp2.Y));
               Polygon aPolygon1 = new Polygon { Fill = Utilities.theBrushRegion, Points = points1, Tag = t1.ToString() };
               myCanvas.Children.Add(aPolygon1);
            }
            return true;
         }
         //--------------------------------
         ITerritory t = myTerritories.Find(key);
         if (null == t)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowRegion(): Unable to find name=" + key);
            return false;
         }
         //--------------------------------
         if (false == SetThumbnailState(myCanvas, t))
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowRegion(): SetThumbnailState returned false name=" + key);
            return false;
         }
         PointCollection points = new PointCollection();
         foreach (IMapPoint mp1 in t.Points)
            points.Add(new System.Windows.Point(mp1.X, mp1.Y));
         Polygon aPolygon = new Polygon { Fill = Utilities.theBrushRegion, Points = points, Tag = t.ToString() };
         myCanvas.Children.Add(aPolygon);
         return true;
      }
      //--------------------------------------------------------------------
      private bool SetThumbnailState(Canvas c, ITerritory t)
      {
         ScrollViewer scrollViewer = (ScrollViewer)c.Parent; // set thumbnails of scroll viewer to find the target hex
         if (null == scrollViewer)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetThumbnailState(): scrollViewer=null");
            return false;
         }
         double percentHeight = (t.CenterPoint.Y / myCanvas.ActualHeight);
         double percentToScroll = 0.0;
         if (percentHeight < 0.25)
            percentToScroll = 0.0;
         else if (0.75 < percentHeight)
            percentToScroll = 1.0;
         else
            percentToScroll = percentHeight / 0.5 - 0.5;
         double amountToScroll = percentToScroll * scrollViewer.ScrollableHeight;
         scrollViewer.ScrollToVerticalOffset(amountToScroll);
         //--------------------------------------------------------------------
         double percentWidth = (t.CenterPoint.X / myCanvas.ActualWidth);
         if (percentWidth < 0.25)
            percentToScroll = 0.0;
         else if (0.75 < percentWidth)
            percentToScroll = 1.0;
         else
            percentToScroll = percentWidth / 0.5 - 0.5;
         amountToScroll = percentToScroll * scrollViewer.ScrollableWidth;
         scrollViewer.ScrollToHorizontalOffset(amountToScroll);
         return true;
      }
      private bool SetButtonState(IGameInstance gi, string key, Button b)
      {
         int cost = 0;
         string content = (string)b.Content;
         if( null == content )
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewer.SetButtonState(): content=null for key=" + key);
            return false;
         }
         if ((key != myGameInstance.EventActive) && (false == content.StartsWith("e")))
         {
            b.IsEnabled = false;
            return true;
         }
         switch (key)
         {
            case "e000d":
               b.IsEnabled = true;
               if (("  +  " == content) && ( 5 < gi.WitAndWile))
                  b.IsEnabled = false;
               if (("  -  " == content) && (gi.WitAndWile < 3))
                  b.IsEnabled = false;
               break;
            case "e002a":
            case "e004":
            case "e005":
               if (Utilities.NO_RESULT == myGameInstance.DieResults[key][0])
               {
                  if (("Talk " == content) || ("Evade" == content) || ("Fight" == content))
                     b.IsEnabled = false;
                  return true;
               }
               break;
            case "e011a":
            case "e012a":
               b.IsEnabled = false;
               if ( ("  +  " == content) && (1 < gi.GetCoins()) && (0 < gi.GetTotalFreeLoad()) )
                  b.IsEnabled = true;
               if (("  -  " == content) && (0 < gi.PurchasedFood))
                  b.IsEnabled = true;
               break;
            case "e014a":
               if (Utilities.NO_RESULT == myGameInstance.DieResults[key][0])
               {
                  if ("Inquiries" == content)
                     b.IsEnabled = false;
                  return true;
               }
               break;
            case "e015b":
               b.IsEnabled = false;
               int coinCount = gi.GetCoins();
               cost = 6;
               if (true == gi.IsMerchantWithParty)
                  cost = (int)Math.Ceiling((double)cost * 0.5);
               if ("  +  " == content)
               {
                  if ((0 < coinCount) && ("FoodPlus" == b.Name) && (0 < gi.GetTotalFreeLoad()))
                     b.IsEnabled = true;
                  if ((cost <= coinCount) && ("MountPlus" == b.Name))
                     b.IsEnabled = true;
               }
               else if ("  -  " == content)
               {
                  if ((0 < gi.PurchasedFood) && ("FoodMinus" == b.Name))
                     b.IsEnabled = true;
                  if ((0 < gi.PurchasedMount) && ("MountMinus" == b.Name))
                     b.IsEnabled = true;
               }
               break;
            case "e020":
               if (Utilities.NO_RESULT == myGameInstance.DieResults[key][0])
               {
                  if (("Talk " == content) || ("Evade" == content) || ("Fight" == content))
                     b.IsEnabled = false;
                  return true;
               }
               break;
            case "e021":
               if ((Utilities.NO_RESULT == myGameInstance.DieResults[key][1]) || (Utilities.NO_RESULT == myGameInstance.DieResults[key][1])) // do nothing until 2nd die is rolled
               {
                  if (("Talk " == content) || ("Evade" == content) || ("Fight" == content))
                     b.IsEnabled = false;
                  return true;
               }
               break;
            case "e023":
               if (Utilities.NO_RESULT == myGameInstance.DieResults[key][0])
               {
                  if (("Talk " == content) || ("Evade" == content) || ("Fight" == content))
                     b.IsEnabled = false;
                  return true;
               }
               break;
            case "e049": // travelling minstrel
               if (0 == myGameInstance.GetFoods())
                  b.IsEnabled = false;
               else
                  b.IsEnabled = true;
               break;
            case "e050":
               if (Utilities.NO_RESULT == myGameInstance.DieResults[key][1])
               {
                  if (("Talk " == content) || ("Evade" == content) || ("Fight" == content))
                     b.IsEnabled = false;
                  return true;
               }
               break;
            case "e052": // goblins
            case "e055": // orcs
               if ((Utilities.NO_RESULT == gi.DieResults[key][0]) && (("Escape" == content) || ("Follow" == content)))
               {
                  b.IsEnabled = false;
               }
               else
               {
                  b.IsEnabled = true;
                  b.Click += Button_Click;
               }
               return true;
            case "e058a":
               if ((false == gi.IsEvadeActive) && (("Escape" == content) || ("Follow" == content)))
               {
                  b.IsEnabled = false;
               }
               else
               {
                  b.IsEnabled = true;
                  b.Click += Button_Click;
               }
               break;
            case "e069":
               int maxLoad = 0;
               foreach (IMapItem mi in gi.PartyMembers)
                  maxLoad += mi.GetMaxFreeLoad();
               if (maxLoad < Utilities.PersonBurden)
               {
                  if ("Carry" == content)
                     b.IsEnabled = false;
               }
               break;
            case "e071":
               if (Utilities.NO_RESULT == myGameInstance.DieResults[key][0])
               {
                  if (("Talk " == content) || ("Evade" == content) || ("Fight" == content))
                     b.IsEnabled = false;
                  return true;
               }
               break;
            case "e079b":
               bool isAnyPartyMemberMounted = false;
               foreach (IMapItem mi in myGameInstance.PartyMembers)
               {
                  if ((true == mi.IsRiding) && (false == mi.IsFlyer()))
                     isAnyPartyMemberMounted = true;
               }
               if (false == isAnyPartyMemberMounted)
               {
                  if ("Dismount" == content)
                     b.IsEnabled = false;
                  return true;
               }
               break;
            case "e081":
               if (Utilities.NO_RESULT == myGameInstance.DieResults[key][0])
               {
                  if (("Talk " == content) || ("Evade" == content) || ("Fight" == content))
                     b.IsEnabled = false;
                  return true;
               }
               break;
            case "e098": // dragon
               if ((Utilities.NO_RESULT == gi.DieResults[key][0]) && (("Evade" == content) || ("Fight" == content)))
                  b.IsEnabled = false;
               else
                  b.IsEnabled = true;
               break;
            case "e107": // falcon
               if ((0 == gi.GetFoods()) && ("Feed" == content))
                  b.IsEnabled = false;
               else
                  b.IsEnabled = true;
               break;
            case "e112":
               if (Utilities.NO_RESULT == myGameInstance.DieResults[key][0])
               {
                  if ((" Fly " == content) || ("Evade" == content) || ("Fight" == content))
                     b.IsEnabled = false;
                  return true;
               }
               else // fly button only enabled if no horses and no non-human/elf party members
               {
                  bool isNonHumanElfInParty = false;
                  bool isNonPegasusMount = false;
                  foreach (IMapItem mi in myGameInstance.PartyMembers)
                  {
                     if (true == mi.Name.Contains("Dwarf") || true == mi.Name.Contains("Halfling"))
                        isNonHumanElfInParty = true;
                     foreach (IMapItem mount in mi.Mounts)
                     {
                        if ((true == mount.Name.Contains("Horse")) || (true == mount.Name.Contains("Griffon")) || (true == mount.Name.Contains("Harpy")))
                           isNonPegasusMount = true;
                     }
                  }
                  if ((" Fly " == content) && ((true == isNonHumanElfInParty) || (true == isNonPegasusMount) || (false == gi.PartyReadyToFly()))) // if unable to fly, do not show button
                     b.IsEnabled = false;
                  else
                     b.Click += Button_Click;
                  return true;
               }
            case "e128": // Merchant 
               b.IsEnabled = false;
               if (" Go " == content)
               {
                  if (3 == gi.DieResults["e128"][0])
                     b.IsEnabled = true;
               }
               else if ("Skip" == content)
               {
                  if (3 == gi.DieResults["e128"][0])
                     b.IsEnabled = true;
               }
               else if ("Ignore" == content) // do not enable Ignore button if user rolled dice
               {
                  if (0 < gi.DieResults["e128"][0])
                     b.IsEnabled = false;
                  else
                     b.IsEnabled = true;
               }
               else
               {
                  b.IsEnabled = true;
               }
               break;
            case "e128b": // Merchant Selling Potion
               if ("  +  " == content)
               {
                  cost = 10;
                  if (true == gi.IsMerchantWithParty)
                     cost = (int)Math.Ceiling((double)cost * 0.5);
                  if (cost <= gi.GetCoins())
                     b.IsEnabled = true;
                  else
                     b.IsEnabled = false;
               }
               else if ("  -  " == content)
               {
                  if (0 < gi.PurchasedPotionCure)
                     b.IsEnabled = true;
                  else
                     b.IsEnabled = false;
               }
               break;
            case "e128c": // Merchant Selling Food
               if ("  +  " == content)
               {
                  if ((0 < gi.GetCoins()) && (gi.PurchasedFood < 8))
                     b.IsEnabled = true;
                  else
                     b.IsEnabled = false;
               }
               else if ("  -  " == content)
               {
                  if (0 < gi.PurchasedFood)
                     b.IsEnabled = true;
                  else
                     b.IsEnabled = false;
               }
               break;
            case "e128e": // Merchant Selling Healing Potions
               if ("  +  " == content)
               {
                  cost = 5;
                  if (true == gi.IsMerchantWithParty)
                     cost = (int)Math.Ceiling((double)cost * 0.5);
                  if (cost <= gi.GetCoins()) // can only sell two horses
                     b.IsEnabled = true;
                  else
                     b.IsEnabled = false;
               }
               else if ("  -  " == content)
               {
                  if (0 < gi.PurchasedPotionHeal)
                     b.IsEnabled = true;
                  else
                     b.IsEnabled = false;
               }
               break;
            case "e128f": // Merchant Selling Mounts
               if ("  +  " == content)
               {
                  cost = 6;
                  if (true == gi.IsMerchantWithParty)
                     cost = (int)Math.Ceiling((double)cost * 0.5);
                  if ((cost <= gi.GetCoins()) && (gi.PurchasedMount < 2)) // can only sell two horses
                     b.IsEnabled = true;
                  else
                     b.IsEnabled = false;
               }
               else if ("  -  " == content)
               {
                  if (0 < gi.PurchasedMount)
                     b.IsEnabled = true;
                  else
                     b.IsEnabled = false;
               }
               break;
            case "e129": // Merchant Caravan
               string contente129 = (string)b.Content;
               if ((Utilities.NO_RESULT < gi.DieResults["e129"][0]) && ("Pass by" == contente129))
                  b.IsEnabled = false;
               break;
            case "e129b": // Caravan Selling Healing Potions
               if ("  +  " == content)
               {
                  cost = 6;
                  if (true == gi.IsMerchantWithParty)
                     cost = (int)Math.Ceiling((double)cost * 0.5);
                  if (cost <= gi.GetCoins()) // can only sell two horses
                     b.IsEnabled = true;
                  else
                     b.IsEnabled = false;
               }
               else if ("  -  " == content)
               {
                  if (0 < gi.PurchasedPotionHeal)
                     b.IsEnabled = true;
                  else
                     b.IsEnabled = false;
               }
               break;
            case "e129c": // SetButtonState() - Caravan Selling Mounts
               if ("  +  " == content)
               {
                  cost = 7;
                  if (true == gi.IsMerchantWithParty)
                     cost = (int)Math.Ceiling((double)cost * 0.5);
                  if ((cost <= gi.GetCoins()) && (gi.PurchasedMount < 7)) // can only sell 6
                     b.IsEnabled = true;
                  else
                     b.IsEnabled = false;
               }
               else if ("  -  " == content)
               {
                  if (0 < gi.PurchasedMount)
                     b.IsEnabled = true;
                  else
                     b.IsEnabled = false;
               }
               break;
            case "e130":
               if (Utilities.NO_RESULT == myGameInstance.DieResults[key][1])
               {
                  if (("Talk " == content) || ("Evade" == content) || ("Fight" == content))
                  {
                     b.IsEnabled = false;
                     return true;
                  }
               }
               break;
            case "e144a": // Rescue True Heir of Huldra Castle
               if ((false == gi.IsSpecialItemHeld(SpecialEnum.MagicSword)) && (false == gi.IsSpecialItemHeld(SpecialEnum.ShieldOfLight)))
               {
                  if ("Impress" == content)
                     b.IsEnabled = false;
               }
               if ((false == gi.IsInMapItems("Magician")) && (false == gi.IsInMapItems("Priest")) && (false == gi.IsInMapItems("Wizard")))
               {
                  if (" Cast  " == content)
                     b.IsEnabled = false;
               }
               if (false == gi.IsSpecialItemHeld(SpecialEnum.CharismaTalisman))
               {
                  if (" Charm " == content)
                     b.IsEnabled = false;
               }
               if (false == gi.IsSpecialItemHeld(SpecialEnum.NerveGasBomb))
               {
                  if (" Kill  " == content)
                     b.IsEnabled = false;
               }
               break;
            case "e163a": // Merchant Selling Slaves
               if ("  +  " == content)
               {
                  if ("SlavePorterPlus" == b.Name)
                  {
                     cost = gi.DieResults["e163"][0];
                     if (true == gi.IsMerchantWithParty)
                        cost = (int)Math.Ceiling((double)cost * 0.5);
                     if (cost <= gi.GetCoins())
                        b.IsEnabled = true;
                     else
                        b.IsEnabled = false;
                  }
                  else if ("SlaveGirlPlus" == b.Name)
                  {
                     cost = gi.DieResults["e163"][1] + 2;
                     if (true == gi.IsMerchantWithParty)
                        cost = (int)Math.Ceiling((double)cost * 0.5);
                     if (cost <= gi.GetCoins())
                        b.IsEnabled = true;
                     else
                        b.IsEnabled = false;
                  }
                  else if ("SlaveWarriorPlus" == b.Name)
                  {
                     cost = gi.DieResults["e163"][2];
                     if (true == gi.IsMerchantWithParty)
                        cost = (int)Math.Ceiling((double)cost * 0.5);
                     if ((0 == gi.PurchasedSlavePorter) && (0 == gi.PurchasedSlaveGirl)) // if no slave or porter bought, the warrior cost 2gp more
                     {
                        if (true == gi.IsMerchantWithParty)
                           cost += 1;
                        else
                           cost += 2;
                     }
                     if ((cost <= gi.GetCoins()) && (0 == gi.PurchasedSlaveWarrior))
                        b.IsEnabled = true;
                     else
                        b.IsEnabled = false;
                  }
                  else
                  {
                     b.IsEnabled = false;
                     Logger.Log(LogEnum.LE_ERROR, "SetButtonState(): e163 plus button with b.Name=" + b.Name);
                  }
               }
               else if ("  -  " == content)
               {
                  if ("SlavePorterMinus" == b.Name)
                  {
                     if (0 < gi.PurchasedSlavePorter)
                        b.IsEnabled = true;
                     else
                        b.IsEnabled = false;
                  }
                  else if ("SlaveGirlMinus" == b.Name)
                  {
                     if (0 < gi.PurchasedSlaveGirl)
                        b.IsEnabled = true;
                     else
                        b.IsEnabled = false;
                  }
                  else if ("SlaveWarriorMinus" == b.Name)
                  {
                     if (0 < gi.PurchasedSlaveWarrior)
                        b.IsEnabled = true;
                     else
                        b.IsEnabled = false;
                  }
                  else
                  {
                     b.IsEnabled = false;
                     Logger.Log(LogEnum.LE_ERROR, "SetButtonState(): e163 minus button with b.Name=" + b.Name);
                  }
               }
               break;
            case "e203":
               if ((true == gi.IsSpecialItemHeld(SpecialEnum.Foulbane)) && ("r146a" == content) && ("0323" == gi.Prince.Territory.Name))
                  b.Visibility = Visibility.Visible;
               if ((true == gi.IsInMapItems("WarriorBoy") && ("r144f" == content) && ("1212" == gi.Prince.Territory.Name)))
                  b.Visibility = Visibility.Visible;
               if ((true == gi.IsInMapItems("WarriorBoy") && ("r144i" == content) && ("1212" == gi.Prince.Territory.Name)))
                  b.Visibility = Visibility.Visible;
               break;
            case "e204a":
               if (0 < myGameInstance.Prince.MovementUsed)
               {
                  if ("Short Hop" == content)
                     b.IsEnabled = false;
                  return true;
               }
               break;
            case "e210d": // Seek Hire - Horse Deal
               if ("  +  " == content)
               {
                  cost = 10;
                  if (true == gi.IsMerchantWithParty)
                     cost = (int)Math.Ceiling((double)cost * 0.5);
                  if (cost <= gi.GetCoins())
                     b.IsEnabled = true;
                  else
                     b.IsEnabled = false;
               }
               else if ("  -  " == content)
               {
                  if (0 < gi.PurchasedMount)
                     b.IsEnabled = true;
                  else
                     b.IsEnabled = false;
               }
               break;
            case "e210f": // Hire Henchmen
               if ("  +  " == content)
               {
                  if ("HenchmanPlus" == b.Name)
                  {
                     if ((gi.PurchasedHenchman < gi.DieResults["e210f"][0]) && (Utilities.NO_RESULT < gi.DieResults["e210f"][0]))
                        b.IsEnabled = true;
                     else
                        b.IsEnabled = false;
                  }
                  else
                  {
                     b.IsEnabled = false;
                     Logger.Log(LogEnum.LE_ERROR, "SetButtonState(): e210f plus button with b.Name=" + b.Name);
                  }
               }
               else if ("  -  " == content)
               {
                  if ("HenchmanMinus" == b.Name)
                  {
                     if ((0 < gi.PurchasedHenchman) && (Utilities.NO_RESULT < gi.DieResults["e210f"][0]))
                        b.IsEnabled = true;
                     else
                        b.IsEnabled = false;
                  }
                  else
                  {
                     b.IsEnabled = false;
                     Logger.Log(LogEnum.LE_ERROR, "SetButtonState(): e210f minus button with b.Name=" + b.Name);
                  }
               }
               break;
            case "e210g": // Seek Hire - Horse Sale
               if ("  +  " == content)
               {
                  cost = 7;
                  if (true == gi.IsMerchantWithParty)
                     cost = (int)Math.Ceiling((double)cost * 0.5);
                  if (cost <= gi.GetCoins()) // can only sell two horses
                     b.IsEnabled = true;
                  else
                     b.IsEnabled = false;
               }
               else if ("  -  " == content)
               {
                  if (0 < gi.PurchasedMount)
                     b.IsEnabled = true;
                  else
                     b.IsEnabled = false;
               }
               break;
            case "e210i": // Hire Porters
               if ("  +  " == content)
               {
                  if ("PorterPlus" == b.Name)
                  {
                     if (gi.PurchasedPorter < 26)
                        b.IsEnabled = true;
                     else
                        b.IsEnabled = false;
                  }
                  else if ("LocalGuidePlus" == b.Name)
                  {
                     if (0 == gi.PurchasedGuide)
                        b.IsEnabled = true;
                     else
                        b.IsEnabled = false;
                  }
                  else
                  {
                     b.IsEnabled = false;
                     Logger.Log(LogEnum.LE_ERROR, "SetButtonState(): e210i plus button with b.Name=" + b.Name);
                  }
               }
               else if ("  -  " == content)
               {
                  if ("PorterMinus" == b.Name)
                  {
                     if (0 < gi.PurchasedPorter)
                        b.IsEnabled = true;
                     else
                        b.IsEnabled = false;
                  }
                  else if ("LocalGuideMinus" == b.Name)
                  {
                     if (0 < gi.PurchasedGuide)
                        b.IsEnabled = true;
                     else
                        b.IsEnabled = false;
                  }
                  else
                  {
                     b.IsEnabled = false;
                     Logger.Log(LogEnum.LE_ERROR, "SetButtonState(): e210i minus button with b.Name=" + b.Name);
                  }
               }
               break;
            case "e211c":
               if (("Dismiss" == content) && ((false == myGameInstance.IsMagicInParty()) || (true == myGameInstance.IsMagicUserDismissed) || (0 < myGameInstance.DieResults["e211c"][0])))
                  b.IsEnabled = false;
               if ((true == gi.IsInMapItems("WarriorBoy") && ("r144f" == content) && ("1212" == gi.Prince.Territory.Name)))
                  b.Visibility = Visibility.Visible;
               if ((true == gi.IsInMapItems("WarriorBoy") && ("r144i" == content) && ("1212" == gi.Prince.Territory.Name)))
                  b.Visibility = Visibility.Visible;
               break;
            case "e211d":
               if ((true == gi.IsSpecialItemHeld(SpecialEnum.Foulbane)) && ("r146a" == content))
                  b.Visibility = Visibility.Visible;
               break;
            case "e211g":
               if (("Continue" == content) && (0 < myGameInstance.DieResults["e211g"][0]))
                  b.IsEnabled = false;
               if (("Dismiss" == content) && ((false == myGameInstance.IsMagicInParty()) || (true == myGameInstance.IsMagicUserDismissed) || (0 < myGameInstance.DieResults["e211g"][0])))
                  b.IsEnabled = false;
               break;
            case "e212": // offering at temple
               if ("Spend" == content)
               {
                  cost = 10;
                  if (true == gi.IsMerchantWithParty)
                     cost = (int)Math.Ceiling((double)cost * 0.5);
                  if (cost <= gi.GetCoins()) 
                     b.IsEnabled = true;
                  else
                     b.IsEnabled = false;
               }
               break;
            default:
               break;
         }
         if ((false == myGameInstance.IsEvadeActive) && ("Evade" == content)) // Evade option not available when IsEvadeActive set to false
         {
            b.IsEnabled = false;
         }
         else if ((false == myGameInstance.IsElfTalkActive) && ("Talk " == content)) // Evade option not available when IsEvadeActive set to false
         {
            b.IsEnabled = false;
         }
         else
         {
            if ((true == gi.IsPartyFlying()) && ("Fly" == content))   // Show fly button if able to fly 
               b.Visibility = Visibility.Visible;
            else if ((true == gi.Prince.IsRiding) && ("Abandon" == content) && (false == gi.IsPartyRiding())) // Show abandon button if riding and not everybody in party has horses
               b.Visibility = Visibility.Visible;
            b.Click += Button_Click;
         }
         return true;
      }
      private void AppendAtEnd(IGameInstance gi, string key)
      {
         Logger.Log(LogEnum.LE_VIEW_APPEND_EVENT, "AppendAtEnd(): k=" + key + " d0=" + gi.DieResults[key][0].ToString() + " d1=" + gi.DieResults[key][1].ToString() + " d2=" + gi.DieResults[key][2].ToString());
         int cost = 0;
         int modifiedWitAndWile = 0;
         switch (key)
         {
            case "e001a":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click anywhere to continue."));
               }
               break;
            case "e002a":
               AppendEscapeMethods(gi, false); // false = cannot make riding escape
               myTextBlock.Inlines.Add(new LineBreak());
               if (true == gi.Prince.IsRiding)
               {
                  myTextBlock.Inlines.Add(new Run("Since you are riding, you can attempt the evade option and add one to the die roll."));
                  if (false == gi.IsPartyRiding())
                  {
                     Button b1 = new Button() { Content = "Abandon", Name = "Abandon", FontFamily = myFontFam1, FontSize = 12 };
                     b1.Click += Button_Click;
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new InlineUIContainer(b1));
                     myTextBlock.Inlines.Add(new Run(" party members without a mount or are not riding"));
                  }
               }
               break;
            case "e002b":
               ReplaceTextForLuckyCharm(gi);  // e002b
               if ((true == myGameInstance.IsGiftCharmActive) || (true == myGameInstance.IsSlaveGirlActive))
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Roll three times and choose one result."));
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e002c":
               if (true == gi.IsPartyRiding())
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Add one to die since party is riding."));
               }
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e002d":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e003": // swordsman has a fast horse
               AppendEscapeMethods(gi, false); // riding escape not possible
               break;
            case "e004": // Mercenaries
               AppendEscapeMethods(gi, false); // riding escape not possible
               foreach (IMapItem mi in gi.PartyMembers) // if there is at least one mount in party, add one to evade
               {
                  if ((0 < mi.Mounts.Count) || (true == mi.IsFlyingMountCarrier()))
                  {
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("Add one to die since party since have at least one mount."));
                     break;
                  }
               }
               break;
            case "e005": // Amazon with no horses
            case "e006": // Dwarf Warrior
               if (true == gi.IsEvadeActive)
                  AppendEscapeMethods(gi, true);
               break;
            case "e003a":
            case "e004a":
            case "e005a":
               ReplaceTextForLuckyCharm(gi);  //e003a, e004a, e005a
               if ((true == myGameInstance.IsGiftCharmActive) || (true == myGameInstance.IsSlaveGirlActive))
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Roll three times and choose one result."));
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e004b":
               foreach (IMapItem mi in gi.PartyMembers) // if there is at least one mount in party, add one to evade
               {
                  if ((0 < mi.Mounts.Count) || (true == mi.IsFlyingMountCarrier()))
                  {
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("Add 1 since there is a mount in party"));
                     break;
                  }
               }
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e003b":
            case "e005b":
            case "e003c":
            case "e004c":
            case "e005c":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e006a": // Dwarf Warrior
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e006c": // Dwarf Warrior
               ReplaceTextForLuckyCharm(gi); // e006c - checks gi.DieResults["e006a"][0]
               if (1 == gi.DieResults["e006a"][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Add one since dwarf is alone."));
               }
               if ((true == myGameInstance.IsGiftCharmActive) || (true == myGameInstance.IsSlaveGirlActive))
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Roll three times and choose one result."));
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e006d": // Dwarf Warrior
            case "e006e": // Dwarf Warrior
               if (1 == gi.DieResults["e006a"][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Add one since dwarf is alone."));
               }
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e006f":
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][1])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e006g":
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e007": // Elf Warrior
               if (("Forest" != gi.Prince.Territory.Type) && (true == gi.IsEvadeActive))
                  AppendEscapeMethods(gi, true);
               break;
            case "e007a": // Elf Warrior
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e007c": // Elf Warrior
               ReplaceTextForLuckyCharm(gi); // e007c
               if ("Forest" == myGameInstance.Prince.Territory.Type)
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Add two since elf is in forest."));
               }
               if ((true == myGameInstance.IsGiftCharmActive) || (true == myGameInstance.IsSlaveGirlActive))
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Roll three times and choose one result."));
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e007d": // Elf Warrior
            case "e007e": // Elf Warrior
               if ("Forest" == myGameInstance.Prince.Territory.Type)
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Add two since elf is in forest."));
               }
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e008a": // Halfling Warrior
               ReplaceTextForLuckyCharm(gi); // e008a
               if ((true == myGameInstance.IsGiftCharmActive) || (true == myGameInstance.IsSlaveGirlActive))
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Roll three times and choose one result."));
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e008b":
            case "e009a":
            case "e009b":
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e010a":
               if (5 <= gi.GetFoods())
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click to give 5 food:"));
                  myTextBlock.Inlines.Add(new LineBreak());
                  Image imge010a = new Image { Source = MapItem.theMapImages.GetBitmapImage("Food"), Width = 75, Height = 75, Name = "FoodGive" };
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge010a));
               }
               break;
            case "e011a":
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               if (0 == gi.GetCoins())
                  myTextBlock.Inlines.Add(new Run("You have no money. Click image to continue."));
               else if (true == gi.IsMerchantWithParty)
                  myTextBlock.Inlines.Add(new Run("Each gp buys 8 food units. Click image to continue."));
               else
                  myTextBlock.Inlines.Add(new Run("Each gp buys 4 food units. Click image to continue."));
               break;
            case "e011d":
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e012a":
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               if (0 == gi.GetCoins())
                  myTextBlock.Inlines.Add(new Run("You have no money. Click image to continue."));
               else if (true == gi.IsMerchantWithParty)
                  myTextBlock.Inlines.Add(new Run("Each gp buys 4 food units. Click image to continue."));
               else
                  myTextBlock.Inlines.Add(new Run("Each gp buys 2 food units. Click image to continue."));
               break;
            case "e014a":
            case "e014c":
            case "e015a":
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e015b":
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               if (0 == gi.GetCoins())
                  myTextBlock.Inlines.Add(new Run("You have no money. Click image to continue."));
               else if (true == gi.IsMerchantWithParty)
                  myTextBlock.Inlines.Add(new Run("Each gp buys 4 food units. Horses cost 3gp. Click image to continue."));
               else
                  myTextBlock.Inlines.Add(new Run("Each gp buys 2 food units. Horses cost 6gp. Click image to continue."));
               break;
            case "e015c":
            case "e016b":
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e016c":
               if (true == myGameInstance.IsSpecialItemHeld(SpecialEnum.ResistanceTalisman))
               {
                  Image imge016c = new Image { Source = MapItem.theMapImages.GetBitmapImage("TalismanResistance"), Name = "TalismanActivate", Width = 75, Height = 75 };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge016c));
                  myTextBlock.Inlines.Add(new Run(" Click talisman to active."));
               }
               break;
            case "e016d":
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e017": // Peasant Mob
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e018a":
            case "e019a":
               ReplaceTextForLuckyCharm(gi); // e018a, e019a
               if ((true == myGameInstance.IsGiftCharmActive) || (true == myGameInstance.IsSlaveGirlActive))
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Roll three times and choose one result."));
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e018b":
            case "e019b":
            case "e019c":
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e020":
               if (4 < gi.DieResults[key][0])
               {
                  Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("MonkTraveling"), Width = 87, Height = 250 };
                  myTextBlock.Inlines.Add(new InlineUIContainer(img1));
               }
               break;
            case "e020a":
               if (4 < gi.DieResults["e020"][0])
               {
                  Image img1 = new Image { Name="E020EncounterTalk", Source = MapItem.theMapImages.GetBitmapImage("MonkTraveling"), Width = 87, Height = 250 };
                  myTextBlock.Inlines.Add(new InlineUIContainer(img1));
               }
               ReplaceTextForLuckyCharm(gi); // e020a
               if ((true == myGameInstance.IsGiftCharmActive) || (true == myGameInstance.IsSlaveGirlActive))
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Roll three times and choose one result."));
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e020b":
               if (4 < gi.DieResults["e020"][0])
               {
                  Image img1 = new Image { Name="E020EncounterEvade", Source = MapItem.theMapImages.GetBitmapImage("MonkTraveling"), Width = 87, Height = 250 };
                  myTextBlock.Inlines.Add(new InlineUIContainer(img1));
               }
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e020c":
               if (4 < gi.DieResults["e020"][0])
               {
                  Image img1 = new Image { Name="E020EncounterFight", Source = MapItem.theMapImages.GetBitmapImage("MonkTraveling"), Width = 87, Height = 250 };
                  myTextBlock.Inlines.Add(new InlineUIContainer(img1));
               }
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e021":
               if (Utilities.NO_RESULT < gi.DieResults[key][1])
               {
                  Image imgE021 = new Image { Source = MapItem.theMapImages.GetBitmapImage("MonkWarrior"), Width = 175, Height = 200 };
                  AppendEscapeMethods(gi, true);
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                  "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgE021));
               }
               break;
            case "e021a":
            case "e023a":
               ReplaceTextForLuckyCharm(gi); // e023a
               if ((true == myGameInstance.IsGiftCharmActive) || (true == myGameInstance.IsSlaveGirlActive))
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Roll three times and choose one result."));
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e021b":
            case "e021c":
            case "e023b":
            case "e023c":
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e024":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               else
               {
                  modifiedWitAndWile = gi.WitAndWile;
                  myTextBlock.Inlines.Add(new Run(" < " + modifiedWitAndWile.ToString()));
                  myTextBlock.Inlines.Add(new Run(" for party to escape."));
               }
               break;
            case "e024a":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               else
               {
                  modifiedWitAndWile = gi.WitAndWile + 1;
                  myTextBlock.Inlines.Add(new Run(" < " + modifiedWitAndWile.ToString()));
                  myTextBlock.Inlines.Add(new Run(" for Prince to escape."));
               }
               break;
            case "e025":
            case "e025b":
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][1])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e026":
               myTextBlock.Inlines.Add(new LineBreak());
               Image imge026 = null;
               if ( true == myGameInstance.WizardAdviceLocations.Contains(myGameInstance.Prince.Territory) )
                  imge026 = new Image { Source = MapItem.theMapImages.GetBitmapImage("WizardAdvice"), Width = 200, Height = 200, Name = "SearchAdviceEnd" };
               else
                  imge026 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Pixie"), Width = 200, Height = 200, Name = "SearchAdviceEnd" };
               myTextBlock.Inlines.Add(new Run("                           "));
               myTextBlock.Inlines.Add(new InlineUIContainer(imge026));
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e028a":
            case "e029":
            case "e032":
            case "e032a":
            case "e033":
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e035a":
               int dieNeeded = gi.WanderingDayCount + 1;
               myTextBlock.Inlines.Add(new Run(" < " + dieNeeded.ToString()));
               break;
            case "e036a":
            case "e037":
            case "e038":
            case "e041":
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e048":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image e0481 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Ally"), Width = 75, Height = 75, Name = "FugitiveAlly" };
                  Image e0482 = new Image { Source = MapItem.theMapImages.GetBitmapImage("CrossedSwords"), Width = 75, Height = 75, Name = "FugitiveFight" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new InlineUIContainer(e0481));
                  myTextBlock.Inlines.Add(new Run(" Click to get ally."));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new InlineUIContainer(e0482));
                  myTextBlock.Inlines.Add(new Run(" Click to fight."));
               }
               break;
            case "e048a":
            case "e048b":
            case "e048e":
            case "e048f":
            case "e048g":
            case "e048h":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e050":
               if (Utilities.NO_RESULT < gi.DieResults[key][1])
               {
                  Image imgE050 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Constabulary"), Width = 250, Height = 250 };
                  AppendEscapeMethods(gi, true);
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                       "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgE050));
               }
               break;
            case "e050b":
               ReplaceTextForLuckyCharm(gi); // e050b
               if (true == myGameInstance.VisitedLocations.Contains(myGameInstance.Prince.Territory))
               {
                  if (true == myGameInstance.EscapedLocations.Contains(myGameInstance.Prince.Territory))
                  {
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("Since you escaped for this previously visited location, you cannot add two because you are undoubtedly a wanted man."));
                  }
                  else
                  {
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("Since you have visited this location without escaping, you can add two to the die."));
                  }
               }
               else
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Since you have never visited this location and are unknown, add one to the die roll."));
               }
               if ((true == myGameInstance.IsGiftCharmActive) || (true == myGameInstance.IsSlaveGirlActive))
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Roll three times and choose one result."));
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e050c":
            case "e050d":
               if (true == myGameInstance.VisitedLocations.Contains(myGameInstance.Prince.Territory))
               {
                  if (true == myGameInstance.EscapedLocations.Contains(myGameInstance.Prince.Territory))
                  {
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("Since you escaped for this previously visited location, you cannot add two because you are undoubtedly a wanted man."));
                  }
                  else
                  {
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("Since you have visited this location without escaping, you can add two to the die."));
                  }
               }
               else
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Since you have never visited this location and are unknown, add one to the die roll."));
               }
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e052a":
               Image img52 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Goblin2"), Width = 170, Height = 250 };
               modifiedWitAndWile = gi.WitAndWile + 1;
               myTextBlock.Inlines.Add(new Run(" < " + modifiedWitAndWile.ToString()));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("                                      "));
               myTextBlock.Inlines.Add(new InlineUIContainer(img52));
               break;
            case "e053":
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click anywhere to continue."));
               }
               break;
            case "e053b":
               modifiedWitAndWile = gi.WitAndWile + 1;
               myTextBlock.Inlines.Add(new Run(" < " + modifiedWitAndWile.ToString()));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               if ("e052" == gi.EventStart)
               {
                  Image img053b = new Image { Source = MapItem.theMapImages.GetBitmapImage("Goblin2"), Width = 53, Height = 80 };
                  myTextBlock.Inlines.Add(new InlineUIContainer(img053b));
               }
               else if ("e055" == gi.EventStart)
               {
                  Image img053b = new Image { Source = MapItem.theMapImages.GetBitmapImage("Orc"), Width = 53, Height = 80 };
                  myTextBlock.Inlines.Add(new InlineUIContainer(img053b));
               }
               else if ("e058a" == gi.EventStart)
               {
                  Image img053b = new Image { Source = MapItem.theMapImages.GetBitmapImage("Dwarfs"), Width = 80, Height = 80 };
                  myTextBlock.Inlines.Add(new InlineUIContainer(img053b));
               }
               else
               {
                  Logger.Log(LogEnum.LE_ERROR, "AppendAtEnd(): reached default with unknown eventStart=" + myGameInstance.EventStart);
               }
               int end = 2 * gi.EncounteredMembers.Count;
               int monsterCount = Math.Min(Utilities.MAX_GRID_ROW - 8, end); // cannot grow over number of goblins that can be shown
               myTextBlock.Inlines.Add(new Run(" = " + monsterCount.ToString()));
               break;
            case "e054a":
               if (Utilities.NO_RESULT == gi.DieResults["e054a"][0])
               {
                  myTextBlock.Inlines.Add(new Run(" < " + gi.WitAndWile.ToString()));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
               }
               else
               {
                  if (gi.DieResults[key][0] < gi.WitAndWile)
                  {
                     Image img54a = new Image { Source = MapItem.theMapImages.GetBitmapImage("Nothing"), Width = 100, Height = 100, Name = "EncounterEnd" };
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("                                            "));
                     myTextBlock.Inlines.Add(new InlineUIContainer(img54a));
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("Escaped! Click image to continue."));
                  }
                  else
                  {
                     Image img54a0 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Goblin"), Width = 170, Height = 250 };
                     Image img54a1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Goblin2"), Width = 170, Height = 250 };
                     Image img54a2 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Goblin2"), Width = 170, Height = 250 };
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new InlineUIContainer(img54a0));
                     myTextBlock.Inlines.Add(new InlineUIContainer(img54a1));
                     myTextBlock.Inlines.Add(new InlineUIContainer(img54a2));
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run(" No Escape Allowed. Pick another option: "));
                     Button b1 = new Button() { Content = "Fight", Name = "e054b", FontFamily = myFontFam1, FontSize = 12 };
                     b1.Click += Button_Click;
                     myTextBlock.Inlines.Add(new InlineUIContainer(b1));
                     myTextBlock.Inlines.Add(new Run("  "));
                     Button b2 = new Button() { Content = "Surrender", Name = "e061", FontFamily = myFontFam1, FontSize = 12 };
                     b2.Click += Button_Click;
                     myTextBlock.Inlines.Add(new InlineUIContainer(b2));
                  }
               }
               break;
            case "e054b":
               if (Utilities.NO_RESULT < gi.DieResults["e054b"][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click anywhere to continue."));
               }
               break;
            case "e055a":
               Image img55 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Orc"), Width = 170, Height = 250 };
               modifiedWitAndWile = gi.WitAndWile + 1;
               myTextBlock.Inlines.Add(new Run(" < " + modifiedWitAndWile.ToString()));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("                                             "));
               myTextBlock.Inlines.Add(new InlineUIContainer(img55));
               break;
            case "e056a":
               if (Utilities.NO_RESULT < gi.DieResults["e056a"][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click anywhere to continue."));
               }
               break;
            case "e057":
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  Image img57a = new Image { Name="E057EncounterRoll", Source = MapItem.theMapImages.GetBitmapImage("Troll"), Width = 170, Height = 250 };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                      "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(img57a));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               else
               {
                  Image img57b = new Image { Source = MapItem.theMapImages.GetBitmapImage("Troll"), Width = 170, Height = 250 };
                  modifiedWitAndWile = gi.WitAndWile + 1;
                  myTextBlock.Inlines.Add(new Run(" < " + modifiedWitAndWile.ToString()));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                      "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(img57b));
               }
               break;
            case "e058":
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e058a":
            case "e058b":
               AppendEscapeMethods(gi, true); // e058a, e058b

               break;
            case "e058c":
               Image img58c = new Image { Source = MapItem.theMapImages.GetBitmapImage("Dwarfs"), Width = 300, Height = 300 };
               modifiedWitAndWile = gi.WitAndWile;
               myTextBlock.Inlines.Add(new Run(" < " + modifiedWitAndWile.ToString()));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("                                "));
               myTextBlock.Inlines.Add(new InlineUIContainer(img58c));
               break;
            case "e058e": // e058e
               ReplaceTextForLuckyCharm(gi); // e058e
               if ((true == myGameInstance.IsGiftCharmActive) || (true == myGameInstance.IsSlaveGirlActive))
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Roll three times and choose one result."));
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e058f":
            case "e058g":
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e059":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image imge059 = new Image { Source = MapItem.theMapImages.GetBitmapImage("DwarfMines"), Width = 300, Height = 150, Name = "EncounterRoll" };
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                 "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge059));
               }
               else
               {
                  modifiedWitAndWile = gi.WitAndWile;
                  myTextBlock.Inlines.Add(new Run(" < " + modifiedWitAndWile.ToString()));
               }
               break;
            case "e060":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image imge060 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Arrested"), Width = 175, Height = 200, Name = "JailArrested" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                          "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge060));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               else
               {
                  if (true == myGameInstance.IsTempleGuardModifer)
                  {
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("Subtract 1 due to hostile guards."));
                  }
               }
               break;
            case "e066":
            case "e066b":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image imge066 = new Image { Source = MapItem.theMapImages.GetBitmapImage("MonksWarrior"), Width = 175, Height = 200, Name = "MonksWarrior" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                               "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge066));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               else
               {
                  myTextBlock.Inlines.Add(new Run(" < " + gi.WitAndWile.ToString()));
               }
               break;
            case "e067":
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e068":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image imge068 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Nothing"), Width = 100, Height = 100, Name = "WizardAbode" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                            "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge068));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e068a":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image imge068a = new Image { Source = MapItem.theMapImages.GetBitmapImage("Magician"), Width = 150, Height = 300, Name = "MagicianHome" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                       "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge068a));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e071":
               bool isElfInParty = gi.IsInMapItems("Elf");
               bool isDwarfInParty = gi.IsInMapItems("Dwarf");
               if ((true == isElfInParty) && (true == isDwarfInParty))
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Subtract one for Elf in party, and add one for Dwarf in party."));
               }
               else if (true == isDwarfInParty)
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Add one for Dwarf in party."));
               }
               else if (true == isElfInParty)
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Subtract one for Elf in party."));
               }
               //----------------------------------------------
               if ((false == myGameInstance.IsElfTalkActive) && (Utilities.NO_RESULT < myGameInstance.DieResults[key][0]))
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Note:") { FontStyle = FontStyles.Italic });
                  myTextBlock.Inlines.Add(new Run(" Talk is inactive because selected to engage in "));
                  Button b1 = new Button() { Content = "e072", FontFamily = myFontFam1, FontSize = 12 };
                  b1.Click += Button_Click;
                  myTextBlock.Inlines.Add(new InlineUIContainer(b1));
                  myTextBlock.Inlines.Add(new Run("."));
               }
               break;
            case "e071a": // e071a
               ReplaceTextForLuckyCharm(gi); // e071a
               if ((true == myGameInstance.IsGiftCharmActive) || (true == myGameInstance.IsSlaveGirlActive))
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Roll three times and choose one result."));
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e071b":
            case "e071c":
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e071d":
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e072c":
               Image imge072c = new Image { Source = MapItem.theMapImages.GetBitmapImage("Elf"), Width = 65, Height = 80 };
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new InlineUIContainer(imge072c));
               myTextBlock.Inlines.Add(new Run(" = " + gi.EncounteredMembers.Count.ToString()));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Click anywhere to continue."));
               break;
            case "e073":
            case "e073c":
            case "e074":
            case "e075b":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e078":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e080a":
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e081":  // mounted patrol
               AppendEscapeMethods(gi, false); // riding escape not possible
               break;
            case "e081a": // e081a
               ReplaceTextForLuckyCharm(gi); // e081a
               if ((true == myGameInstance.IsGiftCharmActive) || (true == myGameInstance.IsSlaveGirlActive))
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Roll three times and choose one result."));
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e081b":
            case "e081c":
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e083a":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image imge083a = new Image { Source = MapItem.theMapImages.GetBitmapImage("BoarCooked"), Width = 400, Height = 250, Name = "BoarCooked" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge083a));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e086a":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image imgee086a = new Image { Source = MapItem.theMapImages.GetBitmapImage("SnowShoes"), Width = 200, Height = 200, Name = "HighPassRedistribute" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                      "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgee086a));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e091":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to find victim."));
               }
               break;
            case "e093":
            case "e094":
            case "e094a":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e098a":
            case "e098b":
            case "e099a":
            case "e099b":
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e100a": // e100a
            case "e101a": // e100a
               ReplaceTextForLuckyCharm(gi);
               if ((true == myGameInstance.IsGiftCharmActive) || (true == myGameInstance.IsSlaveGirlActive))
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Roll three times and choose one result."));
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e100b":
            case "e100c":
            case "e101b":
            case "e101c":
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e105a":  // storm clouds
               if (Utilities.NO_RESULT < gi.DieResults[key][2])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e105":  // storm clouds
            case "e106":  // overcast
            case "e108":  // hawkmen
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e110b":
            case "e111":
               if (Utilities.NO_RESULT < gi.DieResults[key][1])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e112a":
            case "e112b":
            case "e112c":
            case "e113":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e117":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e118a": // e118a
               ReplaceTextForLuckyCharm(gi);
               if ((true == myGameInstance.IsGiftCharmActive) || (true == myGameInstance.IsSlaveGirlActive))
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Roll three times and choose one result."));
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e118b":
            case "e118c":
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e122":
               if (0 < gi.GetCoins())
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click to pay 1gp to cross river:"));
                  myTextBlock.Inlines.Add(new LineBreak());
                  Image imge122 = new Image { Source = MapItem.theMapImages.GetBitmapImage("CoinsStacked"), Width = 75, Height = 75, Name = "RaftsmenCross" };
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge122));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click to pay 1gp to hire ending travel for today:"));
                  myTextBlock.Inlines.Add(new LineBreak());
                  Image imge1221 = new Image { Source = MapItem.theMapImages.GetBitmapImage("CoinsStacked"), Width = 75, Height = 75, Name = "RaftsmenHire" };
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge1221));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e124":
            case "e128":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e128a":
               Image imgE128a1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("CoinsStackedDeny"), Width = 100, Height = 100, Name = "BuyPegasusDeny" };
               cost = 50;
               if (true == gi.IsMerchantWithParty)
                  cost = (int)Math.Ceiling((double)cost * 0.5);
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               string costToPayS1 = "Click to not pay " + cost.ToString() + "gp:";
               myTextBlock.Inlines.Add(new Run(costToPayS1));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new InlineUIContainer(imgE128a1));
               if (cost <= gi.GetCoins())
               {
                  Image img209 = new Image { Source = MapItem.theMapImages.GetBitmapImage("CoinsStacked"), Width = 100, Height = 100, Name = "BuyPegasus" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  string costToPayS = "Click to pay " + cost.ToString() + "gp:";
                  myTextBlock.Inlines.Add(new Run(costToPayS1));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new InlineUIContainer(img209));
               }
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Click image to continue."));
               break;
            case "e128b":
               cost = 10;
               if (true == gi.IsMerchantWithParty)
                  cost = (int)Math.Ceiling((double)cost * 0.5);
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               if (cost <= gi.GetCoins())
               {
                  string costToPayS = "Amulet costs " + cost.ToString() + "gp. ";
                  myTextBlock.Inlines.Add(new Run(costToPayS));
               }
               myTextBlock.Inlines.Add(new Run("Click merchant image to stop buying."));
               break;
            case "e128c":
               double foodCost = 0.5;
               if (true == gi.IsMerchantWithParty)
                  foodCost = foodCost * 0.5;
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               if (1 < gi.GetCoins())
               {
                  string costToPayS = "Each food costs " + foodCost.ToString() + "gp. ";
                  myTextBlock.Inlines.Add(new Run(costToPayS));
               }
               myTextBlock.Inlines.Add(new Run("Click merchant image to stop buying."));
               break;
            case "e128d":
               Image e128d = new Image { Name="E128dEncounterRoll", Source = MapItem.theMapImages.GetBitmapImage("MerchantOutwit"), Width = 400, Height = 200 };
               modifiedWitAndWile = gi.WitAndWile + 1;
               myTextBlock.Inlines.Add(new Run(" < " + modifiedWitAndWile.ToString()));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("               "));
               myTextBlock.Inlines.Add(new InlineUIContainer(e128d));
               if( Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e128e":
               cost = 5;
               if (true == gi.IsMerchantWithParty)
                  cost = (int)Math.Ceiling((double)cost * 0.5);
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               string costToPayPotionS = "Each potion costs " + cost.ToString() + "gp. ";
               myTextBlock.Inlines.Add(new Run(costToPayPotionS));
               myTextBlock.Inlines.Add(new Run("Click merchant image to stop buying."));
               break;
            case "e128f":
               cost = 6;
               if (true == gi.IsMerchantWithParty)
                  cost = (int)Math.Ceiling((double)cost * 0.5);
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               string costToPayHorseS = "Each horse costs " + cost.ToString() + "gp. ";
               myTextBlock.Inlines.Add(new Run(costToPayHorseS));
               myTextBlock.Inlines.Add(new Run("Click merchant image to stop buying."));
               break;
            case "e129":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e129a":
               Image imgE129a1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("CoinsStackedDeny"), Width = 100, Height = 100, Name = "BuyAmuletDeny" };
               cost = 25;
               if (true == gi.IsMerchantWithParty)
                  cost = (int)Math.Ceiling((double)cost * 0.5);
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               string costToPayS2 = "Click to not pay " + cost.ToString() + "gp:";
               myTextBlock.Inlines.Add(new Run(costToPayS2));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new InlineUIContainer(imgE129a1));
               if (cost <= gi.GetCoins())
               {
                  Image imgE129a = new Image { Source = MapItem.theMapImages.GetBitmapImage("CoinsStacked"), Width = 100, Height = 100, Name = "BuyAmulet" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  string costToPayS = "Click to pay " + cost.ToString() + "gp:";
                  myTextBlock.Inlines.Add(new Run(costToPayS2));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgE129a));
               }
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Click image to continue."));
               break;
            case "e129b":
               cost = 6;
               if (true == gi.IsMerchantWithParty)
                  cost = (int)Math.Ceiling((double)cost * 0.5);
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               if (cost <= gi.GetCoins())
               {
                  string costToPayS = "Each potion costs " + cost.ToString() + "gp. ";
                  myTextBlock.Inlines.Add(new Run(costToPayS));
               }
               myTextBlock.Inlines.Add(new Run("Click merchant image to stop buying."));
               break;
            case "e129c": // AppendAtEnd() - Add text at end of user instructions
               cost = 7;
               if (true == gi.IsMerchantWithParty)
                  cost = (int)Math.Ceiling((double)cost * 0.5);
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               string costToPayH1S = "Each horse costs " + cost.ToString() + "gp. ";
               myTextBlock.Inlines.Add(new Run(costToPayH1S));
               myTextBlock.Inlines.Add(new Run("Click merchant image to stop buying."));
               break;
            case "e130a":
               ReplaceTextForLuckyCharm(gi); // e130a
               if ((true == myGameInstance.IsGiftCharmActive) || (true == myGameInstance.IsSlaveGirlActive))
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Roll three times and choose one result."));
               }
               else if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e130b":
            case "e130c":
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e130f":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e130g":
               cost = 10;
               if (true == gi.IsMerchantWithParty)
                  cost = (int)Math.Ceiling((double)cost * 0.5);
               if (cost <= gi.GetCoins())
               {
                  Image imge130ga = new Image { Source = MapItem.theMapImages.GetBitmapImage("CoinsStacked"), Width = 100, Height = 100, Name = "GuardBribe" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  string costToPayS = "Click to pay " + cost.ToString() + "gp:";
                  myTextBlock.Inlines.Add(new Run(costToPayS));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge130ga));
               }
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Click image to continue."));
               break;
            case "e135":
            case "e136":
            case "e137":
            case "e138":
            case "e139":
            case "e140":
            case "e140b":
            case "e141":
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e143a":
               if (0 < gi.ChagaDrugCount)
               {
                  Image imgE143a = new Image { Source = MapItem.theMapImages.GetBitmapImage("DrugChaga"), Width = 100, Height = 100, Name = "ChagaDrugPay" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click to give Chaga drug:"));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgE143a));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e147":
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][1])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e147a":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image imgE147 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Secrets"), Width = 250, Height = 220, Name = "Chest" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                     "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgE147));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e148":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image e148a = new Image { Source = MapItem.theMapImages.GetBitmapImage("CoinsStackedDeny"), Width = 75, Height = 75, Name = "BribeToSeneschalDeny" };
                  Image e148b = new Image { Source = MapItem.theMapImages.GetBitmapImage("CoinsStacked"), Width = 75, Height = 75, Name = "BribeToSeneschalPay" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click to not pay: "));
                  myTextBlock.Inlines.Add(new LineBreak());

                  myTextBlock.Inlines.Add(new InlineUIContainer(e148a));
                  if (gi.Bribe <= gi.GetCoins())
                  {
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     string costToPayS = "Click to pay " + gi.Bribe.ToString() + "gp:";
                     myTextBlock.Inlines.Add(new Run(costToPayS));
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new InlineUIContainer(e148b));
                  }
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e151":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e153":
               cost = 10;
               if (true == gi.IsMerchantWithParty)
                  cost = (int)Math.Ceiling((double)cost * 0.5);
               if (cost <= gi.GetCoins())
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  string costToPayS = "Click to pay " + cost.ToString() + "gp:";
                  myTextBlock.Inlines.Add(new Run(costToPayS));
                  myTextBlock.Inlines.Add(new LineBreak());
                  Image img209 = new Image { Source = MapItem.theMapImages.GetBitmapImage("CoinsStacked"), Width = 75, Height = 75, Name = "MasterOfHouseholdPay" };
                  myTextBlock.Inlines.Add(new InlineUIContainer(img209));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e154":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image imgE154 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Nothing"), Width = 100, Height = 100, Name = "AudienceDaughter" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                       "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgE154));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e154a":
            case "e154b":
            case "e154c":
            case "e154d":
               ITerritory t154x = gi.Prince.Territory;
               Image imgE154x = null;
               if (true == gi.HalflingTowns.Contains(t154x))
                  imgE154x = new Image { Source = MapItem.theMapImages.GetBitmapImage("HalflingDaughter"), Width = 200, Height = 250, Name = "LordsDaughter" };
               else if (true == gi.ElfTowns.Contains(t154x))
                  imgE154x = new Image { Source = MapItem.theMapImages.GetBitmapImage("ElfDaughter"), Width = 200, Height = 250, Name = "LordsDaughter" };
               else if (true == gi.IsInTown(t154x))
                  imgE154x = new Image { Source = MapItem.theMapImages.GetBitmapImage("MayorDaughter"), Width = 200, Height = 250, Name = "LordsDaughter" };
               else if (true == gi.IsInTemple(t154x))
                  imgE154x = new Image { Source = MapItem.theMapImages.GetBitmapImage("PriestDaughter"), Width = 200, Height = 250, Name = "LordsDaughter" };
               else if (true == gi.DwarvenMines.Contains(t154x))
                  imgE154x = new Image { Source = MapItem.theMapImages.GetBitmapImage("DwarfPrincess"), Width = 200, Height = 250, Name = "LordsDaughter" };
               else
                  imgE154x = new Image { Source = MapItem.theMapImages.GetBitmapImage("LordDaughter1"), Width = 200, Height = 250, Name = "LordsDaughter" };
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("                                  "));
               myTextBlock.Inlines.Add(new InlineUIContainer(imgE154x));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Click image to continue."));
               break;
            case "e154e":
               ITerritory t154e = gi.Prince.Territory;
               Image imgE154e = null;
               if (true == gi.HalflingTowns.Contains(t154e))
                  imgE154e = new Image { Source = MapItem.theMapImages.GetBitmapImage("HalflingDaughter"), Width = 200, Height = 250, Name = "LordsDaughterLove" };
               else if (true == gi.ElfTowns.Contains(t154e))
                  imgE154e = new Image { Source = MapItem.theMapImages.GetBitmapImage("ElfDaughter"), Width = 200, Height = 250, Name = "LordsDaughterLove" };
               else if (true == gi.IsInTown(t154e))
                  imgE154e = new Image { Source = MapItem.theMapImages.GetBitmapImage("MayorDaughter"), Width = 200, Height = 250, Name = "LordsDaughterLove" };
               else if (true == gi.IsInTemple(t154e))
                  imgE154e = new Image { Source = MapItem.theMapImages.GetBitmapImage("PriestDaughter"), Width = 200, Height = 250, Name = "LordsDaughterLove" };
               else if (true == gi.DwarvenMines.Contains(t154e))
                  imgE154e = new Image { Source = MapItem.theMapImages.GetBitmapImage("DwarfPrincess"), Width = 200, Height = 250, Name = "LordsDaughterLove" };
               else
                  imgE154e = new Image { Source = MapItem.theMapImages.GetBitmapImage("LordDaughter1"), Width = 200, Height = 250, Name = "LordsDaughterLove" };
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("                                  "));
               myTextBlock.Inlines.Add(new InlineUIContainer(imgE154e));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Click image to continue."));
               break;
            case "e155":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image imgE155 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Nothing"), Width = 100, Height = 100, Name = "AudienceHighPriest" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                            "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgE155));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e156":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image imgE156 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Nothing"), Width = 100, Height = 100, Name = "AudienceMayor" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                            "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgE156));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e156a":
            case "e156b":
            case "e156c":
            case "e156d":
            case "e156f":
               ITerritory t156x = gi.Prince.Territory;
               Image imgE156x = null;
               if (true == gi.HalflingTowns.Contains(t156x))
                  imgE156x = new Image { Source = MapItem.theMapImages.GetBitmapImage("MayorHalfling"), Width = 200, Height = 230, Name = "Mayor" };
               else if (true == gi.ElfTowns.Contains(t156x))
                  imgE156x = new Image { Source = MapItem.theMapImages.GetBitmapImage("MayorElf"), Width = 200, Height = 230, Name = "Mayor" };
               else if (true == gi.IsInTown(t156x))
                  imgE156x = new Image { Source = MapItem.theMapImages.GetBitmapImage("Mayor"), Width = 200, Height = 230, Name = "Mayor" };
               else
                  imgE156x = new Image { Source = MapItem.theMapImages.GetBitmapImage("Mayor"), Width = 200, Height = 230, Name = "Mayor" };
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("                                     "));
               myTextBlock.Inlines.Add(new InlineUIContainer(imgE156x));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Click image to continue."));
               break;
            case "e157":
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Letter of Recommendation for " + ConvertToStructureName(myGameInstance.TargetHex)));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("                                     "));
               Image imgE157 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Letter"), Width = 250, Height = 160, Name = "Letter" };
               myTextBlock.Inlines.Add(new InlineUIContainer(imgE157));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Click image to continue."));
               break;
            case "e158":
               cost = 20;
               if (true == gi.IsMerchantWithParty)
                  cost = (int)Math.Ceiling((double)cost * 0.5);
               Logger.Log(LogEnum.LE_BRIBE, "AppendAtEnd(): bribe=" + gi.Bribe.ToString());
               if (cost <= gi.Prince.Coin)
               {
                  Image imgE158 = new Image { Source = MapItem.theMapImages.GetBitmapImage("CoinsStacked"), Width = 75, Height = 75, Name = "HostileGuardsPay" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  string costToPayS = "Click to pay " + cost.ToString() + "gp:";
                  myTextBlock.Inlines.Add(new Run(costToPayS));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgE158));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e160":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image imgE160 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Nothing"), Width = 100, Height = 100, Name = "AudienceLadyAeravir" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                            "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgE160));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e160e":
            case "e160f":
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e160g":
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Roll two times and choose one result."));
               if (Utilities.NO_RESULT < gi.DieResults[key][1])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click one button to continue."));
               }
               break;
            case "e161":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  if ((true == myGameInstance.IsSpecialItemHeld(SpecialEnum.Foulbane)) && (false == myGameInstance.IsFoulBaneUsedThisTurn)) // e146 - if have foul bane
                  {
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run(" Using the Foulbane, you can choose to: "));
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     Button b161a1 = new Button() { Name = "AcceptRoll", Content = "Accept Roll", FontFamily = myFontFam1, FontSize = 12 };
                     b161a1.Click += Button_Click;
                     myTextBlock.Inlines.Add(new InlineUIContainer(b161a1));
                     myTextBlock.Inlines.Add(new Run(" --or-- "));
                     Button b161a2 = new Button() { Name = "RollAgain", Content = "Roll Again", FontFamily = myFontFam1, FontSize = 12 };
                     b161a2.Click += Button_Click;
                     myTextBlock.Inlines.Add(new InlineUIContainer(b161a2));
                     myTextBlock.Inlines.Add(new Run(" to override first roll."));
                     myTextBlock.Inlines.Add(new LineBreak());
                  }
                  else
                  {
                     Image imgE161a = new Image { Source = MapItem.theMapImages.GetBitmapImage("Nothing"), Width = 100, Height = 100, Name = "AudienceCountDrogat" };
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("                                            "));
                     myTextBlock.Inlines.Add(new InlineUIContainer(imgE161a));
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("Click image to continue."));
                  }
               }
               else
               {
                  if (true == gi.IsSpecialItemHeld(SpecialEnum.TrollSkin))
                  {
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("Add 1 for troll skin."));
                  }
               }
               break;
            case "e162":
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e163":
               if (Utilities.NO_RESULT < gi.DieResults[key][2])
               {
                  Image imgE163 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Coffle"), Width = 400, Height = 200, Name = "SlaveMarketStart" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                 "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgE163));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to go to purchases."));
               }
               break;
            case "e163a":
               int slavePorterCosts = gi.DieResults["e163"][0];
               if (true == gi.IsMerchantWithParty)
                  slavePorterCosts = (int)Math.Ceiling((double)slavePorterCosts * 0.5);
               ReplaceText("SLAVEPORTERCOSTS", slavePorterCosts.ToString());
               int slaveGirlCosts = gi.DieResults["e163"][1] + 2;
               if (true == gi.IsMerchantWithParty)
                  slaveGirlCosts = (int)Math.Ceiling((double)slaveGirlCosts * 0.5);
               ReplaceText("SLAVEGIRLCOSTS", slaveGirlCosts.ToString());
               int slaveWarriorCosts = gi.DieResults["e163"][2];
               if (true == gi.IsMerchantWithParty)
                  slaveWarriorCosts = (int)Math.Ceiling((double)slaveWarriorCosts * 0.5);
               if ((0 == gi.PurchasedSlaveGirl) && (0 == gi.PurchasedSlavePorter))
               {
                  if (true == gi.IsMerchantWithParty)
                     slaveWarriorCosts += 1;
                  else
                     slaveWarriorCosts += 2;
               }
               ReplaceText("SLAVEWARRIORCOSTS", slaveWarriorCosts.ToString());
               break;
            case "e163c":
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("                         "));
               string imageSource = "SlaveGirlFace" + gi.SlaveGirlIndex.ToString();
               Image e163c = new Image { Source = MapItem.theMapImages.GetBitmapImage(imageSource), Width = 300, Height = 300, Name = "SlaveGirlCheck" };
               myTextBlock.Inlines.Add(new InlineUIContainer(e163c));
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e163d":
               if (Utilities.NO_RESULT < gi.DieResults[key][1])
               {
                  Image img209 = new Image { Source = MapItem.theMapImages.GetBitmapImage("OldMan"), Width = 200, Height = 300, Name = "SlaveMarketEnd" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(img209));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to to continue."));
               }
               break;
            case "e165":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image imge165 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Elf"), Width = 175, Height = 200, Name = "EncounterRoll" };
                  myTextBlock.Inlines.Add(new Run(" Click image to to continue."));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                               "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge165));
               }
               else
               {
                  modifiedWitAndWile = gi.WitAndWile + 1;
                  myTextBlock.Inlines.Add(new Run(" < " + modifiedWitAndWile.ToString()));
                  if (true == gi.IsInMapItems("Elf"))
                  {
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("Subtract one for Elf in party."));
                  }
                  else if (true == gi.IsMagicInParty())
                  {
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("Subtract one for magic user in party."));
                  }
                  if (true == gi.IsInMapItems("Dwarf"))
                  {
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("Add one for Dwarf in party."));
                  }
               }
               break;
            case "e166":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image imge166 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Elf"), Width = 175, Height = 200, Name = "EncounterRoll" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                               "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge166));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to to continue."));
               }
               else
               {
                  modifiedWitAndWile = gi.WitAndWile;
                  myTextBlock.Inlines.Add(new Run(" < " + modifiedWitAndWile.ToString()));
                  if (true == gi.IsInMapItems("Elf"))
                  {
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("Subtract one for Elf in party."));
                  }
                  else if (true == gi.IsMagicInParty())
                  {
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("Subtract one for magic user in party."));
                  }
                  if (true == gi.IsInMapItems("Dwarf"))
                  {
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("Add one for Dwarf in party."));
                  }
               }
               break;
            case "e203a":
               if (1 == gi.DieResults["e203a"][0])
               {
                  Image e203a0 = new Image { Source = MapItem.theMapImages.GetBitmapImage("JailBreak"), Name = "Jail", Width = 250, Height = 250 };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                   "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(e203a0));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Prison Escape! On next screen, roll for who escapes with you. Click image to continue."));
               }
               else
               {
                  if ((6 == gi.DieResults["e203a"][0]) && ("e061" == myGameInstance.EventStart))
                  {
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("Your time is up. Click anywhere to continue."));
                  }
                  else if (Utilities.NO_RESULT < gi.DieResults["e203a"][0])
                  {
                     Image e203a1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Jail"), Name = "Jail", Width = 250, Height = 250 };
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("                                   "));
                     myTextBlock.Inlines.Add(new InlineUIContainer(e203a1));
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run(" Another night in jail. Click image to continue."));
                  }
               }
               break;
            case "e203c":
               switch (gi.DieResults["e203c"][0])
               {
                  case 2:
                  case 3:
                     Image imgE203c = new Image { Source = MapItem.theMapImages.GetBitmapImage("DungeonJailBreak"), Name = "JailDungeon", Width = 400, Height = 280 };
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("                    "));
                     myTextBlock.Inlines.Add(new InlineUIContainer(imgE203c));
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("Dungeon Escape! Click image to continue."));
                     break;
                  case 4:
                  case 5:
                  case 6:
                  case 7:
                  case 8:
                  case 9:
                  case 10:
                  case 11:
                  case 12:
                     Image e203a1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("DungeonJail"), Name = "JailDungeon", Width = 400, Height = 280 };
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("Nights In Dungeon = " + gi.NightsInDungeon.ToString()));
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("                    "));
                     myTextBlock.Inlines.Add(new InlineUIContainer(e203a1));
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run(" Another night in the dungeon. Click image to continue."));
                     break;
                  default:
                     myTextBlock.Inlines.Add(new Run(" < 4"));
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     if (0 < gi.NightsInDungeon)
                        myTextBlock.Inlines.Add(new Run("Nights In Dungeon = " + gi.NightsInDungeon.ToString()));
                     else
                        myTextBlock.Inlines.Add(new LineBreak());
                     break;
               }
               break;
            case "e203e":
               if (Utilities.NO_RESULT == gi.DieResults["e203e"][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  if (0 < gi.WanderingDayCount)
                     myTextBlock.Inlines.Add(new Run("Days a Slave = " + gi.WanderingDayCount.ToString()));
                  else
                     myTextBlock.Inlines.Add(new LineBreak());
               }
               else if (6 == gi.DieResults["e203e"][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("             "));
                  Image imgE203e = new Image { Source = MapItem.theMapImages.GetBitmapImage("WizardEscape"), Name = "WizardWander", Width = 400, Height = 200 };
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgE203e));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Wizard Escape! Click image to continue."));
               }
               else
               {
                  Image e203a1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("WizardWander"), Name = "WizardWander", Width = 160, Height = 225 };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                          "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(e203a1));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Days a Slave = " + gi.WanderingDayCount.ToString()));
                  myTextBlock.Inlines.Add(new Run(".  Another night as a slave. Click image to continue."));
               }
               break;
            case "e203f":
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               if (true == gi.IsJailed)
               {
                  Image e203a1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Jail"), Name = "ReturnToJail", Width = 250, Height = 250 };
                  myTextBlock.Inlines.Add(new Run("                                   "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(e203a1));
               }
               else if (true == gi.IsDungeon)
               {
                  Image e203a1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("DungeonJail"), Name = "ReturnToJail", Width = 400, Height = 280 };
                  myTextBlock.Inlines.Add(new Run("                    "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(e203a1));
               }
               else if (true == gi.IsEnslaved)
               {
                  Image e203a1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("WizardWander"), Name = "ReturnToJail", Width = 160, Height = 225 };
                  myTextBlock.Inlines.Add(new Run("                                          "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(e203a1));
               }
               else
               {
                  Logger.Log(LogEnum.LE_ERROR, "AppendTextAtEnd(): reached default with ae=" + myGameInstance.EventActive);
               }
               break;
            case "e204u":
            case "e204m":
            case "e204s":
            case "e204a":
               if (0 < myGameInstance.Prince.MovementUsed)
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Alternatively, click the Prince to end movement for today without additional events."));
               }
               break;
            case "e205c":
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e208": // search ruins
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e209": // Seek News
               if (5 <= gi.GetCoins())
               {
                  Image img209 = new Image { Source = MapItem.theMapImages.GetBitmapImage("CoinsStacked"), Width = 100, Height = 100, Name = "SeekNewsWithPay" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click to pay 5gp:"));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new InlineUIContainer(img209));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               myTextBlock.Inlines.Add(new LineBreak());
               bool isHalfingTown = gi.HalflingTowns.Contains(gi.Prince.Territory);
               bool isKilledLocation = gi.KilledLocations.Contains(gi.Prince.Territory);
               if (4 < gi.WitAndWile)
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Add one since wit and wiles greater than 4."));
               }
               if (true == gi.HalflingTowns.Contains(gi.Prince.Territory))
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  if (true == gi.KilledLocations.Contains(gi.Prince.Territory))
                     myTextBlock.Inlines.Add(new Run("Subtract one since killed in this halfing town."));
                  else if (true == gi.EscapedLocations.Contains(gi.Prince.Territory))
                     myTextBlock.Inlines.Add(new Run("Subtract one since escaped from this halfing town."));
                  else
                     myTextBlock.Inlines.Add(new Run("Add one since in halfling town."));
               }
               break;
            case "e209a":
               myTextBlock.Inlines.Add(new LineBreak());
               if (4 < gi.WitAndWile)
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Add one since wit and wiles > 4"));
               }
               if (true == gi.IsSeekNewModifier)
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Add one since paid for rumors."));
               }
               if (true == gi.FeelAtHomes.Contains(gi.Prince.Territory))
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Add one since feel at home."));
               }
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Nothing"), Width = 100, Height = 100, Name = "SeekNewsNext" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                            "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(img1));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "209h":
               if (Utilities.NO_RESULT < myGameInstance.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e209g": // Buy valuable information
               cost = 10;
               if (true == gi.IsMerchantWithParty)
                  cost = (int)Math.Ceiling((double)cost * 0.5);
               if (cost <= gi.GetCoins())
               {
                  Image imge209g = new Image { Source = MapItem.theMapImages.GetBitmapImage("CoinsStacked"), Width = 75, Height = 75, Name = "BuyInfo" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  string costToPayS = "Click to pay " + cost.ToString() + "gp:";
                  myTextBlock.Inlines.Add(new Run(costToPayS));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge209g));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e210": // Hire Followers
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image imgE210 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Nothing"), Width = 100, Height = 100, Name = "SeekHireNext" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                            "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgE210));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               else
               {
                  if (true == gi.HalflingTowns.Contains(gi.Prince.Territory))
                  {
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     if (true == gi.KilledLocations.Contains(gi.Prince.Territory))
                        myTextBlock.Inlines.Add(new Run("Subtract one since killed in this halfing town."));
                     else if (true == gi.EscapedLocations.Contains(gi.Prince.Territory))
                        myTextBlock.Inlines.Add(new Run("Subtract one since escaped from this halfing town."));
                  }
               }
               break;
            case "e210d":
               int horseCosts = 10;
               if (true == gi.IsMerchantWithParty)
                  horseCosts = (int)Math.Ceiling((double)horseCosts * 0.5);
               ReplaceText("HORSECOSTS", horseCosts.ToString());
               break;
            case "e210f":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Nothing"), Width = 100, Height = 100, Name = "HireHenchmanEnd" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                            "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(img1));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e210g":
               int horseCosts1 = 7;
               if (true == gi.IsMerchantWithParty)
                  horseCosts1 = (int)Math.Ceiling((double)horseCosts1 * 0.5);
               ReplaceText("HORSECOSTS", horseCosts1.ToString());
               break;
            case "e211a": // See audience in town
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image imgE210 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Nothing"), Width = 100, Height = 100, Name = "SeekAudience" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                            "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgE210));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               else
               {
                  int daughterRollModifier = myGameInstance.DaughterRollModifier;
                  if (0 < daughterRollModifier)
                  {
                     StringBuilder sb = new StringBuilder();
                     sb.Append("Add ");
                     sb.Append(daughterRollModifier.ToString());
                     sb.Append(" for daughter's influence.");
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run(sb.ToString()));
                  }
                  if (true == gi.HalflingTowns.Contains(gi.Prince.Territory))
                  {
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     if (true == gi.KilledLocations.Contains(gi.Prince.Territory))
                        myTextBlock.Inlines.Add(new Run("Subtract one since killed in this halfing town."));
                     else if (true == gi.EscapedLocations.Contains(gi.Prince.Territory))
                        myTextBlock.Inlines.Add(new Run("Subtract one since escaped from this halfing town."));
                  }
               }
               break;
            case "e211b":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image imgE211 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Nothing"), Width = 100, Height = 100, Name = "SeekAudience" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                            "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgE211));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               else
               {
                  ITerritory princeTerritory = myGameInstance.Prince.Territory;
                  int letterModifier = 0;
                  foreach (ITerritory t in myGameInstance.LetterOfRecommendations)
                  {
                     if (t.Name == princeTerritory.Name)
                        letterModifier += 2;
                  }
                  if ((0 < letterModifier) || (true == myGameInstance.Purifications.Contains(princeTerritory)) || (true == myGameInstance.IsChagaDrugProvided))
                  {
                     StringBuilder sb = new StringBuilder();
                     if (2 == letterModifier)
                     {
                        sb.Append("Add ");
                        sb.Append(letterModifier.ToString());
                        sb.Append(" for letter of recommendation.");
                     }
                     else if (2 < letterModifier)
                     {
                        sb.Append("Add ");
                        sb.Append(letterModifier.ToString());
                        sb.Append(" for letters of recommendation.");
                     }
                     if (true == myGameInstance.Purifications.Contains(princeTerritory))
                        sb.Append(" Add 2 for purifications.");
                     if (true == myGameInstance.IsChagaDrugProvided)
                        sb.Append(" Add 1 for Chaga.");
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run(sb.ToString()));
                  }
               }
               break;
            case "e211c": // Seeking audence with Baron of Huldra Castle
            case "e211d": // Seeking audience with Baron of Count Drogat
            case "e211e": // Seeking audience with Lady Aeravir
            case "e211f": // Seeking audience with Dwarven King
            case "e211g": // Seeking audence with Baron of Huldra Castle with Real Heir in Party
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image imgE211 = null;
                  if ("e211f" == key)
                  {
                     imgE211 = new Image { Source = MapItem.theMapImages.GetBitmapImage("DwarfKing"), Width = 132, Height = 200, Name = "SeekAudience" };
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("                                            "));
                     myTextBlock.Inlines.Add(new InlineUIContainer(imgE211));
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("Click image to continue."));
                  }
                  else if ("e211g" == key)
                  {
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("Click image to continue."));
                  }
                  else
                  {
                     imgE211 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Nothing"), Width = 100, Height = 100, Name = "SeekAudience" };
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("                                            "));
                     myTextBlock.Inlines.Add(new InlineUIContainer(imgE211));
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("Click image to continue."));
                  }
               }
               else
               {

                  ITerritory princeTerritory = myGameInstance.Prince.Territory;
                  int letterModifier = 0;
                  foreach (ITerritory t in myGameInstance.LetterOfRecommendations)
                  {
                     if (t.Name == princeTerritory.Name)
                        letterModifier += 2;
                  }
                  bool isPurified = myGameInstance.Purifications.Contains(princeTerritory);
                  int purifyModifier = (isPurified ? 2 : 0);
                  int audienceRollModifier = myGameInstance.DaughterRollModifier + myGameInstance.SeneschalRollModifier + letterModifier + purifyModifier;
                  if ("e211e" == key) // Seeking audencie with Lady Aeravir
                  {
                     if (true == gi.IsSecretLadyAeravir)
                        audienceRollModifier++;
                  }
                  else if ("e211c" == key)
                  {
                     if (true == gi.IsMagicUserDismissed)
                        audienceRollModifier++;
                  }
                  else if ("e211d" == key) // Seeking audencie with Count Drogat
                  {
                     if (true == gi.IsSpecialItemHeld(SpecialEnum.Foulbane))
                        audienceRollModifier++;
                     if (true == gi.Prince.IsResurrected)
                        audienceRollModifier++;
                  }
                  else if ("e211g" == key)
                  {
                     if (true == gi.IsMagicUserDismissed)
                        audienceRollModifier++;
                  }
                  //------------------------------------------
                  if (0 < audienceRollModifier)
                  {
                     StringBuilder sb = new StringBuilder();
                     if (2 == letterModifier)
                     {
                        sb.Append("Add ");
                        sb.Append(letterModifier.ToString());
                        sb.Append(" for letter of recommendation.");
                     }
                     else if (2 < letterModifier)
                     {
                        sb.Append("Add ");
                        sb.Append(letterModifier.ToString());
                        sb.Append(" for letters of recommendation.");
                     }
                     if (0 < myGameInstance.DaughterRollModifier)
                     {
                        sb.Append(" Add ");
                        sb.Append(myGameInstance.DaughterRollModifier.ToString());
                        sb.Append(" for daughter's influence.");
                     }
                     if (0 < myGameInstance.SeneschalRollModifier)
                     {
                        sb.Append(" Add ");
                        sb.Append(myGameInstance.SeneschalRollModifier.ToString());
                        sb.Append(" for Seneschal's influence.");
                     }
                     if (0 < purifyModifier)
                     {
                        sb.Append(" Add ");
                        sb.Append(purifyModifier.ToString());
                        sb.Append(" for Purification.");
                     }
                     if ("e211c" == key)
                     {
                        if (true == gi.IsMagicUserDismissed)
                           sb.Append(" Add 1 for dismissing magic user to work for Baron Huldra.");
                     }
                     if ("e211e" == key) //  Seeking audencie with Lady Aeravir
                     {
                        if (true == gi.IsSecretLadyAeravir)
                           sb.Append(" Add 1 for knowing Lady Aeravir's promiscuity.");
                     }
                     if ("e211d" == key) // Seeking audencie with Count Drogat
                     {
                        if (true == gi.IsSpecialItemHeld(SpecialEnum.Foulbane))
                           sb.Append(" Add 1 for holding foulbane.");
                        if (true == gi.Prince.IsResurrected)
                           sb.Append(" Add 1 for ghostly skin glow due to resurrection.");
                     }
                     if ("e211g" == key)
                     {
                        if (true == gi.IsMagicUserDismissed)
                           sb.Append(" Add 1 for dismissing magic user to work for Baron Huldra.");
                     }
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run(sb.ToString()));
                  }
               }
               break;
            case "e212":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  Image imgE212 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Nothing"), Width = 100, Height = 100, Name = "AudienceOffering" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                            "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(imgE212));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               else
               {
                  //-----------------------------------------------------
                  ITerritory princeTerritory = myGameInstance.Prince.Territory;
                  bool isOfferingConstraint = myGameInstance.ForbiddenAudiences.IsOfferingsConstraint(princeTerritory, myGameInstance.Days); // modifier only applies if seeking offering one day later
                  bool isOmenModifier = false;
                  if ((true == myGameInstance.IsOmenModifier) && ((myGameInstance.Days - myGameInstance.DayOfLastOffering) < 2))
                     isOmenModifier = true;
                  bool isInfluenceModifier = false;
                  if ((true == myGameInstance.IsInfluenceModifier) && ((myGameInstance.Days - myGameInstance.DayOfLastOffering) < 2))
                     isInfluenceModifier = true;
                  int letterModifier = 0;
                  foreach (ITerritory t in myGameInstance.LetterOfRecommendations)
                  {
                     if (t.Name == princeTerritory.Name)
                        letterModifier += 2;
                  }
                  bool isSecretRite = gi.SecretRites.Contains(princeTerritory);
                  //----------------------------------
                  if (false == myGameInstance.IsOfferingModifier) // ask if want to spend 10 gold to add one to die roll
                  {
                     cost = 10;
                     cost = (int)Math.Ceiling((double)cost * 0.5);
                     if (cost <= gi.GetCoins())
                     {
                        myTextBlock.Inlines.Add(new LineBreak());
                        myTextBlock.Inlines.Add(new Run("If you "));
                        Button b1 = new Button() { Content = "Spend", FontFamily = myFontFam1, FontSize = 12 };
                        b1.Click += Button_Click;
                        myTextBlock.Inlines.Add(new InlineUIContainer(b1));
                        if (true == myGameInstance.IsMerchantWithParty)
                           myTextBlock.Inlines.Add(new Run(" 5gp, add one to your die result."));
                        else
                           myTextBlock.Inlines.Add(new Run(" 10gp, add one to your die result."));
                     }
                  }
                  else
                  {
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("Add 1 for exceptional herbs and sacrifices."));
                  }
                  if (2 == letterModifier)
                  {
                     myTextBlock.Inlines.Add(new LineBreak());
                     StringBuilder sb = new StringBuilder();
                     sb.Append("Add ");
                     sb.Append(letterModifier.ToString());
                     sb.Append(" for letter of recommendation.");
                     myTextBlock.Inlines.Add(new Run(sb.ToString()));
                  }
                  else if (2 < letterModifier)
                  {
                     myTextBlock.Inlines.Add(new LineBreak());
                     StringBuilder sb = new StringBuilder();
                     sb.Append("Add ");
                     sb.Append(letterModifier.ToString());
                     sb.Append(" for letters of recommendation.");
                     myTextBlock.Inlines.Add(new Run(sb.ToString()));
                  }
                  if (true == myGameInstance.IsChagaDrugProvided)
                  {
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("Add 1 for Chaga."));
                  }
                  if ((true == isOfferingConstraint) || (true == isInfluenceModifier))
                  {
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("Add 3 for priest blessing."));
                  }
                  if (true == isOmenModifier)
                  {
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("Add 1 for omens from yesterday offering."));
                  }
                  if (true == isSecretRite)
                  {
                     myTextBlock.Inlines.Add(new LineBreak());
                     myTextBlock.Inlines.Add(new Run("Add 1 for knowing secret rites."));
                  }
               }
               break;
            case "e212k":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e213a":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                         "));
                  Image imge213a = null;
                  if (12 == gi.DieResults[key][0])
                     imge213a = new Image { Source = MapItem.theMapImages.GetBitmapImage("RaftDeny"), Width = 200, Height = 200, Name = "RaftingEndsForDay" };
                  else
                     imge213a = new Image { Source = MapItem.theMapImages.GetBitmapImage("Raft"), Width = 200, Height = 200, Name = "RaftingEndsForDay" };
                  myTextBlock.Inlines.Add(new InlineUIContainer(imge213a));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e214":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e314":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               else
               {
                  modifiedWitAndWile = gi.WitAndWile + 1;
                  myTextBlock.Inlines.Add(new Run(" < " + modifiedWitAndWile.ToString()));
                  if (true == gi.IsSpecialItemHeld(SpecialEnum.CharismaTalisman))
                  {
                     Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("TalismanCharismaSmall"), Width = 21, Height = 21, VerticalAlignment = VerticalAlignment.Bottom };
                     gi.IsCharismaTalismanActive = true;
                     myTextBlock.Inlines.Add(new Run(" + "));
                     myTextBlock.Inlines.Add(new InlineUIContainer(img1));
                  }
                  if (true == gi.IsElfWitAndWileActive)
                  {
                     Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("ElfWarriorSmall"), Width = theSmallElfImageWidth, Height = theSmallElfImageHeight, VerticalAlignment = VerticalAlignment.Bottom };
                     myTextBlock.Inlines.Add(new Run(" - "));
                     myTextBlock.Inlines.Add(new InlineUIContainer(img1));
                  }
               }
               break;
            case "e315":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               else
               {
                  myTextBlock.Inlines.Add(new Run(" < " + gi.WitAndWile.ToString()));
                  if (true == gi.IsSpecialItemHeld(SpecialEnum.CharismaTalisman))
                  {
                     Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("ElfWarriorSmall"), Width = theSmallElfImageWidth, Height = theSmallElfImageHeight, VerticalAlignment = VerticalAlignment.Bottom };
                     gi.IsCharismaTalismanActive = true;
                     myTextBlock.Inlines.Add(new Run(" + "));
                     myTextBlock.Inlines.Add(new InlineUIContainer(img1));
                  }
                  if (true == gi.IsElfWitAndWileActive)
                  {
                     Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("ElfWarriorSmall"), Width = theSmallElfImageWidth, Height = theSmallElfImageHeight, VerticalAlignment = VerticalAlignment.Bottom };
                     myTextBlock.Inlines.Add(new Run(" - "));
                     myTextBlock.Inlines.Add(new InlineUIContainer(img1));
                  }
               }
               break;
            case "e317":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               else
               {
                  modifiedWitAndWile = gi.WitAndWile + 1;
                  myTextBlock.Inlines.Add(new Run(" < " + modifiedWitAndWile.ToString()));
                  if (true == gi.IsElfWitAndWileActive)
                  {
                     Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("ElfWarriorSmall"), Width = theSmallElfImageWidth, Height = theSmallElfImageHeight, VerticalAlignment = VerticalAlignment.Bottom };
                     myTextBlock.Inlines.Add(new Run(" - "));
                     myTextBlock.Inlines.Add(new InlineUIContainer(img1));
                  }
               }
               break;
            case "e318":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               else
               {
                  myTextBlock.Inlines.Add(new Run(" < " + gi.WitAndWile.ToString()));
                  if (true == gi.IsElfWitAndWileActive)
                  {
                     Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("ElfWarriorSmall"), Width = theSmallElfImageWidth, Height = theSmallElfImageHeight, VerticalAlignment = VerticalAlignment.Bottom };
                     myTextBlock.Inlines.Add(new Run(" - "));
                     myTextBlock.Inlines.Add(new InlineUIContainer(img1));
                  }
               }
               break;
            case "e319":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               else
               {
                  int partySize = gi.PartyMembers.Count - 1;
                  myTextBlock.Inlines.Add(new Run(" > " + partySize.ToString()));
               }
               break;
            case "e320":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               else
               {
                  int partySize1 = gi.PartyMembers.Count;
                  myTextBlock.Inlines.Add(new Run(" > " + partySize1.ToString()));
               }
               break;
            case "e321":
            case "e322":
            case "e323":
            case "e324":
               Image swordsImg = new Image { Source = MapItem.theMapImages.GetBitmapImage("CrossedSwords"), Width = 150, Height = 150, VerticalAlignment = VerticalAlignment.Bottom };
               if (gi.Bribe <= gi.GetCoins())
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  Button b = new Button() { Content = "Bribe", FontFamily = myFontFam1, FontSize = 12 };
                  b.Click += Button_Click;
                  myTextBlock.Inlines.Add(new InlineUIContainer(b));
                  myTextBlock.Inlines.Add(new Run(" to pay encountered or click anywhere else to fight."));
               }
               else
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Since you do not have enough money, you will need to fight. Click anywhere to continue."));
               }
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("                                        "));
               myTextBlock.Inlines.Add(new InlineUIContainer(swordsImg));
               break;
            case "e326":
            case "e327":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               else
               {
                  modifiedWitAndWile = gi.WitAndWile + gi.MonkPleadModifier + 1;
                  myTextBlock.Inlines.Add(new Run(" < " + modifiedWitAndWile.ToString()));
                  if (true == gi.IsSpecialItemHeld(SpecialEnum.CharismaTalisman))
                  {
                     Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("TalismanCharismaSmall"), Width = 21, Height = 21, VerticalAlignment = VerticalAlignment.Bottom };
                     gi.IsCharismaTalismanActive = true;
                     myTextBlock.Inlines.Add(new Run(" + "));
                     myTextBlock.Inlines.Add(new InlineUIContainer(img1));
                  }
                  if (true == gi.IsElfWitAndWileActive)
                  {
                     Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("ElfWarriorSmall"), Width = theSmallElfImageWidth, Height = theSmallElfImageHeight, VerticalAlignment = VerticalAlignment.Bottom };
                     myTextBlock.Inlines.Add(new Run(" - "));
                     myTextBlock.Inlines.Add(new InlineUIContainer(img1));
                  }
               }
               break;
            case "e328":
            case "e329":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               else
               {
                  modifiedWitAndWile = gi.WitAndWile + gi.MonkPleadModifier;
                  myTextBlock.Inlines.Add(new Run(" < " + modifiedWitAndWile.ToString()));
                  if (true == gi.IsSpecialItemHeld(SpecialEnum.CharismaTalisman))
                  {
                     Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("TalismanCharismaSmall"), Width = 21, Height = 21, VerticalAlignment = VerticalAlignment.Bottom };
                     gi.IsCharismaTalismanActive = true;
                     myTextBlock.Inlines.Add(new Run(" + "));
                     myTextBlock.Inlines.Add(new InlineUIContainer(img1));
                  }
                  if (true == gi.IsElfWitAndWileActive)
                  {
                     Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("ElfWarriorSmall"), Width = theSmallElfImageWidth, Height = theSmallElfImageHeight, VerticalAlignment = VerticalAlignment.Bottom };
                     myTextBlock.Inlines.Add(new Run(" - "));
                     myTextBlock.Inlines.Add(new InlineUIContainer(img1));
                  }
               }
               break;
            case "e331":
               if (gi.Bribe <= gi.GetCoins())
               {
                  Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("CoinBar"), Width = 75, Height = 75, Name = "BribeToJoinPay" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  string costToPayS = "Click to pay " + gi.Bribe.ToString() + "gp:";
                  myTextBlock.Inlines.Add(new Run(costToPayS));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new InlineUIContainer(img1));
               }
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Click image to continue."));
               break;
            case "e331a":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e331b":
               if ((true == myGameInstance.IsMinstrelPlaying) || (true == myGameInstance.IsMinstrelInParty()))
               {
                  Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("c60Minstrel"), Width = 75, Height = 75, Name = "MinstrelStart" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click to soothe with a song keeping them happy without paying: "));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new InlineUIContainer(img1));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e332":
               if (gi.Bribe <= gi.GetCoins())
               {
                  Image img332 = new Image { Source = MapItem.theMapImages.GetBitmapImage("CoinsStacked"), Width = 75, Height = 75, Name = "BribeToHirePay" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  string costToPayS = "Click to pay " + gi.Bribe.ToString() + "gp:";
                  myTextBlock.Inlines.Add(new Run(costToPayS));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new InlineUIContainer(img332));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e332a":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e333":
               if (1 < gi.GetCoins())
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click to pay: "));
                  myTextBlock.Inlines.Add(new LineBreak());
                  Image img333 = new Image { Source = MapItem.theMapImages.GetBitmapImage("CoinsStacked"), Width = 75, Height = 75, Name = "HirelingsPay" };
                  myTextBlock.Inlines.Add(new InlineUIContainer(img333));
               }
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Click image to continue."));
               break;
            case "e333a":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e333b":
               int numButtons = Math.Min(gi.GetCoins(), gi.EncounteredMembers.Count);
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               for (int i = 0; i < numButtons; ++i)
               {
                  if (0 == i % 5)
                     myTextBlock.Inlines.Add(new LineBreak());
                  int j = i + 1;
                  string content = " " + j.ToString() + "  ";
                  if (j < 10)
                     content = "  " + j.ToString() + "  ";
                  Button b = new Button() { Content = content, FontFamily = myFontFam1, FontSize = 12, Name = "HirelingCount", VerticalAlignment = VerticalAlignment.Bottom };
                  b.Click += Button_Click;
                  myTextBlock.Inlines.Add(new InlineUIContainer(b));
                  myTextBlock.Inlines.Add(new Run("   "));
               }
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("                                  "));
               Image img333b = new Image { Source = MapItem.theMapImages.GetBitmapImage("Muscle"), Width = 200, Height = 200 };
               myTextBlock.Inlines.Add(new InlineUIContainer(img333b));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Click image to continue."));
               break;
            case "e336":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               else
               {
                  modifiedWitAndWile = gi.WitAndWile + gi.MonkPleadModifier + 1;
                  myTextBlock.Inlines.Add(new Run(" < " + modifiedWitAndWile.ToString()));
                  if (true == gi.IsSpecialItemHeld(SpecialEnum.CharismaTalisman))
                  {
                     Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("TalismanCharismaSmall"), Width = 21, Height = 21, VerticalAlignment = VerticalAlignment.Bottom };
                     gi.IsCharismaTalismanActive = true;
                     myTextBlock.Inlines.Add(new Run(" + "));
                     myTextBlock.Inlines.Add(new InlineUIContainer(img1));
                  }
                  if (true == gi.IsElfWitAndWileActive)
                  {
                     Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("ElfWarriorSmall"), Width = theSmallElfImageWidth, Height = theSmallElfImageHeight, VerticalAlignment = VerticalAlignment.Bottom };
                     myTextBlock.Inlines.Add(new Run(" - "));
                     myTextBlock.Inlines.Add(new InlineUIContainer(img1));
                  }
               }
               break;
            case "e337":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               else
               {
                  modifiedWitAndWile = gi.WitAndWile + gi.MonkPleadModifier;
                  myTextBlock.Inlines.Add(new Run(" < " + modifiedWitAndWile.ToString()));
                  if (true == gi.IsSpecialItemHeld(SpecialEnum.CharismaTalisman))
                  {
                     Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("TalismanCharismaSmall"), Width = 21, Height = 21, VerticalAlignment = VerticalAlignment.Bottom };
                     gi.IsCharismaTalismanActive = true;
                     myTextBlock.Inlines.Add(new Run(" + "));
                     myTextBlock.Inlines.Add(new InlineUIContainer(img1));
                  }
                  if (true == gi.IsElfWitAndWileActive)
                  {
                     Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("ElfWarriorSmall"), Width = theSmallElfImageWidth, Height = theSmallElfImageHeight, VerticalAlignment = VerticalAlignment.Bottom };
                     myTextBlock.Inlines.Add(new Run(" - "));
                     myTextBlock.Inlines.Add(new InlineUIContainer(img1));
                  }
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run(" "));
               }
               break;
            case "e337a":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e338":
               int minCoinsNeeded = 1;
               if ("e005" == myGameInstance.EventStart)
                  minCoinsNeeded = myGameInstance.EncounteredMembers.Count;
               if (minCoinsNeeded < gi.GetCoins())
               {
                  Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("CoinsStacked"), Width = 75, Height = 75, Name = "HirelingsRoll" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  string costToPayS = "Click to pay " + minCoinsNeeded.ToString() + "gp:";
                  myTextBlock.Inlines.Add(new Run(costToPayS));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new InlineUIContainer(img1));
               }
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Click image to continue."));
               break;
            case "e338a":
            case "e338c":
               modifiedWitAndWile = gi.WitAndWile + gi.MonkPleadModifier + 1;
               myTextBlock.Inlines.Add(new Run(" < " + modifiedWitAndWile.ToString()));
               if (true == gi.IsSpecialItemHeld(SpecialEnum.CharismaTalisman))
               {
                  Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("TalismanCharismaSmall"), Width = 21, Height = 21, VerticalAlignment = VerticalAlignment.Bottom };
                  gi.IsCharismaTalismanActive = true;
                  myTextBlock.Inlines.Add(new Run(" + "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(img1));
               }
               if (true == gi.IsElfWitAndWileActive)
               {
                  Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("ElfWarriorSmall"), Width = theSmallElfImageWidth, Height = theSmallElfImageHeight, VerticalAlignment = VerticalAlignment.Bottom };
                  myTextBlock.Inlines.Add(new Run(" - "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(img1));
               }
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e338b":
               Image img338b = new Image { Source = MapItem.theMapImages.GetBitmapImage("Muscle"), Width = 200, Height = 200 };
               int numButtons1 = Math.Min(gi.GetCoins(), gi.EncounteredMembers.Count);
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               for (int i = 0; i < numButtons1; ++i)
               {
                  if (0 == i % 5)
                     myTextBlock.Inlines.Add(new LineBreak());
                  int j = i + 1;
                  string content = " " + j.ToString() + "  ";
                  if (j < 10)
                     content = "  " + j.ToString() + "  ";
                  Button b = new Button() { Content = content, FontFamily = myFontFam1, FontSize = 12, Name = "HirelingCount", VerticalAlignment = VerticalAlignment.Bottom };
                  b.Click += Button_Click;
                  myTextBlock.Inlines.Add(new InlineUIContainer(b));
               }
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("                                  "));
               myTextBlock.Inlines.Add(new InlineUIContainer(img338b));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run(" "));
               break;
            case "e339":
               int coinsNeeded = 2;
               if ("e005" == myGameInstance.EventStart)
                  coinsNeeded = 2 * myGameInstance.EncounteredMembers.Count;
               if (coinsNeeded < gi.GetCoins())
               {
                  Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("CoinsStacked"), Width = 75, Height = 75, Name = "HirelingsRoll" };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Stop to talk: "));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new InlineUIContainer(img1));
               }
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Click image to continue."));
               break;
            case "e339a":
            case "e339d":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               else
               {
                  modifiedWitAndWile = gi.WitAndWile + gi.MonkPleadModifier;
                  myTextBlock.Inlines.Add(new Run(" < " + modifiedWitAndWile.ToString()));
                  if (true == gi.IsSpecialItemHeld(SpecialEnum.CharismaTalisman))
                  {
                     gi.IsCharismaTalismanActive = true;
                     myTextBlock.Inlines.Add(new Run(" + "));
                     Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("TalismanCharismaSmall"), Width = 21, Height = 21, VerticalAlignment = VerticalAlignment.Bottom };
                     myTextBlock.Inlines.Add(new InlineUIContainer(img1));
                  }
                  if (true == gi.IsElfWitAndWileActive)
                  {
                     myTextBlock.Inlines.Add(new Run(" - "));
                     Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("ElfWarriorSmall"), Width = theSmallElfImageWidth, Height = theSmallElfImageHeight, VerticalAlignment = VerticalAlignment.Bottom };
                     myTextBlock.Inlines.Add(new InlineUIContainer(img1));
                  }
               }
               break;
            case "e339c":
               Image img339c = new Image { Source = MapItem.theMapImages.GetBitmapImage("Muscle"), Width = 200, Height = 200 };
               int numButtons2 = Math.Min(gi.GetCoins(), gi.EncounteredMembers.Count);
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               for (int i = 0; i < numButtons2; ++i)
               {
                  if (0 == i % 5)
                     myTextBlock.Inlines.Add(new LineBreak());
                  int j = i + 1;
                  string content = " " + j.ToString() + "  ";
                  if (j < 10)
                     content = "  " + j.ToString() + "  ";
                  Button b = new Button() { Content = content, FontFamily = myFontFam1, FontSize = 12, Name = "HirelingCount", VerticalAlignment = VerticalAlignment.Bottom };
                  b.Click += Button_Click;
                  myTextBlock.Inlines.Add(new InlineUIContainer(b));
                  myTextBlock.Inlines.Add(new Run("   "));
               }
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("                                  "));
               myTextBlock.Inlines.Add(new InlineUIContainer(img339c));
               break;
            case "e340":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               else
               {
                  modifiedWitAndWile = gi.WitAndWile + gi.MonkPleadModifier + 1;
                  myTextBlock.Inlines.Add(new Run(" < " + modifiedWitAndWile.ToString()));
                  if (true == gi.IsSpecialItemHeld(SpecialEnum.CharismaTalisman))
                  {
                     Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("TalismanCharismaSmall"), Width = 21, Height = 21, VerticalAlignment = VerticalAlignment.Bottom };
                     gi.IsCharismaTalismanActive = true;
                     myTextBlock.Inlines.Add(new Run(" + "));
                     myTextBlock.Inlines.Add(new InlineUIContainer(img1));
                  }
                  if (true == gi.IsElfWitAndWileActive)
                  {
                     Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("ElfWarriorSmall"), Width = theSmallElfImageWidth, Height = theSmallElfImageHeight, VerticalAlignment = VerticalAlignment.Bottom };
                     myTextBlock.Inlines.Add(new Run(" - "));
                     myTextBlock.Inlines.Add(new InlineUIContainer(img1));
                  }
               }
               break;
            case "e340a":
               if (Utilities.NO_RESULT < gi.DieResults[key][0])
               {
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e341":
               if (Utilities.NO_RESULT < gi.DieResults["e341"][0])
               {
                  Image img1 = new Image { Name = "Converse", Source = MapItem.theMapImages.GetBitmapImage("Nothing"), Width = 100, Height = 100 };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                            "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(img1));
               }
               string nameConversing = "";
               if (0 < myGameInstance.EncounteredMembers.Count)
               {
                  if (true == myGameInstance.EncounteredMembers[0].Name.Contains("Minstrel"))
                     nameConversing = "with Minstrel After Dinner";
               }
               ReplaceText("MINSTREL", nameConversing);
               break;
            case "e342":
               if (Utilities.NO_RESULT < gi.DieResults["e342"][0])
               {
                  Image img1 = new Image { Name = "Inquiry", Source = MapItem.theMapImages.GetBitmapImage("Nothing"), Width = 100, Height = 100 };
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("                                            "));
                  myTextBlock.Inlines.Add(new InlineUIContainer(img1));
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new LineBreak());
                  myTextBlock.Inlines.Add(new Run("Click image to continue."));
               }
               break;
            case "e501":
               StringBuilder sbEndWon = new StringBuilder();
               sbEndWon.Append("Game ends on Day#");
               ++gi.Days;
               sbEndWon.Append(gi.Days.ToString());
               sbEndWon.Append(" due to '");
               sbEndWon.Append(gi.EndGameReason);
               sbEndWon.Append("' in ");
               sbEndWon.Append(gi.Prince.Territory.Name);
               myTextBlock.Inlines.Add(new Run(sbEndWon.ToString()));
               Image imgEndGameWon = null;
               switch (Utilities.RandomGenerator.Next(10))
               {
                  case 0:
                     imgEndGameWon = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("QuicksandJump"), Width = 300, Height = 300 };
                     break;
                  case 1:
                     imgEndGameWon = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Gems"), Width = 300, Height = 300 };
                     break;
                  case 2:
                     imgEndGameWon = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Muscle"), Width = 300, Height = 300 };
                     break;
                  case 3:
                     imgEndGameWon = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Sword1"), Width = 300, Height = 300 };
                     break;
                  case 4:
                     imgEndGameWon = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Star"), Width = 300, Height = 300 };
                     break;
                  default:
                     imgEndGameWon = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Win"), Width = 300, Height = 300 };
                     break;
               }
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("                                  "));
               myTextBlock.Inlines.Add(new InlineUIContainer(imgEndGameWon));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Click image to continue show game statistics and feats."));
               break;
            case "e502":
               StringBuilder sbEndLost = new StringBuilder();
               sbEndLost.Append("Game ends on Day#");
               ++gi.Days;
               sbEndLost.Append(gi.Days.ToString());
               sbEndLost.Append(" due to '");
               sbEndLost.Append(gi.EndGameReason);
               sbEndLost.Append("' in ");
               sbEndLost.Append(gi.Prince.Territory.Name);
               myTextBlock.Inlines.Add(new Run(sbEndLost.ToString()));
               Image imgEndGameLost = null;
               switch (Utilities.RandomGenerator.Next(11))
               {
                  case 0:
                     imgEndGameLost = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Deny"), Width = 300, Height = 300 };
                     break;
                  case 1:
                     imgEndGameLost = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Idiot"), Width = 300, Height = 300 };
                     break;
                  case 2:
                     imgEndGameLost = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("OBlood1"), Width = 300, Height = 300 };
                     break;
                  case 3:
                     imgEndGameLost = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("FarmerDead"), Width = 300, Height = 300 };
                     break;
                  case 4:
                     imgEndGameLost = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Skulls"), Width = 300, Height = 300 };
                     break;
                  default:
                     imgEndGameLost = new Image { Name = "EndGameShowStats", Source = MapItem.theMapImages.GetBitmapImage("Frown"), Width = 300, Height = 300 };
                     break;
               }
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("                                  "));
               myTextBlock.Inlines.Add(new InlineUIContainer(imgEndGameLost));
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new LineBreak());
               myTextBlock.Inlines.Add(new Run("Click image to continue show game statistics and feats."));
               break;
            default:
               break;
         }
      }
      private void AppendEscapeMethods(IGameInstance gi, bool isRidingEscapePossible)
      {
         myTextBlock.Inlines.Add(new LineBreak());
         if (true == gi.Prince.IsFlying)   // Show fly button if able to fly 
         {
            myTextBlock.Inlines.Add(new LineBreak());
            Button b = new Button() { Content = "Fly Away ", FontFamily = myFontFam1, FontSize = 12 };
            b.Click += Button_Click;
            myTextBlock.Inlines.Add(new Run("*"));
            myTextBlock.Inlines.Add(new InlineUIContainer(b));
            myTextBlock.Inlines.Add(new Run(" to escape per "));
            Button b1 = new Button() { Content = "r313", FontFamily = myFontFam1, FontSize = 12 };
            b1.Click += Button_Click;
            myTextBlock.Inlines.Add(new InlineUIContainer(b1));
            myTextBlock.Inlines.Add(new Run("."));
         }
         if ((true == gi.Prince.IsRiding) && (true == isRidingEscapePossible))
         {
            myTextBlock.Inlines.Add(new LineBreak());
            bool isEncounteredRiding = false;
            if (("e005" != gi.EventActive) && ("e006" != gi.EventActive) && ("e007" != gi.EventActive) && ("e058b" != gi.EventActive)) // dwarf and elf encounters are not riding
               isEncounteredRiding = gi.IsEncounteredRiding();
            if (false == isEncounteredRiding)
            {
               Button b = new Button() { Content = "Ride Away", FontFamily = myFontFam1, FontSize = 12 };
               b.Click += Button_Click;
               myTextBlock.Inlines.Add(new Run("*"));
               myTextBlock.Inlines.Add(new InlineUIContainer(b));
               myTextBlock.Inlines.Add(new Run(" to escape per "));
               Button b1 = new Button() { Content = "r312", FontFamily = myFontFam1, FontSize = 12 };
               b1.Click += Button_Click;
               myTextBlock.Inlines.Add(new InlineUIContainer(b1));
               myTextBlock.Inlines.Add(new Run("."));
            }
         }
      }
      private void ReplaceTextForLuckyCharm(IGameInstance gi)
      {
         if ((true == gi.IsGiftCharmActive) || (true == gi.IsSlaveGirlActive)) // gift charm becomes active when user clicks on it after sending E182CharmGiftSelected
         {
            string keyword = "Roll die";
            string newString = "Choose One";
            TextRange text = new TextRange(myTextBlock.ContentStart, myTextBlock.ContentEnd);
            TextPointer current = text.Start.GetInsertionPosition(LogicalDirection.Forward);
            while (current != null)
            {
               string textInRun = current.GetTextInRun(LogicalDirection.Forward);
               if (!string.IsNullOrWhiteSpace(textInRun))
               {
                  int index = textInRun.IndexOf(keyword);
                  if (index != -1)
                  {
                     TextPointer selectionStart = current.GetPositionAtOffset(index, LogicalDirection.Forward);
                     TextPointer selectionEnd = selectionStart.GetPositionAtOffset(keyword.Length, LogicalDirection.Forward);
                     TextRange selection = new TextRange(selectionStart, selectionEnd);
                     selection.Text = newString;
                  }
               }
               current = current.GetNextContextPosition(LogicalDirection.Forward);
            }
         }
         else if(false == gi.IsTalkRoll)
         {
            bool isGiftOfCharmHeld = gi.IsSpecialItemHeld(SpecialEnum.GiftOfCharm);
            bool isSlaveGirlHeld = gi.IsFedSlaveGirlHeld();
            if ((true == isGiftOfCharmHeld) && (true == isSlaveGirlHeld))
            {
               myTextBlock.Inlines.Add(new LineBreak());
               Image img = new Image { Source = MapItem.theMapImages.GetBitmapImage("CharmGift"), Width = 75, Height = 75, Tag = "CharmGift", Name = "CharmGift" };  // Click this image causes E182CharmGiftSelected
               myTextBlock.Inlines.Add(new InlineUIContainer(img));
               myTextBlock.Inlines.Add(new Run("   or   "));
               Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("c41SlaveGirl"), Width = 75, Height = 75, Tag = "CharmSlaveGirl", Name = "CharmSlaveGirl" }; // Click this image causes E163SlaveGirlSelected
               myTextBlock.Inlines.Add(new InlineUIContainer(img1));
               myTextBlock.Inlines.Add(new Run("    Click on either to reroll twice."));
            }
            else if (true == isGiftOfCharmHeld)
            {
               myTextBlock.Inlines.Add(new LineBreak());
               Image img = new Image { Source = MapItem.theMapImages.GetBitmapImage("CharmGift"), Width = 75, Height = 75, Tag = "CharmGift", Name = "CharmGift" };
               myTextBlock.Inlines.Add(new InlineUIContainer(img));
               myTextBlock.Inlines.Add(new Run(" Click the Gift of Charm to reroll twice."));
            }
            else if (true == isSlaveGirlHeld)
            {
               myTextBlock.Inlines.Add(new LineBreak());
               Image img = new Image { Source = MapItem.theMapImages.GetBitmapImage("c41SlaveGirl"), Width = 75, Height = 75, Tag = "CharmSlaveGirl", Name = "CharmSlaveGirl" };
               myTextBlock.Inlines.Add(new InlineUIContainer(img));
               myTextBlock.Inlines.Add(new Run(" Click the Slave Girl to reroll twice."));
            }
         }
      }
      private void ReplaceText(string keyword, string newString)
      {
         TextRange text = new TextRange(myTextBlock.ContentStart, myTextBlock.ContentEnd);
         TextPointer current = text.Start.GetInsertionPosition(LogicalDirection.Forward);
         while (current != null)
         {
            string textInRun = current.GetTextInRun(LogicalDirection.Forward);
            if (!string.IsNullOrWhiteSpace(textInRun))
            {
               int index = textInRun.IndexOf(keyword);
               if (index != -1)
               {
                  TextPointer selectionStart = current.GetPositionAtOffset(index, LogicalDirection.Forward);
                  TextPointer selectionEnd = selectionStart.GetPositionAtOffset(keyword.Length, LogicalDirection.Forward);
                  TextRange selection = new TextRange(selectionStart, selectionEnd);
                  selection.Text = newString;
               }
            }
            current = current.GetNextContextPosition(LogicalDirection.Forward);
         }
      }
      private string ConvertToStructureName(ITerritory t)
      {
         switch (t.Name)
         {
            case "0323": return "Drogat Castle";
            case "0711": return "Branwyn's Temple";
            case "1212": return "Huldra Castle";
            case "1021": return "Sulwyth Temple";
            case "1309": return "Donat's Temple";
            case "1805": return "Temple of Zhor";
            case "1923": return "Aeravir Castle";
            case "2018": return "Temple Duffy";
            default: return t.Name;
         }
      }
      //--------------------------------------------------------------------
      public void ShowEventViewerDieResult(int dieRoll)
      {
         myGameInstance.EventActive = myGameInstance.EventDisplayed; // As soon as you roll the die, the current event becomes the active event
         GameAction action = myGameInstance.DieRollAction;
         StringBuilder sb11 = new StringBuilder("      ######ShowEventViewerDieResult() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(action.ToString());
         sb11.Append(" dr="); sb11.Append(dieRoll.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref action, dieRoll);
      }
      public bool ShowResultsTravel(bool isLost, bool isEncounter, bool isRiverCrossing)
      {
         GameAction outAction = GameAction.Error;
         if ((GamePhase.Rest == myGameInstance.GamePhase) && (false == isEncounter))
         {
            ++myGameInstance.Statistic.myNumOfRestDays;
            outAction = GameAction.RestHealing;
         }
         else if ((GamePhase.Rest == myGameInstance.GamePhase) && (true == isEncounter))
         {
            ++myGameInstance.Statistic.myNumOfRestDays;
            ++myGameInstance.Statistic.myNumEncounters;
            outAction = GameAction.RestHealingEncounter;
         }
         else if ((GamePhase.SearchCache == myGameInstance.GamePhase) && (false == isEncounter))
         {
            outAction = GameAction.SearchCache;
         }
         else if ((GamePhase.SearchCache == myGameInstance.GamePhase) && (true == isEncounter))
         {
            ++myGameInstance.Statistic.myNumEncounters;
            outAction = GameAction.SearchEncounter;
         }
         else if ((GamePhase.SearchTreasure == myGameInstance.GamePhase) && (false == isEncounter))
         {
            outAction = GameAction.SearchTreasure;
         }
         else if ((GamePhase.SearchTreasure == myGameInstance.GamePhase) && (true == isEncounter))
         {
            ++myGameInstance.Statistic.myNumEncounters;
            outAction = GameAction.SearchEncounter;
         }
         else if ((true == isLost) && (false == isEncounter)) // this is also the code block when river crossing fails
         {
            ++myGameInstance.Statistic.myDaysLost;
            outAction = GameAction.TravelShowLost;
         }
         else if ((true == isRiverCrossing) && (true == isEncounter))
         {
            ++myGameInstance.Statistic.myNumRiverCrossingSuccess;
            ++myGameInstance.Statistic.myNumEncounters;
            outAction = GameAction.TravelShowRiverEncounter;
         }
         else if ((true == isLost) && (true == isEncounter))
         {
            ++myGameInstance.Statistic.myDaysLost;
            ++myGameInstance.Statistic.myNumEncounters;
            outAction = GameAction.TravelShowLostEncounter;
         }
         else if (true == isEncounter)
         {
            ++myGameInstance.Statistic.myNumEncounters;
            outAction = GameAction.TravelShowMovementEncounter;
         }
         else if (true == isRiverCrossing)
         {
            ++myGameInstance.Statistic.myNumRiverCrossingSuccess;
            outAction = GameAction.TravelLostCheck;
         }
         else
         {
            outAction = GameAction.TravelShowMovement;
         }
         StringBuilder sb11 = new StringBuilder("     ######ShowResultsTravel() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" as="); sb11.Append(myGameInstance.EventStart);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         return true;
      }
      public bool ShowResultsTravelThroughArch(bool isLost, bool isEncounter, bool isRiverEncounter)
      {
         myGameInstance.GamePhase = GamePhase.Travel;
         GameAction outAction = GameAction.Error;
         if (true == isEncounter)
         {
            ++myGameInstance.Statistic.myNumEncounters;
            outAction = GameAction.TravelShowMovementEncounter;
         }
         else
         {
            outAction = GameAction.TravelShowMovement;
         }
         StringBuilder sb11 = new StringBuilder("     ######ShowResultsTravelThroughArch() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         return true;
      }
      public bool ShowResultsTreasure()
      {
         GameAction outAction = GameAction.Error;
         if (0 == myGameInstance.CapturedWealthCodes.Count)
            outAction = GameAction.EncounterLootStartEnd;
         else
            outAction = GameAction.EncounterLoot;
         StringBuilder sb11 = new StringBuilder("     ######ShowResultsTreasure() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         return true;
      }
      public bool ShowResultsOfHunt(bool isMobPursit, bool isConstabularyPursuit)
      {
         myGameInstance.GamePhase = GamePhase.Hunt;
         GameAction outAction = GameAction.Error;
         if (true == isMobPursit)
            outAction = GameAction.HuntPeasantMobPursuit;
         else if (true == isConstabularyPursuit)
            outAction = GameAction.HuntConstabularyPursuit;
         else
            outAction = GameAction.HuntEndOfDayCheck;
         StringBuilder sb11 = new StringBuilder("     ######ShowResultsOfHunt() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         return true;
      }
      public bool ShowResultsFeeding()
      {
         GameAction outAction = GameAction.CampfireStarvationEnd;
         StringBuilder sb11 = new StringBuilder("     ######ShowResultsFeeding() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         return true;
      }
      public bool ShowResultsLodging()
      {
         myGameInstance.IsFarmerLodging = false; // only applies when e013a is active
         GameAction outAction = GameAction.Error;
         if ((0 < myGameInstance.LostTrueLoves.Count) && (false == myGameInstance.IsTrueLoveHeartBroken)) // true love does not return until no longer heartbroken
            outAction = GameAction.CampfireTrueLoveCheck;
         else
            outAction = GameAction.CampfireLoadTransport;
         StringBuilder sb11 = new StringBuilder("     ######ShowResultsLodging() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         return true;
      }
      public bool ShowTransport()
      {
         GameAction outAction = GameAction.CampfireWakeup;
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         StringBuilder sb11 = new StringBuilder("     ######ShowTransport() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         return true;
      }
      public bool ShowTransportAfterRedistribute()
      {
         GameAction outAction = GameAction.TransportRedistributeEnd;
         StringBuilder sb11 = new StringBuilder("     ######ShowTransportAfterRedistribute() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         return true;
      }
      public bool ShowResultsCombat(bool isRoute, bool isEscape)
      {
         myGameInstance.ForbiddenAudiences.RemoveMonsterKillConstraints(myGameInstance.NumMonsterKill);
         GameAction outAction = GameAction.Error;
         bool isNecklass = myGameInstance.Prince.IsSpecialItemHeld(SpecialEnum.ResurrectionNecklace);
         if (true == myGameInstance.Prince.IsKilled)
         {
            myGameInstance.GamePhase = GamePhase.EndGame;
            if (true == isNecklass)
            {
               outAction = GameAction.EndGameResurrect;  // ShowResultsCombat()
            }
            else
            {
               outAction = GameAction.EndGameLost;  // ShowResultsCombat()
               myGameInstance.EndGameReason = "Prince died as a warrior in battle";
            }
         }
         else if (true == isEscape)
         {
            if ("e123b" == myGameInstance.EventStart) // fight black knight
               outAction = GameAction.E123BlackKnightCombatEnd;
            else
               outAction = GameAction.EncounterEscape;
         }
         else
         {
            outAction = GameAction.EncounterLootStart;
         }
         StringBuilder sb11 = new StringBuilder("     ######ShowResultsCombat() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" as="); sb11.Append(myGameInstance.EventStart);
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         sb11.Append(" n?="); sb11.Append(isNecklass.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         return true;
      }
      public bool ShowMarkOfCainResult()
      {
         GameAction outAction = GameAction.E018MarkOfCainEnd;
         StringBuilder sb11 = new StringBuilder("     ######ShowMarkOfCainResult() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction, 0);
         return true;
      }
      public bool ShowE010DisgustCheck()
      {
         GameAction outAction = GameAction.CampfireWakeup;
         StringBuilder sb11 = new StringBuilder("     ######ShowE010DisgustCheck() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction, 0);
         return true;
      }
      public bool ShowE039ChestOpeningResult(bool isChestOpen)
      {
         GameAction outAction = GameAction.Error;
         if (true == isChestOpen)
            outAction = GameAction.EncounterLootStart;
         else
            outAction = GameAction.EncounterEnd;
         StringBuilder sb11 = new StringBuilder("     ######ShowE039ChestOpeningResult() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction, 0);
         return true;
      }
      public bool ShowE040ChestOpeningResult(bool isChestOpen, int dieRoll)
      {
         GameAction outAction = GameAction.Error;
         if (true == isChestOpen)
            outAction = GameAction.EncounterLootStart;
         else
            outAction = GameAction.EncounterEnd;
         StringBuilder sb11 = new StringBuilder("     ######ShowE040ChestOpeningResult() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction, dieRoll);
         return true;
      }
      public bool ShowE044InvocationResult(bool isPrinceBlessed, bool isClueFound)
      {
         GameAction outAction = GameAction.Error;
         if (true == isPrinceBlessed)
            outAction = GameAction.E044HighAltarBlessed;
         else if (true == isClueFound)
            outAction = GameAction.E044HighAltarClue;
         else
            outAction = GameAction.E044HighAltarEnd;
         StringBuilder sb11 = new StringBuilder("     ######ShowE044InvocationResult() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         return true;
      }
      public bool ShowE060ReleasePrisonerResult()
      {
         GameAction outAction = GameAction.EncounterEnd;
         StringBuilder sb11 = new StringBuilder("     ######ShowE060ReleasePrisonerResult() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         return true;
      }
      public bool ShowE073WitchSpellResult(bool isTalismanUsed, bool isPrinceFrog)
      {
         GameAction outAction = GameAction.Error;
         if (true == isPrinceFrog)
            outAction = GameAction.E073WitchTurnsPrinceIsFrog;
         else if (true == isTalismanUsed)
            outAction = GameAction.E073WitchCombat;
         else
            outAction = GameAction.EncounterEscape;
         StringBuilder sb11 = new StringBuilder("     ######ShowE073WitchSpellResult() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction, 0);
         return true;
      }
      public bool ShowE079ColdCheckResult(bool isAnyLost)
      {
         GameAction outAction = GameAction.Error;
         if ("e079" == myGameInstance.EventActive)
            outAction = GameAction.EncounterEnd;
         else if (true == isAnyLost) // if this is next day, can either redistribute if anybody lost, or continue travels
            outAction = GameAction.E079HeavyRainsRedistribute;
         else
            outAction = GameAction.E079HeavyRainsContinueTravel;
         StringBuilder sb11 = new StringBuilder("     ######ShowE079ColdCheckResult() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction, 0);
         return true;
      }
      public bool ShowE085FallingResult()
      {
         GameAction outAction = GameAction.EncounterEnd;
         StringBuilder sb11 = new StringBuilder("     ######ShowE085FallingResult() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction, 0);
         return true;
      }
      public bool ShowE088FallingRocksResult()
      {
         GameAction outAction = GameAction.EncounterEnd;
         StringBuilder sb11 = new StringBuilder("     ######ShowE088FallingRocksResult() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction, 0);
         return true;
      }
      public bool ShowE090QuicksandResult()
      {
         GameAction outAction = GameAction.EncounterEnd;
         StringBuilder sb11 = new StringBuilder("     ######ShowE090QuicksandResult() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction, 0);
         return true;
      }
      public bool ShowE095MountCheckResult()
      {
         GameAction outAction = GameAction.Error;
         if (true == myGameInstance.IsMountsAtRisk)
            outAction = GameAction.E095MountAtRiskEnd;
         else
            outAction = GameAction.EncounterEnd;
         StringBuilder sb11 = new StringBuilder("     ######ShowE095MountCheckResult() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction, 0);
         return true;
      }
      public bool ShowE096MountDieCheckResult()
      {
         GameAction outAction = GameAction.CampfireMountDieCheckEnd;
         StringBuilder sb11 = new StringBuilder("     ######ShowE096MountDieCheckResult() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction, 0);
         return true;
      }
      public bool ShowE097FleshRotResult()
      {
         GameAction outAction = GameAction.E097FleshRotEnd;
         StringBuilder sb11 = new StringBuilder("     ######ShowE097FleshRotResult() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction, 0);
         return true;
      }
      public bool ShowE106OvercastLostResult()
      {
         GameAction outAction = GameAction.E106OvercastLostEnd;
         StringBuilder sb11 = new StringBuilder("     ######ShowE106OvercastLostResult() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction, 0);
         return true;
      }
      public bool ShowE107FalconCheckResult()
      {
         GameAction outAction = GameAction.CampfireFalconCheckEnd;
         StringBuilder sb11 = new StringBuilder("     ######ShowE107FalconCheckResult() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction, 0);
         return true;
      }
      public bool ShowE109PegasusCapture()
      {
         GameAction outAction = GameAction.EncounterEnd;
         StringBuilder sb11 = new StringBuilder("     ######ShowE109PegasusCapture() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction, 0);
         return true;
      }
      public bool ShowE111StormDemonRepelResults(bool isAttackRepelled)
      {
         GameAction outAction = GameAction.EncounterEnd;
         if (false == isAttackRepelled)
            outAction = GameAction.E111StormDemonRepelFail;
         StringBuilder sb11 = new StringBuilder("     ######ShowE111StormDemonRepelResults() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction, 0);
         return true;
      }
      public bool ShowE121SunStrokeCheckResult(bool isSunStroke, bool isMountDeath)
      {
         GameAction outAction = GameAction.Error;
         if ((true == isSunStroke) || (true == isMountDeath))
            outAction = GameAction.E121SunStrokeEnd;
         else
            outAction = GameAction.EncounterEnd;
         StringBuilder sb11 = new StringBuilder("     ######ShowE121SunStrokeCheckResult() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction, 0);
         return true;
      }
      public bool ShowE123DisgustCheck()
      {
         GameAction outAction = GameAction.E123BlackKnightRefuseEnd;
         StringBuilder sb11 = new StringBuilder("     ######ShowE123DisgustCheck() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction, 0);
         return true;
      }
      public bool ShowE126RaftInCurrentResult(bool isAnyLost, bool isPrinceLost)
      {
         GameAction outAction = GameAction.Error;
         if (true == isAnyLost)
            outAction = GameAction.E126RaftInCurrentRedistribute;
         else if (true == isPrinceLost)
            outAction = GameAction.E126RaftInCurrentLostRaft;
         else
            outAction = GameAction.E126RaftInCurrentEnd;
         StringBuilder sb11 = new StringBuilder("     ######ShowE126RaftInCurrentResult() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction, 0);
         return true;
      }
      public bool ShowE212TempleCurse()
      {
         GameAction outAction = GameAction.EncounterEnd;
         StringBuilder sb11 = new StringBuilder("     ######ShowE212TempleCurse() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction, 0);
         return true;
      }
      public bool ShowTombOpeningResult()
      {
         GameAction outAction = GameAction.EncounterEnd;
         StringBuilder sb11 = new StringBuilder("     ######ShowTombOpeningResult() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction, 0);
         return true;
      }
      public bool ShowRetreivalResult()
      {
         GameAction outAction = GameAction.E043SmallAltarEnd;
         StringBuilder sb11 = new StringBuilder("     ######E043SmallAltarEnd() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         return true;
      }
      public bool ShowSearchRuinsPlague(bool isPrinceAffected)
      {
         GameAction outAction = GameAction.Error;
         if (true == isPrinceAffected)
            outAction = GameAction.E133PlaguePrince;
         else
            outAction = GameAction.E133PlagueParty;
         StringBuilder sb11 = new StringBuilder("     ######ShowSearchRuinsPlague() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         return true;
      }
      public bool ShowSearchRuinsShakey(bool isSearchRuins)
      {
         GameAction outAction = GameAction.Error;
         if ("e134" == myGameInstance.EventActive)
         {
            if (true == isSearchRuins)
               outAction = GameAction.E134ShakyWallsSearch;
            else
               outAction = GameAction.E134ShakyWallsEnd;
         }
         StringBuilder sb11 = new StringBuilder("     ######ShowSearchRuinsShakey() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         return true;
      }
      public bool ShowPlagueDust()
      {
         GameAction outAction = GameAction.CampfirePlagueDustEnd;
         StringBuilder sb11 = new StringBuilder("     ######ShowPlagueDust() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         myGameInstance.IsGridActive = false;  // ShowPlagueDust() - Can show active event button in status bar
         return true;
      }
      public bool ShowCharismaTalismanDestroy()
      {
         GameAction outAction = GameAction.CampfireTalismanDestroyEnd;
         StringBuilder sb11 = new StringBuilder("     ######ShowCharismaTalismanDestroy() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         myGameInstance.IsGridActive = false;  // ShowCharismaTalismanDestroy() - Can show active event button in status bar
         return true;
      }
      public bool ShowAntipoisonAmuletDestroy()
      {
         GameAction outAction = GameAction.CampfireAntipoisionAmuletDestroyEnd;
         StringBuilder sb11 = new StringBuilder("     ######ShowAntipoisonAmuletDestroy() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         myGameInstance.IsGridActive = false;  // CampfireAntipoisionAmuletDestroyEnd() - Can show active event button in status bar
         return true;
      }
      public bool ShowPrisonBreakResults()
      {
         GameAction outAction = GameAction.E203EscapeFromPrisonEnd;
         StringBuilder sb11 = new StringBuilder("     ######ShowPrisonBreakResults() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         myGameInstance.IsGridActive = false;  // ShowPrisonBreakResults() - Can show active event button in status bar
         return true;
      }
      public bool ShowFindVictimResults()
      {
         GameAction outAction = GameAction.Error;
         if ("e082" == myGameInstance.EventStart)
            outAction = GameAction.E082SpectreMagicEnd;
         else if ("e091" == myGameInstance.EventStart)
            outAction = GameAction.E091PoisonSnakeEnd;
         StringBuilder sb11 = new StringBuilder("     ######ShowFindVictimResults() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         sb11.Append(" kia?="); sb11.Append(myGameInstance.Prince.IsKilled);
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         myGameInstance.IsGridActive = false;  // ShowFindVictimResults() - Can show active event button in status bar
         return true;
      }
      public bool ShowTrueLoveCheck()
      {
         GameAction outAction = GameAction.CampfireLoadTransport;
         StringBuilder sb11 = new StringBuilder("     ######ShowTrueLoveCheck() :");
         sb11.Append(" p="); sb11.Append(myGameInstance.GamePhase.ToString());
         sb11.Append(" ae="); sb11.Append(myGameInstance.EventActive);
         sb11.Append(" a="); sb11.Append(outAction.ToString());
         Logger.Log(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER, sb11.ToString());
         myGameEngine.PerformAction(ref myGameInstance, ref outAction);
         return true;
      }
      //--------------------------------------------------------------------
      private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
      {
         GameAction action = GameAction.Error;
         if (myGameInstance.EventActive != myGameInstance.EventDisplayed) // if an image is clicked, only take action if on active screen
         {
            ReturnToActiveEventDialog dialog = new ReturnToActiveEventDialog(); // Get the name from user
            dialog.Topmost = true;
            if (true == dialog.ShowDialog())
            {
               GameAction actionGoto = GameAction.UpdateEventViewerActive;
               myGameEngine.PerformAction(ref myGameInstance, ref actionGoto);
            }
            return;
         }
         System.Windows.Point p = e.GetPosition((UIElement)sender);
         HitTestResult result = VisualTreeHelper.HitTest(myTextBlock, p);  // Get the Point where the hit test occurrs
         foreach (Inline item in myTextBlock.Inlines)
         {
            if (item is InlineUIContainer ui)
            {
               if (ui.Child is Image)
               {
                  Image img = (Image)ui.Child;
                  if (result.VisualHit == img)
                  {
                     RollEndCallback rollEndCallback = ShowEventViewerDieResult;
                     if (true == img.Name.Contains("DieRoll"))
                     {
                        myDieRoller.RollMovingDie(myCanvas, rollEndCallback);
                        img.Visibility = Visibility.Hidden;
                        return;
                     }
                     else
                     {
                        switch (img.Name)
                        {
                           case "DiceRoll":
                              myDieRoller.RollMovingDice(myCanvas, rollEndCallback);
                              img.Visibility = Visibility.Hidden;
                              return;
                           case "Die3Roll":
                              myDieRoller.Roll3MovingDice(myCanvas, rollEndCallback);
                              img.Visibility = Visibility.Hidden;
                              return;
                           case "AirLowClouds":
                              action = GameAction.E102LowClouds;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "AirHeadWinds":
                              action = GameAction.E103BadHeadWinds;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "AirOvercast":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e106"][0])
                              {
                                 action = GameAction.E106OvercastLost;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              break;
                           case "AirSpiritConfused":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e110b"][1])
                              {
                                 action = GameAction.E110AirSpiritConfusedEnd;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              break;
                           case "AirStormCloud":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e105"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              break;
                           case "StormDemonAttack":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e111"][0])
                              {
                                 action = GameAction.E111StormDemonEnd;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              break;
                           case "StormDemonRepel":
                              action = GameAction.E111StormDemonRepel;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "AirTailWinds":
                              action = GameAction.E104TailWinds;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "AirViolentWeather":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e105a"][2])
                              {
                                 action = GameAction.E105ViolentWeather;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              break;
                           case "Ally":
                              action = GameAction.E334Ally;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "AllyNoble":
                              action = GameAction.E152NobleAlly;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "AltarSmall":
                              action = GameAction.E043SmallAltar;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "AncientTreasure":
                              action = GameAction.E027AncientTreasure;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "Arch":
                              action = GameAction.EncounterStart;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "ArchSkip":
                              action = GameAction.E045ArchOfTravelSkip; // Found an Arch, but skipped traveling through it
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "AudienceDaughter":
                           case "AudienceCountDrogat":
                           case "AudienceLadyAeravir":
                           case "AudienceHighPriest":
                           case "AudienceMayor":
                           case "AudienceOffering":
                              action = GameAction.EncounterRoll;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "Bandit":
                              action = GameAction.EncounterStart;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "BaronDrogat":
                              action = GameAction.E161CountAudienceApplyResults;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "BearAttack":
                              action = GameAction.EncounterStart;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "BearEncounter":
                              action = GameAction.E084BearEncounter;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "BlackKnightRefuse":
                              action = GameAction.E123BlackKnightRefuse;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "BlackKnightFight":
                              action = GameAction.EncounterStart;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "BlackPortal":
                              if ("e046" == myGameInstance.EventActive)
                              {
                                 action = GameAction.EncounterStart;
                              }
                              else if ("e046a" == myGameInstance.EventActive)
                              {
                                 if (Utilities.NO_RESULT < myGameInstance.DieResults["e046a"][0])
                                    action = GameAction.EncounterStart;
                                 else
                                    return;  // do nothing
                              }
                              else if ("e046b" == myGameInstance.EventActive)
                              {
                                 action = GameAction.EncounterEnd;
                              }
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "BoxUnopened":
                              action = GameAction.EncounterEnd;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "Boar":
                              action = GameAction.E083WildBoar;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "BoarCooked":
                              action = GameAction.EncounterRoll;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "Brain":
                              action = GameAction.SetupGameOptionChoice;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "BribeToHireEnd":
                              action = GameAction.UpdateEventViewerActive;
                              myGameInstance.DieRollAction = GameAction.EncounterRoll;
                              myGameInstance.EventDisplayed = myGameInstance.EventActive = "e332a";
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "BribeToHirePay":
                              action = GameAction.E332PayGroup;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "BribeToJoinEnd":
                              action = GameAction.UpdateEventViewerActive;
                              myGameInstance.DieRollAction = GameAction.EncounterRoll;
                              myGameInstance.EventDisplayed = myGameInstance.EventActive = "e331a";
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "BribeToJoinDeny":
                              action = GameAction.E331DenyFickle;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "BribeToJoinPay":
                              action = GameAction.E331PayFickle;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "BribeToSeneschalDeny":
                              action = GameAction.E148SeneschalDeny;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "BribeToSeneschalPay":
                              action = GameAction.E148SeneschalPay;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "BrokenColumns":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e135"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "BrokenLove":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e160e"][0])
                              {
                                 action = GameAction.E160BrokenLove;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "BuyAmulet":
                              action = GameAction.E129aBuyAmulet;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "BuyAmuletDeny":
                              action = GameAction.EncounterEnd;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "BuyInfo":
                              action = GameAction.UpdateEventViewerActive;
                              myGameInstance.EventDisplayed = myGameInstance.EventActive = "e209h";
                              myGameInstance.DieRollAction = GameAction.EncounterRoll;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "BuyInfoEnd":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e209h"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "BuyPegasus":
                              action = GameAction.E128aBuyPegasus;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "BuyPegasusDeny":
                              action = GameAction.EncounterEnd;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "CacheEnd":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e214"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "Campfire":
                              if (("e046a" == myGameInstance.EventActive) && (Utilities.NO_RESULT == myGameInstance.DieResults["e046a"][0]))
                                 return;  // do nothing
                              action = GameAction.EncounterEnd;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "ReturnToJail":
                              action = GameAction.UpdateLoadingGameReturnToJail;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "Caravan":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e129"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "Cavalry":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e151"][0])
                              {
                                 action = GameAction.EncounterEnd;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "Cave":
                              action = GameAction.E028CaveTombs;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "ChagaDrugPay":
                              action = GameAction.E143ChagaDrugPay;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "ChagaDrugDeny":
                              action = GameAction.E143ChagaDrugDeny;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "ChagaDrug":
                              action = GameAction.E143SecretOfTemple;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "CharmGift":
                              action = GameAction.E182CharmGiftSelected;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "CharmSlaveGirl":
                              action = GameAction.E163SlaveGirlSelected;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "Chest":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e147a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "Coin":
                              action = GameAction.EncounterLootStart;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "CoinBag":
                              action = GameAction.EncounterEnd;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "Converse":
                              action = GameAction.EncounterRoll;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "CountDrogatJewels":
                              action = GameAction.E146StealGems;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "Crocodile":
                              if ((Utilities.NO_RESULT < myGameInstance.DieResults["e094"][0]) || (Utilities.NO_RESULT < myGameInstance.DieResults["e094a"][0]))
                              {
                                 action = GameAction.EncounterStart;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              break;
                           case "DecisionFight":
                              action = GameAction.E053CampsiteFight;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "DecisionHide":
                              action = GameAction.EncounterHide;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "DecisionShare":
                              action = GameAction.E072FollowElves;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "Dungeon":
                              action = GameAction.CampfireWakeup;
                              myGameInstance.GamePhase = GamePhase.Campfire;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "Dwarfs":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e058"][0])
                              {
                                 action = GameAction.EncounterStart;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              break;
                           case "DwarfAdvice":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e006f"][1])
                              {
                                 action = GameAction.EncounterEnd;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "DwarfAdviceEnd":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e006g"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "DwarfWarriorBand":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e058h"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              break;
                           case "DwarfWarrior":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e006a"][0])
                              {
                                 action = GameAction.EncounterStart;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              break;
                           case "Eagles":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e113"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              break;
                           case "EaglesAlly":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e117"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              break;
                           case "ElfWarrior":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e007a"][0])
                              {
                                 action = GameAction.EncounterStart;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              break;
                           case "ElfWarriorBand":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e071d"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              break;
                           case "E002EncounterTalk":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e002b"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E002EncounterEvade":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e002c"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E002EncounterFight":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e002d"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E003EncounterTalk":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e003a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E003EncounterEvade":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e003b"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E003EncounterFight":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e003c"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E004EncounterTalk":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e004a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E004EncounterEvade":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e004b"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E004EncounterFight":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e004c"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E005EncounterTalk":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e005a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E005EncounterEvade":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e005b"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E005EncounterFight":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e005c"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E006EncounterTalk":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e006c"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E006EncounterEvade":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e006d"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E006EncounterFight":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e006e"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E007EncounterTalk":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e007c"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E007EncounterEvade":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e007d"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E007EncounterFight":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e007e"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E008EncounterTalk":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e008a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E008bEncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e008b"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E018EncounterTalk":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e018a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E018EncounterFight":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e018b"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E019EncounterTalk":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e019a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E019EncounterEvade":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e019b"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E019EncounterFight":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e019c"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E020EncounterTalk":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e020a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E020EncounterEvade":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e020b"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E020EncounterFight":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e020c"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E021EncounterTalk":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e021a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E021EncounterEvade":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e021b"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E021EncounterFight":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e021c"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E023EncounterTalk":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e023a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E023EncounterEvade":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e023b"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E023EncounterFight":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e023c"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E024EncounterFight":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e024"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E028aEncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e028a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E029EncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e029"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E032EncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e032"][0])
                              {
                                 action = GameAction.EncounterStart;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E032aEncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e032a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E033EncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e033"][0])
                              {
                                 action = GameAction.EncounterStart;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E037EncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e037"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E038EncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e038"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E041EncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e041"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E050EncounterTalk":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e050b"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E050EncounterEvade":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e050c"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E050EncounterFight":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e050d"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E057EncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e057"][0])
                              {
                                 action = GameAction.EncounterStart;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E058EncounterTalk":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e058e"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E058EncounterEvade":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e058f"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E058EncounterFight":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e058g"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E071EncounterTalk":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e071a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E071EncounterEvade":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e071b"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E071EncounterFight":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e071c"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E081EncounterTalk":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e081a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E081EncounterEvade":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e081b"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E081EncounterFight":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e081c"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E098EncounterEvade":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e098a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E098EncounterFight":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e098b"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E099EncounterEvade":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e099a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E099EncounterFight":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e099b"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E100EncounterTalk":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e100a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E100EncounterEvade":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e100b"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E100EncounterFight":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e100c"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E101EncounterTalk":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e101a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E101EncounterEvade":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e101b"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E101EncounterFight":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e101c"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E112EncounterFollow":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e112a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E112EncounterEvade":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e112b"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E112EncounterFight":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e112c"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E118EncounterTalk":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e118a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E118EncounterEvade":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e118b"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E118EncounterFight":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e118c"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E128dEncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e128d"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E130EncounterTalk":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e130a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E130EncounterEvade":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e130b"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E130EncounterFight":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e130c"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E137EncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e137"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E138EncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e138"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E139EncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e139"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E140EncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e140"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E140bEncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e140b"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E141EncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e141"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E147EncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e147"][1])
                              {
                                 action = GameAction.E147ClueToTreasure;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E162EncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e162"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E314EncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e314"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E315EncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e315"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E317EncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e317"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E318EncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e318"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E319EncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e319"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E320EncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e320"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E326EncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e326"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E327EncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e327"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E328EncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e328"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E329EncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e329"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E331aEncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e331a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E332aEncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e332a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E333aEncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e333a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E336EncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e336"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E337EncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e337"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E337aEncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e337a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E338aEncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e338a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E338cEncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e338c"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E339aEncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e339a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E339dEncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e339d"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E340EncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e340"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "E340aEncounterRoll":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e340a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "EncounterEnd":
                              action = GameAction.EncounterEnd;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "EncounterRoll":
                              action = GameAction.EncounterRoll;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "EndGameExit":
                              action = GameAction.EndGameExit;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "EndGameFinal":
                              action = GameAction.EndGameFinal;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "EndGameShowStats":
                              if( false == GameEngine.theFeatsInGame.IsEqual(GameEngine.theFeatsInGameStarting) )
                                 action = GameAction.EndGameShowFeats;
                              else
                                 action = GameAction.EndGameShowStats;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "Exhausted":
                              action = GameAction.E120Exhausted;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "Escapee":
                              action = GameAction.E335Escapee;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "FalconNoFeed":
                              action = GameAction.E107FalconNoFeed;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "FallingRocks":
                              action = GameAction.E088FallingRocks;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "FarmerBoy":
                              action = GameAction.E011FarmerPurchaseEnd;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "FarmerBoyEnd":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e011d"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "FarmlandFriendly":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e009a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "FarmlandBoltDoor":
                              action = GameAction.EncounterStart;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "FarmlandDead":
                           case "FarmlandEnd":
                              action = GameAction.EncounterEnd;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "FarmlandLodging":
                              action = GameAction.E013Lodging;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "FarmlandRaid":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e009b"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "FarmlandPeaceful":
                           case "FarmlandRich":
                           case "Follow":
                              action = GameAction.EncounterStart;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "FineClothesBuy":
                              action = GameAction.EncounterEnd;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "FleshRot":
                              action = GameAction.E097FleshRot;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "Flood":
                              action = GameAction.E092Flood;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "FoodGive":
                              action = GameAction.E010FoodGive;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "FoodDeny":
                              action = GameAction.E010FoodDeny;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "FugitiveAlly":
                              action = GameAction.E048FugitiveAlly;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "FugitiveFight":
                              action = GameAction.E048FugitiveFight;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "Gems":
                              action = GameAction.EncounterLootStart;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "Golem":
                              action = GameAction.EncounterStart;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "GolemEnd":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e036a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "GuardBribe":
                              action = GameAction.E130BribeGuard;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "GuardRob":
                              action = GameAction.E130RobGuard;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "HalflingTown":
                              action = GameAction.E070HalflingTown;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "HawkmenFight":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e108"][0])
                              {
                                 action = GameAction.EncounterStart;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "Herd":
                              action = GameAction.E077HerdCapture;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "HeavyRain":
                              action = GameAction.E079HeavyRains;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "HeirRescue":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e144b"][0])
                              {
                                 action = GameAction.EncounterStart;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              break;
                           case "HiddenRuins":
                              action = GameAction.E064HiddenRuins;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "HiddenTown":
                              action = GameAction.EncounterStart;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "HighPass":
                              action = GameAction.E086HighPass;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "HighPassRedistribute":
                              action = GameAction.EncounterRoll;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "HighPriest":
                              action = GameAction.E155HighPriestAudienceApplyResults;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "HireFreeman":
                              action = GameAction.E210HireFreeman;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "HireLancer":
                              action = GameAction.E210HireLancer;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "HireMerc1":
                              action = GameAction.E210HireMerc1;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "HireMerc2":
                              action = GameAction.E210HireMerc2;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "HireHenchmanEnd":
                              action = GameAction.E210HireHenchmen;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "HireLocalGuide":
                              action = GameAction.E210HireLocalGuide;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "HireRunaway":
                              action = GameAction.E210HireRunaway;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "HirePorterEnd":
                              action = GameAction.E210HirePorter;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "HirelingsDeny": // e333
                              action = GameAction.E333DenyHirelings;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "HirelingsPay":  // e333
                              action = GameAction.E333PayHirelings;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "HirelingsRoll":  // e339a
                              action = GameAction.UpdateEventViewerActive;
                              if ("e338" == myGameInstance.EventActive)
                              {
                                 if ("e005" == myGameInstance.EventStart) // amazons
                                    myGameInstance.EventDisplayed = myGameInstance.EventActive = "e338c"; // all must hire as a group
                                 else
                                    myGameInstance.EventDisplayed = myGameInstance.EventActive = "e338a";
                              }
                              else if ("e339" == myGameInstance.EventActive)
                              {
                                 if ("e005" == myGameInstance.EventStart) // amazons
                                    myGameInstance.EventDisplayed = myGameInstance.EventActive = "e339d"; // all must hire as a group
                                 else
                                    myGameInstance.EventDisplayed = myGameInstance.EventActive = "e339a";
                              }
                              else
                              {
                                 Logger.Log(LogEnum.LE_ERROR, "TextBlock_MouseDown(): invalid path for ae=" + myGameInstance.EventActive);
                              }
                              myGameInstance.DieRollAction = GameAction.EncounterRoll;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "Horde":
                              action = GameAction.E136FallingCoins;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "HostileGuardsPay":
                              action = GameAction.E158HostileGuardPay;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "HostileGuardsDeny":
                              action = GameAction.EncounterStart;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "HuldraAudience":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e211g"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              break;
                           case "HuldraGuardFight":
                              action = GameAction.EncounterStart;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "HuldraFight":
                              action = GameAction.EncounterStart;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "HuntingCat":
                              action = GameAction.E076HuntingCat;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "IdiotStart":
                              action = GameAction.E035IdiotStartDay;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "IdiotContinue":
                              action = GameAction.E035IdiotContinue;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "Inquiry":
                              action = GameAction.EncounterRoll;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "Invocation":
                              action = GameAction.E044HighAltar;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "Jail":
                              myGameInstance.IsGridActive = false; // case "Jail":
                              myGameInstance.DieResults["e203a"][0] = Utilities.NO_RESULT;
                              if (false == myGameInstance.IsJailed)
                                 action = GameAction.E203EscapeFromPrison;
                              else
                                 action = GameAction.CampfireWakeup;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "JailArrested":
                              action = GameAction.EncounterRoll;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "JailDungeon":
                              myGameInstance.DieResults["e203c"][0] = Utilities.NO_RESULT;
                              if (false == myGameInstance.IsDungeon)
                                 action = GameAction.E203EscapeFromDungeon;
                              else
                                 action = GameAction.CampfireWakeup;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "JailOvernight":
                              action = GameAction.E060JailOvernight;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "JailTravels":
                              action = GameAction.E130JailedOnTravels;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "LadyAeravir":
                              action = GameAction.E160LadyAudienceApplyResults;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "LadyAeravirSupport":
                              action = GameAction.EncounterRoll;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "Letter":
                              action = GameAction.E157LetterEnd;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "Lizard":
                              action = GameAction.EncounterStart;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "LootedTomb":
                              action = GameAction.E031LootedTomb;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "LootersPay":
                              action = GameAction.E340PayLooters;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "LootersDeny":
                              action = GameAction.E340DenyLooters;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "LordsDaughter":
                              action = GameAction.E154LordsDaughter;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "LordsDaughterLove":
                              action = GameAction.EncounterLootStart;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "LostInAir":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e205c"][0])
                              {
                                 action = GameAction.EncounterStart;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              break;
                           case "MagicianEnd":
                              action = GameAction.EncounterStart;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "MagicianEndFight":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e016b"][0])
                              {
                                 action = GameAction.EncounterEnd;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "MasterOfHouseholdDeny":
                              action = GameAction.E153MasterOfHouseholdDeny;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "MasterOfHouseholdPay":
                              action = GameAction.E153MasterOfHouseholdPay;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "Mayor":
                              action = GameAction.E156MayorAudienceApplyResults;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "MineAbandoned":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e067"][0])
                              {
                                 action = GameAction.EncounterStart;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              break;
                           case "MinstrelStart":
                              action = GameAction.E049MinstrelStart;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "Mob":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e017"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              break;
                           case "MonksWarrior":
                              action = GameAction.EncounterRoll;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "MountsDie":
                              action = GameAction.E096MountsDie;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "MountTired":
                              action = GameAction.E095MountAtRisk;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "MountTiredDie":
                              action = GameAction.E095MountAtRiskEnd;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "MountainPath":
                              if ((Utilities.NO_RESULT < myGameInstance.DieResults["e078"][0]) && ("e078" == myGameInstance.EventActive))
                              {
                                 action = GameAction.EncounterStart;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              else if ("e078c" == myGameInstance.EventActive)
                              {
                                 action = GameAction.EncounterStart;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "Merchant":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e128"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "MagicianHome":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e068a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "Mirror":
                              action = GameAction.EncounterStart;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "NarrowLedge":
                              action = GameAction.E085Falling;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "Nothing":
                              action = GameAction.EncounterEnd;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "PegasusCapture":
                              action = GameAction.E109PegasusCapture;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "Pixie":
                              action = GameAction.EncounterStart;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "PixieAdvice":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e025b"][1])
                              {
                                 action = GameAction.EncounterEnd;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "PixieGift":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e080a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              break;
                           case "PlaguePrinceEnd":
                              action = GameAction.E133PlaguePrinceEnd;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "PoisonPlant":
                              action = GameAction.EncounterStart;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "PrinceResurrected":
                              action = GameAction.E192PrinceResurrected;
                              myGameInstance.GamePhase = GamePhase.Encounter;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "Prisoner":
                              action = GameAction.CampfireWakeup;
                              myGameInstance.GamePhase = GamePhase.Campfire;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "Quicksand":
                              action = GameAction.E090Quicksand;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "RaftingEndsForDay":
                              action = GameAction.EncounterRoll;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "RaftOverturns":
                              action = GameAction.EncounterStart;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "RaftQuickBuild":
                              if (0 < myGameInstance.DieResults["e124"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              break;
                           case "RaftRoughWater":
                              action = GameAction.EncounterStart;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "RaftWaterCurrent":
                              action = GameAction.E126RaftInCurrent;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "RaftsmenEnd":
                              action = GameAction.E122RaftsmenEnd;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "RaftsmenCross":
                              action = GameAction.E122RaftsmenCross;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "RaftsmenHire":
                              action = GameAction.E122RaftsmenHire;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "Reaver":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e014a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "ReaverEnd":
                              action = GameAction.EncounterEnd;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "ReaverHostileFight":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e014c"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "ReaverFriendlyFight":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e015c"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "ReaverInquiry":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e015a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "RuinsSearch":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e208"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "RiverCrossed":
                              action = GameAction.TravelLostCheck;
                              myGameInstance.GamePhase = GamePhase.Travel;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              break;
                           case "SeekNewsNoPay":
                              action = GameAction.SeekNewsNoPay;
                              myGameInstance.GamePhase = GamePhase.SeekNews;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "SeekNewsThievesGuildNoPay":
                              action = GameAction.E209ThievesGuiildNoPay;
                              myGameInstance.GamePhase = GamePhase.Encounter;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "SeekAudience":
                           case "SeekHireNext":
                              action = GameAction.EncounterRoll;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "SeekNewsNext":
                              action = GameAction.EncounterRoll;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "E209ThievesGuiildNoPay":
                              action = GameAction.E209ThievesGuiildNoPay;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "SeekNewsWithPay":
                              action = GameAction.SeekNewsWithPay;
                              myGameInstance.GamePhase = GamePhase.SeekNews;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "SetupShowStartingWealth":
                              action = GameAction.SetupShowStartingWealth;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "ShakeyWalls":
                              action = GameAction.E134ShakyWalls;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "ShakeyWallsEnd":
                              action = GameAction.E134ShakyWallsEnd;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "SlaveMarketStart":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e163"][2])
                              {
                                 action = GameAction.UpdateEventViewerActive;
                                 myGameInstance.EventStart = myGameInstance.EventDisplayed = myGameInstance.EventActive = "e163a";
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "SlaveMarketEnd":
                              action = GameAction.EncounterLootStart;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "SlaveGirlCheck":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e163c"][0])
                              {
                                 action = GameAction.EncounterLootStart;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "Spectre":
                              action = GameAction.E034CombatSpectre;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "SpectreMagic":
                              action = GameAction.E082SpectreMagic;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "Spiders":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e074"][0])
                              {
                                 action = GameAction.EncounterStart;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              break;
                           case "Snake":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e091"][0])
                              {
                                 action = GameAction.E091PoisonSnake;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "SunStroke":
                              action = GameAction.E121SunStroke;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "TalismanActivate":
                              myGameInstance.EventDisplayed = myGameInstance.EventActive = "e016d";
                              action = GameAction.UpdateEventViewerActive;
                              myGameInstance.DieRollAction = GameAction.EncounterRoll;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "TalismanDestroy":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e016d"][0])
                              {
                                 action = GameAction.EncounterLootStart;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "TalismanPegasus":
                              action = GameAction.E188TalismanPegasusConversion;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "TalismanSave":
                              action = GameAction.E016TalismanSave;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "Temple":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e212"][0])
                              {
                                 action = GameAction.E212Temple;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "TreasureChest":
                              action = GameAction.E039TreasureChest;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "TreasureChest1":
                              action = GameAction.E040TreasureChest;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "TreasureRoomDoor":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e136"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "UnpassableWoods":
                              action = GameAction.E087UnpassableWoods;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "Unpassable":
                              action = GameAction.E089UnpassableMorass;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "Witch":
                              action = GameAction.E073WitchMeet;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "WitchFriend":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e073c"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              break;
                           case "WitchStart":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e073"][0])
                              {
                                 action = GameAction.EncounterStart;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              break;
                           case "WizardAbode":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e068"][0])
                              {
                                 action = GameAction.EncounterStart;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "WizardAdvice":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e025"][1])
                              {
                                 action = GameAction.E023WizardAdvice;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "SearchAdviceEnd":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e026"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "WizardFight":
                              action = GameAction.E024WizardFight;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "WizardCampfire":
                              action = GameAction.E203NightEnslaved;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "WizardPrisoner":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e024a"][0])
                              {
                                 action = GameAction.EncounterRoll;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "WizardSlave":
                              action = GameAction.CampfireWakeup;
                              myGameInstance.GamePhase = GamePhase.Campfire;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "TowerWizard":
                              action = GameAction.E068WizardTower;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "WizardWander":
                              myGameInstance.DieResults["e203e"][0] = Utilities.NO_RESULT;
                              if (false == myGameInstance.IsEnslaved)
                                 action = GameAction.E203EscapeEnslaved;
                              else
                                 action = GameAction.CampfireWakeup;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "WolvesAttack":
                              if (Utilities.NO_RESULT < myGameInstance.DieResults["e075b"][0])
                              {
                                 action = GameAction.EncounterStart;
                                 myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              }
                              return;
                           case "WolvesEncounter":
                              action = GameAction.E075WolvesEncounter;
                              myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
                              return;
                           case "Combat":
                           default:
                              break;// do nothing
                        }
                     }
                  }
               }
            }
         }
         //---------------------------------------------
         // Click anywhere to continue
         switch (myGameInstance.EventActive)
         {
            case "e001a":
               if (Utilities.NO_RESULT < myGameInstance.DieResults["e001a"][0])
               {
                  action = GameAction.SetupFinalize;
                  myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               }
               break;
            case "e033a": // Warrior Wraiths
               if (Utilities.NO_RESULT != myGameInstance.DieResults["e033a"][0]) // if treasure already rolled, click anywhere to continue
               {
                  action = GameAction.EncounterEnd;
                  myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               }
               break;
            case "e034b": // Spectre
               if (Utilities.NO_RESULT != myGameInstance.DieResults["e034b"][0]) // if treasure already rolled, click anywhere to continue
               {
                  action = GameAction.EncounterEnd;
                  myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               }
               break;
            case "e044a": // High Alter
               action = GameAction.EncounterEnd;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "e053": // Campsite Location
               if (Utilities.NO_RESULT < myGameInstance.DieResults["e053"][0])
               {
                  action = GameAction.EncounterRoll;
                  myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               }
               break;
            case "e054b": // Fighting Goblins
               if (Utilities.NO_RESULT < myGameInstance.DieResults["e054b"][0])
               {
                  action = GameAction.EncounterRoll;
                  myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               }
               break;
            case "e056a": // Fighting Orcs
               if (Utilities.NO_RESULT < myGameInstance.DieResults["e056a"][0])
               {
                  action = GameAction.EncounterRoll;
                  myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               }
               break;
            case "e072c":
               action = GameAction.E072DoubleElves;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "e133": // Plague
               action = GameAction.EncounterRoll;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "e135a":
               action = GameAction.EncounterEnd;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "e147": // Clue to Treasure
               if (Utilities.NO_RESULT < myGameInstance.DieResults["e056a"][1])
               {
                  action = GameAction.EncounterEnd;
                  myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               }
               break;
            case "e150": // Pay your respects
               action = GameAction.EncounterEnd;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "e188b":
               action = GameAction.E188TalismanPegasusSkip;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "e203a":
               if ((6 == myGameInstance.DieResults["e203a"][0]) && ("e061" == myGameInstance.EventStart)) // if marked for death
               {
                  action = GameAction.UpdateEventViewerActive;
                  myGameInstance.EventDisplayed = myGameInstance.EventActive = "e203b";
                  myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               }
               break;
            case "e203b": // axe kills
               myGameInstance.DieResults["e203a"][0] = Utilities.NO_RESULT;
               myGameInstance.GamePhase = GamePhase.EndGame;
               if (true == myGameInstance.Prince.IsSpecialItemHeld(SpecialEnum.ResurrectionNecklace))
                  action = GameAction.EndGameResurrect;  // axe kills
               else
                  action = GameAction.EndGameLost; // axe kills
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "e300":
            case "e301":
            case "e302":
            case "e303":
            case "e304":
            case "e305":
            case "e306":
            case "e307":
            case "e308":
            case "e309":
            case "e310":
               action = GameAction.EncounterCombat;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "e311":  // escape
            case "e312":  // escape mounted
               action = GameAction.EncounterEscape;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "e312a":
            case "e312b":
               action = GameAction.EncounterRoll;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "e312c":
            case "e313":
               action = GameAction.EncounterEnd;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "e313a":
            case "e313b":
               action = GameAction.EncounterRoll;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "e313c":
               action = GameAction.EncounterEscapeFly;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "e316":
               action = GameAction.EncounterEnd;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "e321":
            case "e322":
            case "e323":
            case "e324":
               action = GameAction.EncounterRoll;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "e325":
               action = GameAction.EncounterEnd;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "e503a":
               action = GameAction.Error;
               if (GamePhase.Campfire == myGameInstance.GamePhase)
               {
                  if (false == GameEngine.theFeatsInGame.IsEqual(GameEngine.theFeatsInGameStarting))
                  {
                     action = GameAction.CampfireShowFeat;
                  }
                  else
                  {
                     action = GameAction.CampfireShowFeatEnd;
                     myCanvas.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas);
                  }
               }
               else
               {
                  if (false == GameEngine.theFeatsInGame.IsEqual(GameEngine.theFeatsInGameStarting))
                     action = GameAction.EndGameShowFeats;
                  else
                     action = GameAction.EndGameShowStats;
               }
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            default:
               break;
         }
      }
      private void Button_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.Error;
         Button b = (Button)sender;
         e.Handled = true;
         string key = (string)b.Content;
         if ("HirelingCount" == b.Name) // if this button is a number, it indictes hiring Hirelings
         {
            action = GameAction.E333HirelingCount;
            int numHirelings = Int32.Parse(key);
            myGameEngine.PerformAction(ref myGameInstance, ref action, numHirelings);
         }
         else if (true == key.StartsWith("r")) // rules based click
         {
            if (false == ShowRule(key))
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click(): ShowRule() returned false");
               return;
            }
         }
         else if (true == key.StartsWith("t")) // rules based click
         {
            if (false == ShowTable(key))
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click(): ShowTable() returned false");
               return;
            }
         }
         else if (true == key.StartsWith("e")) // event based click
         {
            myGameInstance.EventDisplayed = key;
            action = GameAction.UpdateEventViewerDisplay;
            myGameEngine.PerformAction(ref myGameInstance, ref action);
         }
         else if ((true == myGameInstance.IsGiftCharmActive) || (true == myGameInstance.IsSlaveGirlActive) || (true == myGameInstance.IsLadyAeravirRerollActive)) // User made a selection of one of three die rolls
         {
            string buttonContent = (string)b.Content;
            int dieRoll = Int32.Parse(buttonContent);
            GameAction outAction = GameAction.EncounterRoll;
            myGameEngine.PerformAction(ref myGameInstance, ref outAction, dieRoll);
            myGameInstance.IsGiftCharmActive = false;
            myGameInstance.IsSlaveGirlActive = false;
            return;
         }
         else
         {
            if (false == Button_ClickShowOther(key, b.Name, out action))
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click(): CloseEvent() return false");
               return;
            }
         }
      }
      private bool Button_ClickShowOther(string content, string name, out GameAction action)
      {
         action = GameAction.Error;
         switch (content)
         {
            case "  -  ":
               switch (myGameInstance.EventActive)
               {
                  case "e000d":
                     action = GameAction.SetupManualWitsWiles;
                     break;
                  case "e011a":
                  case "e012a":
                  case "e128c":
                     action = GameAction.E012FoodChange;
                     break;
                  case "e128b":
                     action = GameAction.E128bPotionCureChange;
                     break;
                  case "e128e":
                  case "e129b":
                     action = GameAction.E128ePotionHealChange;
                     break;
                  case "e128f":
                  case "e129c":  // Button_ClickShowOther() - subtract
                  case "e210d":
                  case "e210g":
                     action = GameAction.E015MountChange;
                     break;
                  case "e015b":
                     if ("FoodMinus" == name)
                        action = GameAction.E012FoodChange;
                     else
                        action = GameAction.E015MountChange;
                     break;
                  case "e163a":
                     if ("SlavePorterMinus" == name)
                        action = GameAction.E163SlavePorterChange;
                     else if ("SlaveGirlMinus" == name)
                        action = GameAction.E163SlaveGirlChange;
                     else if ("SlaveWarriorMinus" == name)
                        action = GameAction.E163SlaveWarriorChange;
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetButtonState(): e163- with name=" + name);
                     break;
                  case "e210f":
                     --myGameInstance.PurchasedHenchman;
                     action = GameAction.UpdateEventViewerActive;
                     break;
                  case "e210i":
                     if ("PorterMinus" == name)
                        --myGameInstance.PurchasedPorter; // each purchase will add two porter
                     else if ("LocalGuideMinus" == name)
                        --myGameInstance.PurchasedGuide;
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetButtonState(): e210i- with name=" + name);
                     action = GameAction.UpdateEventViewerActive;
                     break;
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "Button_ClickShowOther(): Reached default c=" + content + " ae=" + myGameInstance.EventActive + " a=" + action.ToString());
                     return false;
               }
               myGameEngine.PerformAction(ref myGameInstance, ref action, -1);
               break;
            case "  +  ":
               switch (myGameInstance.EventActive)
               {
                  case "e000d":
                     action = GameAction.SetupManualWitsWiles;
                     break;
                  case "e011a":
                  case "e012a":
                  case "e128c":
                     action = GameAction.E012FoodChange;
                     break;
                  case "e128b":
                     action = GameAction.E128bPotionCureChange;
                     break;
                  case "e128e":
                  case "e129b":
                     action = GameAction.E128ePotionHealChange;
                     break;
                  case "e128f":
                  case "e129c":  // Button_ClickShowOther() - add
                  case "e210d":
                  case "e210g":
                     action = GameAction.E015MountChange;
                     break;
                  case "e015b":
                     if ("FoodPlus" == name)
                        action = GameAction.E012FoodChange;
                     else
                        action = GameAction.E015MountChange;
                     break;
                  case "e163a":
                     if ("SlavePorterPlus" == name)
                        action = GameAction.E163SlavePorterChange;
                     else if ("SlaveGirlPlus" == name)
                        action = GameAction.E163SlaveGirlChange;
                     else if ("SlaveWarriorPlus" == name)
                        action = GameAction.E163SlaveWarriorChange;
                     else
                        Logger.Log(LogEnum.LE_ERROR, "Button_ClickShowOther(): e163+ with name=" + name);
                     break;
                  case "e210f":
                     ++myGameInstance.PurchasedHenchman;
                     action = GameAction.UpdateEventViewerActive;
                     break;
                  case "e210i":
                     if ("PorterPlus" == name)
                        ++myGameInstance.PurchasedPorter;
                     else if ("LocalGuidePlus" == name)
                        ++myGameInstance.PurchasedGuide;
                     else
                        Logger.Log(LogEnum.LE_ERROR, "SetButtonState(): e210i+ with name=" + name);
                     action = GameAction.UpdateEventViewerActive;
                     break;
                  default:
                     Logger.Log(LogEnum.LE_ERROR, "Button_ClickShowOther(): Reached default c=" + content + " ae=" + myGameInstance.EventActive + " a=" + action.ToString());
                     return false;
               }
               myGameEngine.PerformAction(ref myGameInstance, ref action, +1);
               break;
            case "Abandon":
               action = GameAction.EncounterAbandon;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Accept Roll":
               action = GameAction.EncounterRoll;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Avoid":
               action = GameAction.EncounterEnd;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Begin Game":
               action = GameAction.SetupShowCalArath;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Bribe":
               action = GameAction.EncounterBribe;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Carry":
               action = GameAction.E069WoundedWarriorCarry;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case " Charm ":
               action = GameAction.E144RescueCharm;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case " Cast  ":
               action = GameAction.E144RescueCast;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Continue":
               action = GameAction.E144ContinueNormalAudienceRoll;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Count Drogat ":
               action = GameAction.E042CountDrogatAudience;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Detour":
               action = GameAction.E009FarmDetour;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Dismiss":
               action = GameAction.E211DismissMagicUser;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Dismount":
               action = GameAction.E079HeavyRainsDismount;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Escape":
               action = GameAction.EncounterEscape;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Engage":
               action = GameAction.EncounterStart;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Evade":
               myGameInstance.EventDisplayed = myGameInstance.EventActive = name;
               switch (myGameInstance.EventActive)
               {
                  case "e003b":
                  case "e023b":
                  case "e071b":
                  case "e099a":
                  case "e100b":
                  case "e101b":
                  case "e112b":
                  case "e118b":
                     action = GameAction.EncounterStart;
                     break;
                  case "e006d":
                     action = GameAction.E006DwarfEvade;
                     break;
                  case "e007d":
                     action = GameAction.E007ElfEvade;
                     break;
                  default:
                     action = GameAction.UpdateEventViewerActive;
                     break;
               }
               myGameInstance.DieRollAction = GameAction.EncounterRoll;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Feed":
               action = GameAction.E107FalconAdd;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Fight":
               switch (myGameInstance.EventActive)
               {
                  case "e003":
                  case "e018":
                  case "e023":
                  case "e071":
                  case "e099":
                  case "e100":
                  case "e101":
                  case "e112":
                  case "e118":
                     myGameInstance.EventDisplayed = myGameInstance.EventActive = name;
                     action = GameAction.EncounterStart;
                     break;
                  case "e006":
                     action = GameAction.E006DwarfFight;
                     myGameInstance.EventDisplayed = myGameInstance.EventActive = name;
                     break;
                  case "e007":
                     action = GameAction.E007ElfFight;
                     myGameInstance.EventDisplayed = myGameInstance.EventActive = name;
                     break;
                  case "e008":
                     action = GameAction.EncounterStart;
                     break;
                  case "e053a":
                     action = GameAction.E053CampsiteFight;
                     break;
                  case "e054":
                  case "e054a":
                     myGameInstance.EventDisplayed = myGameInstance.EventActive = "e054b";
                     myGameInstance.DieRollAction = GameAction.EncounterStart;
                     action = GameAction.UpdateEventViewerActive;
                     break;
                  case "e056":
                     myGameInstance.EventDisplayed = myGameInstance.EventActive = "e056a";
                     myGameInstance.DieRollAction = GameAction.EncounterStart;
                     action = GameAction.UpdateEventViewerActive;
                     break;
                  default:
                     myGameInstance.EventDisplayed = myGameInstance.EventActive = name;
                     myGameInstance.DieRollAction = GameAction.EncounterRoll;
                     action = GameAction.UpdateEventViewerActive;
                     break;
               }
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case " Fight ":
               action = GameAction.E144RescueFight;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Fly":
               action = GameAction.EncounterEscape;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case " Fly ":
               action = GameAction.EncounterFollow;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Fly Away ":
               action = GameAction.EncounterEscapeFly;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Follow":
               action = GameAction.EncounterFollow;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Friendly":
               myGameInstance.EventDisplayed = myGameInstance.EventActive = name;
               action = GameAction.UpdateEventViewerActive;
               myGameInstance.DieRollAction = GameAction.EncounterRoll;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case " Fun Options ":
               action = GameAction.SetupChooseFunOptions;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Give":
               action = GameAction.EncounterStart;  // traveling minstrel
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case " Go ":
               action = GameAction.EncounterRoll;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Halt":
               action = GameAction.E078BadGoingHalt;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Heal":
               action = GameAction.E123WoundedBlackKnightRemain;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Hide":
               action = GameAction.EncounterHide;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "High Priest  ":
               action = GameAction.E042HighPriestAudience;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Ignore":
               action = GameAction.EncounterEnd;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Impress":
               action = GameAction.E144RescueImpress;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Join":
               action = GameAction.UpdateEventViewerActive;
               myGameInstance.EventDisplayed = myGameInstance.EventActive = "e209f";
               action = GameAction.EncounterRoll;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case " Kill  ":
               action = GameAction.E144RescueKill;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Inquiries":
               action = GameAction.EncounterInquiry;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Lady Aeravir ":
               action = GameAction.E042LadyAeravirAudience;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Land":
               action = GameAction.E105StormCloudLand;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Mayor of Town":
               action = GameAction.E042MayorAudience;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Original Game":
               action = GameAction.SetupStartingLocation;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Pass ":
            case "Pass":
               action = GameAction.EncounterEnd;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Pay":
               action = GameAction.E209ThievesGuiildPay;
               myGameInstance.GamePhase = GamePhase.Encounter;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Perform":
               action = GameAction.UpdateEventViewerActive;
               myGameInstance.EventDisplayed = myGameInstance.EventActive = "e123b";
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Refuse":
               action = GameAction.UpdateEventViewerActive;
               myGameInstance.EventDisplayed = myGameInstance.EventActive = "e123a";
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "  Raid  ":
               myGameInstance.EventDisplayed = myGameInstance.EventActive = name;
               action = GameAction.UpdateEventViewerActive;
               myGameInstance.DieRollAction = GameAction.EncounterRoll;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Remain":
               action = GameAction.E069WoundedWarriorRemain;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Ride Away":
               action = GameAction.EncounterEscapeMounted;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Skip":
               action = GameAction.EncounterEnd;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Spend":
               action = GameAction.E212TempleTenGold;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Read Rules":
               if (false == ShowRule("r200"))
               {
                  Logger.Log(LogEnum.LE_ERROR, "Button_ClickShowOther(): ShowRule(r200) returned false");
                  return false;
               }
               break;
            case "Request":
               action = GameAction.E212TempleRequestClues;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Roll Again":
               action = GameAction.E146CountAudienceReroll;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Short Hop":
               action = GameAction.TravelShortHop;
               myGameInstance.DieRollAction = GameAction.DieRollActionNone;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Surrender":
               action = GameAction.EncounterSurrender;
               myGameInstance.DieRollAction = GameAction.DieRollActionNone;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Talk ":
               myGameInstance.DieResults["e341"][0] = Utilities.NO_RESULT;
               myGameInstance.EventDisplayed = myGameInstance.EventActive = name; // open the event that corresponds to the talk button: "Talk -> name" on the display
               switch (myGameInstance.EventActive)
               {
                  case "e003a":
                  case "e008a":
                  case "e018a":
                  case "e023a":
                  case "e071a":
                  case "e100a":
                  case "e101a":
                  case "e118a":
                     action = GameAction.EncounterStart;
                     break;
                  case "e006c":
                     action = GameAction.E006DwarfTalk;
                     break;
                  case "e007c":
                     action = GameAction.E007ElfTalk;
                     break;
                  default:
                     action = GameAction.UpdateEventViewerActive;
                     myGameInstance.DieRollAction = GameAction.EncounterRoll;
                     break;
               }
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Dienstal Branch":
               action = GameAction.ShowDienstalBranch;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Largos River":
               action = GameAction.ShowLargosRiver;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Nesser River":
               action = GameAction.ShowNesserRiver;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "Trogoth River":
               action = GameAction.ShowTrogothRiver;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            case "  Use  ":
               action = GameAction.E212TempleRequestInfluence;
               myGameEngine.PerformAction(ref myGameInstance, ref action, 0);
               break;
            default:
               if (false == ShowRegion(content))
               {
                  Logger.Log(LogEnum.LE_ERROR, "Button_ClickShowOther(): ShowRegion() return false c=" + content + " ae=" + myGameInstance.EventActive + " a=" + action.ToString());
                  return false;
               }
               break;
         }
         return true;
      }
   }
}
