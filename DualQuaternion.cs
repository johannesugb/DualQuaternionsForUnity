using System;
using UnityEngine;

namespace Tbx
{
    public struct DualQuaternion 
    {
        // -------------------------------------------------------------------------
        /// @name Factory functions
        // -------------------------------------------------------------------------

        // Generates a dual quaternion with no translation and no rotation either
        public static DualQuaternion Identity
        {
            get 
            {
                return From(Quaternion.identity, Vector3.zero);
            }
        }

        // Generates a dual quaternion from a given rotation quaternion q and a translation vector t:
        public static DualQuaternion From(Quaternion q, Vector3 t)
        {
            float w = -0.5f*( t.x * q.x + t.y * q.y + t.z * q.z);
            float i =  0.5f*( t.x * q.w + t.y * q.z - t.z * q.y);
            float j =  0.5f*(-t.x * q.z + t.y * q.w + t.z * q.x);
            float k =  0.5f*( t.x * q.y - t.y * q.x + t.z * q.w);

            return new DualQuaternion(q, new Quaternion{w = w, x = i, y = j, z = k});
        }

        public static DualQuaternion From(Matrix4x4 m)
        {
            // As proposed by runevision here: https://answers.unity.com/questions/11363/converting-matrix4x4-to-quaternion-vector3.html

            // Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
            Quaternion q = new Quaternion();
            q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] + m[1, 1] + m[2, 2])) / 2;
            q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] - m[1, 1] - m[2, 2])) / 2;
            q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] + m[1, 1] - m[2, 2])) / 2;
            q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] - m[1, 1] + m[2, 2])) / 2;
            q.x *= Mathf.Sign(q.x * (m[2, 1] - m[1, 2]));
            q.y *= Mathf.Sign(q.y * (m[0, 2] - m[2, 0]));
            q.z *= Mathf.Sign(q.z * (m[1, 0] - m[0, 1]));
            
            Vector3 t = m.GetColumn(3);

            return From(q, t);
        }

        // -------------------------------------------------------------------------
        /// @name Constructors
        // -------------------------------------------------------------------------

        // Fill directly the dual quaternion with two quaternion for the non-dual
        // and dual part (in that order):
        public DualQuaternion(Quaternion q0, Quaternion qe)
        {
            _quat_0 = q0;
            _quat_e = qe;
        }

        // -------------------------------------------------------------------------
        /// @name Methods and Properties
        // -------------------------------------------------------------------------

        // normalize in place:
        public void normalize()
        {
            var norm = _quat_0.Norm();
            _quat_0 = _quat_0.DividedBy(norm);
            _quat_e = _quat_e.DividedBy(norm);
        }

        // Get a copy of this dual quaternion which is normalized:
        public DualQuaternion normalized
        {
            get 
            {
                DualQuaternion copy = this;
                copy.normalize();
                return copy;
            }
        }

        // Transformation of point p with the dual quaternion
        public Vector3 Transform(Vector3 p)
        {
            // As the dual quaternions may be the results from a
            // linear blending we have to normalize it:
            var norm = _quat_0.Norm();
            var qblend_0 = _quat_0.DividedBy(norm);
            var qblend_e = _quat_e.DividedBy(norm);

            // Translation from the normalized dual quaternion equals :
            // 2.f * qblend_e * conjugate(qblend_0)
            var v0 = qblend_0.VectorPart();
            var ve = qblend_e.VectorPart();
            Vector3 trans = (ve * qblend_0.w - v0 * qblend_e.w + Vector3.Cross(v0, ve)) * 2.0f;

            // Rotate
            return (qblend_0 * p) + trans;
        }

        // Rotate a vector with the dual quaternion
        public Vector3 Rotate(Vector3 v)
        {
            return _quat_0.normalized * v;
        }

        public Vector3 TranslationVector
        {
            get 
            {
                var norm = _quat_0.Norm();

                // translation vector from dual quaternion part:
                Vector3 t;
                t.x = 2.0f * (-_quat_e.w * _quat_0.x + _quat_e.x * _quat_0.w - _quat_e.y * _quat_0.z + _quat_e.z * _quat_0.y) / norm;
                t.y = 2.0f * (-_quat_e.w * _quat_0.y + _quat_e.x * _quat_0.z + _quat_e.y * _quat_0.w - _quat_e.z * _quat_0.x) / norm;
                t.z = 2.0f * (-_quat_e.w * _quat_0.z - _quat_e.x * _quat_0.y + _quat_e.y * _quat_0.x + _quat_e.z * _quat_0.w) / norm;
                return t;
            }
        }

        // Convert the dual quaternion to a homogenous matrix
        // N.B: Dual quaternion is normalized before conversion
        public Matrix4x4 ToMatrix()
        {
            var norm = _quat_0.Norm();
            var t = TranslationVector;
            return Matrix4x4.TRS(t, _quat_0.DividedBy(norm), Vector3.one);
        }

        // -------------------------------------------------------------------------
        /// @name Operators
        // -------------------------------------------------------------------------

        public static DualQuaternion operator+(DualQuaternion dq1, DualQuaternion dq2)
        {
            return new DualQuaternion(QuaternionUtils.Add(dq1._quat_0, dq2._quat_0), QuaternionUtils.Add(dq1._quat_e, dq2._quat_e));
        }

        public static DualQuaternion operator*(DualQuaternion dq, float scalar)
        {
            return new DualQuaternion(dq._quat_0.MultipliedWith(scalar), dq._quat_e.MultipliedWith(scalar));
        }

        // -------------------------------------------------------------------------
        /// @name Getters and Setters
        // -------------------------------------------------------------------------

        public Quaternion NonDualPart
        {
            get => _quat_0;
            set => _quat_0 = value;
        } 
        public Quaternion Rotation
        {
            get => _quat_0;
            set => _quat_0 = value;
        } 

        public Quaternion DualPart => _quat_e;
        public Quaternion Translation => _quat_e;

        // -------------------------------------------------------------------------
        /// @name Attributes
        // -------------------------------------------------------------------------

        // Non-dual part of the dual quaternion. It also represent the rotation.
        // @warning If you want to compute the rotation with this don't forget
        // to normalize the quaternion as it might be the result of a
        // dual quaternion linear blending
        // (when overloaded operators like '+' or '*' are used)
        private Quaternion _quat_0;

        // Dual part of the dual quaternion which represent the translation.
        // translation can be extracted by computing
        // 2.f * _quat_e * conjugate(_quat_0)
        // @warning don't forget to normalize quat_0 and quat_e :
        // quat_0 = quat_0 / || quat_0 || and quat_e = quat_e / || quat_0 ||
        private Quaternion _quat_e;
    }

}
