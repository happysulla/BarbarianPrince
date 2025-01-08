﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace BarbarianPrince
{
   public class TravelCheckUnitTest : IUnitTest
   {
      public bool CtorError { get; } = false;
      //-----------------------------------------------------------
      private EventViewer myEventViewer = null;
      private IGameInstance myGameInstance = null;
      private Canvas myCanvas = null;
      private readonly List<Button> myButtons = new List<Button>();
      //-----------------------------------------------------------
      private int myIndexName = 0;
      private List<string> myHeaderNames = new List<string>();
      private List<string> myCommandNames = new List<string>();
      public string HeaderName { get { return myHeaderNames[myIndexName]; } }
      public string CommandName { get { return myCommandNames[myIndexName]; } }
      //-----------------------------------------------------------
      public TravelCheckUnitTest(DockPanel dp, IGameInstance gi, EventViewer ev)
      {
         //------------------------------------------
         myIndexName = 0;
         myHeaderNames.Add("09-River->Farmland");
         myHeaderNames.Add("09-CountrySide->Farmland");
         myHeaderNames.Add("09-CountrySide->CountrySide");
         myHeaderNames.Add("09-CountrySide->Forest");
         myHeaderNames.Add("09-Mtn->Hills");
         myHeaderNames.Add("09-Mtn->Mtn");
         myHeaderNames.Add("09-Countryside->Swamp");
         myHeaderNames.Add("09-Hills->Desert");
         myHeaderNames.Add("09-Forest->Swamp");
         myHeaderNames.Add("09-Forest->Swamp");
         myHeaderNames.Add("09-Forest->Swamp");
         //------------------------------------------
         myCommandNames.Add("Cross River");
         myCommandNames.Add("Farmland");
         myCommandNames.Add("CountrySide");
         myCommandNames.Add("Forest");
         myCommandNames.Add("Hills");
         myCommandNames.Add("Mountains");
         myCommandNames.Add("Swamp");
         myCommandNames.Add("Desert");
         myCommandNames.Add("Airborne");
         myCommandNames.Add("One Guide");
         myCommandNames.Add("Three Guide");
         myCommandNames.Add("Finish");
         //------------------------------------------
         if (null == gi)
         {
            Logger.Log(LogEnum.LE_ERROR, "CampfireUnitTest(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         if (null == ev)
         {
            Logger.Log(LogEnum.LE_ERROR, "CampfireUnitTest(): ev=null");
            CtorError = true;
            return;
         }
         myEventViewer = ev;
         //------------------------------------------
         foreach (UIElement ui0 in dp.Children)
         {
            if (ui0 is DockPanel dockPanelInside)
            {
               foreach (UIElement ui1 in dockPanelInside.Children)
               {
                  if (ui1 is ScrollViewer sv)
                  {
                     if (sv.Content is Canvas canvas)
                        myCanvas = canvas;  // Find the Canvas in the visual tree
                  }
               }
            }
         }
         if (null == myCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "MapItemSetupUnitTest(): myCanvas=null");
            CtorError = true;
            return;
         }
      }
      public bool Command(ref IGameInstance gi)
      {
         gi.Prince.Reset();
         if (CommandName == myCommandNames[0]) // Cross River
         {
            //AddGuide(ref gi, "1822");
            gi.MapItemMoves.Clear();
            ITerritory t1 = Territory.theTerritories.Find("1723");
            ITerritory t2 = Territory.theTerritories.Find("1822");
            IMapItemMove mim = new MapItemMove(t1, t2);
            mim.MapItem = myGameInstance.Prince;
            mim.RiverCross = RiverCrossEnum.TC_NO_RIVER;
            gi.MapItemMoves.Add(mim);
            myEventViewer.UpdateView(ref gi, GameAction.TravelLostCheck);
         }
         else if (CommandName == myCommandNames[1]) // farmland
         {
            gi.MapItemMoves.Clear();
            ITerritory t1 = Territory.theTerritories.Find("0417");
            ITerritory t2 = Territory.theTerritories.Find("0418");
            IMapItemMove mim = new MapItemMove(t1, t2);
            mim.MapItem = myGameInstance.Prince;
            gi.MapItemMoves.Add(mim);
            myEventViewer.UpdateView(ref gi, GameAction.TravelLostCheck);
         }
         else if (CommandName == myCommandNames[2])  // countryside
         {
            gi.MapItemMoves.Clear();
            ITerritory t1 = Territory.theTerritories.Find("0415");
            ITerritory t2 = Territory.theTerritories.Find("0416");
            IMapItemMove mim = new MapItemMove(t1, t2);
            mim.MapItem = myGameInstance.Prince;
            gi.MapItemMoves.Add(mim);
            myEventViewer.UpdateView(ref gi, GameAction.TravelLostCheck);
         }
         else if (CommandName == myCommandNames[3]) // Forest
         {
            gi.MapItemMoves.Clear();
            ITerritory t1 = Territory.theTerritories.Find("0413");
            ITerritory t2 = Territory.theTerritories.Find("0414");
            IMapItemMove mim = new MapItemMove(t1, t2);
            mim.MapItem = myGameInstance.Prince;
            gi.MapItemMoves.Add(mim);
            myEventViewer.UpdateView(ref gi, GameAction.TravelLostCheck);
         }
         else if (CommandName == myCommandNames[4]) // Hills
         {
            gi.MapItemMoves.Clear();
            ITerritory t1 = Territory.theTerritories.Find("0405");
            ITerritory t2 = Territory.theTerritories.Find("0406");
            IMapItemMove mim = new MapItemMove(t1, t2);
            mim.MapItem = myGameInstance.Prince;
            gi.MapItemMoves.Add(mim);
            myEventViewer.UpdateView(ref gi, GameAction.TravelLostCheck);
         }
         else if (CommandName == myCommandNames[5]) // Mountains
         {
            gi.MapItemMoves.Clear();
            ITerritory t1 = Territory.theTerritories.Find("0404");
            ITerritory t2 = Territory.theTerritories.Find("0405");
            IMapItemMove mim = new MapItemMove(t1, t2);
            mim.MapItem = myGameInstance.Prince;
            gi.MapItemMoves.Add(mim);
            myEventViewer.UpdateView(ref gi, GameAction.TravelLostCheck);
         }
         else if (CommandName == myCommandNames[6]) // Swamp
         {
            gi.MapItemMoves.Clear();
            ITerritory t1 = Territory.theTerritories.Find("0412");
            ITerritory t2 = Territory.theTerritories.Find("0411");
            IMapItemMove mim = new MapItemMove(t1, t2);
            mim.MapItem = myGameInstance.Prince;
            gi.MapItemMoves.Add(mim);
            myEventViewer.UpdateView(ref gi, GameAction.TravelLostCheck);
         }
         else if (CommandName == myCommandNames[7]) // Desert
         {
            gi.MapItemMoves.Clear();
            ITerritory t1 = Territory.theTerritories.Find("0406");
            ITerritory t2 = Territory.theTerritories.Find("0407");
            IMapItemMove mim = new MapItemMove(t1, t2);
            mim.MapItem = myGameInstance.Prince;
            gi.MapItemMoves.Add(mim);
            myEventViewer.UpdateView(ref gi, GameAction.TravelLostCheck);
         }
         else if (CommandName == myCommandNames[8]) // Airborne
         {
            gi.MapItemMoves.Clear();
            ITerritory t1 = Territory.theTerritories.Find("0314");
            ITerritory t2 = Territory.theTerritories.Find("0313");
            IMapItemMove mim = new MapItemMove(t1, t2);
            mim.MapItem = myGameInstance.Prince;
            gi.MapItemMoves.Add(mim);
            gi.IsAirborne = true;
            myEventViewer.UpdateView(ref gi, GameAction.TravelLostCheck);
         }
         else if (CommandName == myCommandNames[9]) // One Guide
         {
            AddGuide(ref gi, "0313");
            gi.MapItemMoves.Clear();
            ITerritory t1 = Territory.theTerritories.Find("0314");
            ITerritory t2 = Territory.theTerritories.Find("0313");
            IMapItemMove mim = new MapItemMove(t1, t2);
            mim.MapItem = myGameInstance.Prince;
            gi.MapItemMoves.Add(mim);
            gi.IsAirborne = false;
            myEventViewer.UpdateView(ref gi, GameAction.TravelLostCheck);
         }
         else if (CommandName == myCommandNames[10]) // Three Guide
         {
            AddGuides(ref gi);
            gi.MapItemMoves.Clear();
            ITerritory t1 = Territory.theTerritories.Find("0314");
            ITerritory t2 = Territory.theTerritories.Find("0313");
            IMapItemMove mim = new MapItemMove(t1, t2);
            mim.MapItem = myGameInstance.Prince;
            gi.MapItemMoves.Add(mim);
            gi.IsAirborne = false;
            myEventViewer.UpdateView(ref gi, GameAction.TravelLostCheck);
         }
         else if (CommandName == myCommandNames[11])
         {

         }
         return true;
      }
      public bool NextTest(ref IGameInstance gi)
      {
         if (HeaderName == myHeaderNames[0])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[1])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[2])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[3])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[4])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[5])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[6])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[7])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[8])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[9])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[10])
         {
            if (false == Cleanup(ref gi))
               Logger.Log(LogEnum.LE_ERROR, "NextTest(): Cleanup returned error");
         }
         else if (HeaderName == myHeaderNames[11])
         {

         }
         return true;
      }
      public bool Cleanup(ref IGameInstance gi)
      {
         ++gi.GameTurn;
         return true;
      }
      private void AddGuide(ref IGameInstance gi, string territoryName)
      {
         IMapItems partyMembers = gi.PartyMembers;
         string miName = "Dwarf" + Utilities.MapItemNum;
         Utilities.MapItemNum++;
         IMapItem guide= new MapItem(miName, 1.0,  false, false,"c08Dwarf", "c08Dwarf", gi.Prince.Territory, 6, 5, 12);;
         ITerritory t2 = Territory.theTerritories.Find(territoryName);
         if (null == t2)
            Logger.Log(LogEnum.LE_ERROR, "AddGuide(): t2=null");
         guide.GuideTerritories.Add(t2);
         partyMembers.Add(guide);
      }
      private void AddGuides(ref IGameInstance gi)
      {
         IMapItems partyMembers = gi.PartyMembers;
         string miName = "Dwarf" + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         IMapItem guide1= new MapItem(miName, 1.0,  false, false,"c08Dwarf", "c08Dwarf", gi.Prince.Territory, 6, 5, 12); ;
         ITerritory t2 = Territory.theTerritories.Find("0313");
         guide1.GuideTerritories.Add(t2);
         partyMembers.Add(guide1);
         //---------------------------------------
         miName = "RunAway" + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         IMapItem guide2= new MapItem(miName, 1.0,  false, false,"c09Runaway", "c09Runaway", gi.Prince.Territory, 4, 4, 0);
         guide2.GuideTerritories.Add(t2);
         partyMembers.Add(guide2);
         //---------------------------------------
         miName = "Mercenary" + Utilities.MapItemNum.ToString();
         Utilities.MapItemNum++;
         IMapItem guide3= new MapItem(miName, 1.0,  false, false,"c10Mercenary", "c10Mercenary", gi.Prince.Territory, 4, 5, 4);
         guide3.GuideTerritories.Add(t2);
         partyMembers.Add(guide3);
         //---------------------------------------
      }
   }
}
