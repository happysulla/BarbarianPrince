using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace BarbarianPrince
{
    public class ConfigMgrUnitTest : IUnitTest
    {
        public bool CtorError { get; } = false;
        //----------------------------------------------------------------------------
        private ScrollViewer myScrollViewerCanvas = null;
        private ScrollViewer myScrollViewerTextBlock = null;
        private Canvas myCanvas = null;
        private EventViewer myEventViewer = null;
        private int myKeyIndex = 0;
        //----------------------------------------------------------------------------
        private int myIndexName = 0;
        private List<string> myHeaderNames = new List<string>();
        private List<string> myCommandNames = new List<string>();
        public string HeaderName { get { return myHeaderNames[myIndexName]; } }
        public string CommandName { get { return myCommandNames[myIndexName]; } }
        //----------------------------------------------------------------------------
        public ConfigMgrUnitTest(DockPanel dp, EventViewer ev)
        {
            myIndexName = 0;
            myHeaderNames.Add("05-Show Events");
            myHeaderNames.Add("05-Show Tables");
            myHeaderNames.Add("05-Show Rules");
            //------------------------------------------
            myCommandNames.Add("Show Event");
            myCommandNames.Add("Show Table");
            myCommandNames.Add("Show Rule");
            //------------------------------------------
            foreach (UIElement ui0 in dp.Children)
            {
                if (ui0 is DockPanel dockPanelInside)
                {
                    foreach (UIElement ui1 in dockPanelInside.Children)
                    {
                        if (ui1 is ScrollViewer)
                        {
                            myScrollViewerCanvas = (ScrollViewer)ui1;
                            if (myScrollViewerCanvas.Content is Canvas)
                                myCanvas = (Canvas)myScrollViewerCanvas.Content;  // Find the Canvas in the visual tree
                        }
                        if (ui1 is DockPanel dockPanelControls)
                        {
                            foreach (UIElement ui2 in dockPanelControls.Children)
                            {
                                if (ui2 is ScrollViewer)
                                {
                                    myScrollViewerTextBlock = (ScrollViewer)ui2;
                                    break;
                                }
                            }
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
            if (null == myScrollViewerCanvas)
            {
                Logger.Log(LogEnum.LE_ERROR, "MapItemSetupUnitTest(): myScrollViewerCanvas=null");
                CtorError = true;
                return;
            }
            if (null == myScrollViewerTextBlock)
            {
                Logger.Log(LogEnum.LE_ERROR, "MapItemSetupUnitTest(): myScrollViewerTextBlock=null");
                CtorError = true;
                return;
            }
            myKeyIndex = 0; // Set this value to another number if want to skip some events, rules, or tables
            myEventViewer = ev;
        }
        //----------------------------------------------------------------------------
        public bool Command(ref IGameInstance gi) // Performs function based on CommandName string
        {
            if (CommandName == myCommandNames[0])
            {
                string key = myEventViewer.Events.Keys.ElementAt(myKeyIndex);
                if (false == myEventViewer.OpenEvent(gi, key))
                {
                    Logger.Log(LogEnum.LE_ERROR, "Command(): OpenEvent() returned false key=" + gi.EventActive + " keyindex=" + myKeyIndex.ToString());
                    return false;
                }
                ++myKeyIndex;
                if (myEventViewer.Events.Keys.Count <= myKeyIndex)
                    myKeyIndex = 0;
            }
            else if (CommandName == myCommandNames[1])
            {
                string key = myEventViewer.myRulesMgr.Tables.Keys.ElementAt(myKeyIndex);
                if (false == myEventViewer.ShowTable(key))
                {
                    Logger.Log(LogEnum.LE_ERROR, "Command(): ShowTable() returned false");
                    return false;
                }
                ++myKeyIndex;
                if (myEventViewer.myRulesMgr.Tables.Keys.Count <= myKeyIndex)
                    myKeyIndex = 0;
            }
            else if (CommandName == myCommandNames[2])
            {
                string key = myEventViewer.myRulesMgr.Rules.Keys.ElementAt(myKeyIndex);
                if (false == myEventViewer.ShowRule(key))
                {
                    Logger.Log(LogEnum.LE_ERROR, "Command(): ShowRule() returned false");
                    return false;
                }
                ++myKeyIndex;
                if (myEventViewer.myRulesMgr.Rules.Keys.Count <= myKeyIndex)
                    myKeyIndex = 0;
            }
            return true;
        }
        public bool NextTest(ref IGameInstance gi) // Move to the next test in this class's unit tests
        {
            if (HeaderName == myHeaderNames[0])
            {
                ++myIndexName;
                myKeyIndex = 0;
            }
            else if (HeaderName == myHeaderNames[1])
            {
                ++myIndexName;
                myKeyIndex = 0;
            }
            else if (HeaderName == myHeaderNames[2])
            {
            }
            return true;
        }
        public bool Cleanup(ref IGameInstance gi) // Remove an elipses from the canvas and save off Territories.xml file
        {
            //--------------------------------------------------
            // Remove any existing UI elements from the Canvas
            List<UIElement> results = new List<UIElement>();
            foreach (UIElement ui in myCanvas.Children)
            {
                if (ui is Ellipse)
                    results.Add(ui);
            }
            foreach (UIElement ui1 in results)
                myCanvas.Children.Remove(ui1);
            //--------------------------------------------------
            ++gi.GameTurn;
            return true;
        }
    }
}
