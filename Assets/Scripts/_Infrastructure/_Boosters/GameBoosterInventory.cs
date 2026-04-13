using System.Collections.Generic;
using __Gameplay;
using core.boosters;
using Zenject;

public class GameBoosterInventory : BoosterInventory<BoosterId> {
    private PlayerProgressService _progressService;

    // Zenject автоматически передаст кап-лимиты и прогресс-сервис
    public GameBoosterInventory(Dictionary<BoosterId, int> caps, PlayerProgressService progressService) : base(caps) {
        _progressService = progressService;
    }

    public override int GetCount(BoosterId id) {
        return _progressService.GetBoosterCount((int)id);
    }

    protected override void SaveCount(BoosterId id, int newValue) {
        _progressService.SetBoosterCount((int)id, newValue);
    }
}