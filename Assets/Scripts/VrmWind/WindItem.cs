using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VrmWind
{
    public class WindItem
    {
        //==============================================================================
        // �ϐ�
        //==============================================================================

        //==============================================================================
        // �v���p�e�B
        //==============================================================================
        public Vector3 Orientation { get; }
        public float RiseCount { get; }
        public float SitCount { get; }
        public float MaxFactor { get; }
        public float TimeCount { get; set; }

        public float TotalTime => RiseCount + SitCount;
        
        public float CurrentFactor => TimeCount < RiseCount
            ? MaxFactor * TimeCount / RiseCount
            : MaxFactor * (1f - (TimeCount - RiseCount) / SitCount);

        //==============================================================================
        // �R���X�g���N�^
        //==============================================================================
        public WindItem(Vector3 orientation, float riseCount, float sitCount, float maxFactor)
        {
            Orientation = orientation;
            RiseCount = riseCount;
            SitCount = sitCount;
            MaxFactor = maxFactor;
        }

    }

}