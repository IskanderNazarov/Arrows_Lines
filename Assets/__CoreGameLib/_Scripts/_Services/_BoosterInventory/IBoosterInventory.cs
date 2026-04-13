using System;

namespace core.boosters {
    // 'tboosterid' can be any enum. 'where' is the constraint.
    public interface IBoosterInventory<TBoosterId> where TBoosterId : Enum {
        event Action<TBoosterId, int> OnChanged; // boosterid, delta

        int GetCount(TBoosterId id);
        int GetCap(TBoosterId id);
        void SetCap(TBoosterId id, int cap);
        bool TryAdd(TBoosterId id, int amount, string source);
        bool TryConsume(TBoosterId id, int amount, string reason);
    }
}
