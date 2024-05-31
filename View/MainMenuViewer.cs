using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
namespace BarbarianPrince
{
   class MainMenuViewer : IView
   {
      private readonly Menu myMainMenu;                     // Top level menu items: File | View | Options | Help
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
               if (menuItem.Name == "myMenuItemTopLevel1")
               {
                  myMenuItemTopLevel1 = menuItem;
                  myMenuItemTopLevel1.Header = "_File";
                  myMenuItemTopLevel1.InputGestureText = "Ctrl+F";
               }
               //------------------------------------------------
               if (menuItem.Name == "myMenuItemTopLevel2")
               {
                  myMenuItemTopLevel2 = menuItem;
                  myMenuItemTopLevel2.Header = "_Edit";
                  myMenuItemTopLevel2.InputGestureText = "Ctrl+E";
                  myMenuItemTopLevel2.Visibility = Visibility.Visible;
                  MenuItem subItem21 = new MenuItem();
                  subItem21.Header = "_Undo";
                  subItem21.Click += MenuItemEditUndo_Click;
                  myMenuItemTopLevel2.Items.Add(subItem21);
                  MenuItem subItem22 = new MenuItem();
                  subItem22.Header = "_Revert To Daybreak";
                  subItem22.Click += MenuItemEditRecover_Click;
                  myMenuItemTopLevel2.Items.Add(subItem22);
                  MenuItem subItem23 = new MenuItem();
                  subItem23.Header = "_Options...";
                  subItem23.Click += MenuItemEditOptions_Click;
                  myMenuItemTopLevel2.Items.Add(subItem23);
               }
               //------------------------------------------------
               if (menuItem.Name == "myMenuItemTopLevel3")
               {
                  myMenuItemTopLevel3 = menuItem;
                  myMenuItemTopLevel3.Header = "_View";
                  myMenuItemTopLevel3.InputGestureText = "Ctrl+O";
                  myMenuItemTopLevel3.Visibility = Visibility.Visible;
                  MenuItem subItem31 = new MenuItem();
                  subItem31.Header = "_Path";
                  subItem31.Click += MenuItemViewPath_Click;
                  myMenuItemTopLevel3.Items.Add(subItem31);
                  MenuItem subItem32 = new MenuItem();
                  subItem32.Header = "_Rivers";
                  subItem32.Click += MenuItemViewRivers_Click;
                  myMenuItemTopLevel3.Items.Add(subItem32);
                  MenuItem subItem33 = new MenuItem();
                  subItem33.Header = "_Inventory";
                  subItem33.Click += MenuItemViewInventory_Click;
                  myMenuItemTopLevel3.Items.Add(subItem33);
               }
               //------------------------------------------------
               if (menuItem.Name == "myMenuItemTopLevel4")
               {
                  myMenuItemTopLevel4 = menuItem;
                  myMenuItemTopLevel4.Header = "_Help";
                  myMenuItemTopLevel4.InputGestureText = "Ctrl+H";
                  myMenuItemTopLevel4.Visibility = Visibility.Visible;
                  MenuItem subItem41 = new MenuItem();
                  subItem41.Header = "_Rules...";
                  subItem41.Click += MenuItemHelpRules_Click;
                  myMenuItemTopLevel4.Items.Add(subItem41);
                  MenuItem subItem42 = new MenuItem();
                  subItem42.Header = "_Icons...";
                  subItem42.Click += MenuItemHelpIcons_Click;
                  myMenuItemTopLevel4.Items.Add(subItem42);
                  MenuItem subItem43 = new MenuItem();
                  subItem43.Header = "_About...";
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
            subItem1.Header = "_Open...";
            subItem1.Click += MenuItemFileOpen_Click;
            myMenuItemTopLevel1.Items.Add(subItem1);
            MenuItem subItem2 = new MenuItem();
            subItem2.Header = "_Save";
            subItem2.Click += MenuItemSave_Click;
            myMenuItemTopLevel1.Items.Add(subItem2);
            MenuItem subItem3 = new MenuItem();
            subItem3.Header = "_Save As...";
            subItem3.Click += MenuItemSaveAs_Click;
            myMenuItemTopLevel1.Items.Add(subItem3);
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
               return;
         }
      }
      //------------------------------CONTROLLER-------------------------------
      public void MenuItemFileOpen_Click(object sender, RoutedEventArgs e)
      {
         string filename = "test.bpb";
         FileStream fileStream = null;
         try
         {
            OpenFileDialog dlg = new OpenFileDialog();
            if (true == dlg.ShowDialog())
            {
               filename = dlg.FileName;
               fileStream = File.OpenRead(filename);
               BinaryFormatter formatter = new BinaryFormatter();
               IGameInstance gi = (GameInstance)formatter.Deserialize(fileStream);
               Logger.Log(LogEnum.LE_GAME_INIT, "MenuItemFileOpen_Click(): gi=" + myGameInstance.ToString());
               fileStream.Close();
               myGameInstance.Clone(gi);
               GameAction action = GameAction.UpdateLoadingGame;
               myGameEngine.PerformAction(ref myGameInstance, ref action);
            }
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItemFileOpen_Click(): e=" + ex.ToString());
            if (null != fileStream)
               fileStream.Close();
         }
      }
      public void MenuItemSave_Click(object sender, RoutedEventArgs e)
      {
         string filename = "test.bpb";
         FileStream fileStream = null;
         try
         {
            fileStream = File.OpenWrite(filename);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(fileStream, myGameInstance);
            fileStream.Close();
            GameAction action = GameAction.UpdateEventViewerActive;
            myGameEngine.PerformAction(ref myGameInstance, ref action);
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItemSave_Click(): e=" + ex.ToString());
            if (null != fileStream)
               fileStream.Close();
         }
      }
      public void MenuItemSaveAs_Click(object sender, RoutedEventArgs e)
      {
         try
         {
            string filename = "test.bpb";
            FileStream fileStream = File.OpenWrite(filename);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(fileStream, myGameInstance);
            fileStream.Close();
            GameAction action = GameAction.UpdateEventViewerActive;
            myGameEngine.PerformAction(ref myGameInstance, ref action);
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "MenuItemSave_Click(): e=" + ex.ToString());
         }
      }
      public void MenuItemEditUndo_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.UpdateEventViewerActive;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemEditRecover_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.UpdateEventViewerActive;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemEditOptions_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.UpdateEventViewerActive;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemViewPath_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.UpdateEventViewerActive;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemViewRivers_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.UpdateEventViewerActive;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemViewInventory_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.UpdateEventViewerActive;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemHelpRules_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.UpdateEventViewerActive;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      public void MenuItemHelpIcons_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.UpdateEventViewerActive;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
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
