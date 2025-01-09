using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace BarbarianPrince
{
    public class DiceRollerUnitTest : IUnitTest
    {
        public delegate void RollDieDelegate();
        public bool CtorError { get; } = false;
        //-----------------------------------------------------------
        private DockPanel myDockPanel = null;
        private IDieRoller myDieRoller = null;
        private ScrollViewer myScrollViewer = null;
        private Canvas myCanvas = null;
        //-----------------------------------------------------------
        private int myIndexName = 0;
        private List<string> myHeaderNames = new List<string>();
        private List<string> myCommandNames = new List<string>();
        public string HeaderName { get { return myHeaderNames[myIndexName]; } }
        public string CommandName { get { return myCommandNames[myIndexName]; } }
        //-----------------------------------------------------------
        public DiceRollerUnitTest(DockPanel dp, IDieRoller dr)
        {
            //------------------------------------------
            myIndexName = 0;
            myHeaderNames.Add("07-Die Statistics");
            myHeaderNames.Add("07-Die Rolling");
            myHeaderNames.Add("07-Die Moving");
            myHeaderNames.Add("07-Dice Rolling");
            myHeaderNames.Add("07-Dice Moving");
            myHeaderNames.Add("07-Dice Moving 3");
            //------------------------------------------
            myCommandNames.Add("Get Stats");
            myCommandNames.Add("Roll Die");
            myCommandNames.Add("Roll Moving Die");
            myCommandNames.Add("Roll Dice");
            myCommandNames.Add("Roll Moving Dice");
            myCommandNames.Add("Roll Moving Dice 3");
            //------------------------------------------
            myDockPanel = dp;
            foreach (UIElement ui0 in myDockPanel.Children)  // Find the Canvas in the visual tree
            {
                if (ui0 is DockPanel dockPanelInside)
                {
                    foreach (UIElement ui1 in dockPanelInside.Children)
                    {
                        if (ui1 is ScrollViewer)
                        {
                            myScrollViewer = (ScrollViewer)ui1;
                            if (myScrollViewer.Content is Canvas)
                            {
                                myCanvas = (Canvas)myScrollViewer.Content;
                                break;
                            }
                        }
                    }
                    break;
                }
            }
            if (null == myCanvas) // log error and return if canvas not found
            {
                Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateUnitTest() myCanvas=null");
                CtorError = true;
                return;
            }
            if (null == myScrollViewer) // log error and return if canvas not found
            {
                Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateUnitTest(): myScrollViewer=null");
                CtorError = true;
                return;
            }
            if (null == dr)
            {
                Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateUnitTest(): dr=null");
                CtorError = true;
                return;
            }
            myDieRoller = dr;
        }
        public bool Command(ref IGameInstance gi)
        {
            Logger.SetOn(LogEnum.LE_RESET_ROLL_STATE);
            if (CommandName == myCommandNames[0]) // Show 100 rolls of the dice and the average value
            {
                int NUM_OF_ROLLS = 1200;
                StringBuilder sb = new StringBuilder("RandomNumbers=");
                int randomNum = Utilities.RandomGenerator.Next(1, 7);
                double average = randomNum;
                int duplicates = 0;
                int previousNum = 0;
                for (int i = 0; i < NUM_OF_ROLLS; i++)
                {
                    System.Threading.Thread.Sleep(1);
                    sb.Append(randomNum.ToString());
                    sb.Append(",");
                    randomNum = Utilities.RandomGenerator.Next(1, 7);
                    average += randomNum;
                    if (randomNum == previousNum)
                        ++duplicates;
                    previousNum = randomNum;
                }
                average /= (NUM_OF_ROLLS + 1);
                sb.Append(" avg=");
                sb.Append(average.ToString());
                sb.Append(" duplicates=");
                sb.Append(duplicates.ToString());
                MessageBox.Show(sb.ToString());
            }
            else if (CommandName == myCommandNames[1])
            {
                RemoveEllipses();
                RollEndCallback callback = ShowResults;
                int dice = myDieRoller.RollStationaryDie(myCanvas, callback);
                if (0 == dice) // Roll one die
                {
                    Logger.Log(LogEnum.LE_ERROR, "Command(): RollStationaryDie(myMapItemDie1) returned false");
                    return false;
                }
            }
            else if (CommandName == myCommandNames[2])
            {
                RemoveEllipses();
                RollEndCallback callback = ShowResults;
                int dice = myDieRoller.RollMovingDie(myCanvas, callback);
                if (0 == dice) // Roll one die
                {
                    Logger.Log(LogEnum.LE_ERROR, "Command(): RollStationaryDie(myMapItemDie1) returned false");
                    return false;
                }
            }
            else if (CommandName == myCommandNames[3])
            {
                RemoveEllipses();
                RollEndCallback callback = ShowResults;
                int dice = myDieRoller.RollStationaryDice(myCanvas, callback);
                if (0 == dice) // Roll one die
                {
                    Logger.Log(LogEnum.LE_ERROR, "Command(): RollStationaryDie(myMapItemDie1) returned false");
                    return false;
                }
            }
            else if (CommandName == myCommandNames[4])
            {
                RemoveEllipses();
                RollEndCallback callback = ShowResults;
                int dice = myDieRoller.RollMovingDice(myCanvas, callback);
                if (0 == dice) // Roll one die
                {
                    Logger.Log(LogEnum.LE_ERROR, "Command(): RollStationaryDie(myMapItemDie1) returned false");
                    return false;
                }
            }
            else if (CommandName == myCommandNames[5])
            {
                RemoveEllipses();
                RollEndCallback callback = ShowResults;
                int dice = myDieRoller.Roll3MovingDice(myCanvas, callback);
                if (0 == dice) // Roll one die
                {
                    Logger.Log(LogEnum.LE_ERROR, "Command(): RollStationaryDie(myMapItemDie1) returned false");
                    return false;
                }
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
                if (false == Cleanup(ref gi))
                {
                    Logger.Log(LogEnum.LE_ERROR, "NextTest(): Cleanup() returned false");
                    return false;
                }
            }
            return true;
        }
        public bool Cleanup(ref IGameInstance gi)
        {
            try
            {
                List<UIElement> results = new List<UIElement>(); // Remove all elements from Canvas
                foreach (UIElement ui in myCanvas.Children)
                {
                    if (ui is Ellipse)
                        results.Add(ui);
                    if (ui is Rectangle)
                        results.Add(ui);
                    if (ui is Polyline)
                        results.Add(ui);
                }
                foreach (UIElement ui1 in results)
                    myCanvas.Children.Remove(ui1);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("DiceRollerUnitTest.Cleanup() exeption={0}", e.Message);
                return false;
            }
            //-----------------------------------------------------------------------
            Logger.SetOff(LogEnum.LE_RESET_ROLL_STATE);
            ++gi.GameTurn;
            return true;
        }
        //--------------------------------------------------
        private void ShowResults(int dieRoll)
        {
            //MessageBox.Show("ShowResults=" + dieRoll.ToString());
            Logger.Log(LogEnum.LE_RESET_ROLL_STATE, "dieRoll=" + dieRoll.ToString());
        }
        private void RemoveEllipses()
        {
            List<UIElement> results = new List<UIElement>(); // Remove all elements from Canvas
            foreach (UIElement ui in myCanvas.Children)
            {
                if (ui is Ellipse)
                    results.Add(ui);
            }
            foreach (UIElement ui1 in results)
                myCanvas.Children.Remove(ui1);
        }
    }
}
