using TestCardGame.Cards.Core;
using TestCardGame.Cards.VOs;
using UnityEngine;

namespace TestCardGame.Cards.Views
{
    /// <summary>単体カードの表示専用。戦闘手札の枠には追加せずHandViewのBindを使う。</summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CardView))]
    public sealed class CardTemplateBinding : MonoBehaviour
    {
        [Header("Card + Image")]
        [SerializeField, Tooltip("既存のCardDefinitionアセットをそのまま指定します。")]
        private CardDefinitionSO cardDefinition;
        [SerializeField, Range(1, 3)] private int level = 1;
        [SerializeField, Tooltip("任意。未設定ならカード定義のArtworkを使用します。")]
        private Sprite artworkOverride;
        [SerializeField] private CardView cardView;

        private bool refreshRequested;

        private void Awake()
        {
            if (cardView == null) cardView = GetComponent<CardView>();
        }

        private void OnEnable() => refreshRequested = true;

        // OnValidate may run on a loading thread. Never change Unity objects here.
        private void OnValidate() => refreshRequested = true;

        private void Update()
        {
            if (!refreshRequested) return;
            refreshRequested = false;
            Refresh();
        }

        /// <summary>指定した定義から表示実体のみを作成。定義の数値や効果を変更しない。</summary>
        public void Refresh()
        {
            if (cardView == null) cardView = GetComponent<CardView>();
            if (cardView == null) return;
            int selectedLevel = Mathf.Clamp(level, 1, 3);
            if (cardDefinition == null || cardDefinition.GetDataForLevel(selectedLevel) == null)
            {
                cardView.Bind(null);
                return;
            }
            cardView.Bind(new CardBase(cardDefinition, new CardLevel(selectedLevel)), artworkOverride);
        }
    }
}
