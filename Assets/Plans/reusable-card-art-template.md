# Project Overview
- Game Title: TestCardGame（プロジェクト名。正式タイトルの変更は対象外）
- High-Level Concept: 既存のデータ駆動カード戦闘に、画像を差し替えて使えるカード表示テンプレートを追加する。既存CardDefinitionSOを維持し、タイトル・説明・コストを自動表示する。
- Players: 既存のプレイヤー対敵の戦闘構成を維持。ネットワーク機能追加なし。
- Inspiration / Reference Games: 指定なし。既存UIの羊皮紙・暗い金属色を参照する。
- Tone / Art Direction: 羊皮紙と暗い金属枠のファンタジー調。土台のみ生成し、文字・数字・個別カードのイラストは焼き込まない。
- Target Platform: 現状のStandaloneOSXを維持。
- Screen Orientation / Resolution: 既存Canvasの横長1920×1080基準。戦闘手札の150×210サイズと配置仕様を維持。
- Render Pipeline: Built-in。Unity 6000.4.11f1。

## 合意した範囲
- 既存カードアセットに画像欄を追加する方式を採用。
- 再利用テンプレートPrefabを作成し、戦闘手札にも適用。
- カード一覧・詳細・ショップのレイアウト変更は対象外。
- 個別カードのイラスト生成、戦闘バランス調整、追加ライブラリ導入は行わない。必要性が判明した場合は事前に説明する。

# Game Mechanics
## Core Gameplay Loop
既存のカード選択・ドラッグ・使用・ターン終了を維持する。今回の変更は表示のみで、効果・レベル別性能・MOD・クールタイム・保存データの識別方法を変更しない。画像と安定した情報配置によってカードを識別しやすくする。

## Controls and Input Methods
既存のMoveCardDragHandler、BattleHandCardSlotView、HandViewを利用する。画像・装飾・ラベルはraycastTargetを無効にし、既存のカードルートで入力を受ける。新しい入力処理は追加しない。入力バックエンド変更なし。

# UI
## カードレイアウト
```text
┌──────────────────────┐
│ ( 1 )   カード名       │ ← 左上の丸にコスト数字のみ
│           Lv.1        │ ← 補助情報はタイトル付近
│ ┌──────────────────┐ │
│ │                  │ │
│ │ 差し替えイラスト   │ │
│ │                  │ │
│ └──────────────────┘ │
│ 説明文               │
│ MOD / 残りCT         │
│                CT:1  │
└──────────────────────┘
```
- 上からタイトル、画像、説明の順。コスト円がタイトルを隠さないようにタイトルの左余白を確保する。
- コストの円は縦横同寸を保つ独立UI部品とする。生成画像の装飾だけに依存せず、数字の中央揃えと円形を保証する。
- 生成土台は1枚。文字なし・数字なし・キャラクターなし。イラスト領域と説明領域には強い模様を入れず、後からUIを重ねる。
- 縦横比は既存150:210に合わせる。生成モデルの対応解像度を確認して選択し、非対応なら余白付き生成・クロップで対応する。生成画像が形状に合うことを目視確認する。
- イラストはUnityEngine.UI.Imageでアスペクト比を保持し、画像枠に収める。透過PNGや縦長・横長画像でも引き伸ばさない。
- イラスト未設定時は空の画像枠を表示し、文字と操作は正常に機能する。架空のイラストを自動割り当てしない。
- 日本語テキストには既存のAssets/UI/Fonts/msgothic SDF.asset等のフォント構成を流用する。新しいフォント導入は不要。
- 狭い手札サイズの長文は省略表示し、既存の詳細閲覧経路は維持する。画像欄を追加して説明が枠外へはみ出さないよう確認する。

