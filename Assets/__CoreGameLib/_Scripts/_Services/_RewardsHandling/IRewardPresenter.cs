using System;
using Core._RewardPresenter;

namespace core.rewards {
    public interface IRewardPresenter {
        public event Action OnSequenceComplete;
        bool IsRewardingInProgress { get; }
        void ShowReward(Reward reward);
    }
}
