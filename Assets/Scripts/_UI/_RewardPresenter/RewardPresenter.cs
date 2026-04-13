using System;
using System.Collections;
using _Services;
using core.rewards;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Core._RewardPresenter {
    // main orchestrator for the reward sequence logic, driven by coroutines.
    public class RewardPresenter: IRewardPresenter {
        private RewardSequencePanel _panelPrefab;

        private bool _isTapped;
        public bool IsRewardingInProgress { get; private set; }
        public event Action OnSequenceComplete;

        private RewardSequencePanel _panel;
        private DiContainer _container;
        // presenter needs the panel to run coroutines and the inventory for logic.
        [Inject]
        private void Construct(RewardSequencePanel panelPrefab, DiContainer container) {
            _container = container;
            _panelPrefab = panelPrefab;
        }

        // this is the main public method to start the sequence.
        /*public void ShowReward(Reward reward) {
            if (IsRewardingInProgress) return;
            _panel.Run(ShowRewardSequence(reward));
        }*/
        public void ShowReward(Reward reward) {
            if (IsRewardingInProgress) return;
            
            _panel = Object.Instantiate(_panelPrefab);
            _container.InjectGameObject(_panel.gameObject);
            _panel.OnScreenTapped += () => _isTapped = true;


            _panel.Run(ShowRewardSequence(reward));
        }

        private IEnumerator ShowRewardSequence(Reward reward) {
            IsRewardingInProgress = true;
            _panel.Initialize();

            // --- Chest Sequence (if applicable) ---
            /*if (reward.IsChest) {
                // wait for chest to appear
                yield return _panel.AnimateChestAppearance();

                // wait for user to tap the screen
                yield return WaitForTap();

                // wait for chest to open
                yield return _panel.AnimateChestOpening();
            }
            */

            // --- Reward Appearance Sequence ---
            yield return _panel.AnimateRewardAppearance(reward);

            // --- Finalization ---
            /*if (overflowBoosters > 0) {
                // TODO: show a separate dialog here. you can make it a coroutine too.
                // yield return _dialogManager.ShowBoosterOverflowDialog();
                yield return _panel.ShowExtraBoostersConversion(exchangedCoinsOnBoosters);
            }*/
            
            // --- Wait for Confirmation Tap ---
            yield return WaitForTap();

            // TODO: play the fly-to-UI animation here.
            // yield return _panel.AnimateFlyToHUD();

            CloseSequence();
        }

        // a helper coroutine to wait for a screen tap.
        private IEnumerator WaitForTap() {
            _isTapped = false;
            yield return new WaitUntil(() => _isTapped);
        }

        /*private int CheckForBoosterOverflow(Reward reward) {
            if (reward.Boosters == null || !reward.Boosters.Any()) return 0;

            var boosterOverflow = 0;
            foreach (var br in reward.Boosters) {
                var current = _boosterInventory.GetCount(br.BoosterId);
                var cap = _boosterInventory.GetCap(br.BoosterId);
                if (current + br.Amount > cap) {
                    boosterOverflow += (current + br.Amount - cap);
                }
            }

            return boosterOverflow;
        }*/

        private void CloseSequence() {
            _panel.Hide();
            IsRewardingInProgress = false;
            OnSequenceComplete?.Invoke();
        }
    }
}