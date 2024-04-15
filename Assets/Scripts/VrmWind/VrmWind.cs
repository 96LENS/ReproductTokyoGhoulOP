using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRM;

namespace WindForVRM
{
    /// <summary>
    /// VRM�̗h����̂ɑ΂��ĕ��𔭐������锭�����̏���
    /// </summary>
    public class VRMWind : MonoBehaviour
    {
        //�ӂ���Ƌ����Ȃ��Ă����܂��A�̓�����\������A�ʂ̕��v�f
        class WindItem
        {
            public WindItem(Vector3 orientation, float riseCount, float sitCount, float maxFactor)
            {
                Orientation = orientation;
                RiseCount = riseCount;
                SitCount = sitCount;
                MaxFactor = maxFactor;

                TotalTime = RiseCount + SitCount;
            }

            public Vector3 Orientation { get; }
            public float RiseCount { get; }
            public float SitCount { get; }
            public float MaxFactor { get; }
            public float TotalTime { get; }
            public float TimeCount { get; set; }

            public float CurrentFactor =>
                TimeCount < RiseCount
                    ? MaxFactor * TimeCount / RiseCount
                    : MaxFactor * (1 - (TimeCount - RiseCount) / SitCount);
        }


        [Tooltip("���̃R���|�[�l���g��VRM�A�o�^�[�ɃA�^�b�`����Ă���A�����ŏ������������ꍇ�̓`�F�b�N���I���ɂ��܂��B")]
        [SerializeField] private bool loadAutomatic = false;

        [Tooltip("���̌v�Z��L�������邩�ǂ���")]
        [SerializeField] private bool enableWind = true;

        [Tooltip("��{�ɂȂ镗�����B���[���h���W�Ŏw�肵�܂��B")]
        [SerializeField] private Vector3 windBaseOrientation = Vector3.right;

        [Tooltip("��������������ƃ����_���ɂ��邽�߂̃t�@�N�^")]
        [SerializeField] private float windOrientationRandomPower = 0.2f;

        //���̋����A�����p�x�A�����オ��Ɨ���������̎��Ԃ��A���ꂼ��S��Random.Range�ɒʂ����߂ɕ��t���̒l�ɂ���
        [SerializeField] private Vector2 windStrengthRange = new Vector2(0.03f, 0.06f);
        [SerializeField] private Vector2 windIntervalRange = new Vector2(0.7f, 1.9f);
        [SerializeField] private Vector2 windRiseCountRange = new Vector2(0.4f, 0.6f);
        [SerializeField] private Vector2 windSitCountRange = new Vector2(1.3f, 1.8f);

        //��L�̋����Ǝ��Ԃ�萔�{����t�@�N�^
        [SerializeField] private float strengthFactor = 1.0f;
        [SerializeField] private float timeFactor = 1.0f;

        private float _windGenerateCount = 0;
        private VRMSpringBone[] _springBones = new VRMSpringBone[] { };
        private Vector3[] _originalGravityDirections = new Vector3[] { };
        private float[] _originalGravityFactors = new float[] { };
        private readonly List<WindItem> _windItems = new List<WindItem>();

        /// <summary> ���̌v�Z��L���ɂ��邩�ǂ������擾�A�ݒ肵�܂��B </summary>
        public bool EnableWind
        {
            get => enableWind;
            set
            {
                if (enableWind == value)
                {
                    return;
                }
                enableWind = value;
                if (!value)
                {
                    DisableWind();
                }
            }
        }

        /// <summary> ���̕��������[���h���W�Ŏ擾�A�ݒ肵�܂��B </summary>
        public Vector3 WindBaseOrientation
        {
            get => windBaseOrientation;
            set => windBaseOrientation = value;
        }

        /// <summary> ���̕����������_�������鋭����0����1���x�͈̔͂Ŏ擾�A�ݒ肵�܂��B </summary>
        public float WindOrientationRandomPower
        {
            get => windOrientationRandomPower;
            set => windOrientationRandomPower = value;
        }

        /// <summary> ���̋�����{���Ƃ��Ď擾�A�ݒ肵�܂��B�傫�Ȓl�ɂ���قǋ������������Ă��鈵���ɂȂ�܂��B </summary>
        public float StrengthFactor
        {
            get => strengthFactor;
            set => strengthFactor = value;
        }

