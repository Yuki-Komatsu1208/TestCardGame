using TestCardGame.Actions.Core;

namespace TestCardGame.Actions.Effects
{
    /// <summary>使用者を指定マスへ直接転移させる。移動不能なら失敗し、位置補正は行わない。</summary>
    public sealed class eTeleport : ActionEffect
    {
        public override bool CanExecute(ActionContext context) => context.MoveService.GetCellAt(context.TargetPosition)?.CanMove == true;

        public override void Execute(ActionContext context)
        {
            if (CanExecute(context)) context.MoveService.Teleport(context.User.ID, context.TargetPosition);
        }
    }
}
