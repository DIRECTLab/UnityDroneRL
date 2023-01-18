using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class utils
{
    //flips a value around another value
    //flipping 1 around 2 gives 3
    //flipping 2 around 6 gives 10
    public static float flip(float val, float flipLoc)
    {
        float dif = flipLoc - val;

        return val + (dif * 2);
    }

    //flips and weights a val between 0 and 1 (assuming its between 0 and the bound to begin with
    public static float weightedFlip(float val, float bound)
    {
        return flip(val, (bound / 2)) / bound;
    }

    //returns 1 if centered, and decreases towards 0 as angle approaches the bounds
    public static float centerAngleWeight(Vector3 local_pos, float angle_bound, Transform transform)
    {
        Vector3 targetDir = local_pos - transform.localPosition;
        float angle = Vector3.Angle(targetDir, transform.forward);

        //if angle is within  degrees bound 
        if (angle <= angle_bound)
        {
            return weightedFlip(angle, angle_bound);
        }

        return 0;
    }

    private static float randomNoise(float maxNoise)
    {
        return Random.Range(-maxNoise, maxNoise);
    }

    public static Vector3 addNoise(Vector3 pos, float maxNoise = .1f) {
        Vector3 noise = new Vector3(randomNoise(maxNoise), randomNoise(maxNoise), randomNoise(maxNoise));
        return pos + noise;
    }
}
