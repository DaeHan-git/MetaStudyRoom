using System;

[Serializable]
public class AvatarAppearanceData
{
    public string expressionId;  // 표정 ID (예: "exp_smile")
    public string bodyId;        // 몸통 ID (예: "body_default")
    public string accessoryId;   // 악세서리 ID (예: "acc_glasses" 또는 빈 값/none)
}