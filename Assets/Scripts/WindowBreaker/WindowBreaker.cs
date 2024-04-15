using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowBreaker : MonoBehaviour
{
    //==============================================================================
    // SerialzieField変数
    //==============================================================================
    [SerializeField]
    private GameObject _windowRoot;

    [Space]
    [SerializeField]
    [Tooltip("各パーツを動かすときの最小乱数値")]
    private Vector3 _minMovingTrsf = new();
    [SerializeField]
    [Tooltip("各パーツを動かすときの最大乱数値")]
    private Vector3 _maxMovingTrsf = new();

    //==============================================================================
    // 変数
    //==============================================================================
    private Transform[] _windowGlassTrsfs;

    //==============================================================================
    // プロパティ
    //==============================================================================

    //==============================================================================
    // MonoBehaviour関数
    //==============================================================================
    private void Start()
    {
        _windowGlassTrsfs = _windowRoot.GetComponentsInChildren<Transform>();

        StartCoroutine(_CrackingWindow());
    }

    //==============================================================================
    // Private関数
    //==============================================================================
    private IEnumerator _CrackingWindow()
    {
        if (_windowGlassTrsfs == null || _windowGlassTrsfs.Length == 0)
        {
            yield break;
        }

        // Transformでずらして、割れる表現を行う
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
    // Public関数
    //==============================================================================


}
