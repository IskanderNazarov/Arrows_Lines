using Core._RewardPresenter;

namespace core.rewards {
    public interface IRewardApplier {
        void ApplyReward(Reward reward, string placement);
    }
}
