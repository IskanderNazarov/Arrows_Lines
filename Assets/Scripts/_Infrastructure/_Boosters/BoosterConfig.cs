using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace __Gameplay._Services._Boosters {
    public class BoosterConfig {
        private Dictionary<BoosterId, BoosterInfo> boosterInfo;

        //[Inject] private GameConfig _gameConfig;

        private BoosterConfig (GameConfig _gameConfig){
        //public void Initialize() {
            Debug.Log("BoosterConfig Initialize");
            /*boosterInfo = new Dictionary<BoosterId, BoosterInfo> {
                {
                    BoosterId.test_booster_1, new BoosterInfo {
                        Id = BoosterId.RemoveTile,
                        Name = Localizer.RemoveOneBoosterName,
                        Description = Localizer.RemoveOneBoosterInfo,
                        Icon = _gameConfig.boosterRemoveOneIcon,
                        usePromptUI_1 = Localizer.Booster_1_prompt_1,
                        usePromptUI_2 = Localizer.Booster_1_prompt_2
                    }
                }, {
                    BoosterId.RemoveSelectedNumbers, new BoosterInfo {
                        Id = BoosterId.RemoveSelectedNumbers,
                        Name = Localizer.RemoveSelectedBoosterName,
                        Description = Localizer.RemoveSelectedBoosterInfo,
                        Icon = _gameConfig.boosterRemoveSelectIcon,
                        usePromptUI_1 = Localizer.Booster_2_prompt_1,
                        usePromptUI_2 = Localizer.Booster_2_prompt_2
                    }
                }
            };*/
        }

        public BoosterInfo GetInfo(BoosterId boosterId) {
            return boosterInfo[boosterId];
        }
    }
}