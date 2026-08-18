

[System.Serializable]
public class LevelConfig
{
    public string levelName = "Level 1";    // level name
    public int totalTargets = 25;           // targets per level
    public float spawnIntervalMin = 1f;     // spawnMin low
    public float spawnIntervalMax = 2.5f;   // spawnMax high
    public float targetVisibleTime = 3f;    // target life
    public int simultaneousTargets = 1;     // default to 1 for level 1: then raise later for harder levels
    public float slotCooldown = 0.3f;       // delay after target retracts before its slot can trigger again
}