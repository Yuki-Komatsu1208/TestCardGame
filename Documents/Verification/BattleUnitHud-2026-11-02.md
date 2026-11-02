# 戦闘UI改修の実装・検証記録

2026-11-02 / Unity 6000.4.11f1 / Built-in / StandaloneOSX向け / EditorのPlay Modeで検証。

## 同日追補: バーをキャラクターの下へ移動

ユーザー指示によりHP・スタンの2段バーを下側へ変更。ユニット下端から12px下へ追従し、HP→スタンの順を維持。盤面paddingは上8px／下28pxへ入れ替え、セルサイズと盤面の実寸は維持した。通常位置と最下段でバー上端がユニット下端より4px下にあり、最下段でも欠けないことをPlay Modeの実座標・画面で確認。Editor生成処理も同じ余白へ更新した。

[下側表示の確認画像](BattleUnitHud-2026-11-02/bars-below.png)。以下の初回検証画像は変更前の頭上表示を含む。

## 実装結果

- HP（赤）・スタン耐性（黄）の細い2段バーをユニット下端へ追従させる。通常時は名前・詳細数値・状態異常・敵行動記号を常設しない。
- 共有ホバー説明1枚へ既存BattleHudPresenterの表示モデルとEnemyIntentViewが保持する既存予告を渡す。別の戦闘状態モデルは追加していない。
- 通常敵・互換単体敵・動的召喚・死亡・同一IDを含むステージ再構築に対応。敵の識別文字はUnitView.unitLabelへ限定した。
- 上部情報48px、最新通知26px。固定PlayerHudと敵一覧は参照を残して非表示。共通UIはBattleSceneのオーバーライドで変更した。
- BoardViewportは手札上端より上の残り領域へ拡大。正方形セル、下側のバー用余白28px、maximumCellSize=4096。手札RectTransform・50枠・フォーカス領域は変更していない。
- PlayerSettings.activeInputHandler=Both（2）。BattleSceneのEventSystemにはInputSystemUIInputModuleがあり、既存のActionsAsset・Point・LeftClick等への配線を実確認した。今回のホバーはIPointerEnter/Exit通知を使う。
- バーと説明の全GraphicはraycastTarget=false。モーダル・カードPreview中・対象の無効化／破棄・退出で説明を閉じる。
- 共有説明は720pの可読性を確認して21基準pxへ調整した。既存日本語フォントを利用し、その動的グリフ／アトラスの保存差分がある。

## 変更対象

引継ぎ時に既に存在した途中実装も含めた、今回の戦闘UIの対象。

- `Assets/Code/Runtime/UIs/Battle/`のBattleUnitHudPresenter、UnitOverheadBarsView、UnitHoverDetailsView、UnitHudHoverTarget、BattleHudView、EnemyIntentView、BoardView。
- `Assets/Code/Runtime/BoardManage/CellBuilder.cs`、`Assets/Code/Runtime/Character/UnitView.cs`、`Assets/Code/Runtime/Controller/BattleUI.cs`。
- `Assets/Code/Editor/BattleUiAssetBuilder.cs`、`BattleUiAssetValidator.cs`。
- `Assets/GameData/Scenes/BattleScene.unity`、`Assets/GameData/Prefabs/UI/Battle/`のBattleHud、UnitOverheadBars、UnitHoverDetails。
- `Assets/UI/Fonts/msgothic SDF.asset`の動的フォントデータ、本検証記録と証跡。

開始時点で多数の未コミット変更があった。既存差分は巻き戻していない。CardTemplate、BattleHand、カード表示／同期コード、CardDefinitionSO、CommonUiRoot、OutpostScene等について、開始時のSHA-256と照合した83ファイルは変更なし。ゲームルール・性能値・効果定義・パッケージは今回変更していない。

## 盤面寸法の比較

1920×1080、5×5、Canvas基準px。旧値は開始時のSceneのViewport・padding・maximumCellSizeをPlay中だけ再現して実測した。改修前の全HUDを撮影した画像ではない。比較後は新設定へ戻し、Play停止後の保存Sceneにも旧設定を残していない。

