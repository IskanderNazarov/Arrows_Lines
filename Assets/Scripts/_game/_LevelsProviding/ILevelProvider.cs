namespace _game._LevelsProviding {
    public interface ILevelProvider {
        public LevelData GetCurrentLevel();
        public LevelData GetLevel(int level);
    }
}