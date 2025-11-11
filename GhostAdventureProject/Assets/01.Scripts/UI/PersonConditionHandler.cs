
// 빙의오브젝트-사람의 컨디션에 따라 QTE3 난이도를 변경합니다.
// 난이도는 활력일 때 가장 높고 피곤일 때 가장 낮습니다.

[System.Serializable]
public class QTESettings
{
    public float rotationSpeed;
    public float timeLimit;
    public int successZoneCount;
    public float minZoneSize;
    public float maxZoneSize;
}

public abstract class PersonConditionHandler
{
    public abstract QTESettings GetQTESettings();
}

// 활력
public class VitalConditionHandler : PersonConditionHandler
{
    public override QTESettings GetQTESettings() => new QTESettings
    {
        rotationSpeed = 270,
        timeLimit = 2f,
        successZoneCount = 4,
        minZoneSize = 20f,
        maxZoneSize = 30f
    };
}

// 보통
public class NormalConditionHandler : PersonConditionHandler
{
    public override QTESettings GetQTESettings() => new QTESettings
    {
        rotationSpeed = 220,
        timeLimit = 2f,
        successZoneCount = 3,
        minZoneSize = 30f,
        maxZoneSize = 40f
    };
}

// 피곤
public class TiredConditionHandler : PersonConditionHandler
{
    public override QTESettings GetQTESettings() => new QTESettings
    {
        rotationSpeed = 180f,
        timeLimit = 4.2f,
        successZoneCount = 2,
        minZoneSize = 30f,
        maxZoneSize = 50f
    };
}
