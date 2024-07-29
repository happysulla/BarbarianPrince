using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace BarbarianPrince
{
   public class GameEngine : IGameEngine
   {
      //---------------------------------------------------------------------
      static public TreasureMgr theTreasureMgr = new TreasureMgr();
      //---------------------------------------------------------------------
      private readonly MainWindow myMainWindow = null;
      private readonly List<IView> myViews = new List<IView>();
      public List<IView> Views
      {
         get { return myViews; }
      }
      //---------------------------------------------------------------
      public GameEngine(MainWindow mainWindow)
      {
         myMainWindow = mainWindow;
      }
      public void RegisterForUpdates(IView view)
      {
         myViews.Add(view);
      }
      public void PerformAction(ref IGameInstance gi, ref GameAction action, int dieRoll)
      {
         IGameState state = GameState.GetGameState(gi.GamePhase); // First ge the current game state. Then call performNextAction() on the game state.
         if (null == state)
         {
            Logger.Log(LogEnum.LE_ERROR, "GameEngine.PerformAction(): s=null for p=" + gi.GamePhase.ToString());
            return;
         }
         string returnStatus = state.PerformAction(ref gi, ref action, dieRoll); // Perform the next action
         if ("OK" != returnStatus)
         {
            StringBuilder sb1 = new StringBuilder("<<<<ERROR3:::::: GameEngine.PerformAction(): ");
            sb1.Append(" a="); sb1.Append(action.ToString());
            sb1.Append(" dr="); sb1.Append(dieRoll.ToString());
            sb1.Append(" r="); sb1.Append(returnStatus);
            Logger.Log(LogEnum.LE_ERROR, sb1.ToString());
         }
         myMainWindow.UpdateViews(ref gi, action); // Update all registered views when performNextAction() is called
      }
      public bool CreateUnitTests(IGameInstance gi, DockPanel dp, EventViewer ev, IDieRoller dr)
      {
         //-----------------------------------------------------------------------------
         IUnitTest ut8 = new GameInstanceUnitTest(gi);
         if (true == ut8.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateUnitTests(): GameInstanceUnitTest() ctor error");
            return false;
         }
         gi.UnitTests.Add(ut8);
         //-----------------------------------------------------------------------------
         IUnitTest ut1 = new GameViewerCreateUnitTest(dp);
         if (true == ut1.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateUnitTests(): GameViewerCreateUnitTest() ctor error");
            return false;
         }
         gi.UnitTests.Add(ut1);
         //-----------------------------------------------------------------------------
         IUnitTest ut2 = new TerritoryCreateUnitTest(dp, gi);
         if (true == ut2.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateUnitTests(): TerritoryCreateUnitTest() ctor error");
            return false;
         }
         gi.UnitTests.Add(ut2);
         //-----------------------------------------------------------------------------
         IUnitTest ut3 = new TerritoryRegionUnitTest(dp, gi);
         if (true == ut3.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateUnitTests(): TerritoryRegionUnitTest() ctor error");
            return false;
         }
         gi.UnitTests.Add(ut3);
         //-----------------------------------------------------------------------------
         IUnitTest ut4 = new PolylineCreateUnitTest(dp, gi);
         if (true == ut4.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateUnitTests(): PolylineCreateUnitTest() ctor error");
            return false;
         }
         gi.UnitTests.Add(ut4);
         //-----------------------------------------------------------------------------
         IUnitTest ut5 = new ConfigMgrUnitTest(dp, ev);
         if (true == ut5.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateUnitTests(): ConfigMgrUnitTest() ctor error");
            return false;
         }
         gi.UnitTests.Add(ut5);
         //-----------------------------------------------------------------------------
         IUnitTest ut6 = new MapItemSetupUnitTest(dp, gi, this);
         if (true == ut6.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateUnitTests(): MapItemSetupUnitTest() ctor error");
            return false;
         }
         gi.UnitTests.Add(ut6);
         //-----------------------------------------------------------------------------
         IUnitTest ut7 = new DiceRollerUnitTest(dp, dr);
         if (true == ut7.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateUnitTests(): DiceRollerUnitTest() ctor error");
            return false;
         }
         gi.UnitTests.Add(ut7);
         //-----------------------------------------------------------------------------
         IUnitTest ut9 = new TravelCheckUnitTest(dp, gi, ev);
         if (true == ut9.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateUnitTests(): TravelCheckUnitTest() ctor error");
            return false;
         }
         gi.UnitTests.Add(ut9);
         //-----------------------------------------------------------------------------
         IUnitTest ut10 = new HuntUnitTest(ev);
         if (true == ut10.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateUnitTests(): HuntUnitTest() ctor error");
            return false;
         }
         gi.UnitTests.Add(ut10);
         //-----------------------------------------------------------------------------
         IUnitTest ut11 = new StarvationUnitTest(ev);
         if (true == ut11.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateUnitTests(): StarvationUnitTest() ctor error");
            return false;
         }
         gi.UnitTests.Add(ut11);
         //-----------------------------------------------------------------------------
         IUnitTest ut12 = new LodgingMgrUnitTest(ev);
         if (true == ut12.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateUnitTests(): LodgingMgrUnitTest ctor error");
            return false;
         }
         gi.UnitTests.Add(ut12);
         //-----------------------------------------------------------------------------
         IUnitTest ut13 = new TransportMgrUnitTest(ev);
         if (true == ut13.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateUnitTests(): LoadMgrUnitTest() ctor error");
            return false;
         }
         gi.UnitTests.Add(ut13);
         //-----------------------------------------------------------------------------
         IUnitTest ut14 = new CombatUnitTest(dp, gi, ev, dr);
         if (true == ut14.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateUnitTests(): CombatUnitTest() ctor error");
            return false;
         }
         gi.UnitTests.Add(ut14);
         //-----------------------------------------------------------------------------
         IUnitTest ut15 = new TreasureTableUnitTest(ev);
         if (true == ut15.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateUnitTests(): TreasureTableUnitTest() ctor error");
            return false;
         }
         gi.UnitTests.Add(ut15);
         //-----------------------------------------------------------------------------
         return true;
      }
   }
}
