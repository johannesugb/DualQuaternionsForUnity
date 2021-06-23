using System;
using UnityEngine;

namespace Tbx
{
    // Utils and extension functions for Unity's Quaternion class
    public static class QuaternionUtils
    {
        // Computes the norm of a Quaternion
        public static float Norm(this Quaternion q)
        {
            return Mathf.Sqrt(q[0] * q[0] + 
                              q[1] * q[1] + 
                              q[2] * q[2] + 
                              q[3] * q[3] );
        }

        // Returns a new quaternion which has the original quaternion's values
        // multiplied with the given factor x.
        public static Quaternion MultipliedWith(this Quaternion q, float x)
        {
            q[0] *= x;
            q[1] *= x;
            q[2] *= x;
            q[3] *= x;
            return q;
        }

        // Returns a new quaternion which has the original quaternion's values
        // divided by the given factor x.
        public static Quaternion DividedBy(this Quaternion q, float x)
        {
            q[0] /= x;
            q[1] /= x;
            q[2] /= x;
            q[3] /= x;
            return q;
        }

        // Gets the vector part of the given quaternion
        public static Vector3 VectorPart(this Quaternion q)
        {
            return new Vector3(q.x, q.y, q.z);
        }

        // Perform element-wise addition of both quaternions' values
        public static Quaternion Add(Quaternion q1, Quaternion q2)
        {
            return new Quaternion(
                q1.x + q2.x,
                q1.y + q2.y,
                q1.z + q2.z,
                q1.w + q2.w
            );
        }
    }

}