# Key Asset & Context
## 調査済み
- `Assets/Code/Definitions/Cards/CardDefinitionSO.cs`: cardName、level1〜3のdescription/cost/cooldown/effectsを保持。現在画像欄なし。
- `Assets/Code/Runtime/Cards/Core/CardBase.cs`: Definition、Level、GetCost(player)、GetCooldown(player)、残りCT、MODを保持。新しいゲームデータモデルは不要。
- `Assets/Code/Runtime/Cards/Views/CardView.cs`: Bind(CardBase)、RefreshView()、ConfigureInteraction(GameController)。現在nameText/costText/descriptionTextのみ。全Graphicの色をAwake/Bind時にキャッシュし、CTによる色変更を行う。
- `Assets/Code/Runtime/Cards/Views/HandView.cs`: 50枠事前配置、150幅、重なり、20枚超スクロール、戦闘中カード選択を管理。仕様変更なし。
- `Assets/Code/Runtime/UIs/Battle/BattleHandCardSlotView.cs`: カード割り当て、フォーカス、選択入力を管理。可能な限り変更しない。
- `Assets/GameData/Prefabs/UI/Battle/BattleHand.prefab`: 50枠それぞれにCardViewが存在。BattleSceneが参照。
- `Assets/Code/Editor/BattleUiAssetBuilder.cs`: CreateCardSlot()で旧レイアウトを生成。再生成時に新レイアウトが失われないよう更新が必要。
- `Assets/GameData/Cards/MoveCard.asset`ほか21個のカード定義を確認。既存GUIDと効果・数値を維持する。
- CardViewは旧CardPrefab.prefab/CardItem.prefabにも参照される。旧参照・旧表示モードを維持し、今回の対象外UIを壊さない。
- 依存関係専用ツールが現在公開されていないため、調査ではAssetDatabase.GetDependenciesによる依存・参照元確認を実施。実装前にも利用可能な依存関係取得手段で再確認する。
- `Assets/Plans/`の既存計画は見つからなかったため、このファイルを本件の初回計画とする。

## 作成・変更予定
- 修改: `Assets/Code/Definitions/Cards/CardDefinitionSO.cs`に任意の`Sprite artwork`欄を追加。旧データはnullで読み込める。
- 修改: `Assets/Code/Runtime/Cards/Views/CardView.cs`にartworkImageとテンプレート表示用の任意参照・表示モードを追加。既存Bind/RefreshView APIを維持。
- 新規: `Assets/Code/Runtime/Cards/Views/CardTemplateBinding.cs`。InspectorでCardDefinitionSO、レベル（1〜3）、必要時の画像上書きを指定して単体表示できる薄いバインダー。ゲーム中にHandViewがカードをBindする枠には自動バインダーを置かず、実体のレベル・MOD・CTを優先する。
- 新規: `Assets/Code/Editor/CardTemplateAssetBuilder.cs`。テンプレート共通レイアウトの作成と、既存手札への限定適用を行う冪等なEditor処理。
- 新規生成画像: `Assets/GameData/UI/Cards/CardTemplateBase.png`（土台1枚のみ）。実装時に生成機能・対応モデルを確認し、完成画像の確認後に接続する。生成機能が利用できなければ報告し、生成済みと偽らない。
- 新規Prefab: `Assets/GameData/Prefabs/UI/Cards/CardTemplate.prefab`。RectTransformルートの再利用部品。利用時はCanvas配下へ配置する。
- 修改: `Assets/GameData/Prefabs/UI/Battle/BattleHand.prefab`の50枠の内部表示のみ。
- 修改: `Assets/Code/Editor/BattleUiAssetBuilder.cs`のカード枠生成を共通テンプレートレイアウトへ合わせる。
- 条件付き修改: `Assets/GameData/Scenes/BattleScene.unity`。Prefab反映を確認し、関連オーバーライドの調整が必要な場合のみ保存。
- 新規: `Assets/Code/Editor/CardTemplateAssetValidator.cs`。参照・円形・50枠の整合性検証。
- 新規: `Assets/GameData/UI/Cards/CardTemplateUsage.md`。カード＋画像だけで設定する使い方を記載。

# Implementation Steps
1. **現状再確認と影響範囲固定**
   - 上記スクリプト、Prefab、Scene、カード定義を再読込し依存を確認。
   - 既存参照とユーザーのレイアウト変更・フォント指定・手札オーバーライドを記録する。
   - Dependencies: 計画承認。
2. **カード画像データと表示の追加**
   - CardDefinitionSOに任意Sprite欄を追加。既存のフィールド名・GUID・効果は変更しない。
   - CardViewでDefinition.artworkを表示し、Bind(null)や画像未設定時に前カードの画像を残さない。
   - テンプレートモードだけコスト数字を独立円へ表示し、CT・レベル・MOD情報を別ラベル／説明で保持する。旧Prefabでは任意参照がnullでも従来表示を維持する。
   - 描画部品はAwakeより前に構成し、色キャッシュが画像・枠も含むようにする。
   - 単体用バインダーを追加。OnValidateでは永続アセットやゲーム数値を書き換えない安全なプレビュー方針とし、実行時に明示的にBindする。
   - Dependencies: Step 1。
