using System.Collections.Generic;
using System.Windows;

namespace BarbarianPrince
{
    internal class TreasureTableUnitTest : IUnitTest
    {
        public bool CtorError { get; } = false;
        //-----------------------------------------------------------
        private EventViewer myEventViewer = null;
        //-----------------------------------------------------------
        private int myIndexName = 0;
        private int myIndexWealth = 0;
        private List<string> myHeaderNames = new List<string>();
        private List<string> myCommandNames = new List<string>();
        public string HeaderName { get { return myHeaderNames[myIndexName]; } }
        public string CommandName { get { return myCommandNames[myIndexName]; } }
        //-----------------------------------------------------------
        public TreasureTableUnitTest(EventViewer ev)
        {
            //------------------------------------------
            myIndexName = 0;
            myHeaderNames.Add("15-Get Wealth");
            myHeaderNames.Add("15-Get Item");
            myHeaderNames.Add("15-Get Item 110");
            myHeaderNames.Add("15-Get Item 110");
            myHeaderNames.Add("15-Get Item 60");
            myHeaderNames.Add("15-Finish");
            //------------------------------------------
            myCommandNames.Add("01-Show Wealth");
            myCommandNames.Add("02-Show Item");
            myCommandNames.Add("03-Show Item w/o Pegasus");
            myCommandNames.Add("04-Show Item w/ Pegasus");
            myCommandNames.Add("05-Show Item w/ Reroll");
            myCommandNames.Add("Finish");
            //------------------------------------------
            if (null == ev)
            {
                Logger.Log(LogEnum.LE_ERROR, "TreasureTableUnitTest(): ev=null");
                CtorError = true;
                return;
            }
            myEventViewer = ev;
        }
        public bool Command(ref IGameInstance gi)
        {
            if (CommandName == myCommandNames[0])
            {
                gi.ActiveMember = gi.Prince;
                if (false == SetWealthCode(gi))
                {
                    Logger.Log(LogEnum.LE_ERROR, "Command(): SetWealthCode()");
                    return false;
                }
                myEventViewer.UpdateView(ref gi, GameAction.EncounterLootStart);
            }
            else if (CommandName == myCommandNames[1])
            {
                gi.ActiveMember = gi.Prince;
                if (false == SetItemCode(gi))
                {
                    Logger.Log(LogEnum.LE_ERROR, "Command(): SetItemCode()");
                    return false;
                }
                myEventViewer.UpdateView(ref gi, GameAction.EncounterLootStart);
            }
            else if (CommandName == myCommandNames[2])
            {
                gi.ActiveMember = gi.Prince;
                gi.CapturedWealthCodes.Add(110);
                myEventViewer.UpdateView(ref gi, GameAction.EncounterLootStart);
            }
            else if (CommandName == myCommandNames[3])
            {
                gi.ActiveMember = gi.Prince;
                gi.CapturedWealthCodes.Add(110);
                gi.PegasusTreasure = PegasusTreasureEnum.Mount;
                myEventViewer.UpdateView(ref gi, GameAction.EncounterLootStart);
            }
            else if (CommandName == myCommandNames[4])
            {
                gi.ActiveMember = gi.Prince;
                gi.CapturedWealthCodes.Add(60);
                gi.PegasusTreasure = PegasusTreasureEnum.Reroll;
                myEventViewer.UpdateView(ref gi, GameAction.EncounterLootStart);
            }
            else if (CommandName == myCommandNames[5])
            {
                if (false == NextTest(ref gi))
                    Logger.Log(LogEnum.LE_ERROR, "Command(): NextTest() returned error");
            }
            return true;
        }
        public bool NextTest(ref IGameInstance gi)
        {
            if (HeaderName == myHeaderNames[0])
            {
                ++myIndexName;
                myIndexWealth = 0;
            }
            else if (HeaderName == myHeaderNames[1])
            {
                ++myIndexName;
                myIndexWealth = 0;
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
            else
            {
                if (false == Cleanup(ref gi))
                    Logger.Log(LogEnum.LE_ERROR, "NextTest(): Cleanup() returned error");
            }
            return true;
        }
        public bool Cleanup(ref IGameInstance gi)
        {
            gi.ActiveMember = null;
            gi.CapturedWealthCodes.Clear();
            gi.PegasusTreasure = PegasusTreasureEnum.Talisman;
            Application.Current.Shutdown();
            return true;
        }
        //-----------------------------------------------------------
        private bool SetWealthCode(IGameInstance gi)
        {
            switch (myIndexWealth)
            {
                case 0: gi.CapturedWealthCodes.Add(1); break;
                case 1: gi.CapturedWealthCodes.Add(2); break;
                case 2: gi.CapturedWealthCodes.Add(4); break;
                case 3: gi.CapturedWealthCodes.Add(5); break;
                case 4: gi.CapturedWealthCodes.Add(7); break;
                case 5: gi.CapturedWealthCodes.Add(10); break;
                case 6: gi.CapturedWealthCodes.Add(12); break;
                case 7: gi.CapturedWealthCodes.Add(15); break;
                case 8: gi.CapturedWealthCodes.Add(21); break;
                case 9: gi.CapturedWealthCodes.Add(25); break;
                case 10: gi.CapturedWealthCodes.Add(30); break;
                case 11: gi.CapturedWealthCodes.Add(50); break;
                case 12: gi.CapturedWealthCodes.Add(60); break;
                case 13: gi.CapturedWealthCodes.Add(70); break;
                case 14: gi.CapturedWealthCodes.Add(100); break;
                case 15: gi.CapturedWealthCodes.Add(110); break;
                default:
                    Logger.Log(LogEnum.LE_ERROR, "SetWealthCode(): invalid parameter wc=" + myIndexWealth.ToString());
                    return false;
            }
            ++myIndexWealth;
            if (16 == myIndexWealth)
                myIndexWealth = 0;
            return true;
        }
        private bool SetItemCode(IGameInstance gi)
        {
            switch (myIndexWealth)
            {
                case 0: gi.CapturedWealthCodes.Add(5); break;
                case 1: gi.CapturedWealthCodes.Add(12); break;
                case 2: gi.CapturedWealthCodes.Add(25); break;
                case 3: gi.CapturedWealthCodes.Add(60); break;
                case 4: gi.CapturedWealthCodes.Add(110); break;
                default:
                    Logger.Log(LogEnum.LE_ERROR, "SetItemCode(): invalid parameter wc=" + myIndexWealth.ToString());
                    return false;
            }
            ++myIndexWealth;
            if (5 == myIndexWealth)
                myIndexWealth = 0;
            return true;
        }
    }
}
