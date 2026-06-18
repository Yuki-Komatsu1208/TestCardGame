using TestCardGame.Character;
using TestCardGame.Character.StatusEffects;
using TestCardGame.Controller.Services;
using UnityEngine;

namespace TestCardGame.Definitions.StatusEffects
{
    public abstract class StatusEffectSO : ScriptableObject
    {
        [SerializeField] private string effectId;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;

        public string EffectId => effectId;
        public string DisplayName => displayName;
        public string Description => description;

        /// <summary>
        /// エディタ外から状態異常の基本表示情報を設定する。
        /// </summary>
        public void Configure(string id, string name, string desc)
        {
            effectId = id;
            displayName = name;
            description = desc;
        }

        /// <summary>
        /// ターン開始時の効果処理。
        /// </summary>
        public virtual void OnTurnStart(IUnit unit, StatusEffectInstance instance, StatusEffectService service) { }

        /// <summary>
        /// ターン終了時の効果処理。
        /// </summary>
        public virtual void OnTurnEnd(IUnit unit, StatusEffectInstance instance, StatusEffectService service) { }

        /// <summary>
        /// 状態異常中に行動可能か。
        /// </summary>
        public virtual bool CanAct(IUnit unit, StatusEffectInstance instance) => true;

        /// <summary>
        /// 同種の状態異常が重複して付与された際のマージ処理。
        /// </summary>
        public virtual void Merge(StatusEffectInstance current, StatusEffectInstance incoming)
        {
            current.RemainingTurns = Mathf.Max(current.RemainingTurns, incoming.RemainingTurns);
            current.Value = Mathf.Max(current.Value, incoming.Value);
        }
    }
}