3. **共通テンプレートレイアウトを仮部品で構成**
   - Editor builderとPrefabを追加し、既存UI部品による土台・円・画像・ラベルで形状と接続を確認。
   - 150×210と同縦横比の拡大表示で、タイトル／画像／説明／左上円を確認。
   - Dependencies: Step 2。
4. **土台画像1枚を生成・確認**
   - 生成モデルを取得し、UI用Sprite、対応解像度、透過／クロップ可否を確認。
   - 羊皮紙・暗い金属枠、文字なし、数字なし、空のイラスト欄・説明欄を具体的に指定する。
   - 生成中にC#を書き換えない。完成まで待って実画像を目視確認し、プレースホルダーを接続しない。
   - 生成がバックグラウンド継続の場合はその時点で報告し、後のターンで完成確認する。
   - Sprite Single、適切なフィルタ、ミップ不要を確認し共通Prefabへ接続。円のUI部品は独立して真円を保証する。
   - Dependencies: Step 3。
5. **既存戦闘手札へ限定適用**
   - 既存50枠ルート、CardView、DragHandler、CanvasGroup、SlotView、HandView参照を維持したまま内部レイアウトを更新。
   - 既存Name/Cost/Descriptionオブジェクトを再利用し、画像・補助情報だけ追加する。階層全体や手札ルートを破棄再生成しない。
   - BattleUiAssetBuilderのCreateCardSlotを共通レイアウト使用へ変更し、再生成時の再発を防ぐ。BuildAllやInstallIntoSceneを修正適用のために実行しない。
   - BattleSceneでPrefab継承・関連オーバーライドを確認。HUD、体力／スタンバー、一覧・ショップは変更しない。
   - Dependencies: Steps 3–4。
6. **検証と利用手順の整備**
   - Validatorと短い使用ガイドを追加。実際のテストは必要なテスト手順を確認してから実施。
   - 「カード定義のartworkへSpriteを指定→手札は自動表示」「単体PrefabのcardDefinitionを指定→名前／説明／コスト自動表示」を案内。
   - 一時検証データ・スクリプト・画像を片付け、非テストのカード値を変更していないことを確認。
   - Dependencies: Step 5。

# Verification & Testing
- コンパイルエラーなし。既存21カードが同じGUID・カード名・効果・コスト・CTで読める。
- 単体Prefabへ既存MoveCard等を指定し、名前・Lv1〜3説明・コストがカード定義と一致する。
- 仮の既存Spriteをテストで使用し、画像あり／なし／縦長／横長／透過画像を確認。テスト用割当は本番カードへ保存しない。
- 別カードへのBind、Bind(null)、再Bindで画像や文字が残らない。
- コスト0、1、2桁でも円内に収まり、タイトルと重ならない。円の縦横寸法一致。
- CTとMOD情報を維持し、クールタイム時の減色と復帰が枠・画像を含め正常。
- 長い日本語名・説明・MOD複数で枠外にはみ出さず、既存の日本語フォント構成を維持。
- 50枠すべてが新表示の必須参照を持つ。手札0、1、20、21、50枚で従来の重なり・スクロール・フォーカスを維持。
- Play Modeでは通常のOutpost開始から戦闘へ進む経路を確認する。カード使用、ドラッグ、移動選択、戦闘中選択、ターン終了の回帰確認。
- Game Viewの画面でタイトル→画像→説明の順、左上の丸形コスト、土台の可視性を確認。プログラム上の参照確認だけで完了としない。
- Prefab再読込・Scene再読込後も見た目と参照を維持。既存HUDの体力／スタンバー修正が残っている。
- Editor生成レイアウトは一時検証環境で再生成・再実行して冪等性を確認し、本番Sceneへの破壊的BuildAll実行を避ける。

---
# 戦闘UI改修：今回の実装対象（追補）
前半はカードテンプレート作業の履歴であり、再実行しない。今回の承認・実装範囲は以下のみ。以前の土台画像生成は取り消し済み。カードPrefabの手動編集保護および手札へのテンプレート反映方式はCodexの担当として変更しない。

