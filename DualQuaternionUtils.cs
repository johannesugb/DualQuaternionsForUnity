using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tbx
{
    // Utils and extension functions for the DualQuaternion class
    public static class DualQuaternionUtils
    {
        public static Vector3 TransformPosition(Vector3 p, IList<DualQuaternion> boneTransformations, IList<float> weights)
        {
            Debug.Assert(boneTransformations.Count > 0);
            Debug.Assert(boneTransformations.Count == weights.Count);
            var nb_joints = boneTransformations.Count; // Number of joints influencing vertex p

            // Init dual quaternion with first joint transformation:
            var dq_blend = boneTransformations[0] * weights[0];

            var q0 = boneTransformations[0].Rotation;

            // Look up the other joints influencing 'p' if any
            for(int j = 1; j < nb_joints; j++)
            {
                var dq = boneTransformations[j];
                var w  = weights[j];
                if(Quaternion.Dot(dq.Rotation, q0) < 0f)
                {
                    w *= -1f;
                }

                dq_blend = dq_blend + dq * w;
            }

            // Compute animated position
            return dq_blend.Transform(p);
        }
    }
}