| 項目 | 旧設定再現 | 改修後 |
| --- | ---: | ---: |
| Viewport | 1843.2 × 421.2 | 1843.2 × 672.8 |
| Board Content | 421.2 × 421.2 | 652.8 × 672.8 |
| 正方形セル | 77.04 × 77.04 | 123.36 × 123.36 |

セルの一辺は約60.1%拡大。Contentの縦横差20pxはバー用paddingによる。セルは歪めていない。

[旧盤面設定の再現](BattleUnitHud-2026-11-02/before-reproduced.png) / [改修後1920×1080](BattleUnitHud-2026-11-02/1920x1080.png) / [1280×720](BattleUnitHud-2026-11-02/1280x720.png) / [1600×1000（16:10）](BattleUnitHud-2026-11-02/1600x1000.png)

## 検証済み

- Prefab／Sceneの再読込後に必須参照を検証。非表示の旧HUD枠の参照とゲージ修正も保持。
- Filled Imageの実在Sprite、Horizontal、左起点、Clampを確認。100/50/0%および範囲外値についてOnPopulateMeshの実頂点幅と期待する残量長が一致。
- 一時フォルダだけでHUD生成を2回、Sceneレイアウト適用を2回実行し、保存YAMLが同一であることを確認。既存HUDを読んで更新し、内部参照IDを再作成しない方式に修正した。
- 本番Sceneには手札を触らない`Apply Compact Battle HUD Only`を使用。`BuildAll`、`InstallIntoScene`、カードテンプレート生成・同期は実行していない。
- Outpost起動→出発→Battle、複数敵、動的召喚、死亡、互換生成、ステージ再構築、Outpost退出後のバー／説明の消去。
- HP／スタン50%、スタン0、炎上の名前・3ターン・値4をホバーで確認。生存IDごとのモデル割合とバー割合を照合。
- 四隅への実際のMoveToCell経路、攻撃スプライト再生、画面変更後の追従・寸法・バーの盤面内配置・説明のCanvas内Clamp。
- 6×6、8×4、4×8を一時的な実行時Stageで確認。既存Stageアセットは変更していない。
- EventSystem.RaycastAllでバーと説明がユニット本体への入力を遮らないことを確認。
- 敵クリック→範囲表示、Preview中の範囲非表示、キャンセル後の復元、再クリック解除。
- 移動カード使用・マナ消費、ターン終了ボタン、使用後のCT表示と2回のターン経過後の使用可能表示。[CT確認画像](BattleUnitHud-2026-11-02/cooldown.png)
- 手札0/20/21/50枚、21枚以降のスクロール、フォーカス拡大。[50枚とフォーカス](BattleUnitHud-2026-11-02/hand-50-focus.png)
- デッキ・設定・報酬表示とドラッグPreviewによる説明の非表示。既存デバッグ攻撃カードで戦闘勝利と報酬画面を確認。[報酬画面](BattleUnitHud-2026-11-02/post-battle.png)
- AssetDatabase.GetDependenciesで直接・再帰依存とPrefab／Scene参照元を確認。[依存調査](BattleUnitHud-2026-11-02/dependencies.txt)

[実行時の照合記録](BattleUnitHud-2026-11-02/runtime-checks.txt)。このログのScreen値はEditorのウィンドウ側の値で、各PNGの実解像度は1920×1080／1280×720／1600×1000を別途確認した。

一時Scene・生成Prefab・検証用C#・metaは除去し、保存SceneをEdit Modeへ戻した。C#のdiffチェックを通過。Unityのシリアライズ形式が作る空値末尾空白については、対象外の既存差分もあるため一括整形していない。

## 検証の限界

入力はEventSystemのレイキャストと既存UIハンドラ／Controller呼出しで検証し、描画は複数フレーム観測とPlay画像で確認した。実マウスで全操作を通す確認、StandaloneOSXのビルド・実機起動、全報酬分岐を含むRUN完走は未実施。既存移動サービスはセルへの即時リペアレント方式であり、今回、移動アニメーションは追加していない。