# Project Overview
- Game Title: TestCardGame（正式名称の変更なし）。
- High-Level Concept: カード戦闘の盤面を優先したUIへ整理する。HPとスタン耐性は各ユニットの頭上に常時表示し、個体の詳細はホバー時にのみ提示する。
- Players: 既存の1プレイヤー対敵の戦闘を維持。
- Inspiration / Reference Games: 特定ゲームの指定なし。情報の段階的開示により盤面を読みやすくする。
- Tone / Art Direction: 既存の羊皮紙・金属色のuGUIを維持。HPは赤、スタン耐性は黄の細い2段バー。
- Target Platform: StandaloneOSX。
- Screen Orientation / Resolution: Landscape、既存1920×1080基準。異なる画面比率でも表示範囲内へフィット。
- Render Pipeline: Built-in、Unity 6000.4.11f1。

## ユーザー決定と対象外
- HPとスタン耐性の2本を頭上に常時表示。
- 名前、詳細数値、状態異常、個体の敵行動情報はキャラクターホバー中に表示。
- UIに必要なC#接続修正はこちらで実施してよい。
- 手札のサイズ・操作を維持し、残りの領域でボードを最大化する。
- プレイヤーの使用可能マナはカード操作に必須の全体リソースとして細い上部帯に表示。敵や個体の固有情報は上部へ並べない。
- ゲームルール、盤面の行列数、性能、ダメージ、ターン処理、カードPrefabの見た目・生成・同期方式は変更しない。
- ライブラリ追加・画像生成は不要。

# Game Mechanics
## Core Gameplay Loop
カードの選択・ドラッグ・使用、移動先確認、敵行動範囲確認、ターン終了を維持。広い盤面と常時見える残量バーで判断しやすくし、状態異常等の詳細だけをホバーで段階的に開示する。敵クリックによる範囲選択とカードプレビューの優先順位は維持する。
## Controls and Input Methods
キャラクターのIPointerEnterHandler/IPointerExitHandler等による既存EventSystem対応のホバー通知を使う。ポーリングによるLegacy Inputは追加しない。実装前にPlayerSettingsの入力バックエンドとInputSystemUIInputModuleの接続を実確認する。バーと説明パネルのGraphicはraycastTarget=falseとし、ドラッグ・セル操作・敵クリックを遮らない。モーダル表示、ドラッグ中、ユニット破棄時にはホバー説明を閉じる。

# UI
```text
┌ TURN / 遠征・ステージ / マナ / 保留G / [デッキ][終了][設定] ┐
├ 最新通知の細い1行（既存ログへの接続を維持）───────────┤
│                                                        │
│          最大フィットする戦場・ボード                    │
│             ─── HP ───                                │
│             ── スタン ─                                │
│             キャラクター                               │
│                 ┌ ホバー時のみ ──────────┐             │
│                 │ 名前・HP/スタン数値     │             │
│                 │ 状態異常・残りターン    │             │
│                 │ 敵行動などの個体情報    │             │
│                 └──────────────────────┘             │
├────────────────────────────────────────────────────────┤
│              既存手札（配置・サイズ維持）               │
└────────────────────────────────────────────────────────┘
```
- 上部は情報帯40〜48基準px＋通知24〜28pxを初期目標とする。実画面の日本語可読性とボタンの当たり判定を確認して最終調整。
- 大きなPlayerHudと敵一覧を常設表示しない。参照先を無断で破棄せず、表示経路を置換して既存検証・Renderの期待と整合させる。
- 頭上バーは細い2段、HPとスタンの順。背景・残量色で判別できるようにし、通常は詳細テキストを出さない。
- Filled Imageには実在するSpriteを設定し、FillMethod=Horizontal、左起点、0〜1へClamp。以前のバー不具合を再発させない。
- バーの位置はユニット表示の上端近くへ追従し、画面端・最上段でも欠けないようボード内に余白を確保する。
- 共有ホバーパネルはCanvas上のオーバーレイとして1枚用意し、画面内へClamp。移動中や状態変化時も対象と内容を更新する。
- ボードは残り領域に正方形セルを最大フィット。盤面が正方形なら画面横幅いっぱいに引き伸ばさず、利用可能な高さが上限になる。左右余白を無理に埋めるためのルール変更はしない。
- 既存手札RectTransform・50枠・フォーカス拡大領域を尊重する。カードが拡大する際の盤面下側との重なりは許容するが、操作対象を遮らない。

