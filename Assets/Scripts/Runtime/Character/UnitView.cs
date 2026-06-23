using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TestCardGame.Character
{
    public class UnitView : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Image image;

        [Header("Animation Frames")]
        [SerializeField] private Sprite[] idleFrames;
        [SerializeField] private Sprite[] attackFrames;

        [Header("Settings")]
        [SerializeField] private float frameDuration = 0.08f;

        private Sprite[] currentAnimation;
        private int currentFrameIndex;
        private float frameTimer;
        private bool isPlayingOnce;
        private Action onCompleteOnce;

        public Sprite[] IdleFrames { get => idleFrames; set => idleFrames = value; }
        public Sprite[] AttackFrames { get => attackFrames; set => attackFrames = value; }

        private void Awake()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            if (image == null)
            {
                image = GetComponent<Image>();
            }
        }

        private void Start()
        {
            // Auto start playing Idle if frames are set
            PlayIdle();
        }

        private void Update()
        {
            if (currentAnimation == null || currentAnimation.Length == 0) return;

            frameTimer += Time.deltaTime;
            if (frameTimer >= frameDuration)
            {
                frameTimer -= frameDuration;
                currentFrameIndex++;
                if (currentFrameIndex >= currentAnimation.Length)
                {
                    if (isPlayingOnce)
                    {
                        isPlayingOnce = false;
                        var callback = onCompleteOnce;
                        onCompleteOnce = null;
                        callback?.Invoke();
                        PlayIdle();
                    }
                    else
                    {
                        currentFrameIndex = 0;
                    }
                }

                if (image != null && currentFrameIndex < currentAnimation.Length)
                {
                    image.sprite = currentAnimation[currentFrameIndex];
                }
            }
        }

        public void Initialize(Sprite fallbackSprite)
        {
            if (image == null)
            {
                return;
            }

            image.enabled = true;
            if (image.sprite == null)
            {
                image.sprite = fallbackSprite;
            }

            PlayIdle();
        }

        public void PlayIdle()
        {
            if (idleFrames != null && idleFrames.Length > 0)
            {
                currentAnimation = idleFrames;
                currentFrameIndex = 0;
                frameTimer = 0f;
                isPlayingOnce = false;
                onCompleteOnce = null;
                if (image != null)
                {
                    image.sprite = currentAnimation[0];
                }
            }
        }

        public void PlayAttack(Action onComplete = null)
        {
            if (attackFrames != null && attackFrames.Length > 0)
            {
                currentAnimation = attackFrames;
                currentFrameIndex = 0;
                frameTimer = 0f;
                isPlayingOnce = true;
                onCompleteOnce = onComplete;
                if (image != null)
                {
                    image.sprite = currentAnimation[0];
                }
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        public void MoveToCell(RectTransform cellRect)
        {
            if (rectTransform == null || cellRect == null)
            {
                return;
            }

            rectTransform.SetParent(cellRect, false);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.SetAsLastSibling();
        }
    }
}
