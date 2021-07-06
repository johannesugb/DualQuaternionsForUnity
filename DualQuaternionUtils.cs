using System;
using System.Collections.Generic;
using UnityEngine;

namespace DQU
{
    // Utils and extension functions for the DualQuaternion class
    public static class DualQuaternionUtils
    {
        public static DualQuaternion BlendWeighted(IList<DualQuaternion> boneTransformations, IList<float> weights)
        {
            Debug.Assert(boneTransformations.Count > 0);
            Debug.Assert(boneTransformations.Count == weights.Count);
            var nb_joints = boneTransformations.Count; // Number of joints influencing vertex p

            // Init dual quaternion with first joint transformation:
            var dq_blend = boneTransformations[0] * weights[0];

            var q0 = boneTransformations[0].Rotation;

            // Look up the other joints influencing 'p' if any
            for (int j = 1; j < nb_joints; j++)
            {
                var dq = boneTransformations[j];
                var w = weights[j];
                var dt = Quaternion.Dot(dq.Rotation, q0);
                if (dt <= 0.0f) // TODO: Use Quaternion.kEpsilon maybe?
                {
                    w *= -1f;
                }

                var add_dq = dq * w;
                dq_blend = dq_blend + add_dq;
            }

            return dq_blend;
        }

        public static Vector3 TransformPosition(Vector3 p, IList<DualQuaternion> boneTransformations, IList<float> weights)
        {
            var dq_blend = BlendWeighted(boneTransformations, weights);

            // Compute animated position
            return dq_blend.Transform(p);
        }

        public static DualQuaternion ToDualQuaternion(this Vector3 p)
        {
            return DualQuaternion.From(Quaternion.identity, p);
        }

        // Returns a new dual quaternion which is computed from the element-wise
        // minima of the input dual quaternions. (No idea if this makes any sense ^^)
        public static DualQuaternion Min(DualQuaternion a, DualQuaternion b)
        {
            return new DualQuaternion(
                QuaternionUtils.Min(a.Real, b.Real),
                QuaternionUtils.Min(a.Dual, b.Dual)
            );
        }

        // Returns a new dual quaternion which is computed from the element-wise
        // maxima of the input dual quaternions. (No idea if this makes any sense ^^)
        public static DualQuaternion Max(DualQuaternion a, DualQuaternion b)
        {
            return new DualQuaternion(
                QuaternionUtils.Max(a.Real, b.Real),
                QuaternionUtils.Max(a.Dual, b.Dual)
            );
        }
    }
}
