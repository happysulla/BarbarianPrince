using System;
using System.IO;
using System.IO.Pipes;
using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BarbarianPrince.View;
using Microsoft.Win32;
namespace BarbarianPrince
{
   public static class CustomCommands
   {
      public static readonly RoutedUICommand NewCommand = new RoutedUICommand("New", "New", typeof(CustomCommands), new InputGestureCollection() { new KeyGesture(Key.N, ModifierKeys.Control) });
   }
   class MainMenuViewer : IView
   {
      private readonly Menu myMainMenu;                     // Top level menu items: File | View | Options | Help
      private readonly MenuItem myMenuItemTopLevel1 = null;
      private readonly MenuItem myMenuItemTopLevel2 = null;
      private readonly MenuItem myMenuItemTopLevel21 = null;
      private readonly MenuItem myMenuItemTopLevel22 = null;
      private readonly MenuItem myMenuItemTopLevel3 = null;
      private readonly MenuItem myMenuItemTopLevel4 = null;
      private readonly IGameEngine myGameEngine = null;
      private IGameInstance myGameInstance = null;
      public Options NewGameOptions { get; set; } = null;
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
               if (menuItem.Name == "myMenuItemTopLevel1")
               {
                  myMenuItemTopLevel1 = menuItem;
                  myMenuItemTopLevel1.Header = "_File";
               }
               //------------------------------------------------
               if (menuItem.Name == "myMenuItemTopLevel2")
               {
                  myMenuItemTopLevel2 = menuItem;
                  myMenuItemTopLevel2.Header = "_Edit";
                  myMenuItemTopLevel2.Visibility = Visibility.Visible;
                  myMenuItemTopLevel21 = new MenuItem();
                  myMenuItemTopLevel21.Header = "_Undo";
                  myMenuItemTopLevel21.InputGestureText = "Ctrl+U";
                  myMenuItemTopLevel21.IsEnabled = false;
                  myMenuItemTopLevel21.Click += MenuItemEditUndo_Click;
                  myMenuItemTopLevel2.Items.Add(myMenuItemTopLevel21);
                  myMenuItemTopLevel22 = new MenuItem();
                  myMenuItemTopLevel22.Header = "_Revert To Daybreak";
                  myMenuItemTopLevel22.InputGestureText = "Ctrl+R";
                  myMenuItemTopLevel22.Click += MenuItemEditRecover_Click;
                  myMenuItemTopLevel22.IsEnabled = false;
                  myMenuItemTopLevel2.Items.Add(myMenuItemTopLevel22);
               }
               //------------------------------------------------
               if (menuItem.Name == "myMenuItemTopLevel3")
               {
                  myMenuItemTopLevel3 = menuItem;
                  myMenuItemTopLevel3.Header = "_View";
                  myMenuItemTopLevel3.Visibility = Visibility.Visible;
                  MenuItem subItem31 = new MenuItem();
                  subItem31.Header = "_Path";
                  subItem31.InputGestureText = "Ctrl+P";
                  subItem31.Click += MenuItemViewPath_Click;
                  myMenuItemTopLevel3.Items.Add(subItem31);
                  MenuItem subItem32 = new MenuItem();
                  subItem32.Header = "_Rivers";
                  subItem32.InputGestureText = "Ctrl+Shift+R";
                  subItem32.Click += MenuItemViewRivers_Click;
                  myMenuItemTopLevel3.Items.Add(subItem32);
                  MenuItem subItem33 = new MenuItem();
                  subItem33.Header = "_Inventory...";
                  subItem33.InputGestureText = "Ctrl+I";
                  subItem33.Click += MenuItemViewInventory_Click;
                  myMenuItemTopLevel3.Items.Add(subItem33);
               }
               //------------------------------------------------
               if (menuItem.Name == "myMenuItemTopLevel4")
               {
                  myMenuItemTopLevel4 = menuItem;
                  myMenuItemTopLevel4.Header = "_Help";
                  myMenuItemTopLevel4.Visibility = Visibility.Visible;
                  MenuItem subItem41 = new MenuItem();
                  subItem41.Header = "_Rules...";
                  subItem41.InputGestureText = "F1";
                  subItem41.Click += MenuItemHelpRules_Click;
                  myMenuItemTopLevel4.Items.Add(subItem41);
                  MenuItem subItem42 = new MenuItem();
                  subItem42.Header = "_Icons...";
                  subItem42.InputGestureText = "F2";
                  subItem42.Click += MenuItemHelpIcons_Click;
                  myMenuItemTopLevel4.Items.Add(subItem42);
                  MenuItem subItem43 = new MenuItem();
                  subItem43.Header = "_About...";
                  subItem43.InputGestureText = "Ctrl+A";
                  subItem43.Click += MenuItemHelpAbout_Click;
                  myMenuItemTopLevel4.Items.Add(subItem43);
               }
            } // end foreach (Control item in myMainMenu.Items)
         } // end foreach (Control item in myMainMenu.Items)
