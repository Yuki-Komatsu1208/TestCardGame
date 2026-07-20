# ADR Index

このフォルダでは、設計判断やゲーム方針をカテゴリ別に管理します。
1つのADRには1つの判断だけを記録します。
後から方針を変更する場合は、既存ADRを直接上書きせず、新しいADRまたはステータス更新で履歴を残します。

ADR番号はカテゴリフォルダごとに `ADR-001` から採番します。

## Architecture

* [ADR-001: グリッド制ターンバトルおよびカード効果システムの基盤設計](Architecture/ADR-001-grid-turn-battle-card-effect-system.md)
* [ADR-002: 状態管理はC#コードに集約し、Viewは表示だけを担当する](Architecture/ADR-002-code-owned-state-and-view-only-presentation.md)

## Enemy

* [ADR-001: 敵行動の対象指定は当面 EnemyActionPlan に閉じ込める](Enemy/ADR-001-enemy-action-targeting.md)

## GameDesign

* [ADR-001: 固定手札・コスト連動クールタイム・Modifier中心成長を第一案とする](GameDesign/ADR-001-fixed-hand-cooldown-modifier-growth.md)
* [ADR-002: シールドカードは永続シールドを前提に、上限付きで扱う第一案とする](GameDesign/ADR-002-shield-card-role-and-stacking-rule.md)
* [ADR-003: 職業を先に固定せず、カード軸とレリック主導でビルドを成立させる第一案とする](GameDesign/ADR-003-build-first-card-axis-over-class.md)
* [ADR-004: カードレベル・マナコスト・クールタイムを独立した調整軸にする](GameDesign/ADR-004-independent-card-level-cost-cooldown.md)
* [ADR-005: 一般戦闘報酬はMOD中心の三択強化にする](GameDesign/ADR-005-battle-reward-design.md)
* [ADR-006: RUNは複数遠征で構成し、遠征クリア後に帰還か続行を選ぶ](GameDesign/ADR-006-expedition-return-and-push-forward-structure.md)
* [ADR-007: 1ターン中のカード使用枚数はリソースとクールタイムで制限する](GameDesign/ADR-007-multiple-card-plays-per-turn-by-resource.md)
* [ADR-008: 街・ゴールド・OverHuntを含む遠征ループを採用する](GameDesign/ADR-008-town-gold-and-overhunt-loop.md)
* [ADR-009: アイテム3層構造でビルド軸と擬似職業を成立させる](GameDesign/ADR-009-item-tiers-and-build-archetype-guidance.md)
* [ADR-010: 初期キーストーン3種を魔法・騎士・ガンナーで開始する（実装状況あり）](GameDesign/ADR-010-initial-keystone-lineup.md)
* [ADR-011: 初期エンジンは各キーストーンに2〜3個ずつ用意する](GameDesign/ADR-011-initial-engine-lineup.md)
* [ADR-012: 魔法使い・騎士・ガンナーの三すくみコンセプトを明確化する](GameDesign/ADR-012-archetype-triangle-for-mage-knight-gunner.md)
