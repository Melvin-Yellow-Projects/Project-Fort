/**
 * File Name: Bezier.cs
 * Description: A script for getting quadratic Bézier data; see Additional Comments for more details
 * 
 * Authors: Catlike Coding, Will Lacey
 * Date Created: October 8, 2020
 * 
 * Additional Comments: 
 *      The original version of this file can be found here:
 *      https://catlikecoding.com/unity/tutorials/hex-map/ within Catlike Coding's tutorial series:
 *      Hex Map; this file has been updated it to better fit this project
 *
 *      "A Bézier curve is defined by a sequence of points. It starts at the first point and ends at
 *          the last point, but does not need to go through the intermediate points. Instead, those
 *          points pull the curve away from being a straight line. [...] The idea of Bézier curves
 *          is that they are parametric. If you give it a value, typically named t, between zero and
 *          one, you get a point on the curve. As t increases from zero to one, you move from the
 *          first point of the curve to the last point."
 *
 *          TODO: verify the comments in the Bezier script
 **/

using UnityEngine;

/// <summary>
/// Utility class with methods for getting a point on a quadratic Beziér curve
/// </summary>
public static class Bezier
{

    /********** MARK: Variables **********/
    #region Variables

    /// <summary>
    /// Gets a point on a quadratic Bézier curve; see link for more details
    /// https://www.theappguruz.com/app/uploads/2015/07/bezier_quadratic.gif
    /// </summary>
    /// <param name="a">starting point</param>
    /// <param name="b">point to create curve from</param>
    /// <param name="c">ending point</param>
    /// <param name="t">interpolator</param>
    /// <param name="clamp">whether or not to clamp the interpolator</param>
    /// <returns>a quadratic Bézier point</returns>
    public static Vector3 GetPoint(Vector3 a, Vector3 b, Vector3 c, float t, bool clamp = false)
	{
        // clamp the interpolator
        if (clamp) t = Mathf.Clamp(t, 0f, 1f);

        // inverts t such that 0 corresponds to a and 1 to b
		float r = 1f - t;

        // Bézier formula
        return r * r * a + 2f * r * t * b + t * t * c;
	}

    /// <summary>
    /// Gets a derivative point on a quadratic Bézier curve
    /// </summary>
    /// <param name="a">starting point</param>
    /// <param name="b">point to create curve from</param>
    /// <param name="c">ending point</param>
    /// <param name="t">interpolator</param>
    /// <param name="clamp">whether or not to clamp the interpolator</param>
    /// <returns>a quadratic Bézier derivative point</returns>
    public static Vector3 GetDerivative(
        Vector3 a, Vector3 b, Vector3 c, float t,
        bool clamp = false)
    {
        // clamp the interpolator
        if (clamp) t = Mathf.Clamp(t, 0f, 1f);

        // Bézier derivative formula
        return 2f * ((1f - t) * (b - a) + t * (c - b));
    }

    #endregion
}