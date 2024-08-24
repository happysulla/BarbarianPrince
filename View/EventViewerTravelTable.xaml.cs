using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Documents.DocumentStructures;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using WpfAnimatedGif;
using static System.Collections.Specialized.BitVector32;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using Button = System.Windows.Controls.Button;
using Label = System.Windows.Controls.Label;

namespace BarbarianPrince
{
   public partial class EventViewerTravelTable : UserControl
   {
      public delegate bool EndTravelEventCallback(bool isLost, bool isEncounter, bool isRiverEncounter);
      private const int STARTING_ASSIGNED_ROW = 8;
      //---------------------------------------------
      public enum EnumR204
      {
         TC_LOST_ROLL,                  // Start with TC_LOST_ROLL 
         TC_LOST_SHOW_RESULTS,          // Set to this state after TC_LOST_ROLL_EVENT state
         TC_LOST_SHOW_CROSS_RESULT,     // Lost attempting to cross river means stop travel for today... but no travel event
         TC_DESERTION_ROLL,             // guides may desert if get party lost
         TC_DESERTION_SHOW,             // show results of guide deserting
         TC_EVENT_ROLL,                 // not lost, so move to the event encounter roll
         TC_EVENT_ROLL_ROAD,            // if no travel event happens on a road, see if encounter happens in hex - transition to this state after TC_EVENT_ROLL happens
         TC_EVENT_ROLL_REFERENCE,       // roll to determine which roll rule table to use
         TC_EVENT_ROLL_EVENT,           // within the rule table, determine the encountered event
         TC_EVENT_ROLL_EVENT_R230,      // If rafting event happens, need to look up on rafting table R230
         TC_EVENT_ROLL_EVENT_R232,      // If rafting event happens, followed by roll of 8, need to look up on R232 table
         TC_EVENT_ROLL_EVENT_R232_SHOW, // Show Results
         TC_EVENT_ROLL_REFERENCE_R281,  // If airborne event happens, followed by roll of 6, need to look up on R281 table - which is reroll reference using terrain type
         TC_EVENT_ROLL_EVENT_R281,      // If airborne event happens, followed by roll of 6, need to look up on R281 table - which is reroll event using terrain type
         TC_EVENT_ROLL_EVENT_R281_SHOW, // Show Results
         TC_EVENT_SHOW_RESULTS,
         TC_END,
         TC_ERROR
      };
      public struct GridRow // Grid Rows only used to show Local Guides that might desert
      {
         public IMapItem myAssignable;
         public bool myIsSelected; // This is the local guide selected to possible desert
         public int myDieRoll;
         public int myResult;
      };
      //---------------------------------------------
      public bool CtorError { get; } = false;
      //---------------------------------------------
      private EnumR204 myState = EnumR204.TC_EVENT_ROLL;
      private bool myIsLost = false;
      private bool myIsEncounter = false;
      private IMapItemMove myMapItemMove = null;
      private EndTravelEventCallback myCallback = null;
      //---------------------------------------------
      private IMapItems myPartyMembers = null;
      private IMapItems myLocalGuides = null;
      private GridRow[] myGridRows = new GridRow[Utilities.MAX_GRID_ROW];
      //---------------------------------------------
      private IGameInstance myGameInstance = null;
      private readonly Canvas myCanvas = null;
      private readonly ScrollViewer myScrollViewer = null;
      private RuleDialogViewer myRulesMgr = null;
      //---------------------------------------------
      private bool myIsTravelingRoad = false;
      private bool myIsTravelingAir = false;
      private bool myIsRiverCrossing = false;
      private bool myIsRaftEncounter = false;
      private static int theConsecutiveLostCount = 0;
      //---------------------------------------------
      private bool myOptionAutoLostDecrease = false; // reduce chances of lost on consecutive rolls
      private bool myOptionNoLostRoll = false;
      private bool myOptionNoLostEvent = false;
      private bool myOptionForceLostEvent = false;
      private bool myOptionForceNoCrossEvent = false;
      private bool myOptionForceCrossEvent = false;
      private bool myOptionForceLostAfterCrossEvent = false;
      private bool myOptionForceNoEvent = false;
      private bool myOptionForceEvent = false;
      private bool myOptionForceNoRoadEvent = false;
      private bool myOptionForceNoAirEvent = false;
      private bool myOptionForceAirEvent = false;
      private bool myOptionForceNoRaftEvent = false;
      private bool myOptionForceRaftEvent = false;
      //---------------------------------------------
      private IDieRoller myDieRoller = null;
      private int myRollResultRowNum = 0;
      private bool myIsRollInProgress = false;
      private int myRollReference = 0;
      private int myRollEvent = 0;
      private string myKeyReference = "";
      private Dictionary<string, string[]> myReferences = null;
      private Dictionary<string, string[]> myEvents = null;
      //---------------------------------------------
      private readonly DoubleCollection myDashArray = new DoubleCollection();
      private readonly FontFamily myFontFam1 = new FontFamily("Courier New");
      private readonly FontFamily myFontFam2 = new FontFamily("Tahoma");
      //-----------------------------------------------------------------------------------------
      public EventViewerTravelTable(IGameInstance gi, Canvas c, ScrollViewer sv, RuleDialogViewer rdv, IDieRoller dr)
      {
         InitializeComponent();
         //--------------------------------------------------
         if (null == gi) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "MapItemHuntViewer(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //--------------------------------------------------
         if (null == c) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "MapItemHuntViewer(): c=null");
            CtorError = true;
            return;
         }
         myCanvas = c;
         //--------------------------------------------------
         if (null == sv) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "MapItemHuntViewer(): sv=null");
            CtorError = true;
            return;
         }
         myScrollViewer = sv;
         //--------------------------------------------------
         if (null == rdv)
         {
            Logger.Log(LogEnum.LE_ERROR, "MapItemHuntViewer(): rdv=null");
            CtorError = true;
            return;
         }
         myRulesMgr = rdv;
         //--------------------------------------------------
         if (null == dr)
         {
            Logger.Log(LogEnum.LE_ERROR, "MapItemHuntViewer(): cfm=true");
            CtorError = true;
            return;
         }
         myDieRoller = dr;
         //--------------------------------------------------
         myDashArray.Add(4);  // used for dotted lines
         myDashArray.Add(2);  // used for dotted lines
         myGridTravelTable.MouseDown += Grid_MouseDown;
         //--------------------------------------------------
         myReferences = new Dictionary<string, string[]>();
         myReferences["Farmland"] = new String[6] { "e009", "r231", "r232", "r233", "r234", "r235" };
         myReferences["Countryside"] = new String[6] { "r232", "r236", "r237", "r238", "r239", "r240" };
         myReferences["Forest"] = new String[6] { "r232", "r241", "r242", "r243", "r244", "r240" };
         myReferences["Hills"] = new String[6] { "r232", "r245", "r246", "r247", "r248", "r249" };
         myReferences["Mountains"] = new String[6] { "r232", "r250", "r251", "r252", "r253", "r248" };
         myReferences["Swamp"] = new String[6] { "r232", "r254", "r255", "r256", "r257", "r258" };
         myReferences["Desert"] = new String[6] { "r259", "r260", "r261", "r262", "r263", "r264" };
         myReferences["Cross River"] = new String[6] { "r232", "r265", "r266", "r267", "r268", "r269" };
         myReferences["On Road"] = new String[6] { "r270", "r271", "r272", "r273", "r274", "r275" };
         myReferences["Airborne"] = new String[6] { "r276", "r277", "r278", "r279", "r280", "r281" };
         myReferences["Rafting"] = new String[12] { "r230", "r230", "r230", "r230", "r230", "r230", "r230a", "r230a", "r230a", "r230a", "r230a", "r230a" };
         myReferences["Rafting1"] = new String[6] { "r232", "r232", "r232", "r232", "r232", "r232" };
         myEvents = new Dictionary<string, string[]>();
         myEvents["r230"]  = new String[6] { "    ", "e125", "e126", "e018", "e129", "e127" };
         myEvents["r230a"] = new String[6] { "e128", "r232", "e051", "e094", "e091", "e129" };
         myEvents["r231"]  = new String[6] { "e018", "e018", "e022", "e022", "e023", "e130" };
         myEvents["r232"]  = new String[6] { "e003", "e004", "e005", "e006", "e007", "e008" };
         myEvents["r233"]  = new String[6] { "e128", "e128", "e128", "e128", "e129", "e017" };
         myEvents["r234"]  = new String[6] { "e049", "e048", "e032", "e081", "e050", "e050" };
         myEvents["r235"]  = new String[6] { "e078", "e078", "e079", "e079", "e009", "e009" };
         myEvents["r236"]  = new String[6] { "e009", "e009", "e050", "e018", "e022", "e023" };
         myEvents["r237"]  = new String[6] { "e052", "e055", "e057", "e051", "e034", "e072" };
         myEvents["r238"]  = new String[6] { "e077", "e075", "e075", "e075", "e076", "e081" };
         myEvents["r239"]  = new String[6] { "e044", "e046", "e067", "e064", "e068", "e069" };
         myEvents["r240"]  = new String[6] { "e078", "e078", "e078", "e078", "e079", "e079" };
         myEvents["r241"]  = new String[6] { "e074", "e074", "e073", "e009", "e051", "e128" };
         myEvents["r242"]  = new String[6] { "e072", "e072", "e052", "e082", "e080", "e080" };
         myEvents["r243"]  = new String[6] { "e083", "e083", "e084", "e084", "e076", "e075" };
         myEvents["r244"]  = new String[6] { "e165", "e166", "e065", "e064", "e087", "e087" };
         myEvents["r245"]  = new String[6] { "e098", "e112", "e023", "e051", "e068", "e022" };
         myEvents["r246"]  = new String[6] { "e028", "e028", "e058", "e070", "e055", "e056" };
         myEvents["r247"]  = new String[6] { "e076", "e076", "e076", "e075", "e128", "e128" };
         myEvents["r248"]  = new String[6] { "e118", "e052", "e059", "e067", "e066", "e064" };
         myEvents["r249"]  = new String[6] { "e078", "e078", "e078", "e085", "e079", "e079" };
         myEvents["r250"]  = new String[6] { "e099", "e100", "e023", "e068", "e101", "e112" };
         myEvents["r251"]  = new String[6] { "e028", "e028", "e058", "e055", "e052", "e054" };
         myEvents["r252"]  = new String[6] { "e078", "e078", "e079", "e079", "e088", "e065" };
         myEvents["r253"]  = new String[6] { "e085", "e085", "e086", "e086", "e086", "e095" };
         myEvents["r254"]  = new String[6] { "e022", "e009", "e073", "e051", "e051", "e074" };
         myEvents["r255"]  = new String[6] { "e034", "e082", "e164", "e052", "e057", "e098" };
         myEvents["r256"]  = new String[6] { "e091", "e091", "e094", "e094", "e092", "e092" };
         myEvents["r257"]  = new String[6] { "e089", "e089", "e089", "e090", "e064", "e093" };
         myEvents["r258"]  = new String[6] { "e078", "e078", "e078", "e095", "e095", "e097" };
         myEvents["r259"]  = new String[6] { "e022", "e129", "e128", "e051", "e023", "e068" };
         myEvents["r260"]  = new String[6] { "e028", "e082", "e055", "e003", "e004", "e028" };
         myEvents["r261"]  = new String[6] { "e005", "e120", "e120", "e120", "e067", "e066" };
         myEvents["r262"]  = new String[6] { "e034", "e164", "e164", "e091", "e091", "e120" };
         myEvents["r263"]  = new String[6] { "e064", "e064", "e121", "e121", "e121", "e093" };
         myEvents["r264"]  = new String[6] { "e078", "e078", "e078", "e078", "e096", "e096" };
         myEvents["r265"]  = new String[6] { "e122", "e122", "e122", "e009", "e051", "e074" };
         myEvents["r266"]  = new String[6] { "e123", "e123", "e057", "e057", "e052", "e055" };
         myEvents["r267"]  = new String[6] { "e094", "e094", "e091", "e091", "e075", "e084" };
         myEvents["r268"]  = new String[6] { "e083", "e076", "e077", "e124", "e124", "e124" };
         myEvents["r269"]  = new String[6] { "e122", "e122", "e122", "e125", "e126", "e127" };
         myEvents["r270"]  = new String[6] { "e018", "e022", "e023", "e073", "e009", "e009" };
         myEvents["r271"]  = new String[6] { "e050", "e051", "e051", "e051", "e003", "e003" };
         myEvents["r272"]  = new String[6] { "e004", "e004", "e005", "e006", "e006", "e008" };
         myEvents["r273"]  = new String[6] { "e007", "e007", "e057", "e130", "e128", "e128" };
         myEvents["r274"]  = new String[6] { "e049", "e048", "e081", "e128", "e129", "e129" };
         myEvents["r275"]  = new String[6] { "e078", "e078", "e079", "e079", "e128", "e129" };
         myEvents["r276"]  = new String[6] { "e102", "e102", "e103", "e103", "e104", "e104" };
         myEvents["r277"]  = new String[6] { "e112", "e112", "e112", "e112", "e108", "e108" };
         myEvents["r278"]  = new String[6] { "e106", "e106", "e105", "e105", "e079", "e079" };
         myEvents["r279"]  = new String[6] { "e107", "e109", "e077", "e101", "e110", "e111" };
         myEvents["r280"]  = new String[6] { "e099", "e098", "e100", "e101", "e064", "e065" };
         myEvents["r281"]  = new String[6] { "r281", "r281", "r281", "r281", "r281", "r281" };
      }
      public bool PerformTravel(EndTravelEventCallback callback)
      {
         //--------------------------------------------------
         if (null == callback)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformTravel(): callback=null");
            return false;
         }
         myCallback = callback;
         //--------------------------------------------------
         myPartyMembers = myGameInstance.PartyMembers;
         if (null == myGameInstance.PartyMembers)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformTravel(): partyMembers=null");
            return false;
         }
         if (myPartyMembers.Count < 1) // at a minimum, the prince needs to be in party
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformTravel(): invalid state party.count=" + myPartyMembers.Count.ToString());
            return false;
         }
         //--------------------------------------------------
         if (0 == myGameInstance.MapItemMoves.Count) // Should have one MapItemMove queued up ready to go
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformTravel(): invalid state mim.count=" + myGameInstance.MapItemMoves.Count.ToString());
            return false;
         }
         myMapItemMove = myGameInstance.MapItemMoves[0];
         if (null == myMapItemMove.OldTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformTravel(): invalid state mim.ot=null");
            return false;
         }
         if (null == myMapItemMove.NewTerritory)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformTravel(): invalid state mim.nt=null");
            return false;
         }
         //--------------------------------------------------
         if (false == SetTravelOptions()) 
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformTravel(): SetTravelOptions() returned false");
            return false;
         }
         //--------------------------------------------------
         myRollResultRowNum = 0;
         myIsRollInProgress = false;
         myRollReference = 0;
         myRollEvent = 0;
         myKeyReference = "";
         myIsLost = false;
         myIsEncounter = false;
         myIsTravelingRoad = false;
         myIsTravelingAir = false;
         myIsRiverCrossing = false;
         myIsRaftEncounter = false;
         myLocalGuides = new MapItems();  // Some party members can be guides to help prevent getting lost
         //--------------------------------------------------
         int i = 0;
         foreach (IMapItem mi in myGameInstance.PartyMembers)
         {
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "PerformTravel(): mi=null");
               return false;
            }
            foreach (ITerritory t in mi.GuideTerritories) // If this territory has a local guide, add to the myLocalGuides container
            {
               if (t.Name == myMapItemMove.NewTerritory.Name)
               {
                  myLocalGuides.Add(mi);
                  myGridRows[i] = new GridRow();
                  myGridRows[i].myAssignable = mi;
                  myGridRows[i].myResult = Utilities.NO_RESULT;
                  if (0 < i)                               // If there is only one local guide, it is the selected one
                     myGridRows[i].myIsSelected = false;
                  else
                     myGridRows[i].myIsSelected = true;
                  ++i;
                  break;
               }
            }
         }
         myState = EnumR204.TC_LOST_ROLL;
         bool isRiverCrossing = false;
         bool isStructure = myGameInstance.IsInStructure(myMapItemMove.OldTerritory);
         //--------------------------------------------------
         if (true == myGameInstance.IsFalconFed) // do not get lost if falcon with party
            myState = EnumR204.TC_EVENT_ROLL;
         //--------------------------------------------------
         myIsTravelingRoad = CheckRoadTravel(myMapItemMove.OldTerritory, myMapItemMove.NewTerritory);
         if (true == myIsTravelingRoad) // PerformTravel() - cannot get lost if on road
            myState = EnumR204.TC_EVENT_ROLL;
         //--------------------------------------------------
         if (true == myOptionNoLostRoll) // do not get lost if property says no
            myState = EnumR204.TC_EVENT_ROLL;
         //--------------------------------------------------
         if (myMapItemMove.OldTerritory == myMapItemMove.NewTerritory) // cannot get lost if in same territory
            myState = EnumR204.TC_EVENT_ROLL;
         //--------------------------------------------------
         if (true == myGameInstance.IsAirborne)
         {
            myIsTravelingAir = true;
            myMapItemMove.RiverCross = RiverCrossEnum.TC_CROSS_YES_SHOWN; // assume accross the river
         }
         else
         {
            if (true == myGameInstance.IsAirborneEnd) // cannot get lost when landing from air
               myState = EnumR204.TC_EVENT_ROLL;
            //--------------------------------------------------
            if (true == isStructure)
               myState = EnumR204.TC_EVENT_ROLL;
            //--------------------------------------------------

            foreach (String river in myMapItemMove.OldTerritory.Rivers) // Check for crossing rivers
            {
               if (river == myMapItemMove.NewTerritory.Name)
                  isRiverCrossing = true;
            }
            if (false == isRiverCrossing)   // if no river or it is already crossed, no need to check for river crossing event
               myMapItemMove.RiverCross = RiverCrossEnum.TC_NO_RIVER;
            else if (RiverCrossEnum.TC_CROSS_YES_SHOWN == myMapItemMove.RiverCross) // If already crossed, assume accross river
               myMapItemMove.RiverCross = RiverCrossEnum.TC_CROSS_YES_SHOWN;
            else if (true == myGameInstance.IsPartyFlying())  // If able to fly, assume accross river
               myMapItemMove.RiverCross = RiverCrossEnum.TC_CROSS_YES;
            else if (RiverCrossEnum.TC_CROSS_YES == myMapItemMove.RiverCross) // If already crossed, assume accross river
               myMapItemMove.RiverCross = RiverCrossEnum.TC_CROSS_YES;
            else
               myMapItemMove.RiverCross = RiverCrossEnum.TC_ATTEMPTING_TO_CROSS;
            if ((RiverCrossEnum.TC_CROSS_YES == myMapItemMove.RiverCross) && (true == myOptionForceLostAfterCrossEvent)) // cause event after crossing river
            {
               myOptionNoLostEvent = false;
               myOptionForceLostEvent = true;
            }
            //--------------------------------------------------
            if (RaftEnum.RE_RAFT_CHOSEN == myGameInstance.RaftState) // PerformTravel() - cannot get lost if on rafting
            {
               myMapItemMove.RiverCross = RiverCrossEnum.TC_NO_RIVER;
               myState = EnumR204.TC_EVENT_ROLL;
               myIsRaftEncounter = true;
            }
            if (RaftEnum.RE_RAFT_ENDS_TODAY == myGameInstance.RaftState) // PerformTravel() - If rafting ended, only perform travel check in ending hex
            {
               myMapItemMove.RiverCross = RiverCrossEnum.TC_NO_RIVER;
               myState = EnumR204.TC_EVENT_ROLL;
            }
         }
         Logger.Log(LogEnum.LE_VIEW_TRAVEL_CHECK, "PerformTravel(): s=" + myState.ToString() + " mim=" + myMapItemMove.ToString() + " river?=" + isRiverCrossing.ToString()  + " road?=" + myIsTravelingRoad.ToString() + " s?=" + isStructure.ToString() + " raft?=" + myGameInstance.RaftState.ToString() + " a?=" + myIsTravelingAir.ToString() + " f?=" + myGameInstance.IsFalconFed.ToString());
         //--------------------------------------------------
         // Update the grid and continue
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformTravel(): UpdateGrid() return false");
            return false;
         }
         myScrollViewer.Content = myGridTravelTable;
         return true;
      }
      private bool SetTravelOptions()
      {
         if (null == myGameInstance)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetTravelOptions():  myGameInstance=null");
            return false;
         }
         //--------------------------------------------------
         Option optionAutoLostDescrease= myGameInstance.Options.Find("AutoLostDecrease");
         if (null == optionAutoLostDescrease)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetTravelOptions(): myGameInstance.Options.Find(AutoLostDecrease) returned null");
            return false;
         }
         myOptionAutoLostDecrease = optionAutoLostDescrease.IsEnabled;
         //--------------------------------------------------
         Option optionNoLostRoll = myGameInstance.Options.Find("NoLostRoll");
         if (null == optionNoLostRoll)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetTravelOptions(): myGameInstance.Options.Find(ForceNoLostEvent) returned null");
            return false;
         }
         myOptionNoLostRoll = optionNoLostRoll.IsEnabled;
         //--------------------------------------------------
         Option optionNoLostEvent = myGameInstance.Options.Find("ForceNoLostEvent");
         if (null == optionNoLostEvent)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetTravelOptions(): myGameInstance.Options.Find(ForceNoLostEvent) returned null");
            return false;
         }
         myOptionNoLostEvent = optionNoLostEvent.IsEnabled;
         //--------------------------------------------------
         Option optionForceLostEvent = myGameInstance.Options.Find("ForceLostEvent");
         if (null == optionForceLostEvent)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetTravelOptions(): myGameInstance.Options.Find(ForceLostResult) returned null");
            return false;
         }
         myOptionForceLostEvent = optionForceLostEvent.IsEnabled;
         //--------------------------------------------------
         Option optionForceNoCrossEvent = myGameInstance.Options.Find("ForceNoCrossEvent");
         if (null == optionForceNoCrossEvent)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetTravelOptions(): myGameInstance.Options.Find(ForceNoCrossEvent) returned null");
            return false;
         }
         myOptionForceNoCrossEvent = optionForceNoCrossEvent.IsEnabled;
         //--------------------------------------------------
         Option optionForceLostAfterCrossEvent = myGameInstance.Options.Find("ForceLostAfterCrossEvent");
         if (null == optionForceLostAfterCrossEvent)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetTravelOptions(): myGameInstance.Options.Find(ForceLostAfterCrossEvent) returned null");
            return false;
         }
         myOptionForceLostAfterCrossEvent = optionForceLostAfterCrossEvent.IsEnabled;
         //--------------------------------------------------
         Option optionForceCrossEvent = myGameInstance.Options.Find("ForceCrossEvent");
         if (null == optionForceCrossEvent)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetTravelOptions(): myGameInstance.Options.Find(ForceCrossEvent) returned null");
            return false;
         }
         myOptionForceCrossEvent = optionForceCrossEvent.IsEnabled;
         //--------------------------------------------------
         Option optionForceNoEvent = myGameInstance.Options.Find("ForceNoEvent");
         if (null == optionForceNoEvent)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetTravelOptions(): myGameInstance.Options.Find(ForceNoEvent) returned null");
            return false;
         }
         myOptionForceNoEvent = optionForceNoEvent.IsEnabled;
         //--------------------------------------------------
         Option optionForceEvent = myGameInstance.Options.Find("ForceEvent");
         if (null == optionForceEvent)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetTravelOptions(): myGameInstance.Options.Find(ForceEvent) returned null");
            return false;
         }
         myOptionForceEvent = optionForceEvent.IsEnabled;
         //--------------------------------------------------
         Option optionForceNoRoadEvent = myGameInstance.Options.Find("ForceNoRoadEvent");
         if (null == optionForceNoRoadEvent)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetTravelOptions(): myGameInstance.Options.Find(ForceNoRoadEvent) returned null");
            return false;
         }
         myOptionForceNoRoadEvent = optionForceNoRoadEvent.IsEnabled;
         //--------------------------------------------------
         Option optionForceNoAirEvent = myGameInstance.Options.Find("ForceNoAirEvent");
         if (null == optionForceNoAirEvent)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetTravelOptions(): myGameInstance.Options.Find(ForceNoAirEvent) returned null");
            return false;
         }
         myOptionForceNoAirEvent = optionForceNoAirEvent.IsEnabled;
         //--------------------------------------------------
         Option optionForceAirEvent = myGameInstance.Options.Find("ForceAirEvent");
         if (null == optionForceAirEvent)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetTravelOptions(): myGameInstance.Options.Find(ForceAirEvent) returned null");
            return false;
         }
         myOptionForceAirEvent = optionForceAirEvent.IsEnabled;
         //--------------------------------------------------
         Option optionForceNoRaftEvent = myGameInstance.Options.Find("ForceNoRaftEvent");
         if (null == optionForceNoRaftEvent)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetTravelOptions(): myGameInstance.Options.Find(ForceNoRaftEvent) returned null");
            return false;
         }
         myOptionForceNoRaftEvent = optionForceNoRaftEvent.IsEnabled;
         //--------------------------------------------------
         Option optionForceRaftEvent = myGameInstance.Options.Find("ForceRaftEvent");
         if (null == optionForceRaftEvent)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetTravelOptions(): myGameInstance.Options.Find(ForceRaftEvent) returned null");
            return false;
         }
         myOptionForceRaftEvent = optionForceRaftEvent.IsEnabled;
         return true;
      }
      //-----------------------------------------------------------------------------------------
      private bool UpdateGrid()
      {
         if (false == UpdateEndState())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdatedEndState() returned false");
            return false;
         }
         if (EnumR204.TC_END == myState)
            return true;
         if (false == UpdateHeader())
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateHeader() returned false");
            return false;
         }
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
         if (EnumR204.TC_END == myState)
         {
            if (null == myCallback)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): myEndHuntCallback=null");
               return false;
            }
            if (false == myCallback(myIsLost, myIsEncounter, myIsRiverCrossing))
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): myEndHuntCallback() returned false");
               return false;
            }
         }
         return true;
      }
      private bool UpdateHeader()
      {
         myTextBlockHeader.Inlines.Clear();
         Button b1 = new Button() { Content = "r205", FontFamily = myFontFam1, FontSize = 12, Margin = new Thickness(5, 0, 0, 0), Height = 14 };
         Button b2 = new Button() { Content = "t207", FontFamily = myFontFam1, FontSize = 12, Margin = new Thickness(5, 0, 0, 0), Height = 14 };
         b1.Click += ButtonRule_Click;
         b2.Click += ButtonRule_Click;
         myStackPanelCheckMarks.Children.Clear();
         CheckBox cb1 = new CheckBox() { FontSize = 12, IsEnabled = false, IsChecked = myIsLost, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0), Content = "Party Lost" };
         CheckBox cb2 = new CheckBox() { FontSize = 12, IsEnabled = false, IsChecked = myIsEncounter, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0), Content = "Encounter" };
         switch (myState)
         {
            case EnumR204.TC_LOST_ROLL:
               myTextBlockHeader.Inlines.Add(new Run("e204 Travel"));
               b1.Content = "r204";
               myTextBlockHeader.Inlines.Add(new InlineUIContainer(b1));
               myTextBlockHeader.Inlines.Add(new InlineUIContainer(b2));
               break;
            case EnumR204.TC_DESERTION_ROLL:
            case EnumR204.TC_DESERTION_SHOW:
            case EnumR204.TC_LOST_SHOW_RESULTS:
            case EnumR204.TC_LOST_SHOW_CROSS_RESULT:
               myTextBlockHeader.Inlines.Add(new Run("e205 Lost"));
               myTextBlockHeader.Inlines.Add(new InlineUIContainer(b1));
               myTextBlockHeader.Inlines.Add(new InlineUIContainer(b2));
               break;
            case EnumR204.TC_EVENT_ROLL:  
               if (true == myIsLost)
               {
                  b1.Content = "r205";
                  myTextBlockHeader.Inlines.Add(new Run("e205 Lost"));
               }
               else
               {
                  if (true == myIsTravelingAir)
                     b1.Content = "r204d";
                  else
                     b1.Content = "r204b";
                  myTextBlockHeader.Inlines.Add(new Run("e204 Travel"));
               }
               myTextBlockHeader.Inlines.Add(new InlineUIContainer(b1));
               myTextBlockHeader.Inlines.Add(new InlineUIContainer(b2));
               break;
            case EnumR204.TC_EVENT_ROLL_ROAD:
               b1.Content = "r204b";
               myTextBlockHeader.Inlines.Add(new Run("e204 Travel"));
               myTextBlockHeader.Inlines.Add(new InlineUIContainer(b1));
               myTextBlockHeader.Inlines.Add(new InlineUIContainer(b2));
               break;
            case EnumR204.TC_EVENT_ROLL_EVENT_R230:
            case EnumR204.TC_EVENT_ROLL_EVENT_R232:
            case EnumR204.TC_EVENT_ROLL_EVENT_R232_SHOW:
               b1.Content = "r213";
               myTextBlockHeader.Inlines.Add(new Run("e204 Travel"));
               myTextBlockHeader.Inlines.Add(new InlineUIContainer(b1));
               myTextBlockHeader.Inlines.Add(new InlineUIContainer(b2));
               break;
            case EnumR204.TC_EVENT_ROLL_REFERENCE_R281:
            case EnumR204.TC_EVENT_ROLL_EVENT_R281:
            case EnumR204.TC_EVENT_ROLL_EVENT_R281_SHOW:
               myTextBlockHeader.Inlines.Add(new Run("e204 Travel"));
               b1.Content = "r204d";
               myTextBlockHeader.Inlines.Add(new InlineUIContainer(b1));
               myTextBlockHeader.Inlines.Add(new InlineUIContainer(b2));
               Button b3 = new Button() { Content = "r281", FontFamily = myFontFam1, FontSize = 12, Margin = new Thickness(5, 0, 0, 0), Height = 14 };
               b3.Click += ButtonRule_Click;
               myTextBlockHeader.Inlines.Add(new InlineUIContainer(b3));
               break;
            case EnumR204.TC_EVENT_ROLL_REFERENCE:
            case EnumR204.TC_EVENT_ROLL_EVENT:
            case EnumR204.TC_EVENT_SHOW_RESULTS:
               if (true == myIsLost)
               {
                  b1.Content = "r205";
                  myTextBlockHeader.Inlines.Add(new Run("e205 Lost"));
               }
               else
               {
                  if( true == myIsTravelingAir )
                     b1.Content = "r204d";
                  else
                     b1.Content = "r204b";
                  myTextBlockHeader.Inlines.Add(new Run("e204 Travel"));
               }
               myTextBlockHeader.Inlines.Add(new InlineUIContainer(b1));
               myTextBlockHeader.Inlines.Add(new InlineUIContainer(b2));
               break;
            case EnumR204.TC_END:
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateHeader(): reached default s=" + myState.ToString());
               return false;
         }
         myStackPanelCheckMarks.Children.Add(cb1);
         myStackPanelCheckMarks.Children.Add(cb2);
         return true;
      }
      private bool UpdateUserInstructions()
      {
         myTextBlockInstructions.Inlines.Clear();
         switch (myState)
         {
            case EnumR204.TC_LOST_ROLL:
               if (true == myIsTravelingAir)
                  myTextBlockInstructions.Inlines.Add(new Run("Roll for getting lost in air:"));
               else if (RiverCrossEnum.TC_ATTEMPTING_TO_CROSS == myMapItemMove.RiverCross)
                  myTextBlockInstructions.Inlines.Add(new Run("Roll to see if can cross river:"));
               else if (RiverCrossEnum.TC_CROSS_YES == myMapItemMove.RiverCross)
                  myTextBlockInstructions.Inlines.Add(new Run("River Crossed! Roll again for getting lost on other side of river in " + myMapItemMove.NewTerritory.Type + ":"));
               else
                  myTextBlockInstructions.Inlines.Add(new Run("Roll for getting lost trying to exit " + myMapItemMove.OldTerritory.Type + ":"));
               break;
            case EnumR204.TC_DESERTION_ROLL:
               if (true == myIsTravelingAir)
                  myTextBlockInstructions.Inlines.Add(new Run("Roll for guide deserting:"));
               else if (RiverCrossEnum.TC_CROSS_FAIL == myMapItemMove.RiverCross)
                  myTextBlockInstructions.Inlines.Add(new Run("Unable to find raft material or place to ford. Roll for guide deserting:"));
               else
                  myTextBlockInstructions.Inlines.Add(new Run("Roll for guide deserting:"));
               break;
            case EnumR204.TC_DESERTION_SHOW:
               myTextBlockInstructions.Inlines.Add(new Run("Click image to continue."));
               break;
            case EnumR204.TC_LOST_SHOW_CROSS_RESULT:
               myTextBlockInstructions.Inlines.Add(new Run("Unable to find raft material or place to ford. Click image to continue."));
               break;
            case EnumR204.TC_LOST_SHOW_RESULTS:
               myTextBlockInstructions.Inlines.Add(new Run("Click anywhere to continue."));
               break;
            case EnumR204.TC_EVENT_ROLL:
               if ((true == myIsTravelingAir) && (true == myIsLost) )
                  myTextBlockInstructions.Inlines.Add(new Run("Lost in Air - Roll for possible lost encounter:"));
               else if (true == myIsTravelingAir)
                  myTextBlockInstructions.Inlines.Add(new Run("Not lost. Roll for possible air encounter:"));
               else if (true == myGameInstance.IsAirborneEnd)
                  myTextBlockInstructions.Inlines.Add(new Run("Roll for possible landing encounter in " + myMapItemMove.NewTerritory.Type + ":"));
               else if (true == myIsRaftEncounter)
                  myTextBlockInstructions.Inlines.Add(new Run("Roll for possible rafting travel encounter:")); 
               else if (RiverCrossEnum.TC_CROSS_FAIL == myMapItemMove.RiverCross)
                  myTextBlockInstructions.Inlines.Add(new Run("River crossing failed! Roll for travel encounter on river:"));
               else if (RiverCrossEnum.TC_ATTEMPTING_TO_CROSS == myMapItemMove.RiverCross)
                  myTextBlockInstructions.Inlines.Add(new Run("Crossing River! Roll for possible travel encounter on river:"));
               else if (((RiverCrossEnum.TC_CROSS_YES == myMapItemMove.RiverCross) || (RiverCrossEnum.TC_CROSS_YES == myMapItemMove.RiverCross)) && (true == myIsLost))
                  myTextBlockInstructions.Inlines.Add(new Run("River crossed but lost! Roll for possible lost encounter in " + myMapItemMove.NewTerritory.Type + ":"));
               else if (((RiverCrossEnum.TC_CROSS_YES == myMapItemMove.RiverCross) || (RiverCrossEnum.TC_CROSS_YES == myMapItemMove.RiverCross)) && (false == myIsLost))
                  myTextBlockInstructions.Inlines.Add(new Run("River crossed and not lost. Roll for possible travel encounter in " + myMapItemMove.NewTerritory.Type + ":"));
               else if ( true == myIsLost )
                  myTextBlockInstructions.Inlines.Add(new Run("Lost - Movement Ends! Roll for possible lost encounter in " + myMapItemMove.NewTerritory.Type + ":"));
               else if (true == myIsTravelingRoad)
                  myTextBlockInstructions.Inlines.Add(new Run("Roll for possible road encounter:"));
               else
                  myTextBlockInstructions.Inlines.Add(new Run("Not Lost! Roll for possible travel encounter in " + myMapItemMove.NewTerritory.Type + ":"));
               break;
            case EnumR204.TC_EVENT_ROLL_ROAD:
               myTextBlockInstructions.Inlines.Add(new Run("No Lost Rolls on Road! Roll for Travel encounter off road in " + myMapItemMove.NewTerritory.Type + ":"));
               break;
            case EnumR204.TC_EVENT_ROLL_EVENT_R230:
            case EnumR204.TC_EVENT_ROLL_EVENT_R232:
               myTextBlockInstructions.Inlines.Add(new Run("Roll for rafting travel event:"));
               break;
            case EnumR204.TC_EVENT_ROLL_REFERENCE_R281:
               myTextBlockInstructions.Inlines.Add(new Run("Encounter on Ground! Roll for encounter in " + myMapItemMove.NewTerritory.Type + ":"));
               break;
            case EnumR204.TC_EVENT_ROLL_REFERENCE:
               if (true == myIsTravelingAir)
                  myTextBlockInstructions.Inlines.Add(new Run("Encounter! Roll for air travel reference:"));
               else if (true == myGameInstance.IsAirborneEnd)
                  myTextBlockInstructions.Inlines.Add(new Run("Encounter! Roll for travel event reference in " + myMapItemMove.NewTerritory.Type + ":"));
               else if (true == myIsRaftEncounter)
                  myTextBlockInstructions.Inlines.Add(new Run("Encounter! Roll for rafting travel reference:"));
               else if (RiverCrossEnum.TC_CROSS_FAIL == myMapItemMove.RiverCross)
                  myTextBlockInstructions.Inlines.Add(new Run("Encounter! Crossing river encounter:"));
               else if (RiverCrossEnum.TC_ATTEMPTING_TO_CROSS == myMapItemMove.RiverCross)
                  myTextBlockInstructions.Inlines.Add(new Run("River Encounter! Roll for encounter:"));
               else if (true == myIsLost)
                  myTextBlockInstructions.Inlines.Add(new Run("Lost Encounter! Roll for event reference in " + myMapItemMove.NewTerritory.Type + ":"));
               else if (true == myIsTravelingRoad)
                  myTextBlockInstructions.Inlines.Add(new Run("Encounter! Roll for travel event reference for road:"));
               else
                  myTextBlockInstructions.Inlines.Add(new Run("Encounter! Roll for travel event reference in " + myMapItemMove.NewTerritory.Type + ":"));
               break;
            case EnumR204.TC_EVENT_ROLL_EVENT:
               if (true == myIsTravelingAir)
                  myTextBlockInstructions.Inlines.Add(new Run("Roll for air travel event:"));
               else if (true == myGameInstance.IsAirborneEnd)
                  myTextBlockInstructions.Inlines.Add(new Run("Roll for travel event in " + myMapItemMove.NewTerritory.Type + ":"));
               else if (RiverCrossEnum.TC_CROSS_FAIL == myMapItemMove.RiverCross)
                  myTextBlockInstructions.Inlines.Add(new Run("Roll for travel event in crossing river:"));
               else if (RiverCrossEnum.TC_ATTEMPTING_TO_CROSS == myMapItemMove.RiverCross)
                  myTextBlockInstructions.Inlines.Add(new Run("Roll for travel event for river:"));
               else if ((true == myIsLost) && (RiverCrossEnum.TC_CROSS_YES_SHOWN == myMapItemMove.RiverCross))
                  myTextBlockInstructions.Inlines.Add(new Run("Lost Encounter! Roll for event in " + myMapItemMove.NewTerritory.Type + ":"));
               else if (true == myIsLost)
                  myTextBlockInstructions.Inlines.Add(new Run("Lost Encounter! Roll for event in " + myMapItemMove.OldTerritory.Type + ":"));
               else if (true == myIsTravelingRoad)
                  myTextBlockInstructions.Inlines.Add(new Run("Roll for travel event for road:"));
               else
                  myTextBlockInstructions.Inlines.Add(new Run("Roll for travel event in " + myMapItemMove.NewTerritory.Type + ":"));
               break;
            case EnumR204.TC_EVENT_ROLL_EVENT_R281:
               myTextBlockInstructions.Inlines.Add(new Run("Roll for travel event in " + myMapItemMove.NewTerritory.Type + ":"));
               break;
            case EnumR204.TC_EVENT_SHOW_RESULTS:
            case EnumR204.TC_EVENT_ROLL_EVENT_R232_SHOW:
            case EnumR204.TC_EVENT_ROLL_EVENT_R281_SHOW:
               myTextBlockInstructions.Inlines.Add(new Run("Click anywhere to continue."));
               break;
            case EnumR204.TC_END:
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateUserInstructions(): reached default s=" + myState.ToString());
               return false; ;
         }
         return true;
      }
      private bool UpdateAssignablePanel()
      {
         Rectangle r = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
         Rectangle r1 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemOffset };
         BitmapImage bmi = new BitmapImage();
         bmi.BeginInit();
         bmi.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
         bmi.EndInit();
         Image img1 = new Image { Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
         ImageBehavior.SetAnimatedSource(img1, bmi);
         myStackPanelAssignable.Children.Add(img1);
         Label label = new Label() { FontFamily = myFontFam2, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
         //-------------------------------------------------------------------
         myStackPanelAssignable.Children.Clear(); // clear out assignable panel 
         switch (myState)
         {
            case EnumR204.TC_LOST_ROLL:
               label.Content = GetLabelForLostCheck(myGameInstance.MapItemMoves[0]);
               myStackPanelAssignable.Children.Add(label);
               myStackPanelAssignable.Children.Add(img1);
               if (true == myGameInstance.IsEagleHunt) // they act as guides
               {
                  Label label1 = new Label() { FontFamily = myFontFam2, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = " -1 for " };
                  myStackPanelAssignable.Children.Add(label1);
                  Image img2 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Eagle"), Width = 2 * Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img2);
               }
               myStackPanelAssignable.Children.Add(r);
               break;
            case EnumR204.TC_DESERTION_ROLL:
               label.Content = "3 < Desertion";
               myStackPanelAssignable.Children.Add(r);
               myStackPanelAssignable.Children.Add(r1);
               myStackPanelAssignable.Children.Add(label);
               break;
            case EnumR204.TC_DESERTION_SHOW:
               Image img22 = null; 
               if (RiverCrossEnum.TC_CROSS_FAIL == myMapItemMove.RiverCross)
                  img22 = new Image { Source = MapItem.theMapImages.GetBitmapImage("RaftDeny"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               else
                  img22 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Nothing"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               label.Content = "3 < Desertion";
               myStackPanelAssignable.Children.Add(img22);
               myStackPanelAssignable.Children.Add(r1);
               myStackPanelAssignable.Children.Add(label);
               break;
            case EnumR204.TC_EVENT_ROLL_REFERENCE:
            case EnumR204.TC_EVENT_ROLL_EVENT:
            case EnumR204.TC_EVENT_ROLL_EVENT_R230:
            case EnumR204.TC_EVENT_ROLL_EVENT_R232:
            case EnumR204.TC_EVENT_ROLL_REFERENCE_R281:
            case EnumR204.TC_EVENT_ROLL_EVENT_R281:
               myStackPanelAssignable.Children.Add(img1);
               myStackPanelAssignable.Children.Add(r);
               break;
            case EnumR204.TC_EVENT_ROLL:
            case EnumR204.TC_EVENT_ROLL_ROAD:
               label.Content = GetLabelForEventCheck(myGameInstance.MapItemMoves[0]);
               myStackPanelAssignable.Children.Add(label);
               myStackPanelAssignable.Children.Add(img1);
               myStackPanelAssignable.Children.Add(r);
               break;
            case EnumR204.TC_LOST_SHOW_RESULTS:
            case EnumR204.TC_EVENT_SHOW_RESULTS:
            case EnumR204.TC_EVENT_ROLL_EVENT_R232_SHOW:
            case EnumR204.TC_EVENT_ROLL_EVENT_R281_SHOW:
               myStackPanelAssignable.Children.Add(r);
               break;
            case EnumR204.TC_LOST_SHOW_CROSS_RESULT:
               Image imgRaft = new Image { Source = MapItem.theMapImages.GetBitmapImage("RaftDeny"), Width=Utilities.ZOOM * Utilities.theMapItemSize, Height=Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(imgRaft);
               break;
            case EnumR204.TC_END:
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanel(): reached default s=" + myState.ToString());
               return false; ;
         }
         return true;
      }
      private bool UpdateGridRows()
      {
         //------------------------------------------------------------
         // Clear out existing Grid Row data
         List<UIElement> results = new List<UIElement>();
         foreach (UIElement ui in myGridTravelTable.Children)
         {
            int rowNum = Grid.GetRow(ui);
            if (STARTING_ASSIGNED_ROW <= rowNum)
               results.Add(ui);
         }
         foreach (UIElement ui1 in results)
            myGridTravelTable.Children.Remove(ui1);
         //------------------------------------------------------------
         switch (myState)
         {
            case EnumR204.TC_DESERTION_ROLL:
            case EnumR204.TC_DESERTION_SHOW:
               for (int i = 0; i < myLocalGuides.Count; ++i)
                  UpdateGridRowForDesertion(i);
               break;
            case EnumR204.TC_LOST_ROLL:
            case EnumR204.TC_EVENT_ROLL:
            case EnumR204.TC_EVENT_ROLL_ROAD:
            case EnumR204.TC_EVENT_ROLL_REFERENCE:
            case EnumR204.TC_LOST_SHOW_CROSS_RESULT:
               break;
            case EnumR204.TC_EVENT_ROLL_EVENT:
            case EnumR204.TC_EVENT_ROLL_EVENT_R230:
            case EnumR204.TC_EVENT_ROLL_EVENT_R281:
               if (false == UpdateReferenceRow(myGameInstance.MapItemMoves[0]))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateReferenceRow() returned false");
                  return false;
               }
               break;
            case EnumR204.TC_LOST_SHOW_RESULTS:
            case EnumR204.TC_EVENT_SHOW_RESULTS:
            case EnumR204.TC_EVENT_ROLL_EVENT_R232_SHOW:
            case EnumR204.TC_EVENT_ROLL_REFERENCE_R281:
            case EnumR204.TC_EVENT_ROLL_EVENT_R281_SHOW:
               if (false == UpdateReferenceRow(myGameInstance.MapItemMoves[0]))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateReferenceRow() returned false");
                  return false;
               }
               if (false == UpdateEventRow())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateEventRow() returned false");
                  return false;
               }
               break;
            case EnumR204.TC_END:
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): reached default s=" + myState.ToString());
               return false;
         }
         return true;
      }
      private void UpdateGridRowForDesertion(int i)
      {
         int rowNum = i + STARTING_ASSIGNED_ROW;
         Button b = CreateButton(myGridRows[i].myAssignable);
         myGridTravelTable.Children.Add(b);
         Grid.SetRow(b, rowNum);
         Grid.SetColumn(b, 0);
         //----------------------------------------
         CheckBox cb = new CheckBox() { FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
         if ((1 == myLocalGuides.Count) || (Utilities.NO_RESULT < myGridRows[i].myResult))
         {
            cb.IsEnabled = false;
         }
         else
         {
            cb.IsEnabled = true;
            cb.Checked += CheckBoxPrimary_Click;
            cb.Unchecked += CheckBoxPrimary_Click;
         }
         if (true == myGridRows[i].myIsSelected)
            cb.IsChecked = true;
         else
            cb.IsChecked = false;
         myGridTravelTable.Children.Add(cb);
         Grid.SetRow(cb, rowNum);
         Grid.SetColumn(cb, 1);
         //----------------------------------------
         if (Utilities.NO_RESULT == myGridRows[i].myResult)
         {
            if (true == myGridRows[i].myIsSelected)
            {
               BitmapImage bmi = new BitmapImage();
               bmi.BeginInit();
               bmi.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
               bmi.EndInit();
               Image img1 = new Image { Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
               ImageBehavior.SetAnimatedSource(img1, bmi);
               myGridTravelTable.Children.Add(img1);
               Grid.SetRow(img1, rowNum);
               Grid.SetColumn(img1, 3);
            }
         }
         else
         {
            Label label = new Label() { FontFamily = myFontFam2, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myResult.ToString() };
            myGridTravelTable.Children.Add(label);
            Grid.SetRow(label, rowNum);
            Grid.SetColumn(label, 3);
         }
         //----------------------------------------
         if (Utilities.NO_RESULT < myGridRows[i].myResult)
         {
            string answer = "yes";
            if (myGridRows[i].myResult < 4)
               answer = "no";
            Label label = new Label() { FontFamily = myFontFam2, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = answer };
            myGridTravelTable.Children.Add(label);
            Grid.SetRow(label, rowNum);
            Grid.SetColumn(label, 6);
         }
      }
      private bool UpdateReferenceRow(IMapItemMove mim)
      {
         Label label = new Label() { FontFamily = myFontFam1, FontSize = 16, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Reference Row:" };
         myGridTravelTable.Children.Add(label);
         Grid.SetRow(label, STARTING_ASSIGNED_ROW);
         Grid.SetColumn(label, 0);
         //----------------------------------------------
         myKeyReference = "";
         if (true == myIsTravelingAir) // Check for airborne
         {
            myKeyReference = "Airborne";
            if ( (EnumR204.TC_EVENT_ROLL_EVENT_R281 == myState) || ( EnumR204.TC_EVENT_ROLL_EVENT_R281_SHOW == myState ) )
               myKeyReference = mim.NewTerritory.Type;
         }
         else if (true == myIsRaftEncounter)
         {
            if((EnumR204.TC_EVENT_ROLL_EVENT_R232 == myState) || (EnumR204.TC_EVENT_ROLL_EVENT_R232_SHOW == myState))
               myKeyReference = "Rafting1";
            else
               myKeyReference = "Rafting";
         }
         if ("" == myKeyReference)
         {
            foreach (String road in mim.OldTerritory.Roads) // Check for road travel
            {
               if (road == mim.NewTerritory.Name)
               {
                  myKeyReference = "On Road";
                  break;
               }
            }
         }
         if ("" == myKeyReference)
         {
            if ((RiverCrossEnum.TC_CROSS_FAIL == myMapItemMove.RiverCross) || (RiverCrossEnum.TC_ATTEMPTING_TO_CROSS == myMapItemMove.RiverCross))
            {
               foreach (String river in mim.OldTerritory.Rivers) // Check for crossing rivers
               {
                  if (river == mim.NewTerritory.Name)
                  {
                     myKeyReference = "Cross River";
                     break;
                  }
               }
            }
         }
         if ("" == myKeyReference)
            myKeyReference = mim.NewTerritory.Type;
         //------------------------------------------------
         try
         {
            Button[] buttons = new Button[6];
            buttons[0] = new Button() { IsEnabled = false, FontFamily = myFontFam1, FontSize = 16, Height = 20, Background = Utilities.theBrushControlButton };
            buttons[1] = new Button() { IsEnabled = false, FontFamily = myFontFam1, FontSize = 16, Height = 20, Background = Utilities.theBrushControlButton };
            buttons[2] = new Button() { IsEnabled = false, FontFamily = myFontFam1, FontSize = 16, Height = 20, Background = Utilities.theBrushControlButton };
            buttons[3] = new Button() { IsEnabled = false, FontFamily = myFontFam1, FontSize = 16, Height = 20, Background = Utilities.theBrushControlButton };
            buttons[4] = new Button() { IsEnabled = false, FontFamily = myFontFam1, FontSize = 16, Height = 20, Background = Utilities.theBrushControlButton };
            buttons[5] = new Button() { IsEnabled = false, FontFamily = myFontFam1, FontSize = 16, Height = 20, Background = Utilities.theBrushControlButton };
            for (int i = 0; i < 6; i++)
            {
               if (i + 1 == myRollReference)
                  buttons[i].IsEnabled = true;
               myGridTravelTable.Children.Add(buttons[i]);
               Grid.SetRow(buttons[i], STARTING_ASSIGNED_ROW);
               Grid.SetColumn(buttons[i], i + 1);
               buttons[i].Content = myReferences[myKeyReference][i];
            }
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateReferenceRow(): e=" + e.ToString());
            return false;
         }
         return true;
      }
      private bool UpdateEventRow()
      {
         int rowNum = STARTING_ASSIGNED_ROW + 1;
         Label label = new Label() { FontFamily = myFontFam1, FontSize = 16, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = "Event Row:" };
         myGridTravelTable.Children.Add(label);
         Grid.SetRow(label, rowNum);
         Grid.SetColumn(label, 0);
         //------------------------------------------------
         string key = "";
         int index = myRollReference - 1;
         try
         {
            key = myReferences[myKeyReference][index];
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateEventRow(): k=" + key + "=myReferences[" + myKeyReference + "][" + index.ToString() + "] e=" + e.ToString());
            return false;
         }
         //---------------------------------------------
         if (("Farmland" == myKeyReference) && (1 == myRollReference))
         {
            Button b = new Button() { Content = "e009", IsEnabled = true, FontFamily = myFontFam1, FontSize = 16, Height = 20, Background = Utilities.theBrushControlButton };
            myGridTravelTable.Children.Add(b);
            Grid.SetRow(b, STARTING_ASSIGNED_ROW + 1);
            Grid.SetColumn(b, 1);
            Grid.SetColumnSpan(b, 6);
            myGameInstance.EventDisplayed = myGameInstance.EventActive = "e009";
            if(EnumR204.TC_EVENT_ROLL_EVENT_R281 == myState)
               myState = myState = EnumR204.TC_EVENT_SHOW_RESULTS;
            else
               myState = EnumR204.TC_EVENT_SHOW_RESULTS;
         }
         else if (EnumR204.TC_EVENT_ROLL_REFERENCE_R281 == myState)
         {
            Button b = new Button() { Content = "r281", IsEnabled = true, FontFamily = myFontFam1, FontSize = 16, Height = 20, Background = Utilities.theBrushControlButton };
            myGridTravelTable.Children.Add(b);
            Grid.SetRow(b, STARTING_ASSIGNED_ROW + 1);
            Grid.SetColumn(b, 1);
            Grid.SetColumnSpan(b, 6);
         }
         else
         {
            string content = ""; // Want to show all buttons. However, only the selected button should be highlighted
            int i = 0;
            Button[] buttons = new Button[6];
            buttons[0] = new Button() { IsEnabled = false, FontFamily = myFontFam1, FontSize = 16, Height = 20, Background = Utilities.theBrushControlButton };
            buttons[1] = new Button() { IsEnabled = false, FontFamily = myFontFam1, FontSize = 16, Height = 20, Background = Utilities.theBrushControlButton };
            buttons[2] = new Button() { IsEnabled = false, FontFamily = myFontFam1, FontSize = 16, Height = 20, Background = Utilities.theBrushControlButton };
            buttons[3] = new Button() { IsEnabled = false, FontFamily = myFontFam1, FontSize = 16, Height = 20, Background = Utilities.theBrushControlButton };
            buttons[4] = new Button() { IsEnabled = false, FontFamily = myFontFam1, FontSize = 16, Height = 20, Background = Utilities.theBrushControlButton };
            buttons[5] = new Button() { IsEnabled = false, FontFamily = myFontFam1, FontSize = 16, Height = 20, Background = Utilities.theBrushControlButton };
            for (i = 0; i < 6; ++i)
            {
               try
               {
                  if (i + 1 == myRollEvent)
                  {
                     buttons[i].IsEnabled = true;
                     myGameInstance.EventDisplayed = myGameInstance.EventActive = myEvents[key][i];
                     if( ("e044" == myEvents[key][i]) && (false == myGameInstance.IsReligionInParty()) )
                        myGameInstance.EventDisplayed = myGameInstance.EventActive = "e044a";
                     else if (("e050" == myEvents[key][i]) && (true == IsStructureWithinRange(myGameInstance,3)) )
                        myGameInstance.EventDisplayed = myGameInstance.EventActive = "e050a";
                     else if (("e094" == myEvents[key][i]) && ("Cross River" == myKeyReference)) // crocs in river instead of swamp
                        myGameInstance.EventDisplayed = myGameInstance.EventActive = "e094a";
                     else if (("e110" == myEvents[key][i]) && (true == myGameInstance.IsSpecialistInParty())) // encounter air spirit
                        myGameInstance.EventDisplayed = myGameInstance.EventActive = "e110a";
                     else if (("e111" == myEvents[key][i]) && (true == myGameInstance.IsSpecialistInParty())) // encounter storm demon
                        myGameInstance.EventDisplayed = myGameInstance.EventActive = "e111a";
                  }
               }
               catch (Exception e)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateEventRow(): k=" + key + "=myReferences[" + myKeyReference + "][=" + i.ToString() + "] roll=" + myRollEvent.ToString() + " c=" + content + " e=" + e.ToString());
                  return false;
               }
               buttons[i].Content = myEvents[key][i];
               myGridTravelTable.Children.Add(buttons[i]);
               Grid.SetRow(buttons[i], rowNum);
               Grid.SetColumn(buttons[i], i + 1);
            }
         }
         return true;
      }
      //-----------------------------------------------------------------------------------------
      public void ShowDieResults(int dieRoll)
      {
         Logger.Log(LogEnum.LE_UNDO_COMMAND, "EventViewerTravelTable.ShowDieResults(): cmd=" + myGameInstance.IsUndoCommandAvailable.ToString() + "-->false");
         myGameInstance.IsUndoCommandAvailable = false;
         Logger.Log(LogEnum.LE_VIEW_TRAVEL_CHECK, "ShowDieResults(): s=" + myState.ToString() + " dr=" + dieRoll.ToString());
         switch (myState)
         {
            case EnumR204.TC_LOST_ROLL:
               if (true == myOptionNoLostEvent) 
                  dieRoll = 0;
               else if (true == myOptionForceLostEvent) 
                  dieRoll = 13;
               myState = GetLostResult(myGameInstance.MapItemMoves[0], dieRoll);
               Logger.Log(LogEnum.LE_VIEW_TRAVEL_CHECK, "ShowDieResults(): state=TC_LOST_ROLL-->" + myState.ToString() + " dr=" + dieRoll.ToString());
               break;
            case EnumR204.TC_DESERTION_ROLL:
               myState = EnumR204.TC_DESERTION_SHOW;
               int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
               myGridRows[i].myResult = dieRoll;
               if (4 < dieRoll)
               {
                  IMapItem guide = myGridRows[i].myAssignable;
                  myGameInstance.RemoveAbandonerInParty(guide);
               }
               break;
            case EnumR204.TC_EVENT_ROLL:
               if (true == myIsLost)
               {
                  if (true == myOptionNoLostEvent)
                     dieRoll = 0;
                  else if (true == myOptionForceLostEvent)
                     dieRoll = 13;
               }
               else // only check travel event if not already lost 
               {
                  if (true == myOptionForceNoEvent)
                     dieRoll = 0;
                  else if (true == myOptionForceEvent)
                     dieRoll = 13;
               }
               myState = GetEventResult(myGameInstance.MapItemMoves[0], dieRoll);
               Logger.Log(LogEnum.LE_VIEW_TRAVEL_CHECK, "ShowDieResults(): state=TC_EVENT_ROLL-->" + myState.ToString() + " dr=" + dieRoll.ToString());
               break;
            case EnumR204.TC_EVENT_ROLL_ROAD:
               myIsTravelingRoad = false;  // Road event already checked. Switch to non-road check
               if (true == myOptionForceNoEvent)
                  dieRoll = 0;
               else if (true == myOptionForceEvent)
                  dieRoll = 13;
               myState = GetEventResult(myGameInstance.MapItemMoves[0], dieRoll);
               Logger.Log(LogEnum.LE_VIEW_TRAVEL_CHECK, "ShowDieResults(): state=TC_EVENT_ROLL_ROAD-->" + myState.ToString() + " dr=" + dieRoll.ToString());
               break;
            case EnumR204.TC_EVENT_ROLL_REFERENCE:
               //dieRoll = 5; // <cgs> TEST
               myRollReference = dieRoll; // column number in travel table r207 - reference row
               if ((6 == myRollReference) && (true == myIsTravelingAir) ) // if traveling by air and roll reference 6, need to look  at Table r281
                  myState = EnumR204.TC_EVENT_ROLL_REFERENCE_R281;
               else
                  myState = EnumR204.TC_EVENT_ROLL_EVENT;
               Logger.Log(LogEnum.LE_VIEW_TRAVEL_CHECK, "ShowDieResults(): state=TC_EVENT_ROLL_REFERENCE-->" + myState.ToString() + " dr=" + dieRoll.ToString());
               break;
            case EnumR204.TC_EVENT_ROLL_EVENT:
               //dieRoll = 3; // <cgs> TEST
               myRollEvent = dieRoll; // column number in traveling event reference - event row
               myState = EnumR204.TC_EVENT_SHOW_RESULTS;
               Logger.Log(LogEnum.LE_VIEW_TRAVEL_CHECK, "ShowDieResults(): state=TC_EVENT_ROLL_EVENT-->" + myState.ToString() + " dr=" + dieRoll.ToString());
               break;
            case EnumR204.TC_EVENT_ROLL_EVENT_R230:
               myState = EnumR204.TC_EVENT_SHOW_RESULTS;
               //dieRoll = 3; // <cgs> TEST
               myRollReference = dieRoll;
               if (dieRoll < 7)
               {
                  myTextBlockCol1.Text = " ";
                  myTextBlockCol2.Text = "2";
                  myTextBlockCol3.Text = "3";
                  myTextBlockCol4.Text = "4";
                  myTextBlockCol5.Text = "5";
                  myTextBlockCol6.Text = "6";
               }
               else
               {
                  myTextBlockCol1.Text = "7";
                  myTextBlockCol2.Text = "8";
                  myTextBlockCol3.Text = "9";
                  myTextBlockCol4.Text = "10";
                  myTextBlockCol5.Text = "11";
                  myTextBlockCol6.Text = "12";
                  dieRoll -= 6;
               }
               myRollEvent = dieRoll;
               Logger.Log(LogEnum.LE_VIEW_TRAVEL_CHECK, "ShowDieResults(): state=TC_EVENT_ROLL_EVENT_R230-->" + myState.ToString() + " dr=" + dieRoll.ToString());
               break;
            case EnumR204.TC_EVENT_ROLL_EVENT_R232:
               myRollReference = dieRoll;
               myRollEvent     = dieRoll; 
               myState = EnumR204.TC_EVENT_ROLL_EVENT_R232_SHOW;
               Logger.Log(LogEnum.LE_VIEW_TRAVEL_CHECK, "ShowDieResults(): state=TC_EVENT_ROLL_EVENT_R232-->" + myState.ToString() + " dr=" + dieRoll.ToString());
               break;
            case EnumR204.TC_EVENT_ROLL_REFERENCE_R281:
               myRollReference = dieRoll; // column number in travel table r207 - reference row
               myState = EnumR204.TC_EVENT_ROLL_EVENT_R281;
               Logger.Log(LogEnum.LE_VIEW_TRAVEL_CHECK, "ShowDieResults(): state=TC_EVENT_ROLL_REFERENCE_R281-->" + myState.ToString() + " dr=" + dieRoll.ToString());
               break;
            case EnumR204.TC_EVENT_ROLL_EVENT_R281:
               myRollEvent = dieRoll; // column number in traveling event reference - event row
               myState = EnumR204.TC_EVENT_ROLL_EVENT_R281_SHOW;
               Logger.Log(LogEnum.LE_VIEW_TRAVEL_CHECK, "ShowDieResults(): state=TC_EVENT_ROLL_EVENT_R281-->" + myState.ToString() + " dr=" + dieRoll.ToString());
               break;
            default:
               break;
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowDieResults(): UpdateGrid() return false");
         myIsRollInProgress = false;
      }
      private bool CheckRoadTravel(ITerritory startT, ITerritory endT)
      {
         foreach (String roadHex in startT.Roads) // Check if traveling on road
         {
            if (roadHex == endT.Name)
               return true;
         }
         return false;
      }
      private String GetLabelForLostCheck(IMapItemMove mim)
      {
         int dieRollNeeded = 0;
         if (true == myIsTravelingAir) // Check for airborne
         {
            dieRollNeeded = 11 + theConsecutiveLostCount;
            if (0 < myLocalGuides.Count)
               ++dieRollNeeded;
            String rtn = " " + dieRollNeeded.ToString() + " < ";
            return rtn;
         }
         if (RiverCrossEnum.TC_ATTEMPTING_TO_CROSS == myMapItemMove.RiverCross)
         {
            dieRollNeeded = 7 + theConsecutiveLostCount;
            if (0 < myLocalGuides.Count)
               ++dieRollNeeded;
            String rtn = " " + dieRollNeeded.ToString() + " < ";
            return rtn;
         }
         ITerritory tLost = mim.OldTerritory; // Roll check performed on territory being left
         if (RiverCrossEnum.TC_CROSS_YES_SHOWN == myMapItemMove.RiverCross)
            tLost = mim.NewTerritory;
         switch (tLost.Type)
         {
            case "Farmland":
               dieRollNeeded = 9 + theConsecutiveLostCount;
               break;
            case "Countryside":
               dieRollNeeded = 8 + theConsecutiveLostCount;
               break;
            case "Forest":
            case "Hills":
               dieRollNeeded = 7 + theConsecutiveLostCount;
               break;
            case "Mountains":
               dieRollNeeded = 6 + theConsecutiveLostCount;
               break;
            case "Swamp":
               dieRollNeeded = 4 + theConsecutiveLostCount;
               break;
            case "Desert":
               dieRollNeeded = 5 + theConsecutiveLostCount;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "GetLabelForLostCheck(): reached default t=" + mim.NewTerritory.Type);
               return "ERROR";
         }
         if (0 < myLocalGuides.Count)
            ++dieRollNeeded;
         String returnStatus = " " + dieRollNeeded.ToString() + " < ";
         return returnStatus;
      }
      private EnumR204 GetLostResult(IMapItemMove mim, int dieRoll)
      {
         if ( (0 < myLocalGuides.Count) || ( true == myGameInstance.IsEagleHunt ) )
            --dieRoll;
         EnumR204 state = EnumR204.TC_EVENT_ROLL;
         if (true == myIsTravelingAir) // Check for airborne
         {
            if (11 + theConsecutiveLostCount < dieRoll)
            {
               Logger.Log(LogEnum.LE_VIEW_TRAVEL_CHECK, "GetLostResult(): myLost=true for t=air s=" + myState.ToString() + " dr=" + dieRoll.ToString());
               myIsLost = true;
               if (true == myOptionAutoLostDecrease)
                  ++theConsecutiveLostCount;
               if (0 < myLocalGuides.Count)
               {
                  myTextBlockCol0.Text = "Guide";
                  myTextBlockCol0.HorizontalAlignment = HorizontalAlignment.Center;
                  myTextBlockCol1.Text = "Primary";
                  myTextBlockCol2.Visibility = Visibility.Hidden;
                  myTextBlockCol3.Text = "Roll";
                  myTextBlockCol4.Visibility = Visibility.Hidden;
                  myTextBlockCol5.Visibility = Visibility.Hidden;
                  myTextBlockCol6.Text = "Desert?";
                  state = EnumR204.TC_DESERTION_ROLL;
               }
               else
               {
                  state = EnumR204.TC_END;  // GetLostResult() - myIsTravelingAir
               }
            }
            else
            {
               theConsecutiveLostCount = 0;
            }
            return state;
         }
         if (RiverCrossEnum.TC_ATTEMPTING_TO_CROSS == myMapItemMove.RiverCross)
         {
            if (7 + theConsecutiveLostCount < dieRoll)
            {
               Logger.Log(LogEnum.LE_VIEW_TRAVEL_CHECK, "GetLostResult(): myLost=true for t=river cross s=" + myState.ToString() + " dr=" + dieRoll.ToString());
               myIsLost = true;
               if (true == myOptionAutoLostDecrease)
                  ++theConsecutiveLostCount;
               myMapItemMove.RiverCross = RiverCrossEnum.TC_CROSS_FAIL;
               if (0 < myLocalGuides.Count)
               {
                  myTextBlockCol0.Text = "Guide";
                  myTextBlockCol0.HorizontalAlignment = HorizontalAlignment.Center;
                  myTextBlockCol1.Text = "Primary";
                  myTextBlockCol2.Visibility = Visibility.Hidden;
                  myTextBlockCol3.Text = "Roll";
                  myTextBlockCol4.Visibility = Visibility.Hidden;
                  myTextBlockCol5.Visibility = Visibility.Hidden;
                  myTextBlockCol6.Text = "Desert?";
                  return EnumR204.TC_DESERTION_ROLL;
               }
               return EnumR204.TC_LOST_SHOW_CROSS_RESULT;   // GetLostResult() - attempting to cross river fails, so show river crossing failure
            }
            theConsecutiveLostCount = 0;
            return EnumR204.TC_EVENT_ROLL;   // GetLostResult() - found place to crossed river. Now determine if there event on river
         }
         ITerritory tLost = mim.OldTerritory;
         if (RiverCrossEnum.TC_CROSS_YES_SHOWN == myMapItemMove.RiverCross)
            tLost = mim.NewTerritory;
         switch (tLost.Type)
         {
            case "Farmland":
               if (9 + theConsecutiveLostCount < dieRoll)
               {
                  Logger.Log(LogEnum.LE_VIEW_TRAVEL_CHECK, "GetLostResult(): myLost=true for t=farmland s=" + myState.ToString() + " dr=" + dieRoll.ToString());
                  myIsLost = true;
                  if (true == myOptionAutoLostDecrease)
                     ++theConsecutiveLostCount;
                  if (0 < myLocalGuides.Count)
                     state = EnumR204.TC_DESERTION_ROLL;
                  else
                     state = EnumR204.TC_EVENT_ROLL;  // GetLostResult()
               }
               else
               {
                  theConsecutiveLostCount = 0;
               }
               break;
            case "Countryside":
               if (8 + theConsecutiveLostCount < dieRoll)
               {
                  Logger.Log(LogEnum.LE_VIEW_TRAVEL_CHECK, "GetLostResult(): myLost=true for t=countryside s=" + myState.ToString() + " dr=" + dieRoll.ToString());
                  myIsLost = true;
                  if (true == myOptionAutoLostDecrease)
                     ++theConsecutiveLostCount;
                  if (0 < myLocalGuides.Count)
                     state = EnumR204.TC_DESERTION_ROLL;
                  else
                     state = EnumR204.TC_EVENT_ROLL;  // GetLostResult()
               }
               else
               {
                  theConsecutiveLostCount = 0;
               }
               break;
            case "Forest":
               if (7 + theConsecutiveLostCount < dieRoll)
               {
                  Logger.Log(LogEnum.LE_VIEW_TRAVEL_CHECK, "GetLostResult(): myLost=true for t=forest s=" + myState.ToString() + " dr=" + dieRoll.ToString());
                  myIsLost = true;
                  ++theConsecutiveLostCount;
                  if (0 < myLocalGuides.Count)
                     state = EnumR204.TC_DESERTION_ROLL;
                  else
                     state = EnumR204.TC_EVENT_ROLL;  // GetLostResult()
               }
               else
               {
                  theConsecutiveLostCount = 0;
               }
               break;
            case "Hills":
               if (7 + theConsecutiveLostCount < dieRoll)
               {
                  Logger.Log(LogEnum.LE_VIEW_TRAVEL_CHECK, "GetLostResult(): myLost=true for t=hills s=" + myState.ToString() + " dr=" + dieRoll.ToString());
                  myIsLost = true;
                  if (true == myOptionAutoLostDecrease)
                     ++theConsecutiveLostCount;
                  if (0 < myLocalGuides.Count)
                     state = EnumR204.TC_DESERTION_ROLL;
                  else
                     state = EnumR204.TC_EVENT_ROLL;  // GetLostResult()
               }
               else
               {
                  theConsecutiveLostCount = 0;
               }
               break;
            case "Mountains":
               if (6 + theConsecutiveLostCount < dieRoll)
               {
                  Logger.Log(LogEnum.LE_VIEW_TRAVEL_CHECK, "GetLostResult(): myLost=true for t=mountains s=" + myState.ToString() + " dr=" + dieRoll.ToString());
                  myIsLost = true;
                  if (true == myOptionAutoLostDecrease)
                     ++theConsecutiveLostCount;
                  if (0 < myLocalGuides.Count)
                     state = EnumR204.TC_DESERTION_ROLL;
                  else
                     state = EnumR204.TC_EVENT_ROLL;  // GetLostResult()
               }
               else
               {
                  theConsecutiveLostCount = 0;
               }
               break;
            case "Swamp":
               if (4 + theConsecutiveLostCount < dieRoll)
               {
                  Logger.Log(LogEnum.LE_VIEW_TRAVEL_CHECK, "GetLostResult(): myLost=true for t=swamp s=" + myState.ToString() + " dr=" + dieRoll.ToString());
                  myIsLost = true;
                  if (true == myOptionAutoLostDecrease)
                     ++theConsecutiveLostCount;
                  if (0 < myLocalGuides.Count)
                     state = EnumR204.TC_DESERTION_ROLL;
                  else
                     state = EnumR204.TC_EVENT_ROLL;  // GetLostResult()
               }
               else
               {
                  theConsecutiveLostCount = 0;
               }
               break;
            case "Desert":
               if (5 + theConsecutiveLostCount < dieRoll)
               {
                  Logger.Log(LogEnum.LE_VIEW_TRAVEL_CHECK, "GetLostResult(): myLost=true for t=desert s=" + myState.ToString() + " dr=" + dieRoll.ToString());
                  myIsLost = true;
                  if( true == myOptionAutoLostDecrease )
                     ++theConsecutiveLostCount;
                  if (0 < myLocalGuides.Count)
                     state = EnumR204.TC_DESERTION_ROLL;
                  else
                     state = EnumR204.TC_EVENT_ROLL;  // GetLostResult()
               }
               else
               {
                  theConsecutiveLostCount = 0;
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "GetLostResult(): reached default t=" + mim.NewTerritory.Type);
               state = EnumR204.TC_ERROR;
               break;
         }
         if (EnumR204.TC_DESERTION_ROLL == state)
         {
            myTextBlockCol0.Text = "Guide";
            myTextBlockCol0.HorizontalAlignment = HorizontalAlignment.Center;
            myTextBlockCol1.Text = "Primary";
            myTextBlockCol2.Visibility = Visibility.Hidden;
            myTextBlockCol3.Text = "Roll";
            myTextBlockCol4.Visibility = Visibility.Hidden;
            myTextBlockCol5.Visibility = Visibility.Hidden;
            myTextBlockCol6.Text = "Desert?";
         }
         return state;
      }
      private String GetLabelForEventCheck(IMapItemMove mim)
      {
         if (true == myIsTravelingAir) // Check for airborne
         {
            if( true == myIsLost )
               return " 3 < ";
            else
               return " 9 < ";
         }

         if (true == myIsRaftEncounter)
            return " 9 < ";
         foreach (String river in mim.OldTerritory.Rivers) // Check for crossing rivers
         {
            if (river == mim.NewTerritory.Name)
            {
               if (RiverCrossEnum.TC_CROSS_FAIL == myMapItemMove.RiverCross)
                  return " 9 < ";
               else if (RiverCrossEnum.TC_ATTEMPTING_TO_CROSS == myMapItemMove.RiverCross)
                  return " 9 < ";
            }
         }
         foreach (String road in mim.OldTerritory.Roads) // Check for crossing roads
         {
            if (road == mim.NewTerritory.Name)
               return " 8 < ";
         }
         switch (mim.NewTerritory.Type)
         {
            case "Farmland":
               return " 7 < ";
            case "Countryside":
               return " 8 < ";
            case "Forest":
               return " 8 < ";
            case "Hills":
               return " 9 < ";
            case "Mountains":
               return " 8 < ";
            case "Swamp":
               return " 9 < ";
            case "Desert":
               return " 9 < ";
            default:
               Logger.Log(LogEnum.LE_ERROR, "GetLabelForEventCheck(): reached default t=" + mim.NewTerritory.Type);
               break;
         }
         return "ERROR";
      }
      private EnumR204 GetEventResult(IMapItemMove mim, int dieRoll)
      {
         EnumR204 state = EnumR204.TC_ERROR;
         //----------------------------------------------------------------------------
         if (( true == myIsTravelingRoad) && (false == myOptionForceNoRoadEvent) )
            state = EnumR204.TC_EVENT_ROLL_ROAD;
         else
            state = EnumR204.TC_END;
         if (null != myGameInstance.GoblinKeeps.Find(mim.NewTerritory.Name)) // if reenter goblin keep hex, automatic capture per e054
         {
            myGameInstance.EventDisplayed = myGameInstance.EventActive = "e054c";
            myGameInstance.IsJailed = true;
            myIsEncounter = true;
            return state;
         }
         if (null != myGameInstance.ElfTowns.Find(mim.NewTerritory.Name)) // if reenter elf town may be arrested
         {
            myGameInstance.EventDisplayed = myGameInstance.EventActive = "e165";
            myIsEncounter = true;
            return state;
         }
         if (null != myGameInstance.ElfCastles.Find(mim.NewTerritory.Name)) // if reenter elf castle may be arrested
         {
            myGameInstance.EventDisplayed = myGameInstance.EventActive = "e166";
            myIsEncounter = true;
            return state;
         }
         if (null != myGameInstance.WizardTowers.Find(mim.NewTerritory.Name)) // if reenter wizard tower all magic users arrested and leave party
         {
            myGameInstance.EventDisplayed = myGameInstance.EventActive = "e068b";
            myIsEncounter = true;
            return state;
         }
         //-------------------------------------------------------
         if (true == myIsTravelingAir) 
         {
            if (true == myOptionForceNoAirEvent)
               dieRoll = 0;
            else if (true == myOptionForceAirEvent)
               dieRoll = 13;
            if (9 < dieRoll)
            {
               myIsEncounter = true;
               state = EnumR204.TC_EVENT_ROLL_REFERENCE;
            }
            return state;
         }
         //-------------------------------------------------------
         if (true == myIsRaftEncounter)
         {
            if (true == myOptionForceNoRaftEvent)
               dieRoll = 0;
            else if (true == myOptionForceRaftEvent)
               dieRoll = 13;
            if (9 < dieRoll)
            {
               myIsEncounter = true;
               state = EnumR204.TC_EVENT_ROLL_EVENT_R230;
            }
            return state;
         }
         //-------------------------------------------------------
         if(RiverCrossEnum.TC_CROSS_YES_SHOWN != myMapItemMove.RiverCross ) // if already accross river, do not check for river crossing
         {
            foreach (String riverHex in mim.OldTerritory.Rivers) // Check for crossing rivers
            {
               if ((riverHex == mim.NewTerritory.Name) && (RiverCrossEnum.TC_CROSS_YES != myMapItemMove.RiverCross))
               {
                  myIsRiverCrossing = true;
                  if (true == myOptionForceNoCrossEvent)
                     dieRoll = 0;
                  else if (true == myOptionForceCrossEvent)
                     dieRoll = 13;
                  if (9 < dieRoll)
                  {
                     myIsEncounter = true;
                     return EnumR204.TC_EVENT_ROLL_REFERENCE;
                  }
                  if (true == myIsLost)
                     return EnumR204.TC_END; // if lost but no encounter, end travel
                  myMapItemMove.RiverCross = RiverCrossEnum.TC_CROSS_YES; // river is crossed
                  return state; // need to roll for getting lost on other side of river
               }
            }
         }
         //-------------------------------------------------------
         if ((true == myIsTravelingRoad) && (false == myOptionForceNoRoadEvent))
         {
            if (8 < dieRoll)
            {
               myIsEncounter = true;
               state = EnumR204.TC_EVENT_ROLL_REFERENCE;
            }
            return state;
         }
         //-------------------------------------------------------
         switch (mim.NewTerritory.Type)
         {
            case "Farmland":
               if (7 < dieRoll)
               {
                  myIsEncounter = true;
                  state = EnumR204.TC_EVENT_ROLL_REFERENCE;
               }
               break;
            case "Countryside":
               if (8 < dieRoll)
               {
                  myIsEncounter = true;
                  state = EnumR204.TC_EVENT_ROLL_REFERENCE;
               }
               break;
            case "Forest":
               if (8 < dieRoll)
               {
                  myIsEncounter = true;
                  state = EnumR204.TC_EVENT_ROLL_REFERENCE;
               }
               break;
            case "Hills":
               if (9 < dieRoll)
               {
                  myIsEncounter = true;
                  state = EnumR204.TC_EVENT_ROLL_REFERENCE;
               }
               break;
            case "Mountains":
               if (8 < dieRoll)
               {
                  myIsEncounter = true;
                  state = EnumR204.TC_EVENT_ROLL_REFERENCE;
               }
               break;
            case "Swamp":
               if (9 < dieRoll)
               {
                  myIsEncounter = true;
                  state = EnumR204.TC_EVENT_ROLL_REFERENCE;
               }
               break;
            case "Desert":
               if (9 < dieRoll)
               {
                  myIsEncounter = true;
                  state = EnumR204.TC_EVENT_ROLL_REFERENCE;
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "GetEventResult(): reached default t=" + mim.NewTerritory.Type);
               state = EnumR204.TC_ERROR;
               break;
         }
         return state;
      }
      private Button CreateButton(IMapItem mi)
      {
         System.Windows.Controls.Button b = new Button { };
         b.Name = Utilities.RemoveSpaces(mi.Name);
         b.Width = Utilities.ZOOM * Utilities.theMapItemSize;
         b.Height = Utilities.ZOOM * Utilities.theMapItemSize;
         b.BorderThickness = new Thickness(0);
         b.Background = new SolidColorBrush(Colors.Transparent);
         b.Foreground = new SolidColorBrush(Colors.Transparent);
         b.IsEnabled = true;
         MapItem.SetButtonContent(b, mi, false, true); // This sets the image as the button's content
         return b;
      }
      protected bool IsStructureWithinRange(IGameInstance gi, int range)
      {
         Logger.Log(LogEnum.LE_HEX_WITHIN_RANGE, "=> startT=" + gi.Prince.Territory.Name);
         ITerritory startT = gi.Prince.Territory;
         List<string> masterList = new List<string>();
         Queue<string> tStack = new Queue<string>();
         Queue<int> depthStack = new Queue<int>();
         Dictionary<string, bool> visited = new Dictionary<string, bool>();
         tStack.Enqueue(startT.Name);
         depthStack.Enqueue(0);
         visited[startT.Name] = false;
         masterList.Add(startT.Name);
         StringBuilder stringBuilder0 = new StringBuilder();
         stringBuilder0.Append("initialList=[");
         while (0 < tStack.Count)
         {
            String nameCurrent = tStack.Dequeue();
            int depth = depthStack.Dequeue();
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < depth; ++i)
               stringBuilder.Append("\t");
            if (true == visited[nameCurrent])
               continue;
            if (range <= depth)
               continue;
            visited[nameCurrent] = true;
            ITerritory t = Territory.theTerritories.Find(nameCurrent);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsStructureWithinRange(): t=null for " + nameCurrent);
               return false;
            }
            Logger.Log(LogEnum.LE_HEX_WITHIN_RANGE, stringBuilder.ToString() + "==>> t=" + nameCurrent);
            stringBuilder.Append("\t");
            foreach (string adjName in t.Adjacents)
            {
               ITerritory adjacent = Territory.theTerritories.Find(adjName);
               if (null == adjacent)
               {
                  Logger.Log(LogEnum.LE_ERROR, "IsStructureWithinRange(): adjacent=null for " + adjName + " for t=" + nameCurrent); 
                  return false;
               }
               Logger.Log(LogEnum.LE_HEX_WITHIN_RANGE, stringBuilder.ToString() + "-->> a=" + adjName);
               tStack.Enqueue(adjacent.Name);
               depthStack.Enqueue(depth + 1);
               if (false == masterList.Contains(adjName))
               {
                  stringBuilder0.Append(adjName);
                  masterList.Add(adjName);
                  visited[adjName] = false;
               }
            }
         }
         stringBuilder0.Append("]");
         Logger.Log(LogEnum.LE_HEX_WITHIN_RANGE, stringBuilder0.ToString());
         //----------------------------------------------------------------
         StringBuilder stringBuilder1 = new StringBuilder();
         stringBuilder1.Append("masterList=[");
         int count = 0;
         foreach (String name in masterList)
         {
            stringBuilder1.Append(name);
            ITerritory t = Territory.theTerritories.Find(name);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsStructureWithinRange(): t=null in masterlist for t=" + name);
               return false;
            }
            if (true == myGameInstance.IsInStructure(t))
               return true;
            if( ++count != masterList.Count) // print comma only if not last entry
               stringBuilder1.Append(",");
         }
         stringBuilder1.Append("]");
         Logger.Log(LogEnum.LE_HEX_WITHIN_RANGE, stringBuilder1.ToString());
         return false;
      }
      //-----------------------------------------------------------------------------------------
      private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
      {
         if( (EnumR204.TC_EVENT_ROLL_EVENT_R232_SHOW == myState) || (EnumR204.TC_EVENT_ROLL_EVENT_R281_SHOW == myState) )
         {
            myState = EnumR204.TC_END; // If there is an active event, encounter that event in UpdateGrid()
            if (false == UpdateGrid())
               Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
            return;
         }
         else if ((EnumR204.TC_LOST_SHOW_RESULTS == myState) || (EnumR204.TC_LOST_SHOW_CROSS_RESULT == myState) || (EnumR204.TC_EVENT_SHOW_RESULTS == myState))
         {
            if ((true == myIsRaftEncounter) && (8 == myRollReference))
            {
               myTextBlockCol1.Text = "1";
               myTextBlockCol2.Text = "2";
               myTextBlockCol3.Text = "3";
               myTextBlockCol4.Text = "4";
               myTextBlockCol5.Text = "5";
               myTextBlockCol6.Text = "6";
               myState = EnumR204.TC_EVENT_ROLL_EVENT_R232;
            }
            else 
            {
               myState = EnumR204.TC_END; // If there is an active event, encounter that event in UpdateGrid()
            }
            if (false == UpdateGrid())
               Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
            return;
         }
         else if (EnumR204.TC_DESERTION_SHOW == myState)
         {
            myTextBlockCol0.Text = "Row Description";
            myTextBlockCol0.HorizontalAlignment = HorizontalAlignment.Left;
            myTextBlockCol1.Text = "1";
            myTextBlockCol2.Visibility = Visibility.Visible;
            myTextBlockCol3.Text = "3";
            myTextBlockCol4.Visibility = Visibility.Visible;
            myTextBlockCol5.Visibility = Visibility.Visible;
            myTextBlockCol6.Text = "6";
            if (RiverCrossEnum.TC_CROSS_FAIL == myMapItemMove.RiverCross)
               myState = EnumR204.TC_END;
            else
               myState = EnumR204.TC_EVENT_ROLL;
            if (false == UpdateGrid())
               Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
            return;
         }
         //--------------------------------------------------------
         System.Windows.Point p = e.GetPosition((UIElement)sender);
         HitTestResult result = VisualTreeHelper.HitTest(myGridTravelTable, p);  // Get the Point where the hit test occurrs
         foreach (UIElement ui in myGridTravelTable.Children)
         {
            if (ui is StackPanel panel)
            {
               foreach (UIElement ui1 in panel.Children)
               {
                  if (ui1 is Image imgPanel) // Check all images within the myStackPanelAssignable
                  {
                     if (result.VisualHit == imgPanel)
                     {
                        if (false == myIsRollInProgress) // myStackPanelAssignable image is clicked
                        {
                           int rowNum = Grid.GetRow(imgPanel);
                           myIsRollInProgress = true;
                           RollEndCallback callback = ShowDieResults;
                           switch (myState)
                           {
                              case EnumR204.TC_EVENT_ROLL_REFERENCE:
                              case EnumR204.TC_EVENT_ROLL_EVENT:
                              case EnumR204.TC_EVENT_ROLL_EVENT_R232:
                              case EnumR204.TC_EVENT_ROLL_REFERENCE_R281:
                              case EnumR204.TC_EVENT_ROLL_EVENT_R281:
                                 myDieRoller.RollMovingDie(myCanvas, callback);
                                 break;
                              case EnumR204.TC_EVENT_ROLL:
                                 if ( (true == myIsLost) && (true == myIsTravelingAir) )
                                    myDieRoller.RollMovingDie(myCanvas, callback);
                                 else
                                    myDieRoller.RollMovingDice(myCanvas, callback);
                                 break;
                              case EnumR204.TC_LOST_ROLL:
                              case EnumR204.TC_EVENT_ROLL_ROAD:
                              case EnumR204.TC_EVENT_ROLL_EVENT_R230:
                                 myDieRoller.RollMovingDice(myCanvas, callback);
                                 break;
                              default:
                                 Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): reached default s=" + myState.ToString());
                                 break;
                           }
                           imgPanel.Visibility = Visibility.Hidden;
                           return;
                        }
                     }
                  }
               }
            }
            if (ui is Image imgRow) // next check all images within the Grid Rows
            {
               if (result.VisualHit == imgRow)
               {
                  if (false == myIsRollInProgress)
                  {
                     myRollResultRowNum = Grid.GetRow(imgRow);
                     myIsRollInProgress = true;
                     RollEndCallback callback = ShowDieResults;
                     int dieRoll = myDieRoller.RollMovingDie(myCanvas, callback);
                     imgRow.Visibility = Visibility.Hidden;
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
         if ("t207" == key)
         {
            if (false == myRulesMgr.ShowTable(key))
               Logger.Log(LogEnum.LE_ERROR, "ButtonRule_Click(): myRulesMgr.ShowTable() returned false key" + key);
         }
         else if( true == key.StartsWith("e"))
         {
            StringBuilder newKey = new StringBuilder(key);
            newKey[0] = 'r';
            if (false == myRulesMgr.ShowRule(newKey.ToString()))
               Logger.Log(LogEnum.LE_ERROR, "ButtonRule_Click(): myRulesMgr.ShowRule() returned false key" + newKey);
         }
         else
         {
            if (false == myRulesMgr.ShowRule(key))
               Logger.Log(LogEnum.LE_ERROR, "ButtonRule_Click(): myRulesMgr.ShowRule() returned false key" + key);
         }
      }
      private void CheckBoxPrimary_Click(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         int rowNum = Grid.GetRow(cb);
         cb.IsChecked = true;
         if (rowNum < STARTING_ASSIGNED_ROW) // rowNum might be zero when multiple local guides and only one is checked
            return;
         int i = rowNum - STARTING_ASSIGNED_ROW;
         for (int j = 0; j < myLocalGuides.Count; ++j)
            myGridRows[j].myIsSelected = false;
         myGridRows[i].myIsSelected = true;
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxDouble_Checked(): UpdateGrid() return false");
      }
   }
}
