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
   public partial class EventViewerE043Mgr : UserControl
   {
      public delegate bool EndE043Callback();
      private const int STARTING_ASSIGNED_ROW = 6;
      //---------------------------------------------
      public struct GridRow
      {
         public IMapItem myMapItem;
         public int myDieRoll;
         public GridRow(IMapItem mi)
         {
            myMapItem = mi;
            myDieRoll = Utilities.NO_RESULT;
         }
      };
      public enum E043Enum
      {
         ROLL_ITEM,
         ROLL_SPELL,
         CHOOSE_VICTIM,
         CHOOSE_VICTIM_ROLLER,
         MAGIC_SHOCK_SPELL_BEFORE,
         MAGIC_SHOCK_SPELL_AFTER,
         MAGIC_FIRE_SPELL_BEFORE,
         MAGIC_FIRE_SPELL_AFTER,
         POISON_AIR_SPELL,
         FEAR_SPELL,
         SHOW_RESULTS,
         END
      };
      //---------------------------------------------
      public bool CtorError { get; } = false;
      private SpecialEnum myItemOnAltar = SpecialEnum.None;
      private int myDieRollForSpell = Utilities.NO_RESULT;
      private int myRollResultRowNum = 0;
      private string myItemName = "";
      //---------------------------------------------
      private E043Enum myState = E043Enum.ROLL_ITEM;
      private EndE043Callback myCallback = null;
      private int myMaxRowCount = 0;
      private GridRow[] myGridRows = null;
      //---------------------------------------------
      private IGameInstance myGameInstance = null;
      private readonly Canvas myCanvas = null;
      private readonly ScrollViewer myScrollViewer = null;
      private RuleDialogViewer myRulesMgr = null;
      //---------------------------------------------
      private IDieRoller myDieRoller = null;
      private bool myIsRollInProgress = false;
      //---------------------------------------------
      private readonly SolidColorBrush mySolidColorBrushBlack = new SolidColorBrush() { Color = Colors.Black };
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      private readonly FontFamily myFontFam1 = new FontFamily("Courier New");
      //-----------------------------------------------------------------------------------------
      public EventViewerE043Mgr(IGameInstance gi, Canvas c, ScrollViewer sv, RuleDialogViewer rdv, IDieRoller dr)
      {
         InitializeComponent();
         //--------------------------------------------------
         if (null == gi) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE043Mgr(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //--------------------------------------------------
         if (null == c) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE043Mgr(): c=null");
            CtorError = true;
            return;
         }
         myCanvas = c;
         //--------------------------------------------------
         if (null == sv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE043Mgr(): sv=null");
            CtorError = true;
            return;
         }
         myScrollViewer = sv;
         //--------------------------------------------------
         if (null == rdv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE043Mgr(): rdv=null");
            CtorError = true;
            return;
         }
         myRulesMgr = rdv;
         //--------------------------------------------------
         if (null == dr)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerE043Mgr(): dr=true");
            CtorError = true;
            return;
         }
         myDieRoller = dr;
         //--------------------------------------------------
         myGrid.MouseDown += Grid_MouseDown;
      }
      public bool CheckRetreival(EndE043Callback callback)
      {
         if (null == myGameInstance.PartyMembers)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckRetreival(): partyMembers=null");
            return false;
         }
         if (myGameInstance.PartyMembers.Count < 1) // at a minimum, the prince needs to be in party
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckRetreival(): myGameInstance.PartyMembers.Count < 1");
            return false;
         }
         //--------------------------------------------------
         myGridRows = new GridRow[Utilities.MAX_GRID_ROW];
         myState = E043Enum.ROLL_ITEM;
         myMaxRowCount = myGameInstance.PartyMembers.Count;
         myIsRollInProgress = false;
         myRollResultRowNum = 0;
         myCallback = callback;
         myItemOnAltar = SpecialEnum.None;
         myDieRollForSpell = Utilities.NO_RESULT;
         //--------------------------------------------------
         int i = 0;
         IMapItem prince = null;
         foreach (IMapItem mi in myGameInstance.PartyMembers)
         {
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "CheckRetreival(): mi=null");
               return false;
            }
            if ("Prince" == mi.Name)
               prince = mi;
            myGridRows[i] = new GridRow(mi);
            ++i;
         }
         if (null == prince)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckRetreival(): prince=null");
            return false;
         }
         //--------------------------------------------------
         // Add the unassignable mapitems that never move or change to the Grid Rows
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckRetreival(): UpdateGrid() return false");
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
         if (E043Enum.END == myState)
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
         if (E043Enum.END == myState)
         {
            if (null == myCallback)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateEndState(): myCallback=null");
               return false;
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
            case E043Enum.ROLL_ITEM:
               myTextBlockInstructions.Inlines.Add(new Run("Click campfire or roll die to determine item."));
               break;
            case E043Enum.ROLL_SPELL:
               myTextBlockInstructions.Inlines.Add(new Run(myItemName + ": Roll die to determine spell surrounding item."));
               break;
            case E043Enum.CHOOSE_VICTIM_ROLLER:
               myTextBlockInstructions.Inlines.Add(new Run(myItemName + ": Click campfire or roll for spell effect."));
               break;
            case E043Enum.CHOOSE_VICTIM:
               myTextBlockInstructions.Inlines.Add(new Run(myItemName + ": Click campfire or select victim."));
               break;
            case E043Enum.MAGIC_SHOCK_SPELL_BEFORE:
               myTextBlockInstructions.Inlines.Add(new Run("Leave Item. Click campfire to continue."));
               break;
            case E043Enum.MAGIC_SHOCK_SPELL_AFTER:
               myTextBlockInstructions.Inlines.Add(new Run("One wound and repulsed. Click campfire to continue."));
               break;
            case E043Enum.MAGIC_FIRE_SPELL_BEFORE:
               myTextBlockInstructions.Inlines.Add(new Run("Roll for wounds entering fire."));
               break;
            case E043Enum.MAGIC_FIRE_SPELL_AFTER:
               myTextBlockInstructions.Inlines.Add(new Run("Item Retreived. Roll for wounds again."));
               break;
            case E043Enum.POISON_AIR_SPELL:
               myTextBlockInstructions.Inlines.Add(new Run("Roll for poison wounds."));
               break;
            case E043Enum.FEAR_SPELL:
               myTextBlockInstructions.Inlines.Add(new Run("Scared but item retreived & given to Prince. Click fire to continue"));
               break;
            case E043Enum.SHOW_RESULTS:
               myTextBlockInstructions.Inlines.Add(new Run("Item Retreived. Click campfire to continue"));
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateUserInstructions(): reached default state=" + myState.ToString());
               return false;
         }
         return true;
      }
      private bool UpdateAssignablePanel()
      {
         myStackPanelAssignable.Children.Clear(); // clear out assignable panel 
         switch (myState)
         {
            case E043Enum.ROLL_ITEM:
               BitmapImage bmi0 = new BitmapImage();
               bmi0.BeginInit();
               bmi0.UriSource = new Uri("../../Images/CampFire2.gif", UriKind.Relative);
               bmi0.EndInit();
               Image img0 = new Image { Tag = "Campfire", Source = bmi0, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               ImageBehavior.SetAnimatedSource(img0, bmi0);
               myStackPanelAssignable.Children.Add(img0);
               //-----------------------------------------
               Rectangle r0 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemOffset };
               myStackPanelAssignable.Children.Add(r0);
               //-----------------------------------------
               BitmapImage bmi1 = new BitmapImage();
               bmi1.BeginInit();
               bmi1.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
               bmi1.EndInit();
               Image img1 = new Image { Tag = "DieRoll", Source = bmi1, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               ImageBehavior.SetAnimatedSource(img1, bmi1);
               myStackPanelAssignable.Children.Add(img1);
               break;
            case E043Enum.ROLL_SPELL:
               Rectangle r0a = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r0a);
               //-----------------------------------------
               Rectangle r2 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemOffset };
               myStackPanelAssignable.Children.Add(r2);
               //-----------------------------------------
               if (false == UpdateAssignablePanelShowItem())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanel(ROLL_SPELL): UpdateAssignablePanelShowItem()=false");
                  return false;
               }
               //-----------------------------------------
               Rectangle r3 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemOffset };
               myStackPanelAssignable.Children.Add(r3);
               //-----------------------------------------
               BitmapImage bmi3 = new BitmapImage();
               bmi3.BeginInit();
               bmi3.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
               bmi3.EndInit();
               Image img3 = new Image { Tag = "DieRoll", Source = bmi3, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               ImageBehavior.SetAnimatedSource(img3, bmi3);
               myStackPanelAssignable.Children.Add(img3);
               break;
            case E043Enum.CHOOSE_VICTIM_ROLLER:
               BitmapImage bmi4 = new BitmapImage();
               bmi4.BeginInit();
               bmi4.UriSource = new Uri("../../Images/CampFire2.gif", UriKind.Relative);
               bmi4.EndInit();
               Image img4 = new Image { Tag = "Campfire", Source = bmi4, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               ImageBehavior.SetAnimatedSource(img4, bmi4);
               myStackPanelAssignable.Children.Add(img4);
               //-----------------------------------------
               Rectangle r4 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemOffset };
               myStackPanelAssignable.Children.Add(r4);
               //-----------------------------------------
               if (false == UpdateAssignablePanelShowItem())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanel(CHOOSE_VICTIM): UpdateAssignablePanelShowItem()=false");
                  return false;
               }
               break;
            case E043Enum.CHOOSE_VICTIM:
               BitmapImage bmi4a = new BitmapImage();
               bmi4a.BeginInit();
               bmi4a.UriSource = new Uri("../../Images/CampFire2.gif", UriKind.Relative);
               bmi4a.EndInit();
               Image img4a = new Image { Tag = "Campfire", Source = bmi4a, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               ImageBehavior.SetAnimatedSource(img4a, bmi4a);
               myStackPanelAssignable.Children.Add(img4a);
               //-----------------------------------------
               Rectangle r4a = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemOffset };
               myStackPanelAssignable.Children.Add(r4a);
               //-----------------------------------------
               if (false == UpdateAssignablePanelShowItem())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanel(CHOOSE_VICTIM): UpdateAssignablePanelShowItem()=false");
                  return false;
               }
               //-----------------------------------------
               Rectangle r5 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemOffset };
               myStackPanelAssignable.Children.Add(r5);
               //-----------------------------------------
               if (false == UpdateAssignablePanelShowSpell())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanel(CHOOSE_VICTIM): UpdateAssignablePanelShowSpell()=false");
                  return false;
               }
               break;
            case E043Enum.MAGIC_SHOCK_SPELL_BEFORE:
               BitmapImage bmi5 = new BitmapImage();
               bmi5.BeginInit();
               bmi5.UriSource = new Uri("../../Images/CampFire2.gif", UriKind.Relative);
               bmi5.EndInit();
               Image img5 = new Image { Tag = "Campfire", Source = bmi5, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               ImageBehavior.SetAnimatedSource(img5, bmi5);
               myStackPanelAssignable.Children.Add(img5);
               //-----------------------------------------
               Rectangle r6 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r6);
               //-----------------------------------------
               Rectangle r7 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemOffset };
               myStackPanelAssignable.Children.Add(r7);
               //-----------------------------------------
               if (false == UpdateAssignablePanelShowSpell())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanel(CHOOSE_VICTIM): UpdateAssignablePanelShowSpell()=false");
                  return false;
               }
               break;
            case E043Enum.MAGIC_SHOCK_SPELL_AFTER:
               BitmapImage bmi5a = new BitmapImage();
               bmi5a.BeginInit();
               bmi5a.UriSource = new Uri("../../Images/CampFire2.gif", UriKind.Relative);
               bmi5a.EndInit();
               Image img5a = new Image { Tag = "Campfire", Source = bmi5a, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               ImageBehavior.SetAnimatedSource(img5a, bmi5a);
               myStackPanelAssignable.Children.Add(img5a);
               break;
            case E043Enum.MAGIC_FIRE_SPELL_BEFORE:
            case E043Enum.POISON_AIR_SPELL:
            case E043Enum.MAGIC_FIRE_SPELL_AFTER:
               Rectangle r8 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r8);
               //-----------------------------------------
               Rectangle r9 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemOffset };
               myStackPanelAssignable.Children.Add(r9);
               //-----------------------------------------
               if (false == UpdateAssignablePanelShowItem())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanel(CHOOSE_VICTIM): UpdateAssignablePanelShowItem()=false");
                  return false;
               }
               break;
            case E043Enum.FEAR_SPELL:
            case E043Enum.SHOW_RESULTS:
               BitmapImage bmi6 = new BitmapImage();
               bmi6.BeginInit();
               bmi6.UriSource = new Uri("../../Images/CampFire2.gif", UriKind.Relative);
               bmi6.EndInit();
               Image img6 = new Image { Tag = "Campfire", Source = bmi6, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               ImageBehavior.SetAnimatedSource(img6, bmi6);
               myStackPanelAssignable.Children.Add(img6);
               //-----------------------------------------
               Rectangle r9a = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemOffset };
               myStackPanelAssignable.Children.Add(r9a);
               //-----------------------------------------
               if (false == UpdateAssignablePanelShowItem())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanel(CHOOSE_VICTIM): UpdateAssignablePanelShowItem()=false");
                  return false;
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanel(): reached default s=" + myState.ToString());
               return false;
         }
         return true;
      }
      private bool UpdateAssignablePanelShowItem()
      {
         switch (myItemOnAltar)
         {
            case SpecialEnum.None:
               break;
            case SpecialEnum.MagicSword:
               Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Sword"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img1);
               break;
            case SpecialEnum.CharismaTalisman:
               Image img2 = new Image { Source = MapItem.theMapImages.GetBitmapImage("TalismanCharisma"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img2);
               break;
            case SpecialEnum.ResistanceRing:
               Image img3 = new Image { Source = MapItem.theMapImages.GetBitmapImage("RingResistence"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img3);
               break;
            case SpecialEnum.ResurrectionNecklace:
               Image img4 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Necklace"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img4);
               break;
            case SpecialEnum.ShieldOfLight:
               Image img5 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Shield"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img5);
               break;
            case SpecialEnum.RoyalHelmOfNorthlands:
               Image img6 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Helmet"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img6);
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanelShowItem(): reached default myItemOnAltar=" + myItemOnAltar.ToString());
               return false;
         }
         return true;
      }
      private bool UpdateAssignablePanelShowSpell()
      {
         Label label = new Label() { FontFamily = myFontFam, FontSize = 18, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
         switch (myDieRollForSpell)
         {
            case 1:
            case 2:
               label.Content = "Shock Spell causes wound & prevents entry";
               break;
            case 3:
            case 4:
               label.Content = "Fire spell causes damage before & after";
               break;
            case 5:
               label.Content = "Posion spell causes poison wounds";
               break;
            case 6:
               label.Content = "Fear spell causes shaky knees";
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanelShowSpell(): reached default myItemOnAltar=" + myItemOnAltar.ToString());
               return false;
         }
         myStackPanelAssignable.Children.Add(label);
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
         if (E043Enum.ROLL_ITEM == myState)
         {
            if (false == UpdateGridRowsShowItems())
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateGridRowsShowItems() returned false");
               return false;
            }
            return true;
         }
         //------------------------------------------------------------
         switch (myState)
         {
            case E043Enum.ROLL_ITEM: myTextBlock1.Text = "Die Roll"; myTextBlock2.Text = "Item"; break;
            case E043Enum.ROLL_SPELL: myTextBlock1.Text = ""; myTextBlock2.Text = ""; break;
            case E043Enum.CHOOSE_VICTIM: myTextBlock1.Text = "Member"; myTextBlock2.Text = "Select Victim"; break;
            case E043Enum.CHOOSE_VICTIM_ROLLER: myTextBlock1.Text = "Member"; myTextBlock2.Text = "Roll for Spell"; break;
            case E043Enum.MAGIC_SHOCK_SPELL_BEFORE: myTextBlock1.Text = ""; myTextBlock2.Text = ""; break;
            case E043Enum.MAGIC_SHOCK_SPELL_AFTER: myTextBlock1.Text = "Member"; myTextBlock2.Text = "Wound"; break;
            case E043Enum.MAGIC_FIRE_SPELL_BEFORE: myTextBlock1.Text = "Member"; myTextBlock2.Text = "Roll for Wounds"; break;
            case E043Enum.MAGIC_FIRE_SPELL_AFTER: myTextBlock1.Text = "Member"; myTextBlock2.Text = "Roll for Wounds"; break;
            case E043Enum.POISON_AIR_SPELL: myTextBlock1.Text = "Member"; myTextBlock2.Text = "Roll for Poison"; break;
            case E043Enum.FEAR_SPELL: myTextBlock1.Text = "Member"; myTextBlock2.Text = "Item"; break;
            case E043Enum.SHOW_RESULTS: myTextBlock1.Text = "Member"; myTextBlock2.Text = "Wounds"; break;
            case E043Enum.END: break;
            default: break;
         }
         //------------------------------------------------------------
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            IMapItem mi = myGridRows[i].myMapItem;
            //------------------------------------
            switch (myState)
            {
               case E043Enum.ROLL_SPELL:
                  break;
               case E043Enum.CHOOSE_VICTIM:
                  if ((false == mi.IsUnconscious) && (false == mi.IsKilled))
                  {
                     Button b1 = CreateButton(mi);
                     myGrid.Children.Add(b1);
                     Grid.SetRow(b1, rowNum);
                     Grid.SetColumn(b1, 0);
                     CheckBox cb = new CheckBox() { Tag = i.ToString(), IsEnabled = true, IsChecked = false, FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                     cb.Checked += CheckBox_Checked;
                     myGrid.Children.Add(cb);
                     Grid.SetRow(cb, rowNum);
                     Grid.SetColumn(cb, 1);
                  }
                  break;
               case E043Enum.CHOOSE_VICTIM_ROLLER:
                  if ((false == mi.IsUnconscious) && (false == mi.IsKilled))
                  {
                     Button b2 = CreateButton(mi);
                     myGrid.Children.Add(b2);
                     Grid.SetRow(b2, rowNum);
                     Grid.SetColumn(b2, 0);
                     BitmapImage bmi = new BitmapImage();
                     bmi.BeginInit();
                     bmi.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
                     bmi.EndInit();
                     Image img = new Image { Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                     ImageBehavior.SetAnimatedSource(img, bmi);
                     myGrid.Children.Add(img);
                     Grid.SetRow(img, rowNum);
                     Grid.SetColumn(img, 1);
                  }
                  break;
               case E043Enum.MAGIC_SHOCK_SPELL_BEFORE:
                  break;
               case E043Enum.MAGIC_SHOCK_SPELL_AFTER:
                  if (myRollResultRowNum == rowNum)
                  {
                     Button b3 = CreateButton(mi);
                     myGrid.Children.Add(b3);
                     Grid.SetRow(b3, rowNum);
                     Grid.SetColumn(b3, 0);
                     Label label1 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                     label1.Content = "1";
                     myGrid.Children.Add(label1);
                     Grid.SetRow(label1, rowNum);
                     Grid.SetColumn(label1, 1);
                  }
                  break;
               case E043Enum.MAGIC_FIRE_SPELL_BEFORE:
                  if (myRollResultRowNum == rowNum)
                  {
                     Button b4 = CreateButton(mi);
                     myGrid.Children.Add(b4);
                     Grid.SetRow(b4, rowNum);
                     Grid.SetColumn(b4, 0);
                     BitmapImage bmi1 = new BitmapImage();
                     bmi1.BeginInit();
                     bmi1.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
                     bmi1.EndInit();
                     Image img1 = new Image { Source = bmi1, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                     ImageBehavior.SetAnimatedSource(img1, bmi1);
                     myGrid.Children.Add(img1);
                     Grid.SetRow(img1, rowNum);
                     Grid.SetColumn(img1, 1);
                  }
                  break;
               case E043Enum.MAGIC_FIRE_SPELL_AFTER:
                  if (myRollResultRowNum == rowNum)
                  {
                     Button b4a = CreateButton(mi);
                     myGrid.Children.Add(b4a);
                     Grid.SetRow(b4a, rowNum);
                     Grid.SetColumn(b4a, 0);
                     BitmapImage bmi1 = new BitmapImage();
                     bmi1.BeginInit();
                     bmi1.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
                     bmi1.EndInit();
                     Image img1 = new Image { Source = bmi1, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                     ImageBehavior.SetAnimatedSource(img1, bmi1);
                     myGrid.Children.Add(img1);
                     Grid.SetRow(img1, rowNum);
                     Grid.SetColumn(img1, 1);
                  }
                  break;
               case E043Enum.POISON_AIR_SPELL:
                  if (myRollResultRowNum == rowNum)
                  {
                     Button b5 = CreateButton(mi);
                     myGrid.Children.Add(b5);
                     Grid.SetRow(b5, rowNum);
                     Grid.SetColumn(b5, 0);
                     BitmapImage bmi1 = new BitmapImage();
                     bmi1.BeginInit();
                     bmi1.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
                     bmi1.EndInit();
                     Image img1 = new Image { Source = bmi1, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                     ImageBehavior.SetAnimatedSource(img1, bmi1);
                     myGrid.Children.Add(img1);
                     Grid.SetRow(img1, rowNum);
                     Grid.SetColumn(img1, 1);
                  }
                  break;
               case E043Enum.FEAR_SPELL:
                  if ("Prince" == mi.Name)
                  {
                     Button b6 = CreateButton(mi);
                     myGrid.Children.Add(b6);
                     Grid.SetRow(b6, rowNum);
                     Grid.SetColumn(b6, 0);
                     if (false == UpdateGridRowsShowItem(rowNum))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateGridRowsShowItem() returned false");
                        return false;
                     }
                  }
                  break;
               case E043Enum.SHOW_RESULTS:
                  if (myRollResultRowNum == rowNum)
                  {
                     Button b7 = CreateButton(mi);
                     myGrid.Children.Add(b7);
                     Grid.SetRow(b7, rowNum);
                     Grid.SetColumn(b7, 0);
                     Label label1 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                     label1.Content = myGridRows[i].myDieRoll.ToString();
                     myGrid.Children.Add(label1);
                     Grid.SetRow(label1, rowNum);
                     Grid.SetColumn(label1, 1);
                  }
                  break;
               case E043Enum.END:
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): reached default s=" + myState.ToString());
                  return false;
            }
         }
         return true;
      }
      private bool UpdateGridRowsShowItem(int rowNum)
      {
         switch (myItemOnAltar)
         {
            case SpecialEnum.MagicSword:
               Image img = new Image { Source = MapItem.theMapImages.GetBitmapImage("Sword"), VerticalAlignment = VerticalAlignment.Center, Width = Utilities.theMapItemSize, Height = Utilities.theMapItemSize };
               myGrid.Children.Add(img);
               Grid.SetRow(img, rowNum);
               Grid.SetColumn(img, 1);
               break;
            case SpecialEnum.CharismaTalisman:
               Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("TalismanCharisma"), VerticalAlignment = VerticalAlignment.Center, Width = Utilities.theMapItemSize, Height = Utilities.theMapItemSize };
               myGrid.Children.Add(img1);
               Grid.SetRow(img1, rowNum);
               Grid.SetColumn(img1, 1);
               break;
            case SpecialEnum.ResistanceRing:
               Image img2 = new Image { Source = MapItem.theMapImages.GetBitmapImage("RingResistence"), VerticalAlignment = VerticalAlignment.Center, Width = Utilities.theMapItemSize, Height = Utilities.theMapItemSize };
               myGrid.Children.Add(img2);
               Grid.SetRow(img2, rowNum);
               Grid.SetColumn(img2, 1);
               break;
            case SpecialEnum.ResurrectionNecklace:
               Image img3 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Necklace"), VerticalAlignment = VerticalAlignment.Center, Width = Utilities.theMapItemSize, Height = Utilities.theMapItemSize };
               myGrid.Children.Add(img3);
               Grid.SetRow(img3, rowNum);
               Grid.SetColumn(img3, 1);
               break;
            case SpecialEnum.ShieldOfLight:
               Image img4 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Shield"), VerticalAlignment = VerticalAlignment.Center, Width = Utilities.theMapItemSize, Height = 0.8 * Utilities.theMapItemSize };
               myGrid.Children.Add(img4);
               Grid.SetRow(img4, rowNum);
               Grid.SetColumn(img4, 1);
               break;
            case SpecialEnum.RoyalHelmOfNorthlands:
               Image img5 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Helmet"), VerticalAlignment = VerticalAlignment.Center, Width = Utilities.theMapItemSize, Height = 0.8 * Utilities.theMapItemSize };
               myGrid.Children.Add(img5);
               Grid.SetRow(img5, rowNum);
               Grid.SetColumn(img5, 1);
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRowsShowItem(): reached default item=" + myItemOnAltar.ToString());
               return false;
         }
         return true;
      }
      private bool UpdateGridRowsShowItems()
      {
         //------------------------------------------------------------
         Label label = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "1" };
         myGrid.Children.Add(label);
         Grid.SetRow(label, STARTING_ASSIGNED_ROW + 0);
         Grid.SetColumn(label, 0);
         Image img = new Image { Source = MapItem.theMapImages.GetBitmapImage("Sword"), VerticalAlignment = VerticalAlignment.Center, Width = Utilities.theMapItemSize, Height = Utilities.theMapItemSize };
         myGrid.Children.Add(img);
         Grid.SetRow(img, STARTING_ASSIGNED_ROW + 0);
         Grid.SetColumn(img, 1);
         //------------------------------------------------------------
         Label label1 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "2" };
         myGrid.Children.Add(label1);
         Grid.SetRow(label1, STARTING_ASSIGNED_ROW + 1);
         Grid.SetColumn(label1, 0);
         Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("TalismanCharisma"), VerticalAlignment = VerticalAlignment.Center, Width = Utilities.theMapItemSize, Height = Utilities.theMapItemSize };
         myGrid.Children.Add(img1);
         Grid.SetRow(img1, STARTING_ASSIGNED_ROW + 1);
         Grid.SetColumn(img1, 1);
         //------------------------------------------------------------
         Label label2 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "3" };
         myGrid.Children.Add(label2);
         Grid.SetRow(label2, STARTING_ASSIGNED_ROW + 2);
         Grid.SetColumn(label2, 0);
         Image img2 = new Image { Source = MapItem.theMapImages.GetBitmapImage("RingResistence"), VerticalAlignment = VerticalAlignment.Center, Width = Utilities.theMapItemSize, Height = Utilities.theMapItemSize };
         myGrid.Children.Add(img2);
         Grid.SetRow(img2, STARTING_ASSIGNED_ROW + 2);
         Grid.SetColumn(img2, 1);
         //------------------------------------------------------------
         Label label3 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "4" };
         myGrid.Children.Add(label3);
         Grid.SetRow(label3, STARTING_ASSIGNED_ROW + 3);
         Grid.SetColumn(label3, 0);
         Image img3 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Necklace"), VerticalAlignment = VerticalAlignment.Center, Width = Utilities.theMapItemSize, Height = Utilities.theMapItemSize };
         myGrid.Children.Add(img3);
         Grid.SetRow(img3, STARTING_ASSIGNED_ROW + 3);
         Grid.SetColumn(img3, 1);
         //------------------------------------------------------------
         Label label4 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "5" };
         myGrid.Children.Add(label4);
         Grid.SetRow(label4, STARTING_ASSIGNED_ROW + 4);
         Grid.SetColumn(label4, 0);
         Image img4 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Shield"), VerticalAlignment = VerticalAlignment.Center, Width = Utilities.theMapItemSize, Height = 0.8 * Utilities.theMapItemSize };
         myGrid.Children.Add(img4);
         Grid.SetRow(img4, STARTING_ASSIGNED_ROW + 4);
         Grid.SetColumn(img4, 1);
         //------------------------------------------------------------
         Label label5 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "6" };
         myGrid.Children.Add(label5);
         Grid.SetRow(label5, STARTING_ASSIGNED_ROW + 5);
         Grid.SetColumn(label5, 0);
         Image img5 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Helmet"), VerticalAlignment = VerticalAlignment.Center, Width = Utilities.theMapItemSize, Height = 0.8 * Utilities.theMapItemSize };
         myGrid.Children.Add(img5);
         Grid.SetRow(img5, STARTING_ASSIGNED_ROW + 5);
         Grid.SetColumn(img5, 1);
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
         if (E043Enum.ROLL_ITEM == myState)
         {
            switch (dieRoll)
            {
               case 1: myItemOnAltar = SpecialEnum.MagicSword; myItemName = "Magic Sword"; break;
               case 2: myItemOnAltar = SpecialEnum.CharismaTalisman; myItemName = "Charisma Talisman"; break;
               case 3: myItemOnAltar = SpecialEnum.ResistanceRing; myItemName = "Resistance Ring"; break;
               case 4: myItemOnAltar = SpecialEnum.ResurrectionNecklace; myItemName = "Resurrection Necklass"; break;
               case 5: myItemOnAltar = SpecialEnum.ShieldOfLight; myItemName = "Shield of Light"; break;
               case 6: myItemOnAltar = SpecialEnum.RoyalHelmOfNorthlands; myItemName = "Helm of Northlands"; break;
               default: Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): reached default" + myState.ToString()); return;
            }
            if (true == myGameInstance.IsMagicInParty())
               myState = E043Enum.ROLL_SPELL;
            else
               myState = E043Enum.CHOOSE_VICTIM_ROLLER;
         }
         else if (E043Enum.ROLL_SPELL == myState)
         {
            myDieRollForSpell = dieRoll;
            switch (dieRoll)
            {
               case 1: myState = E043Enum.MAGIC_SHOCK_SPELL_BEFORE; myItemOnAltar = SpecialEnum.None; break;
               case 2: myState = E043Enum.MAGIC_SHOCK_SPELL_BEFORE; myItemOnAltar = SpecialEnum.None; break;
               case 3: myState = E043Enum.CHOOSE_VICTIM; break;
               case 4: myState = E043Enum.CHOOSE_VICTIM; break;
               case 5: myState = E043Enum.CHOOSE_VICTIM; break;
               case 6: myState = E043Enum.FEAR_SPELL; myGameInstance.AddSpecialItem(myItemOnAltar); break; // give to prince
               default: Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): reached default" + myState.ToString()); return;
            }
         }
         else if (E043Enum.CHOOSE_VICTIM_ROLLER == myState)
         {
            int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
            if (i < 0)
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): invalid state i=" + i.ToString());
               return;
            }
            myDieRollForSpell = dieRoll;
            switch (dieRoll)
            {
               case 1:
               case 2: // a magic shock spell gives one wound and prevents getting item
                  myState = E043Enum.MAGIC_SHOCK_SPELL_AFTER;
                  myItemOnAltar = SpecialEnum.None;
                  IMapItem mi = myGridRows[i].myMapItem;
                  mi.SetWounds(1, 0);
                  break;
               case 3: myState = E043Enum.MAGIC_FIRE_SPELL_BEFORE; break;
               case 4: myState = E043Enum.MAGIC_FIRE_SPELL_BEFORE; break;
               case 5: myState = E043Enum.POISON_AIR_SPELL; break;
               case 6: myState = E043Enum.FEAR_SPELL; myGameInstance.AddSpecialItem(myItemOnAltar); break; // give to prince
               default: Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): reached default" + myState.ToString()); return;
            }
         }
         else if (E043Enum.MAGIC_FIRE_SPELL_BEFORE == myState)
         {
            int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
            if (i < 0)
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): invalid state i=" + i.ToString());
               return;
            }
            IMapItem mi = myGridRows[i].myMapItem;
            mi.SetWounds(dieRoll, 0);
            myGridRows[i].myDieRoll = dieRoll;
            if (false == mi.IsKilled)
               myState = E043Enum.MAGIC_FIRE_SPELL_AFTER;
            else
               myState = E043Enum.CHOOSE_VICTIM;
         }
         else if (E043Enum.MAGIC_FIRE_SPELL_AFTER == myState)
         {
            int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
            if (i < 0)
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): invalid state i=" + i.ToString());
               return;
            }
            IMapItem mi = myGridRows[i].myMapItem;
            mi.SetWounds(dieRoll, 0);
            myGridRows[i].myDieRoll += dieRoll;
            myState = E043Enum.SHOW_RESULTS;
            if (false == mi.IsKilled)
               myGameInstance.AddSpecialItem(myItemOnAltar, mi);
            else
               myGameInstance.AddSpecialItem(myItemOnAltar);
         }
         else if (E043Enum.POISON_AIR_SPELL == myState)
         {
            int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
            if (i < 0)
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): invalid state i=" + i.ToString());
               return;
            }
            IMapItem mi = myGridRows[i].myMapItem;
            mi.SetWounds(0, dieRoll);
            myGridRows[i].myDieRoll = dieRoll;
            myState = E043Enum.SHOW_RESULTS;
            if (false == mi.IsKilled)
               myGameInstance.AddSpecialItem(myItemOnAltar, mi);
            else
               myGameInstance.AddSpecialItem(myItemOnAltar);
         }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): Reached Default myState=" + myState.ToString());
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): UpdateGrid() return false");
         myIsRollInProgress = false;
      }
      //-----------------------------------------------------------------------------------------
      private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
      {
         Point p = e.GetPosition((UIElement)sender);
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
                        string name = (string)img.Tag;
                        if ("Campfire" == name)
                           myState = E043Enum.END;
                        if ("DieRoll" == name)
                        {
                           if (false == myIsRollInProgress)
                           {
                              myIsRollInProgress = true;
                              RollEndCallback callback = ShowDieResults;
                              myDieRoller.RollMovingDie(myCanvas, callback);
                              img.Visibility = Visibility.Hidden;
                           }
                           return;
                        }
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
                     myRollResultRowNum = Grid.GetRow(img1);
                     myIsRollInProgress = true;
                     RollEndCallback callback = ShowDieResults;
                     myDieRoller.RollMovingDie(myCanvas, callback);
                     img1.Visibility = Visibility.Hidden;
                  }
                  return;
               }
            }
         }
      }
      private void ButtonRule_Click(object sender, RoutedEventArgs e)
      {
         if (null == myRulesMgr)
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonRule_Click(): myRulesMgr=null");
            return;
         }
         Button b = (Button)sender;
         string key = (string)b.Content;
         if (false == myRulesMgr.ShowRule(key))
            Logger.Log(LogEnum.LE_ERROR, "ButtonRule_Click(): myRulesMgr.ShowRule() returned false key=" + key);
      }
      private void CheckBox_Checked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         cb.IsChecked = true;
         myRollResultRowNum = Grid.GetRow(cb);
         int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
         if (i < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBox_Checked(): invalid state myRollResultRowNum=" + myDieRollForSpell.ToString());
            return;
         }
         IMapItem mi = myGridRows[i].myMapItem;
         switch (myDieRollForSpell)
         {
            case 1:
            case 2:
               mi.SetWounds(1, 0);
               myState = E043Enum.MAGIC_SHOCK_SPELL_BEFORE;
               break;
            case 3:
            case 4:
               myState = E043Enum.MAGIC_FIRE_SPELL_BEFORE;
               break;
            case 5:
               myState = E043Enum.POISON_AIR_SPELL;
               myGameInstance.AddSpecialItem(myItemOnAltar, mi);
               break;
            case 6:
               myState = E043Enum.FEAR_SPELL;
               myGameInstance.AddSpecialItem(myItemOnAltar, mi);
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "CheckBox_Checked(): reached default dr=" + myDieRollForSpell.ToString());
               break;
         }

         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBox_Checked(): UpdateGrid() return false");
      }
   }
}
