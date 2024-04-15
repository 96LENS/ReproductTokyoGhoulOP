using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowBreaker : MonoBehaviour
{
    //==============================================================================
    // SerialzieField�ϐ�
    //==============================================================================
    [SerializeField]
    private GameObject _windowRoot;

    [Space]
    [SerializeField]
    [Tooltip("�e�p�[�c�𓮂����Ƃ��̍ŏ������l")]
    private Vector3 _minMovingTrsf = new();
    [SerializeField]
    [Tooltip("�e�p�[�c�𓮂����Ƃ��̍ő嗐���l")]
    private Vector3 _maxMovingTrsf = new();

    //==============================================================================
    // �ϐ�
    //==============================================================================
    private Transform[] _windowGlassTrsfs;

    //==============================================================================
    // �v���p�e�B
    //==============================================================================

    //==============================================================================
    // MonoBehaviour�֐�
    //==============================================================================
    private void Start()
    {
        _windowGlassTrsfs = _windowRoot.GetComponentsInChildren<Transform>();

        StartCoroutine(_CrackingWindow());
    }

    //==============================================================================
    // Private�֐�
    //==============================================================================
    private IEnumerator _CrackingWindow()
    {
        if (_windowGlassTrsfs == null || _windowGlassTrsfs.Length == 0)
        {
            yield break;
        }

        // Transform�ł��炵�āA�����\�����s��
        foreach (var trsf in _windowGlassTrsfs)
        {
            var x = Random.Range(_minMovingTrsf.x, _maxMovingTrsf.x);
            var y = Random.Range(_minMovingTrsf.y, _maxMovingTrsf.y);
            var z = Random.Range(_minMovingTrsf.z, _maxMovingTrsf.z);

            var pos = trsf.localPosition;
            pos.x += x;
            pos.y += y;
            pos.z += z;

            trsf.localPosition = pos;
        }

        yield return null;
    }

    //==============================================================================
    // Public�֐�
    //==============================================================================


}
