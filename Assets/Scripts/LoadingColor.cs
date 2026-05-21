using System.Collections;
using UnityEngine;

public class LoadingColor : MonoBehaviour
{
    [SerializeField]
    private Color[] _possibleColors;

    [SerializeField]
    private float _changeColorTime;

    private bool _changeColor;
    private SpriteRenderer _sprRend;

    void Awake()
    {
        _sprRend = GetComponent<SpriteRenderer>();
    }

    public void StartChangeColor()
    {
        _sprRend.enabled = true;
        _changeColor = true;
        StartCoroutine(ChangeColor());
    }

    public void StopChangeColor()
    {
        _changeColor = false;
        _sprRend.enabled = false;
        StopAllCoroutines();
    }

    private IEnumerator ChangeColor()
    {
        while (_changeColor)
        {
            Color randColor = _possibleColors[Random.Range(0, _possibleColors.Length)];

            if (_sprRend != null)
                _sprRend.color = randColor;

            yield return new WaitForSeconds(_changeColorTime);
        }
    }
}
