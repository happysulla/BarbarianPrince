using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BarbarianPrince
{
   public enum AudienceConstraintEnum
   {
      DAY,
      LETTER,
      LETTER_GIVEN,
      ASSISTANT_OR_LETTER,
      OFFERING,      // e155c & 155d (fa.Day is set)
      RELIGION,      // Religious constraint is never removed... it can be deactivated if Priest/Monk in party
      PURIFICATION,
      CLOTHES,   // e149 - learn court manners
      MONSTER_KILL,
      ERROR
   };
   //------------------------------------------------------------------------
   public interface IForbiddenAudience
   {
      AudienceConstraintEnum Constraint { get; set; }
      ITerritory ForbiddenTerritory { get; set; }
      ITerritory TargetTerritory { get; set; }
      IMapItem Assistant { get; set; }
      int Day { get; set; }
   }
   //------------------------------------------------------------------------
   public interface IForbiddenAudiences : System.Collections.IEnumerable
   {
      int Count { get; }
      void Add(IForbiddenAudience fa);
      IForbiddenAudience RemoveAt(int index);
      void Insert(int index, IForbiddenAudience fa);
      void Reverse();
      void Clear();
      bool Contains(IForbiddenAudience fa);
      int IndexOf(IForbiddenAudience fa);
      void Remove(IForbiddenAudience fa);
      IForbiddenAudience this[int index] { get; set; }
      //----------------------------------------------
      void AddPurifyConstaint(ITerritory forbidden);
      void AddOfferingConstaint(ITerritory forbidden, int offeringDay);
      void AddLetterConstraint(ITerritory forbidden, ITerritory targetTerritory=null); // target territory is null when user is selecting territory with bullseye
      void AddLetterGivenConstraint(ITerritory forbidden);
      void AddAssistantConstraint(ITerritory forbidden, IMapItem assistant);
      void AddTimeConstraint(ITerritory forbidden, int day);
      void AddClothesConstraint(ITerritory forbidden);
      void AddReligiousConstraint(ITerritory forbidden);
      void AddMonsterKillConstraint(ITerritory forbidden);
      //----------------------------------------------
      bool Contains(IGameInstance gi);
      bool UpdateLetterLocation(ITerritory letterTerritory);
      //----------------------------------------------
      bool IsClothesConstraint();
      bool IsReligiousConstraint(ITerritory t);
      bool IsOfferingsConstraint(ITerritory offeringTerritory, int offeringDay);
      void RemoveOfferingsConstraints(ITerritory offeringTerritory);
      void RemovePurifyConstraints(ITerritory offeringTerritory, ITerritories purifications);
      void RemoveLetterConstraints(ITerritory letterTerritory);
      void RemoveLetterGivenConstraints(ITerritory letterTerritory);
      void RemoveAssistantConstraints(IMapItem mi);
      void RemoveTimeConstraints(int day);
      void RemoveClothesConstraints();
      void RemoveReligionConstraint(ITerritory t);
      void RemoveMonsterKillConstraints(int numKills);
   }
}
