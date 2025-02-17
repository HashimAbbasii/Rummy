using UnityEngine;

public class GamePanel : MonoBehaviour
{
    public ScreenAnimations MyAnimationType;
    public float TimeToAnimate;
    public Vector3 StartingValue;
    public Vector3 EndingValue;
    public LeanTweenType MyTweenType;

    private void OnEnable()
    {
        if (MyAnimationType == ScreenAnimations.MoveScreen)
        {
            ScreenAnimation.MoveScreen(GetComponent<RectTransform>(), TimeToAnimate, StartingValue, EndingValue, MyTweenType);
        }
        else if (MyAnimationType == ScreenAnimations.ScaleScreen)
        {
            ScreenAnimation.ScaleScreen(GetComponent<RectTransform>(), TimeToAnimate, StartingValue, EndingValue, MyTweenType);
        }
    }
}
