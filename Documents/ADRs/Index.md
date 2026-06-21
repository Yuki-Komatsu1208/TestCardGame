# ADR Index

このフォルダでは、設計判断やゲーム方針をカテゴリ別に管理します。
1つのADRには1つの判断だけを記録します。
後から方針を変更する場合は、既存ADRを直接上書きせず、新しいADRまたはステータス更新で履歴を残します。

ADR番号はカテゴリフォルダごとに `ADR-001` から採番します。

## Architecture

* [ADR-001: グリッド制ターンバトルおよびカード効果システムの基盤設計](Architecture/ADR-001-grid-turn-battle-card-effect-system.md)

## Enemy

* [ADR-001: 敵行動の対象指定は当面 EnemyActionPlan に閉じ込める](Enemy/ADR-001-enemy-action-targeting.md)

## GameDesign

* [ADR-001: 固定手札・コスト連動クールタイム・Modifier中心成長を第一案とする](GameDesign/ADR-001-fixed-hand-cooldown-modifier-growth.md)
* [ADR-002: シールドカードは永続シールドを前提に、上限付きで扱う第一案とする](GameDesign/ADR-002-shield-card-role-and-stacking-rule.md)
* [ADR-003: 職業を先に固定せず、カード軸とレリック主導でビルドを成立させる第一案とする](GameDesign/ADR-003-build-first-card-axis-over-class.md)
