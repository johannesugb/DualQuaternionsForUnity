using System;
using UnityEngine;

namespace DQU
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
        public static DualQuaternion From(Quaternion r, Vector3 t)
        {
            // // Implementation from https://github.com/brainexcerpts/Dual-Quaternion-Skinning-Sample-Codes:
            // float w = -0.5f*( t.x * r.x + t.y * r.y + t.z * r.z);
            // float i =  0.5f*( t.x * r.w + t.y * r.z - t.z * r.y);
            // float j =  0.5f*(-t.x * r.z + t.y * r.w + t.z * r.x);
            // float k =  0.5f*( t.x * r.y - t.y * r.x + t.z * r.w);
            // return new DualQuaternion(r, new Quaternion{w = w, x = i, y = j, z = k});

            // Alternative implementation:
            var real = r.normalized;
            return new DualQuaternion(
                real,
                (t.ToPureQuaternion() * real).MultipliedWith(0.5f)
            );
        }

        // Generates a dual quaternion from a given transformation matrix:
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

        // Calculates the dot product between two DualQuaternions
        public static float Dot(DualQuaternion a, DualQuaternion b)
        {
            return Quaternion.Dot(a.Real, b.Real);
        }

        // -------------------------------------------------------------------------
        /// @name Constructors
        // -------------------------------------------------------------------------

        // Fill directly the dual quaternion with two quaternion for the non-dual
        // and dual part (in that order):
        public DualQuaternion(Quaternion real, Quaternion dual)
        {
            _quatReal = real;
            _quatDual = dual;
        }

        // -------------------------------------------------------------------------
        /// @name Methods and Properties
        // -------------------------------------------------------------------------

        // normalize in place:
        public void Normalize()
        {
            var norm = Mathf.Max(_quatReal.Norm(), 1e-6f);
            _quatReal = _quatReal.DividedBy(norm);
            _quatDual = _quatDual.DividedBy(norm);
        }

        // Get a copy of this dual quaternion which is normalized:
        public DualQuaternion normalized
        {
            get 
            {
                DualQuaternion copy = this;
                copy.Normalize();
                return copy;
            }
        }

        public static Vector3 TranslationVectorFrom(Quaternion real, Quaternion dual)
        {
            // Translation from the normalized dual quaternion equals :
            // 2.f * qblend_e * conjugate(qblend_0)
            var vr = real.VectorPart();
            var vd = dual.VectorPart();
            Vector3 trans = (vd * real.w - vr * dual.w + Vector3.Cross(vr, vd)) * 2.0f;
            return trans;
        }

        private static Vector3 Transform(Vector3 p, Quaternion real, Quaternion dual)
        {
            // Rotate, then translate:
            return (real * p) + TranslationVectorFrom(real, dual);
        }

        // Transformation of point p with the dual quaternion
        public Vector3 Transform(Vector3 p)
        {
            // As the dual quaternions may be the results from a
            // linear blending we have to normalize it:
            var nrmlzd = normalized;
            return Transform(p, nrmlzd._quatReal, nrmlzd._quatDual);
        }

        public Vector3 TransformUnnormalized(Vector3 p)
        {
            // Who knows what the result of that might be...
            return Transform(p, _quatReal, _quatDual);
        }

        // Rotate a vector with the dual quaternion
        public Vector3 Rotate(Vector3 v)
        {
            return _quatReal.normalized * v;
        }

        public Vector3 TranslationVector
        {
            get 
            {
                var norm = _quatReal.Norm();

                // translation vector from dual quaternion part:
                Vector3 t;
                t.x = 2.0f * (-_quatDual.w * _quatReal.x + _quatDual.x * _quatReal.w - _quatDual.y * _quatReal.z + _quatDual.z * _quatReal.y) / norm;
                t.y = 2.0f * (-_quatDual.w * _quatReal.y + _quatDual.x * _quatReal.z + _quatDual.y * _quatReal.w - _quatDual.z * _quatReal.x) / norm;
                t.z = 2.0f * (-_quatDual.w * _quatReal.z - _quatDual.x * _quatReal.y + _quatDual.y * _quatReal.x + _quatDual.z * _quatReal.w) / norm;
                return t;
            }
        }

        public Vector3 GetTranslation()
        {
            Quaternion t = QuaternionUtils.Multiply(QuaternionUtils.MultipliedWith(Dual, 2.0f), Real.Conjugate());
            return new Vector3(t.x, t.y, t.z);
        }

        // Convert the dual quaternion to a homogenous matrix
        // N.B: Dual quaternion is normalized before conversion
        public Matrix4x4 ToMatrix()
        {
            var norm = _quatReal.Norm();
            var t = TranslationVector;
            return Matrix4x4.TRS(t, _quatReal.DividedBy(norm), Vector3.one);
        }

        // Returns a new DualQuaternion which is the conjugate of this
        public DualQuaternion Conjugate()
        {
            return new DualQuaternion(Real.Conjugate(), Dual.Conjugate());
        }

        // Returns a new DualQuaternion which is the inverse of this
        public DualQuaternion Inverse()
        {
            // var realInverse = Quaternion.Inverse(Real);
            // return new DualQuaternion(
            //     realInverse,
            //     realInverse * Dual * realInverse
            // );

            // GLM's implementation of tdualquat<T, Q>::inverse
            var realConj = Real.Conjugate();
            var dualConj = Dual.Conjugate();
            return new DualQuaternion(
                realConj,
                QuaternionUtils.Add(dualConj, realConj.MultipliedWith(-2.0f * Quaternion.Dot(realConj, dualConj)))
            );
        }

        // -------------------------------------------------------------------------
        /// @name Operators
        // -------------------------------------------------------------------------

        // Adds two dual quaternions, which means element-wise addition
        public static DualQuaternion operator+(DualQuaternion dq1, DualQuaternion dq2)
        {
            return new DualQuaternion(QuaternionUtils.Add(dq1._quatReal, dq2._quatReal), QuaternionUtils.Add(dq1._quatDual, dq2._quatDual));
        }

        // Multiplies every element of the dual quaternion with the given scalar
        public static DualQuaternion operator*(DualQuaternion dq, float scalar)
        {
            return new DualQuaternion(dq._quatReal.MultipliedWith(scalar), dq._quatDual.MultipliedWith(scalar));
        }

        // Multiplies two dual quaternions from left to right
        public static DualQuaternion operator*(DualQuaternion lhs, DualQuaternion rhs)
        {
            // lhs.Normalize();
            // rhs.Normalize();
            return new DualQuaternion(
                QuaternionUtils.Multiply(lhs.Real, rhs.Real),
                QuaternionUtils.Add(QuaternionUtils.Multiply(lhs.Real, rhs.Dual), QuaternionUtils.Multiply(lhs.Dual, rhs.Real))
            );
            // return new DualQuaternion(
            //     QuaternionUtils.Multiply(rhs.Real, lhs.Real),
            //     QuaternionUtils.Add(QuaternionUtils.Multiply(rhs.Dual, lhs.Real), QuaternionUtils.Multiply(rhs.Real, lhs.Dual))
            // );
        }

        // -------------------------------------------------------------------------
        /// @name Getters and Setters
        // -------------------------------------------------------------------------

        public Quaternion Real
        {
            get => _quatReal;
            set => _quatReal = value;
        } 
        public Quaternion Rotation
        {
            get => _quatReal;
            set => _quatReal = value;
        } 

        public Quaternion Dual => _quatDual;
        public Quaternion Translation => _quatDual;

        // -------------------------------------------------------------------------
        /// @name Attributes
        // -------------------------------------------------------------------------

        // Non-dual part of the dual quaternion. It also represent the rotation.
        // @warning If you want to compute the rotation with this don't forget
        // to normalize the quaternion as it might be the result of a
        // dual quaternion linear blending
        // (when overloaded operators like '+' or '*' are used)
        private Quaternion _quatReal;

        // Dual part of the dual quaternion which represent the translation.
        // translation can be extracted by computing
        // 2.f * _quat_e * conjugate(_quat_0)
        // @warning don't forget to normalize quat_0 and quat_e :
        // quat_0 = quat_0 / || quat_0 || and quat_e = quat_e / || quat_0 ||
        private Quaternion _quatDual;
    }

}
