using Core._Services.SoundManagement;

// Твой SaveManager

namespace Game.SoundManagement {
    public class GameSoundStateProvider : ISoundStateProvider {
        private readonly PlayerProgressService _saveManager;

        public GameSoundStateProvider(PlayerProgressService saveManager) {
            _saveManager = saveManager;
        }

        public bool IsSoundOn {
            get => _saveManager.Data.IsSoundOn;
            set {
                _saveManager.Data.IsSoundOn = value;
                _saveManager.MarkDirty();
            }
        }

        public bool IsMusicOn {
            get => _saveManager.Data.IsMusicOn;
            set {
                _saveManager.Data.IsMusicOn = value;
                _saveManager.MarkDirty();
            }
        }
    }
}