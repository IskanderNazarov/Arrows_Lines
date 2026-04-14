using _Data;

namespace _Services {
    // Этот слепок сервис отдает UI, чтобы тот знал, что именно нужно анимировать
    public struct ProgressUpdateResult {
        public int StartStars; // Сколько было до начисления
        public int TargetStarsBefore; // Какая была цель до начисления

        public int EndStars; // Сколько стало в итоге (остаток после переполнения)
        public int TargetStarsAfter; // Какая стала цель (если открыли новый левел прогресса)

        public bool DidUnlock; // Случилось ли переполнение (открытие)
        public PlanetRewardData UnlockedReward; // Что именно открыли

        public override string ToString() {
            return $"StartStars: {StartStars} \n" +
                   $"TargetStarsBefore: {TargetStarsBefore}\n" +
                   $"EndStars: {EndStars}\n" +
                   $"TargetStarsAfter: {TargetStarsAfter}\n" +
                   $"DidUnlock: {DidUnlock}\n" +
                   $"UnlockedReward: {UnlockedReward}\n"
                   ;
        }
    }
}