using System;
using UnityEngine;

namespace Tbx
{
    public struct DualQuat 
    {
        // -------------------------------------------------------------------------
        /// @name Factory functions
        // -------------------------------------------------------------------------

        static DualQuat From(Quaternion q, Vector3 t)
        {
            float w = -0.5f*( t.x * q.x + t.y * q.y + t.z * q.z);
            float i =  0.5f*( t.x * q.w + t.y * q.z - t.z * q.y);
            float j =  0.5f*(-t.x * q.z + t.y * q.w + t.z * q.x);
            float k =  0.5f*( t.x * q.y - t.y * q.x + t.z * q.w);

            return new DualQuat 
            {
                _quat_0 = q,
                _quat_e = new Quaternion{w = w, x = i, y = j, z = k}
            };
        }

        /// Generates a dual quaternion with no translation and no rotation either
        static DualQuat Identity
        {
            get 
            {
                return From(Quaternion.identity, Vector3.zero);
            }
        }

        // -------------------------------------------------------------------------
        /// @name Constructors
        // -------------------------------------------------------------------------


        public DualQuat()
        {
            var tmp = 
            this._quat_0 = tmp._quat_0;
            this._quat_e = tmp._quat_e;
        }



        // -------------------------------------------------------------------------
        /// @name Operators
        // -------------------------------------------------------------------------

        public static DualQuat operator+(DualQuat q1, DualQuat q2)
        {
            return DualQuat()
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

        /// Non-dual part of the dual quaternion. It also represent the rotation.
        /// @warning If you want to compute the rotation with this don't forget
        /// to normalize the quaternion as it might be the result of a
        /// dual quaternion linear blending
        /// (when overloaded operators like '+' or '*' are used)
        private Quaternion _quat_0;

        /// Dual part of the dual quaternion which represent the translation.
        /// translation can be extracted by computing
        /// 2.f * _quat_e * conjugate(_quat_0)
        /// @warning don't forget to normalize quat_0 and quat_e :
        /// quat_0 = quat_0 / || quat_0 || and quat_e = quat_e / || quat_0 ||
        private Quaternion _quat_e;
    }

}
