using System;
using UnityEngine;

namespace Tbx
{
    public struct ScrewCoordinates
    {
        public static ScrewCoordinates From(DualQuaternion dq)
        {
            var v_r = new Vector3(dq.NonDualPart.x, dq.NonDualPart.y, dq.NonDualPart.z);
            var v_d = new Vector3(dq.DualPart.x,    dq.DualPart.y,    dq.DualPart.z);
            ScrewCoordinates result;
            result._p = Vector3.zero;
            result._theta = 2f * Mathf.Acos(dq.NonDualPart.w);
            result._d = -2f * dq.DualPart.w / v_r.magnitude; 
            result._line = v_r.normalized;
            result._moment = (v_d - result._line * result._d * dq.NonDualPart.w / 2f) / v_r.magnitude;
            return result;
        }

        // Returns a point on... ?
        public Vector3 PointOnTheLine => _p;

        // Returns the angle of rotation around ??? in radians
        public float AngleOfRotationRad => _theta;
        // Returns the angle of rotation around ??? in degrees
        public float AngleOfRotationDeg => _theta * Mathf.Rad2Deg;

        // Returns the translation along the axis
        public float TranslationAlongTheAxis => _d;

        // Returns the line to rotate about, which represent to be the Pluecker Coordiantes 1, 2, and 3.
        public Vector3 Line => _line;

        // Returns the moment vector, which happens to represent the Pluecker Coordinates 4, 5, and 6.
        public Vector3 MomentVector => _moment;

        public DualQuaternion ToDualQuaternion()
        {
            var cosHalfTheta = Mathf.Cos(_theta / 2f);
            var sinHalfTheta = Mathf.Sin(_theta / 2f);
            var halfD = _d / 2f;

            var w_r = cosHalfTheta;
            var v_r = _line * sinHalfTheta;
            var w_d = -halfD * sinHalfTheta;
            var v_d = sinHalfTheta * _moment + halfD * cosHalfTheta * _line;

            return new DualQuaternion(new Quaternion(v_r.x, v_r.y, v_r.z, w_r), new Quaternion(v_d.x, v_d.y, v_d.z, w_d));
        }

        private Vector3 _p;
        private float _theta;
        private float _d;
        private Vector3 _line;
        private Vector3 _moment;
    }
}
