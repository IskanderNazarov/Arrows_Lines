// Game.asmdef

using __Gameplay;
using core.boosters;

namespace Game.Boosters {
    // Просто наследуем дженерик с конкретным типом
    public interface IGameBoosterInventory : IBoosterInventory<BoosterId> { 
    }
}