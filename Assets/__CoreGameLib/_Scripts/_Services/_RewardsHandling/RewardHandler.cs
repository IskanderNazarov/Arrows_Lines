// file: assets/game/scripts/rewardhandler.cs
// assembly: game.asmdef

using System;
using System.Threading.Tasks;
using Core._RewardPresenter;
using core.rewards;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace core.purchasing {
    public class RewardHandler {
        private IRewardApplier _rewardApplier;
        private IRewardPresenter _rewardPresenter;

        public Action<Reward> OnRewardShowFinished;

        [Inject]
        private void Construct(IRewardApplier rewardApplier, IRewardPresenter rewardPresenter) {
            _rewardApplier = rewardApplier;
            _rewardPresenter = rewardPresenter;
        }

        /// <summary>
        /// public method to apply and show a reward.
        /// </summary>
        /// <param name="reward">the reward to grant</param>
        /// <param name="placement">analytics placement string</param>
        /// <param name="showRewardPresenter">show the ui popup?</param>
        public async Task HandlerReward(Reward reward, string placement, bool showRewardPresenter = true) {
            _rewardApplier.ApplyReward(reward, placement);
            if (showRewardPresenter) {
                _rewardPresenter.ShowReward(reward);
                //var t = UniTask.WaitWhile(() => _rewardPresenter.IsRewardingInProgress);
                await UniTask.WaitWhile(() => _rewardPresenter.IsRewardingInProgress);
                
            }

            OnRewardShowFinished?.Invoke(reward);
            //return UniTask.CompletedTask;
           // return UniTask.CompletedTask;
        }
    }
}