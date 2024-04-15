using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniVRM10;

namespace VrmWind
{
    public class VrmWindController : MonoBehaviour
    {
        //==============================================================================
        // 定義・定数
        //==============================================================================
        public class VRMSpringBoneJointGravityData
        {
            //==============================================================================
            // 変数
            //==============================================================================
            private Vector3 _gravityDirection = new();
            private float _gravityPower = 0f;

            //==============================================================================
            // プロパティ
            //==============================================================================
            public Vector3 GravityDirection => _gravityDirection;
            public float GravityPower => _gravityPower;

            //==============================================================================
            // コンストラクタ
            //==============================================================================
            public VRMSpringBoneJointGravityData(Vector3 gravityDirection, float gravityPower)
            {
                _gravityDirection = gravityDirection;
                _gravityPower = gravityPower;
            }
        }

        //==============================================================================
        // SerializeField変数
        //==============================================================================
        [SerializeField]
        private Vrm10Instance _vrmInstance = null;
        [Space]
        [SerializeField]
        private Vector3 _WindBaseOrientation = Vector3.zero;
        [SerializeField]
        private float _WindOrientationRandomPower = 0.2f;
        [SerializeField]
        private Vector2 _WindStrengthRange = new Vector2(0.03f, 0.06f);
        [SerializeField]
        private Vector2 _WindIntervalRange = new Vector2(0.7f, 1.9f);
        [SerializeField]
        private Vector2 _WindRiseCountRange = new Vector2(0.4f, 0.6f);
        [SerializeField]
        private Vector2 _WindSitCountRange = new Vector2(1.3f, 1.8f);
        [Space]
        [SerializeField]
        private float _StrengthFactor = 1.0f;
        [SerializeField]
        private float _TimeFactor = 1.0f;

        //==============================================================================
        // 変数
        //==============================================================================
        private float _windGenerateIntervalTime = 0;
        private Vector3 _originalExternalForce = new();
        private List<WindItem> _windItemList = new();
        private List<VRM10SpringBoneJoint> _springBoneJointList = new();
        private List<VRMSpringBoneJointGravityData> _vrmOriginalGravityDataList = new();

        //==============================================================================
        // プロパティ
        //==============================================================================
        /// <summary> 風の方向をワールド座標で取得、設定します。 </summary>
        public Vector3 WindBaseOrientation
        {
            get => _WindBaseOrientation;
            set => _WindBaseOrientation = value;
        }

        /// <summary> 風の方向をランダム化する強さを0から1程度の範囲で取得、設定します。 </summary>
        public float WindOrientationRandomPower
        {
            get => _WindOrientationRandomPower;
            set => _WindOrientationRandomPower = value;
        }

        /// <summary> 風の強さを倍率として取得、設定します。大きな値にするほど強い風が吹いている扱いになります。 </summary>
        public float StrengthFactor
        {
            get => _StrengthFactor;
            set => _StrengthFactor = value;
        }

        /// <summary> 個別の風要素を生成する間隔を倍率で取得、設定します。小さい値にするほど、風が細かく生成されます。 </summary>
        public float TimeFactor
        {
            get => TimeFactor;
            set => TimeFactor = value;
        }

        //==============================================================================
        // MonoBehaviour関数
        //==============================================================================
        private void Start()
        {
            _Setup();
        }

        private void Update()
        {
            _UpdateWind();
        }

        private void OnDisable()
        {
            _DisableWind();
        }

        //==============================================================================
        // Private関数
        //==============================================================================
        /// <summary>
        /// コンポーネントのセットアップ
        /// </summary>
        private void _Setup()
        {
            if (_vrmInstance == null)
            {
                return;
            }

            var springBones = _vrmInstance.SpringBone.Springs;
            if (springBones.Count != 0)
            {
                for (int i = 0; i < springBones.Count; i++)
                {
                    foreach (var joint in springBones[i].Joints)
                    {
                        if (joint == null)
                        {
                            continue;
                        }

                        _springBoneJointList.Add(joint);
                        _vrmOriginalGravityDataList.Add(new(joint.m_gravityDir, joint.m_gravityPower));
                    }
                }
            }
        }

        /// <summary>
        /// 風の影響をリセット
        /// </summary>
        private bool _DisableWind()
        {
            if (_springBoneJointList == null || _springBoneJointList.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < _springBoneJointList.Count; ++i)
            {
                var springBone = _springBoneJointList[i];
                springBone.m_gravityDir = _vrmOriginalGravityDataList[i].GravityDirection;
                springBone.m_gravityPower = _vrmOriginalGravityDataList[i].GravityPower;
            }

            return true;
        }

        private void _UpdateWind()
        {
            // 風の情報を更新
            _UpdateWindGenerate(ref _windItemList);

            // 風の時間を更新
            _UpdateWindTimes(ref _windItemList);

            // 風の情報から向きと強さを計算
            var windForce = _GetWindForce(_windItemList);

            // 風を反映
            _ApplyWindToSpringBone(windForce);

        }

        /// <summary>
        /// 風の情報を更新
        /// </summary>
        private void _UpdateWindGenerate(ref List<WindItem> windItemList)
        {
            _windGenerateIntervalTime -= Time.deltaTime;
            if (_windGenerateIntervalTime > 0f)
            {
                return;
            }

            _windGenerateIntervalTime = Random.Range(_WindIntervalRange.x, _WindIntervalRange.y) * _TimeFactor;

            var windOrientation = (
                _WindBaseOrientation.normalized +
                new Vector3(
                   Random.Range(-_WindOrientationRandomPower, _WindOrientationRandomPower),
                   Random.Range(-_WindOrientationRandomPower, _WindOrientationRandomPower),
                   Random.Range(-_WindOrientationRandomPower, _WindOrientationRandomPower)
                    )).normalized;

            windItemList.Add(new WindItem(
                windOrientation,
                Random.Range(_WindRiseCountRange.x, _WindRiseCountRange.y),
                Random.Range(_WindSitCountRange.x, _WindSitCountRange.y),
                Random.Range(_WindStrengthRange.x, _WindStrengthRange.y) * _StrengthFactor
            ));
        }


        /// <summary>
        /// WindItemの時間を更新
        /// </summary>
        /// <param name="windItemList"></param>
        private void _UpdateWindTimes(ref List<WindItem> windItemList)
        {
            // Removeする可能性があるので逆順に行う
            for (int i = windItemList.Count - 1; i >= 0; i--)
            {
                var item = _windItemList[i];
                item.TimeCount += Time.deltaTime;
                if (item.TimeCount >= item.TotalTime)
                {
                    windItemList.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 風の強さを計算
        /// </summary>
        /// <returns></returns>
        private Vector3 _GetWindForce(List<WindItem> windItemList)
        {
            var windForce = Vector3.zero;
            for (int i = 0; i < _windItemList.Count; i++)
            {
                windForce += windItemList[i].CurrentFactor * windItemList[i].Orientation;
            }

            return windForce;
        }

        /// <summary>
        /// springBoneへ風を反映
        /// </summary>
        private void _ApplyWindToSpringBone(Vector3 windForce)
        {
            _vrmInstance.Runtime.ExternalForce = windForce;
        }


    }// class VrmWindController
}// namespace VrmWind