# Key Asset & Context
## 調査結果
- `Assets/GameData/Scenes/BattleScene.unity`: BoardViewport、Board、Playerテンプレート、BattleHud、CommonUiRoot、手札を保持。現在実際に読み込まれているのはOutpost側だったため、実装前にBattleSceneを明示的に確認する。
- `Assets/Code/Runtime/UIs/Battle/BoardView.cs`: ApplyLayoutでBoardLayoutCalculatorを利用、maximumCellSize既定96。自動フィット上限が盤面拡大を妨げる。ResizeUnitViewsはセルサイズに対して0.78倍を設定。
- `Assets/Code/Runtime/Character/UnitView.cs`: MoveToCellでセルへリペアレント、アンカーをStretchへ設定する。頭上バーやスケールは実際の移動経路と合わせて検証する必要がある。
- `Assets/Code/Runtime/BoardManage/CellBuilder.cs`: Playerテンプレートを複製して通常・互換・動的召喚の敵を作成。IUnitとViewの対応辞書を保持。敵の表示文字をGetComponentInChildren<TextMeshProUGUI>()で探して書き換えているため、新しい頭上ラベルへ誤代入しないよう明示的な既存表示文字参照へ限定する。
- `Assets/Code/Runtime/UIs/Battle/EnemyIntentView.cs`: 常時頭上記号、選択枠、IPointerClickHandler。クリック範囲表示は残し、個体の常時記号はホバー説明へ移す。
- `Assets/Code/Runtime/UIs/Battle/EnemyIntentPresenter.cs`: BoardStateChangedを購読しLateUpdateで更新。カードプレビュー中は敵範囲を消す優先制御を維持。
- `Assets/Code/Runtime/Controller/BattleUI.cs`、BattleHudPresenter、BattleHudView、UnitHudSlotView: 現在の戦闘状態→表示モデル→固定HUDの経路。状態取得を流用し、二重の状態モデルを作らない。
- `Assets/Code/Runtime/UIs/CommonUiController.cs`: statusBar/messageLog/settingsModalの必須参照とイベント購読。BattleSceneのレイアウト上書きに限定し、Outpostの共通Prefabは改変しない。
- `Assets/Code/Runtime/UIs/TooltipView.cs`: Camera.main.WorldToScreenPointを直接利用。今回のキャラクター説明にはCanvas座標対応の専用共有パネルを使い、既存セルツールチップと表示競合を抑制する。
- 専用依存ツールが公開されていない場合、AssetDatabase.GetDependenciesによる直接・再帰依存と参照元調査で補う。既にBattleScene/BattleHudの再帰依存を確認した。各変更前に最新状態と影響範囲を再確認する。

## 作成・変更候補（最小範囲に絞る）
- 新規 `Assets/GameData/Prefabs/UI/Battle/UnitOverheadBars.prefab`: 2本のバーと背景。エディターで見た目を編集できる構成。
- 新規 `Assets/GameData/Prefabs/UI/Battle/UnitHoverDetails.prefab`: 名前・詳細数値・状態異常・敵行動の共有説明欄。
- 新規 `Assets/Code/Runtime/UIs/Battle/UnitOverheadBarsView.cs`、`UnitHoverDetailsView.cs`、`BattleUnitHudPresenter.cs`: Viewは表示のみ、PresenterはIUnit/Viewの対応と状態・ホバー・追従を仲介。
- 必要な接続のみ変更: CellBuilder.cs、BattleUI.cs、BattleHudPresenter.cs、BattleHudView.cs、EnemyIntentView.cs／EnemyIntentPresenter.cs。
- BoardView.csまたはBoardLayoutCalculatorは、InspectorのmaximumCellSize引き上げとViewport余白調整だけで足りれば変更しない。リサイズ通知やユニットのStretch寸法と干渉する場合のみ表示レイアウトを修正。
- BattleScene.unity、BattleHud.prefab: 細帯・ボタン位置・頭上バー接続・ボード範囲。
- BattleUiAssetBuilder／BattleUiAssetValidator: 今回の戦闘HUDレイアウトだけ再生成・検証で失われないよう最小修正。カード生成部分は触らずBuildAllやInstallIntoSceneを既存Sceneへの適用に実行しない。
- CommonUiRoot.prefab、CardTemplate.prefab、BattleHand.prefabのカード内部、CardTemplateAssetBuilder、CardTemplateBinding、CardDefinitionSOは対象外。

