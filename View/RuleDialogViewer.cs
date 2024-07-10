using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace BarbarianPrince
{
   public class RuleDialogViewer
   {
      public bool CtorError { get; } = false;
      private Dictionary<string, string> myRules = null;
      public Dictionary<string, string> Rules { get => myRules; }
      private Dictionary<string, string> myTables = null;
      public Dictionary<string, string> Tables { get => myTables; }
      private Dictionary<string, TableDialog> myTableDialogs = new Dictionary<string, TableDialog>();
      private Dictionary<string, BannerDialog> myBannerDialogs = new Dictionary<string, BannerDialog>();
      private IGameEngine myGameEngine = null;
      private IGameInstance myGameInstance = null;
      public IGameInstance GameInstance{ set => myGameInstance = value; } // the game instance changes when a Game is loaded
      //--------------------------------------------------------------------
      public RuleDialogViewer(IGameInstance gi, IGameEngine ge)
      {
         if( null == gi )
         {
            Logger.Log(LogEnum.LE_ERROR, "RuleDialogViewer(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         if (null == ge)
         {
            Logger.Log(LogEnum.LE_ERROR, "RuleDialogViewer(): ge=null");
            CtorError = true;
            return;
         }
         myGameEngine = ge;
         if (false == CreateTables())
         {
            Logger.Log(LogEnum.LE_ERROR, "RuleDialogViewer(): CreateTables() returned false");
            CtorError = true;
            return;
         }
         if (null == myTables)
         {
            Logger.Log(LogEnum.LE_ERROR, "RuleDialogViewer(): myTables=null");
            CtorError = true;
            return;
         }
         if (false == CreateRules())
         {
            Logger.Log(LogEnum.LE_ERROR, "RuleDialogViewer(): CreateRules() returned false");
            CtorError = true;
            return;
         }
      }
      public bool ShowRule(string key)
      {
         try
         {
            BannerDialog dialog = myBannerDialogs[key];
            if (null != dialog)
            {
               dialog.Activate(); // bring to top
               dialog.Focus();
               return true;
            }
         }
         catch (System.Collections.Generic.KeyNotFoundException e1)
         {
            // do nothing. This is expected first time dialog is created
         }
         try
         {
            if (null == myRules)
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowRule(): myRules=null for key=" + key);
               return false;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(@"<TextBlock xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' Name='myTextBlockDisplay' xml:space='preserve' Width='555' Height='690' FontFamily='Old English Text MT' FontSize='20' TextWrapping='WrapWithOverflow' IsHyphenationEnabled='true' HorizontalAlignment='Left' VerticalAlignment='Top' Margin='15,0,0,0'>");
            sb.Append(myRules[key]);
            sb.Append(@"</TextBlock>");
            StringReader sr = new StringReader(sb.ToString());
            BannerDialog dialog = new BannerDialog(key, sr);
            if (true == dialog.CtorError)
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowRule(): BannerDialog() returned false");
               return false;
            }
            dialog.Closed += BannerDialog_Closed;
            foreach (Inline inline in dialog.TextBoxDiplay.Inlines)
            {
               if (inline is InlineUIContainer)
               {
                  InlineUIContainer ui = (InlineUIContainer)inline;
                  if (ui.Child is Button b)
                     b.Click += Button_Click;
               }
            }
            myBannerDialogs[key] = dialog;
            dialog.Show();
            return true;
         }
         catch (Exception e2)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowRule(): e=" + e2.ToString() + " for key=" + key);
            return false;
         }
      }
      public string GetTitle(string key)
      {
         try
         {
            if (null == myRules)
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowTitle(): myRules=null for key=" + key);
               return null;
            }
            string multilineString = myRules[key];
            int indexOfStart = multilineString.IndexOf(key);
            if( -1 == indexOfStart)
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowTitle(): IndexOf() returned -1 for key=" + key);
               return null;
            }
            indexOfStart += key.Length + 1; // add one to get past preceeding space
            string startOfTitle = multilineString.Substring(indexOfStart);
            int indexOfEnd = startOfTitle.IndexOf('<');
            string title = startOfTitle.Substring(0,indexOfEnd);
            return title;
         }
         catch (Exception e2)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowTitle(): e=" + e2.ToString() + " for key=" + key);
            return null;
         }
      }
      public bool ShowTable(string key)
      {
         try
         {
            TableDialog dialog = myTableDialogs[key];
            if (null != dialog)
            {
               dialog.Activate(); // bring to top
               dialog.Focus();
               return true;
            }
         }
         catch (System.Collections.Generic.KeyNotFoundException e1)
         {
            // do nothing. This is expected first time dialog is created
            Logger.Log(LogEnum.LE_GAME_INIT, "ShowTable(): Unable to find key=" + key + " e=" + e1.ToString());
         }
         try
         {
            if (null == myTables)
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowTable(): myRules=null for key=" + key);
               return false;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(@"<FlowDocument xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' Name='myFlowDocument'>");
            sb.Append(myTables[key]);
            sb.Append(@"</FlowDocument>");
            StringReader sr = new StringReader(sb.ToString());
            TableDialog dialog = new TableDialog(key, sr);
            if (true == dialog.CtorError)
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowTable(): TableDialog() ctor error");
               return false;
            }
            switch (key)
            {
               case "t207":
                  dialog.Title = "r207 Travel Table";
                  dialog.myFlowDocumentScrollViewer.Width = 900;
                  dialog.myFlowDocumentScrollViewer.Height = 300;
                  break;
               case "t220":
                  dialog.Title = "r220 Combat Table";
                  dialog.myFlowDocumentScrollViewer.Width = 410;
                  dialog.myFlowDocumentScrollViewer.Height = 410;
                  break;
               case "t226":
                  dialog.Title = "r226 Treasure Table";
                  dialog.myFlowDocumentScrollViewer.Width = 400;
                  dialog.myFlowDocumentScrollViewer.Height = 380;
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "ShowTable(): reached default key=" + key);
                  return false;
            }
            dialog.Closed += TableDialog_Closed;
            IEnumerable<Button> buttons = FindButtons(dialog.myFlowDocumentScrollViewer.Document);
            foreach (Button button in buttons)
               button.Click += Button_Click;
            myTableDialogs[key] = dialog;
            dialog.Show();
            return true;
         }
         catch (Exception e2)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowTable(): e=" + e2.ToString() + " for key=" + key);
            return false;
         }
      }
      public bool ShowEvent(string key)
      {
         if (true == myGameInstance.IsGridActive)
         {
            MessageBox.Show("Must wait for event to complete before showing.");
         }
         else
         {
            myGameInstance.EventDisplayed = key;
            GameAction action = GameAction.UpdateEventViewerDisplay;
            myGameEngine.PerformAction(ref myGameInstance, ref action);
         }
         return true;
      }
      //--------------------------------------------------------------------
      private bool CreateRules()
      {
         try
         {
            ConfigFileReader cfr = new ConfigFileReader("../Config/Rules.txt");
            if (true == cfr.CtorError)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateRules(): cfr.CtorError=true");
               return false;
            }
            myRules = cfr.Output;
            if (0 == myRules.Count)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateRules(): myRules.Count=0");
               return false;
            }
            return true;
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateRules(): e=" + e.ToString());
            return false;
         }
      }
      private bool CreateTables()
      {
         try
         {
            ConfigFileReader cfr = new ConfigFileReader("../Config/Tables.txt");
            if (true == cfr.CtorError)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateTables(): cfr.CtorError=true");
               return false;
            }
            myTables = cfr.Output;
            if (0 == myTables.Count)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateTables(): myTables.Count=0");
               return false;
            }
            return true;
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateTables(): e=" + e.ToString());
            return false;
         }
      }
      private IEnumerable<Button> FindButtons(FlowDocument document)
      {
         return document.Blocks.SelectMany(block => FindButtons(block));
      }
      private IEnumerable<Button> FindButtons(Block block)
      {
         if (block is Paragraph)
         {
            List<Button> buttons = new List<Button>();
            var para = ((Paragraph)block).Inlines;
            foreach (var i in para)
            {
               if (i is InlineUIContainer)
               {
                  var inlineUiContainer = i as InlineUIContainer;
                  Button button = ((InlineUIContainer)inlineUiContainer).Child as Button;
                  if (null != button)
                  {
                     buttons.Add(button);
                  }
               }
               else if (i is Figure)
               {
                  var figure = i as Figure;
                  var buttons1 = figure.Blocks.SelectMany(blocks => FindButtons(blocks));
                  buttons.AddRange(buttons1);
               }
               else if (i is Floater)
               {
                  var floater = i as Floater;
                  var buttons2 = floater.Blocks.SelectMany(blocks => FindButtons(blocks));
                  buttons.AddRange(buttons2);
               }
            }
            return buttons;
         }
         if (block is Table)
         {
            return ((Table)block).RowGroups.SelectMany(x => x.Rows).SelectMany(x => x.Cells).SelectMany(x => x.Blocks).SelectMany(innerBlock => FindButtons(innerBlock));
         }
         if (block is BlockUIContainer)
         {
            Button b = ((BlockUIContainer)block).Child as Button;
            return b == null ? new List<Button>() : new List<Button>(new[] { b });
         }
         if (block is List)
         {
            return ((List)block).ListItems.SelectMany(listItem => listItem.Blocks.SelectMany(innerBlock => FindButtons(innerBlock)));
         }
         throw new InvalidOperationException("Unknown block type: " + block.GetType());
      }
      //--------------------------------------------------------------------
      private void BannerDialog_Closed(object sender, EventArgs e)
      {
         BannerDialog dialog = (BannerDialog)sender;
         foreach (Inline inline in dialog.TextBoxDiplay.Inlines)
         {
            if (inline is InlineUIContainer)
            {
               InlineUIContainer ui = (InlineUIContainer)inline;
               if (ui.Child is Button b)
                  b.Click -= Button_Click;
            }
         }
         myBannerDialogs[dialog.Key] = null;
      }
      private void TableDialog_Closed(object sender, EventArgs e)
      {
         TableDialog dialog = (TableDialog)sender;
         myTableDialogs[dialog.Key] = null;
      }
      private void Button_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         string key = (string)b.Content;
         if (true == key.StartsWith("r")) // rules based click
         {
            if (false == ShowRule(key))
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click(): ShowRule() returned false");
               return;
            }
         }
         else if (true == key.StartsWith("t")) // rules based click
         {
            if (false == ShowTable(key))
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click():  ShowTable() returned false");
               return;
            }
         }
         else if (true == key.StartsWith("e")) // rules based click
         {
            if (false == ShowEvent(key))
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click():  ShowEvent() returned false");
               return;
            }
         }
      }
   }
}
