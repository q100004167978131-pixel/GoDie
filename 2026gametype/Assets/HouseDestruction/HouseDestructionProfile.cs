using UnityEngine;

[CreateAssetMenu(menuName = "GoDie/House Destruction Profile")]
public class HouseDestructionProfile : ScriptableObject
{
    public Sprite intactSprite;
    public Sprite ruinedSprite;
    [Min(0f)] public float transitionDuration = 0.6f;
    [Min(0f)] public float shakeDistance = 0.035f;
}