# Implementation Steps
1. **最新の配線・依存確認**
   - BattleScene、上記スクリプト、HUD/Player/Boardの参照とオーバーライドを確認。PlayerSettingsとEventSystemを確認。
   - 変更対象と対象外のファイルを記録。CellBuilder通常・互換・召喚・ステージ再生成のView生成経路を全部確認。
   - Dependencies: この追補の承認。
2. **頭上バーとホバーパネルのPrefab作成**
   - Canvasを確認し、既存カラー・TMPフォントで細い2段Filledバーと説明Viewを作成。画像生成なし。
   - 個体情報はPrefabへ固定保存しない。バーはraycast無効、共有説明は非表示で初期化。
   - Dependencies: Step 1。
3. **ユニットへのデータ接続・追従・ホバー処理**
   - IUnitとUnitViewをIDで関連付け。HP/スタン/状態変化、移動、敵召喚、死亡、ステージ再構築で更新。
   - 通常・互換・動的召喚に同じ接続。GetComponentInChildrenによる敵文字の誤代入を防ぐ。
   - 敵行動詳細は既存プレビューから表示、敵クリック範囲とカードプレビュー優先は維持。
   - イベント購読解除、ホバー対象破棄、モーダル/ドラッグによる非表示を実装。
   - Dependencies: Steps 1–2。
4. **上部情報を薄い帯へ集約**
   - BattleHudの全体情報・マナ・デッキ・ターン終了・設定を配置。最新通知を1行へ縮小する。
   - 固定個体HUDを非表示／表示経路置換。参照検証とRenderを合わせ、不要なログや重複バーを出さない。
   - Outpostへ影響する共通Prefabは変更せずBattleSceneのオーバーライドまたは戦闘専用設定で対応。
   - Dependencies: Step 3。
5. **残り領域にボードを最大フィット**
   - BoardViewportの上端を情報帯下、下端を既存手札上へ拡大。背景パネルも整合させる。
   - maximumCellSizeの上限96を緩和し、自動フィットで盤面全体が収まる最大サイズにする。セルを正方形のまま維持。
   - 最上段の頭上バー余白、共有説明の画面内Clamp、ユニット実寸の移動・リサイズ干渉を確認。
   - Dependencies: Step 4。
6. **検証・保存・引き継ぎ**
   - 検証済みPrefab/Sceneを保存。生成メニューは一時検証で冪等性を確認し、本番のカード編集を上書きしない。
   - 変更ファイル・見た目の確認結果・未検証事項を報告。テストだけに使った一時素材・コード・実体を除去。
   - Dependencies: Steps 2–5。

# Verification & Testing
- Edit Mode: 必須参照、Filled Sprite/左起点/比率、raycast、Canvas、EventSystem、全体情報の重複、変更対象外ファイルの差分なし。
- HPとスタンを100%、50%、0%へ変更したとき、頭上バー実際の長さとモデルの割合が一致。状態異常は非ホバー時は表示されず、ホバー時に名前・残りターン・値を確認できる。
- 最上段・四隅・隣接ユニットで頭上バーが切れず、説明が画面外へ出ない。共有パネルがマウスを遮ってホバーの点滅を起こさない。
- 通常Outpost→Battle、複数敵、動的召喚、死亡、ステージ遷移で対応IDと残量を確認。退出後に古い説明やイベント購読が残らない。
- 移動／攻撃アニメーション中に追従し、バーだけ前セルに残らない。ユニットのセルへのリペアレントでサイズが二重に増えない。
- デッキ閲覧、設定、報酬等のモーダルではホバーを閉じる。カードドラッグ中のツールチップと範囲表示の競合なし。
- カード使用、移動先選択、敵クリック範囲、選択解除、ターン終了、CT更新、手札0/20/21/50枚とフォーカスの回帰確認。
- 1920×1080、1280×720、16:10で可読性・細帯ボタン・盤面全体と手札が収まることを確認。5×5以外の既存Stage.boardSizeでも最大フィット。
- 改修前後のボードの実寸を記録し、明確に拡大されていることを数値とPlay Modeスクリーンショットで確認。横長画面の余白を埋めるためにセルを歪めない。
- Play Modeテストは適切な複数フレーム観測・画面確認を実施する。生成コードやテストスクリプトの保存を伴う場合は実装承認後のみ実行。
- CardTemplate.prefabの手動編集、既存カード性能、Outpost UIに変更がないことを最後に検証。
