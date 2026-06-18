using TestCardGame.Definitions.StatusEffects;
using TestCardGame.Controller.Services;

namespace TestCardGame.Character.StatusEffects
{
    /// <summary>
    /// バトル中にユニットへ付与されている状態異常の実体。
    /// </summary>
    public class StatusEffectInstance
    {
        public StatusEffectSO Definition { get; }
        public int RemainingTurns { get; set; }
        public int Value { get; set; } // 炎上ダメージなどに使う汎用の強度値。

        public bool IsExpired => RemainingTurns <= 0;

        /// <summary>
        /// 状態異常定義、残りターン、強度値を指定して実体を作成する。
        /// </summary>
        public StatusEffectInstance(StatusEffectSO definition, int remainingTurns, int value = 0)
        {
            Definition = definition;
            RemainingTurns = remainingTurns;
            Value = value;
        }

        /// <summary>
        /// 同種の状態異常をこのインスタンスへ統合する。
        /// </summary>
        public void Merge(StatusEffectInstance other)
        {
            Definition.Merge(this, other);
        }

        /// <summary>
        /// ターン開始時処理を状態異常定義へ委譲する。
        /// </summary>
        public void OnTurnStart(IUnit unit, StatusEffectService service)
        {
            Definition.OnTurnStart(unit, this, service);
        }

        /// <summary>
        /// ターン終了時処理を状態異常定義へ委譲する。
        /// </summary>
        public void OnTurnEnd(IUnit unit, StatusEffectService service)
        {
            Definition.OnTurnEnd(unit, this, service);
        }

        /// <summary>
        /// この状態異常下でユニットが行動可能か判定する。
        /// </summary>
        public bool CanAct(IUnit unit)
        {
            return Definition.CanAct(unit, this);
        }
    }
}
