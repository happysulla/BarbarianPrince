using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace BarbarianPrince
{
   class StatusBarViewer : IView
   {
      private readonly StatusBar myStatusBar;
      private IGameInstance myGameInstance = null;
      private IGameEngine myGameEngine = null;
      private Canvas myCanvas = null;
      private Cursor myTargetCursor = null;
      private bool myIsCoinShown = false;
      private bool myIsWitAndWilesShown = false;
      //---------------------------------------------
      private readonly SolidColorBrush mySolidColorBrushBlack = new SolidColorBrush() { Color = Colors.Black };
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      private readonly FontFamily myFontFam1 = new FontFamily("Courier New");
      //------------------------------------------------------------------------------------------------------
      public StatusBarViewer(StatusBar sb, IGameEngine ge, IGameInstance gi, Canvas c)
      {
         myStatusBar = sb;
         myGameInstance = gi;
         myGameEngine = ge;
         myCanvas = c;
      }
      //-----------------------------------------------------------------
      public void UpdateView(ref IGameInstance gi, GameAction action)
      {
         if (false == myIsCoinShown) // If autostart option is selected, show the party's coin and Wits and Wiles images
         {
            IOption option = gi.Options.Find("AutoSetup");
            if (null != option)
            {
               if (true == option.IsEnabled)
               {
                  myIsCoinShown = true;
                  myIsWitAndWilesShown = true;
               }
            }
         }
         //-------------------------------------------------------
         if ((null != myTargetCursor) && (GameAction.UpdateStatusBar == action)) // increase/decrease size of cursor when zoom in or out
         {
            myTargetCursor.Dispose();
            double sizeCursor = Utilities.ZoomCanvas * Utilities.ZOOM * Utilities.theMapItemSize;
            Point hotPoint = new Point(Utilities.theMapItemOffset, sizeCursor * 0.5); // set the center of the MapItem as the hot point for the cursor
            Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Target"), Width = sizeCursor, Height = sizeCursor };
            myTargetCursor = Utilities.ConvertToCursor(img1, hotPoint);
            this.myCanvas.Cursor = myTargetCursor;
         }
         //-------------------------------------------------------
         Logger.Log(LogEnum.LE_VIEW_UPDATE_STATUS_BAR, "---------------StatusBarViewer::UpdateView() ==> a=" + action.ToString());
         switch (action)
         {
            case GameAction.EncounterLootStart:
               myIsCoinShown = true;
               break;
            case GameAction.SetupRollWitsWiles:
               myIsWitAndWilesShown = true;
               break;
            case GameAction.E045ArchOfTravel:
            case GameAction.E156MayorTerritorySelection:
               double sizeCursor = Utilities.ZoomCanvas * Utilities.ZOOM * Utilities.theMapItemSize;
               Point hotPoint = new Point(Utilities.theMapItemOffset, sizeCursor * 0.5); // set the center of the MapItem as the hot point for the cursor
               Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Target"), Width = sizeCursor, Height = sizeCursor };
               myTargetCursor = Utilities.ConvertToCursor(img1, hotPoint);
               this.myCanvas.Cursor = myTargetCursor; // set the cursor in the canvas
               break;
            case GameAction.E045ArchOfTravelEnd:
            case GameAction.E156MayorTerritorySelectionEnd:
               if (null != myTargetCursor)
                  myTargetCursor.Dispose();
               myTargetCursor = null;
               this.myCanvas.Cursor = Cursors.Arrow; // get rid of the canvas cursor
               break;
            default:
               break;
         }
         //--------------------------------------------
         myStatusBar.Items.Clear();
         System.Windows.Controls.Button buttonZoomIn = new System.Windows.Controls.Button { Content=" - ", FontFamily=myFontFam1, Height = 15, Width = 30 };
         buttonZoomIn.Click += ButtonZoomIn_Click;
         myStatusBar.Items.Add(buttonZoomIn);
         Label labelOr = new Label() { FontFamily = myFontFam, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Content = "or" };
         myStatusBar.Items.Add(labelOr);
         System.Windows.Controls.Button buttonZoomOut = new System.Windows.Controls.Button { Content = " + ", FontFamily = myFontFam1, Height = 15, Width = 30 };
         buttonZoomOut.Click += ButtonZoomOut_Click;
         myStatusBar.Items.Add(buttonZoomOut);
         StringBuilder sbZ = new StringBuilder("Zoom=");
         sbZ.Append(Utilities.ZoomCanvas.ToString("#.##"));
         Label labelZoom = new Label() { FontFamily = myFontFam, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Content = sbZ.ToString() };
         myStatusBar.Items.Add(labelZoom);
         //--------------------------------------------
         myStatusBar.Items.Add(new Separator());
         Label labelGoto = new Label() { FontFamily = myFontFam, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Content = "Goto:" };
         myStatusBar.Items.Add(labelGoto);
         System.Windows.Controls.Button buttonGoto = new System.Windows.Controls.Button { Content = myGameInstance.EventActive, FontFamily = myFontFam1, Height = 15, Width = 40 };
         if (true == gi.IsGridActive)
            buttonGoto.IsEnabled = false;
         else
            buttonGoto.IsEnabled = true;
         buttonGoto.Click += ButtonEventActive_Click;
         myStatusBar.Items.Add(buttonGoto);
         //--------------------------------------------
         myStatusBar.Items.Add(new Separator());
         int foodSum = 0;
         foreach (IMapItem mi in gi.PartyMembers)
            foodSum += mi.Food;
         StringBuilder sbF = new StringBuilder("Food=");
         sbF.Append(foodSum);
         Label labelFood = new Label() { FontFamily = myFontFam, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Content = sbF.ToString() };
         Image imgFood = new Image { Source = MapItem.theMapImages.GetBitmapImage("Food"), Width = 30, Height = 30 };
         myStatusBar.Items.Add(labelFood);
         myStatusBar.Items.Add(imgFood);
         //--------------------------------------------
         if (true == myIsCoinShown)
         {
            myStatusBar.Items.Add(new Separator());
            int coinSum = 0;
            foreach (IMapItem mi in gi.PartyMembers)
               coinSum += mi.Coin;
            StringBuilder sbW = new StringBuilder("Coin=");
            sbW.Append(coinSum);
            Label labelCoin = new Label() { FontFamily = myFontFam, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Content = sbW.ToString() };
            Image imgCoin = new Image { Source = MapItem.theMapImages.GetBitmapImage("Coin"), Width = 30, Height = 30 };
            myStatusBar.Items.Add(labelCoin);
            myStatusBar.Items.Add(imgCoin);
         }
         //--------------------------------------------
         if (true == myIsWitAndWilesShown)
         {
            myStatusBar.Items.Add(new Separator());
            StringBuilder sbWW = new StringBuilder("Wit & Wiles=");
            sbWW.Append(gi.WitAndWile.ToString());
            Label labelWW = new Label() { FontFamily = myFontFam, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Content = sbWW.ToString() };
            Image imgWW = new Image { Source = MapItem.theMapImages.GetBitmapImage("Brain1"), Width = 30, Height = 30 };
            myStatusBar.Items.Add(labelWW);
            myStatusBar.Items.Add(imgWW);
         }
         //--------------------------------------------
         if (0 < gi.LetterOfRecommendations.Count)
         {
            myStatusBar.Items.Add(new Separator());
            StringBuilder sbLetter = new StringBuilder(" Letters=");
            sbLetter.Append(gi.LetterOfRecommendations.Count);
            Label labelLetter = new Label() { FontFamily = myFontFam, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Content = sbLetter.ToString() };
            Image imgLetter = new Image { Source = MapItem.theMapImages.GetBitmapImage("Letter"), Width = 40, Height = 25 };
            myStatusBar.Items.Add(labelLetter);
            myStatusBar.Items.Add(imgLetter);
         }
         //--------------------------------------------
         if ((true == gi.IsSecretTempleKnown) || (0 < gi.ChagaDrugCount) )
         {
            myStatusBar.Items.Add(new Separator());
            StringBuilder sbCD = new StringBuilder("Chaga=");
            sbCD.Append(gi.ChagaDrugCount);
            Label labelChaga = new Label() { FontFamily = myFontFam, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Content = sbCD.ToString() };
            Image imgChaga = new Image { Source = MapItem.theMapImages.GetBitmapImage("DrugChaga"), Width = 30, Height = 30 };
            myStatusBar.Items.Add(labelChaga);
            myStatusBar.Items.Add(imgChaga);
         }
         //--------------------------------------------
         if (0 < gi.HydraTeethCount)
         {
            myStatusBar.Items.Add(new Separator());
            StringBuilder sbTeeth = new StringBuilder(" Hydra Teeth=");
            sbTeeth.Append(gi.HydraTeethCount);
            Label labelTeeth = new Label() { FontFamily = myFontFam, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Content = sbTeeth.ToString() };
            Image imgTeeth = new Image { Source = MapItem.theMapImages.GetBitmapImage("Teeth"), Width = 40, Height = 30 };
            myStatusBar.Items.Add(labelTeeth);
            myStatusBar.Items.Add(imgTeeth);
         }
         //--------------------------------------------
         if (true == gi.IsBlessed)
         {
            myStatusBar.Items.Add(new Separator());
            Label labelBlessed = new Label() { FontFamily = myFontFam, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Content = " Blessed" };
            Image imgBlessed = new Image { Source = MapItem.theMapImages.GetBitmapImage("God"), Width = 15, Height = 30 };
            myStatusBar.Items.Add(labelBlessed);
            myStatusBar.Items.Add(imgBlessed);
         }
         //--------------------------------------------
         if (true == gi.IsExhausted)
         {
            myStatusBar.Items.Add(new Separator());
            Label labelBlessed = new Label() { FontFamily = myFontFam, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Content = " Exhausted" };
            Image imgBlessed = new Image { Source = MapItem.theMapImages.GetBitmapImage("Exhausted"), Width = 20, Height = 30 };
            myStatusBar.Items.Add(labelBlessed);
            myStatusBar.Items.Add(imgBlessed);
         }
         //--------------------------------------------
         if (true == gi.IsMerchantWithParty)
         {
            myStatusBar.Items.Add(new Separator());
            Label labelBlessed = new Label() { FontFamily = myFontFam, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Content = " Negotiator" };
            Image imgBlessed = new Image { Source = MapItem.theMapImages.GetBitmapImage("Negotiator"), Width = 30, Height = 30 };
            myStatusBar.Items.Add(labelBlessed);
            myStatusBar.Items.Add(imgBlessed);
         }
         //--------------------------------------------
         if (true == gi.IsMarkOfCain)
         {
            myStatusBar.Items.Add(new Separator());
            Label labelMarkOfCain = new Label() { FontFamily = myFontFam, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Content = " No Monks/Priests" };
            Image imgMarkOfCain = new Image { Source = MapItem.theMapImages.GetBitmapImage("MarkOfCain"), Width = 15, Height = 30 };
            myStatusBar.Items.Add(labelMarkOfCain);
            myStatusBar.Items.Add(imgMarkOfCain);
         }
         //--------------------------------------------
         if (true == gi.IsSpecialItemHeld(SpecialEnum.StaffOfCommand))
         {
            myStatusBar.Items.Add(new Separator());
            Label labelStaff = new Label() { FontFamily = myFontFam, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Content = " Staff of Command" };
            Image imgStaff = new Image { Source = MapItem.theMapImages.GetBitmapImage("Staff"), Width = 15, Height = 30 };
            myStatusBar.Items.Add(labelStaff);
            myStatusBar.Items.Add(imgStaff);
         }
         //--------------------------------------------
         if (true == gi.IsSpecialItemHeld(SpecialEnum.TrollSkin))
         {
            myStatusBar.Items.Add(new Separator());
            StringBuilder sbTrollSkins = new StringBuilder(" Troll Skins=");
            sbTrollSkins.Append(gi.GetCountSpecialItem(SpecialEnum.TrollSkin));
            Label labelTrollSkin = new Label() { FontFamily = myFontFam, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Content = sbTrollSkins.ToString() };
            Image imgTrollSkin = new Image { Source = MapItem.theMapImages.GetBitmapImage("TrollSkin"), Width = 40, Height = 20 };
            myStatusBar.Items.Add(labelTrollSkin);
            myStatusBar.Items.Add(imgTrollSkin);
         }
         //--------------------------------------------
         if (true == gi.IsSpecialItemHeld(SpecialEnum.MagicBox))
         {
            myStatusBar.Items.Add(new Separator());
            StringBuilder sbMagicBox = new StringBuilder(" Magic Box=");
            sbMagicBox.Append(gi.GetCountSpecialItem(SpecialEnum.MagicBox));
            Label labelBox = new Label() { FontFamily = myFontFam, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Content = sbMagicBox.ToString() };
            Image imgBox = new Image { Source = MapItem.theMapImages.GetBitmapImage("BoxUnopened"), Width = 40, Height = 30 };
            myStatusBar.Items.Add(labelBox);
            myStatusBar.Items.Add(imgBox);
         }
         //--------------------------------------------
         if (true == gi.IsSpecialItemHeld(SpecialEnum.DragonEye))
         {
            myStatusBar.Items.Add(new Separator());
            StringBuilder sbDragonEye = new StringBuilder(" Dragon Eye=");
            sbDragonEye.Append(gi.GetCountSpecialItem(SpecialEnum.DragonEye));
            Label labelEye = new Label() { FontFamily = myFontFam, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Content = sbDragonEye.ToString() };
            Image imgEye = new Image { Source = MapItem.theMapImages.GetBitmapImage("DragonEye"), Width = 30, Height = 30 };
            myStatusBar.Items.Add(labelEye);
            myStatusBar.Items.Add(imgEye);
         }
         //--------------------------------------------
         if (true == gi.IsSpecialItemHeld(SpecialEnum.RocBeak))
         {
            myStatusBar.Items.Add(new Separator());
            StringBuilder sbRocBeak = new StringBuilder(" Roc Beak=");
            sbRocBeak.Append(gi.GetCountSpecialItem(SpecialEnum.RocBeak));
            Label labelBeak= new Label() { FontFamily = myFontFam, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Content = sbRocBeak.ToString() };
            Image imgBeak = new Image { Source = MapItem.theMapImages.GetBitmapImage("RocBeak"), Width = 30, Height = 30 };
            myStatusBar.Items.Add(labelBeak);
            myStatusBar.Items.Add(imgBeak);
         }         
         //--------------------------------------------
         if (true == gi.IsSpecialItemHeld(SpecialEnum.GriffonClaws))
         {
            myStatusBar.Items.Add(new Separator());
            StringBuilder sbGriffonClaws = new StringBuilder(" Claws=");
            sbGriffonClaws.Append(gi.GetCountSpecialItem(SpecialEnum.GriffonClaws));
            Label labelBeak = new Label() { FontFamily = myFontFam, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Content = sbGriffonClaws.ToString() };
            Image imgBeak = new Image { Source = MapItem.theMapImages.GetBitmapImage("GriffonClaw"), Width = 30, Height = 30 };
            myStatusBar.Items.Add(labelBeak);
            myStatusBar.Items.Add(imgBeak);
         }
         //--------------------------------------------
         if (true == gi.IsSpecialItemHeld(SpecialEnum.RoyalHelmOfNorthlands))
         {
            myStatusBar.Items.Add(new Separator());
            StringBuilder sbRoyalHelmOfNorthland = new StringBuilder(" Helm=");
            sbRoyalHelmOfNorthland.Append(gi.GetCountSpecialItem(SpecialEnum.RoyalHelmOfNorthlands));
            Label labelHelm = new Label() { FontFamily = myFontFam, FontSize = 12, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Content = sbRoyalHelmOfNorthland.ToString() };
            Image imgHelm = new Image { Source = MapItem.theMapImages.GetBitmapImage("Helmet"), Width = 30, Height = 30 };
            myStatusBar.Items.Add(labelHelm);
            myStatusBar.Items.Add(imgHelm);
         }
      }
      //-----------------------------------------------------------------
      private void ButtonEventActive_Click(object sender, RoutedEventArgs e)
      {
         GameAction action = GameAction.UpdateEventViewerActive;
         myGameEngine.PerformAction(ref myGameInstance, ref action);
      }
      private void ButtonZoomOut_Click(object sender, RoutedEventArgs e)
      {
         Utilities.ZoomCanvas += 0.25;
         myCanvas.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas);
         UpdateView(ref myGameInstance, GameAction.UpdateStatusBar);
      }
      private void ButtonZoomIn_Click(object sender, RoutedEventArgs e)
      {
         Utilities.ZoomCanvas -= 0.25;
         myCanvas.LayoutTransform = new ScaleTransform(Utilities.ZoomCanvas, Utilities.ZoomCanvas);
         UpdateView(ref myGameInstance, GameAction.UpdateStatusBar);
      }
   }
}
