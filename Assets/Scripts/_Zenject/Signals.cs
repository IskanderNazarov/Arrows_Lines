using __Gameplay;
using UnityEngine;
using UnityEngine.UI;

public class LevelCompletedSignal {
}

public class NextLevelClickedSignal {
}

public class RestartLevelClickedSignal {
    public int levelNumber = -1;
}

public class BoosterBtnClickSignal {
    public Transform StartPos { get; private set; }
    public BoosterId BoosterId { get; private set; }

    public BoosterBtnClickSignal(BoosterId boosterId, Transform startPos) {
        BoosterId = boosterId;
        StartPos = startPos;
    }
}

public class LivesChangedSignal {
    public int CurrentLives;
    public bool IsLose;
}

public class GameOverSignal {
}

public class ReviveSignal {
}

public class ProcessAdForReviveSignal {
}

public class OnSnakeClickedSignal {
}

public class ScreenSizeChangedSignal {
    public float AspectRatio;
}