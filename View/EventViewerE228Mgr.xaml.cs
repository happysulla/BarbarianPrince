using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfAnimatedGif;

namespace BarbarianPrince
{
   public partial class EventViewerE228Mgr : UserControl
   {
      public delegate bool EndE228Callback();
      private const int STARTING_ASSIGNED_ROW = 6;
      //---------------------------------------------
      public struct GridRow
      {
         public IMapItem myMapItem;
         public int myReuniteDieRoll;
         public int myMountDieRoll;
         public GridRow(IMapItem mi)
         {
            myMapItem = mi;
            myReuniteDieRoll = Utilities.NO_RESULT;
            myMountDieRoll = Utilities.NO_RESULT;
         }
      };
      public enum E228Enum
      {
         RETURN_CHECK,
         SHOW_REUNITE_RESULTS,
         MOUNT_CHECK,
         SHOW_MOUNT_RESULTS,
         END
      };
      //---------------------------------------------
      public bool CtorError { get; } = false;
      //---------------------------------------------
      private E228Enum myState = E228Enum.RETURN_CHECK;
      private EndE228Callback myCallback = null;
      private int myMaxRowCount = 0;
      private GridRow[] myGridRows = null;
      //---------------------------------------------
      private IGameInstance myGameInstance = null;
      private readonly Canvas myCanvas = null;
      private readonly ScrollViewer myScrollViewer = null;
      private RuleDialogViewer myRulesMgr = null;
      //---------------------------------------------
      private IDieRoller myDieRoller = null;
      private int myRollResulltRowNum = 0;
      private bool myIsRollInProgress = false;
      //---------------------------------------------
      private readonly SolidColorBrush mySolidColorBrushBlack = new SolidColorBrush() { Color = Colors.Black };
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      private readonly FontFamily myFontFam1 = new FontFamily("Courier New");
      //-----------------------------------------------------------------------------------------
      public EventViewerE228Mgr(IGameInstance gi, Canvas c, ScrollViewer sv, RuleDialogViewer rdv, IDieRoller dr)
      {
         InitializeComponent();
         //--------------------------------------------------
         if (null == gi) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE134Mgr(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //--------------------------------------------------
         if (null == c) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE134Mgr(): c=null");
            CtorError = true;
            return;
         }
         myCanvas = c;
         //--------------------------------------------------
         if (null == sv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE134Mgr(): sv=null");
            CtorError = true;
            return;
         }
         myScrollViewer = sv;
         //--------------------------------------------------
         if (null == rdv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE134Mgr(): rdv=null");
            CtorError = true;
            return;
         }
         myRulesMgr = rdv;
         //--------------------------------------------------
         if (null == dr)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE134Mgr(): dr=true");
            CtorError = true;
            return;
         }
         myDieRoller = dr;
         //--------------------------------------------------
         myGrid.MouseDown += Grid_MouseDown;
      }
      public bool CheckTrueLoveReturn(EndE228Callback callback)
      {
         if (null == myGameInstance.LostTrueLoves)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckTrueLoveReturn(): partyMembers=null");
            return false;
         }
         //--------------------------------------------------
         myGridRows = new GridRow[Utilities.MAX_GRID_ROW];
         myState = E228Enum.RETURN_CHECK;
         myMaxRowCount = myGameInstance.LostTrueLoves.Count;
         myIsRollInProgress = false;
         myRollResulltRowNum = 0;
         myCallback = callback;
         //--------------------------------------------------
         int i = 0;
         foreach (IMapItem mi in myGameInstance.LostTrueLoves)
         {
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "CheckTrueLoveReturn(): mi=null");
               return false;
            }
            myGridRows[i] = new GridRow(mi);
            ++i;
         }
         //--------------------------------------------------
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckTrueLoveReturn(): UpdateGrid() return false");
            return false;
         }
         myScrollViewer.Content = myGrid;
         return true;
      }
      //-----------------------------------------------------------------------------------------
      private bool UpdateGrid()
      {
         if (false == UpdateEndState())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateEndState() returned false");
            return false;
         }
         if (E228Enum.END == myState)
            return true;
         if (false == UpdateUserInstructions())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateUserInstructions() returned false");
            return false;
         }
         if (false == UpdateAssignablePanel())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateAssignablePanel() returned false");
            return false;
         }
         if (false == UpdateGridRows())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateGridRows() returned false");
            return false;
         }
         return true;
      }
      private bool UpdateEndState()
      {
         if (E228Enum.END == myState)
         {
            if (null == myCallback)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): myCallback=null");
               return false;
            }
            IMapItems trueLoves = new MapItems();
            for (int i = 0; i < myMaxRowCount; i++)
            {
               IMapItem trueLove = myGridRows[i].myMapItem;
               if (12 == myGridRows[i].myReuniteDieRoll)
               {
                  myGameInstance.LostTrueLoves.Remove(trueLove.Name);
               }
               else if ((10 == myGridRows[i].myReuniteDieRoll) || (11 == myGridRows[i].myReuniteDieRoll))
               {
                  myGameInstance.LostTrueLoves.Remove(trueLove.Name);
                  if (12 == myGridRows[i].myMountDieRoll)
                     trueLove.AddNewMount(MountEnum.Pegasus);
                  else if (8 < myGridRows[i].myMountDieRoll)
                     trueLove.AddNewMount(MountEnum.Horse);
                  myGameInstance.AddCompanion(trueLove);
               }
            }
            if (false == myCallback())
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): myCallback() returned false");
               return false;
            }
         }
         return true;
      }
      private bool UpdateUserInstructions()
      {
         myTextBlockInstructions.Inlines.Clear();
         switch (myState)
         {
            case E228Enum.RETURN_CHECK:
               myTextBlockInstructions.Inlines.Add(new Run("Roll dice for each true love. A 10 or 11 reunites. A 12 and she dies."));
               break;
            case E228Enum.SHOW_REUNITE_RESULTS:
               bool isTrueLoveReturning = false;
               for (int i = 0; i < myMaxRowCount; i++)
               {
                  if ((10 == myGridRows[i].myReuniteDieRoll) || (10 == myGridRows[i].myReuniteDieRoll))
                     isTrueLoveReturning = true;
               }
               if (true == isTrueLoveReturning)
                  myTextBlockInstructions.Inlines.Add(new Run("Click Mount to continue."));
               else
                  myTextBlockInstructions.Inlines.Add(new Run("Click Campfire to continue."));
               break;
            case E228Enum.MOUNT_CHECK:
               myTextBlockInstructions.Inlines.Add(new Run("Roll dice for each return. A 9+ means returns with mount."));
               break;
            case E228Enum.SHOW_MOUNT_RESULTS:
               myTextBlockInstructions.Inlines.Add(new Run("Click Campfire to continue."));
               break;
            case E228Enum.END:
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateUserInstructions(): reached default" + myState.ToString());
               return false;
         }
         return true;
      }
      private bool UpdateAssignablePanel()
      {
         myStackPanelAssignable.Children.Clear(); // clear out assignable panel 
         switch (myState)
         {
            case E228Enum.RETURN_CHECK:
               Rectangle r8 = new Rectangle()
               {
                  Visibility = Visibility.Hidden,
                  Width = Utilities.ZOOM * Utilities.theMapItemSize,
                  Height = Utilities.ZOOM * Utilities.theMapItemSize
               };
               myStackPanelAssignable.Children.Add(r8);
               break;
            case E228Enum.SHOW_REUNITE_RESULTS:
               bool isTrueLoveReturning = false;
               for (int i = 0; i < myMaxRowCount; i++)
               {
                  if ((10 == myGridRows[i].myReuniteDieRoll) || (10 == myGridRows[i].myReuniteDieRoll))
                     isTrueLoveReturning = true;
               }
               if (true == isTrueLoveReturning)
               {
                  Image imgMount = new Image { Name = "Mount", Source = MapItem.theMapImages.GetBitmapImage("Mount"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(imgMount);
               }
               else
               {
                  BitmapImage bmi1 = new BitmapImage();
                  bmi1.BeginInit();
                  bmi1.UriSource = new Uri(MapImage.theImageDirectory + "CampFire2.gif", UriKind.Absolute);
                  bmi1.EndInit();
                  Image img1 = new Image { Name = "Campfire", Source = bmi1, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  ImageBehavior.SetAnimatedSource(img1, bmi1);
                  myStackPanelAssignable.Children.Add(img1);
               }
               break;
            case E228Enum.MOUNT_CHECK:
               Rectangle r1 = new Rectangle()
               {
                  Visibility = Visibility.Hidden,
                  Width = Utilities.ZOOM * Utilities.theMapItemSize,
                  Height = Utilities.ZOOM * Utilities.theMapItemSize
               };
               myStackPanelAssignable.Children.Add(r1);
               break;
            case E228Enum.SHOW_MOUNT_RESULTS:
               BitmapImage bmi6 = new BitmapImage();
               bmi6.BeginInit();
               bmi6.UriSource = new Uri(MapImage.theImageDirectory + "CampFire2.gif", UriKind.Absolute);
               bmi6.EndInit();
               Image img6 = new Image { Name = "Campfire", Source = bmi6, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               ImageBehavior.SetAnimatedSource(img6, bmi6);
               myStackPanelAssignable.Children.Add(img6);
               //-------------------------------------------
               Rectangle r9 = new Rectangle()
               {
                  Visibility = Visibility.Hidden,
                  Width = Utilities.ZOOM * Utilities.theMapItemOffset,
                  Height = Utilities.ZOOM * Utilities.theMapItemOffset
               };
               myStackPanelAssignable.Children.Add(r9);
               break;
            case E228Enum.END:
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanel(): reached default s=" + myState.ToString());
               return false;
         }
         return true;
      }
      private bool UpdateGridRows()
      {
         //------------------------------------------------------------
         // Clear out existing Grid Row data
         List<UIElement> results = new List<UIElement>();
         foreach (UIElement ui in myGrid.Children)
         {
            int rowNum = Grid.GetRow(ui);
            if (STARTING_ASSIGNED_ROW <= rowNum)
               results.Add(ui);
         }
         foreach (UIElement ui1 in results)
            myGrid.Children.Remove(ui1);
         //------------------------------------------------------------
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            IMapItem mi = myGridRows[i].myMapItem;
            Image imgFace = new Image { Source = MapItem.theMapImages.GetBitmapImage(mi.BottomImageName), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
            myGrid.Children.Add(imgFace);
            Grid.SetRow(imgFace, rowNum);
            Grid.SetColumn(imgFace, 0);
            //------------------------------------
            switch (myState)
            {
               case E228Enum.RETURN_CHECK:
               case E228Enum.SHOW_REUNITE_RESULTS:
                  if (Utilities.NO_RESULT == myGridRows[i].myReuniteDieRoll)
                  {
                     //Image img = new Image { Name = "dieRoll", Source = MapItem.theMapImages.GetBitmapImage("c01die"), Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                     BitmapImage bmi = new BitmapImage(); //c01die
                     bmi.BeginInit();
                     bmi.UriSource = new Uri(MapImage.theImageDirectory + "dieRoll.gif", UriKind.Absolute);
                     bmi.EndInit();
                     Image img = new Image { Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                     ImageBehavior.SetAnimatedSource(img, bmi);
                     myGrid.Children.Add(img);
                     Grid.SetRow(img, rowNum);
                     Grid.SetColumn(img, 1);
                  }
                  else
                  {
                     Label labelRenuniteRoll = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myReuniteDieRoll.ToString() };
                     myGrid.Children.Add(labelRenuniteRoll);
                     Grid.SetRow(labelRenuniteRoll, rowNum);
                     Grid.SetColumn(labelRenuniteRoll, 1);
                     Image imgReuniteResult = null;
                     if (12 == myGridRows[i].myReuniteDieRoll)
                        imgReuniteResult = new Image { Source = MapItem.theMapImages.GetBitmapImage("Skulls"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                     else if (9 < myGridRows[i].myReuniteDieRoll)
                        imgReuniteResult = new Image { Source = MapItem.theMapImages.GetBitmapImage("TrueLove"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                     else
                        imgReuniteResult = new Image { Source = MapItem.theMapImages.GetBitmapImage("Deny"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                     myGrid.Children.Add(imgReuniteResult);
                     Grid.SetRow(imgReuniteResult, rowNum);
                     Grid.SetColumn(imgReuniteResult, 2);
                  }
                  break;
               case E228Enum.MOUNT_CHECK:
               case E228Enum.SHOW_MOUNT_RESULTS:
                  if ((10 == myGridRows[i].myReuniteDieRoll) || (11 == myGridRows[i].myReuniteDieRoll))
                  {
                     Label labelRenuniteRoll = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myReuniteDieRoll.ToString() };
                     myGrid.Children.Add(labelRenuniteRoll);
                     Grid.SetRow(labelRenuniteRoll, rowNum);
                     Grid.SetColumn(labelRenuniteRoll, 1);
                     Image imgReuniteResult = null;
                     if (12 == myGridRows[i].myReuniteDieRoll)
                        imgReuniteResult = new Image { Source = MapItem.theMapImages.GetBitmapImage("Skulls"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                     else if (9 < myGridRows[i].myReuniteDieRoll)
                        imgReuniteResult = new Image { Source = MapItem.theMapImages.GetBitmapImage("TrueLove"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                     else
                        imgReuniteResult = new Image { Source = MapItem.theMapImages.GetBitmapImage("Deny"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                     myGrid.Children.Add(imgReuniteResult);
                     Grid.SetRow(imgReuniteResult, rowNum);
                     Grid.SetColumn(imgReuniteResult, 2);
                     if (Utilities.NO_RESULT == myGridRows[i].myMountDieRoll)
                     {
                        //Image img2 = new Image { Name="dieRoll", Source = MapItem.theMapImages.GetBitmapImage("c01die"), Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                        BitmapImage bmi2 = new BitmapImage();
                        bmi2.BeginInit();
                        bmi2.UriSource = new Uri(MapImage.theImageDirectory + "dieRoll.gif", UriKind.Absolute);
                        bmi2.EndInit();
                        Image img2 = new Image { Source = bmi2, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                        ImageBehavior.SetAnimatedSource(img2, bmi2);
                        myGrid.Children.Add(img2);
                        Grid.SetRow(img2, rowNum);
                        Grid.SetColumn(img2, 3);
                     }
                     else
                     {
                        Label labelMountRoll = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myMountDieRoll.ToString() };
                        myGrid.Children.Add(labelMountRoll);
                        Grid.SetRow(labelMountRoll, rowNum);
                        Grid.SetColumn(labelMountRoll, 3);
                        Image imgMountResult = null;
                        if (12 == myGridRows[i].myMountDieRoll)
                           imgMountResult = new Image { Source = MapItem.theMapImages.GetBitmapImage("Pegasus"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                        else if (8 < myGridRows[i].myMountDieRoll)
                           imgMountResult = new Image { Source = MapItem.theMapImages.GetBitmapImage("Mount"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                        else
                           imgMountResult = new Image { Source = MapItem.theMapImages.GetBitmapImage("Deny"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                        myGrid.Children.Add(imgMountResult);
                        Grid.SetRow(imgMountResult, rowNum);
                        Grid.SetColumn(imgMountResult, 4);
                     }
                  }
                  break;
               case E228Enum.END:
               default:
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): reached default" + myState.ToString());
                  return false;
            }
         }
         return true;
      }
      //-----------------------------------------------------------------------------------------
      private Button CreateButton(IMapItem mi)
      {
         System.Windows.Controls.Button b = new Button { };
         b.Name = Utilities.RemoveSpaces(mi.Name);
         b.Width = Utilities.ZOOM * Utilities.theMapItemSize;
         b.Height = Utilities.ZOOM * Utilities.theMapItemSize;
         b.BorderThickness = new Thickness(1);
         b.BorderBrush = Brushes.Black;
         b.Background = new SolidColorBrush(Colors.Transparent);
         b.Foreground = new SolidColorBrush(Colors.Transparent);
         MapItem.SetButtonContent(b, mi, false, true); // This sets the image as the button's content
         return b;
      }
      public void ShowDieResults(int dieRoll)
      {
         //-----------------------------------------------------------------
         int k = myRollResulltRowNum - STARTING_ASSIGNED_ROW;
         if (k < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): k < 0 for " + myState.ToString());
            return;
         }
         switch (myState)
         {
            case E228Enum.RETURN_CHECK:
               myGridRows[k].myReuniteDieRoll = dieRoll;
               myState = E228Enum.SHOW_REUNITE_RESULTS;
               for (int i = 0; i < myMaxRowCount; i++)
               {
                  if (Utilities.NO_RESULT == myGridRows[i].myReuniteDieRoll)
                     myState = E228Enum.RETURN_CHECK;
               }
               break;
            case E228Enum.MOUNT_CHECK:
               myGridRows[k].myMountDieRoll = dieRoll;
               myState = E228Enum.SHOW_MOUNT_RESULTS;
               for (int i = 0; i < myMaxRowCount; i++)
               {
                  if (Utilities.NO_RESULT == myGridRows[i].myMountDieRoll)
                     myState = E228Enum.MOUNT_CHECK;
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): reached default myState=" + myState.ToString());
               break;
         }
         //-----------------------------------------------------------------
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): UpdateGrid() return false");
         myIsRollInProgress = false;
      }
      //-----------------------------------------------------------------------------------------
      private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
      {
         System.Windows.Point p = e.GetPosition((UIElement)sender);
         HitTestResult result = VisualTreeHelper.HitTest(myGrid, p);  // Get the Point where the hit test occurrs
         foreach (UIElement ui in myGrid.Children)
         {
            if (ui is StackPanel panel)
            {
               foreach (UIElement ui1 in panel.Children)
               {
                  if (ui1 is Image img) // Check all images within the myStackPanelAssignable
                  {
                     if (result.VisualHit == img)
                     {
                        if ("Campfire" == img.Name)
                           myState = E228Enum.END;
                        if ("Mount" == img.Name)
                           myState = E228Enum.MOUNT_CHECK;
                        if (false == UpdateGrid())
                           Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                        return;
                     }
                  }
               }
            }
            if (ui is Image img1) // next check all images within the Grid Rows
            {
               if (result.VisualHit == img1)
               {
                  if (false == myIsRollInProgress)
                  {
                     myRollResulltRowNum = Grid.GetRow(img1);
                     myIsRollInProgress = true;
                     myDieRoller.RollMovingDice(myCanvas, ShowDieResults);
                     img1.Visibility = Visibility.Hidden;
                     return;
                  }
               }
            }
         }
      }
      private void ButtonRule_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         String content = (String)b.Content;
         if (null == myRulesMgr)
            Logger.Log(LogEnum.LE_ERROR, "ButtonRule_Click(): myRulesMgr=null");
         else if (false == myRulesMgr.ShowRule(content))
            Logger.Log(LogEnum.LE_ERROR, "ButtonRule_Click(): myRulesMgr.ShowRule() returned false for c=" + content);
      }
   }
}
