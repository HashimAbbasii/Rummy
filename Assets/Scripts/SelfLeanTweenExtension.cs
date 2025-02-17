using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SelfLeanTweenExtension
{
    public static void LeanTransform(this Transform transform, Transform to, float time)
    {
        transform.LeanMove(to.position, time);
        transform.LeanRotate(to.rotation.eulerAngles, time);
        transform.LeanScale(to.localScale, time);
    }
}
