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
      public EllipseDisplayDialog(EnteredHex hex)
      {
         InitializeComponent();
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
               myTextBlock.Inlines.Add(new Run("Event Name: " + hex.EventName));
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
            case ColorActionEnum.CAE_ESCAPE:
               myTextBlock.Inlines.Add(new Run("Escape"));
               break;
            case ColorActionEnum.CAE_FOLLOW:
               myTextBlock.Inlines.Add(new Run("Follow"));
               break;
            case ColorActionEnum.CAE_SEARCH:
               if (true == hex.IsEncounter)
                  myTextBlock.Inlines.Add(new Run("Search Encounter"));
               else
                  myTextBlock.Inlines.Add(new Run("Search"));
               break;
            case ColorActionEnum.CAE_STRUCTURE:
               myTextBlock.Inlines.Add(new Run(" Town, Temple, Castle Action" + hex.EventName));
               break;
         }
         //-------------------------------------------------------------
         myTextBlock.Inlines.Add(new Run(" on Day #" + hex.Day.ToString()));
         myTextBlock.Inlines.Add(new LineBreak());
         myTextBlock.Inlines.Add(new LineBreak());
         //-------------------------------------------------------------
         if ( true == hex.IsEncounter )
         {
            myTextBlock.Inlines.Add(new Run("Event Name: " + hex.EventName));
            myTextBlock.Inlines.Add(new LineBreak());
         }
         //-------------------------------------------------------------
         StringBuilder sb22 = new StringBuilder();
         if ( 0 < hex.Party.Count )
         {
            int i = 0;
            int lastEntry = hex.Party.Count;
            foreach (String partyMember in hex.Party)
            {
               sb22.Append(partyMember);
               if (++i != lastEntry)
                  sb22.Append(", ");
            }
            myTextBlock.Inlines.Add(new Run("Party Members: " + sb22.ToString()));
         }
         //-------------------------------------------------------------
      }
   }
}
