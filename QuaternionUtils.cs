using System;
using UnityEngine;

namespace DQU
{
    // Utils and extension functions for Unity's Quaternion class
    public static class QuaternionUtils
    {
        // Converts a vector to its quaternion representation, which is a quaternion
        // with no rotation, and its vector-components set to the input vector.
        public static Quaternion ToPureQuaternion(this Vector3 vec)
        {
            return new Quaternion(vec.x, vec.y, vec.z, 0f);
        }

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

        // Performs quaternion multiplication of two given unit-norm quaternions.
        // I.e. a single multiplication that is, no sandwiching or so!
        public static Quaternion Multiply(Quaternion lhs, Quaternion rhs)
        {
            // lhs.Normalize();
            // rhs.Normalize();
            var wp = lhs.w * rhs.w - Vector3.Dot(lhs.VectorPart(), rhs.VectorPart());
            var vp = rhs.VectorPart() * lhs.w + lhs.VectorPart() * rhs.w + Vector3.Cross(lhs.VectorPart(), rhs.VectorPart());
            return new Quaternion(vp.x, vp.y, vp.z, wp);
        }

        // Returns a new quaternion which has the original quaternion's values
        // divided by the given factor x.
        public static Quaternion DividedBy(this Quaternion q, float d)
        {
            var x = 1f / d;
            return q.MultipliedWith(x);
        }

        // Gets the vector part of the given quaternion
        public static Vector3 VectorPart(this Quaternion q)
        {
            return new Vector3(q.x, q.y, q.z);
        }

        // Gets the vector part of the given quaternion, and performs a check if it really is a pure quaternion
        public static Vector3 PureQuaternionToPositionVector(this Quaternion q)
        {
            Debug.Assert(Mathf.Abs(q.w) < 1e-6);
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

        // Return a new quaternion which is the conjugate of the original
        public static Quaternion Conjugate(this Quaternion q)
        {
            return new Quaternion(-q.x, -q.y, -q.z, q.w);
        }

        // Returns a new quaternion which is computed from the element-wise
        // minima of the input quaternions. (No idea if this makes any sense ^^)
        public static Quaternion Min(Quaternion a, Quaternion b)
        {
            return new Quaternion(
                Mathf.Min(a.x, b.x),
                Mathf.Min(a.y, b.y),
                Mathf.Min(a.z, b.z),
                Mathf.Min(a.w, b.w)
            );
        }

        // Returns a new quaternion which is computed from the element-wise
        // maxima of the input quaternions. (No idea if this makes any sense ^^)
        public static Quaternion Max(Quaternion a, Quaternion b)
        {
            return new Quaternion(
                Mathf.Max(a.x, b.x),
                Mathf.Max(a.y, b.y),
                Mathf.Max(a.z, b.z),
                Mathf.Max(a.w, b.w)
            );
        }
    }

}
