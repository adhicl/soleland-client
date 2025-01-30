using System;
using DG.Tweening;
using Interfaces;
using Unity.VisualScripting;
using UnityEngine;

public class Mole : MonoBehaviour
{
    public float DelaySpring = 3f;
    
    private bool _isStayUp = false;
    private float _timerStayUp = 0f;
    public int index = 0;
    
    public void SpringUp()
    {
        _isStayUp = true;
        _timerStayUp = DelaySpring;
        this.transform.DOMoveY(0f, 0.5f).SetEase(Ease.OutBounce);
    }

    private void SpringDown()
    {
        this.transform.DOMoveY(-3f, 0.5f).SetEase(Ease.OutBounce);
    }
    
    private void GetHit()
    {
        this.transform.DOMoveY(-3f, 0.2f).SetEase(Ease.OutBounce);
        //Vector3 currentPosition = this.transform.position;
        //currentPosition.y = -3f;
        //transform.position = currentPosition;
    }

    private void Update()
    {
        if (_isStayUp)
        {
            _timerStayUp -= Time.deltaTime;
            if (_timerStayUp < 0f)
            {
                SpringDown();
                _isStayUp = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GetHit();
            other.gameObject.GetComponent<IHitable>().DoScoreHit(this.index);
        }
    }
}