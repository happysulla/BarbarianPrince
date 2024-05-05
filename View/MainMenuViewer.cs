using System.Text;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Forms;
namespace BarbarianPrince
{
    class MainMenuViewer : IView
    {
        private readonly Menu myMainMenu;
        private readonly MenuItem myMenuItemTopLevel1 = null;
        private readonly MenuItem myMenuItemTopLevel2 = null;
        private readonly MenuItem myMenuItemTopLevel3 = null;
        private readonly MenuItem myMenuItemTopLevel4 = null;
        private readonly IGameEngine myGameEngine = null;
        private IGameInstance myGameInstance = null;
        //-----------------------------------------------------------------------
        public MainMenuViewer(Menu mi, IGameEngine ge, IGameInstance gi) // Constructor creates default top level menus that get changed with UpdateView() based on GamePhase and GameAction
        {
            myGameEngine = ge;
            myGameInstance = gi;
            myMainMenu = mi;
            foreach (Control item in myMainMenu.Items) // Initialize all the menu items
            {
                if (item is MenuItem menuItem)
                {
                    // top level menu items: File | View | Options | Help
                    if (menuItem.Name == "myMenuItemTopLevel1")
                    {
                        myMenuItemTopLevel1 = menuItem;
                        myMenuItemTopLevel1.Width = 300;
                        myMenuItemTopLevel1.Header = "_File";
                        myMenuItemTopLevel1.InputGestureText = "Ctrl+S";
                        myMenuItemTopLevel1.Click += MenuItemStart_Click;
                    }
                    //------------------------------------------------
                    if (menuItem.Name == "myMenuItemTopLevel2")
                    {
                        myMenuItemTopLevel2 = menuItem;
                        myMenuItemTopLevel2.Header = "_View";
                        myMenuItemTopLevel2.InputGestureText = "Ctrl+V";
                        myMenuItemTopLevel2.Visibility = Visibility.Hidden;
                    }
                    //------------------------------------------------
                    if (menuItem.Name == "myMenuItemTopLevel3")
                    {
                        myMenuItemTopLevel3 = menuItem;
                        myMenuItemTopLevel3.Header = "_Options";
                        myMenuItemTopLevel3.InputGestureText = "Ctrl+O";
                        myMenuItemTopLevel3.Visibility = Visibility.Hidden;
                    }
                    //------------------------------------------------
                    if (menuItem.Name == "myMenuItemTopLevel4")
                    {
                        myMenuItemTopLevel4 = menuItem;
                        myMenuItemTopLevel4.Header = "_Help";
                        myMenuItemTopLevel4.InputGestureText = "Ctrl+H";
                        myMenuItemTopLevel4.Visibility = Visibility.Hidden;
                    }
                } // end foreach (Control item in myMainMenu.Items)
            } // end foreach (Control item in myMainMenu.Items)
        } // end MainMenuViewer()
          //-----------------------------------------------------------------------
        public void UpdateView(ref IGameInstance gi, GameAction action)
        {
            myGameInstance = gi;
            StringBuilder sb = new StringBuilder("-----------------MainMenuViewer::UpdateView() => a="); sb.Append(action.ToString());
            Logger.Log(LogEnum.LE_VIEW_UPDATE_MENU, sb.ToString());
            switch (gi.GamePhase)
            {
                case GamePhase.GameSetup:
                    return;
                case GamePhase.UnitTest:
                    IUnitTest ut = gi.UnitTests[gi.GameTurn];
                    myMenuItemTopLevel1.Header = ut.HeaderName;
                    if (0 < myMenuItemTopLevel1.Items.Count)
                    {
                        MenuItem menuItem0 = (MenuItem)myMenuItemTopLevel1.Items[0];
                        menuItem0.Header = ut.CommandName;
                    }
                    break;
                default:
                    //Console.WriteLine("ERROR<<<<<MainMenuViewer::UpdateView() reached default with action={0} NextAction={1} phase={2}", action.ToString(), gi.NextAction, gi.GamePhase);
                    return;
            }
        }
        //-----------------------------------------------------------------------
        public void MenuItemStart_Click(object sender, RoutedEventArgs e) // Setup the initial menu options
        {
            if (0 == myMenuItemTopLevel1.Items.Count)
            {
                MenuItem subItem1 = new MenuItem();
                subItem1.Click += MenuItemCommand_Click;
                myMenuItemTopLevel1.Items.Add(subItem1);
                MenuItem subItem2 = new MenuItem();
                subItem2.Header = "_NextTest";
                subItem2.Click += MenuItemNextTest_Click;
                myMenuItemTopLevel1.Items.Add(subItem2);
                MenuItem subItem3 = new MenuItem();
                subItem3.Header = "_Cleanup";
                subItem3.Click += MenuItemCleanup_Click;
                myMenuItemTopLevel1.Items.Add(subItem3);
            }
            GameAction action = GameAction.UnitTestStart;
            myGameEngine.PerformAction(ref myGameInstance, ref action);
        }
        public void MenuItemCommand_Click(object sender, RoutedEventArgs e)
        {
            GameAction action = GameAction.UnitTestCommand;
            myGameEngine.PerformAction(ref myGameInstance, ref action);
        }
        public void MenuItemNextTest_Click(object sender, RoutedEventArgs e)
        {
            GameAction action = GameAction.UnitTestNext;
            myGameEngine.PerformAction(ref myGameInstance, ref action);
        }
        public void MenuItemCleanup_Click(object sender, RoutedEventArgs e)
        {
            GameAction action = GameAction.UnitTestCleanup;
            myGameEngine.PerformAction(ref myGameInstance, ref action);
        }
    }
}
