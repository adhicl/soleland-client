using System;
using Interfaces;
using UnityEngine;

    public class Hammer : MonoBehaviour, IHitable
    {
        [SerializeField] private PlayerController playerParent;
        public void DoScoreHit()
        {
            playerParent.PlayerScoreHit();
        }
    }