using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class utils
{
    /*//flips a value around another value
    //flipping 1 around 2 gives 3
    //flipping 2 around 6 gives 10
    public static float flip(float val, float flipLoc)
    {
        float dif = flipLoc - val;

        return val + (dif * 2);
    }

    //flips  a val between 0 and bound then weights to be between 0 and 1 (assuming its between 0 and the bound to begin with)
    public static float weightedFlip(float val, float bound)
    {
        return flip(val, (bound / 2)) / bound;
    }*/

    public static float angle(Vector3 target, Transform home)
    {
        Vector3 targetDir = target - home.localPosition;
        return Vector3.Angle(targetDir, home.forward);

    }

    //returns 0 if centered, and decreases towards -1 as angle approaches the bounds
    public static float centerAngleWeight(float angle, float angle_bound)
    {
        //if angle is within  degrees bound 
        if (angle < angle_bound)
        {
            return - angle / angle_bound;
        }

        return -1;
    }

    private static float randomNoise(float maxNoise)
    {
        float n =  Random.Range(-(float)maxNoise, (float)maxNoise);
        return n;
    }

    //adds a random amount of noise to each dimension of the position vector,
    //returning a new vector
    public static Vector3 addNoise(Vector3 pos, float maxNoise = 0.1f) {
        Vector3 noise = new Vector3();
        noise.x = randomNoise(maxNoise);
        noise.y = randomNoise(maxNoise);
        noise.z = randomNoise(maxNoise);

        return pos + noise;
    }
}
