using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BarbarianPrince
{
   public partial class EllipseDisplayDialog : Window
   {
      public RuleDialogViewer myRulesMgr = null;
      public EllipseDisplayDialog(EnteredHex hex, RuleDialogViewer rm)
      {
         InitializeComponent();
         //-------------------------------------------------------------
         if( null == rm )
         {
            Logger.Log(LogEnum.LE_ERROR, "EllipseDisplayDialog(): rm=null");
            return;
         }
         myRulesMgr = rm;
         //-------------------------------------------------------------
         myTextBlock.Inlines.Add(new Run(" Hex #" + hex.HexName));
         myTextBlock.Inlines.Add(new LineBreak());
         myTextBlock.Inlines.Add(new LineBreak());
         //-------------------------------------------------------------
         switch (hex.ColorAction)
         {
            case ColorActionEnum.CAE_START:
               myTextBlock.Inlines.Add(new Run("Start of the Game"));
               break;
            case ColorActionEnum.CAE_LOST:
               if (true == hex.IsEncounter)
                  myTextBlock.Inlines.Add(new Run("Lost Encounter"));
               else
                  myTextBlock.Inlines.Add(new Run("Lost"));
               break;
            case ColorActionEnum.CAE_REST:
               if (true == hex.IsEncounter)
                  myTextBlock.Inlines.Add(new Run("Rest Encounter"));
               else
                  myTextBlock.Inlines.Add(new Run("Rest"));
               break;
            case ColorActionEnum.CAE_JAIL:
               myTextBlock.Inlines.Add(new Run("Jailed"));
               break;
            case ColorActionEnum.CAE_TRAVEL:
               if (true == hex.IsEncounter)
                  myTextBlock.Inlines.Add(new Run("Travel Encounter"));
               else
                  myTextBlock.Inlines.Add(new Run("Travel"));
               break;
            case ColorActionEnum.CAE_TRAVEL_AIR:
               if (true == hex.IsEncounter)
                  myTextBlock.Inlines.Add(new Run("Air Encounter"));
               else
                  myTextBlock.Inlines.Add(new Run("Air Travel"));
               break;
            case ColorActionEnum.CAE_TRAVEL_RAFT:
               if (true == hex.IsEncounter)
                  myTextBlock.Inlines.Add(new Run("Raft Encounter"));
               else
                  myTextBlock.Inlines.Add(new Run("Raft Travel"));
               break;
            case ColorActionEnum.CAE_TRAVEL_DOWNRIVER:
               myTextBlock.Inlines.Add(new Run("Raft Drifted Downriver"));
               break;
            case ColorActionEnum.CAE_ESCAPE:
               myTextBlock.Inlines.Add(new Run("Escape"));
               break;
            case ColorActionEnum.CAE_FOLLOW:
               myTextBlock.Inlines.Add(new Run("Follow"));
               break;
            case ColorActionEnum.CAE_SEARCH_RUINS:
               myTextBlock.Inlines.Add(new Run("Search Ruins"));
               break;
            case ColorActionEnum.CAE_SEARCH:
               if (true == hex.IsEncounter)
                  myTextBlock.Inlines.Add(new Run("Search Encounter"));
               else
                  myTextBlock.Inlines.Add(new Run("Search"));
               break;
            case ColorActionEnum.CAE_SEEK_NEWS:
               myTextBlock.Inlines.Add(new Run("Seek News"));
               break;
            case ColorActionEnum.CAE_HIRE:
               myTextBlock.Inlines.Add(new Run("Hire"));
               break;
            case ColorActionEnum.CAE_AUDIENCE:
               myTextBlock.Inlines.Add(new Run("Seek Audience"));
               break;
            case ColorActionEnum.CAE_OFFERING:
               myTextBlock.Inlines.Add(new Run("Make Offering"));
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "EllipseDisplayDialog(): Reached default with ColorAction=" + hex.ColorAction.ToString());
               return;
         }
         //-------------------------------------------------------------
         if(ColorActionEnum.CAE_START != hex.ColorAction)
            myTextBlock.Inlines.Add(new Run(" on Day #" + hex.Day.ToString()));
         myTextBlock.Inlines.Add(new LineBreak());
         myTextBlock.Inlines.Add(new LineBreak());
         //-------------------------------------------------------------
         StringBuilder sb22 = new StringBuilder();
         if (0 < hex.Party.Count)
         {
            int i = 0;
            foreach (String partyMember in hex.Party)
            {
               sb22.Append(partyMember);
               if (++i != hex.Party.Count)
                  sb22.Append(", ");
            }
            myTextBlock.Inlines.Add(new Run("Party Members: " + sb22.ToString()));
            myTextBlock.Inlines.Add(new LineBreak());
            myTextBlock.Inlines.Add(new LineBreak());
         }
         //-------------------------------------------------------------
         string title = null;
         if (ColorActionEnum.CAE_JAIL == hex.ColorAction)
         {
            switch( hex.EventName )
            {
               case "e203a":
                  title = "Prison Escape Attempt";
                  break;
               case "e203c":
                  title = "Night in Dungeon";
                  break;
               case "e203e":
                  title = "Wizard's Slave";
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "EllipseDisplayDialog(): Reached default with hex.EventName =" + hex.EventName);
                  return;
            }
         }
         else if("e000" == hex.EventName)
         {
            return;
         }
         else
         {
            title = myRulesMgr.GetEventTitle(hex.EventName);
         }
         if ( true == hex.IsEncounter )
         {
            myTextBlock.Inlines.Add(new Run(hex.EventName + ": " + title));
            myTextBlock.Inlines.Add(new LineBreak());
         }
         //-------------------------------------------------------------
      }
   }
}
