using System;
using UnityEngine;
using DG.Tweening;


namespace Draft
{
    [Serializable]
    public class TransitionInfo
    {
        public enum TransitionType
        {
            Fade,
            CrossFade,
            Slide,
        }



        [SerializeField] public int m_Duration = 500;
        [SerializeField] public TransitionType m_Type;
        [SerializeField] public Color m_FadeColor = Color.black;

        [SerializeField] public Vector2 m_StartPosition;
        [SerializeField] public Vector2 m_EndPosition;
        
        [SerializeField] public DG.Tweening.Ease m_Ease = DG.Tweening.Ease.InOutQuad;
    }

}

