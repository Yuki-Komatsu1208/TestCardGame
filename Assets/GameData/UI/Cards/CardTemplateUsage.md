# カードテンプレートの使い方

土台はUnity uGUIのImage・RectTransform・TextMeshProだけで構成しています。画像生成や追加ライブラリは不要です。

## 単体のカード表示
1. `Assets/GameData/Prefabs/UI/Cards/CardTemplate.prefab`をCanvasの子へドラッグします。
2. ルートの **Card Template Binding** にある **Card Definition** に既存カードアセット（`Assets/GameData/Cards/MoveCard.asset`など）を指定します。
3. **Level** を1〜3から選ぶと、そのレベルの名前・説明・コスト・CTが自動表示されます。
4. イラストは任意です。**Artwork Override**へSpriteを指定すると、その表示だけ画像を変更できます。未指定なら空の画像枠で使用できます。

配置は上からタイトル、画像枠、説明。左上の丸にコスト数値、タイトル下にレベル、右下にCTを表示します。
基本サイズは150×210。拡大したい場合はRectTransformのLocal Scaleを縦横同率にすると文字・丸も一緒に拡大されます。

## 戦闘手札
戦闘手札の既存50枠に同じ土台を適用済みです。HandViewが実際のカードを割り当てるので、手札枠にCardTemplateBindingを追加しないでください。
既存のドラッグ・フォーカス・選択・クールタイムによる減色を維持しています。

個別画像は生成・割当していません。後から画像を使いたくなった場合だけ、既存CardDefinitionアセットの **Card Artwork / Artwork** 欄へSpriteを指定してください。同じカードの手札表示に自動反映されます。
画像ファイルのTexture Typeは **Sprite (2D and UI)** にしてください。画像は比率を保って枠に収めます。

長い名前・説明は手札サイズでは末尾を省略します。元の説明やカード効果・コストは変更しません。MODと残りCTも元のカード実体から表示します。
カード一覧・詳細・ショップのUIは変更していません。

## Editorメニュー
- `Tools > Test Card Game > UI > Build Card Template`: 共通Prefabの土台を更新（割当済みCard Definitionは維持）。
- `Tools > Test Card Game > UI > Apply Card Template To Battle Hand`: 手札50枠の内部表示のみ更新。
- `Tools > Test Card Game > UI > Validate Card Template`: 必須参照と丸形コスト、入力を遮らない設定を検証。

土台の色・枠・テキスト位置はPrefab内の部品ごとに調整可能です。再適用メニューを実行するとレイアウトは既定配置へ戻ります。
