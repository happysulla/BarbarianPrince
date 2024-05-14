using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

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
         myHeaderNames.Add("08-River->Farmland");
         myHeaderNames.Add("08-CountrySide->Farmland");
         myHeaderNames.Add("08-CountrySide->CountrySide");
         myHeaderNames.Add("08-CountrySide->Forest");
         myHeaderNames.Add("08-Mtn->Hills");
         myHeaderNames.Add("08-Mtn->Mtn");
         myHeaderNames.Add("08-Countryside->Swamp");
         myHeaderNames.Add("08-Hills->Desert");
         myHeaderNames.Add("08-Forest->Swamp");
         myHeaderNames.Add("08-Forest->Swamp");
         myHeaderNames.Add("08-Forest->Swamp");
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
         if (CommandName == myCommandNames[0]) // Cross River
         {
            AddPrince(ref gi);
            //AddGuide(ref gi, "1822");
            gi.MapItemMoves.Clear();
            ITerritory t1 = gi.Territories.Find("1723");
            ITerritory t2 = gi.Territories.Find("1822");
            IMapItemMove mim = new MapItemMove(t1, t2);
            mim.MapItem = myGameInstance.Prince;
            mim.RiverCross = RiverCrossEnum.TC_NO_RIVER;
            gi.MapItemMoves.Add(mim);
            myEventViewer.UpdateView(ref gi, GameAction.TravelLostCheck);
         }
         else if (CommandName == myCommandNames[1]) // farmland
         {
            AddPrince(ref gi);
            gi.MapItemMoves.Clear();
            ITerritory t1 = gi.Territories.Find("0417");
            ITerritory t2 = gi.Territories.Find("0418");
            IMapItemMove mim = new MapItemMove(t1, t2);
            mim.MapItem = myGameInstance.Prince;
            gi.MapItemMoves.Add(mim);
            myEventViewer.UpdateView(ref gi, GameAction.TravelLostCheck);
         }
         else if (CommandName == myCommandNames[2])  // countryside
         {
            AddPrince(ref gi);
            gi.MapItemMoves.Clear();
            ITerritory t1 = gi.Territories.Find("0415");
            ITerritory t2 = gi.Territories.Find("0416");
            IMapItemMove mim = new MapItemMove(t1, t2);
            mim.MapItem = myGameInstance.Prince;
            gi.MapItemMoves.Add(mim);
            myEventViewer.UpdateView(ref gi, GameAction.TravelLostCheck);
         }
         else if (CommandName == myCommandNames[3]) // Forest
         {
            AddPrince(ref gi);
            gi.MapItemMoves.Clear();
            ITerritory t1 = gi.Territories.Find("0413");
            ITerritory t2 = gi.Territories.Find("0414");
            IMapItemMove mim = new MapItemMove(t1, t2);
            mim.MapItem = myGameInstance.Prince;
            gi.MapItemMoves.Add(mim);
            myEventViewer.UpdateView(ref gi, GameAction.TravelLostCheck);
         }
         else if (CommandName == myCommandNames[4]) // Hills
         {
            AddPrince(ref gi);
            gi.MapItemMoves.Clear();
            ITerritory t1 = gi.Territories.Find("0405");
            ITerritory t2 = gi.Territories.Find("0406");
            IMapItemMove mim = new MapItemMove(t1, t2);
            mim.MapItem = myGameInstance.Prince;
            gi.MapItemMoves.Add(mim);
            myEventViewer.UpdateView(ref gi, GameAction.TravelLostCheck);
         }
         else if (CommandName == myCommandNames[5]) // Mountains
         {
            AddPrince(ref gi);
            gi.MapItemMoves.Clear();
            ITerritory t1 = gi.Territories.Find("0404");
            ITerritory t2 = gi.Territories.Find("0405");
            IMapItemMove mim = new MapItemMove(t1, t2);
            mim.MapItem = myGameInstance.Prince;
            gi.MapItemMoves.Add(mim);
            myEventViewer.UpdateView(ref gi, GameAction.TravelLostCheck);
         }
         else if (CommandName == myCommandNames[6]) // Swamp
         {
            AddPrince(ref gi);
            gi.MapItemMoves.Clear();
            ITerritory t1 = gi.Territories.Find("0412");
            ITerritory t2 = gi.Territories.Find("0411");
            IMapItemMove mim = new MapItemMove(t1, t2);
            mim.MapItem = myGameInstance.Prince;
            gi.MapItemMoves.Add(mim);
            myEventViewer.UpdateView(ref gi, GameAction.TravelLostCheck);
         }
         else if (CommandName == myCommandNames[7]) // Desert
         {
            AddPrince(ref gi);
            gi.MapItemMoves.Clear();
            ITerritory t1 = gi.Territories.Find("0406");
            ITerritory t2 = gi.Territories.Find("0407");
            IMapItemMove mim = new MapItemMove(t1, t2);
            mim.MapItem = myGameInstance.Prince;
            gi.MapItemMoves.Add(mim);
            myEventViewer.UpdateView(ref gi, GameAction.TravelLostCheck);
         }
         else if (CommandName == myCommandNames[8]) // Airborne
         {
            AddPrince(ref gi);
            gi.MapItemMoves.Clear();
            ITerritory t1 = gi.Territories.Find("0314");
            ITerritory t2 = gi.Territories.Find("0313");
            IMapItemMove mim = new MapItemMove(t1, t2);
            mim.MapItem = myGameInstance.Prince;
            gi.MapItemMoves.Add(mim);
            gi.IsAirborne = true;
            myEventViewer.UpdateView(ref gi, GameAction.TravelLostCheck);
         }
         else if (CommandName == myCommandNames[9]) // One Guide
         {
            AddPrince(ref gi);
            AddGuide(ref gi, "0313");
            gi.MapItemMoves.Clear();
            ITerritory t1 = gi.Territories.Find("0314");
            ITerritory t2 = gi.Territories.Find("0313");
            IMapItemMove mim = new MapItemMove(t1, t2);
            mim.MapItem = myGameInstance.Prince;
            gi.MapItemMoves.Add(mim);
            gi.IsAirborne = false;
            myEventViewer.UpdateView(ref gi, GameAction.TravelLostCheck);
         }
         else if (CommandName == myCommandNames[10]) // Three Guide
         {
            AddPrince(ref gi);
            AddGuides(ref gi);
            gi.MapItemMoves.Clear();
            ITerritory t1 = gi.Territories.Find("0314");
            ITerritory t2 = gi.Territories.Find("0313");
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
      //-----------------------------------------------------------
      private void AddPrince(ref IGameInstance gi)
      {
         IMapItems partyMembers = gi.PartyMembers;
         partyMembers.Clear();
         IMapItem prince = gi.MapItems.Find("Prince");
         if (null == prince)
            Logger.Log(LogEnum.LE_ERROR, "AddPrince(): mi=null");
         prince.Reset();
         partyMembers.Add(prince);
      }
      private void AddGuide(ref IGameInstance gi, string territoryName)
      {
         IMapItems partyMembers = gi.PartyMembers;
         IMapItem guide = gi.MapItems.Find("Dwarf");
         if (null == guide)
            Logger.Log(LogEnum.LE_ERROR, "AddGuide(): mi=null");
         guide.Reset();
         ITerritory t2 = gi.Territories.Find(territoryName);
         if (null == t2)
            Logger.Log(LogEnum.LE_ERROR, "AddGuide(): t2=null");
         guide.GuideTerritories.Add(t2);
         partyMembers.Add(guide);
      }
      private void AddGuides(ref IGameInstance gi)
      {
         IMapItems partyMembers = gi.PartyMembers;
         IMapItem guide1 = gi.MapItems.Find("Dwarf");
         if (null == guide1)
            Logger.Log(LogEnum.LE_ERROR, "AddGuides(): mi=null");
         guide1.Reset();
         ITerritory t2 = gi.Territories.Find("0313");
         guide1.GuideTerritories.Add(t2);
         partyMembers.Add(guide1);
         //---------------------------------------
         IMapItem guide2 = gi.MapItems.Find("Runaway");
         if (null == guide2)
            Logger.Log(LogEnum.LE_ERROR, "AddGuides(): mi=null");
         guide2.Reset();
         guide2.GuideTerritories.Add(t2);
         partyMembers.Add(guide2);
         //---------------------------------------
         IMapItem guide3 = gi.MapItems.Find("Mercenary");
         if (null == guide3)
            Logger.Log(LogEnum.LE_ERROR, "AddGuides(): mi=null");
         guide3.Reset();
         guide3.GuideTerritories.Add(t2);
         partyMembers.Add(guide3);
         //---------------------------------------
      }
   }
}