#if UT2
            myMenuItemTopLevel1.Width = 300;
            myMenuItemTopLevel1.Click += MenuItemStart_Click;
            myMenuItemTopLevel2.Visibility = Visibility.Hidden;
            myMenuItemTopLevel3.Visibility = Visibility.Hidden;
            myMenuItemTopLevel4.Visibility = Visibility.Hidden;
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
#else
         MenuItem subItem1 = new MenuItem();
         subItem1.Header = "_New";
         subItem1.InputGestureText = "Ctrl+N";
         subItem1.Click += MenuItemNew_Click;
         myMenuItemTopLevel1.Items.Add(subItem1);
         MenuItem subItem2 = new MenuItem();
         subItem2.Header = "_Open...";
         subItem2.InputGestureText = "Ctrl+O";
         subItem2.Click += MenuItemFileOpen_Click;
         myMenuItemTopLevel1.Items.Add(subItem2);
         MenuItem subItem3 = new MenuItem();
         subItem3.Header = "_Close";
         subItem3.InputGestureText = "Ctrl+C";
         subItem3.Click += MenuItemClose_Click;
         myMenuItemTopLevel1.Items.Add(subItem3);
         MenuItem subItem4 = new MenuItem();
         subItem4.Header = "_Save As...";
         subItem4.InputGestureText = "Ctrl+S";
         subItem4.Click += MenuItemSaveAs_Click;
         myMenuItemTopLevel1.Items.Add(subItem4);
         MenuItem subItem5 = new MenuItem();
         subItem5.Header = "_Options...";
         subItem5.InputGestureText = "Ctrl+Shift+O";
         subItem5.Click += MenuItemFileOptions_Click;
         myMenuItemTopLevel1.Items.Add(subItem5);
#endif
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
               if (true == GameLoadMgr.theIsCheckFileExist)
                  myMenuItemTopLevel22.IsEnabled = true;
               Logger.Log(LogEnum.LE_UNDO_COMMAND, "MainMenuViewer.UpdateView(): cmd=" + myGameInstance.IsUndoCommandAvailable.ToString());
                  if (true == myGameInstance.IsUndoCommandAvailable)
                  myMenuItemTopLevel21.IsEnabled = true;
               else
                  myMenuItemTopLevel21.IsEnabled = false;
               return;
         }
      }
      //------------------------------CONTROLLER-------------------------------
      public void MenuItemNew_Click(object sender, RoutedEventArgs e)
      {
         if (null == NewGameOptions)
            myGameInstance = new GameInstance();
         else
            myGameInstance = new GameInstance(NewGameOptions);
         if (true == myGameInstance.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItemNew_Click(): myGameInstance.CtorError = true");
            return;
         }
         GameAction action = GameAction.UpdateNewGame;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemClose_Click(object sender, RoutedEventArgs e)
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItemClose_Click(): myGameInstance=null");
         }
         else
         {
            GameAction action = GameAction.EndGameClose;
            myGameEngine.PerformAction(ref myGameInstance, ref action);
         }
      }
      public void MenuItemFileOpen_Click(object sender, RoutedEventArgs e)
      {
         IGameInstance gi = GameLoadMgr.OpenGameFromFile();
         if (null != gi)
         {
            myGameInstance = gi;
            GameAction action = GameAction.UpdateLoadingGame;
            myGameEngine.PerformAction(ref gi, ref action);
         }
      }
      public void MenuItemSaveAs_Click(object sender, RoutedEventArgs e)
      {
         if (false == GameLoadMgr.SaveGameAsToFile(myGameInstance))
            Logger.Log(LogEnum.LE_ERROR, "MenuItemSave_Click(): GameLoadMgr.SaveGameAs() returned false");
      }
      public void MenuItemFileOptions_Click(object sender, RoutedEventArgs e)
      {
         OptionSelectionDialog dialog = new OptionSelectionDialog(myGameInstance.Options); // Set Options in Game
         if (true == dialog.CtorError)
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItemFileOptions_Click(): OptionSelectionDialog CtorError=true");
            return;
         }
         if (true == dialog.ShowDialog())
         {
            this.NewGameOptions = dialog.NewOptions;
            myGameInstance.Options = dialog.NewOptions;
            GameAction action = GameAction.UpdateGameOptions;
            myGameEngine.PerformAction(ref myGameInstance, ref action);
         }
      }
      public void MenuItemEditUndo_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.UpdateUndo;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemEditUndo_ClickCanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         if (true == myGameInstance.IsUndoCommandAvailable)
            e.CanExecute = true;
         else
            e.CanExecute = false;
      }
      public void MenuItemEditRecover_Click(object sender, RoutedEventArgs e)
      {
         IGameInstance gi = GameLoadMgr.OpenGame();
         if (null != gi)
         {
            myGameInstance = gi;
            GameAction action = GameAction.UpdateLoadingGame;
            myGameEngine.PerformAction(ref gi, ref action);
         }
      }
      public void MenuItemEditRecover_ClickCanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         if (true == GameLoadMgr.theIsCheckFileExist)
            e.CanExecute = true;
         else
            e.CanExecute = false;
      }
      public void MenuItemViewPath_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.UpdateEventViewerActive;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemViewRivers_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.ShowAllRivers;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemViewInventory_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.ShowInventory;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemHelpRules_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.UpdateEventViewerActive;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemHelpIcons_Click(object sender, RoutedEventArgs e)
      {
         IconDisplayDialog dialog = new IconDisplayDialog();
         dialog.Show();
      }
      public void MenuItemHelpAbout_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.UpdateEventViewerActive;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemStart_Click(object sender, RoutedEventArgs e) // Setup the initial menu options
      {
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
