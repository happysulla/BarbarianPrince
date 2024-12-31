using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace BarbarianPrince
{
   [Serializable]
   public enum MountEnum
   {
      Horse,
      Pegasus,
      Griffon,
      Harpy,
      Any
   };
   public interface IMapItem
   {
      //----------------------------------------
      // Basic Properties
      string Name { get; set; }
      int Endurance { get; set; }
      int Movement { get; set; }
      int Combat { get; set; }
      string TopImageName { get; set; }
      string BottomImageName { get; set; }
      string OverlayImageName { get; set; }
      List<BloodSpot> WoundSpots { get; }
      List<BloodSpot> PoisonSpots { get; }
      double Zoom { get; set; }
      bool IsHidden { get; set; }
      bool IsExposedToUser { get; set; } // some dialogs require clicking on mount to rotate it. This variable tracks if user has seen this item yet.
      bool IsAnimated { get; set; }
      IMapPoint Location { get; set; }
      //----------------------------------------
      bool IsRunAway { get; set; }      // caused by nerve gas in combat
      bool IsShowFireball { get; set; } // e023 - wizard attack
      bool IsSecretGatewayToDarknessKnown { set; get; }  // e046  
      bool IsFugitive { set; get; }  // e048 
      bool IsPlayedMusic { get; set; } // e049 - minstrel plays music on request one time
      bool IsCatchCold { get; set; } // e079 - catch cold from heavy rains
      bool IsMountSick { get; set; } // e096 - check for mount death at end of day
      bool IsExhausted { get; set; }  // e120 - member exhausted until rested
      bool IsSunStroke { get; set; } // e121 - member must be carried
      bool IsPlagued { get; set; }  // e133 - kill plagued members
      bool IsPoisonApplied { get; set; } // e185
      bool IsResurrected { set; get; } // e192 - Resurrection Necklace used to revive character
      bool IsShieldApplied { get; set; } // e193
      int PlagueDustWound { get; set; } // r227 - trap with plague dust
      bool IsTrueLove { set; get; } //e228
      bool IsFickle { set; get; }  // e331
      int GroupNum { set; get; }  // e332
      int PayDay { set; get; }
      int Wages { set; get; } //e333
      bool IsAlly { set; get; } //e334
      bool IsLooter { set; get; }  // e340
      bool IsTownCastleTempleLeave { set; get; } // e341
      bool IsDisappear { get; set; } // r343 - disappear
      //----------------------------------------
      bool IsGuide { get; set; }
      bool IsRiding { get; set; }
      bool IsFlying { get; set; }
      int StarveDayNum { get; set; }
      int StarveDayNumOld { get; set; }
      bool IsUnconscious { get; set; }
      bool IsKilled { get; set; }
      int Wound { get; set; }
      int Poison { get; set; }
      int Coin { get; set; }
      int WealthCode { get; set; }
      List<SpecialEnum> SpecialKeeps { get; } // Special possessions that cannot be shared
      List<SpecialEnum> SpecialShares { get; } // Special possessions that can be given away
      int Food { get; set; }
      int MovementUsed { get; set; }
      //----------------------------------------
      IMapItem Rider { get; set; } // griffon/harpy can have a rider
      IMapItems Mounts { get; set; }
      IMapItems LeftOnGroundMounts { set; get; } // horses left when air travel...if undo, need to revert these back to party
      Dictionary<IMapItem, int> CarriedMembers { get; set; } // This mapitem carries this much load (CarriedMembers.Value) of this MapItem (CaarriedMembers.Key)
      //----------------------------------------
      ITerritory Territory { get; set; }
      ITerritory TerritoryStarting { get; set; }
      ITerritories GuideTerritories { get; set; }
      //----------------------------------------
      void SetLocation(int counterCount);
      bool AddNewMount(MountEnum mt = MountEnum.Horse);
      bool AddMount(IMapItem mount);
      void SetMountState(IMapItem mount);
      void RemoveMountedMount();
      void RemoveUnmountedMounts();
      bool RemoveNonFlyingMounts();
      bool RemoveMountWithLoad(IMapItem deadMount);
      void SetWounds(int wounds, int poisonWounds);
      void HealWounds(int wounds, int poisonWound);
      bool IsSpecialItemHeld(SpecialEnum item);
      bool IsSpecialist();
      bool IsMagicUser();
      int GetNumSpecialItem(SpecialEnum item);
      bool AddSpecialItemToKeep(SpecialEnum item);
      bool AddSpecialItemToShare(SpecialEnum item);
      bool RemoveSpecialItem(SpecialEnum item);
      int GetMaxFreeLoad();
      int GetFreeLoad();
      int GetFreeLoadWithoutModify();
      int GetFlyLoad();
      bool RemoveVictimMountAndLoad(); // Remove what is carried by person and mount if riding
      bool IsFlyer();
      bool IsFlyingMount();
      bool IsFlyingMountCarrier();
      void Reset();
      void ResetPartial();
   }
   public interface IMapItems : System.Collections.IEnumerable
   {
      int Count { get; }
      void Add(IMapItem mi);
      IMapItem RemoveAt(int index);
      void Insert(int index, IMapItem mi);
      void Clear();
      bool Contains(IMapItem mi);
      int IndexOf(IMapItem mi);
      void Remove(IMapItem miName);
      void Reverse();
      IMapItem Remove(string miName);
      IMapItem Find(string miName);
      IMapItem this[int index] { get; set; }
      IMapItems Shuffle();
      IMapItems SortOnFreeLoad();
      IMapItems SortOnCombat();
      IMapItems SortOnFood();
      IMapItems SortOnCoin();
      IMapItems SortOnMount();
      IMapItems SortOnGroupNum();
      void Rotate(int numOfRotates);
   }
}
