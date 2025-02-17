using UnityEngine;

public class GameScreen : MonoBehaviour
{
    public ScreenAnimations MyAnimationType;
    public GameScreens MyName;
    public float TimeToAnimate;
    public Vector3 StartingValue;
    public Vector3 EndingValue;
    public LeanTweenType MyTweenType;
    public static MainScreenHandler screenHandler;
   

    private void OnEnable()
    {
        switch (MyAnimationType)
        {
            case ScreenAnimations.MoveScreen:
                ScreenAnimation.MoveScreen(GetComponent<RectTransform>(), TimeToAnimate, StartingValue, EndingValue, MyTweenType);
                break;
            case ScreenAnimations.ScaleScreen:
                ScreenAnimation.ScaleScreen(GetComponent<RectTransform>(), TimeToAnimate, StartingValue, EndingValue, MyTweenType);
                break;
        }
    }
}