        /// <summary> �ʂ̕��v�f�𐶐�����Ԋu��{���Ŏ擾�A�ݒ肵�܂��B�������l�ɂ���قǁA�����ׂ�����������܂��B </summary>
        public float TimeFactor
        {
            get => timeFactor;
            set => timeFactor = value;
        }


        /// <summary>
        /// �ΏۂƂȂ�VRM�̃��[�g�v�f���w�肵��VRM��ǂݍ��݂܂��B
        /// loadAutomatic���I���ŁA���炩���߂��̃R���|�[�l���g��VRM�ɃA�^�b�`����Ă���ꍇ�A�Ăяo���͕s�v�ł��B
        /// </summary>
        /// <param name="vrmRoot"></param>
        public void LoadVrm(Transform vrmRoot)
        {
            _springBones = vrmRoot.GetComponentsInChildren<VRMSpringBone>();
            _originalGravityDirections = _springBones.Select(b => b.m_gravityDir).ToArray();
            _originalGravityFactors = _springBones.Select(b => b.m_gravityPower).ToArray();
        }

        /// <summary>
        /// VRM��j������Ƃ��A�������̃R���|�[�l���g���j������Ȃ��ꍇ�́A������Ăяo���ă��\�[�X��������܂��B
        /// </summary>
        public void UnloadVrm()
        {
            _springBones = new VRMSpringBone[] { };
            _originalGravityDirections = new Vector3[] { };
            _originalGravityFactors = new float[] { };
        }

        private void Start()
        {
            if (loadAutomatic)
            {
                LoadVrm(transform);
            }
        }

        private void Update()
        {
            if (!EnableWind)
            {
                return;
            }

            UpdateWindGenerateCount();
            UpdateWindItems();

            Vector3 windForce = Vector3.zero;
            for (int i = 0; i < _windItems.Count; i++)
            {
                windForce += _windItems[i].CurrentFactor * _windItems[i].Orientation;
            }

            for (int i = 0; i < _springBones.Length; i++)
            {
                var bone = _springBones[i];
                //NOTE: �͂��������Ď΂߂ɗ͂�������̂��_��
                var forceSum = _originalGravityFactors[i] * _originalGravityDirections[i] + windForce;
                bone.m_gravityDir = forceSum.normalized;
                bone.m_gravityPower = forceSum.magnitude;
            }
        }

        /// <summary> ���̉e�������Z�b�g���ASpringBone��Gravity�Ɋւ���ݒ��������Ԃɖ߂��܂��B </summary>
        private void DisableWind()
        {
            for (int i = 0; i < _springBones.Length; i++)
            {
                var bone = _springBones[i];
                bone.m_gravityDir = _originalGravityDirections[i];
                bone.m_gravityPower = _originalGravityFactors[i];
            }
        }

        /// <summary> ���Ԃ��J�E���g���邱�ƂŁA�K�v�ȃ^�C�~���O�Ń����_���ȋ����ƕ�����������WindItem�𐶐����܂��B </summary>
        private void UpdateWindGenerateCount()
        {
            _windGenerateCount -= Time.deltaTime;
            if (_windGenerateCount > 0)
            {
                return;
            }
            _windGenerateCount = Random.Range(windIntervalRange.x, windIntervalRange.y) * timeFactor;

            var windOrientation = (
                windBaseOrientation.normalized +
                new Vector3(
                   Random.Range(-windOrientationRandomPower, windOrientationRandomPower),
                   Random.Range(-windOrientationRandomPower, windOrientationRandomPower),
                   Random.Range(-windOrientationRandomPower, windOrientationRandomPower)
                    )).normalized;

            _windItems.Add(new WindItem(
                windOrientation,
                Random.Range(windRiseCountRange.x, windRiseCountRange.y),
                Random.Range(windSitCountRange.x, windSitCountRange.y),
                Random.Range(windStrengthRange.x, windStrengthRange.y) * strengthFactor
            ));
        }

        /// <summary> �ʂ�WindItem�ɂ��āA���Ԃ̌o�ߏ�Ԃ��X�V���A�s�v�ȃI�u�W�F�N�g������Δj�����܂��B </summary>
        private void UpdateWindItems()
        {
            //Remove����\��������̂ŋt���ɂ���Ă܂�
            for (int i = _windItems.Count - 1; i >= 0; i--)
            {
                var item = _windItems[i];
                item.TimeCount += Time.deltaTime;
                if (item.TimeCount >= item.TotalTime)
                {
                    _windItems.RemoveAt(i);
                }
            }
        }

    }
}