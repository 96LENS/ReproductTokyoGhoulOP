using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniVRM10;

namespace VrmWind
{
    public class VrmWindController : MonoBehaviour
    {
        //==============================================================================
        // ��`�E�萔
        //==============================================================================
        public class VRMSpringBoneJointGravityData
        {
            //==============================================================================
            // �ϐ�
            //==============================================================================
            private Vector3 _gravityDirection = new();
            private float _gravityPower = 0f;

            //==============================================================================
            // �v���p�e�B
            //==============================================================================
            public Vector3 GravityDirection => _gravityDirection;
            public float GravityPower => _gravityPower;

            //==============================================================================
            // �R���X�g���N�^
            //==============================================================================
            public VRMSpringBoneJointGravityData(Vector3 gravityDirection, float gravityPower)
            {
                _gravityDirection = gravityDirection;
                _gravityPower = gravityPower;
            }
        }

        //==============================================================================
        // SerializeField�ϐ�
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
        // �ϐ�
        //==============================================================================
        private float _windGenerateIntervalTime = 0;
        private Vector3 _originalExternalForce = new();
        private List<WindItem> _windItemList = new();
        private List<VRM10SpringBoneJoint> _springBoneJointList = new();
        private List<VRMSpringBoneJointGravityData> _vrmOriginalGravityDataList = new();

        //==============================================================================
        // �v���p�e�B
        //==============================================================================
        /// <summary> ���̕��������[���h���W�Ŏ擾�A�ݒ肵�܂��B </summary>
        public Vector3 WindBaseOrientation
        {
            get => _WindBaseOrientation;
            set => _WindBaseOrientation = value;
        }

        /// <summary> ���̕����������_�������鋭����0����1���x�͈̔͂Ŏ擾�A�ݒ肵�܂��B </summary>
        public float WindOrientationRandomPower
        {
            get => _WindOrientationRandomPower;
            set => _WindOrientationRandomPower = value;
        }

        /// <summary> ���̋�����{���Ƃ��Ď擾�A�ݒ肵�܂��B�傫�Ȓl�ɂ���قǋ������������Ă��鈵���ɂȂ�܂��B </summary>
        public float StrengthFactor
        {
            get => _StrengthFactor;
            set => _StrengthFactor = value;
        }

        /// <summary> �ʂ̕��v�f�𐶐�����Ԋu��{���Ŏ擾�A�ݒ肵�܂��B�������l�ɂ���قǁA�����ׂ�����������܂��B </summary>
        public float TimeFactor
        {
            get => TimeFactor;
            set => TimeFactor = value;
        }

        //==============================================================================
        // MonoBehaviour�֐�
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
        // Private�֐�
        //==============================================================================
        /// <summary>
        /// �R���|�[�l���g�̃Z�b�g�A�b�v
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
        /// ���̉e�������Z�b�g
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
            // ���̏����X�V
            _UpdateWindGenerate(ref _windItemList);

            // ���̎��Ԃ��X�V
            _UpdateWindTimes(ref _windItemList);

            // ���̏�񂩂�����Ƌ������v�Z
            var windForce = _GetWindForce(_windItemList);

            // ���𔽉f
            _ApplyWindToSpringBone(windForce);

        }

        /// <summary>
        /// ���̏����X�V
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
        /// WindItem�̎��Ԃ��X�V
        /// </summary>
        /// <param name="windItemList"></param>
        private void _UpdateWindTimes(ref List<WindItem> windItemList)
        {
            // Remove����\��������̂ŋt���ɍs��
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
        /// ���̋������v�Z
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
        /// springBone�֕��𔽉f
        /// </summary>
        private void _ApplyWindToSpringBone(Vector3 windForce)
        {
            _vrmInstance.Runtime.ExternalForce = windForce;
        }


    }// class VrmWindController
}// namespace VrmWind

