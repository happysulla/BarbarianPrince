using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfAnimatedGif;
using Button = System.Windows.Controls.Button;
using Cursor = System.Windows.Input.Cursor;
using Cursors = System.Windows.Input.Cursors;
using Label = System.Windows.Controls.Label;
using Point = System.Windows.Point;

namespace BarbarianPrince
{
   public partial class EventViewerCombatMgr : System.Windows.Controls.UserControl
   {
      public delegate bool EndRoundCallback(bool isRoute, bool isEscape);
      private const int STARTING_ASSIGNED_ROW = 8;
      private const int NO_EFFECT_THIS_ATTACK = -1;
      private const int SPECTRE_REQUIRES_MAGIC = -2;
      private const int DO_NOT_OWN_TALISMAN = -1;
      private const int TALISMAN_NOT_USED_BY_THIS_GUY = -1;
      private const int WIZARD_ESCAPE = -10;
      private const bool IS_ENABLE = true;
      private const bool NO_ENABLE = false;
      private const bool IS_STATS = true;
      private const bool NO_STATS = false;
      private const bool IS_ADORN = true;
      private const bool NO_ADORN = false;
      private const bool IS_CURSOR = true;
      private const bool NO_CURSOR = false;
      public enum StrikeEnum
      {
         BOTH,  // Strikes in both directions
         LEFT,  // Defender only strike
         RIGHT, // Attacker only strike
         MIRROR, // Choose if attack occurs
         NONE   // Nothing is displayed
      };
      public enum BattleEnum
      {
         ERROR,
         R300, // Suprise
         R301, // Suprise if roll less than or equals W&W - otherwise strike first
         R302, // Suprise if roll less than W&W - otherwise strike first
         R303, // Suprise if party size less than roll - otherwise strike first
         R304, // Attack
         R305, // Attack if roll less than or equals W&W  - otherwise encountered strike first
         R306, // Attack if roll less than W&W - otherwise encountered strike first
         R307, // Attacked
         R308, // Suprised if roll exceeds W&W - otherwise encountered strike first
         R309, // Suprised if roll equals or exceeds W&W - otherwise encountered strike first
         R310, // Suprised
         RCTR  // Counter Attack
      };
      public enum CombatEnum
      {
         NONE,                  // Initialization Condition,
         ACTIVATE_ITEMS,        // Choose to Use Poison Drug     
         APPLY_POISON,          // Show Poison Drug
         APPLY_SHIELD,          // Show Shield
         APPLY_RING,            // Show die for ring
         APPLY_RING_SHOW,       // Show the results for the ring
         APPLY_NERVE_GAS,       // Show Nerve Gas Bomb
         APPLY_NERVE_GAS_SHOW,  // Show last die roll
         APPLY_NERVE_GAS_NEXT,  // Move to the next state - either attack or attacked
                                //-----------------------------------------------------
         END_POISON,            // After Combat - check for poison disappearing
         END_TALISMAN,          // After Combat - check for talisman disappearing
         END_SHIELD,            // After Combat - check for sheild disappearing
         END_POISON_SHOW,       // Show last die roll
         END_SHIELD_SHOW,       // Show last die roll
         END_TALISMAN_SHOW,     // Show last die roll
                                //-----------------------------------------------------
         WIZARD_STRIKE,         // e023 - wizard strike
         WIZARD_STRIKE_SHOW,    
                                //-----------------------------------------------------
         MIRROR_STRIKE,         // e047 - follower may srike you instead of mirror
         MIRROR_STRIKE_SHOW,    // show the roll
                                //-----------------------------------------------------
         KNIGHT_STRIKE,         // e123b - roll to see who strikes first
         KNIGHT_STRIKE_SHOW,    // show the roll
                                //-----------------------------------------------------
         CAVALRY_STRIKE,        // e153 - cavalry always win
                                //-----------------------------------------------------
         ROLL_FOR_PROTECTOR,       // Only applies for "e012b" - protector may arrive at end of round
         ROLL_FOR_PROTECTOR_SHOW,  // Only applies for "e012b" - protector may arrive at end of round
                                   //-----------------------------------------------------
         ROLL_FOR_BATTLE_STATE,    // Determine which BattleEnum to start with
         FINALIZE_BATTLE_STATE,    // Account for initial roll on Surprise, Surprised, Attack, or Attacked 
         ASSIGN,                   // Assign the assignable MapItems to grid rows
         ASSIGN_AFTER_ESCAPE,      // Assign the assignable MapItems to grid rows after failing an escape attempt
         ASSIGN_STRIKES,           // All MapItems assigned - ready to start rolling
         STARTED_STRIKES,          // Attackers strike defenders
         SHOW_LAST_STRIKE,         // Show die results
         SWITCH_ATTACK,            // Switch attackers and defenders
         STARTED_COUNTER,          // Defenders strike attackers
         SHOW_LAST_COUNTER,        // Show die results
         ROLL_FOR_HALFLING,        // e008 - Check if Halfling is routed
         ROLL_FOR_HALFLING_SHOW,   // e008 - Check if Halfling is routed
         WOLF_REMOVES_MOUNT,       // e075b - Remove a mount
         NEXT_ROUND,               // Click to start next round
         ROUTE,                    // Encountered members flee
         ESCAPE                    // Party members flee
      };
      public struct GridRow
      {
         public IMapItem myUnassignable;
         public IMapItem myAssignable;
         public int myAssignmentCount;
         public StrikeEnum myDirection;
         public int myModifier;
         public int myResult;
         public int myWoundsPending;
         public int myPoisonPending;
         public int myDamage;
         public int myDamageFireball; // only appies for e023
         public MirrorTargetEnum myAttackMirrorState;  // only applies for e047
         public GridRow(IMapItem mi)
         {
            myAssignable = null;
            myUnassignable = mi;
            myDirection = StrikeEnum.BOTH;
            myModifier = 0;
            myResult = Utilities.NO_RESULT;
            myDamage = Utilities.NO_RESULT;
            myDamageFireball = Utilities.NO_RESULT;
            myWoundsPending = 0;
            myPoisonPending = 0;
            myAssignmentCount = 0;
            myAttackMirrorState = MirrorTargetEnum.NONE;
         }
      }
      public enum MirrorTargetEnum
      {
         NONE,
         PRINCE_STRIKE,
         MIRROR_STRIKE,
         ROLL_FOR_TARGET
      };
      private enum DragStateEnum
      {
         KEEPER_DRUG,
         SHARER_DRUG,
         KEEPER_SHIELD,
         SHARER_SHIELD,
         NONE
      };
      //---------------------------------------------
      public bool CtorError { get; } = false;
      private IGameInstance myGameInstance = null;
      //---------------------------------------------
      private DragStateEnum myDragState = DragStateEnum.NONE;
      private int myDragStateRowNum = 0;
      private int myDragStateColNum = 0;
      private bool myIsTalismanShown = false;
      private bool myIsTalismanActivated = false;
      private bool myIsNerveGasBombShown = false;
      private IMapItem myNerveGasOwner = null; //e007 - Elf can be owner of nerve gas
      private bool myIsHalflingFight = false; // e008 - halfling roll if disappears
      private bool myIsDrugShown = false;
      private bool myIsDrugResultsStarted = false;
      private bool myIsDrugResultsEnded = false;
      private bool myIsShieldShown = false;
      private bool myIsShieldResultsStarted = false;
      private bool myIsShieldResultsEnded = false;
      private bool myIsProtectorArriving = false; // e012b - protector coming
      private IMapItem myEncounteredWizard = null;  // e023 - Fighting Wizard
      private bool myIsWizardFight = false; // e023 - Fighting Wizard
      private bool myIsWizardEscape = false; // e023 - If fireball used when wizard is damaged, he escapes
      private int myFireballDamage = 0;
      private int myWizardFireballRoundNum = 0;
      private bool myIsMirrorFight = false; // e047 - Fighting a mirror of the prince
      private bool myIsMinstelFight = false; // e049 - get in fight with mistrel
      private bool myIsSpiderFight = false; // e074
      private bool myIsWolvesFight = false; // e075
      private IMapItem myCatVictim = null;  // e076
      private bool myIsBoarFight = false;   // e083
      private bool myIsBearFight = false;   // e085
      private bool myIsKnightOnBridge = false; // e123b
      private bool myIsSurprise = false; // e300, e301, e302, e303
      private bool myIsSurprised = false; // e007 - elf may be owner of nerve gas
      //---------------------------------------------
      private CombatEnum myState = CombatEnum.ASSIGN;
      private CombatEnum myPreviousCombatState = CombatEnum.NONE;
      private IMapItem myMapItemDragged = null;
      private IMapItems myAssignables = null;
      private IMapItems myUnassignables = null;
      private IMapItems myNonCombatants = new MapItems(); // e014b - hired reavers do not fight
      private IMapItems myEncounteredSlaveGirls = new MapItems(); // slave girls can be traded for negotiations - get returned if enter combat and win battle
      private bool myIsPartyMembersAssignable = false;
      //---------------------------------------------
      private bool myIsRouteOfEnemyPossible = false;
      private bool myIsEscapePossible = true; // can the party escape from this encounter
      private bool myIsEscape = false;
      private bool myIsRoute = false;
      private EndRoundCallback myCallback = null;
      //---------------------------------------------
      private int myColumnAssignable = 0;
      private GridRow[] myGridRows = new GridRow[Utilities.MAX_GRID_ROW];
      private int myMaxRowCount = 0;
      private int myRoundNum = 1;
      //---------------------------------------------
      private IDieRoller myDieRoller = null;
      private int myRollResult = Utilities.NO_RESULT;
      private int myRollResultHalfling = Utilities.NO_RESULT;  
      private int myRollResultRing = Utilities.NO_RESULT;
      private int myRollResultMirror = Utilities.NO_RESULT;
      private int myRollResultKnight = Utilities.NO_RESULT;
      private int myRollResultProtector = Utilities.NO_RESULT;
      private int myRollResultRowNum = 0;
      private bool myIsRollInProgress = false;
      //---------------------------------------------
      private int myWitAndWiles = 4;
      private BattleEnum myBattleEnum;
      private BattleEnum myBattleEnumInitial;
      //---------------------------------------------
      private int myDeadPartyMemberCoin = 0;
      private List<int> myCapturedWealthCodes = new List<int>();
      private IMapItems myCapturedMounts = new MapItems();
      private List<SpecialEnum> myCapturedPossessions = new List<SpecialEnum>();
      //---------------------------------------------
      private readonly Canvas myCanvas = null;
      private readonly ScrollViewer myScrollViewer = null;
      private readonly RuleDialogViewer myRulesMgr = null;
      private readonly StackPanel myStackPanelPrinceEndurance = null;
      //---------------------------------------------
      private readonly Dictionary<string, Cursor> myCursors = new Dictionary<string, Cursor>();
      private readonly DoubleCollection myDashArray = new DoubleCollection();
      private readonly SolidColorBrush mySolidColorBrushBlack = new SolidColorBrush() { Color = Colors.Black };
      private readonly FontFamily myFontFam = new FontFamily("Tahoma");
      private readonly FontFamily myFontFam1 = new FontFamily("Courier New");
      //-----------------------------------------------------------------------------------------
      public EventViewerCombatMgr(IGameInstance gi, Canvas c, ScrollViewer sv, RuleDialogViewer rdv, IDieRoller dr, StackPanel sp)
      {
         InitializeComponent();
         //--------------------------------------------------
         if (null == gi) // check parameter inputs
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerCombatMgr(): gi=null");
            CtorError = true;
            return;
         }
         myGameInstance = gi;
         //--------------------------------------------------
         if (null == c)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerCombatMgr(): c=null");
            CtorError = true;
            return;
         }
         myCanvas = c;
         //--------------------------------------------------
         if (null == sv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerCombatMgr(): sv=null");
            CtorError = true;
            return;
         }
         myScrollViewer = sv;
         //--------------------------------------------------
         if (null == rdv)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerCombatMgr(): rdv=null");
            CtorError = true;
            return;
         }
         myRulesMgr = rdv;
         //--------------------------------------------------
         if (null == dr)
         {
            Logger.Log(LogEnum.LE_ERROR, "EventViewerCombatMgr(): cfm=true");
            CtorError = true;
            return;
         }
         myDieRoller = dr;
         //--------------------------------------------------
         if (null == sp)
         {
            Logger.Log(LogEnum.LE_ERROR, "MapItemCombatViewer(): sp=null");
            CtorError = true;
            return;
         }
         myStackPanelPrinceEndurance = sp;
         //--------------------------------------------------
         myDashArray.Add(4);  // used for dotted lines
         myDashArray.Add(2);  // used for dotted lines
         myGrid.MouseDown += Grid_MouseDown;
         myStackPanelCheckMarks.MouseDown += Header_MouseDown;
      }
      public bool PerformCombat(EndRoundCallback callback)
      {
         myCallback = callback;
         myWitAndWiles = myGameInstance.WitAndWile;
         if (true == myGameInstance.IsElfWitAndWileActive)  // e007
            --myWitAndWiles;
         myIsEscapePossible = true;
         myIsRouteOfEnemyPossible = false;  // PerformCombat()
         myIsEscape = false;
         myIsRoute = false;
         myRoundNum = 1;
         myDeadPartyMemberCoin = 0;
         myCapturedWealthCodes.Clear();
         myCapturedMounts.Clear();
         myCapturedPossessions.Clear();
         myDragState = DragStateEnum.NONE;
         myDragStateRowNum = 0;
         myDragStateColNum = 0;
         myIsNerveGasBombShown = false;
         myNerveGasOwner = null;
         myIsSurprise = false;
         myIsSurprised = false;
         myIsHalflingFight = false;
         myIsTalismanShown = false;
         myIsTalismanActivated = false;
         myIsDrugShown = false;
         myIsDrugResultsStarted = false;
         myIsDrugResultsEnded = false;
         myIsShieldShown = false;
         myIsShieldResultsStarted = false;
         myIsShieldResultsEnded = false;
         myPreviousCombatState = CombatEnum.NEXT_ROUND;
         myRollResult = Utilities.NO_RESULT;
         myRollResultRing = Utilities.NO_RESULT;
         myRollResultMirror = Utilities.NO_RESULT;
         myRollResultKnight = Utilities.NO_RESULT;
         myRollResultProtector = Utilities.NO_RESULT;
         myIsWizardFight = false;
         myIsWizardEscape = false;
         myFireballDamage = 0;
         myEncounteredWizard = null;
         myWizardFireballRoundNum = 0;
         myIsMirrorFight = false;
         myIsBoarFight = false;
         myIsSpiderFight = false;
         myIsWolvesFight = false;
         myIsMinstelFight = false;
         myCatVictim = null;
         myIsProtectorArriving = false;
         myIsBearFight = false;
         myIsKnightOnBridge = false;
         myCursors.Clear();
         myNonCombatants.Clear();
         myEncounteredSlaveGirls.Clear();
         //-------------------------------------------------
         if (null == myGameInstance.PartyMembers)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformCombat(): partyMembers=null");
            return false;
         }
         if (0 == myGameInstance.PartyMembers.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformCombat(): PartyMembers.Count=0");
            return false;
         }
         if (null == myGameInstance.EncounteredMembers)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformCombat(): encounterMembers=null");
            return false;
         }
         if (0 == myGameInstance.EncounteredMembers.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformCombat(): EncounteredMembers.Count=0");
            return false;
         }
         //--------------------------------------------------
         System.Windows.Point hotPoint = new System.Windows.Point(Utilities.theMapItemOffset, Utilities.theMapItemOffset); // set the center of the MapItem as the hot point for the cursor
         Image img1 = new Image { Source = MapItem.theMapImages.GetBitmapImage("PoisonDrug"), Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
         myCursors["PoisonDrug"] = Utilities.ConvertToCursor(img1, hotPoint);
         Image img2 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Shield"), Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
         myCursors["Shield"] = Utilities.ConvertToCursor(img2, hotPoint);
         //-------------------------------------------------
         switch (myGameInstance.EventStart)
         {
            case "e008": myIsHalflingFight = true; break;
            case "e012b": myIsProtectorArriving = true; break;
            case "e023": myIsWizardFight = true; break;
            case "e024c": myIsTalismanActivated = true; break; // Wizard Attack thwarted by resistance talisman
            case "e047": myIsMirrorFight = true; break;
            case "e074":
               myIsSpiderFight = true;
               foreach (IMapItem mi in myGameInstance.PartyMembers)   // spiders reduce combat by one due to webs
                  --mi.Combat;
               break;
            case "e075b":
               bool isMountExist = false;
               foreach (IMapItem mi in myGameInstance.PartyMembers)
               {
                  if (0 < mi.Mounts.Count)
                     isMountExist = true;
               }
               myIsWolvesFight = isMountExist;
               myIsEscapePossible = false;
               break;
            case "e076":
               int randomNum = Utilities.RandomGenerator.Next(myGameInstance.PartyMembers.Count);
               myCatVictim = myGameInstance.PartyMembers[randomNum];
               if ("Prince" == myCatVictim.Name) // must fight if victim is Prince
                  myIsEscapePossible = false;
               Button b0 = CreateButton(myGameInstance.EncounteredMembers[0], IS_ENABLE, false, NO_STATS, NO_ADORN, IS_CURSOR);
               myCursors[myGameInstance.EncounteredMembers[0].Name] = Utilities.ConvertToCursor(b0, hotPoint);
               break;
            case "e083":
               myIsBoarFight = true;
               Button b1 = CreateButton(myGameInstance.EncounteredMembers[0], IS_ENABLE, false, NO_STATS, NO_ADORN, IS_CURSOR);
               myCursors[myGameInstance.EncounteredMembers[0].Name] = Utilities.ConvertToCursor(b1, hotPoint);
               break;
            case "e084b":
               myIsBearFight = true;
               Button b2 = CreateButton(myGameInstance.EncounteredMembers[0], IS_ENABLE, false, NO_STATS, NO_ADORN, IS_CURSOR);
               myCursors[myGameInstance.EncounteredMembers[0].Name] = Utilities.ConvertToCursor(b2, hotPoint);
               break;
            case "e046":
            case "e054b":
            case "e094":
            case "e098b":
            case "e108":
               myIsEscapePossible = false;
               break;
            case "e123b":
               myIsEscapePossible = false;
               myIsKnightOnBridge = true;
               break;
            default: break; // do nothing
         }
         //--------------------------------------------------
         foreach (IMapItem mi in myGameInstance.PartyMembers)
         {
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "PerformCombat(): mi=null");
               return false;
            }
            mi.IsShieldApplied = false;
            mi.IsPoisonApplied = false;
            if (("e014a" == myGameInstance.EventStart) && (true == mi.Name.Contains("Reaver"))) // if in e014b, hired reavers do not fight for the party
               myNonCombatants.Add(mi);
            if ((true == mi.Name.Contains("Slave")) && (false == myIsBoarFight) && (null == myCatVictim) && (false == myIsBearFight)) // slaves do not fight in combat
               myNonCombatants.Add(mi);
            else if ((true == mi.Name.Contains("Porter")) && (false == myIsBoarFight) && (null == myCatVictim) && (false == myIsBearFight)) // porters  do not fight in combat
               myNonCombatants.Add(mi);
            else if (((true == mi.Name.Contains("TrueLove")) && (0 == mi.Combat)) && (false == myIsBoarFight) && (null == myCatVictim) && (false == myIsBearFight)) // true Love do not fight in combat
               myNonCombatants.Add(mi);
            else if ((true == mi.Name.Contains("Minstrel")) && (false == myIsBoarFight) && (null == myCatVictim) && (false == myIsBearFight)) // minstrel do not fight in combat
               myNonCombatants.Add(mi);
            else if ((true == mi.Name.Contains("Falcon")) && (false == myIsBoarFight) && (null == myCatVictim) && (false == myIsBearFight)) // minstrel do not fight in combat
               myNonCombatants.Add(mi);
            else if ( (true == myGameInstance.IsAssassination) && (false == mi.Name.Contains("Prince")) ) // only the Prince fights in assassination attempt
               myNonCombatants.Add(mi);
            else if ((true == myIsKnightOnBridge) && (false == mi.Name.Contains("Prince"))) // only the Prince fights in knight on bridge fight
               myNonCombatants.Add(mi);
         }
         foreach (IMapItem mi in myNonCombatants)  // removed any noncombatants party members 
            myGameInstance.PartyMembers.Remove(mi);
         //-------------------------------------------------
         foreach (IMapItem mi in myGameInstance.EncounteredMembers)
         {
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "PerformCombat(): mi=null in myGameInstance.EncounteredMembers");
               return false;
            }
            if (true == mi.Name.Contains("Wizard"))
               myEncounteredWizard = mi;
            if (true == mi.Name.Contains("Slave"))
               myEncounteredSlaveGirls.Add(mi);
            if (true == mi.Name.Contains("Minstrel"))
               myIsMinstelFight = true;
         }
         foreach (IMapItem mi in myEncounteredSlaveGirls)  // removed any traded slave girls
            myGameInstance.EncounteredMembers.Remove(mi);
         if( (true == myIsWizardFight) && (null == myEncounteredWizard) )
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformCombat(): myEncounteredWizard=null in myGameInstance.EncounteredMembers");
            return false;
         }
         //-------------------------------------------------
         foreach (IMapItem mi in myGameInstance.EncounteredMembers)
         {
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "PerformCombat(): mi=null");
               return false;
            }
            if (true == mi.IsSpecialItemHeld(SpecialEnum.NerveGasBomb))
               myNerveGasOwner = mi;
            mi.IsShieldApplied = false;
            mi.IsPoisonApplied = false;
         }
         //-------------------------------------------------
         if (false == ResetGridForCombat(myGameInstance.PartyMembers, myGameInstance.EncounteredMembers))  // PerformCombat()
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformCombat(): ResetGridForCombat() return false");
            return false;
         }
         //--------------------------------------------------
         switch (myGameInstance.EventActive)
         {
            case "e300":
               myBattleEnum = BattleEnum.R300;
               myIsSurprise = true;
               if (false == SetInitialFightState("PerformCombat(e300)"))
               {
                  Logger.Log(LogEnum.LE_ERROR, "PerformCombat(): SetInitialFightState() return false for BattleEnum.R300");
                  return false;
               }
               break;
            case "e301":
               myBattleEnum = BattleEnum.R301;
               Logger.Log(LogEnum.LE_COMBAT_STATE, "PerformCombat(): " + myState.ToString() + "-->ROLL_FOR_BATTLE_STATE  ae=" + myGameInstance.EventActive);
               myState = CombatEnum.ROLL_FOR_BATTLE_STATE;
               break;
            case "e302":
               myBattleEnum = BattleEnum.R302;
               Logger.Log(LogEnum.LE_COMBAT_STATE, "PerformCombat(): " + myState.ToString() + "-->ROLL_FOR_BATTLE_STATE  ae=" + myGameInstance.EventActive);
               myState = CombatEnum.ROLL_FOR_BATTLE_STATE;
               break;
            case "e303":
               myBattleEnum = BattleEnum.R303;
               Logger.Log(LogEnum.LE_COMBAT_STATE, "PerformCombat(): " + myState.ToString() + "-->ROLL_FOR_BATTLE_STATE  ae=" + myGameInstance.EventActive);
               myState = CombatEnum.ROLL_FOR_BATTLE_STATE;
               break;
            case "e304":
               myBattleEnum = BattleEnum.R304;
               if (false == SetInitialFightState("PerformCombat(e304)"))
               {
                  Logger.Log(LogEnum.LE_COMBAT_STATE, "PerformCombat(): " + myState.ToString() + "-->ROLL_FOR_BATTLE_STATE  ae=" + myGameInstance.EventActive);
                  return false;
               }
               break;
            case "e305":
               myBattleEnum = BattleEnum.R305;
               Logger.Log(LogEnum.LE_COMBAT_STATE, "PerformCombat(): " + myState.ToString() + "-->ROLL_FOR_BATTLE_STATE  ae=" + myGameInstance.EventActive);
               myState = CombatEnum.ROLL_FOR_BATTLE_STATE;
               break;
            case "e306":
               myBattleEnum = BattleEnum.R306;
               Logger.Log(LogEnum.LE_COMBAT_STATE, "PerformCombat(): " + myState.ToString() + "-->ROLL_FOR_BATTLE_STATE  ae=" + myGameInstance.EventActive);
               myState = CombatEnum.ROLL_FOR_BATTLE_STATE;
               break;
            case "e307":
               myBattleEnum = BattleEnum.R307;
               if (false == SetInitialFightState("PerformCombat(v)"))
               {
                  Logger.Log(LogEnum.LE_ERROR, "PerformCombat(): SetInitialFightState() return false for BattleEnum.R307");
                  return false;
               }
               break;
            case "e308":
               myBattleEnum = BattleEnum.R308;
               Logger.Log(LogEnum.LE_COMBAT_STATE, "PerformCombat(): " + myState.ToString() + "-->ROLL_FOR_BATTLE_STATE  ae=" + myGameInstance.EventActive);
               myState = CombatEnum.ROLL_FOR_BATTLE_STATE;
               break;
            case "e309":
               myBattleEnum = BattleEnum.R309;
               Logger.Log(LogEnum.LE_COMBAT_STATE, "PerformCombat(): " + myState.ToString() + "-->ROLL_FOR_BATTLE_STATE  ae=" + myGameInstance.EventActive);
               myState = CombatEnum.ROLL_FOR_BATTLE_STATE;
               break;
            case "e310":
               myBattleEnum = BattleEnum.R310;
               myIsSurprised = true;
               if (null != myNerveGasOwner)  // e007 - elf can have nerve gas bomb
               {
                  Logger.Log(LogEnum.LE_COMBAT_STATE, "PerformCombat(): " + myState.ToString() + "-->APPLY_NERVE_GAS");
                  myState = CombatEnum.APPLY_NERVE_GAS;
                  if (false == ResetGridForNerveGas(myGameInstance.PartyMembers))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "PerformCombat(): ResetGridForNerveGas(PartyMembers)=false myState=" + myState.ToString());
                     return false;
                  }
               }
               else if (false == SetInitialFightState("PerformCombat(e310)"))
               {
                  Logger.Log(LogEnum.LE_ERROR, "PerformCombat(): SetInitialFightState() return false for BattleEnum.R310");
                  return false;
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "PerformCombat(): reached default setting myBattleEnum=null b/c ae=" + myGameInstance.EventActive);
               return false;
         }
         myBattleEnumInitial = myBattleEnum;
         //--------------------------------------------------
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "PerformCombat(): UpdateGrid() return false");
            return false;
         }
         //--------------------------------------------------
         myTextBlockHeader.Text = "ROUND #1 COMBAT";
         myScrollViewer.Content = myGrid;
         return true;
      }
      //-----------------------------------------------------------------------------------------
      private bool UpdateGrid()
      {
         Logger.Log(LogEnum.LE_COMBAT_STATE, "UpdateGrid():==============================================================s=" + myState.ToString());
         switch (myState)
         {
            case CombatEnum.ROLL_FOR_BATTLE_STATE:
            case CombatEnum.ACTIVATE_ITEMS:
            case CombatEnum.APPLY_POISON:
            case CombatEnum.APPLY_SHIELD:
            case CombatEnum.APPLY_RING:
            case CombatEnum.APPLY_RING_SHOW:
            case CombatEnum.WIZARD_STRIKE:
            case CombatEnum.WIZARD_STRIKE_SHOW:
            case CombatEnum.MIRROR_STRIKE:
            case CombatEnum.MIRROR_STRIKE_SHOW:
            case CombatEnum.KNIGHT_STRIKE:
            case CombatEnum.KNIGHT_STRIKE_SHOW:
            case CombatEnum.CAVALRY_STRIKE:
            case CombatEnum.APPLY_NERVE_GAS:
            case CombatEnum.APPLY_NERVE_GAS_SHOW:
            case CombatEnum.APPLY_NERVE_GAS_NEXT:
            case CombatEnum.END_POISON:
            case CombatEnum.END_SHIELD:
            case CombatEnum.END_TALISMAN:
            case CombatEnum.FINALIZE_BATTLE_STATE:
               if (false == UpdateHeader()) // Changes to new state
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateHeader() returned false");
                  return false;
               }
               if (false == UpdateGridRows())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateGridRows() returned false");
                  return false;
               }
               if (false == UpdateAssignablePanel())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateAssignablePanel() returned false");
                  return false;
               }
               if (false == UpdateUserInstructions())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateUserInstructions() returned false");
                  return false;
               }
               break;
            case CombatEnum.ASSIGN:
            case CombatEnum.ASSIGN_AFTER_ESCAPE:
               if (false == UpdateGridRows())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateGridRows() returned false");
                  return false;
               }
               if (false == UpdateAssignablePanel())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateAssignablePanel() returned false");
                  return false;
               }
               if (false == UpdateUserInstructions())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateUserInstructions() returned false");
                  return false;
               }
               break;
            case CombatEnum.ASSIGN_STRIKES:
               if (false == UpdateGridRows())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateGridRows() returned false");
                  return false;
               }
               if (false == UpdateAssignablePanel())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateAssignablePanel() returned false");
                  return false;
               }
               if (false == UpdateUserInstructions())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateUserInstructions() returned false");
                  return false;
               }
               break;
            case CombatEnum.STARTED_STRIKES:
               myIsRouteOfEnemyPossible = false; // UpdateGrid() - STARTED_STRIKES
               Logger.Log(LogEnum.LE_COMBAT_STATE_ROUTE, "UpdateGrid(): s=" + myState.ToString() + " route?=" + myIsRouteOfEnemyPossible.ToString());
               if (false == UpdateGridRows())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateGridRows() returned false");
                  return false;
               }
               if (false == UpdateAssignablePanel())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateAssignablePanel() returned false");
                  return false;
               }
               if (false == UpdateUserInstructions())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateUserInstructions() returned false");
                  return false;
               }
               break;
            case CombatEnum.SHOW_LAST_STRIKE:
               if (false == UpdateGridRows())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateGridRows() returned false");
                  return false;
               }
               if (false == UpdateAssignablePanel())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateAssignablePanel() returned false");
                  return false;
               }
               if (false == UpdateUserInstructions())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateUserInstructions() returned false");
                  return false;
               }
               break;
            case CombatEnum.SWITCH_ATTACK:
               bool isEnd2 = false;
               if (false == myIsProtectorArriving) // combat can only end if a protector is not coming
               {
                  if (false == UpdateCombatEnd(ref isEnd2))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateCombatEnd() returned false");
                     return false;
                  }
                  if ((true == isEnd2) && (CombatEnum.END_POISON != myState) && (CombatEnum.END_SHIELD != myState) && (CombatEnum.END_TALISMAN != myState))
                     return true;
               }
               else
               {
                  if (true == IsAllDefendersDead())  // can protector save the day?
                  {
                     Logger.Log(LogEnum.LE_COMBAT_STATE, "UpdateGrid(): " + myState.ToString() + "-->ROLL_FOR_PROTECTOR");
                     myState = CombatEnum.ROLL_FOR_PROTECTOR;
                  }
               }
               //--------------------------------
               if (false == UpdateHeader()) // This can change the state to ASSIGN_STRIKES
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateHeader() returned false");
                  return false;
               }
               if (false == UpdateGridRows())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateGridRows() returned false");
                  return false;
               }
               if (false == UpdateAssignablePanel())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateAssignablePanel() returned false");
                  return false;
               }
               if (false == UpdateUserInstructions())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateUserInstructions() returned false");
                  return false;
               }
               break;
            case CombatEnum.STARTED_COUNTER:
               myIsRouteOfEnemyPossible = false; // UpdateGrid() - STARTED_COUNTER
               Logger.Log(LogEnum.LE_COMBAT_STATE_ROUTE, "UpdateGrid(): s=" + myState.ToString() + " route?=" + myIsRouteOfEnemyPossible.ToString());
               if (false == UpdateGridRows())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateGridRows() returned false");
                  return false;
               }
               if (false == UpdateAssignablePanel())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateAssignablePanel() returned false");
                  return false;
               }
               if (false == UpdateUserInstructions())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateUserInstructions() returned false");
                  return false;
               }
               break;
            case CombatEnum.SHOW_LAST_COUNTER:
               if (false == UpdateGridRows())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateGridRows() returned false");
                  return false;
               }
               if (false == UpdateUserInstructions())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateUserInstructions() returned false");
                  return false;
               }
               break;
            case CombatEnum.ROLL_FOR_HALFLING:
            case CombatEnum.ROLL_FOR_HALFLING_SHOW:
            case CombatEnum.WOLF_REMOVES_MOUNT:
               if (false == UpdateHeader()) // This can change the state to ASSIGN_STRIKES
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateHeader() returned false");
                  return false;
               }
               if (false == UpdateUserInstructions())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateAssignablePanel() returned false");
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
               break;
            case CombatEnum.NEXT_ROUND:
               //-----------------------------------------------
               if (true == myIsProtectorArriving)
               {
                  Logger.Log(LogEnum.LE_COMBAT_STATE, "UpdateGrid(): " + myState.ToString() + "-->ROLL_FOR_PROTECTOR");
                  myState = CombatEnum.ROLL_FOR_PROTECTOR;
                  if (false == UpdateHeader()) // Changes to new state
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateHeader() returned false");
                     return false;
                  }
               }
               else
               {
                  int fightCountBefore1 = myGameInstance.PartyMembers.Count + myGameInstance.EncounteredMembers.Count;
                  if (false == RemoveCasualties()) // UpdateGrid() - NEXT_ROUND
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): RemoveCasualties() returned false");
                     return false;
                  }
                  int fightCountAfter1 = myGameInstance.PartyMembers.Count + myGameInstance.EncounteredMembers.Count;
                  if (false == UpdateHeader()) // Changes to new state
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateHeader() returned false");
                     return false;
                  }
                  bool isEnd11 = false;
                  if (false == UpdateCombatEnd(ref isEnd11)) // combat can only end if a protector is not coming
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateCombatEnd() returned false");
                     return false;
                  }
                  if ((true == isEnd11) && ((CombatEnum.END_POISON != myState) && (CombatEnum.END_SHIELD != myState) && (CombatEnum.END_TALISMAN != myState)))
                     return true;
                  if ( (false== isEnd11) && (fightCountBefore1 != fightCountAfter1) )
                  {
                     if (false == ResetGridForCombat(myGameInstance.PartyMembers, myGameInstance.EncounteredMembers)) // UpdateGrid() - NEXT_ROUND
                     {
                        Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): ResetGridForCombat() returned false for CombatEnum.NEXT_ROUND");
                        return false;
                     }
                  }
               }
               //-----------------------------------------------
               if (false == UpdateGridRows())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateGridRows() returned false");
                  return false;
               }
               if (false == UpdateAssignablePanel())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateAssignablePanel() returned false");
                  return false;
               }
               if (false == UpdateUserInstructions())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateUserInstructions() returned false");
                  return false;
               }
               break;
            case CombatEnum.ROLL_FOR_PROTECTOR:
               if (false == UpdateHeader()) // Changes to new state
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateHeader() returned false");
                  return false;
               }
               if (false == UpdateAssignablePanel())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateAssignablePanel() returned false");
                  return false;
               }
               if (false == UpdateUserInstructions())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateUserInstructions() returned false");
                  return false;
               }
               break;
            case CombatEnum.ROLL_FOR_PROTECTOR_SHOW:
               if (false == RemoveCasualties()) // UpdateGrid() - ROLL_FOR_PROTECTOR_SHOW
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): RemoveCasualties() returned false");
                  return false;
               }
               if (false == UpdateHeader()) // Changes to new state
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateHeader() returned false");
                  return false;
               }
               //-----------------------------------------------
               if (true == IsAllDefendersDead())
               {
                  bool isEnd1 = false;
                  if (false == UpdateCombatEnd(ref isEnd1))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateCombatEnd() returned false");
                     return false;
                  }
                  if ((true == isEnd1) && ((CombatEnum.END_POISON != myState) && (CombatEnum.END_SHIELD != myState) && (CombatEnum.END_TALISMAN != myState)))
                     return true;
               }
               //-----------------------------------------------
               if (false == ResetGridForCombat(myGameInstance.PartyMembers, myGameInstance.EncounteredMembers)) // // UpdateGrid() - ROLL_FOR_PROTECTOR_SHOW
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): ResetGridForCombat() returned false for CombatEnum.ROLL_FOR_PROTECTOR_SHOW");
                  return false;
               }
               if (false == UpdateGridRows())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateGridRows() returned false");
                  return false;
               }
               if (false == UpdateAssignablePanel())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateAssignablePanel() returned false");
                  return false;
               }
               if (false == UpdateUserInstructions())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateUserInstructions() returned false");
                  return false;
               }
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): reached default for myState=" + myState.ToString());
               return false;
         }
         return true;
      }
      private bool ResetGridForCombat(IMapItems partyMembers, IMapItems encounteredMembers)
      {
         System.Windows.Point hotPoint = new System.Windows.Point(Utilities.theMapItemOffset, Utilities.theMapItemOffset); // set the center of the MapItem as the hot point for the cursor
         //------------------------------------
         if (true == myGameInstance.IsCavalryEscort)
         {
            IMapItem cavalry = null;
            foreach (IMapItem mi in partyMembers)
            {
               if (true == mi.Name.Contains("Cavalry"))
                  cavalry = mi;
            }
            if (null == cavalry)
            {
               Logger.Log(LogEnum.LE_ERROR, "ResetGridForCombat(): cavalry=null");
               myGameInstance.IsCavalryEscort = false;
            }
            else
            {
               myIsPartyMembersAssignable = true;
               myAssignables = partyMembers;
               myUnassignables = encounteredMembers;
               myColumnAssignable = 0;
               myMaxRowCount = encounteredMembers.Count;
               for (int i = 0; i < myMaxRowCount; ++i)
               {
                  IMapItem encountered = encounteredMembers[i];
                  encountered.SetWounds(encountered.Endurance, 0); // kill the encountered member by a cavalry unit
                  myGridRows[i] = new GridRow(encountered);
                  myGridRows[i].myAssignable = new MapItem(cavalry);
               }
               Button b = CreateButton(cavalry, IS_ENABLE, false, NO_STATS, NO_ADORN, IS_CURSOR);
               myCursors[cavalry.Name] = Utilities.ConvertToCursor(b, hotPoint);
               return true;
            }
         }
         //------------------------------------
         if (true == myIsMirrorFight)
         {
            myIsPartyMembersAssignable = false;
            myAssignables = encounteredMembers;
            myUnassignables = partyMembers;
            myColumnAssignable = 0;
            myGridRows[0] = new GridRow(myGameInstance.Prince);
            myGridRows[0].myAssignable = encounteredMembers[0];
            myGridRows[0].myAttackMirrorState = MirrorTargetEnum.MIRROR_STRIKE;
            myMaxRowCount = partyMembers.Count;
            for (int i = 0; i < myMaxRowCount; ++i)
            {
               if ("Prince" == partyMembers[i].Name)  // already assigned to first position
                  continue;
               myGridRows[i] = new GridRow(partyMembers[i]);        // Prince is unassignable
               myGridRows[i].myAttackMirrorState = MirrorTargetEnum.NONE;
               myGridRows[i].myAssignable = encounteredMembers[0];
            }
            return true;
         }
         //------------------------------------
         if ( (true == myIsBoarFight) || (true == myIsBearFight) ) // wild boar or bear attack
         {
            if (1 != encounteredMembers.Count)
            {
               Logger.Log(LogEnum.LE_ERROR, "ResetGridForCombat(): encounteredMembers.Count=" + encounteredMembers.Count.ToString());
               return false;
            }
            myIsPartyMembersAssignable = false;
            myAssignables = encounteredMembers;
            myUnassignables = partyMembers;
            myColumnAssignable = 0;
            myMaxRowCount = partyMembers.Count;
            IMapItem enemy = encounteredMembers[0];
            for (int i = 0; i < myMaxRowCount; ++i)
               myGridRows[i] = new GridRow(partyMembers[i]);
            int randomNum = Utilities.RandomGenerator.Next(myMaxRowCount);
            myGridRows[randomNum].myAssignable = enemy;
            myGridRows[randomNum].myAssignmentCount = GetAssignedCount(enemy.Name);
            for (int i = 0; i < myMaxRowCount; ++i)
            {
               if (null == myGridRows[i].myAssignable)
               {
                  myGridRows[i].myAssignable = enemy;
                  myGridRows[i].myAssignmentCount = GetAssignedCount(enemy.Name);
               }
            }
            return true;
         }
         //------------------------------------
         if (null != myCatVictim) // hunting cat
         {
            if (1 < myRoundNum) // only assign in first round until victim is dead or cat is dead
               return true;
            if (1 != encounteredMembers.Count)
            {
               Logger.Log(LogEnum.LE_ERROR, "ResetGridForCombat(): myCatVictim -> encounteredMembers.Count=" + encounteredMembers.Count.ToString());
               return false;
            }
            myIsPartyMembersAssignable = false;
            myAssignables = encounteredMembers;
            myUnassignables = partyMembers;
            myColumnAssignable = 0;
            myMaxRowCount = partyMembers.Count;
            IMapItem huntingCat = encounteredMembers[0];
            for (int i = 0; i < myMaxRowCount; ++i)
               myGridRows[i] = new GridRow(partyMembers[i]);
            for (int i = 0; i < myMaxRowCount; ++i) // need to assign all other party members
            {
               myGridRows[i].myAssignable = huntingCat;
               if (myGridRows[i].myUnassignable.Name == myCatVictim.Name)
                  myGridRows[i].myAssignmentCount = 1;
               else
                  myGridRows[i].myAssignmentCount = GetAssignedCount(huntingCat.Name) + 1;
            }
            return true;
         }
         //------------------------------------
         if (true == myGameInstance.IsAssassination)
         {
            myIsPartyMembersAssignable = true;
            myMaxRowCount = encounteredMembers.Count;
            for (int i = 0; i < myMaxRowCount; ++i)
               myGridRows[i] = new GridRow(encounteredMembers[i]);
            myAssignables = partyMembers;
            myUnassignables = encounteredMembers;
            Button b = CreateButton(myGameInstance.Prince, IS_ENABLE, false, NO_STATS, NO_ADORN, IS_CURSOR);
            myCursors[myGameInstance.Prince.Name] = Utilities.ConvertToCursor(b, hotPoint);
            if (1 == encounteredMembers.Count) // if only one member from each side, sort circuit assignment
            {
               myGridRows[0].myAssignable = myGameInstance.Prince;
               myGridRows[0].myAssignmentCount = GetAssignedCount(myGameInstance.Prince.Name);
            }
            myColumnAssignable = 2;
            return true;
         }
         //------------------------------------
         if (true == myIsKnightOnBridge)
         {
            if (1 != encounteredMembers.Count)
            {
               Logger.Log(LogEnum.LE_ERROR, "ResetGridForCombat(): myIsKnightOnBridge -> encounteredMembers.Count=" + encounteredMembers.Count.ToString());
               return false;
            }
            if (1 != partyMembers.Count)
            {
               Logger.Log(LogEnum.LE_ERROR, "ResetGridForCombat(): myIsKnightOnBridge -> encounteredMembers.Count=" + partyMembers.Count.ToString());
               return false;
            }
            myMaxRowCount = 1;
            myColumnAssignable = 2;
            if (3 < myRollResultKnight)
            {
               myIsPartyMembersAssignable = true;
               myAssignables = partyMembers;
               myUnassignables = encounteredMembers;
               myGridRows[0] = new GridRow(encounteredMembers[0]);
               myGridRows[0].myAssignable = myGameInstance.Prince;
               myGridRows[0].myAssignmentCount = GetAssignedCount(myGameInstance.Prince.Name);
               myBattleEnumInitial = myBattleEnum = BattleEnum.R307;
            }
            else
            {
               myIsPartyMembersAssignable = false;
               myAssignables = encounteredMembers;
               myUnassignables = partyMembers;
               myGridRows[0] = new GridRow(partyMembers[0]);
               myGridRows[0].myAssignable = encounteredMembers[0];
               myGridRows[0].myAssignmentCount = GetAssignedCount(encounteredMembers[0].Name);
               myBattleEnumInitial = myBattleEnum = BattleEnum.R304;
            }
            return true;
         }
         //--------------------------------------------------
         bool isSwitchingAssignableColumn = false;
         //--------------------------------------------------
         StringBuilder sb = new StringBuilder();
         sb.Append(" ResetGridForCombat(): s="); sb.Append(myState.ToString());
         sb.Append(" e.c="); sb.Append(encounteredMembers.Count.ToString());
         sb.Append(" p.c="); sb.Append(partyMembers.Count.ToString());
         if (encounteredMembers.Count < partyMembers.Count) // encountered members are assignable
         {
            if (true == myIsPartyMembersAssignable)
               isSwitchingAssignableColumn = true;
            sb.Append(" a?="); sb.Append(myIsPartyMembersAssignable.ToString());
            myIsPartyMembersAssignable = false;
            sb.Append("=>"); sb.Append(myIsPartyMembersAssignable.ToString());
            myMaxRowCount = partyMembers.Count;
            for (int i = 0; i < myMaxRowCount; ++i)
            {
               myGridRows[i] = new GridRow(partyMembers[i]);
               sb.Append(" r="); sb.Append(i.ToString());
               sb.Append(" mi="); sb.Append(partyMembers[i].Name);
            }
            myAssignables = encounteredMembers;
            myUnassignables = partyMembers;
            foreach (IMapItem mi in encounteredMembers)  // create the cursors for the encountered member buttons
            {
               Button b = CreateButton(mi, IS_ENABLE, false, NO_STATS, NO_ADORN, IS_CURSOR);
               myCursors[mi.Name] = Utilities.ConvertToCursor(b, hotPoint);
            }
            if ((1 == encounteredMembers.Count) && (1 == partyMembers.Count)) // if only one member from each side, sort circuit assignment
            {
               myGridRows[0].myAssignable = encounteredMembers[0];
               myGridRows[0].myAssignmentCount = GetAssignedCount(encounteredMembers[0].Name);
            }
         }
         else  // party members are assignable
         {
            if (false == myIsPartyMembersAssignable)
               isSwitchingAssignableColumn = true;
            sb.Append(" a?="); sb.Append(myIsPartyMembersAssignable.ToString());
            myIsPartyMembersAssignable = true;
            sb.Append("=>"); sb.Append(myIsPartyMembersAssignable.ToString());
            myMaxRowCount = encounteredMembers.Count;
            for (int i = 0; i < myMaxRowCount; ++i)
            {
               myGridRows[i] = new GridRow(encounteredMembers[i]);
               sb.Append(" r="); sb.Append(i.ToString());
               sb.Append(" mi="); sb.Append(encounteredMembers[i].Name);
            }
            myAssignables = partyMembers;
            myUnassignables = encounteredMembers;
            foreach (IMapItem mi in partyMembers) // create the cursors for the party member buttons
            {
               Button b = CreateButton(mi, IS_ENABLE, false, NO_STATS, NO_ADORN, IS_CURSOR);
               myCursors[mi.Name] = Utilities.ConvertToCursor(b, hotPoint);
            }
            if ((1 == encounteredMembers.Count) && (1 == partyMembers.Count)) // if only one member from each side, sort circuit assignment
            {
               myGridRows[0].myAssignable = partyMembers[0];
               myGridRows[0].myAssignmentCount = GetAssignedCount(partyMembers[0].Name);
            }
         }
         if (true == isSwitchingAssignableColumn)
         {
            if (0 == myColumnAssignable)
               myColumnAssignable = 2;
            else
               myColumnAssignable = 0;
         }
         Logger.Log(LogEnum.LE_VIEW_RESET_BATTLE_GRID, sb.ToString());
         return true;
      }
      private bool ResetGridForNonCombat(IMapItems mapItems)
      {
         if (null == mapItems)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResetGridForNonCombat(): mapItems=null");
            return false;
         }
         for (int i = 0; i < mapItems.Count; ++i)
            myGridRows[i] = new GridRow(mapItems[i]);
         myMaxRowCount = mapItems.Count;
         return true;
      }
      private bool ResetGridForNerveGas(IMapItems mapItems)
      {
         if (null == mapItems)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResetGridForNerveGas(): mapItems=null");
            return false;
         }
         if (null == myNerveGasOwner)
         {
            Logger.Log(LogEnum.LE_ERROR, "ResetGridForNerveGas(): myNerveGasOwner=null");
            return false;
         }
         myMaxRowCount = mapItems.Count;
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            myGridRows[i] = new GridRow(mapItems[i]);
            myGridRows[i].myAssignable = myNerveGasOwner;
            myGridRows[i].myAssignmentCount = GetAssignedCount(myNerveGasOwner.Name);
         }
         return true;
      }
      private bool UpdateCombatEnd(ref bool isEnd)
      {
         if (null == myCallback)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckForEndOfCombat(): myCallback=null");
            return false;
         }
         //--------------------------------------------------
         isEnd = false;
         bool isAnyPartyMemberAlive = false;
         bool isAnyEncounteredMemberLeft = false;
         foreach (IMapItem mi in myAssignables)
         {
            if (mi == null)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateCombatEnd(): myGridRows[i].myAssignable=null");
               return false;
            }
            if ((true == myIsWizardEscape) && (true == mi.Name.Contains("Wizard"))) // e023 - wizard escapes if damaged and sent fireball - being removed in RemoveCasualties()
               continue;
            if ((false == mi.IsKilled) && (false == mi.IsUnconscious))
            {
               if (true == myIsPartyMembersAssignable)
                  isAnyPartyMemberAlive = true;
               else
                  isAnyEncounteredMemberLeft = true;
            }
            else
            {
               if (false == myIsPartyMembersAssignable)
                  myGameInstance.KilledLocations.Add(myGameInstance.Prince.Territory);
            }
         }
         foreach (IMapItem mi1 in myUnassignables)
         {
            if (mi1 == null)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateCombatEnd(): myGridRows[i].myUnassignable=null");
               return false;
            }
            if ((true == myIsWizardEscape) && (true == mi1.Name.Contains("Wizard"))) // e023 - wizard escapes if damaged and sent fireball - being removed in RemoveCasualties()
               continue;
            if ((false == mi1.IsKilled) && (false == mi1.IsUnconscious))
            {
               if (false == myIsPartyMembersAssignable)
                  isAnyPartyMemberAlive = true;
               else
                  isAnyEncounteredMemberLeft = true;
            }
            else
            {
               if (true == myIsPartyMembersAssignable)
                  myGameInstance.KilledLocations.Add(myGameInstance.Prince.Territory);
            }
         }
         //--------------------------------------------------------
         if  (true == myGameInstance.Prince.IsRunAway) 
         {
            isEnd = true;
            myIsEscape = true;
            foreach (IMapItem mi in myGameInstance.PartyMembers)
               mi.IsRunAway = false;
            if (false == myCallback(myIsRoute, myIsEscape)) // UpdateCombatEnd() - Players lost combat
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateCombatEnd(): lost combat and myCallback() returned false");
               return false;
            }
         }
         else if ((true == myGameInstance.Prince.IsKilled) || (false == isAnyPartyMemberAlive))
         {
            if( (true == myIsKnightOnBridge) && (true == myGameInstance.Prince.IsUnconscious) )
            {
               myGameInstance.Prince.HealWounds(1, 0);
               foreach (IMapItem mi in myNonCombatants) // UpdateCombatEnd() - return noncombatants to party
                  myGameInstance.PartyMembers.Add(mi);
               myIsEscape = true;
               if (false == SetStateIfItemUsed()) // UpdateGrid() performed in caller routine
               {
                  if (false == myCallback(myIsRoute, true)) // UpdateCombatEnd() - Prince lost combat against Black Knight
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCombatEnd(): myCallback() returned false");
                     return false;
                  }
               }
               if (false == ResetGridForNonCombat(myGameInstance.PartyMembers))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCombatEnd(): ResetGridForNonCombat()=false");
                  return false;
               }
            }
            else
            {
               isEnd = true;
               if (false == myCallback(myIsRoute, myIsEscape)) // UpdateCombatEnd() - Players lost combat
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCombatEnd(): lost combat and myCallback() returned false");
                  return false;
               }
            }
         }
         else
         {
            if (null != myCatVictim)
            {
               if ((true == myCatVictim.IsKilled) || (true == myCatVictim.IsUnconscious))
               {
                  if (false == myGameInstance.RemoveVictimInParty(myCatVictim))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCombatEnd(): RemoveVictimInParty() returned false");
                     return false;
                  }
                  isAnyEncounteredMemberLeft = false; // Hunting cat leaves combat taking victim with it
               }
            }
            if (false == isAnyEncounteredMemberLeft) // Won combat - take end of combat actions
            {
               if (true == myIsSpiderFight)  // UpdateCombatEnd()
               {
                  foreach (IMapItem mi in myGameInstance.PartyMembers)   // spiders reduce combat by one due to webs - need to reset
                     ++mi.Combat;
               }
               //-------------------------------
               foreach (IMapItem mi in myNonCombatants) // UpdateCombatEnd() - return non combatants to party 
                  myGameInstance.PartyMembers.Add(mi);
               foreach (IMapItem mi in myEncounteredSlaveGirls) // return slave girls to party - ones given in negotiation
                  myGameInstance.PartyMembers.Add(mi);
               DistributeDeadWealth();
               //-------------------------------
               if (false == SetStateIfItemUsed()) // UpdateGrid() performed in caller routine
               {
                  isEnd = true;
                  if (false == myCallback(myIsRoute, myIsEscape)) // UpdateCombatEnd() - Players won combat
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateCombatEnd(): myCallback() returned false");
                     return false;
                  }
               }
               if (false == ResetGridForNonCombat(myGameInstance.PartyMembers))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCombatEnd(): ResetGridForNonCombat()=false");
                  return false;
               }
            }
         }
         return true;
      }
      private bool UpdateUserInstructions()
      {
         bool isEscapeButtonShown = false; // determine if the escape button should be shown
         bool isEndCombatButtonShown = false; // determine if the escape button should be shown
         if ((true == myIsPartyMembersAssignable) && (0 == myColumnAssignable)) 
         {
            bool isAnyEncounteredMemberLeft = false;
            foreach(IMapItem mi in myUnassignables)
            {
               if (true == mi.IsKilled || true == mi.IsUnconscious)
                  continue;
               isAnyEncounteredMemberLeft = true;
               break;
            }
            if (false == isAnyEncounteredMemberLeft) // only show escape button if there is a active encountered member
            {
               isEscapeButtonShown = false;
               isEndCombatButtonShown = false;
            }
            else
            {
               isEscapeButtonShown = true;
               isEndCombatButtonShown = true;
            }
         }
         else if ((false == myIsPartyMembersAssignable) && (2 == myColumnAssignable))
         {
            bool isAnyEncounteredMemberLeft = false;
            foreach (IMapItem mi in myAssignables) // only show escape button if there is a active encountered member
            {
               if (true == mi.IsKilled || true == mi.IsUnconscious)
                  continue;
               isAnyEncounteredMemberLeft = true;
               break;
            }
            if (false == isAnyEncounteredMemberLeft)
            {
               isEscapeButtonShown = false;
               isEndCombatButtonShown = false;
            }
            else
            {
               isEscapeButtonShown = true;
               isEndCombatButtonShown = true;
            }
         }
         //--------------------------------------
         myTextBlockInstructions.Inlines.Clear();
         switch (myState)
         {
            case CombatEnum.ROLL_FOR_BATTLE_STATE:
               myTextBlockInstructions.Inlines.Add(new Run("Roll die to determine who strikes first."));
               break;
            case CombatEnum.ACTIVATE_ITEMS:
               myTextBlockInstructions.Inlines.Add(new Run("Click"));
               if (true == myIsDrugShown)
                  myTextBlockInstructions.Inlines.Add(new Run(" drug to apply or"));
               if ((true == myIsShieldShown) && (false == myIsShieldResultsStarted))
                  myTextBlockInstructions.Inlines.Add(new Run(" shield or"));
               if ((true == myIsTalismanShown) && (false == myIsTalismanActivated))
                  myTextBlockInstructions.Inlines.Add(new Run(" talisman or"));
               if (true == myIsNerveGasBombShown)
                  myTextBlockInstructions.Inlines.Add(new Run(" gas bomb or"));
               myTextBlockInstructions.Inlines.Add(new Run(" swords to fight."));
               break;
            case CombatEnum.APPLY_POISON:
               myTextBlockInstructions.Inlines.Add(new Run("Drag drug to apply column. Click"));
               if ((true == myIsShieldShown) && (false == myIsShieldResultsStarted))
                  myTextBlockInstructions.Inlines.Add(new Run(" shield or"));
               if ((true == myIsTalismanShown) && (false == myIsTalismanActivated))
                  myTextBlockInstructions.Inlines.Add(new Run(" talisman or"));
               if (true == myIsNerveGasBombShown)
                  myTextBlockInstructions.Inlines.Add(new Run(" gas bomb or"));
               myTextBlockInstructions.Inlines.Add(new Run(" swords to fight."));
               break;
            case CombatEnum.APPLY_SHIELD:
               myTextBlockInstructions.Inlines.Add(new Run("Drag shield to apply column. Click"));
               if ((true == myIsTalismanShown) && (false == myIsTalismanActivated))
                  myTextBlockInstructions.Inlines.Add(new Run(" talisman or"));
               if (true == myIsNerveGasBombShown)
                  myTextBlockInstructions.Inlines.Add(new Run(" gas bomb or"));
               myTextBlockInstructions.Inlines.Add(new Run(" swords to fight."));
               break;
            case CombatEnum.APPLY_NERVE_GAS:
               myTextBlockInstructions.Inlines.Add(new Run("Roll die for effects: 1-4 KIA, 5-Runs Away, 6-No Effect"));
               break;
            case CombatEnum.END_POISON:
               myTextBlockInstructions.Inlines.Add(new Run("Click on die in Results column to check for poison drug removal."));
               break;
            case CombatEnum.END_SHIELD:
               myTextBlockInstructions.Inlines.Add(new Run("Click on die in Results column to check for shield removal."));
               break;
            case CombatEnum.END_TALISMAN:
               myTextBlockInstructions.Inlines.Add(new Run("Click on die in Results column to check for talisman destruction."));
               break;
            case CombatEnum.APPLY_RING:
               myTextBlockInstructions.Inlines.Add(new Run("Roll die to deflect attack."));
               break;
            case CombatEnum.WIZARD_STRIKE:
               if( false == myIsTalismanShown )
                  myTextBlockInstructions.Inlines.Add(new Run("Roll die to launch fireball on 5 or 6."));
               else
                  myTextBlockInstructions.Inlines.Add(new Run("Roll die to launch fireball on 5 or 6 or click talisman to thwart magic."));
               break;
            case CombatEnum.WIZARD_STRIKE_SHOW:
               myTextBlockInstructions.Inlines.Add(new Run("Roll die to set damage on all party members."));
               break;
             case CombatEnum.MIRROR_STRIKE:
               myTextBlockInstructions.Inlines.Add(new Run("Roll die to determine target."));
               break;
            case CombatEnum.MIRROR_STRIKE_SHOW:
               myTextBlockInstructions.Inlines.Add(new Run("Click on die in Results column to strike."));
               break;
            case CombatEnum.KNIGHT_STRIKE:
               myTextBlockInstructions.Inlines.Add(new Run("Roll die to determine first strike."));
               break;
            case CombatEnum.KNIGHT_STRIKE_SHOW:
               myTextBlockInstructions.Inlines.Add(new Run("Click anywhere to continue."));
               break;
            case CombatEnum.CAVALRY_STRIKE:
               myTextBlockInstructions.Inlines.Add(new Run("Click anywhere to continue."));
               break;
            case CombatEnum.APPLY_RING_SHOW:
            case CombatEnum.APPLY_NERVE_GAS_SHOW:
            case CombatEnum.APPLY_NERVE_GAS_NEXT:
            case CombatEnum.END_TALISMAN_SHOW:
            case CombatEnum.END_POISON_SHOW:
            case CombatEnum.END_SHIELD_SHOW:
               myTextBlockInstructions.Inlines.Add(new Run("Click anywhere to continue."));
               break;
            case CombatEnum.FINALIZE_BATTLE_STATE:
               myTextBlockInstructions.Inlines.Add(new Run("Drag drug to apply column. Each must be assigned at least once."));
               break;
            case CombatEnum.ASSIGN:
               Logger.Log(LogEnum.LE_COMBAT_STATE_ROUTE, "UpdateUserInstructions(): s=" + myState.ToString() + " route?=" + myIsRouteOfEnemyPossible.ToString() + " firstCol?=" + isEscapeButtonShown.ToString());
               if (true == myIsRouteOfEnemyPossible)
               {
                  Button buttonRoute = new Button() { Content = " Route ", FontFamily = myFontFam1, FontSize = 12, Height = 16 };
                  buttonRoute.Click += ButtonRoute_Click;
                  myTextBlockInstructions.Inlines.Add(new InlineUIContainer(buttonRoute));
                  myTextBlockInstructions.Inlines.Add(new Run(" or "));
               }
               Logger.Log(LogEnum.LE_COMBAT_STATE_ESCAPE, "UpdateUserInstructions(): s=" + myState.ToString() + " escape?=" + myIsEscapePossible.ToString() + " firstCol?=" + isEscapeButtonShown.ToString());
               if ((true == myIsEscapePossible) && (true == isEscapeButtonShown))
               {
                  Button buttonEscape0 = new Button() { Content = "Escape", FontFamily = myFontFam1, FontSize = 12, Height = 16 };
                  buttonEscape0.Click += ButtonEscape_Click;
                  myTextBlockInstructions.Inlines.Add(new InlineUIContainer(buttonEscape0));
                  myTextBlockInstructions.Inlines.Add(new Run(" or Drag and Drop. Each must be assigned at least once."));
               }
               else if ((true == myIsKnightOnBridge) && (true == isEndCombatButtonShown))
               {
                  Button buttonEnd0 = new Button() { Content = "End Combat", FontFamily = myFontFam1, FontSize = 12, Height = 16 };
                  buttonEnd0.Click += ButtonEndKnightCombat_Click;
                  myTextBlockInstructions.Inlines.Add(new InlineUIContainer(buttonEnd0));
                  myTextBlockInstructions.Inlines.Add(new Run(" or Drag and Drop. Each must be assigned at least once."));
               }
               else
               {
                  myTextBlockInstructions.Inlines.Add(new Run("Drag and Drop. Each must be assigned at least once."));
               }
               break;
            case CombatEnum.ASSIGN_AFTER_ESCAPE:
               myTextBlockInstructions.Inlines.Add(new Run("Drag and Drop. Each must be assigned at least once."));
               break;
            case CombatEnum.ASSIGN_STRIKES:
               Logger.Log(LogEnum.LE_COMBAT_STATE_ROUTE, "UpdateUserInstructions(): s=" + myState.ToString() + " route?=" + myIsRouteOfEnemyPossible.ToString() + " firstCol?=" + isEscapeButtonShown.ToString());
               if (true == myIsRouteOfEnemyPossible)
               {
                  Button buttonRoute = new Button() { Content = " Route ", FontFamily = myFontFam1, FontSize = 12, Height = 16 };
                  buttonRoute.Click += ButtonRoute_Click;
                  myTextBlockInstructions.Inlines.Add(new InlineUIContainer(buttonRoute));
                  myTextBlockInstructions.Inlines.Add(new Run(" or "));
               }
               Logger.Log(LogEnum.LE_COMBAT_STATE_ESCAPE, "UpdateUserInstructions(): s=" + myState.ToString() + " escape?=" + myIsEscapePossible.ToString() + " firstCol?=" + isEscapeButtonShown.ToString());
               if ((true == myIsEscapePossible) && (true == isEscapeButtonShown))
               {
                  Button buttonEscape1 = new Button() { Content = "Escape", FontFamily = myFontFam1, FontSize = 12, Height = 16 };
                  buttonEscape1.Click += ButtonEscape_Click;
                  myTextBlockInstructions.Inlines.Add(new InlineUIContainer(buttonEscape1));
                  myTextBlockInstructions.Inlines.Add(new Run(" or Click on die in Results column to start attacks."));
               }
               else if ((true == myIsKnightOnBridge) && (true == isEndCombatButtonShown))
               {
                  Button buttonEnd = new Button() { Content = "End Combat", FontFamily = myFontFam1, FontSize = 12, Height = 16 };
                  buttonEnd.Click += ButtonEndKnightCombat_Click;
                  myTextBlockInstructions.Inlines.Add(new InlineUIContainer(buttonEnd));
                  myTextBlockInstructions.Inlines.Add(new Run(" or Click on die in Results column to start attacks."));
               }
               else
               {
                  myTextBlockInstructions.Inlines.Add(new Run("Click on die in Results column to start attacks."));
               }
               break;
            case CombatEnum.STARTED_STRIKES:
               myTextBlockInstructions.Inlines.Add(new Run("Click on die in Results column to continue attacks."));
               break;
            case CombatEnum.SHOW_LAST_STRIKE:
               myTextBlockInstructions.Inlines.Add(new Run("Click anywhere to continue attacks."));
               break;
            case CombatEnum.SWITCH_ATTACK:
               Logger.Log(LogEnum.LE_COMBAT_STATE_ROUTE, "UpdateUserInstructions(): s=" + myState.ToString() + " route?=" + myIsRouteOfEnemyPossible.ToString() + " firstCol?=" + isEscapeButtonShown.ToString());
               if (true == myIsRouteOfEnemyPossible)
               {
                  Button buttonRoute = new Button() { Content = " Route ", FontFamily = myFontFam1, FontSize = 12, Height = 16 };
                  buttonRoute.Click += ButtonRoute_Click;
                  myTextBlockInstructions.Inlines.Add(new InlineUIContainer(buttonRoute));
                  myTextBlockInstructions.Inlines.Add(new Run(" or "));
               }
               Logger.Log(LogEnum.LE_COMBAT_STATE_ESCAPE, "UpdateUserInstructions(): s=" + myState.ToString() + " escape?=" + myIsEscapePossible.ToString() + " firstCol?=" + isEscapeButtonShown.ToString());
               if ((true == myIsEscapePossible) && (true == isEscapeButtonShown))
               {
                  Button buttonEscape1 = new Button() { Content = "Escape", FontFamily = myFontFam1, FontSize = 12, Height = 16 };
                  buttonEscape1.Click += ButtonEscape_Click;
                  myTextBlockInstructions.Inlines.Add(new InlineUIContainer(buttonEscape1));
                  myTextBlockInstructions.Inlines.Add(new Run(" or "));
               }
               else if ((true == myIsKnightOnBridge) && (true == isEndCombatButtonShown))
               {
                  Button buttonEnd = new Button() { Content = "End Combat", FontFamily = myFontFam1, FontSize = 12, Height = 16 };
                  buttonEnd.Click += ButtonEndKnightCombat_Click;
                  myTextBlockInstructions.Inlines.Add(new InlineUIContainer(buttonEnd));
                  myTextBlockInstructions.Inlines.Add(new Run(" or "));
               }
               if (true == myIsMirrorFight)
                  myTextBlockInstructions.Inlines.Add(new Run("Click Result column to attack."));
               else if ((BattleEnum.R300 != myBattleEnum) && (BattleEnum.R310 != myBattleEnum))
                  myTextBlockInstructions.Inlines.Add(new Run("Click die to counter strike."));
               else
                  myTextBlockInstructions.Inlines.Add(new Run("Click die to continue strikes."));
               break;
            case CombatEnum.STARTED_COUNTER:
               myTextBlockInstructions.Inlines.Add(new Run("Click on die in Results column to continue attacks."));
               break;
            case CombatEnum.SHOW_LAST_COUNTER:
               myTextBlockInstructions.Inlines.Add(new Run("Click anywhere to continue attacks."));
               break;
            case CombatEnum.ROLL_FOR_PROTECTOR:
               myTextBlockInstructions.Inlines.Add(new Run("Roll to see if farmer protector is coming."));
               break;
            case CombatEnum.ROLL_FOR_PROTECTOR_SHOW:
               myTextBlockInstructions.Inlines.Add(new Run("Click anywhere to continue attacks."));
               break;
            case CombatEnum.ROLL_FOR_HALFLING:
               myTextBlockInstructions.Inlines.Add(new Run("Roll die to determine if halfling disappears into brush."));
               break;
            case CombatEnum.ROLL_FOR_HALFLING_SHOW:
               myTextBlockInstructions.Inlines.Add(new Run("Click anywhere to continue."));
               break;
            case CombatEnum.WOLF_REMOVES_MOUNT:
               myTextBlockInstructions.Inlines.Add(new Run("Wolf kills mount. Click anywhere to continue."));
               break;
            case CombatEnum.NEXT_ROUND: // This state is never hit b/c it changes in UpdateHeader() prior to calling this function
               Logger.Log(LogEnum.LE_ERROR, "UpdateUserInstructions(): invalid state NEXT_ROUND");
               return false;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateUserInstructions(): reached default");
               return false;
         }
         return true;
      }
      private bool UpdateHeader()
      {
         myStackPanelCheckMarks.Children.Clear();
         CheckBox cb = new CheckBox() { FontSize = 12, IsEnabled = false, IsChecked = true, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
         if (CombatEnum.ACTIVATE_ITEMS == myState)
         {
            cb.Content = "Choose to Apply Items";
            myStackPanelCheckMarks.Children.Add(cb);
         }
         else if (CombatEnum.APPLY_POISON == myState)
         {
            cb.Content = "Apply Drug to Weapons";
            myStackPanelCheckMarks.Children.Add(cb);
         }
         else if (CombatEnum.APPLY_SHIELD == myState)
         {
            cb.Content = "Apply Shield";
            myStackPanelCheckMarks.Children.Add(cb);
         }
         else if ((CombatEnum.APPLY_RING_SHOW == myState) || (CombatEnum.APPLY_RING == myState))
         {
            cb.Content = "Apply Ring";
            myStackPanelCheckMarks.Children.Add(cb);
         }
         else if (CombatEnum.MIRROR_STRIKE_SHOW == myState)
         {
            cb.Content = "Counter Strike";
            myStackPanelCheckMarks.Children.Add(cb);
         }
         else if (CombatEnum.WIZARD_STRIKE == myState)
         {
            cb.Content = "Roll for fireball spell";
            myStackPanelCheckMarks.Children.Add(cb);
         }
         else if (CombatEnum.WIZARD_STRIKE_SHOW == myState)
         {
            cb.Content = "Roll for damage";
            myStackPanelCheckMarks.Children.Add(cb);
         }
         else if (CombatEnum.MIRROR_STRIKE == myState)
         {
            cb.Content = "Roll for target";
            myStackPanelCheckMarks.Children.Add(cb);
         }
         else if (CombatEnum.MIRROR_STRIKE_SHOW == myState)
         {
            cb.Content = "Counter Strike";
            myStackPanelCheckMarks.Children.Add(cb);
         }
         else if (CombatEnum.KNIGHT_STRIKE == myState)
         {
            cb.Content = "Roll for 1st strike";
            myStackPanelCheckMarks.Children.Add(cb);
         }
         else if (CombatEnum.KNIGHT_STRIKE_SHOW == myState)
         {
            if (4 < myRollResultKnight)
               cb.Content = "Knight strikes first";
            else
               cb.Content = "Prince strikes first";
            myStackPanelCheckMarks.Children.Add(cb);
         }
         else if (CombatEnum.ROLL_FOR_PROTECTOR == myState)
         {
            cb.Content = "Roll for Protector";
            myStackPanelCheckMarks.Children.Add(cb);
         }
         else if ((CombatEnum.APPLY_NERVE_GAS == myState) || (CombatEnum.APPLY_NERVE_GAS_SHOW == myState))
         {
            cb.Content = "Throw Nerve Gas Bomb at opponents";
            myStackPanelCheckMarks.Children.Add(cb);
         }
         else if ((CombatEnum.END_POISON == myState) || (CombatEnum.END_SHIELD == myState) || (CombatEnum.END_TALISMAN == myState) || (CombatEnum.END_POISON_SHOW == myState) || (CombatEnum.END_SHIELD_SHOW == myState) || (CombatEnum.END_TALISMAN_SHOW == myState))
         {
            cb.Content = "On Six Remove";
            myStackPanelCheckMarks.Children.Add(cb);
         }
         else if (CombatEnum.CAVALRY_STRIKE == myState)
         {
            cb.Content = "Cavalry Strike";
            myStackPanelCheckMarks.Children.Add(cb);
         }
         else if ((CombatEnum.ROLL_FOR_HALFLING == myState) || (CombatEnum.ROLL_FOR_HALFLING_SHOW == myState) )
         {
            cb.Content = "Halfling hides";
            myStackPanelCheckMarks.Children.Add(cb);
         }
         else if (CombatEnum.WOLF_REMOVES_MOUNT == myState)
         {
            cb.Content = "Wolf Kills Mount";
            myStackPanelCheckMarks.Children.Add(cb);
         }
         else if (CombatEnum.ROLL_FOR_BATTLE_STATE == myState)
         {
            BitmapImage bmi = new BitmapImage();
            bmi.BeginInit();
            bmi.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
            bmi.EndInit();
            Image img = new Image { Name = "FirstStrikeDie", Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
            ImageBehavior.SetAnimatedSource(img, bmi);
            switch (myBattleEnum)
            {
               case BattleEnum.R301:  // Suprise if Roll for Equals W&W Or Exceeds - otherwise strike first
                  if (true == myIsPartyMembersAssignable) myColumnAssignable = 2; else myColumnAssignable = 0;
                  cb.Content = "Surprise if roll equals or less than wit & wiles = " + myWitAndWiles.ToString();
                  myStackPanelCheckMarks.Children.Add(cb);
                  myStackPanelCheckMarks.Children.Add(img);
                  break;
               case BattleEnum.R302:  // Suprise if Roll for Exceeds W&W Exceeds - otherwise strike first
                  if (true == myIsPartyMembersAssignable) myColumnAssignable = 0; else myColumnAssignable = 2;
                  cb.Content = "Surprise if roll less than wit & wiles = " + myWitAndWiles.ToString();
                  myStackPanelCheckMarks.Children.Add(cb);
                  myStackPanelCheckMarks.Children.Add(img);
                  break;
               case BattleEnum.R303:  // Suprise if party size  less than roll - otherwise strike first
                  int partySize = (true == myIsPartyMembersAssignable) ? myAssignables.Count : myUnassignables.Count;
                  cb.Content = "Surprise if roll greater than party size = " + partySize.ToString();
                  myStackPanelCheckMarks.Children.Add(cb);
                  myStackPanelCheckMarks.Children.Add(img);
                  break;
               case BattleEnum.R305:  // Attack first if Roll Equals W&W or exceeds - otherwise encountered strike first
                  if (true == myIsPartyMembersAssignable) myColumnAssignable = 0; else myColumnAssignable = 2;
                  cb.Content = "Attack first if roll less than or equal to wit & wiles = " + myWitAndWiles.ToString();
                  myStackPanelCheckMarks.Children.Add(cb);
                  myStackPanelCheckMarks.Children.Add(img);
                  break;
               case BattleEnum.R306:  // Attack first if Roll Exceeds W&W - otherwise encountered strike first
                  if (true == myIsPartyMembersAssignable) myColumnAssignable = 0; else myColumnAssignable = 2;
                  cb.Content = "Attack first if roll less than wit & wiles = " + myWitAndWiles.ToString();
                  myStackPanelCheckMarks.Children.Add(cb);
                  myStackPanelCheckMarks.Children.Add(img);
                  break;
               case BattleEnum.R308:  // Suprised if roll exeeds W&W - otherwise encountered strike first
                  if (true == myIsPartyMembersAssignable) myColumnAssignable = 2; else myColumnAssignable = 0;
                  cb.Content = "Surprised if roll greater than wit & wiles = " + myWitAndWiles.ToString();
                  myStackPanelCheckMarks.Children.Add(cb);
                  myStackPanelCheckMarks.Children.Add(img);
                  break;
               case BattleEnum.R309:  // Suprised if roll equals or exeeds W&W - otherwise encountered strike first
                  if (true == myIsPartyMembersAssignable) myColumnAssignable = 2; else myColumnAssignable = 0;
                  cb.Content = "Surprised if roll greater than or equal to wit & wiles = " + myWitAndWiles.ToString();
                  myStackPanelCheckMarks.Children.Add(cb);
                  myStackPanelCheckMarks.Children.Add(img);
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "UpdateHeader(): reached default for myState=ROLL_FOR_BATTLE_STATE and battleEnum=" + myBattleEnum.ToString());
                  return false;
            }
         }
         else
         {
            BitmapImage bmi = new BitmapImage();
            bmi.BeginInit();
            bmi.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
            bmi.EndInit();
            Image img = new Image { Name = myState.ToString(), Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
            ImageBehavior.SetAnimatedSource(img, bmi);
            BattleEnum previousBe = myBattleEnum;
            switch (myBattleEnum)
            {
               case BattleEnum.R300:  // Suprise - repeat attack
                  if (CombatEnum.FINALIZE_BATTLE_STATE == myState)
                  {
                     if (true == myIsPartyMembersAssignable) myColumnAssignable = 0; else myColumnAssignable = 2;
                     cb.Content = "Surprise";
                  }
                  else
                  {
                     myBattleEnum = BattleEnum.R304;
                     myBattleEnumInitial = BattleEnum.R304;
                     cb.Content = "Attack";
                     int fightCountBefore300 = myGameInstance.PartyMembers.Count + myGameInstance.EncounteredMembers.Count; // remove any surprised dead people
                     if (false == RemoveCasualties()) // UpdateHeader(R300)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "UpdateHeader(): RemoveCasualties() returned false");
                        return false;
                     }
                     int fightCountAfter300 = myGameInstance.PartyMembers.Count + myGameInstance.EncounteredMembers.Count;
                     if (fightCountBefore300 != fightCountAfter300)
                     {
                        if (false == ResetGridForCombat(myGameInstance.PartyMembers, myGameInstance.EncounteredMembers)) // UpdateHeader(R300) 
                        {
                           Logger.Log(LogEnum.LE_ERROR, "UpdateHeader(): ResetGridForCombat() returned false");
                           return false;
                        }
                     }
                  }
                  myStackPanelCheckMarks.Children.Add(cb);
                  Logger.Log(LogEnum.LE_COMBAT_STATE, "UpdateHeader(): " + myState.ToString() + "-->ASSIGN_STRIKES be=" + myBattleEnum.ToString());
                  myState = CombatEnum.ASSIGN_STRIKES;
                  break;
               case BattleEnum.R301:  // Suprise if Roll for Equals W&W Or Exceeds - otherwise strike first
                  if (CombatEnum.FINALIZE_BATTLE_STATE == myState)
                  {
                     if (true == myIsPartyMembersAssignable) myColumnAssignable = 2; else myColumnAssignable = 0;
                     cb.Content = "Surprise if roll equals or less than wit & wiles = " + myWitAndWiles.ToString();
                     myStackPanelCheckMarks.Children.Add(cb);
                     myStackPanelCheckMarks.Children.Add(img);
                  }
                  break;
               case BattleEnum.R302:  // Suprise if Roll for Exceeds W&W Exceeds - otherwise strike first
                  if (CombatEnum.FINALIZE_BATTLE_STATE == myState)
                  {
                     if (true == myIsPartyMembersAssignable) myColumnAssignable = 0; else myColumnAssignable = 2;
                     cb.Content = "Surprise if roll less than wit & wiles = " + myWitAndWiles.ToString();
                     myStackPanelCheckMarks.Children.Add(cb);
                     myStackPanelCheckMarks.Children.Add(img);
                  }
                  break;
               case BattleEnum.R303:  // Suprise if party size  less than roll - otherwise strike first
                  if (CombatEnum.FINALIZE_BATTLE_STATE == myState)
                  {
                     int partySize = (true == myIsPartyMembersAssignable) ? myAssignables.Count : myUnassignables.Count;
                     cb.Content = "Surprise if roll greater than the party size = " + partySize.ToString();
                     myStackPanelCheckMarks.Children.Add(cb);
                     myStackPanelCheckMarks.Children.Add(img);
                  }
                  break;
               case BattleEnum.R304:  // Attack
                  if (CombatEnum.FINALIZE_BATTLE_STATE == myState)
                  {
                     if (true == myIsPartyMembersAssignable) myColumnAssignable = 0; else myColumnAssignable = 2;
                     cb.Content = "Attack";
                     Logger.Log(LogEnum.LE_COMBAT_STATE, "UpdateHeader(): " + myState.ToString() + "-->ASSIGN_STRIKES be=" + myBattleEnum.ToString());
                     myState = CombatEnum.ASSIGN_STRIKES;
                  }
                  else
                  {
                     Logger.Log(LogEnum.LE_COMBAT_STATE, "UpdateHeader(): s=" + myState.ToString() + " " + myBattleEnum.ToString() + "-->RCTR");
                     myBattleEnum = BattleEnum.RCTR;
                     cb.Content = "Counter Strike";
                     if (0 == myColumnAssignable) myColumnAssignable = 2; else myColumnAssignable = 0;
                  }
                  myStackPanelCheckMarks.Children.Add(cb);
                  break;
               case BattleEnum.R305:  // Attack first if Roll Equals W&W or exceeds - otherwise encountered strike first
                  if (CombatEnum.FINALIZE_BATTLE_STATE == myState)
                  {
                     if (true == myIsPartyMembersAssignable) myColumnAssignable = 0; else myColumnAssignable = 2;
                     cb.Content = "Attack first if roll less than or equal to wit & wiles = " + myWitAndWiles.ToString();
                     myStackPanelCheckMarks.Children.Add(cb);
                     myStackPanelCheckMarks.Children.Add(img);
                  }
                  break;
               case BattleEnum.R306:  // Attack first if Roll Exceeds W&W - otherwise encountered strike first
                  if (CombatEnum.FINALIZE_BATTLE_STATE == myState)
                  {
                     if (true == myIsPartyMembersAssignable) myColumnAssignable = 0; else myColumnAssignable = 2;
                     cb.Content = "Attack first if roll less than wit & wiles = " + myWitAndWiles.ToString();
                     myStackPanelCheckMarks.Children.Add(cb);
                     myStackPanelCheckMarks.Children.Add(img);
                  }
                  break;
               case BattleEnum.R307:  // Attacked
                  if (CombatEnum.FINALIZE_BATTLE_STATE == myState)
                  {
                     if (true == myIsPartyMembersAssignable) myColumnAssignable = 2; else myColumnAssignable = 0;
                     cb.Content = "Attacked";
                     Logger.Log(LogEnum.LE_COMBAT_STATE, "UpdateHeader(): " + myState.ToString() + "-->ASSIGN_STRIKES be=" + myBattleEnum.ToString());
                     myState = CombatEnum.ASSIGN_STRIKES;
                  }
                  else
                  {
                     Logger.Log(LogEnum.LE_COMBAT_STATE, "UpdateHeader(): s=" + myState.ToString() + " " + myBattleEnum.ToString() + "-->RCTR");
                     myBattleEnum = BattleEnum.RCTR;
                     cb.Content = "Counter Strike";
                     if (0 == myColumnAssignable) myColumnAssignable = 2; else myColumnAssignable = 0;
                  }
                  myStackPanelCheckMarks.Children.Add(cb);
                  break;
               case BattleEnum.R308:  // Suprised if roll exeeds W&W - otherwise encountered strike first
                  if (CombatEnum.FINALIZE_BATTLE_STATE == myState)
                  {
                     if (true == myIsPartyMembersAssignable) myColumnAssignable = 2; else myColumnAssignable = 0;
                     cb.Content = "Surprised if roll greater than wit & wiles = " + myWitAndWiles.ToString();
                     myStackPanelCheckMarks.Children.Add(cb);
                     myStackPanelCheckMarks.Children.Add(img);
                  }
                  break;
               case BattleEnum.R309:  // Suprised if roll equals or exeeds W&W - otherwise encountered strike first
                  if (CombatEnum.FINALIZE_BATTLE_STATE == myState)
                  {
                     if (true == myIsPartyMembersAssignable) myColumnAssignable = 2; else myColumnAssignable = 0;
                     cb.Content = "Surprised if roll greater than or equal to wit & wiles = " + myWitAndWiles.ToString();
                     myStackPanelCheckMarks.Children.Add(cb);
                     myStackPanelCheckMarks.Children.Add(img);
                  }
                  break;
               case BattleEnum.R310:  // Suprised - repeat attack
                  if (CombatEnum.FINALIZE_BATTLE_STATE == myState)
                  {
                     if (true == myIsPartyMembersAssignable) myColumnAssignable = 2; else myColumnAssignable = 0;
                     cb.Content = "Surprised";
                  }
                  else
                  {
                     if ((1 == myRoundNum) && (true == myIsBoarFight)) // e083 - boar reduces to 5 after surprise attack
                     {
                        IMapItem boar = myGridRows[0].myAssignable;
                        if (5 < boar.Combat)
                           boar.Combat -= 3;
                     }
                     myBattleEnum = BattleEnum.R307;
                     myBattleEnumInitial = BattleEnum.R307;
                     cb.Content = "Attacked";
                     int fightCountBeforeR310 = myGameInstance.PartyMembers.Count + myGameInstance.EncounteredMembers.Count;
                     if (false == RemoveCasualties()) // UpdateHeader(R310) 
                     {
                        Logger.Log(LogEnum.LE_ERROR, "UpdateHeader(): RemoveCasualties() returned false");
                        return false;
                     }
                     int fightCountAfterR310 = myGameInstance.PartyMembers.Count + myGameInstance.EncounteredMembers.Count;
                     if (fightCountBeforeR310 != fightCountAfterR310)
                     {
                        if (false == ResetGridForCombat(myGameInstance.PartyMembers, myGameInstance.EncounteredMembers)) // UpdateHeader(R310) 
                        {
                           Logger.Log(LogEnum.LE_ERROR, "UpdateHeader(): ResetGridForCombat() returned false");
                           return false;
                        }
                     }
                  }
                  myStackPanelCheckMarks.Children.Add(cb);
                  Logger.Log(LogEnum.LE_COMBAT_STATE, "UpdateHeader(): " + myState.ToString() + "-->ASSIGN_STRIKES be=" + myBattleEnum.ToString());
                  myState = CombatEnum.ASSIGN_STRIKES;
                  break;
               case BattleEnum.RCTR:  // Counter
                  ++myRoundNum;
                  myBattleEnum = myBattleEnumInitial;
                  if ("e012b" == myGameInstance.EventStart)
                  {
                     if (true == myIsPartyMembersAssignable) myColumnAssignable = 2; else myColumnAssignable = 0;  // protectors are always first in combat
                  }
                  else
                  {
                     if (0 == myColumnAssignable) myColumnAssignable = 2; else myColumnAssignable = 0;  // switch columns
                  }
                  if( true == myIsKnightOnBridge )
                  {
                     Logger.Log(LogEnum.LE_COMBAT_STATE, "UpdateHeader(): " + myState.ToString() + "-->KNIGHT_STRIKE be=RCTR-->" + myBattleEnum.ToString());
                     myState = CombatEnum.KNIGHT_STRIKE;
                     cb.Content = "Roll for 1st strike";
                  }
                  else
                  {
                     Logger.Log(LogEnum.LE_COMBAT_STATE, "UpdateHeader(): " + myState.ToString() + "-->ASSIGN_STRIKES be=RCTR-->" + myBattleEnum.ToString());
                     myState = CombatEnum.ASSIGN_STRIKES;
                     if (BattleEnum.R304 == myBattleEnumInitial) // change the 1st row to indicate original Battle Enumeration
                     {
                        cb.Content = "Attack";
                     }
                     else if (BattleEnum.R307 == myBattleEnumInitial)
                     {
                        cb.Content = "Attacked";
                     }
                     else
                     {
                        Logger.Log(LogEnum.LE_ERROR, "UpdateHeader(): Reached default  -  InitialBattleEnum=" + myBattleEnumInitial.ToString() + " Not R304 or R307 fore myBattleEnum=RCTR");
                        return false;
                     }
                  }
                  //------------------------------
                  myStackPanelCheckMarks.Children.Add(cb);
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "UpdateHeader(): reached default for battleEnum=" + myBattleEnum.ToString());
                  return false;
            }
         }
         StringBuilder sb = new StringBuilder("ROUND #");
         sb.Append(myRoundNum.ToString());
         sb.Append(" COMBAT");
         myTextBlockHeader.Text = sb.ToString(); // Updated the round number in top header
         return true;
      }
      private bool UpdateAssignablePanel()
      {
         myStackPanelAssignable.Children.Clear(); // clear out assignable panel 
         switch (myState)
         {
            case CombatEnum.ACTIVATE_ITEMS:
               Image img1 = new Image { Name = "CrossedSwords", Source = MapItem.theMapImages.GetBitmapImage("CrossedSwords"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img1);
               if (true == myIsDrugShown)
               {
                  Rectangle r1 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(r1);
                  Image img2 = new Image { Name = "PoisonDrug", Source = MapItem.theMapImages.GetBitmapImage("PoisonDrug"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img2);
               }
               if (true == myIsShieldShown)
               {
                  Rectangle r1 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(r1);
                  Image img2 = new Image { Name = "Shield", Source = MapItem.theMapImages.GetBitmapImage("Shield"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img2);
               }
               if (true == myIsTalismanShown)
               {
                  Rectangle r2 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(r2);
                  if (true == myIsTalismanActivated)
                  {
                     BitmapImage bmi0a = new BitmapImage();
                     bmi0a.BeginInit();
                     bmi0a.UriSource = new Uri("../../Images/lightening.gif", UriKind.Relative);
                     bmi0a.EndInit();
                     Image img0a = new Image { Name = "Lightening", Source = bmi0a, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                     ImageBehavior.SetAnimatedSource(img0a, bmi0a);
                     myStackPanelAssignable.Children.Add(img0a);
                  }
                  else
                  {
                     Image img3 = new Image { Name = "TalismanResistance", Source = MapItem.theMapImages.GetBitmapImage("TalismanResistance"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                     myStackPanelAssignable.Children.Add(img3);
                  }
               }
               if (true == myIsNerveGasBombShown)
               {
                  Rectangle r1 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(r1);
                  Image img2 = new Image { Name = "NerveGasBomb", Source = MapItem.theMapImages.GetBitmapImage("NerveGasBomb"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img2);
               }
               break;
            case CombatEnum.APPLY_POISON:
               Image img4 = new Image { Name = "CrossedSwords", Source = MapItem.theMapImages.GetBitmapImage("CrossedSwords"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img4);
               if (true == myIsShieldShown)
               {
                  Rectangle r1 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(r1);
                  Image img2 = new Image { Name = "Shield", Source = MapItem.theMapImages.GetBitmapImage("Shield"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img2);
               }
               if (true == myIsTalismanShown)
               {
                  Rectangle r2 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(r2);
                  if (true == myIsTalismanActivated)
                  {
                     BitmapImage bmi0b = new BitmapImage();
                     bmi0b.BeginInit();
                     bmi0b.UriSource = new Uri("../../Images/lightening.gif", UriKind.Relative);
                     bmi0b.EndInit();
                     Image img0b = new Image { Name = "Lightening", Source = bmi0b, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                     ImageBehavior.SetAnimatedSource(img0b, bmi0b);
                     myStackPanelAssignable.Children.Add(img0b);
                  }
                  else
                  {
                     Image img3 = new Image { Name = "TalismanResistance", Source = MapItem.theMapImages.GetBitmapImage("TalismanResistance"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                     myStackPanelAssignable.Children.Add(img3);
                  }
               }
               if (true == myIsNerveGasBombShown)
               {
                  Rectangle r1 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(r1);
                  Image img2 = new Image { Name = "NerveGasBomb", Source = MapItem.theMapImages.GetBitmapImage("NerveGasBomb"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img2);
               }
               break;
            case CombatEnum.APPLY_SHIELD:
               Image img4a = new Image { Name = "CrossedSwords", Source = MapItem.theMapImages.GetBitmapImage("CrossedSwords"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img4a);
               if (true == myIsDrugShown)
               {
                  Rectangle r1 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(r1);
                  Image img2 = new Image { Name = "PoisonDrug", Source = MapItem.theMapImages.GetBitmapImage("PoisonDrug"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img2);
               }
               if (true == myIsTalismanShown)
               {
                  Rectangle r2 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(r2);
                  if (true == myIsTalismanActivated)
                  {
                     BitmapImage bmi0c = new BitmapImage();
                     bmi0c.BeginInit();
                     bmi0c.UriSource = new Uri("../../Images/lightening.gif", UriKind.Relative);
                     bmi0c.EndInit();
                     Image img0c = new Image { Name = "Lightening", Source = bmi0c, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                     ImageBehavior.SetAnimatedSource(img0c, bmi0c);
                     myStackPanelAssignable.Children.Add(img0c);
                  }
                  else
                  {
                     Image img3 = new Image { Name = "TalismanResistance", Source = MapItem.theMapImages.GetBitmapImage("TalismanResistance"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                     myStackPanelAssignable.Children.Add(img3);
                  }
               }
               if (true == myIsNerveGasBombShown)
               {
                  Rectangle r1 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(r1);
                  Image img2 = new Image { Name = "NerveGasBomb", Source = MapItem.theMapImages.GetBitmapImage("NerveGasBomb"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img2);
               }
               break;
            case CombatEnum.APPLY_RING:
               BitmapImage bmi0 = new BitmapImage();
               bmi0.BeginInit();
               bmi0.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
               bmi0.EndInit();
               Image img0 = new Image { Name = "RingRoll", Source = bmi0, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               ImageBehavior.SetAnimatedSource(img0, bmi0);
               myStackPanelAssignable.Children.Add(img0);
               break;
            case CombatEnum.WIZARD_STRIKE:
               Image img10112 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Fireball"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img10112);
               Label labelForFireball = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "4 < " };
               myStackPanelAssignable.Children.Add(labelForFireball);
               BitmapImage bmi101 = new BitmapImage();
               bmi101.BeginInit();
               bmi101.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
               bmi101.EndInit();
               Image img101 = new Image { Name = "WizardStrike", Source = bmi101, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               ImageBehavior.SetAnimatedSource(img101, bmi101);
               myStackPanelAssignable.Children.Add(img101);
               if (true == myGameInstance.IsSpecialItemHeld(SpecialEnum.ResistanceTalisman))
               {
                  Rectangle r2 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(r2);
                  Image img3 = new Image { Name = "TalismanResistance", Source = MapItem.theMapImages.GetBitmapImage("TalismanResistance"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img3);
               }
               break;
            case CombatEnum.WIZARD_STRIKE_SHOW:
               BitmapImage bmi100 = new BitmapImage();
               bmi100.BeginInit();
               bmi100.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
               bmi100.EndInit();
               Image img100 = new Image { Name = "WizardStrike", Source = bmi100, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               ImageBehavior.SetAnimatedSource(img100, bmi100);
               myStackPanelAssignable.Children.Add(img100);
               break;
            case CombatEnum.MIRROR_STRIKE:
               BitmapImage bmi10 = new BitmapImage();
               bmi10.BeginInit();
               bmi10.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
               bmi10.EndInit();
               Image img10 = new Image { Name = "MirrorStrike", Source = bmi10, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               ImageBehavior.SetAnimatedSource(img10, bmi10);
               myStackPanelAssignable.Children.Add(img10);
               break;
            case CombatEnum.MIRROR_STRIKE_SHOW:
               if (myRollResultMirror < 0)
               {
                  Rectangle r52 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(r52);
               }
               else
               {
                  Image img21 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Mirror"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img21);
                  Label labelForMirrorResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                  labelForMirrorResult.Content = " = ";
                  labelForMirrorResult.Content += myRollResultMirror.ToString();
                  if (4 < myRollResultMirror)
                     labelForMirrorResult.Content += " > 4 strikes prince";
                  else
                     labelForMirrorResult.Content += " < 5 strikes mirror prince";
                  myStackPanelAssignable.Children.Add(labelForMirrorResult);
                  myRollResultMirror = Utilities.NO_RESULT;
               }
               break;
            case CombatEnum.KNIGHT_STRIKE:
               BitmapImage bmi10111 = new BitmapImage();
               bmi10111.BeginInit();
               bmi10111.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
               bmi10111.EndInit();
               Image img10111 = new Image { Name = "KnightStrike", Source = bmi10111, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               ImageBehavior.SetAnimatedSource(img10111, bmi10111);
               myStackPanelAssignable.Children.Add(img10111);
               break;
            case CombatEnum.KNIGHT_STRIKE_SHOW:
               Label labelForBlackKnightResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
               labelForBlackKnightResult.Content = myRollResultKnight.ToString();
               labelForBlackKnightResult.Content += " =";
               myStackPanelAssignable.Children.Add(labelForBlackKnightResult);
               Image img2112 = null;
               if (myRollResultKnight < 4)
                  img2112 = new Image { Source = MapItem.theMapImages.GetBitmapImage("BlackKnightThumbDeny"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               else
                  img2112 = new Image { Source = MapItem.theMapImages.GetBitmapImage("BlackKnightThumb"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img2112);
               break;
            case CombatEnum.ROLL_FOR_PROTECTOR:
               BitmapImage bmi1011 = new BitmapImage();
               bmi1011.BeginInit();
               bmi1011.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
               bmi1011.EndInit();
               Image img1011 = new Image { Name = "Protector", Source = bmi1011, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               ImageBehavior.SetAnimatedSource(img1011, bmi1011);
               myStackPanelAssignable.Children.Add(img1011);
               break;
            case CombatEnum.ROLL_FOR_PROTECTOR_SHOW:
               Image img211 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Protector"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img211);
               Label labelForProtectorResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
               if (2 < myRollResultProtector)
                  labelForProtectorResult.Content = " roll < 3 --> protector coming";
               else
                  labelForProtectorResult.Content = " roll > 2 --> protector arrives";
               myStackPanelAssignable.Children.Add(labelForProtectorResult);
               break;
            case CombatEnum.ROLL_FOR_HALFLING:
               BitmapImage bmi110 = new BitmapImage();
               bmi110.BeginInit();
               bmi110.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
               bmi110.EndInit();
               Image img110 = new Image { Name = "HalflingRoll", Source = bmi110, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               ImageBehavior.SetAnimatedSource(img110, bmi110);
               myStackPanelAssignable.Children.Add(img110);
               break;
            case CombatEnum.ROLL_FOR_HALFLING_SHOW:
               Image img311 = new Image { Source = MapItem.theMapImages.GetBitmapImage("HalflingWarrior"), Width = Utilities.ZOOM * Utilities.theMapItemOffset, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img311);
               Label labelForHalflingResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
               if (1 < myRollResultHalfling)
                  labelForHalflingResult.Content = " roll > 1 --> halfling escapes";
               else
                  labelForHalflingResult.Content = " roll = 1 --> continue attacks";
               myStackPanelAssignable.Children.Add(labelForHalflingResult);
               break;
            case CombatEnum.WOLF_REMOVES_MOUNT:
               Image img3112 = new Image { Source = MapItem.theMapImages.GetBitmapImage("MountDeny"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(img3112);
               break;
            case CombatEnum.CAVALRY_STRIKE:
               foreach (IMapItem mi in myAssignables) // // Add a button for each assignable that has not reached max
               {
                  if (false == mi.Name.Contains("Cavalry"))
                  {
                     Button b = CreateButton(mi, IS_ENABLE, false, IS_STATS, NO_ADORN, NO_CURSOR);
                     myStackPanelAssignable.Children.Add(b);
                  }
               }
               break;
            case CombatEnum.ASSIGN:
            case CombatEnum.ASSIGN_STRIKES:
            case CombatEnum.ASSIGN_AFTER_ESCAPE:
               if (true == myIsMirrorFight)
               {
                  if (0 < myGameInstance.HydraTeethCount)
                  {
                     Image imgTeeth = new Image { Name = "Teeth", Source = MapItem.theMapImages.GetBitmapImage("Teeth"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                     myStackPanelAssignable.Children.Add(imgTeeth);
                  }
                  Rectangle r52 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(r52);
               }
               else
               {
                  int openRowCount = GetOpenCount();
                  int unassignedCount = GetUnassignedCount();
                  bool isRectNeedToBeShown = false;
                  foreach (IMapItem mi in myAssignables) // // Add a button for each assignable that has not reached max
                  {
                     if ((unassignedCount < openRowCount) || (0 == GetAssignedCount(mi.Name)))
                     {
                        bool isRectangleBorderAdded = false; // If dragging a map item, show rectangle around that MapItem
                        if (null != myMapItemDragged && mi.Name == myMapItemDragged.Name)
                           isRectangleBorderAdded = true;
                        Button b = CreateButton(mi, IS_ENABLE, isRectangleBorderAdded, IS_STATS, NO_ADORN, NO_CURSOR);
                        myStackPanelAssignable.Children.Add(b);
                     }
                     else
                     {
                        isRectNeedToBeShown = true;
                     }
                  }
                  if (true == isRectNeedToBeShown) // Add rectangle if at least one MapItem is assigned
                  {
                     Rectangle r6 = new Rectangle()
                     {
                        Visibility = Visibility.Visible,
                        Stroke = mySolidColorBrushBlack,
                        Fill = Brushes.Transparent,
                        StrokeThickness = 2.0,
                        StrokeDashArray = myDashArray,
                        Width = Utilities.ZOOM * Utilities.theMapItemSize,
                        Height = Utilities.ZOOM * Utilities.theMapItemSize
                     };
                     if ((1 == myAssignables.Count) && (1 == myUnassignables.Count)) // if only one assignable and one unassignable, they are automatically assigned
                        r6.Visibility = Visibility.Hidden;
                     myStackPanelAssignable.Children.Add(r6);
                  }
                  if (0 < myGameInstance.HydraTeethCount)
                  {
                     Image imgTeeth = new Image { Name = "Teeth", Source = MapItem.theMapImages.GetBitmapImage("Teeth"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                     myStackPanelAssignable.Children.Add(imgTeeth);
                  }
               }
               break;
            case CombatEnum.STARTED_STRIKES:
            case CombatEnum.STARTED_COUNTER:
            case CombatEnum.SHOW_LAST_STRIKE:
            case CombatEnum.SHOW_LAST_COUNTER:
            case CombatEnum.APPLY_RING_SHOW:
               if (myRollResultRing < 0)
               {
                  Rectangle r52 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(r52);
               }
               else
               {
                  Image img21 = new Image { Name="Nothing", Source = MapItem.theMapImages.GetBitmapImage("RingResistence"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myStackPanelAssignable.Children.Add(img21);
                  Label labelForRingResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                  labelForRingResult.Content = " = ";
                  labelForRingResult.Content += myRollResultRing.ToString();
                  if (12 == myRollResultRing)
                  {
                     labelForRingResult.Content += " ring melts adding one wound";
                  }
                  else if (8 < myRollResultRing)
                  {
                     int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
                     if (i < 0)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanel(): 0 > i=" + i.ToString());
                        return false;
                     }
                     labelForRingResult.Content += " blow skids causing one less wound";
                  }
                  else
                  {
                     labelForRingResult.Content += " blow warded, ignore wounds";
                  }
                  myStackPanelAssignable.Children.Add(labelForRingResult);
                  myRollResultRing = Utilities.NO_RESULT;
               }
               break;
            case CombatEnum.ROLL_FOR_BATTLE_STATE:
            case CombatEnum.APPLY_NERVE_GAS:
            case CombatEnum.APPLY_NERVE_GAS_SHOW:
            case CombatEnum.APPLY_NERVE_GAS_NEXT:
            case CombatEnum.END_TALISMAN:
            case CombatEnum.END_TALISMAN_SHOW:
            case CombatEnum.END_POISON:
            case CombatEnum.END_POISON_SHOW:
            case CombatEnum.END_SHIELD:
            case CombatEnum.END_SHIELD_SHOW:
            case CombatEnum.SWITCH_ATTACK:
               Rectangle r5 = new Rectangle() { Visibility = Visibility.Hidden, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myStackPanelAssignable.Children.Add(r5);
               break;
            case CombatEnum.NEXT_ROUND: // This state is never hit b/c it changes in UpdateHeader() prior to calling this function
               Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanel(): invalid state NEXT_ROUND");
               return false;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateAssignablePanel(): reached default for myState=" + myState.ToString());
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
         if (CombatEnum.APPLY_POISON == myState)
         {
            if (false == UpdateGridRowsDrug())
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateGridRowsDrug() returned false");
               return false;
            }
         }
         else if (CombatEnum.APPLY_SHIELD == myState)
         {
            if (false == UpdateGridRowsShield())
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateGridRowsShield() returned false");
               return false;
            }
         }
         else if ((CombatEnum.APPLY_NERVE_GAS == myState) || (CombatEnum.APPLY_NERVE_GAS_SHOW == myState) || (CombatEnum.APPLY_NERVE_GAS_NEXT == myState))
         {
            if (false == UpdateGridRowsNerveGasBomb())
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateGridRowsNerveGasBomb() returned false");
               return false;
            }
         }
         else if ((CombatEnum.END_POISON == myState) || (CombatEnum.END_POISON_SHOW == myState))
         {
            if (false == UpdateGridRowsDrugEnd())
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateGridRowsDrugEnd() returned false");
               return false;
            }
         }
         else if ((CombatEnum.END_SHIELD == myState) || (CombatEnum.END_SHIELD_SHOW == myState))
         {
            if (false == UpdateGridRowsShieldEnd())
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateGridRowsShieldEnd() returned false");
               return false;
            }
         }
         else if ((CombatEnum.END_TALISMAN == myState) || (CombatEnum.END_TALISMAN_SHOW == myState))
         {
            if (false == UpdateGridRowsTalismanEnd())
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateGridRowsTalismanEnd() returned false");
               return false;
            }
         }
         else if (CombatEnum.CAVALRY_STRIKE == myState)  // e151
         {
            if (false == UpdateGridRowsForCavalry())
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateGridRowsForMirror() returned false");
               return false;
            }
         }
         else if ( (CombatEnum.ROLL_FOR_HALFLING == myState) || (CombatEnum.ROLL_FOR_HALFLING_SHOW == myState) )  
         {
            // show nothing
         }
         else if (CombatEnum.WOLF_REMOVES_MOUNT == myState)
         {
            // show nothing
         }
         else if ((CombatEnum.KNIGHT_STRIKE == myState) || (CombatEnum.KNIGHT_STRIKE_SHOW == myState))
         {
            // show nothing
         }
         else
         {
            if (true == myIsMirrorFight)  // e047
            {
               if (false == UpdateGridRowsForMirror())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateGridRowsForMirror() returned false");
                  return false;
               }
            }
            else if (false == UpdateGridRowsCombat())
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateGridRowsCombat() returned false");
               return false;
            }
         }
         return true;
      }
      private bool UpdateGridRowsCombat()
      {
         myTextBlockCol0.Visibility = Visibility.Visible;
         myTextBlockCol1.Visibility = Visibility.Visible;
         myTextBlockCol2.Visibility = Visibility.Visible;
         myTextBlockCol3.Visibility = Visibility.Visible;
         myTextBlockCol4.Visibility = Visibility.Visible;
         myTextBlockCol5.Visibility = Visibility.Visible;
         myTextBlockCol0.Text = "Attacker";
         myTextBlockCol1.Text = "Strikes";
         myTextBlockCol2.Text = "Defender";
         myTextBlockCol3.Text = "Modifiers";
         myTextBlockCol4.Text = "Result";
         myTextBlockCol5.Text = "Wounds";
         for (int i = 0; i < myMaxRowCount; ++i) // Add buttons based on what is in myGridRows. 
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            if (null == myGridRows[i].myAssignable) // Add either Rectangle of Button for Assignable column
            {
               myGridRows[i].myAssignmentCount = 0;
               Rectangle r = new Rectangle()
               {
                  Visibility = Visibility.Visible,
                  Stroke = mySolidColorBrushBlack,
                  Fill = Brushes.Transparent,
                  StrokeThickness = 2.0,
                  StrokeDashArray = myDashArray,
                  Width = Utilities.ZOOM * Utilities.theMapItemSize,
                  Height = Utilities.ZOOM * Utilities.theMapItemSize
               };
               myGrid.Children.Add(r);
               Grid.SetRow(r, rowNum);
               Grid.SetColumn(r, myColumnAssignable);
            }
            else
            {
               bool isButtonEnabled = false;
               if ((CombatEnum.ASSIGN == myState) || (CombatEnum.ASSIGN_STRIKES == myState) || (CombatEnum.ASSIGN_AFTER_ESCAPE == myState))
                  isButtonEnabled = true;
               Button b = CreateButton(myGridRows[i].myAssignable, isButtonEnabled, false, IS_STATS, IS_ADORN, NO_CURSOR);
               myGrid.Children.Add(b);
               Grid.SetRow(b, rowNum);
               Grid.SetColumn(b, myColumnAssignable);
            }
            // Add button for unassignable column
            int colUnassignable = (0 == myColumnAssignable) ? 2 : 0;
            Button b2 = CreateButton(myGridRows[i].myUnassignable, NO_ENABLE, false, IS_STATS, IS_ADORN, NO_CURSOR);
            myGrid.Children.Add(b2);
            Grid.SetRow(b2, rowNum);
            Grid.SetColumn(b2, colUnassignable);
         }
         //------------------------------------------------------------
         // Determine if all buttons are assigned. If so, switch states.
         bool isAllMapItemsAssigned = true;
         for (int i = 0; i < myMaxRowCount; ++i)  // reset to remove showing fireball for attacked mapitems
         {
            if (null == myGridRows[i].myAssignable)
               isAllMapItemsAssigned = false;
            else
               myGridRows[i].myAssignable.IsShowFireball = false;
            myGridRows[i].myUnassignable.IsShowFireball = false;
            if( CombatEnum.SWITCH_ATTACK == myState )
               myGridRows[i].myDamageFireball = Utilities.NO_RESULT; // reset fire damage for all defenders
         }
         if (true == isAllMapItemsAssigned)
         {
            if (CombatEnum.ASSIGN == myState) //$$$$$$
            {
               Logger.Log(LogEnum.LE_COMBAT_STATE, "UpdateGridRowsCombat(): isAllDiceResultsShown=t " + myState.ToString() + "-->ASSIGN_STRIKES");
               myState = CombatEnum.ASSIGN_STRIKES;
            }
            else if (CombatEnum.ASSIGN_AFTER_ESCAPE == myState)
            {
               Logger.Log(LogEnum.LE_COMBAT_STATE, "UpdateGridRowsCombat(): isAllDiceResultsShown=t " + myState.ToString() + "-->SWITCH_ATTACK");
               myState = CombatEnum.SWITCH_ATTACK;
            }
            if ((CombatEnum.APPLY_RING_SHOW == myState) && (0 < myFireballDamage) )
            {
               Logger.Log(LogEnum.LE_COMBAT_STATE, "UpdateGridRowsCombat(): 0 < myFireballDamage && myState=APPLY_RING_SHOW --> entering ApplyWizardFireballAttack()");
               if (false == ApplyWizardFireballAttack())
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRowsCombat(): ApplyWizardFireballAttack() return false");
                  return false;
               }
            }
         }
         else
         {
            if (CombatEnum.ASSIGN_STRIKES == myState)
            {
               Logger.Log(LogEnum.LE_COMBAT_STATE, "UpdateGridRowsCombat(): isAllDiceResultsShown=f " + myState.ToString() + "-->ASSIGN");
               myState = CombatEnum.ASSIGN;
            }
            if (CombatEnum.SWITCH_ATTACK == myState)
            {
               Logger.Log(LogEnum.LE_COMBAT_STATE, "UpdateGridRowsCombat(): isAllDiceResultsShown=f " + myState.ToString() + "-->ASSIGN_AFTER_ESCAPE");
               myState = CombatEnum.ASSIGN_AFTER_ESCAPE;
            }
         }
         //------------------------------------------------------------
         // Update strike directions for column 1
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            IMapItem attacker = myGridRows[i].myUnassignable;
            IMapItem defender = myGridRows[i].myAssignable;
            if (0 == myColumnAssignable)
            {
               attacker = myGridRows[i].myAssignable;
               defender = myGridRows[i].myUnassignable;
            }
            if (false == UpdateStrikeDirection(rowNum, attacker, defender))
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateStrikeDirection() returned false");
               return false;
            }
         }
         //------------------------------------------------------------
         // Update modifier
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            IMapItem attacker = myGridRows[i].myUnassignable;
            IMapItem defender = myGridRows[i].myAssignable;
            if (0 == myColumnAssignable)
            {
               attacker = myGridRows[i].myAssignable;
               defender = myGridRows[i].myUnassignable;
            }
            UpdateModifier(rowNum, attacker, defender);
         }
         //------------------------------------------------------------
         // Update Results Column with die image or with results
         bool isAllDiceResultsShown = true;
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            if (false == UpdateResultsColumn(rowNum, ref isAllDiceResultsShown))
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateResultColumn() returned false s=" + myState.ToString() + " rn=" + rowNum.ToString() + " all?=" + isAllDiceResultsShown.ToString());
               return false;
            }
         }
         if (true == isAllDiceResultsShown)
         {
            if (CombatEnum.APPLY_RING_SHOW == myState)
            {
               if (CombatEnum.STARTED_STRIKES == myPreviousCombatState)
               {
                  Logger.Log(LogEnum.LE_COMBAT_STATE, "UpdateGridRowsCombat(): isAllDiceResultsShown=t " + myState.ToString() + "-->SHOW_LAST_STRIKE");
                  myState = CombatEnum.SHOW_LAST_STRIKE;
               }
               else if (CombatEnum.STARTED_COUNTER == myPreviousCombatState)
               {
                  Logger.Log(LogEnum.LE_COMBAT_STATE, "UpdateGridRowsCombat(): isAllDiceResultsShown=t " + myState.ToString() + "-->SHOW_LAST_COUNTER");
                  myState = CombatEnum.SHOW_LAST_COUNTER;
               }
            }
            else
            {
               if (CombatEnum.STARTED_STRIKES == myState)
               {
                  Logger.Log(LogEnum.LE_COMBAT_STATE, "UpdateGridRowsCombat(): isAllDiceResultsShown=t " + myState.ToString() + "-->SHOW_LAST_STRIKE");
                  myState = CombatEnum.SHOW_LAST_STRIKE;
               }
               else if (CombatEnum.STARTED_COUNTER == myState)
               {
                  Logger.Log(LogEnum.LE_COMBAT_STATE, "UpdateGridRowsCombat(): isAllDiceResultsShown=t " + myState.ToString() + "-->SHOW_LAST_COUNTER");
                  myState = CombatEnum.SHOW_LAST_COUNTER;
               }
            }
         }
         else
         {
            if (CombatEnum.APPLY_RING_SHOW == myState)  
            {
               Logger.Log(LogEnum.LE_COMBAT_STATE, "UpdateGridRowsCombat(): @@@ isAllDiceResultsShown=f " + myState.ToString() + "-->" + myPreviousCombatState.ToString());
               myState = myPreviousCombatState;
            }
         }
         //------------------------------------------------------------
         // Update Wound Column
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            if (false == UpdateForWounds(rowNum))
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRows(): UpdateForWounds() returned false");
               return false;
            }
         }
         return true;
      }
      private bool UpdateGridRowsForMirror()
      {
         myTextBlockCol0.Visibility = Visibility.Visible;
         myTextBlockCol1.Visibility = Visibility.Visible;
         myTextBlockCol2.Visibility = Visibility.Visible;
         myTextBlockCol3.Visibility = Visibility.Visible;
         myTextBlockCol4.Visibility = Visibility.Visible;
         myTextBlockCol5.Visibility = Visibility.Visible;
         myTextBlockCol0.Text = "Attacker";
         myTextBlockCol1.Text = "Strikes";
         myTextBlockCol2.Text = "Defender";
         myTextBlockCol3.Text = "Modifiers";
         myTextBlockCol4.Text = "Result";
         myTextBlockCol5.Text = "Wounds";
         //------------------------------------------------------------
         // Update Results Column with die image or with results
         bool isAllDiceResultsShown = true;
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            if (0 == myColumnAssignable)
            {
               if (0 == i)
               {
                  if (null == myGridRows[i].myAssignable)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateGridRowsForMirror(): invalid myGridRows[0].myAssignable=null");
                     return false;
                  }
                  IMapItem mirror = myGridRows[i].myAssignable;
                  Button b1 = CreateButton(mirror, false, false, IS_STATS, NO_ADORN, NO_CURSOR); // Mirror is assignable
                  myGrid.Children.Add(b1);
                  Grid.SetRow(b1, rowNum);
                  Grid.SetColumn(b1, 0);
                  //-----------------------------------
                  Label labelforDirection1 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                  labelforDirection1.Content = "->";
                  myGrid.Children.Add(labelforDirection1);
                  Grid.SetRow(labelforDirection1, rowNum);
                  Grid.SetColumn(labelforDirection1, 1);
                  //-----------------------------------
                  UpdateModifier(rowNum, mirror, myGridRows[i].myUnassignable);
               }
               //-----------------------------------
               Button b2 = CreateButton(myGridRows[i].myUnassignable, NO_ENABLE, false, IS_STATS, IS_ADORN, NO_CURSOR);  // Prince is unassignable
               myGrid.Children.Add(b2);
               Grid.SetRow(b2, rowNum);
               Grid.SetColumn(b2, 2);
            }
            else
            {
               IMapItem prince = myGridRows[i].myUnassignable;
               Button b2 = CreateButton(prince, NO_ENABLE, false, IS_STATS, IS_ADORN, NO_CURSOR);
               myGrid.Children.Add(b2);
               Grid.SetRow(b2, rowNum);
               Grid.SetColumn(b2, 0);
               if ((false == prince.IsKilled) && (false == prince.IsUnconscious))
               {
                  //----------------------------------
                  Label labelforDirection1 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                  labelforDirection1.Content = "->";
                  myGrid.Children.Add(labelforDirection1);
                  Grid.SetRow(labelforDirection1, rowNum);
                  Grid.SetColumn(labelforDirection1, 1);
                  //----------------------------------
                  if (MirrorTargetEnum.MIRROR_STRIKE == myGridRows[i].myAttackMirrorState)
                  {
                     Button b111 = CreateButton(myGridRows[0].myAssignable, NO_ENABLE, false, IS_STATS, NO_ADORN, NO_CURSOR); // Mirror is assignable
                     myGrid.Children.Add(b111);
                     Grid.SetRow(b111, rowNum);
                     Grid.SetColumn(b111, 2);
                     UpdateModifier(rowNum, myGridRows[i].myUnassignable, myGridRows[0].myAssignable);
                  }
                  else if (MirrorTargetEnum.PRINCE_STRIKE == myGridRows[i].myAttackMirrorState)
                  {
                     Button b112 = CreateButton(myGridRows[0].myUnassignable, NO_ENABLE, false, IS_STATS, IS_ADORN, NO_CURSOR);  // Prince is unassignable
                     myGrid.Children.Add(b112);
                     Grid.SetRow(b112, rowNum);
                     Grid.SetColumn(b112, 2);
                     UpdateModifier(rowNum, myGridRows[i].myUnassignable, myGridRows[0].myUnassignable);
                  }
               }
            }
            //-----------------------------------
            switch (myState)
            {
               case CombatEnum.ASSIGN_STRIKES:
                  myGridRows[i].myResult = Utilities.NO_RESULT;
                  if (0 == i)
                  {
                     BitmapImage bmi = new BitmapImage();
                     bmi.BeginInit();
                     bmi.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
                     bmi.EndInit();
                     Image img = new Image { Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                     ImageBehavior.SetAnimatedSource(img, bmi);
                     myGrid.Children.Add(img);
                     Grid.SetRow(img, rowNum);
                     Grid.SetColumn(img, 4);
                  }
                  else
                  {
                     myGridRows[i].myDirection = StrikeEnum.MIRROR;
                     myGridRows[i].myAttackMirrorState = MirrorTargetEnum.NONE;
                     myGridRows[i].myAssignable = myGridRows[0].myAssignable;  // assign to mirror
                  }
                  break;
               case CombatEnum.SWITCH_ATTACK:
                  if (MirrorTargetEnum.NONE == myGridRows[i].myAttackMirrorState)
                  {
                     CheckBox cb = new CheckBox() { IsEnabled = true, IsChecked = false, FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                     cb.Checked += CheckBoxFightMirror_Checked;
                     myGrid.Children.Add(cb);
                     Grid.SetRow(cb, rowNum);
                     Grid.SetColumn(cb, 4);
                  }
                  else
                  {
                     myGridRows[i].myResult = Utilities.NO_RESULT;
                     BitmapImage bmi = new BitmapImage();
                     bmi.BeginInit();
                     bmi.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
                     bmi.EndInit();
                     Image img = new Image { Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                     ImageBehavior.SetAnimatedSource(img, bmi);
                     myGrid.Children.Add(img);
                     Grid.SetRow(img, rowNum);
                     Grid.SetColumn(img, 4);
                  }
                  break;
               case CombatEnum.MIRROR_STRIKE:
                  if (Utilities.NO_RESULT < myGridRows[i].myResult)
                  {
                     Label labelforResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myResult.ToString() };
                     myGrid.Children.Add(labelforResult);
                     Grid.SetRow(labelforResult, rowNum);
                     Grid.SetColumn(labelforResult, 4);
                  }
                  break;
               case CombatEnum.MIRROR_STRIKE_SHOW:
                  if (Utilities.NO_RESULT < myGridRows[i].myResult)
                  {
                     Label labelforResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myResult.ToString() };
                     myGrid.Children.Add(labelforResult);
                     Grid.SetRow(labelforResult, rowNum);
                     Grid.SetColumn(labelforResult, 4);
                  }
                  else
                  {
                     if (MirrorTargetEnum.NONE == myGridRows[i].myAttackMirrorState)
                     {
                        CheckBox cb = new CheckBox() { IsEnabled = true, IsChecked = false, FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                        cb.Checked += CheckBoxFightMirror_Checked;
                        myGrid.Children.Add(cb);
                        Grid.SetRow(cb, rowNum);
                        Grid.SetColumn(cb, 4);
                     }
                     else
                     {
                        //------------------------------------
                        BitmapImage bmi = new BitmapImage();
                        bmi.BeginInit();
                        bmi.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
                        bmi.EndInit();
                        Image img = new Image { Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                        ImageBehavior.SetAnimatedSource(img, bmi);
                        myGrid.Children.Add(img);
                        Grid.SetRow(img, rowNum);
                        Grid.SetColumn(img, 4);
                     }
                  }
                  break;
               case CombatEnum.APPLY_RING:
                  if (Utilities.NO_RESULT < myGridRows[i].myResult)
                  {
                     Label labelforResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myResult.ToString() };
                     myGrid.Children.Add(labelforResult);
                     Grid.SetRow(labelforResult, rowNum);
                     Grid.SetColumn(labelforResult, 4);
                  }
                  break;
               case CombatEnum.APPLY_RING_SHOW:
               case CombatEnum.STARTED_COUNTER:
                  if ((0 < myGridRows[i].myWoundsPending) || (0 < myGridRows[i].myPoisonPending))
                  {
                     Label labelforResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "?" };
                     myGrid.Children.Add(labelforResult);
                     Grid.SetRow(labelforResult, rowNum);
                     Grid.SetColumn(labelforResult, 4);
                  }
                  else if (Utilities.NO_RESULT < myGridRows[i].myResult)
                  {
                     Label labelforResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myResult.ToString() };
                     myGrid.Children.Add(labelforResult);
                     Grid.SetRow(labelforResult, rowNum);
                     Grid.SetColumn(labelforResult, 4);
                  }
                  else if (2 == myColumnAssignable)
                  {
                     if ((MirrorTargetEnum.ROLL_FOR_TARGET == myGridRows[i].myAttackMirrorState) || (MirrorTargetEnum.PRINCE_STRIKE == myGridRows[i].myAttackMirrorState) || (MirrorTargetEnum.MIRROR_STRIKE == myGridRows[i].myAttackMirrorState))
                     {
                        myGridRows[i].myResult = Utilities.NO_RESULT;
                        BitmapImage bmi = new BitmapImage();
                        bmi.BeginInit();
                        bmi.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
                        bmi.EndInit();
                        Image img = new Image { Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                        ImageBehavior.SetAnimatedSource(img, bmi);
                        myGrid.Children.Add(img);
                        Grid.SetRow(img, rowNum);
                        Grid.SetColumn(img, 4);
                        isAllDiceResultsShown = false;
                     }
                     else
                     {
                        CheckBox cb = new CheckBox() { IsEnabled = true, IsChecked = false, FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                        cb.Checked += CheckBoxFightMirror_Checked;
                        myGrid.Children.Add(cb);
                        Grid.SetRow(cb, rowNum);
                        Grid.SetColumn(cb, 4);
                     }
                  }
                  break;
               case CombatEnum.STARTED_STRIKES:
               case CombatEnum.SHOW_LAST_STRIKE:
               case CombatEnum.SHOW_LAST_COUNTER:
                  if ((0 < myGridRows[i].myWoundsPending) || (0 < myGridRows[i].myPoisonPending))
                  {
                     Label labelforResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "?" };
                     myGrid.Children.Add(labelforResult);
                     Grid.SetRow(labelforResult, rowNum);
                     Grid.SetColumn(labelforResult, 4);
                  }
                  else if (Utilities.NO_RESULT < myGridRows[i].myResult)
                  {
                     Label labelforResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myResult.ToString() };
                     myGrid.Children.Add(labelforResult);
                     Grid.SetRow(labelforResult, rowNum);
                     Grid.SetColumn(labelforResult, 4);
                  }
                  break;
               case CombatEnum.ROLL_FOR_BATTLE_STATE:
               case CombatEnum.ACTIVATE_ITEMS:
               case CombatEnum.APPLY_POISON:
               case CombatEnum.APPLY_SHIELD:
               case CombatEnum.FINALIZE_BATTLE_STATE:
               case CombatEnum.ASSIGN:
               case CombatEnum.ASSIGN_AFTER_ESCAPE:
               case CombatEnum.ROLL_FOR_HALFLING:
               case CombatEnum.NEXT_ROUND:
                  break;
               default:
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRowsForMirror(): reached default for myState=" + myState.ToString());
                  return false;
            }
         }
         //------------------------------------------------------------
         if (true == isAllDiceResultsShown)
         {
            if (CombatEnum.APPLY_RING_SHOW == myState)
            {
               if (CombatEnum.STARTED_STRIKES == myPreviousCombatState)
               {
                  Logger.Log(LogEnum.LE_COMBAT_STATE, "UpdateGridRowsForMirror(): isAllDiceResultsShown=t " + myState.ToString() + "-->SHOW_LAST_STRIKE");
                  myState = CombatEnum.SHOW_LAST_STRIKE;
               }
               else if (CombatEnum.STARTED_COUNTER == myPreviousCombatState)
               {
                  Logger.Log(LogEnum.LE_COMBAT_STATE, "UpdateGridRowsForMirror(): isAllDiceResultsShown=t " + myState.ToString() + "-->SHOW_LAST_COUNTER");
                  myState = CombatEnum.SHOW_LAST_COUNTER;
               }
               else
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateGridRowsForMirror(): invalid state=" + myPreviousCombatState.ToString());
                  return false;
               }
            }
            else
            {
               if (CombatEnum.STARTED_STRIKES == myState)
               {
                  Logger.Log(LogEnum.LE_COMBAT_STATE, "UpdateGridRowsForMirror(): isAllDiceResultsShown=t " + myState.ToString() + "-->c=SHOW_LAST_STRIKE");
                  myState = CombatEnum.SHOW_LAST_STRIKE;
               }
               else if (CombatEnum.STARTED_COUNTER == myState)
               {
                  Logger.Log(LogEnum.LE_COMBAT_STATE, "UpdateGridRowsForMirror(): isAllDiceResultsShown=t " + myState.ToString() + "-->SHOW_LAST_COUNTER");
                  myState = CombatEnum.SHOW_LAST_COUNTER;
               }
            }
         }
         else
         {
            if (CombatEnum.APPLY_RING_SHOW == myState)
            {
               Logger.Log(LogEnum.LE_COMBAT_STATE, "Grid_MouseDown(): isAllDiceResultsShown=f " + myState.ToString() + "-->" + myPreviousCombatState.ToString());
               myState = myPreviousCombatState;
            }
         }
         //------------------------------------------------------------
         // Update Wound Column
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            if (false == UpdateForWounds(rowNum))
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRowsForMirror(): UpdateForWounds() returned false");
               return false;
            }
         }
         return true;
      }
      private bool UpdateGridRowsForCavalry()
      {
         myTextBlockCol0.Visibility = Visibility.Visible;
         myTextBlockCol1.Visibility = Visibility.Visible;
         myTextBlockCol2.Visibility = Visibility.Visible;
         myTextBlockCol3.Visibility = Visibility.Visible;
         myTextBlockCol4.Visibility = Visibility.Visible;
         myTextBlockCol5.Visibility = Visibility.Visible;
         myTextBlockCol0.Text = "Attacker";
         myTextBlockCol1.Text = "Strikes";
         myTextBlockCol2.Text = "Defender";
         myTextBlockCol3.Text = "Modifiers";
         myTextBlockCol4.Text = "Result";
         myTextBlockCol5.Text = "Wounds";
         //------------------------------------------------------------
         // Update Results Column with die image or with results
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            int rowNum = i + STARTING_ASSIGNED_ROW;
            Button b = CreateButton(myGridRows[i].myAssignable, NO_ENABLE, false, NO_STATS, NO_ADORN, NO_CURSOR);
            myGrid.Children.Add(b);
            Grid.SetRow(b, rowNum);
            Grid.SetColumn(b, 0);
            Label labelforDirection = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "->" };
            myGrid.Children.Add(labelforDirection);
            Grid.SetRow(labelforDirection, rowNum);
            Grid.SetColumn(labelforDirection, 1);
            Button b1 = CreateButton(myGridRows[i].myUnassignable, NO_ENABLE, false, IS_STATS, NO_ADORN, NO_CURSOR);
            myGrid.Children.Add(b1);
            Grid.SetRow(b1, rowNum);
            Grid.SetColumn(b1, 2);
            Label labelforResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
            myGrid.Children.Add(labelforResult);
            Grid.SetRow(labelforResult, rowNum);
            Grid.SetColumn(labelforResult, 4);
            Label labelforWounds = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myUnassignable.Endurance.ToString() };
            myGrid.Children.Add(labelforWounds);
            Grid.SetRow(labelforWounds, rowNum);
            Grid.SetColumn(labelforWounds, 5);
         }
         return true;
      }
      private bool UpdateGridRowsDrug()
      {
         myTextBlockCol0.Text = "Member";
         myTextBlockCol1.Visibility = Visibility.Hidden;
         myTextBlockCol2.Text = "Owned";
         myTextBlockCol3.Text = "Shared";
         myTextBlockCol4.Visibility = Visibility.Hidden;
         myTextBlockCol5.Visibility = Visibility.Visible;
         myTextBlockCol5.Text = "Apply";
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            IMapItem mi = myGridRows[i].myUnassignable;
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRowsDrug(): mi=null");
               return false;
            }
            //----------------------------------------------
            int rowNum = i + STARTING_ASSIGNED_ROW;
            Button b = CreateButton(mi, NO_ENABLE, false, IS_STATS, IS_ADORN, NO_CURSOR);
            myGrid.Children.Add(b);
            Grid.SetRow(b, rowNum);
            Grid.SetColumn(b, 0);
            //----------------------------------------------
            if (DragStateEnum.NONE != myDragState) // If dragging, show rect where image used to be
            {
               Rectangle r2 = new Rectangle()
               {
                  Visibility = Visibility.Visible,
                  Stroke = mySolidColorBrushBlack,
                  Fill = Brushes.Transparent,
                  StrokeThickness = 2.0,
                  StrokeDashArray = myDashArray,
                  Width = Utilities.ZOOM * Utilities.theMapItemSize,
                  Height = Utilities.ZOOM * Utilities.theMapItemSize
               };
               myGrid.Children.Add(r2);
               Grid.SetRow(r2, myDragStateRowNum);
               Grid.SetColumn(r2, myDragStateColNum);
            }
            else
            {
               if (true == IsDrugOwned(mi))
               {
                  Image img3 = new Image { Name = "PoisonDrug", Source = MapItem.theMapImages.GetBitmapImage("PoisonDrug"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myGrid.Children.Add(img3);
                  Grid.SetRow(img3, rowNum);
                  Grid.SetColumn(img3, 2);
               }
               if (true == IsDrugShared(mi))
               {
                  Image img3 = new Image { Name = "PoisonDrug", Source = MapItem.theMapImages.GetBitmapImage("PoisonDrug"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myGrid.Children.Add(img3);
                  Grid.SetRow(img3, rowNum);
                  Grid.SetColumn(img3, 3);
               }
            }
            //-----------------------------------------------
            if (true == mi.IsPoisonApplied)
            {
               Image img4 = new Image { Source = MapItem.theMapImages.GetBitmapImage("CrossedSwordsPoisoned"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myGrid.Children.Add(img4);
               Grid.SetRow(img4, rowNum);
               Grid.SetColumn(img4, 5);
            }
            else
            {
               Rectangle r6 = new Rectangle() // show rectangle where to drag to
               {
                  Visibility = Visibility.Visible,
                  Stroke = mySolidColorBrushBlack,
                  Fill = Brushes.Transparent,
                  StrokeThickness = 2.0,
                  StrokeDashArray = myDashArray,
                  Width = Utilities.ZOOM * Utilities.theMapItemSize,
                  Height = Utilities.ZOOM * Utilities.theMapItemSize
               };
               myGrid.Children.Add(r6);
               Grid.SetRow(r6, rowNum);
               Grid.SetColumn(r6, 5);
            }
         }
         return true;
      }
      private bool UpdateGridRowsDrugEnd()
      {
         myTextBlockCol0.Text = "Member";
         myTextBlockCol1.Visibility = Visibility.Hidden;
         myTextBlockCol2.Visibility = Visibility.Hidden;
         myTextBlockCol3.Text = "Result";
         myTextBlockCol4.Visibility = Visibility.Hidden;
         myTextBlockCol5.Visibility = Visibility.Visible;
         myTextBlockCol5.Text = "Empty/Full";
         //--------------------------------------------------------
         bool isAllDiceResultsShown = true;
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            IMapItem mi = myGridRows[i].myUnassignable;
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRowsDrug(): mi=null");
               return false;
            }
            //----------------------------------------------
            int rowNum = i + STARTING_ASSIGNED_ROW;
            Button b = CreateButton(mi, NO_ENABLE, false, IS_STATS, IS_ADORN, NO_CURSOR);
            myGrid.Children.Add(b);
            Grid.SetRow(b, rowNum);
            Grid.SetColumn(b, 0);
            //----------------------------------------------
            if (false == mi.IsPoisonApplied)
            {
               myGridRows[i].myResult = 0;
               Label labelforResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
               myGrid.Children.Add(labelforResult);
               Grid.SetRow(labelforResult, rowNum);
               Grid.SetColumn(labelforResult, 3);
            }
            else if (Utilities.NO_RESULT < myGridRows[i].myResult)
            {
               Label labelforResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myResult.ToString() };
               myGrid.Children.Add(labelforResult);
               Grid.SetRow(labelforResult, rowNum);
               Grid.SetColumn(labelforResult, 3);
            }
            else
            {
               isAllDiceResultsShown = false;
               BitmapImage bmi0 = new BitmapImage();
               bmi0.BeginInit();
               bmi0.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
               bmi0.EndInit();
               Image img0 = new Image { Source = bmi0, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
               ImageBehavior.SetAnimatedSource(img0, bmi0);
               myGrid.Children.Add(img0);
               Grid.SetRow(img0, rowNum);
               Grid.SetColumn(img0, 3);
            }
            //----------------------------------------------
            if (0 < myGridRows[i].myResult)
            {
               if (6 == myGridRows[i].myResult)
               {
                  Image img5 = new Image { Source = MapItem.theMapImages.GetBitmapImage("PoisonDrugEmpty"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myGrid.Children.Add(img5);
                  Grid.SetRow(img5, rowNum);
                  Grid.SetColumn(img5, 5);
               }
               else
               {
                  Image img5 = new Image { Source = MapItem.theMapImages.GetBitmapImage("PoisonDrug"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myGrid.Children.Add(img5);
                  Grid.SetRow(img5, rowNum);
                  Grid.SetColumn(img5, 5);
               }
            }
         }
         if (true == isAllDiceResultsShown)
         {
            Logger.Log(LogEnum.LE_COMBAT_STATE, "UpdateGridRowsDrugEnd(): isAllDiceResultsShown=t " + myState.ToString() + "-->END_POISON_SHOW");
            myState = CombatEnum.END_POISON_SHOW;
            myIsDrugResultsEnded = true;
         }
         return true;
      }
      private bool UpdateGridRowsShield()
      {
         myTextBlockCol0.Text = "Member";
         myTextBlockCol1.Visibility = Visibility.Hidden;
         myTextBlockCol2.Text = "Owned";
         myTextBlockCol3.Text = "Shared";
         myTextBlockCol4.Visibility = Visibility.Hidden;
         myTextBlockCol5.Visibility = Visibility.Visible;
         myTextBlockCol5.Text = "Use";
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            IMapItem mi = myGridRows[i].myUnassignable;
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRowsShield(): mi=null");
               return false;
            }
            //----------------------------------------------
            int rowNum = i + STARTING_ASSIGNED_ROW;
            Button b = CreateButton(mi, NO_ENABLE, false, IS_STATS, IS_ADORN, NO_CURSOR);
            myGrid.Children.Add(b);
            Grid.SetRow(b, rowNum);
            Grid.SetColumn(b, 0);
            //----------------------------------------------
            if (DragStateEnum.NONE != myDragState) // If dragging, show rect where image used to be
            {
               Rectangle r2 = new Rectangle()
               {
                  Visibility = Visibility.Visible,
                  Stroke = mySolidColorBrushBlack,
                  Fill = Brushes.Transparent,
                  StrokeThickness = 2.0,
                  StrokeDashArray = myDashArray,
                  Width = Utilities.ZOOM * Utilities.theMapItemSize,
                  Height = Utilities.ZOOM * Utilities.theMapItemSize
               };
               myGrid.Children.Add(r2);
               Grid.SetRow(r2, myDragStateRowNum);
               Grid.SetColumn(r2, myDragStateColNum);
            }
            else
            {
               if (true == IsShieldOwned(mi))
               {
                  Image img3 = new Image { Name = "Shield", Source = MapItem.theMapImages.GetBitmapImage("Shield"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myGrid.Children.Add(img3);
                  Grid.SetRow(img3, rowNum);
                  Grid.SetColumn(img3, 2);
               }
               if (true == IsShieldShared(mi))
               {
                  Image img3 = new Image { Name = "Shield", Source = MapItem.theMapImages.GetBitmapImage("Shield"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myGrid.Children.Add(img3);
                  Grid.SetRow(img3, rowNum);
                  Grid.SetColumn(img3, 3);
               }
            }
            //-----------------------------------------------
            if (true == mi.IsShieldApplied)
            {
               Image img4 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Shield"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
               myGrid.Children.Add(img4);
               Grid.SetRow(img4, rowNum);
               Grid.SetColumn(img4, 5);
            }
            else
            {
               Rectangle r6 = new Rectangle() // show rectangle where to drag to
               {
                  Visibility = Visibility.Visible,
                  Stroke = mySolidColorBrushBlack,
                  Fill = Brushes.Transparent,
                  StrokeThickness = 2.0,
                  StrokeDashArray = myDashArray,
                  Width = Utilities.ZOOM * Utilities.theMapItemSize,
                  Height = Utilities.ZOOM * Utilities.theMapItemSize
               };
               myGrid.Children.Add(r6);
               Grid.SetRow(r6, rowNum);
               Grid.SetColumn(r6, 5);
            }
         }
         return true;
      }
      private bool UpdateGridRowsShieldEnd()
      {
         myTextBlockCol0.Text = "Member";
         myTextBlockCol1.Visibility = Visibility.Hidden;
         myTextBlockCol2.Visibility = Visibility.Hidden;
         myTextBlockCol3.Text = "Result";
         myTextBlockCol4.Visibility = Visibility.Hidden;
         myTextBlockCol5.Visibility = Visibility.Visible;
         myTextBlockCol5.Text = "Shiny/Dull";
         //--------------------------------------------------------
         bool isAllDiceResultsShown = true;
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            IMapItem mi = myGridRows[i].myUnassignable;
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRowsShieldEnd(): mi=null");
               return false;
            }
            //----------------------------------------------
            int rowNum = i + STARTING_ASSIGNED_ROW;
            Button b = CreateButton(mi, NO_ENABLE, false, IS_STATS, IS_ADORN, NO_CURSOR);
            myGrid.Children.Add(b);
            Grid.SetRow(b, rowNum);
            Grid.SetColumn(b, 0);
            //----------------------------------------------
            if (false == mi.IsShieldApplied)
            {
               myGridRows[i].myResult = 0;
               Label labelforResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
               myGrid.Children.Add(labelforResult);
               Grid.SetRow(labelforResult, rowNum);
               Grid.SetColumn(labelforResult, 3);
            }
            else if (Utilities.NO_RESULT < myGridRows[i].myResult)
            {
               Label labelforResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myResult.ToString() };
               myGrid.Children.Add(labelforResult);
               Grid.SetRow(labelforResult, rowNum);
               Grid.SetColumn(labelforResult, 3);
            }
            else
            {
               isAllDiceResultsShown = false;
               BitmapImage bmi0 = new BitmapImage();
               bmi0.BeginInit();
               bmi0.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
               bmi0.EndInit();
               Image img0 = new Image { Source = bmi0, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
               ImageBehavior.SetAnimatedSource(img0, bmi0);
               myGrid.Children.Add(img0);
               Grid.SetRow(img0, rowNum);
               Grid.SetColumn(img0, 3);
            }
            //----------------------------------------------
            if (0 < myGridRows[i].myResult)
            {
               if (6 == myGridRows[i].myResult)
               {
                  Image img5 = new Image { Source = MapItem.theMapImages.GetBitmapImage("ShieldDull"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myGrid.Children.Add(img5);
                  Grid.SetRow(img5, rowNum);
                  Grid.SetColumn(img5, 5);
               }
               else
               {
                  Image img5 = new Image { Source = MapItem.theMapImages.GetBitmapImage("Shield"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myGrid.Children.Add(img5);
                  Grid.SetRow(img5, rowNum);
                  Grid.SetColumn(img5, 5);
               }
            }
         }
         if (true == isAllDiceResultsShown)
         {
            Logger.Log(LogEnum.LE_COMBAT_STATE, "UpdateGridRowsShieldEnd(): " + myState.ToString() + "-->END_SHIELD_SHOW");
            myState = CombatEnum.END_SHIELD_SHOW;
            myIsShieldResultsEnded = true;
         }
         return true;
      }
      private bool UpdateGridRowsTalismanEnd()
      {
         myTextBlockCol0.Text = "Member";
         myTextBlockCol1.Visibility = Visibility.Hidden;
         myTextBlockCol2.Visibility = Visibility.Hidden;
         myTextBlockCol3.Text = "Result";
         myTextBlockCol4.Visibility = Visibility.Hidden;
         myTextBlockCol5.Visibility = Visibility.Visible;
         myTextBlockCol5.Text = "Status";
         //--------------------------------------------------------
         bool isOneDiceResultsShown = false;
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            IMapItem mi = myGridRows[i].myUnassignable;
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRowsTalismanEnd(): mi=null");
               return false;
            }
            //----------------------------------------------
            int rowNum = i + STARTING_ASSIGNED_ROW;
            Button b = CreateButton(mi, NO_ENABLE, false, IS_STATS, IS_ADORN, NO_CURSOR);
            myGrid.Children.Add(b);
            Grid.SetRow(b, rowNum);
            Grid.SetColumn(b, 0);
            //----------------------------------------------
            if (false == mi.IsSpecialItemHeld(SpecialEnum.ResistanceTalisman))
            {
               myGridRows[i].myResult = DO_NOT_OWN_TALISMAN; // UpdateGridRowsTalismanEnd() 
               Label labelforResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
               myGrid.Children.Add(labelforResult);
               Grid.SetRow(labelforResult, rowNum);
               Grid.SetColumn(labelforResult, 3);
               continue;
            }
            //----------------------------------------------
            if (NO_EFFECT_THIS_ATTACK < myGridRows[i].myResult)
            {
               isOneDiceResultsShown = true;
               Label labelforResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myResult.ToString() };
               myGrid.Children.Add(labelforResult);
               Grid.SetRow(labelforResult, rowNum);
               Grid.SetColumn(labelforResult, 3);
            }
            else if (NO_EFFECT_THIS_ATTACK == myGridRows[i].myResult) // show this if do not own Talisman
            {
               Label labelforResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "NA" };
               myGrid.Children.Add(labelforResult);
               Grid.SetRow(labelforResult, rowNum);
               Grid.SetColumn(labelforResult, 3);
            }
            else
            {
               BitmapImage bmi0 = new BitmapImage();
               bmi0.BeginInit();
               bmi0.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
               bmi0.EndInit();
               Image img0 = new Image { Source = bmi0, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
               ImageBehavior.SetAnimatedSource(img0, bmi0);
               myGrid.Children.Add(img0);
               Grid.SetRow(img0, rowNum);
               Grid.SetColumn(img0, 3);
            }
            //----------------------------------------------
            if (NO_EFFECT_THIS_ATTACK < myGridRows[i].myResult)
            {
               if (6 == myGridRows[i].myResult)
               {
                  BitmapImage bmi0 = new BitmapImage();
                  bmi0.BeginInit();
                  bmi0.UriSource = new Uri("../../Images/TalismanResistanceDestroy.gif", UriKind.Relative);
                  bmi0.EndInit();
                  Image img0 = new Image { Source = bmi0, Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  ImageBehavior.SetAnimatedSource(img0, bmi0);
                  ImageBehavior.SetAutoStart(img0, true);
                  ImageBehavior.SetRepeatBehavior(img0, new RepeatBehavior(1));
                  myGrid.Children.Add(img0);
                  Grid.SetRow(img0, rowNum);
                  Grid.SetColumn(img0, 5);
                  if( false == mi.RemoveSpecialItem(SpecialEnum.ResistanceTalisman))
                  {
                     Logger.Log(LogEnum.LE_ERROR, "UpdateGridRowsTalismanEnd(): RemoveSpecialItem() returned false for mi=" + mi.Name);
                     return false;
                  }
               }
               else
               {
                  Image img5 = new Image { Name = "TalismanResistance", Source = MapItem.theMapImages.GetBitmapImage("TalismanResistance"), Width = Utilities.ZOOM * Utilities.theMapItemSize, Height = Utilities.ZOOM * Utilities.theMapItemSize };
                  myGrid.Children.Add(img5);
                  Grid.SetRow(img5, rowNum);
                  Grid.SetColumn(img5, 5);
               }
            }
         }
         if (true == isOneDiceResultsShown)
         {
            Logger.Log(LogEnum.LE_COMBAT_STATE, "UpdateGridRowsTalismanEnd(): " + myState.ToString() + "-->END_TALISMAN_SHOW");
            myState = CombatEnum.END_TALISMAN_SHOW;
         }
         return true;
      }
      private bool UpdateGridRowsNerveGasBomb()
      {
         myTextBlockCol0.Text = "Victim";
         myTextBlockCol1.Visibility = Visibility.Hidden;
         myTextBlockCol2.Visibility = Visibility.Hidden;
         myTextBlockCol3.Text = "Result";
         myTextBlockCol4.Visibility = Visibility.Hidden;
         myTextBlockCol5.Visibility = Visibility.Hidden;
         //--------------------------------------------------------
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            IMapItem mi = myGridRows[i].myUnassignable;
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGridRowsDrug(): mi=null");
               return false;
            }
            //----------------------------------------------
            int rowNum = i + STARTING_ASSIGNED_ROW;
            Button b = CreateButton(mi, NO_ENABLE, false, IS_STATS, IS_ADORN, NO_CURSOR);
            myGrid.Children.Add(b);
            Grid.SetRow(b, rowNum);
            Grid.SetColumn(b, 0);
            //----------------------------------------------
            if (0 < myGridRows[i].myResult)
            {
               Label labelforResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myResult.ToString() };
               myGrid.Children.Add(labelforResult);
               Grid.SetRow(labelforResult, rowNum);
               Grid.SetColumn(labelforResult, 3);
            }
            else
            {
               BitmapImage bmi0 = new BitmapImage();
               bmi0.BeginInit();
               bmi0.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
               bmi0.EndInit();
               Image img0 = new Image { Source = bmi0, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
               ImageBehavior.SetAnimatedSource(img0, bmi0);
               myGrid.Children.Add(img0);
               Grid.SetRow(img0, rowNum);
               Grid.SetColumn(img0, 3);
            }
         }
         return true;
      }
      private bool UpdateStrikeDirection(int rowNum, IMapItem attacker, IMapItem defender)
      {
         // Determine direction
         int i = rowNum - STARTING_ASSIGNED_ROW;
         myGridRows[i].myDirection = StrikeEnum.NONE;
         switch (myState)
         {
            case CombatEnum.ROLL_FOR_BATTLE_STATE:
            case CombatEnum.ACTIVATE_ITEMS:
            case CombatEnum.APPLY_POISON:
            case CombatEnum.APPLY_SHIELD:
            case CombatEnum.FINALIZE_BATTLE_STATE:
            case CombatEnum.ROLL_FOR_HALFLING:
            case CombatEnum.NEXT_ROUND:
            case CombatEnum.ASSIGN:
            case CombatEnum.ASSIGN_AFTER_ESCAPE:
               if (null == myGridRows[i].myAssignable) // do not assign direction if no assignable character
                  break;
               if ((true == attacker.IsKilled) || (true == attacker.IsUnconscious)) // do not assign direction if attacker is MIA or KIA
                  break;
               if ((true == attacker.Name.Contains("Slave")) || ((true == attacker.Name.Contains("TrueLove")) && (0 == attacker.Combat)) || (true == attacker.Name.Contains("Porter")) || (true == attacker.Name.Contains("Falcon"))) // do not assign direction if non-combatant
                  break;
               if ((true == attacker.Name.Contains("Minstrel")) && (false == myIsMinstelFight) ) // do not assign direction if non-combatant
                  break;
               if (true == defender.IsKilled) // do not assign direction if defender is KIA
                  break;
               if (1 == myGridRows[i].myAssignmentCount)
               {
                  myGridRows[i].myDirection = StrikeEnum.BOTH;
               }
               else
               {
                  if (0 == myColumnAssignable)
                     myGridRows[i].myDirection = StrikeEnum.LEFT;
                  else
                     myGridRows[i].myDirection = StrikeEnum.RIGHT;
               }
               break;
            case CombatEnum.ASSIGN_STRIKES:
            case CombatEnum.STARTED_STRIKES:
            case CombatEnum.SHOW_LAST_STRIKE:
            case CombatEnum.SWITCH_ATTACK:
            case CombatEnum.STARTED_COUNTER:
            case CombatEnum.SHOW_LAST_COUNTER:
            case CombatEnum.APPLY_RING:
            case CombatEnum.APPLY_RING_SHOW:
               if (null == myGridRows[i].myAssignable)
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateStrikeDirection(): myGridRows[i].myAssignable=null");
                  return false;
               }
               if ((true == attacker.IsKilled) || (true == attacker.IsUnconscious))
                  break;
               if ((true == attacker.Name.Contains("Slave")) || ((true == attacker.Name.Contains("TrueLove")) && (0 == attacker.Combat)) || (true == attacker.Name.Contains("Porter")) || (true == attacker.Name.Contains("Falcon"))) // do not assign direction if non-combatant
                  break;
               if ((true == attacker.Name.Contains("Minstrel")) && (false == myIsMinstelFight)) // do not assign direction if non-combatant
                  break;
               if (true == defender.IsKilled)
                  break;
               if (0 == myColumnAssignable)
               {
                  if (1 == myGridRows[i].myAssignmentCount)
                     myGridRows[i].myDirection = StrikeEnum.RIGHT;
                  else
                     myGridRows[i].myDirection = StrikeEnum.NONE;
               }
               else
               {
                  myGridRows[i].myDirection = StrikeEnum.RIGHT;
               }
               break;
            case CombatEnum.KNIGHT_STRIKE:
            case CombatEnum.KNIGHT_STRIKE_SHOW:
            case CombatEnum.WIZARD_STRIKE:
            case CombatEnum.WIZARD_STRIKE_SHOW:
            case CombatEnum.ROLL_FOR_PROTECTOR:
            case CombatEnum.ROLL_FOR_PROTECTOR_SHOW:
               myGridRows[i].myDirection = StrikeEnum.NONE;
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateStrikeDirection(): reached default for myState=" + myState.ToString());
               return false;
         }
         Label labelforDirection = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
         switch (myGridRows[i].myDirection)
         {
            case StrikeEnum.BOTH:
               labelforDirection.Content = "<->";
               break;
            case StrikeEnum.LEFT:
               labelforDirection.Content = "<-";
               break;
            case StrikeEnum.RIGHT:
               labelforDirection.Content = "->";
               break;
            case StrikeEnum.NONE:
               labelforDirection.Content = " ";
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "GetStrikeDirection(): Reached default");
               return false;
         }
         myGrid.Children.Add(labelforDirection);
         Grid.SetRow(labelforDirection, rowNum);
         Grid.SetColumn(labelforDirection, 1);
         return true;
      }
      private void UpdateModifier(int rowNum, IMapItem attacker, IMapItem defender)
      {
         int i = rowNum - STARTING_ASSIGNED_ROW;
         myGridRows[i].myModifier = 0;
         if ((null == attacker) || (null == defender)) // null if not assigned yet
            return;
         if ((true == attacker.IsUnconscious) || (true == attacker.IsKilled))
            return;
         //---------------------------------------------------
         myGridRows[i].myModifier += attacker.Combat;
         if (0 < (attacker.Wound + attacker.Poison))
            --myGridRows[i].myModifier;
         if (0 < (attacker.Wound + attacker.Poison))
         {
            if (attacker.Endurance / 2 <= (attacker.Wound + attacker.Poison))
               --myGridRows[i].myModifier;
         }
         if (true == attacker.IsShieldApplied) // if attacher has shield, the defender combat reduced by one
            ++myGridRows[i].myModifier;
         //---------------------------------------------------
         if ((false == defender.IsUnconscious) && (false == defender.IsKilled))// uncounscious or kia cannot defend and combat is zero
            myGridRows[i].myModifier -= defender.Combat;
         if (0 < (defender.Wound + defender.Poison))
         {
            if (defender.Endurance / 2 <= (defender.Wound + defender.Poison))
               myGridRows[i].myModifier += 2;
         }
         if (true == defender.IsShieldApplied)
            --myGridRows[i].myModifier;
         //---------------------------------------------------
         myGridRows[i].myModifier -= attacker.StarveDayNum; // subtract for number of attacker starvation days (1 from combat for each day)
         myGridRows[i].myModifier += defender.StarveDayNum; // add for number of defender starvation days (1 from combat for each day)
         //---------------------------------------------------
         Label labelforModifier = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myModifier.ToString() };
         myGrid.Children.Add(labelforModifier);
         Grid.SetRow(labelforModifier, rowNum);
         Grid.SetColumn(labelforModifier, 3);
      }
      private bool UpdateResultsColumn(int rowNum, ref bool isAllDiceResultsShownForCombat)
      {
         int i = rowNum - STARTING_ASSIGNED_ROW;
         switch (myState)
         {
            case CombatEnum.ASSIGN_STRIKES:
               myGridRows[i].myResult = Utilities.NO_RESULT;
               if ((StrikeEnum.RIGHT == myGridRows[i].myDirection) || (StrikeEnum.BOTH == myGridRows[i].myDirection))
               {
                  BitmapImage bmi = new BitmapImage();
                  bmi.BeginInit();
                  bmi.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
                  bmi.EndInit();
                  Image img = new Image { Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                  ImageBehavior.SetAnimatedSource(img, bmi);
                  myGrid.Children.Add(img);
                  Grid.SetRow(img, rowNum);
                  Grid.SetColumn(img, 4);
               }
               break;
            case CombatEnum.APPLY_RING:
               if (Utilities.NO_RESULT < myGridRows[i].myResult)
               {
                  Label labelforResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myResult.ToString() };
                  myGrid.Children.Add(labelforResult);
                  Grid.SetRow(labelforResult, rowNum);
                  Grid.SetColumn(labelforResult, 4);
               }
               break;
            case CombatEnum.STARTED_STRIKES:
            case CombatEnum.STARTED_COUNTER:
            case CombatEnum.APPLY_RING_SHOW:
               if ((0 < myGridRows[i].myWoundsPending) || (0 < myGridRows[i].myPoisonPending))
               {
                  Label labelforResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "?" };
                  myGrid.Children.Add(labelforResult);
                  Grid.SetRow(labelforResult, rowNum);
                  Grid.SetColumn(labelforResult, 4);
               }
               else if (Utilities.NO_RESULT < myGridRows[i].myResult)
               {
                  Label labelforResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = myGridRows[i].myResult.ToString() };
                  myGrid.Children.Add(labelforResult);
                  Grid.SetRow(labelforResult, rowNum);
                  Grid.SetColumn(labelforResult, 4);
               }
               else if ((StrikeEnum.RIGHT == myGridRows[i].myDirection) || (StrikeEnum.BOTH == myGridRows[i].myDirection))
               {
                  BitmapImage bmi = new BitmapImage();
                  bmi.BeginInit();
                  bmi.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
                  bmi.EndInit();
                  Image img = new Image { Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                  ImageBehavior.SetAnimatedSource(img, bmi);
                  myGrid.Children.Add(img);
                  Grid.SetRow(img, rowNum);
                  Grid.SetColumn(img, 4);
                  isAllDiceResultsShownForCombat = false;
               }
               break;
            case CombatEnum.SWITCH_ATTACK:
               myGridRows[i].myResult = Utilities.NO_RESULT;
               if (StrikeEnum.RIGHT == myGridRows[i].myDirection)
               {
                  BitmapImage bmi = new BitmapImage();
                  bmi.BeginInit();
                  bmi.UriSource = new Uri("../../Images/dieRoll.gif", UriKind.Relative);
                  bmi.EndInit();
                  Image img = new Image { Source = bmi, Width = Utilities.theMapItemOffset, Height = Utilities.theMapItemOffset };
                  ImageBehavior.SetAnimatedSource(img, bmi);
                  myGrid.Children.Add(img);
                  Grid.SetRow(img, rowNum);
                  Grid.SetColumn(img, 4);
               }
               break;
            case CombatEnum.ROLL_FOR_BATTLE_STATE:
            case CombatEnum.ACTIVATE_ITEMS:
            case CombatEnum.APPLY_POISON:
            case CombatEnum.APPLY_SHIELD:
            case CombatEnum.FINALIZE_BATTLE_STATE:
            case CombatEnum.ASSIGN:
            case CombatEnum.ASSIGN_AFTER_ESCAPE:
            case CombatEnum.SHOW_LAST_STRIKE:
            case CombatEnum.SHOW_LAST_COUNTER:
            case CombatEnum.ROLL_FOR_HALFLING:
            case CombatEnum.NEXT_ROUND:
            case CombatEnum.WIZARD_STRIKE:
            case CombatEnum.WIZARD_STRIKE_SHOW:
            case CombatEnum.KNIGHT_STRIKE:
            case CombatEnum.KNIGHT_STRIKE_SHOW:
            case CombatEnum.ROLL_FOR_PROTECTOR:
            case CombatEnum.ROLL_FOR_PROTECTOR_SHOW:
               break;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateResultsColumn(): reached default for myState=" + myState.ToString());
               return false;
         }
         return true;
      }
      private bool UpdateForWounds(int rowNum)
      {
         int i = rowNum - STARTING_ASSIGNED_ROW;
         if (i < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "UpdateForWounds(): 0 >i=" + i.ToString());
            return false;
         }
         IMapItem defender = null;
         if (0 == myColumnAssignable)
            defender = myGridRows[i].myUnassignable;
         else
            defender = myGridRows[i].myAssignable;
         string defenderName = "null";
         if (null != defender)
            defenderName = defender.Name;
         switch (myState)
         {
            case CombatEnum.ROLL_FOR_BATTLE_STATE:
            case CombatEnum.ACTIVATE_ITEMS:
            case CombatEnum.APPLY_POISON:
            case CombatEnum.APPLY_SHIELD:
            case CombatEnum.FINALIZE_BATTLE_STATE:
            case CombatEnum.ASSIGN:
            case CombatEnum.ASSIGN_AFTER_ESCAPE:
            case CombatEnum.ASSIGN_STRIKES:
            case CombatEnum.SWITCH_ATTACK:
               myGridRows[i].myDamage = Utilities.NO_RESULT;
               return true;
            case CombatEnum.APPLY_RING:
               Logger.Log(LogEnum.LE_COMBAT_RESULT, "UpdateForWounds() myState=" + myState.ToString() + " mi=" + defenderName +  " wp=" + myGridRows[i].myWoundsPending.ToString() + " pp=" + myGridRows[i].myPoisonPending.ToString());
               if ((0 < myGridRows[i].myWoundsPending) || (0 < myGridRows[i].myPoisonPending))
               {
                  Label labelforResult = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Content = "?" };
                  myGrid.Children.Add(labelforResult);
                  Grid.SetRow(labelforResult, rowNum);
                  Grid.SetColumn(labelforResult, 5);
               }
               else
               {
                  int damage1 = 0;
                  if (NO_EFFECT_THIS_ATTACK < myGridRows[i].myDamage)
                     damage1 += myGridRows[i].myDamage;
                  if (NO_EFFECT_THIS_ATTACK < myGridRows[i].myDamageFireball)
                     damage1 += myGridRows[i].myDamageFireball;
                  if( 0 < damage1 )
                  {
                     Label labelforWounds1 = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                     labelforWounds1.Content = damage1.ToString();
                     myGrid.Children.Add(labelforWounds1);
                     Grid.SetRow(labelforWounds1, rowNum);
                     Grid.SetColumn(labelforWounds1, 5);
                  }
               }
               return true;
            case CombatEnum.STARTED_STRIKES:
            case CombatEnum.SHOW_LAST_STRIKE:
            case CombatEnum.STARTED_COUNTER:
            case CombatEnum.SHOW_LAST_COUNTER:
            case CombatEnum.APPLY_RING_SHOW:
            case CombatEnum.WIZARD_STRIKE:
            case CombatEnum.WIZARD_STRIKE_SHOW:
            case CombatEnum.MIRROR_STRIKE:
            case CombatEnum.MIRROR_STRIKE_SHOW:
            case CombatEnum.KNIGHT_STRIKE:
            case CombatEnum.KNIGHT_STRIKE_SHOW:
            case CombatEnum.ROLL_FOR_PROTECTOR:
            case CombatEnum.ROLL_FOR_PROTECTOR_SHOW:
               if ((Utilities.NO_RESULT == myGridRows[i].myDamage) && (Utilities.NO_RESULT == myGridRows[i].myDamageFireball) )
                  return true;
               break;
            case CombatEnum.ROLL_FOR_HALFLING:
            case CombatEnum.NEXT_ROUND:
               return true;
            default:
               Logger.Log(LogEnum.LE_ERROR, "UpdateForWounds(): reached default for myState=" + myState.ToString());
               return false;
         }
         Label labelforWounds = null;
         if (SPECTRE_REQUIRES_MAGIC == myGridRows[i].myDamage)
         {
            labelforWounds = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            labelforWounds.Content = "NA";
         }
         else
         {
            int damage = 0;
            if (NO_EFFECT_THIS_ATTACK < myGridRows[i].myDamage)
               damage += myGridRows[i].myDamage;
            if (NO_EFFECT_THIS_ATTACK < myGridRows[i].myDamageFireball)
               damage += myGridRows[i].myDamageFireball;
            labelforWounds = new Label() { FontFamily = myFontFam, FontSize = 24, HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            if (WIZARD_ESCAPE == myGridRows[i].myDamage)
               labelforWounds.Content = "Escape";
            else
               labelforWounds.Content = damage.ToString();
            Logger.Log(LogEnum.LE_COMBAT_RESULT, "UpdateForWounds() myState=" + myState.ToString() + " mi=" + defenderName + " dmg[" + i.ToString() + "]=" + damage.ToString());
         }
         myGrid.Children.Add(labelforWounds);
         Grid.SetRow(labelforWounds, rowNum);
         Grid.SetColumn(labelforWounds, 5);
         return true;
      }
      //-----------------------------------------------------------------------------------------
      private Button CreateButton(IMapItem mi, bool isEnabled, bool isRectangleAdded, bool isStatsShown, bool isAdornmentsShown, bool isCursor)
      {
         System.Windows.Controls.Button b = new Button { };
         b.Name = Utilities.RemoveSpaces(mi.Name);
         if (true == isCursor)
         {
            b.Width = Utilities.theMapItemSize;
            b.Height = Utilities.theMapItemSize;
         }
         else
         {
            b.Width = Utilities.ZOOM * Utilities.theMapItemSize;
            b.Height = Utilities.ZOOM * Utilities.theMapItemSize;
         }
         if (false == isRectangleAdded)
         {
            b.BorderThickness = new Thickness(0);
         }
         else
         {
            b.BorderThickness = new Thickness(1);
            b.BorderBrush = Brushes.Black;
         }
         b.Background = new SolidColorBrush(Colors.Transparent);
         b.Foreground = new SolidColorBrush(Colors.Transparent);
         if (true == isEnabled)
         {
            b.IsEnabled = isEnabled;
            b.Click += this.Button_Click;
         }
         MapItem.SetButtonContent(b, mi, isStatsShown, isAdornmentsShown, true); // This sets the image as the button's content
         return b;
      }
      private int GetAssignedCount(string name)
      {
         int count = 0;
         for (int i = 0; i < myMaxRowCount; ++i) // set the default grid data
         {
            IMapItem assignable = myGridRows[i].myAssignable;
            if (null == assignable)
               continue;
            if (Utilities.RemoveSpaces(name) == Utilities.RemoveSpaces(assignable.Name))
               ++count;
         }
         return count;
      }
      private int GetOpenCount()
      {
         int count = 0;
         for (int i = 0; i < myMaxRowCount; ++i) // set the default grid data
         {
            IMapItem assignable = myGridRows[i].myAssignable;
            if (null == assignable)
               ++count;
         }
         return count;
      }
      private int GetUnassignedCount()
      {
         int count = 0;
         foreach (IMapItem mi in myAssignables)
         {
            if (0 == GetAssignedCount(mi.Name))
               ++count;
         }
         return count;
      }
      private bool DecrementAssignmentCounts(int gridIndex)
      {
         int countNumBeingRemoved = myGridRows[gridIndex].myAssignmentCount;
         IMapItem miBeingRemoved = myGridRows[gridIndex].myAssignable;
         if (null == miBeingRemoved)
         {
            Logger.Log(LogEnum.LE_ERROR, "DecrementAssignmentCounts(): miBeingRemoved=null for row=" + gridIndex.ToString());
            return false;
         }
         // For each number at or above this number, decrement by one number
         StringBuilder sb = new StringBuilder();
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            sb.Append(i.ToString());
            if (null == myGridRows[i].myAssignable)
            {
               sb.Append("=null\n");
            }
            else
            {
               if (i == gridIndex)
               {
                  sb.Append("=index count=0\n");
                  myGridRows[gridIndex].myAssignmentCount = 0; // set to zero if button being moved is this row
               }
               else
               {
                  sb.Append("=? n=");
                  sb.Append(miBeingRemoved.Name);
                  sb.Append(" ");
                  sb.Append(myGridRows[i].myAssignable.Name);
                  sb.Append(" ");
                  // If this value is greater than the button being removed, decrement count
                  if ((miBeingRemoved.Name == myGridRows[i].myAssignable.Name) && (countNumBeingRemoved <= myGridRows[i].myAssignmentCount))
                  {
                     --myGridRows[i].myAssignmentCount;
                     sb.Append(" --");
                  }
                  sb.Append(myGridRows[i].myAssignmentCount.ToString());
                  sb.Append("\n");
               }
            }
         }
         Logger.Log(LogEnum.LE_VIEW_DEC_COUNT_GRID, sb.ToString());
         return true;
      }
      private void SetRemainingAssignments()
      {
         int openRowCount = GetOpenCount();
         int unassignedCount = GetUnassignedCount();
         if ((1 == unassignedCount) && (0 < openRowCount)) // one remaining unassigned and open rows left - assign to all remaining open rows
         {
            foreach (IMapItem mi in myAssignables)
            {
               if (0 == GetAssignedCount(mi.Name))
               {
                  for (int i = 0; i < myMaxRowCount; ++i) // assign this mi to the remaining unassigned rows
                  {
                     if (null == myGridRows[i].myAssignable)
                     {
                        myGridRows[i].myAssignable = mi;
                        myGridRows[i].myAssignmentCount = GetAssignedCount(mi.Name);
                     }
                  }
                  return;
               }
            }
         }
         else if ((1 == myAssignables.Count) && (0 < openRowCount)) // only one assignable and open rows left - assign to all remaining open rows
         {
            for (int i = 0; i < myMaxRowCount; ++i) // assign this mi to the remaining unassigned rows
            {
               IMapItem assignable = myAssignables[0];
               if (null == myGridRows[i].myAssignable)
               {
                  myGridRows[i].myAssignable = assignable;
                  myGridRows[i].myAssignmentCount = GetAssignedCount(assignable.Name);
               }
            }
         }
      }
      private bool SetInitialFightState(string caller)
      {
         if (true == myGameInstance.IsCavalryEscort)
         {
            Logger.Log(LogEnum.LE_COMBAT_STATE, "SetInitialFightState(): " + myState.ToString() + "-->CAVALRY_STRIKE");
            myState = CombatEnum.CAVALRY_STRIKE;
         }
         else
         {
            myBattleEnumInitial = myBattleEnum;
            myIsDrugShown = IsDrugShown();
            myIsShieldShown = IsShieldShown();
            myIsTalismanShown = IsTalismanShown();
            myIsNerveGasBombShown = IsNerveGasBombShown();
            if (((true == myIsDrugShown) || (true == myIsShieldShown) || (true == myIsTalismanShown) || (true == myIsNerveGasBombShown)) && (false == myIsMirrorFight))
            {
               Logger.Log(LogEnum.LE_COMBAT_STATE, "SetInitialFightState(): from " + caller + ": " + myState.ToString() + "-->ACTIVATE_ITEMS");
               myState = CombatEnum.ACTIVATE_ITEMS;
            }
            else if (true == myIsKnightOnBridge)
            {
               Logger.Log(LogEnum.LE_COMBAT_STATE, "SetInitialFightState(): " + myState.ToString() + "-->KNIGHT_STRIKE");
               myState = CombatEnum.KNIGHT_STRIKE;
            }
            else
            {
               Logger.Log(LogEnum.LE_COMBAT_STATE, "SetInitialFightState(): from " + caller + ": " + myState.ToString() + "-->FINALIZE_BATTLE_STATE");
               myState = CombatEnum.FINALIZE_BATTLE_STATE;
            }
         }
         return true;
      }
      private bool SetStateIfItemUsed()
      {
         bool isDrugApplied = false;
         bool isShieldApplied = false;
         foreach (IMapItem mi in myGameInstance.PartyMembers) // if drug is applied (see e185), need to check for removal
         {
            if (true == mi.IsPoisonApplied)
               isDrugApplied = true;
            if (true == mi.IsShieldApplied)
               isShieldApplied = true;
         }
         if (true == isDrugApplied)
         {
            Logger.Log(LogEnum.LE_COMBAT_STATE, "SetStateIfItemUsed(): " + myState.ToString() + "-->END_POISON");
            myState = CombatEnum.END_POISON;
            return true;
         }
         if (true == isShieldApplied)
         {
            Logger.Log(LogEnum.LE_COMBAT_STATE, "SetStateIfItemUsed(): " + myState.ToString() + "-->END_SHIELD");
            myState = CombatEnum.END_SHIELD;
            return true;
         }
         //-------------------------------
         if (true == myIsTalismanActivated)
         {
            Logger.Log(LogEnum.LE_COMBAT_STATE, "SetStateIfItemUsed(): " + myState.ToString() + "-->END_TALISMAN");
            myState = CombatEnum.END_TALISMAN;
            return true;
         }
         return false; // return false if state not set
      }
      private void DistributeDeadWealth()
      {
         //---------------------------------------------
         foreach (IMapItem mi in myGameInstance.EncounteredMembers) // Handle ones killed at end but not removed as casualties yet
         {
            if ((true == mi.IsKilled) || (true == mi.IsUnconscious))
            {
               Logger.Log(LogEnum.LE_ADD_WEALTH_CODE, "DistributeDeadWealth(): mi=" + mi.Name + " coffs up wc=" +  mi.WealthCode.ToString());
               if (0 < mi.WealthCode)
                  myCapturedWealthCodes.Add(mi.WealthCode);
               foreach (SpecialEnum possession in mi.SpecialKeeps)
                  myCapturedPossessions.Add(possession);
               foreach (SpecialEnum possession in mi.SpecialShares)
                  myCapturedPossessions.Add(possession);
               foreach (IMapItem mount in mi.Mounts)
                  myCapturedMounts.Add(mount);
               if (true == mi.Name.Contains("Troll"))  // e057 - Trolls skins are worth money
                  myCapturedPossessions.Add(SpecialEnum.TrollSkin);
               if (true == mi.Name.Contains("Roc"))  // e099 - Roc Beaks worth money
                  myCapturedPossessions.Add(SpecialEnum.RocBeak);
               if (true == mi.Name.Contains("Griffon"))  // e100 - Griffon Claws worth money
                  myCapturedPossessions.Add(SpecialEnum.GriffonClaws);
            }
         }
         //---------------------------------------------
         IMapItems killedMembers = new MapItems();
         foreach (IMapItem mi in myGameInstance.PartyMembers) // Handle ones killed at end but not removed as casualties yet
         {
            if ((true == mi.IsKilled) || (true == mi.IsUnconscious))
            {
               myDeadPartyMemberCoin += mi.Coin;
               foreach (IMapItem mount in mi.Mounts)
                  myCapturedMounts.Add(mount);
               foreach (SpecialEnum possession in mi.SpecialShares)
                  myCapturedPossessions.Add(possession);
               if (true == mi.IsKilled) // If party member is killed, can have the special possessions that they own
               {
                  killedMembers.Add(mi);
                  foreach (SpecialEnum possession in mi.SpecialKeeps)
                     myCapturedPossessions.Add(possession);
               }
            }
         }
         foreach (IMapItem mi in killedMembers)
            myGameInstance.PartyMembers.Remove(mi);
         //---------------------------------------------
         myGameInstance.CapturedWealthCodes = myCapturedWealthCodes;
         myGameInstance.AddCoins(myDeadPartyMemberCoin, false); // looters do not get share of this pile
         myGameInstance.TransferMounts(myCapturedMounts);
         myGameInstance.AddSpecialItems(myCapturedPossessions);
         Logger.Log(LogEnum.LE_ADD_WEALTH_CODE, "DistributeDeadWealth(): CapturedWealthCodes.Count=" + myGameInstance.CapturedWealthCodes.Count.ToString());
      }
      private bool RemoveCasualties()
      {
         if (true == myIsMirrorFight)
         {
            for (int i = 0; i < myMaxRowCount; ++i)
               myGridRows[i].myAssignable = myAssignables[0];  // assign every row to mirror image
         }
         bool isPartyMemberDied = false;  // e331 - fickle party members leave if anybody dies
         IMapItems results1 = new MapItems();
         IMapItems results2 = new MapItems();
         if (true == myIsPartyMembersAssignable)
         {
            foreach (IMapItem mi in myAssignables)
            {
               if (true == mi.IsKilled)
               {
                  isPartyMemberDied = true;
                  results1.Add(mi);
                  if (true == mi.IsSpecialItemHeld(SpecialEnum.ResurrectionNecklace))
                  {
                     mi.RemoveSpecialItem(SpecialEnum.ResurrectionNecklace);
                     myGameInstance.ResurrectedMembers.Add(mi);
                  }
                  foreach (IMapItem mount in mi.Mounts)
                     myCapturedMounts.Add(mount);
                  foreach (SpecialEnum possession in mi.SpecialKeeps)
                     myCapturedPossessions.Add(possession);
                  foreach (SpecialEnum possession in mi.SpecialShares)
                     myCapturedPossessions.Add(possession);
                  myDeadPartyMemberCoin += mi.Coin;
                  if (true == mi.Name.Contains("Griffon"))  // e100 - griffon claws helps with Lady Aeravir
                     myCapturedPossessions.Add(SpecialEnum.GriffonClaws);
               }
               else if ((true == mi.IsRunAway) && (false == myGameInstance.Prince.IsRunAway)) // e007 - runaways are caused by nerve gas
               {
                  results1.Add(mi);
               }
            }
            foreach (IMapItem mi in myUnassignables)
            {
               if ((true == myIsWizardEscape) && (true == mi.Name.Contains("Wizard"))) // e023 - wizard escapes if damaged and sent fireball
               {
                  results2.Add(mi);
                  continue;
               }
               if (true == mi.IsKilled)
               {
                  results2.Add(mi);
                  foreach (IMapItem mount in mi.Mounts)
                     myCapturedMounts.Add(mount);
                  foreach (SpecialEnum possession in mi.SpecialKeeps)
                     myCapturedPossessions.Add(possession);
                  foreach (SpecialEnum possession in mi.SpecialShares)
                     myCapturedPossessions.Add(possession);
                  Logger.Log(LogEnum.LE_ADD_WEALTH_CODE, "RemoveCasualties(): mi=" + mi.Name + " coffs up wc=" + mi.WealthCode.ToString());
                  if (0 < mi.WealthCode)
                     myCapturedWealthCodes.Add(mi.WealthCode);
                  if ((mi.Endurance < 9) && (mi.Combat < 9) && ("e054b" != myGameInstance.EventStart) && ("e033" != myGameInstance.EventStart)) // Endurance/Combat of enemy must be less than nine to route
                  {
                     myIsRouteOfEnemyPossible = true;
                     Logger.Log(LogEnum.LE_COMBAT_STATE_ROUTE, "RemoveCasualties(): s=" + myState.ToString() + " route?=" + myIsRouteOfEnemyPossible.ToString());
                  }
                  if (true == mi.Name.Contains("Troll"))  // e057 - Trolls skins are worth money
                     myCapturedPossessions.Add(SpecialEnum.TrollSkin);
                  if (true == mi.Name.Contains("Roc"))  // e099 - Roc Beaks worth money
                     myCapturedPossessions.Add(SpecialEnum.RocBeak);
                  if (true == mi.Name.Contains("Griffon"))  // e100 - Griffon Claws help with Lady Aeravir
                     myCapturedPossessions.Add(SpecialEnum.GriffonClaws);
                  myGameInstance.KilledLocations.Add(myGameInstance.Prince.Territory);
               }
               else if (true == mi.IsRunAway) // runaways are caused by nerve gas
               {
                  results2.Add(mi);
               }
            }
         }
         else
         {
            foreach (IMapItem mi in myUnassignables) // party members are unassignable
            {
               if (true == mi.IsKilled)
               {
                  isPartyMemberDied = true;
                  results2.Add(mi);
                  if (true == mi.IsSpecialItemHeld(SpecialEnum.ResurrectionNecklace))
                  {
                     mi.RemoveSpecialItem(SpecialEnum.ResurrectionNecklace);
                     myGameInstance.ResurrectedMembers.Add(mi);
                  }
                  foreach (IMapItem mount in mi.Mounts)
                     myCapturedMounts.Add(mount);
                  foreach (SpecialEnum possession in mi.SpecialKeeps)
                     myCapturedPossessions.Add(possession);
                  foreach (SpecialEnum possession in mi.SpecialShares)
                     myCapturedPossessions.Add(possession);
                  myDeadPartyMemberCoin += mi.Coin;
                  if (true == mi.Name.Contains("Griffon"))  // e100 - griffon claws helps with Lady Aeravir
                     myCapturedPossessions.Add(SpecialEnum.GriffonClaws);
               }
               else if ((true == mi.IsRunAway) && (false == myGameInstance.Prince.IsRunAway)) // e007 - runaways are caused by nerve gas
               {
                  results2.Add(mi);
               }
            }
            foreach (IMapItem mi in myAssignables)
            {
               if ((true == myIsWizardEscape) && (true == mi.Name.Contains("Wizard"))) // e023 - wizard escapes if damaged and sent fireball
               {
                  results1.Add(mi);
                  continue;
               }
               if (true == mi.IsKilled)
               {
                  results1.Add(mi);
                  foreach (IMapItem mount in mi.Mounts)
                     myCapturedMounts.Add(mount);
                  foreach (SpecialEnum possession in mi.SpecialKeeps)
                     myCapturedPossessions.Add(possession);
                  foreach (SpecialEnum possession in mi.SpecialShares)
                     myCapturedPossessions.Add(possession);
                  Logger.Log(LogEnum.LE_ADD_WEALTH_CODE, "RemoveCasualties(): mi=" + mi.Name + " coffs up wc=" + mi.WealthCode.ToString());
                  if (0 < mi.WealthCode)
                     myCapturedWealthCodes.Add(mi.WealthCode);
                  if ((mi.Endurance < 9) && (mi.Combat < 9) && ("e054b" != myGameInstance.EventStart) && ("e033" != myGameInstance.EventStart)) // Endurance/Combat of enemy must be less than nine to route
                  {
                     myIsRouteOfEnemyPossible = true;
                     Logger.Log(LogEnum.LE_COMBAT_STATE_ROUTE, "RemoveCasualties(): s=" + myState.ToString() + " route?=" + myIsRouteOfEnemyPossible.ToString());
                  }
                  if (true == mi.Name.Contains("Troll"))  // e057 - Trolls skins are worth money
                     myCapturedPossessions.Add(SpecialEnum.TrollSkin);
                  if (true == mi.Name.Contains("Roc"))  // e099 - Roc Beaks worth money
                     myCapturedPossessions.Add(SpecialEnum.RocBeak);
                  if (true == mi.Name.Contains("Griffon"))  // e099 - griffon claws help with Lady Aeravir
                     myCapturedPossessions.Add(SpecialEnum.GriffonClaws);
                  myGameInstance.KilledLocations.Add(myGameInstance.Prince.Territory);
               }
               else if (true == mi.IsRunAway) // runaways are caused by nerve gas
               {
                  results1.Add(mi);
               }
            }
         }
         //-------------------------------------
         for (int i = 0; i < myMaxRowCount; ++i)
         {
            IMapItem mi0 = myGridRows[i].myAssignable;
            if (null == mi0)
            {
               Logger.Log(LogEnum.LE_ERROR, "RemoveCasualties(): 1-mi0=null for i=" + i.ToString());
               return false;
            }
            if (true == mi0.IsKilled)
            {
               if (false == DecrementAssignmentCounts(i))
               {
                  Logger.Log(LogEnum.LE_ERROR, "RemoveCasualties(): 1-DecrementAssignmentCounts() returned false");
                  return false;
               }
            }
         }
         //-------------------------------------
         if (true == isPartyMemberDied) // e331 - fickle member depart if anybody dies
         {
            for (int i = 0; i < myMaxRowCount; ++i)
            {
               if (null == myGridRows[i].myAssignable)
               {
                  Logger.Log(LogEnum.LE_ERROR, "RemoveCasualties(): 2-myGridRows[i].myAssignable=null for i=" + i.ToString());
                  return false;
               }
               if (null == myGridRows[i].myUnassignable) // Next check the unassignable mapitems
               {
                  Logger.Log(LogEnum.LE_ERROR, "RemoveCasualties(): 2-myGridRows[i].myUnassignable=null for i=" + i.ToString());
                  return false;
               }
               IMapItem mi0 = myGridRows[i].myAssignable;
               IMapItem mi1 = myGridRows[i].myUnassignable;
               if (true == myIsPartyMembersAssignable)
               {
                  if (true == mi0.IsFickle)
                  {
                     results1.Add(mi0);
                     if (false == DecrementAssignmentCounts(i))
                     {
                        Logger.Log(LogEnum.LE_ERROR, "RemoveCasualties(): 2-DecrementAssignmentCounts() returned false");
                        return false;
                     }
                  }
               }
               else
               {
                  if (true == mi1.IsFickle)
                     results2.Add(mi1);
               }
            }
         }
         //-------------------------------------
         foreach (IMapItem mi in results1)
            myAssignables.Remove(mi);
         foreach (IMapItem mi in results2)
            myUnassignables.Remove(mi);
         return true;
      }
      private bool SetWounds(int i, int dieRoll)
      {
         if (i < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetWounds(): i < 0");
            return false;
         }
         if (myMaxRowCount <= i)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetWounds(): myMaxRowCount <= i");
            return false;
         }
         IMapItem attacker = null;
         IMapItem defender = null;
         if (2 == myColumnAssignable)
         {
            attacker = myGridRows[i].myUnassignable;
            defender = myGridRows[i].myAssignable;
         }
         else
         {
            attacker = myGridRows[i].myAssignable;
            defender = myGridRows[i].myUnassignable;
         }
         if (null == attacker)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetWounds(): attacker=null");
            return false;
         }
         if (null == defender)
         {
            Logger.Log(LogEnum.LE_ERROR, "SetWounds(): defender=null");
            return false;
         }
         //---------------------------------
         if ((true == defender.Name.Contains("Wizard")) && (true == myIsWizardEscape)) // e023 - no damage to escaping wizard
         {
            myGridRows[i].myResult = 0;
            myGridRows[i].myDamage = WIZARD_ESCAPE;
            return true;
         }
         //---------------------------------
         int wound = 0;
         if (0 < myFireballDamage)
         {
            if (attacker.Name == myEncounteredWizard.Name)
               myGridRows[i].myResult = myFireballDamage;
            else
               attacker = myEncounteredWizard;
            wound = myFireballDamage;
         }
         else
         {
            myGridRows[i].myResult = 0;
            myGridRows[i].myResult += myGridRows[i].myModifier;
            myGridRows[i].myResult += dieRoll;
            switch (myGridRows[i].myResult)
            {
               case -1:
               case 1:
               case 3:
               case 5:
               case 8:
               case 11:
               case 17:
                  wound = 1;
                  break;
               case 10:
               case 12:
               case 13:
                  wound = 2;
                  break;
               case 14:
                  wound = 3;
                  break;
               case 16:
               case 18:
               case 19:
                  wound = 5;
                  break;
               case 20:
               case 21:
               case 22:
               case 23:
               case 24:
               case 25:
               case 26:
               case 27:
               case 28:
               case 29:
                  wound = 6;
                  break;
               default:
                  break; // no wound
            }
            if (0 == wound) // do noting if there are no wound
               return true;
         }
         //--------------------------------------
         int poison = 0;
         if (true == defender.Name.Contains("Spectre")) // e034 Sectre
         {
            bool isMagicSword = attacker.IsSpecialItemHeld(SpecialEnum.MagicSword);
            bool isSpecialist = (true == attacker.Name.Contains("Priest")) || (true == attacker.Name.Contains("Monk")) || (true == attacker.Name.Contains("Wizard")) || (true == attacker.Name.Contains("Magician")) || (true == attacker.Name.Contains("Witch"));
            if ( (true == isMagicSword) || (true == isSpecialist) || (true == myIsTalismanActivated) )
            {
               if (true == attacker.IsPoisonApplied)
                  poison = 2 * wound;
               else
                  poison = wound;
            }
            else
            {
               myGridRows[i].myDamage = SPECTRE_REQUIRES_MAGIC;  // Spectre only affected by magic - indicate not able to cause wounds
            }
            wound = 0;
         }
         else if (true == attacker.Name.Contains("Spider")) // e074 
         {
            poison = wound; // all wounds are poison
            wound = 0;
         }
         else
         {
            if (true == attacker.IsPoisonApplied)  // e185 Poison Drug causes one poison wound for every regular wound
               poison = wound;
         }
         if ((0 == wound) && (0 == poison))
            return true;
         //--------------------------------------
         if (true == defender.IsSpecialItemHeld(SpecialEnum.ResistanceRing))
         {
            Logger.Log(LogEnum.LE_COMBAT_STATE, "SetWounds(): " + myState.ToString() + "-->APPLY_RING");
            myState = CombatEnum.APPLY_RING; // ring may deflect wounds
            myGridRows[i].myWoundsPending = wound;
            myGridRows[i].myPoisonPending = poison;
            Logger.Log(LogEnum.LE_COMBAT_RESULT, "SetWounds(): wp=" + myGridRows[i].myWoundsPending.ToString() + " pp=" + myGridRows[i].myPoisonPending.ToString() );
         }
         else
         {
            if (0 < myFireballDamage)
               myGridRows[i].myDamageFireball = wound;
            else if (NO_EFFECT_THIS_ATTACK != myGridRows[i].myDamage)
               myGridRows[i].myDamage = (wound + poison);
            defender.SetWounds(wound, poison);
            Logger.Log(LogEnum.LE_COMBAT_RESULT, "SetWounds(): defender=" + defender.Name + " row[" + i.ToString() + "]=" + myGridRows[i].myDamage.ToString() + " w=" + wound.ToString() + " p=" + poison.ToString());
            if (("Prince" == defender.Name) && (0 < myGridRows[i].myDamage))
            {
               if (false == UpdatePrinceEndurance())
               {
                  Logger.Log(LogEnum.LE_ERROR, "SetWounds(): UpdatePrinceEndurance() returned false");
                  return false;
               }
            }
            else if (("Prince" == attacker.Name) && (true == defender.IsKilled))
            {
               ++myGameInstance.NumMonsterKill; // e161e - need to kill 5 monsters to seek audience with count
            }
         }
         return true;
      }
      private bool UpdatePrinceEndurance()
      {
         int healthRemaining = myGameInstance.Prince.Endurance - myGameInstance.Prince.Wound - myGameInstance.Prince.Poison;
         foreach (UIElement ui in myStackPanelPrinceEndurance.Children)
         {
            if (ui is Button b)
            {
               if (healthRemaining.ToString() == (string)b.Content)
               {
                  b.IsEnabled = true;
                  b.Background = Utilities.theBrushControlButton;
                  b.FontWeight = FontWeights.Bold;
               }
               else if ((0 == healthRemaining) && ("dead" == (string)b.Content))
               {
                  b.IsEnabled = true;
                  b.Background = Utilities.theBrushControlButton;
                  b.FontWeight = FontWeights.Bold;
               }
               else if ((1 == healthRemaining) && ("unc." == (string)b.Content))
               {
                  b.IsEnabled = true;
                  b.Background = Utilities.theBrushControlButton;
                  b.FontWeight = FontWeights.Bold;
               }
               else
               {
                  b.IsEnabled = false;
                  b.FontWeight = FontWeights.Normal;
               }
            }
            else
            {
               continue;  // Ignore the label
            }
         }
         return true;
      }
      private bool IsDrugShown()
      {
         if (true == myIsDrugResultsStarted)
            return false;
         foreach (IMapItem mi in myGameInstance.PartyMembers)
         {
            if ((true == mi.IsUnconscious) || (true == mi.IsKilled))
               continue;
            foreach (SpecialEnum possession in mi.SpecialKeeps)
            {
               if (SpecialEnum.PoisonDrug == possession)
                  return true;
            }
            foreach (SpecialEnum possession in mi.SpecialShares)
            {
               if (SpecialEnum.PoisonDrug == possession)
                  return true;
            }
         }
         return false;
      }
      private bool IsShieldShown()
      {
         if (true == myIsShieldResultsStarted)
            return false;
         foreach (IMapItem mi in myGameInstance.PartyMembers)
         {
            if ((true == mi.IsUnconscious) || (true == mi.IsKilled))
               continue;
            foreach (SpecialEnum possession in mi.SpecialKeeps)
            {
               if (SpecialEnum.ShieldOfLight == possession)
                  return true;
            }
            foreach (SpecialEnum possession in mi.SpecialShares)
            {
               if (SpecialEnum.ShieldOfLight == possession)
                  return true;
            }
         }
         return false;
      }
      private bool IsTalismanShown()
      {
         if (true == myIsTalismanActivated)
            return false;
         IMapItems mapItems = myGameInstance.EncounteredMembers;
         if ( true == ( (true == myGameInstance.IsInMapItems("Spectre", mapItems)) || (true == myGameInstance.IsInMapItems("Wizard", mapItems)) ) )
         {
            if( true == myGameInstance.IsSpecialItemHeld(SpecialEnum.ResistanceTalisman))
               return true;
         }
         return false;
      }
      private bool IsNerveGasBombShown()
      {
         if (true == myIsSurprise)
         {
            foreach (IMapItem mi in myGameInstance.PartyMembers)
            {
               if ((true == mi.IsUnconscious) || (true == mi.IsKilled))
                  continue;
               foreach (SpecialEnum possession in mi.SpecialKeeps)
               {
                  if (SpecialEnum.NerveGasBomb == possession)
                  {
                     myNerveGasOwner = mi;
                     return true;
                  }
               }
               foreach (SpecialEnum possession in mi.SpecialShares)
               {
                  if (SpecialEnum.NerveGasBomb == possession)
                  {
                     myNerveGasOwner = mi;
                     return true;
                  }
               }
            }
         }
         return false;
      }
      private bool IsDrugOwned(IMapItem mi)
      {
         if ((true == mi.IsUnconscious) || (true == mi.IsKilled))
            return false;
         foreach (SpecialEnum possession in mi.SpecialKeeps)
         {
            if (SpecialEnum.PoisonDrug == possession)
               return true;
         }
         return false;
      }
      private bool IsShieldOwned(IMapItem mi)
      {
         if ((true == mi.IsUnconscious) || (true == mi.IsKilled))
            return false;
         foreach (SpecialEnum possession in mi.SpecialKeeps)
         {
            if (SpecialEnum.ShieldOfLight == possession)
               return true;
         }
         return false;
      }
      private bool IsDrugShared(IMapItem mi)
      {
         if ((true == mi.IsUnconscious) || (true == mi.IsKilled))
            return false;
         foreach (SpecialEnum possession in mi.SpecialShares)
         {
            if (SpecialEnum.PoisonDrug == possession)
               return true;
         }
         return false;
      }
      private bool IsShieldShared(IMapItem mi)
      {
         if ((true == mi.IsUnconscious) || (true == mi.IsKilled))
            return false;
         foreach (SpecialEnum possession in mi.SpecialShares)
         {
            if (SpecialEnum.ShieldOfLight == possession)
               return true;
         }
         return false;
      }
      private bool IsAllDefendersDead()
      {
         foreach (IMapItem mi in myGameInstance.EncounteredMembers) // if all defenders are dead, move to end of round
         {
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsAllDefendersDead(): defender=null");
               continue;
            }
            if ((false != mi.IsUnconscious) && (false != mi.IsKilled))
               return false;
         }
         return true;
      }
      private bool IsEnemyWizardAttacking(int rowNum)
      {
         int i = rowNum - STARTING_ASSIGNED_ROW;
         if (i < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEnemyWizardAttacking(): 0 > i=" + i.ToString());
            return false;
         }
         IMapItem attacker = null;
         if (2 == myColumnAssignable)
            attacker = myGridRows[i].myUnassignable;
         else
            attacker = myGridRows[i].myAssignable;
         if (null == attacker)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEnemyWizardAttacking(): attacker=null");
            return false;
         }
         if ((true == attacker.Name.Contains("Wizard")) && (false == attacker.IsUnconscious) && (false == attacker.IsKilled)) // only return
         {
            Logger.Log(LogEnum.LE_COMBAT_WIZARD, "IsEnemyWizardAttacking(): myIsWizardFight=" + myIsWizardFight.ToString() + " attacker=" + attacker.Name + " myWizardFireballRoundNum=" + myWizardFireballRoundNum.ToString() + " myRoundNum=" + myRoundNum.ToString());
            if ((false == myIsTalismanActivated) && (true == myIsWizardFight) && (myWizardFireballRoundNum != myRoundNum))
               return true;
         }
         return false;
      }
      private bool ApplyWizardFireballAttack()
      {
         for (int j = 0; j < myMaxRowCount; ++j)
         {
            IMapItem defender = null;
            if (0 == myColumnAssignable)
               defender = myGridRows[j].myUnassignable;
            else
               defender = myGridRows[j].myAssignable;
            Logger.Log(LogEnum.LE_COMBAT_RESULT, "ApplyWizardFireballAttack(): setWounds(" + myFireballDamage.ToString() + ") for mi=" + defender.Name + " myDamageFireball=" + myGridRows[j].myDamageFireball);
            if (Utilities.NO_RESULT < myGridRows[j].myDamageFireball)
               continue;
            if (false == SetWounds(j, myFireballDamage)) // nominal case - die roll sets wounds
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowCombatResults(): SetWounds() returned false");
               return false;
            }
            if (CombatEnum.APPLY_RING == myState)
            {
               Logger.Log(LogEnum.LE_COMBAT_RESULT, "ApplyWizardFireballAttack(): APPLY_RING for mi=" + defender.Name);
               myRollResultRowNum = j + STARTING_ASSIGNED_ROW;
               return true;
            }
         }
         myFireballDamage = 0;
         return true;
      }
      //-----------------------------------------------------------------------------------------
      public void ShowCombatResults(int dieRoll)
      {
         int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
         if (i < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowCombatResults(): 0 > i=" + i.ToString());
            return;
         }
         if (false == SetWounds(i, dieRoll)) // nominal case - die roll sets wounds
            Logger.Log(LogEnum.LE_ERROR, "ShowCombatResults(): SetWounds() returned false");
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowCombatResults(): UpdateGrid() return false");
         myIsRollInProgress = false;
      }
      public void ShowEscapeResults(int dieRoll)
      {
         Logger.Log(LogEnum.LE_COMBAT_STATE_ESCAPE, "ShowEscapeResults(): s=" + myState.ToString());
         if (3 < dieRoll) // if 4+, escape enemy
         {
            foreach (IMapItem mi in myNonCombatants) // ShowEscapeResults() -return noncombatants to party
               myGameInstance.PartyMembers.Add(mi);
            //-------------------------------
            if (true == myIsSpiderFight)  // ShowEscapeResults()
            {
               foreach (IMapItem mi in myGameInstance.PartyMembers)   // spiders reduce combat by one due to webs - need to reset
                  ++mi.Combat;
            }
            //-------------------------------
            if (null != myCatVictim) // ShowEscapeResults() - victim does not escape
            {
               if (false == myGameInstance.RemoveVictimInParty(myCatVictim))
               {
                  Logger.Log(LogEnum.LE_ERROR, "UpdateCombatEnd(): RemoveVictimInParty() returned false");
                  return;
               }
            }
            //-------------------------------
            if( "e164" == myGameInstance.EventStart) // if fighing giant lizard, only mounted can escape
            {
               IMapItems adbandonedMembers = new MapItems();
               foreach(IMapItem partyMember in myGameInstance.PartyMembers)
               {
                  if ( (false == partyMember.IsFlying) && (false == partyMember.IsRiding) )
                     adbandonedMembers.Add(partyMember);
               }
               IMapItems fickleMembers = new MapItems();
               foreach (IMapItem partyMember in myGameInstance.PartyMembers)
               {
                  if ( (true == partyMember.IsFickle) && (0 < adbandonedMembers.Count) )
                     fickleMembers.Add(partyMember);
               }
               foreach (IMapItem adbandoned in adbandonedMembers)
                  myGameInstance.RemoveAbandonedInParty(adbandoned); // no wealth or possessions are transferred
               foreach (IMapItem fickle in fickleMembers)
                  myGameInstance.RemoveAbandonerInParty(fickle);  // all wealth or possessions are transferred
            }
            //-------------------------------
            if ("e050" == myGameInstance.EventStart) // if escaping from constable, all fugitives leave
            {
               IMapItems adbandonedMembers = new MapItems();
               foreach (IMapItem partyMember in myGameInstance.PartyMembers)
               {
                  if (true == partyMember.IsFugitive)
                     adbandonedMembers.Add(partyMember);
               }
               IMapItems fickleMembers = new MapItems();
               foreach (IMapItem partyMember in myGameInstance.PartyMembers)
               {
                  if ((true == partyMember.IsFickle) && (0 < adbandonedMembers.Count))
                     fickleMembers.Add(partyMember);
               }
               foreach (IMapItem adbandoned in adbandonedMembers)
                  myGameInstance.RemoveAbandonedInParty(adbandoned); // no wealth or possessions are transferred
               foreach (IMapItem fickle in fickleMembers)
                  myGameInstance.RemoveAbandonerInParty(fickle);  // all wealth or possessions are transferred
            }
            //-------------------------------
            myIsEscape = true;
            myIsRoute = false;
            if (false == SetStateIfItemUsed())
            {
               if (null == myCallback)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ShowEscapeResults(): myCallback=null");
                  return;
               }
               if (false == myCallback(myIsRoute, myIsEscape))  // ShowEscapeResults()
                  Logger.Log(LogEnum.LE_ERROR, "ShowEscapeResults(): myCallback=null");
               return;
            }
            if (false == ResetGridForNonCombat(myGameInstance.PartyMembers))
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowEscapeResults(): ResetGridForNonCombat()=false");
               return;
            }
         }
         else
         {
            if (CombatEnum.ASSIGN == myState)
            {
               if ((true == myIsPartyMembersAssignable) && (0 == myColumnAssignable)) myColumnAssignable = 2; else myColumnAssignable = 0;
               Logger.Log(LogEnum.LE_COMBAT_STATE, "ShowEscapeResults(): " + myState.ToString() + "-->ASSIGN_AFTER_ESCAPE " + myBattleEnum.ToString() + "-->RCTR");
               myState = CombatEnum.ASSIGN_AFTER_ESCAPE;
               myBattleEnum = BattleEnum.RCTR;
               myBattleEnumInitial = BattleEnum.R304;
            }
            else
            {
               Logger.Log(LogEnum.LE_COMBAT_STATE, "ShowEscapeResults(): " + myState.ToString() + "-->SWITCH_ATTACK");
               myState = CombatEnum.SWITCH_ATTACK;
               if( BattleEnum.RCTR == myBattleEnum )
               {
                  int fightCountBefore1 = myGameInstance.PartyMembers.Count + myGameInstance.EncounteredMembers.Count;
                  if (false == RemoveCasualties()) // UpdateGrid() - NEXT_ROUND
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ShowEscapeResults(): RemoveCasualties() returned false");
                     return;
                  }
                  int fightCountAfter1 = myGameInstance.PartyMembers.Count + myGameInstance.EncounteredMembers.Count;
                  if (fightCountBefore1 != fightCountAfter1)
                  {
                     if (false == ResetGridForCombat(myGameInstance.PartyMembers, myGameInstance.EncounteredMembers)) // ShowEscapeResults() - BattleEnum.RCTR
                     {
                        Logger.Log(LogEnum.LE_ERROR, "ShowEscapeResults(): ResetGridForCombat() returned false");
                        return;
                     }
                  }
               }
            }
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowEscapeResults(): UpdateGrid() return false");
      }
      public void ShowRouteResults(int dieRoll)
      {
         myIsRouteOfEnemyPossible = false;   // ShowRouteResults()
         Logger.Log(LogEnum.LE_COMBAT_STATE_ROUTE, "ShowRouteResults(): s=" + myState.ToString() + " route?=" + myIsRouteOfEnemyPossible.ToString());
         if (5 < dieRoll) // if 6+, route enemy
         {
            myIsEscape = false;
            myIsRoute = true;
            //-------------------------------
            if (true == myIsSpiderFight) // ShowRouteResults()
            {
               foreach (IMapItem mi in myGameInstance.PartyMembers)   // spiders reduce combat by one due to webs - need to reset
                  ++mi.Combat;
            }
            //-------------------------------
            foreach (IMapItem mi in myNonCombatants) // ShowRouteResults() - return noncombatants to party
               myGameInstance.PartyMembers.Add(mi);
            foreach (IMapItem mi in myEncounteredSlaveGirls) // return slave girls to party
               myGameInstance.PartyMembers.Add(mi);
            DistributeDeadWealth(); // all wealth of dead encounter members or dead party members goes to party
            //-------------------------------
            if (false == SetStateIfItemUsed()) // hunting cat, boar are not routed so no need to check
            {
               if (null == myCallback)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ShowRouteResults(): myCallback=null");
                  return;
               }
               if (false == myCallback(myIsRoute, myIsEscape)) // ShowRouteResults()
                  Logger.Log(LogEnum.LE_ERROR, "ShowRouteResults(): myCallback=null");
               return;
            }
            if (false == ResetGridForNonCombat(myGameInstance.PartyMembers))
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowRouteResults(): ResetGridForNonCombat()=false");
               return;
            }
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowRouteResults(): UpdateGrid() return false");
      }
      public void ShowFirstStrikeResults(int dieRoll)
      {
         switch (myBattleEnum)
         {
            case BattleEnum.R301:  // Suprise if roll less than or equls W&W - otherwise strike first
               if (dieRoll <= myWitAndWiles)
               {
                  myBattleEnum = BattleEnum.R300;
                  myIsSurprise = true;
               }
               else
               {
                  myBattleEnum = BattleEnum.R304;
               }
               myBattleEnumInitial = BattleEnum.R304;
               break;
            case BattleEnum.R302:  // Suprise if roll less than W&W - otherwise strike first
               if (dieRoll < myWitAndWiles)
               {
                  myBattleEnum = BattleEnum.R300;
                  myIsSurprise = true;
               }
               else
               {
                  myBattleEnum = BattleEnum.R304;
               }
               myBattleEnumInitial = BattleEnum.R304;
               break;
            case BattleEnum.R303:  // Suprise if party size less than roll - otherwise strike first
               int partySize = (true == myIsPartyMembersAssignable) ? myAssignables.Count : myUnassignables.Count;
               if (partySize < myRollResult)
               {
                  myBattleEnum = BattleEnum.R300;
                  myIsSurprise = true;
               }
               else
               {
                  myBattleEnum = BattleEnum.R304;
               }
               myBattleEnumInitial = BattleEnum.R304;
               break;
            case BattleEnum.R305:  // Attack first if roll less or equal to W&W - otherwise encountered strike first
               if (dieRoll <= myWitAndWiles)
               {
                  if (true == myIsPartyMembersAssignable) myColumnAssignable = 0; else myColumnAssignable = 2;
                  myBattleEnum = BattleEnum.R304;
               }
               else
               {
                  if (true == myIsPartyMembersAssignable) myColumnAssignable = 2; else myColumnAssignable = 0;
                  myBattleEnum = BattleEnum.R307;
               }
               myBattleEnumInitial = myBattleEnum;
               break;
            case BattleEnum.R306:  // Attack first if roll less than W&W  - otherwise encountered strike first
               if (dieRoll < myWitAndWiles)
               {
                  if (true == myIsPartyMembersAssignable) myColumnAssignable = 0; else myColumnAssignable = 2;
                  myBattleEnum = BattleEnum.R304;
               }
               else
               {
                  if (true == myIsPartyMembersAssignable) myColumnAssignable = 2; else myColumnAssignable = 0;
                  myBattleEnum = BattleEnum.R307;
               }
               myBattleEnumInitial = myBattleEnum;
               break;
            case BattleEnum.R308:  // Surprised if roll exceeds W&W - otherwise encountered strike first
               if (dieRoll <= myWitAndWiles)
               {
                  myBattleEnum = BattleEnum.R307;
               }
               else
               {
                  myBattleEnum = BattleEnum.R310;
                  myIsSurprised = true;
               }
               myBattleEnumInitial = BattleEnum.R307;
               break;
            case BattleEnum.R309:  // Suprised if Roll equal or exceeds to W&W - otherwise encountered strike first
               if (dieRoll < myWitAndWiles)
               {
                  myBattleEnum = BattleEnum.R307;
               }
               else
               {
                  myBattleEnum = BattleEnum.R310;
                  myIsSurprised = true;
               }
               myBattleEnumInitial = BattleEnum.R307;
               break;
            case BattleEnum.R300:  // Surprise - repeat attack
            case BattleEnum.R304:  // Surprise - repeat attack
            case BattleEnum.R307:  // Attacked
            case BattleEnum.RCTR:  // Counter
            case BattleEnum.R310:  // Surprised - repeat attack
            default:
               Logger.Log(LogEnum.LE_ERROR, "ShowFirstStrikeResults(): reached default for myState=" + myState.ToString() + " for battleEnum=" + myBattleEnum.ToString());
               return;
         }
         //--------------------------------------------------
         if( (true == myIsSurprised) && (null != myNerveGasOwner) )  // e007 - elf can have nerve gas bomb
         {
            Logger.Log(LogEnum.LE_COMBAT_STATE, "PerformCombat(): " + myState.ToString() + "-->APPLY_NERVE_GAS");
            myState = CombatEnum.APPLY_NERVE_GAS;
            if (false == ResetGridForNerveGas(myGameInstance.PartyMembers))
               Logger.Log(LogEnum.LE_ERROR, "PerformCombat(): ResetGridForNerveGas(PartyMembers)=false myState=" + myState.ToString());
            if (false == UpdateGrid())
               Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false myState=" + myState.ToString());
         }
         else
         {
            if (false == SetInitialFightState("ShowFirstStrikeResults()"))
               Logger.Log(LogEnum.LE_ERROR, "ShowFirstStrikeResults(): SetInitialFightState()=false for myState=" + myBattleEnum.ToString());
            if (false == ResetGridForCombat(myGameInstance.PartyMembers, myGameInstance.EncounteredMembers))  // ShowFirstStrikeResults()
               Logger.Log(LogEnum.LE_ERROR, "ShowFirstStrikeResults(): ResetGridForCombat() return false");
            else if (false == UpdateGrid())
               Logger.Log(LogEnum.LE_ERROR, "ShowFirstStrikeResults(): UpdateGrid() return false");
         }
         myIsRollInProgress = false;
      }
      public void ShowDrugEndResults(int dieRoll)
      {
         myIsDrugResultsStarted = true;
         int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
         if (i < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDrugEndResults(): i = " + i.ToString());
            return;
         }
         //----------------------------------------
         // If six is rolled, need to find a Poison Drug to remove
         if (6 == dieRoll)
         {
            IMapItem mi = myGridRows[i].myUnassignable;
            if (false == myGameInstance.RemoveSpecialItem(SpecialEnum.PoisonDrug, mi))
               Logger.Log(LogEnum.LE_ERROR, "ShowDrugEndResults(): RemoveSpecialItem(PoisonDrug) return false w/ i=" + i.ToString());
         }
         //----------------------------------------
         myGridRows[i].myResult = dieRoll;
         myIsRollInProgress = false;
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowDrugEndResults(): UpdateGrid() return false");
      }
      public void ShowShieldEndResults(int dieRoll)
      {
         myIsShieldResultsStarted = true;
         int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
         if (i < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowShieldEndResults(): i = " + i.ToString());
            return;
         }
         //----------------------------------------
         // If six is rolled, need to find a Poison Drug to remove
         if (6 == dieRoll)
         {
            IMapItem mi = myGridRows[i].myUnassignable;
            if (false == myGameInstance.RemoveSpecialItem(SpecialEnum.ShieldOfLight, mi))
               Logger.Log(LogEnum.LE_ERROR, "ShowShieldEndResults(): RemoveSpecialItem(ShieldOfLight) return false w/ i=" + i.ToString());
         }
         //----------------------------------------
         myGridRows[i].myResult = dieRoll;
         myIsRollInProgress = false;
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowShieldEndResults(): UpdateGrid() return false");
      }
      public void ShowRingRollResult(int dieRoll)
      {
         int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
         if (i < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowRingRollResult(): 0 > i=" + i.ToString());
            return;
         }
         IMapItem defender = null;
         if (2 == myColumnAssignable)
            defender = myGridRows[i].myAssignable;
         else
            defender = myGridRows[i].myUnassignable;
         if (null == defender)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowRingRollResult(): attacker=null");
            return;
         }
         //----------------------------------------
         Logger.Log(LogEnum.LE_COMBAT_STATE, "ShowRingRollResult(): " + myState.ToString() + "-->APPLY_RING_SHOW");
         myState = CombatEnum.APPLY_RING_SHOW;
         if (12 == dieRoll)
         {
            if (false == defender.RemoveSpecialItem(SpecialEnum.ResistanceRing))
               Logger.Log(LogEnum.LE_ERROR, "ShowRingRollResult(): RemoveSpecialItem() returned false for mi=" + defender.Name);
            ++myGridRows[i].myWoundsPending;
            if( 0 < myFireballDamage)
               myGridRows[i].myDamageFireball = myGridRows[i].myWoundsPending;
            else
               myGridRows[i].myDamage = myGridRows[i].myWoundsPending + myGridRows[i].myPoisonPending;
            Logger.Log(LogEnum.LE_COMBAT_RESULT, "ShowRingRollResult(): mi=" + defender.Name + " dmg[" + i.ToString() + "]=" + myGridRows[i].myDamage.ToString());
            defender.SetWounds(myGridRows[i].myWoundsPending, myGridRows[i].myPoisonPending);
            if ("Prince" == defender.Name)
            {
               if (false == UpdatePrinceEndurance())
                  Logger.Log(LogEnum.LE_ERROR, "ShowRingRollResult(): UpdatePrinceEndurance() returned false");
            }
         }
         else if (8 < dieRoll)
         {
            if (0 < myGridRows[i].myWoundsPending)
               --myGridRows[i].myWoundsPending;
            if (0 < myFireballDamage)
               myGridRows[i].myDamageFireball = myGridRows[i].myWoundsPending;
            else
               myGridRows[i].myDamage = myGridRows[i].myWoundsPending + myGridRows[i].myPoisonPending;
            Logger.Log(LogEnum.LE_COMBAT_RESULT, "ShowRingRollResult(): mi=" + defender.Name + " dmg[" + i.ToString() + "]=" + myGridRows[i].myDamage.ToString());
            defender.SetWounds(myGridRows[i].myWoundsPending, myGridRows[i].myPoisonPending);
            if (("Prince" == defender.Name) && (0 < myGridRows[i].myDamage))
            {
               if (false == UpdatePrinceEndurance())
                  Logger.Log(LogEnum.LE_ERROR, "ShowRingRollResult(): UpdatePrinceEndurance() returned false");
            }
         }
         else // did not cause damage this turn - talisman deflects damage
         {
            if (0 < myFireballDamage)
               myGridRows[i].myDamageFireball = NO_EFFECT_THIS_ATTACK;
            else
               myGridRows[i].myDamage = NO_EFFECT_THIS_ATTACK;
            Logger.Log(LogEnum.LE_COMBAT_RESULT, "ShowRingRollResult(): mi=" + defender.Name + " dmg[" + i.ToString() + "]=" + myGridRows[i].myDamage.ToString());
         }
         myGridRows[i].myWoundsPending = 0;
         myGridRows[i].myPoisonPending = 0;
         //----------------------------------------
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowRingRollResult(): UpdateGrid() return false");
         myIsRollInProgress = false;
      }
      public void ShowWizardStrikeResult(int dieRoll)
      {
         int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
         if (i < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowWizardStrikeResult(): 0 > i=" + i.ToString());
            return;
         }
         if (CombatEnum.WIZARD_STRIKE == myState)
         {
            myWizardFireballRoundNum = myRoundNum;
            if (4 < dieRoll)
            {
               Logger.Log(LogEnum.LE_COMBAT_STATE, "ShowWizardStrikeResult(): " + myState.ToString() + "-->WIZARD_STRIKE_SHOW");
               myState = CombatEnum.WIZARD_STRIKE_SHOW;
               if (0 < myEncounteredWizard.Wound || 0 < myEncounteredWizard.Poison) // if fireball is used and wizard is wounded, he escapes
                  myIsWizardEscape = true;
            }
            else
            {
               Logger.Log(LogEnum.LE_COMBAT_STATE, "ShowWizardStrikeResult(): " + myState.ToString() + "-->ASSIGN");
               myState = CombatEnum.ASSIGN;
            }
         }
         else if (CombatEnum.WIZARD_STRIKE_SHOW == myState)
         {
            for (int j = 0; j < myMaxRowCount; ++j)
            {
               myGridRows[j].myDamageFireball = Utilities.NO_RESULT; // reset fire damage for all defenders
               IMapItem defender = null;
               if (0 == myColumnAssignable)
                  defender = myGridRows[j].myUnassignable;
               else
                  defender = myGridRows[j].myAssignable;
               defender.IsShowFireball = true; // used to show fireball on counter in IMapItem.ShowButtonContent()
            }
            myFireballDamage = dieRoll; 
            Logger.Log(LogEnum.LE_COMBAT_RESULT, "ShowWizardStrikeResult(): WIZARD_STRIKE_SHOW - entering ApplyWizardFireballAttack()");
            if (false == ApplyWizardFireballAttack())
            {
               Logger.Log(LogEnum.LE_ERROR, "ShowWizardStrikeResult(): ApplyWizardFireballAttack() return false");
               return;
            }
            if ( CombatEnum.APPLY_RING == myState )
            {
               myPreviousCombatState = CombatEnum.STARTED_STRIKES;
            }
            else
            {
               Logger.Log(LogEnum.LE_COMBAT_STATE, "ApplyWizardFireballAttack(): " + myState.ToString() + "-->STARTED_STRIKES");
               myState = CombatEnum.STARTED_STRIKES;
            }
          }
         else
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowWizardStrikeResult(): unknown state=" + myState.ToString());
            return;
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowWizardStrikeResult(): UpdateGrid() return false");
         myIsRollInProgress = false;
      }
      public void ShowMirrorStrikeResult(int dieRoll)
      {
         int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
         if (i < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowMirrorStrikeResult(): 0 > i=" + i.ToString());
            return;
         }
         //----------------------------------------
         Logger.Log(LogEnum.LE_COMBAT_STATE, "ShowMirrorStrikeResult(): " + myState.ToString() + "-->MIRROR_STRIKE_SHOW");
         myState = CombatEnum.MIRROR_STRIKE_SHOW;
         if (4 < dieRoll)
         {
            myGridRows[i].myAttackMirrorState = MirrorTargetEnum.PRINCE_STRIKE;
            myGridRows[i].myAssignable = myGridRows[0].myUnassignable;  // assign to prince
         }
         else
         {
            myGridRows[i].myAttackMirrorState = MirrorTargetEnum.MIRROR_STRIKE;
            myGridRows[i].myAssignable = myGridRows[0].myAssignable;  // assign to mirror
         }
         myGridRows[i].myWoundsPending = 0;
         myGridRows[i].myPoisonPending = 0;
         //----------------------------------------
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowMirrorStrikeResult(): UpdateGrid() return false");
         myIsRollInProgress = false;
      }
      public void ShowKnightStrikeResult(int dieRoll)
      {
         Logger.Log(LogEnum.LE_COMBAT_STATE, "ShowKnightStrikeResult(): " + myState.ToString() + "-->KNIGHT_STRIKE_SHOW");
         myState = CombatEnum.KNIGHT_STRIKE_SHOW;
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowKnightStrikeResult(): UpdateGrid() return false");
         myIsRollInProgress = false;
      }
      public void ShowProtectorResults(int dieRoll)
      {
         Logger.Log(LogEnum.LE_COMBAT_STATE, "ShowProtectorResults(): " + myState.ToString() + "-->ROLL_FOR_PROTECTOR_SHOW");
         myState = CombatEnum.ROLL_FOR_PROTECTOR_SHOW;
         if (2 < dieRoll) // on 3+, the protector crew arrives to help out in battle
         {
            myIsProtectorArriving = false; // do not want to check for protector crew if it already came 
            Option isEasyMonstersOption = myGameInstance.Options.Find("EasyMonsters");
            if (null == isEasyMonstersOption)
               Logger.Log(LogEnum.LE_ERROR, "ShowProtectorResults(): returned option=null");
            string miName = "ProtectorBoss" + Utilities.MapItemNum.ToString();
            ++Utilities.MapItemNum;
            IMapItem protectorBoss = new MapItem(miName, 1.0, false, false, false, "C38ProtectorBoss", "C38ProtectorBoss", myGameInstance.Prince.Territory, 5, 5, 25);
            if (true == isEasyMonstersOption.IsEnabled)
               protectorBoss = new MapItem(miName, 1.0, false, false, false, "C38ProtectorBoss", "C38ProtectorBoss", myGameInstance.Prince.Territory, 1, 1, 25);
            myGameInstance.EncounteredMembers.Add(protectorBoss);
            int numOfMembersInCrew = 4;
            if (true == isEasyMonstersOption.IsEnabled)
               numOfMembersInCrew = 1;
            for (int i = 0; i < numOfMembersInCrew; ++i)
            {
               miName = "Protector" + Utilities.MapItemNum.ToString();
               ++Utilities.MapItemNum;
               IMapItem protectorMan = new MapItem(miName, 1.0, false, false, false, "C39Protector", "C39Protector", myGameInstance.Prince.Territory, 5, 5, 4);
               if (true == isEasyMonstersOption.IsEnabled)
                  protectorMan = new MapItem(miName, 1.0, false, false, false, "C39Protector", "C39Protector", myGameInstance.Prince.Territory, 1, 1, 4);
               myGameInstance.EncounteredMembers.Add(protectorMan);
            }
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowProtectorResults(): UpdateGrid() return false");
         myIsRollInProgress = false;
      }
      public void ShowTalismanEndResults(int dieRoll)
      {
         int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
         if (i < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowDrugEndResults(): i = " + i.ToString());
            return;
         }
         myGridRows[i].myResult = dieRoll;
         for (int j = 0; j < myMaxRowCount; ++j) // Only one can be destroyed per combat - set other rows die result to zero
         {
            if (i == j)
               continue;
            myGridRows[j].myResult = TALISMAN_NOT_USED_BY_THIS_GUY; //ShowTalismanEndResults()
         }
         myIsRollInProgress = false;
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowTalismanEndResults(): UpdateGrid() return false");
      }
      public void ShowNerveGasResults(int dieRoll)
      {
         int i = myRollResultRowNum - STARTING_ASSIGNED_ROW;
         if (i < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowNerveGasResults(): i = " + i.ToString());
            return;
         }
         myGridRows[i].myResult = dieRoll;
         IMapItem defender = myGridRows[i].myUnassignable;
         if (null == defender)
         {
            Logger.Log(LogEnum.LE_ERROR, "ShowNerveGasResults(): defender=null for i=" + i.ToString());
            return;
         }
         //----------------------------------------
         if (6 == dieRoll)
         {
            // do nothing
         }
         else if (5 == dieRoll)
         {
            defender.IsRunAway = true;
         }
         else
         {
            defender.SetWounds(0, defender.Endurance); // encountered is KIA due to nerve gas
         }
         //----------------------------------------
         Logger.Log(LogEnum.LE_COMBAT_STATE, "ShowNerveGasResults(): " + myState.ToString() + "-->APPLY_NERVE_GAS_SHOW");
         myState = CombatEnum.APPLY_NERVE_GAS_SHOW;
         for (int j = 0; j < myMaxRowCount; ++j)
         {
            if (Utilities.NO_RESULT == myGridRows[j].myResult)
            {
               Logger.Log(LogEnum.LE_COMBAT_STATE, "ShowNerveGasResults(): " + myState.ToString() + "-->APPLY_NERVE_GAS");
               myState = CombatEnum.APPLY_NERVE_GAS;
            }
         }
         //----------------------------------------
         myIsRollInProgress = false;
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowNerveGasResults(): UpdateGrid() return false");
      }
      public void ShowHalflingEscape(int dieRoll)
      {
         Logger.Log(LogEnum.LE_COMBAT_STATE, "ShowHalflingEscape(): " + myState.ToString() + "-->ROLL_FOR_HALFLING_SHOW");
         myState = CombatEnum.ROLL_FOR_HALFLING_SHOW;
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ShowHalflingEscape(): UpdateGrid() return false");
         myIsRollInProgress = false;
      }
      // -----------------CONTROLLER FUNCTIONS---------------------------------------------------
      private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
      {
         if( CombatEnum.ROLL_FOR_HALFLING_SHOW == myState )
         {
            if (1 < myRollResultHalfling) // little bugger escapes so easily
            {
               ShowRouteResults(6); // force a route of enemy
               return;
            }
            myRollResultHalfling = Utilities.NO_RESULT;
            Logger.Log(LogEnum.LE_COMBAT_STATE, "Grid_MouseDown(): " + myState.ToString() + "-->NEXT_ROUND");
            myState = CombatEnum.NEXT_ROUND;
            if (false == UpdateGrid())
               Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
            return;
         }
         else if (CombatEnum.WOLF_REMOVES_MOUNT == myState)
         {
            myGameInstance.ReduceMount(MountEnum.Any);
            bool isMountExist = false;
            foreach (IMapItem mi in myGameInstance.PartyMembers)
            {
               if (0 < mi.Mounts.Count)
                  isMountExist = true;
            }
            myIsWolvesFight = isMountExist;
            Logger.Log(LogEnum.LE_COMBAT_STATE, "Grid_MouseDown(): " + myState.ToString() + "-->NEXT_ROUND");
            myState = CombatEnum.NEXT_ROUND;
            if (false == UpdateGrid())
               Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
            return;
         }
         else if (CombatEnum.CAVALRY_STRIKE == myState)
         {
            bool isEnd = true;
            if (false == UpdateCombatEnd(ref isEnd))
               Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateCombatEnd() return false");
            if (false == isEnd)
               Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): invalid state isEnd=false");
            return;
         }
         else if (CombatEnum.END_POISON_SHOW == myState) // Poison removal check finished
         {
            bool isShieldApplied = false;
            foreach (IMapItem mi in myGameInstance.PartyMembers) // if shield is applied, need to check for removal
            {
               if (true == mi.IsShieldApplied)
                  isShieldApplied = true;
            }
            if ((true == isShieldApplied) && (false == myIsShieldResultsEnded))
            {
               Logger.Log(LogEnum.LE_COMBAT_STATE, "ShowNerveGasResults(): " + myState.ToString() + "-->END_SHIELD");
               myState = CombatEnum.END_SHIELD;
               if (false == ResetGridForNonCombat(myGameInstance.PartyMembers))
                  Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): ResetGridForNonCombat()=false");
               if (false == UpdateGrid())
                  Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
            }
            else if (true == myIsTalismanActivated)
            {
               Logger.Log(LogEnum.LE_COMBAT_STATE, "ShowNerveGasResults(): " + myState.ToString() + "-->END_TALISMAN");
               myState = CombatEnum.END_TALISMAN;
               if (false == ResetGridForNonCombat(myGameInstance.PartyMembers))
                  Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): ResetGridForNonCombat()=false");
               if (false == UpdateGrid())
                  Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
            }
            else // this is called after UpdateCombatEnd() finished
            {
               if (null == myCallback)
                  Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): myCallback=null");
               else if (false == myCallback(myIsRoute, myIsEscape))  // GridMouseDown() - End poison show
                  Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): prince.IsKilled myCallback() returned false");
            }
            return;
         }
         else if (CombatEnum.END_SHIELD_SHOW == myState) // Shield removal check finished
         {
            bool isDrugApplied = false;
            foreach (IMapItem mi in myGameInstance.PartyMembers) // if poison is applied (see e185), need to check for removal
            {
               if (true == mi.IsPoisonApplied)
                  isDrugApplied = true;
            }
            if ((true == isDrugApplied) && (false == myIsDrugResultsEnded))
            {
               Logger.Log(LogEnum.LE_COMBAT_STATE, "Grid_MouseDown(): " + myState.ToString() + "-->END_POISON");
               myState = CombatEnum.END_POISON;
               if (false == ResetGridForNonCombat(myGameInstance.PartyMembers))
                  Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): ResetGridForNonCombat()=false");
               if (false == UpdateGrid())
                  Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
            }
            else if (true == myIsTalismanActivated)
            {
               Logger.Log(LogEnum.LE_COMBAT_STATE, "Grid_MouseDown(): " + myState.ToString() + "-->END_TALISMAN");
               myState = CombatEnum.END_TALISMAN;
               if (false == ResetGridForNonCombat(myGameInstance.PartyMembers))
                  Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): ResetGridForNonCombat()=false");
               if (false == UpdateGrid())
                  Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
            }
            else // this is called after UpdateCombatEnd() finished
            {
               if (null == myCallback)
                  Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): myCallback=null");
               else if (false == myCallback(myIsRoute, myIsEscape)) // GridMouseDown() - End shield show
                  Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): prince.IsKilled myCallback() returned false");
            }
            return;
         }
         else if (CombatEnum.END_TALISMAN_SHOW == myState) // Talisman removal check finished
         {
            if (null == myCallback)
               Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): myCallback=null");
            else if (false == myCallback(myIsRoute, myIsEscape)) // GridMouseDown() - End talisman show
               Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): prince.IsKilled myCallback() returned false");
            return;
         }
         else if (CombatEnum.APPLY_NERVE_GAS_SHOW == myState) // Spider, Boars, Hunting Cat cannot be suprised - no need to check for special finishes
         {
            if (null == myNerveGasOwner) // e007 - Nerve gas delivered by Elf 
            {
               Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): myNerveGasOwner=null when showing APPLY_NSERVE_GAS_SHOW");
               return;
            }
            myNerveGasOwner.RemoveSpecialItem(SpecialEnum.NerveGasBomb);
            Logger.Log(LogEnum.LE_COMBAT_STATE, "ShowNerveGasResults(): " + myState.ToString() + "-->APPLY_NERVE_GAS_NEXT");
            myState = CombatEnum.APPLY_NERVE_GAS_NEXT;
            if (false == UpdateHeader()) // Removes causalties and changes to new state
            {
               Logger.Log(LogEnum.LE_ERROR, "UpdateGrid(): UpdateHeader() returned false");
               return;
            }
            //---------------------------------------
            bool isEnd = false;
            if (false == UpdateCombatEnd(ref isEnd))   // Grid_MouseDown() - APPLY_NERVE_GAS_SHOW
            {
               Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateCombatEnd() returned false"); 
               return;
            }
            if (true == isEnd)
               return;
            //---------------------------------------
            if (false == SetInitialFightState("Grid_MouseDown() - Nerve Gas Attack"))
               Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): SetInitialFightState()=false");
            if (false == ResetGridForCombat(myGameInstance.PartyMembers, myGameInstance.EncounteredMembers)) // Grid_MouseDown() - APPLY_NERVE_GAS_SHOW
               Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): ResetGridForCombat() return false");
            else if (false == UpdateGrid())
               Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
            return;
         }
         else if (CombatEnum.SHOW_LAST_STRIKE == myState)
         {
            Logger.Log(LogEnum.LE_COMBAT_STATE, "Grid_MouseDown(): " + myState.ToString() + "-->SWITCH_ATTACK");
            myState = CombatEnum.SWITCH_ATTACK;
            if (false == UpdateGrid())
               Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
            return;
         }
         else if (CombatEnum.SHOW_LAST_COUNTER == myState)
         {
            if (0 == myRoundNum % 2) // on even rounds
            {
               IMapItems mapItems = null;
               if (false == myIsPartyMembersAssignable)
                  mapItems = myAssignables;
               else
                  mapItems = myUnassignables;
               foreach (IMapItem mi in mapItems)
               {
                  if ((false == mi.IsKilled) && (true == mi.Name.Contains("Troll")))  // e057 - Trolls heal one wound every even round
                  {
                     mi.HealWounds(1, 0);
                     Logger.Log(LogEnum.LE_COMBAT_TROLL_HEAL, "Grid_MouseDown(): troll=" + mi.Wound.ToString());
                  }
               }
            }
            if (true == myIsHalflingFight)
            {
               IMapItem halfling = null;
               if (true == myIsPartyMembersAssignable) // determine if halfling is dead or unconscious
               {
                  if( 0 < myUnassignables.Count )
                     halfling = myUnassignables[0]; 
               }
               else
               {
                  if (0 < myAssignables.Count)
                     halfling = myAssignables[0];
               }
               if( null != halfling)
               {
                  if ((false == halfling.IsKilled) && (false == halfling.IsUnconscious)) // if not dead or unconscious, halfling tries to escape
                  {
                     Logger.Log(LogEnum.LE_COMBAT_STATE, "Grid_MouseDown(): " + myState.ToString() + "-->ROLL_FOR_HALFLING");
                     myState = CombatEnum.ROLL_FOR_HALFLING;
                  }
               }
            }
            else if (true == myIsWolvesFight)
            {
               bool isWolfAlive = false;
               foreach (IMapItem mi in myGameInstance.EncounteredMembers)
               {
                  if ((false == mi.IsKilled) && (false == mi.IsUnconscious))
                     isWolfAlive = true;
               }
               if (true == isWolfAlive)
               {
                  myIsWolvesFight = false;
                  Logger.Log(LogEnum.LE_COMBAT_STATE, "Grid_MouseDown(): " + myState.ToString() + "-->WOLF_REMOVES_MOUNT");
                  myState = CombatEnum.WOLF_REMOVES_MOUNT;
               }
               else
               {
                  Logger.Log(LogEnum.LE_COMBAT_STATE, "Grid_MouseDown(): " + myState.ToString() + "-->NEXT_ROUND");
                  myState = CombatEnum.NEXT_ROUND;
               }
            }
            else
            {
               Logger.Log(LogEnum.LE_COMBAT_STATE, "Grid_MouseDown(): " + myState.ToString() + "-->NEXT_ROUND");
               myState = CombatEnum.NEXT_ROUND;
            }
            if (false == UpdateGrid())
               Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
            return;
         }
         else if (CombatEnum.ROLL_FOR_PROTECTOR_SHOW == myState)
         {
            Logger.Log(LogEnum.LE_COMBAT_STATE, "Grid_MouseDown(): " + myState.ToString() + "-->FINALIZE_BATTLE_STATE");
            myState = CombatEnum.FINALIZE_BATTLE_STATE;
            if (true == myIsPartyMembersAssignable) myColumnAssignable = 2; else myColumnAssignable = 0;  // protectors are always first in combat
            if (false == UpdateGrid())
               Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
            return;
         }
         else if (CombatEnum.KNIGHT_STRIKE_SHOW == myState)
         {
            Logger.Log(LogEnum.LE_COMBAT_STATE, "Grid_MouseDown(): " + myState.ToString() + "-->ASSIGN_STRIKES");
            myState = CombatEnum.ASSIGN_STRIKES;
            if (false == ResetGridForCombat(myGameInstance.PartyMembers, myGameInstance.EncounteredMembers))  // Grid_MouseDown() - KNIGHT_STRIKE_SHOW
            {
               Logger.Log(LogEnum.LE_ERROR, "PerformCombat(): ResetGridForCombat() return false");
               return ;
            }
            if (false == UpdateGrid())
               Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
            return;
         }
         //------------------------------------------------------------------------------
         System.Windows.Point p = e.GetPosition((UIElement)sender);
         HitTestResult result = VisualTreeHelper.HitTest(myGrid, p);  // Get the Point where the hit test occurs
         foreach (UIElement ui in myGrid.Children)
         {
            if (null != myMapItemDragged) // If dragging something, check if dragged to rectangle either in StackPanel or GridRow
            {
               if (ui is StackPanel panel)  // First check all rectangles in the myStackPanelAssignable
               {
                  foreach (UIElement ui1 in panel.Children)
                  {
                     if (ui1 is Rectangle rect)
                     {
                        if (result.VisualHit == rect)
                        {
                           myGrid.Cursor = Cursors.Arrow;
                           myMapItemDragged = null;
                           if (false == UpdateGrid())
                              Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                           return;
                        }
                     }
                  }
               }
               else if (ui is Rectangle rect) // next check all rectangles in the grid rows
               {
                  if (result.VisualHit == rect)
                  {
                     myGrid.Cursor = Cursors.Arrow;
                     int rowNum = Grid.GetRow(rect);
                     int i = rowNum - STARTING_ASSIGNED_ROW;
                     myGridRows[i].myAssignable = myMapItemDragged;
                     myGridRows[i].myAssignmentCount = GetAssignedCount(myMapItemDragged.Name);
                     myMapItemDragged = null;
                     SetRemainingAssignments();
                     if (false == UpdateGrid())
                        Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                     return;
                  }
               }
            } // end if (null != myMapItemDragged)
            else
            {
               if (ui is StackPanel panel) // Check all images within the myStackPanelAssignable
               {
                  foreach (UIElement ui1 in panel.Children)
                  {
                     if (ui1 is Image img)
                     {
                        if (result.VisualHit == img)
                        {
                           string name = (string)img.Name;
                           if ("CrossedSwords" == name)
                           {
                              if (true == myIsKnightOnBridge)
                              {
                                 Logger.Log(LogEnum.LE_COMBAT_STATE, "Grid_MouseDown(): " + myState.ToString() + "-->KNIGHT_STRIKE");
                                 myState = CombatEnum.KNIGHT_STRIKE;
                              }
                              else
                              {
                                 Logger.Log(LogEnum.LE_COMBAT_STATE, "Grid_MouseDown(): " + myState.ToString() + "-->FINALIZE_BATTLE_STATE");
                                 myState = CombatEnum.FINALIZE_BATTLE_STATE;
                              }
                              if (false == ResetGridForCombat(myGameInstance.PartyMembers, myGameInstance.EncounteredMembers)) // Grid_MouseDown() - CrossedSwords
                              {
                                 Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): ResetGridForCombat() return false");
                                 return;
                              }
                           }
                           else if ("PoisonDrug" == name)
                           {
                              Logger.Log(LogEnum.LE_COMBAT_STATE, "Grid_MouseDown(): " + myState.ToString() + "-->APPLY_POISON");
                              myState = CombatEnum.APPLY_POISON;
                              if (false == ResetGridForNonCombat(myGameInstance.PartyMembers))
                              {
                                 Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): ResetGridForNonCombat()=false myState=" + myState.ToString()); ;
                                 return;
                              }
                           }
                           else if ("Shield" == name)
                           {
                              Logger.Log(LogEnum.LE_COMBAT_STATE, "Grid_MouseDown(): " + myState.ToString() + "-->APPLY_SHIELD");
                              myState = CombatEnum.APPLY_SHIELD;
                              if (false == ResetGridForNonCombat(myGameInstance.PartyMembers))
                              {
                                 Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): ResetGridForNonCombat()=false myState=" + myState.ToString()); ;
                                 return;
                              }
                           }
                           else if ("NerveGasBomb" == name)
                           {
                              Logger.Log(LogEnum.LE_COMBAT_STATE, "Grid_MouseDown(): " + myState.ToString() + "-->APPLY_NERVE_GAS");
                              myState = CombatEnum.APPLY_NERVE_GAS;
                              if (false == ResetGridForNerveGas(myGameInstance.EncounteredMembers))
                              {
                                 Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): ResetGridForNerveGas(EncounteredMembers)=false myState=" + myState.ToString());
                                 return;
                              }
                           }
                           else if (("TalismanResistance" == name) || ("Lightening" == name))
                           {
                              myIsTalismanActivated = true;
                              if(CombatEnum.WIZARD_STRIKE == myState)
                              {
                                 Logger.Log(LogEnum.LE_COMBAT_STATE, "Grid_MouseDown(): " + myState.ToString() + "-->STARTED_STRIKES");
                                 myState = CombatEnum.STARTED_STRIKES;
                                 myIsRollInProgress = false;
                              }
                           }
                           else if ("FirstStrikeDie" == name)
                           {
                              if (false == myIsRollInProgress)
                              {
                                 myIsRollInProgress = true;
                                 myRollResult = myDieRoller.RollMovingDie(myCanvas, ShowFirstStrikeResults);
                                 img.Visibility = Visibility.Hidden;
                                 return;
                              }
                           }
                           else if ("Teeth" == name)
                           {
                              if (false == myGameInstance.RemoveSpecialItem(SpecialEnum.HydraTeeth))
                                 Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): RemoveSpecialItem(SpecialEnum.HydraTeeth) returned false");
                              for (int k = 0; k < myGameInstance.HydraTeethCount; ++k)
                              {
                                 string nameUndead = "Undead" + k.ToString();
                                 IMapItem undeadWarrior = new MapItem(nameUndead, 1.0, false, false, false, "c32UndeadWarrior", "c32UndeadWarrior", myGameInstance.Prince.Territory, 4, 5, 0);
                                 myGameInstance.PartyMembers.Add(undeadWarrior);
                              }
                              myGameInstance.HydraTeethCount = 0;
                              if (false == ResetGridForCombat(myGameInstance.PartyMembers, myGameInstance.EncounteredMembers)) // Grid_MouseDown() - Teeth
                                 Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): ResetGridForCombat() returned false");
                              else if (false == UpdateGrid())
                                 Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                           }
                           else if ("RingRoll" == name)
                           {
                              myIsRollInProgress = true;
                              myRollResultRing = myDieRoller.RollMovingDice(myCanvas, ShowRingRollResult);
                              img.Visibility = Visibility.Hidden;
                              return;
                           }
                           else if ("WizardStrike" == name)
                           {
                              myIsRollInProgress = true;
                              myRollResultMirror = myDieRoller.RollMovingDie(myCanvas, ShowWizardStrikeResult);
                              img.Visibility = Visibility.Hidden;
                              return;
                           }
                           else if ("MirrorStrike" == name)
                           {
                              myIsRollInProgress = true;
                              myRollResultMirror = myDieRoller.RollMovingDie(myCanvas, ShowMirrorStrikeResult);
                              img.Visibility = Visibility.Hidden;
                              return;
                           }
                           else if ("KnightStrike" == name)
                           {
                              myIsRollInProgress = true;
                              myRollResultKnight = myDieRoller.RollMovingDie(myCanvas, ShowKnightStrikeResult);
                              img.Visibility = Visibility.Hidden;
                              return;
                           }
                           else if ("Protector" == name)
                           {
                              myIsRollInProgress = true;
                              myRollResultMirror = myDieRoller.RollMovingDie(myCanvas, ShowProtectorResults);
                              img.Visibility = Visibility.Hidden;
                              return;
                           }
                           else if ("HalflingRoll" == name)
                           {
                              myIsRollInProgress = true;
                              myRollResultHalfling = myDieRoller.RollMovingDie(myCanvas, ShowHalflingEscape);
                              img.Visibility = Visibility.Hidden;
                              return;
                           }
                           else if ("Nothing" == name)
                           {
                              return;
                           }
                           if (false == UpdateGrid())
                              Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false myState=" + myState.ToString());
                           return;
                        }
                     }
                  }
               }
               else if (ui is Image img) // next check all images in grid rows
               {
                  if (result.VisualHit == img)
                  {
                     if (CombatEnum.APPLY_POISON == myState)
                     {
                        if (DragStateEnum.NONE == myDragState)
                        {
                           myDragStateRowNum = Grid.GetRow(img);
                           myDragStateColNum = Grid.GetColumn(img);
                           int i = myDragStateRowNum - STARTING_ASSIGNED_ROW;
                           if (i < 0)
                           {
                              myDragStateRowNum = 0;
                              myDragStateColNum = 0;
                              Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): invalid index=" + i.ToString());
                              return;
                           }
                           IMapItem mi = myGridRows[i].myUnassignable;
                           if (null == mi)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): invalid mi=null for i=" + i.ToString());
                              return;
                           }
                           if (2 == myDragStateColNum)
                           {
                              mi.SpecialKeeps.Remove(SpecialEnum.PoisonDrug);
                              myDragState = DragStateEnum.KEEPER_DRUG;
                           }
                           else if (3 == myDragStateColNum)
                           {
                              mi.SpecialShares.Remove(SpecialEnum.PoisonDrug);
                              myDragState = DragStateEnum.SHARER_DRUG;
                           }
                           else
                           {
                              Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): invalid col=" + myDragStateColNum.ToString());
                              return;
                           }
                           myGrid.Cursor = myCursors["PoisonDrug"]; // change cursor of button being dragged
                           if (false == UpdateGrid())
                              Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                           return;
                        }
                     }
                     else if (CombatEnum.APPLY_SHIELD == myState)
                     {
                        if (DragStateEnum.NONE == myDragState)
                        {
                           myDragStateRowNum = Grid.GetRow(img);
                           myDragStateColNum = Grid.GetColumn(img);
                           int i = myDragStateRowNum - STARTING_ASSIGNED_ROW;
                           if (i < 0)
                           {
                              myDragStateRowNum = 0;
                              myDragStateColNum = 0;
                              Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): invalid index=" + i.ToString());
                              return;
                           }
                           IMapItem mi = myGridRows[i].myUnassignable;
                           if (null == mi)
                           {
                              Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): invalid mi=null for i=" + i.ToString());
                              return;
                           }
                           if (2 == myDragStateColNum)
                           {
                              mi.SpecialKeeps.Remove(SpecialEnum.ShieldOfLight);
                              myDragState = DragStateEnum.KEEPER_SHIELD;
                           }
                           else if (3 == myDragStateColNum)
                           {
                              mi.SpecialShares.Remove(SpecialEnum.ShieldOfLight);
                              myDragState = DragStateEnum.SHARER_SHIELD;
                           }
                           else
                           {
                              Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): invalid col=" + myDragStateColNum.ToString());
                              return;
                           }
                           myGrid.Cursor = myCursors["Shield"]; // change cursor of button being dragged
                           if (false == UpdateGrid())
                              Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                           return;
                        }
                     }
                     else // any other state where not assigning/moving items within grid row
                     {
                        if (CombatEnum.ASSIGN_STRIKES == myState)
                        {
                           Logger.Log(LogEnum.LE_COMBAT_STATE, "Grid_MouseDown(): " + myState.ToString() + "-->STARTED_STRIKES");
                           myPreviousCombatState = myState = CombatEnum.STARTED_STRIKES;
                        }
                        if (CombatEnum.SWITCH_ATTACK == myState)
                        {
                           Logger.Log(LogEnum.LE_COMBAT_STATE, "Grid_MouseDown(): " + myState.ToString() + "-->STARTED_COUNTER");
                           myPreviousCombatState = myState = CombatEnum.STARTED_COUNTER;
                        }
                        if (false == myIsRollInProgress) // handle die roll
                        {
                           myIsRollInProgress = true;
                           myRollResultRowNum = Grid.GetRow(img);
                           RollEndCallback callback = null;
                           if (CombatEnum.END_POISON == myState)
                           {
                              callback = ShowDrugEndResults;
                              myRollResult = myDieRoller.RollMovingDie(myCanvas, callback);
                           }
                           else if (CombatEnum.END_SHIELD == myState)
                           {
                              callback = ShowShieldEndResults;
                              myRollResult = myDieRoller.RollMovingDie(myCanvas, callback);
                           }
                           else if (CombatEnum.END_TALISMAN == myState)
                           {
                              callback = ShowTalismanEndResults;
                              myRollResult = myDieRoller.RollMovingDie(myCanvas, callback);
                           }
                           else if (CombatEnum.APPLY_NERVE_GAS == myState)
                           {
                              callback = ShowNerveGasResults;
                              myRollResult = myDieRoller.RollMovingDie(myCanvas, callback);
                           }
                           else if (CombatEnum.MIRROR_STRIKE_SHOW == myState)
                           {
                              Logger.Log(LogEnum.LE_COMBAT_STATE, "Grid_MouseDown(): " + myState.ToString() + "-->STARTED_COUNTER");
                              myState = CombatEnum.STARTED_COUNTER;
                              myRollResult = myDieRoller.RollMovingDice(myCanvas, ShowCombatResults);
                              img.Visibility = Visibility.Hidden;
                           }
                           else
                           {
                              if (STARTING_ASSIGNED_ROW <= myRollResultRowNum)
                              {
                                 if (true == IsEnemyWizardAttacking(myRollResultRowNum))
                                 {
                                    Logger.Log(LogEnum.LE_COMBAT_STATE, "Grid_MouseDown(): " + myState.ToString() + "-->WIZARD_STRIKE");
                                    myState = CombatEnum.WIZARD_STRIKE;
                                    if (false == UpdateGrid())
                                       Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                                    return;
                                 }
                                 myRollResult = myDieRoller.RollMovingDice(myCanvas, ShowCombatResults);
                              }
                              else
                              {
                                 myRollResult = myDieRoller.RollMovingDice(myCanvas, ShowFirstStrikeResults);
                              }
                           }
                           img.Visibility = Visibility.Hidden;
                        }
                     }
                     return;
                  }
               }
               else if (ui is Rectangle rect) // next check all rectangles in the grid rows
               {
                  if (result.VisualHit == rect)
                  {
                     if (DragStateEnum.NONE != myDragState)
                     {
                        int rowNum = Grid.GetRow(rect);
                        int colNum = Grid.GetColumn(rect);
                        int i = rowNum - STARTING_ASSIGNED_ROW;
                        if (i < 0)
                        {
                           Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): 1-invalid index=" + i.ToString());
                           return;
                        }
                        IMapItem mi = myGridRows[i].myUnassignable;
                        if (null == mi)
                        {
                           Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): invalid mi=null for i=" + i.ToString());
                           return;
                        }
                        int j = myDragStateRowNum - STARTING_ASSIGNED_ROW;
                        if (j < 0)
                        {
                           Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): 2-invalid index=" + j.ToString());
                           return;
                        }
                        IMapItem miReturn = myGridRows[j].myUnassignable;
                        if (null == miReturn)
                        {
                           Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): invalid miReturn=null for i=" + i.ToString());
                           return;
                        }
                        if (DragStateEnum.KEEPER_DRUG == myDragState)
                        {
                           if ((rowNum == myDragStateRowNum) && (5 == colNum))
                              mi.IsPoisonApplied = true;
                           else
                              miReturn.AddSpecialItemToKeep(SpecialEnum.PoisonDrug);
                        }
                        else if (DragStateEnum.SHARER_DRUG == myDragState)
                        {
                           if (5 == colNum)
                              mi.IsPoisonApplied = true;
                           else
                              miReturn.AddSpecialItemToShare(SpecialEnum.PoisonDrug);
                        }
                        else if (DragStateEnum.KEEPER_SHIELD == myDragState)
                        {
                           if ((rowNum == myDragStateRowNum) && (5 == colNum))
                              mi.IsShieldApplied = true;
                           else
                              miReturn.AddSpecialItemToKeep(SpecialEnum.ShieldOfLight);
                        }
                        else if (DragStateEnum.SHARER_SHIELD == myDragState)
                        {
                           if (5 == colNum)
                              mi.IsShieldApplied = true;
                           else
                              miReturn.AddSpecialItemToShare(SpecialEnum.ShieldOfLight);
                        }
                        else
                        {
                           Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): reached default myDragState=" + myDragState.ToString());
                        }
                        myGrid.Cursor = Cursors.Arrow;
                        myDragState = DragStateEnum.NONE;
                        if (false == UpdateGrid())
                           Logger.Log(LogEnum.LE_ERROR, "Grid_MouseDown(): UpdateGrid() return false");
                        return;
                     }
                  }
               }
            }// end else (null != myMapItemDragged)
         } // end foreach
      }
      private void Header_MouseDown(object sender, MouseButtonEventArgs e)
      {
         System.Windows.Point p = e.GetPosition((UIElement)sender);
         HitTestResult result = VisualTreeHelper.HitTest(myStackPanelCheckMarks, p);  // Get the Point where the hit test occurrs
         foreach (UIElement ui in myStackPanelCheckMarks.Children)
         {
            if (ui is Image img) // next check all images which should be die rolls
            {
               if (result.VisualHit == img)
               {
                  RollEndCallback callback = ShowFirstStrikeResults;
                  myRollResult = myDieRoller.RollMovingDie(myCanvas, callback);
                  img.Visibility = Visibility.Hidden;
               }
            }
         }
      }
      private void Button_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         if (CombatEnum.FINALIZE_BATTLE_STATE == myState)  // Need to get starting state based on die roll. Do nothing until that happens
            return;
         int rowNum = Grid.GetRow(b);
         if (null != myMapItemDragged) // If dragging and clicked on same map item, end the dragging operation
         {
            if (myMapItemDragged.Name != b.Name && STARTING_ASSIGNED_ROW <= rowNum)
            {
               int i = rowNum - STARTING_ASSIGNED_ROW;
               if (false == DecrementAssignmentCounts(i))
                  Logger.Log(LogEnum.LE_ERROR, "Button_Click(): DecrementAssignmentCounts() returned false");
               myGridRows[i].myAssignable = myMapItemDragged; // take position of this assignable mapitem in this row
               myGridRows[i].myAssignmentCount = GetAssignedCount(myMapItemDragged.Name);
            }
            myMapItemDragged = null;
            myGrid.Cursor = Cursors.Arrow;
         }
         else
         {
            if ((true == b.Name.Contains("Cat")) || (true == b.Name.Contains("Boar")) || (true == b.Name.Contains("Cavalry")) || (CombatEnum.CAVALRY_STRIKE == myState))
               return;
            myMapItemDragged = myAssignables.Find(b.Name);
            if (null == myMapItemDragged)
            {
               Logger.Log(LogEnum.LE_ERROR, "Button_Click(): mi=null for b.Name=" + b.Name);
               return;
            }
            myGrid.Cursor = myCursors[myMapItemDragged.Name]; // change cursor of button being dragged
            if (STARTING_ASSIGNED_ROW <= rowNum)
            {
               int i = rowNum - STARTING_ASSIGNED_ROW;
               if (false == DecrementAssignmentCounts(i))
                  Logger.Log(LogEnum.LE_ERROR, "Button_Click(): DecrementAssignmentCounts() returned false");
               myGridRows[i].myAssignable = null;
            }
         }
         if (false == UpdateGrid())
         {
            Logger.Log(LogEnum.LE_ERROR, "Button_Click(): UpdateGrid() return false");
            return;
         }
      }
      private void ButtonR220_Click(object sender, RoutedEventArgs e)
      {
         if (null == myRulesMgr)
            Logger.Log(LogEnum.LE_ERROR, "ButtonR220_Click(): myRulesMgr=null");
         else if (false == myRulesMgr.ShowRule("r220"))
            Logger.Log(LogEnum.LE_ERROR, "ButtonR220_Click(): myRulesMgr.ShowRule() returned false");
      }
      private void ButtonT220_Click(object sender, RoutedEventArgs e)
      {
         if (null == myRulesMgr)
            Logger.Log(LogEnum.LE_ERROR, "ButtonT220_Click(): myRulesMgr=null");
         else if (false == myRulesMgr.ShowTable("t220"))
            Logger.Log(LogEnum.LE_ERROR, "ButtonT220_Click(): myEvmyRulesMgrentViewer.ShowTable() returned false");
      }
      private void ButtonRoute_Click(object sender, RoutedEventArgs e)
      {
         Button b = (Button)sender;
         b.IsEnabled = false;
         RollEndCallback callback = ShowRouteResults;
         myRollResult = myDieRoller.RollMovingDie(myCanvas, callback);
      }
      private void ButtonEscape_Click(object sender, RoutedEventArgs e)
      {
         if (CombatEnum.FINALIZE_BATTLE_STATE == myState)  // Need to get starting state based on die roll. Do nothing until that happens
            return;
         Button b = (Button)sender;
         b.IsEnabled = false;
         RollEndCallback callback = ShowEscapeResults;
         myRollResult = myDieRoller.RollMovingDie(myCanvas, callback);
      }
      private void ButtonEndKnightCombat_Click(object sender, RoutedEventArgs e)
      {
         foreach (IMapItem mi in myNonCombatants) // ShowEscapeResults() -return noncombatants to party
            myGameInstance.PartyMembers.Add(mi);
         //-------------------------------
         myIsEscape = true;
         myIsRoute = false;
         if (false == SetStateIfItemUsed())
         {
            if (null == myCallback)
            {
               Logger.Log(LogEnum.LE_ERROR, "ButtonEndKnightCombat_Click(): myCallback=null");
               return;
            }
            if (false == myCallback(myIsRoute, myIsEscape))  // ShowEscapeResults()
               Logger.Log(LogEnum.LE_ERROR, "ButtonEndKnightCombat_Click(): myCallback=null");
            return;
         }
         if (false == ResetGridForNonCombat(myGameInstance.PartyMembers))
         {
            Logger.Log(LogEnum.LE_ERROR, "ButtonEndKnightCombat_Click(): ResetGridForNonCombat()=false");
            return;
         }
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "ButtonEndKnightCombat_Click(): UpdateGrid() return false");
      }
      private void CheckBoxFightMirror_Checked(object sender, RoutedEventArgs e)
      {
         CheckBox cb = (CheckBox)sender;
         int rowNum = Grid.GetRow(cb);
         if (rowNum < STARTING_ASSIGNED_ROW)
         {
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxFightMirror_Checked(): rowNum=" + rowNum.ToString());
            return;
         }
         myRollResultRowNum = rowNum;
         int i = rowNum - STARTING_ASSIGNED_ROW;
         myGridRows[i].myAttackMirrorState = MirrorTargetEnum.ROLL_FOR_TARGET;
         Logger.Log(LogEnum.LE_COMBAT_STATE, "CheckBoxFightMirror_Checked(): " + myState.ToString() + "-->MIRROR_STRIKE");
         myState = CombatEnum.MIRROR_STRIKE;
         if (false == UpdateGrid())
            Logger.Log(LogEnum.LE_ERROR, "CheckBoxFightMirror_Checked(): UpdateGrid() return false");
      }
   }
}
