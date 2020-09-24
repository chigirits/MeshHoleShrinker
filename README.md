# MeshHoleShrinker

メッシュに開いた穴を縮めるためのシェイプキーを追加し、新しいメッシュとして保存するUnityエディタ拡張です。

Unity上でアバター頭部を他の素体に接合するとき、接合部にすきまが開いてしまうのを防ぐなどの用途に有用です。

![demo](https://repository-images.githubusercontent.com/295406109/a2eb0680-f6d5-11ea-9672-da464eeef28f)

## 動作環境

Unity 2018.4 以降

## インポート手順

### unitypackageをインポートする方法

[Releasesページ](https://github.com/chigirits/MeshHoleShrinker/releases) より最新版の `MeshHoleShrinker-vX.X.X.unitypackage` をダウンロードし、Unityにインポートする

### パッケージマネージャを用いる方法

1. インポート先プロジェクトの `Packages/manifest.json` をテキストエディタ等で開く
2. `dependencies` オブジェクト内に以下の要素を追加
   
   ```
   "com.github.chigirits.MeshHoleShrinker": "https://github.com/chigirits/MeshHoleShrinker.git",
   ```

こちらの方法でインポートした場合、以下の説明文中で示される本パッケージのプレハブやプリセットは `Assets/Chigiri/MeshHoleShrinker/...` 下ではなく `Packages/MeshHoleShrinker/Assets/Chigiri/MeshHoleShrinker/...` 下から選択してください。

## 使い方

1. シーンにアバターを配置し、頭部以外を非表示にしてください。
2. 「メニュー/Chigiri/Create MeshHoleShrinker」を選択すると、ヒエラルキーのトップレベルに MeshHoleShrinker が配置されます（一部販売モデルは[プリセット](#プリセットについて)を利用できますのでそちらをご利用ください）。
3. MeshHoleShrinker の `Target` に、アバター頭部の SkinnedMeshRenderer（一般的には Body オブジェクト）を指定してください。<br>
   このとき、対象にアタッチされているメッシュが `Source Mesh` に自動的にセットされます。
4. `New Shape Key Name` に追加するシェイプキーの名前を指定してください（通常はデフォルトのままで問題ありません）。
   
   ![usage-01](https://user-images.githubusercontent.com/61717977/93324194-87583b00-f850-11ea-857b-50700a16e84b.png)
5. Shaded Wireframe 表示にした状態で MeshHoleShrinker の位置・角度・スケールを変更し、縮めたい穴を構成する頂点が半透明の円筒にすべて含まれるように調整してください。
   
   ![usage-02](https://user-images.githubusercontent.com/61717977/93084788-89df5700-f6cf-11ea-8d8b-5166c0bbc20a.png)<br>
   穴の辺ループ上にない頂点や舌などを巻き込まないよう注意し、必要十分な頂点が含まれているかを全方位から眺めて確認してください。<br>
   ![usage-03](https://user-images.githubusercontent.com/61717977/93084790-8a77ed80-f6cf-11ea-9f0a-491ec3954c05.png)
6. `Process And Save As...` ボタンを押して、生成された新しいメッシュを保存してください。
   
   ![usage-04](https://user-images.githubusercontent.com/61717977/93324199-88896800-f850-11ea-85b9-c7173f566ba7.png)


   保存が完了すると、`Target` の SkinnedMeshRenderer に新しいメッシュがアタッチされます。
   この差し替えられたメッシュに新しいシェイプキーがウェイト100で追加されていることを確認してください。
   
   ![usage-05](https://user-images.githubusercontent.com/61717977/93084798-8c41b100-f6cf-11ea-88b7-012b4444ccda.png)
   
   このとき、穴が均一に縮んでいないなど期待した結果と異なる場合は Undo して 5. からやり直してください。必要な頂点がすべて含まれていないと円形に縮まず、余計な頂点が含まれていると捻じれが起こりやすいです。

### 処理後の調整

追加されたシェイプキーのウェイトは、必要最低限の値に設定することを推奨します。ウェイトを大きくし過ぎる（穴を小さくし過ぎる）と、穴の周辺ポリゴンが重なり合うなど表示上の不都合が発生しやすいです。

### プリセットについて

以下の販売モデルは、あらかじめ位置合わせ済みのプリセットを `Assets/Chigiri/MeshHoleShrinker/Prefabs/Presets/` 下に用意しています。モデルのプレハブを未加工のまま原点に配置した状態で適用してください。

- [みみの](https://booth.pm/ja/items/1336133)
- [アーモンド](https://booth.pm/ja/items/2012982)
- [メープル](https://booth.pm/ja/items/1948102)

他のモデルについては、各自 **ご自分のモデルへの適用時の `Transform` コンポーネントのスクリーンショットを [#MeshHoleShrinker](https://twitter.com/search?q=%23MeshHoleShrinker&src=typed_query) タグで Twitter に共有** することをおすすめします。

なお、私が所持しているモデルしか確認が取れないため、プリセット追加に関する Pull Request は原則として Reject させていただきます。

## 注意事項

- **Undo すると `Target` のメッシュが消える場合があります**。これは、複数回続けて `Process And Save As...` を行うと発生する現象で、直前に保存したメッシュを上書きすると上書き前のメッシュが復元できなくなるために起こります。通常は `Revert Target` を押すことで適用前の状態に戻すことができます。
- **`Target` にシアー（回転後に各軸で異なる値のスケール）がかかっている場合、頂点の範囲を正常に特定できません**。対象オブジェクトをヒエラルキーのルートに置いた状態で位置合わせをしてからお試しください。
- 既存のブレンドシェイプで位置が変わっている頂点が円筒に含まれている場合、位置合わせがうまく行かない場合があります。ただ、一般的なアバターモデルでは首まわりの頂点位置が変化するようなブレンドシェイプは存在しないため、通常は問題になることはありません。不安な方は位置合わせ時に `Target` のブレンドシェイプ量をあらかじめすべて `0` にしてください。
- 通常は楕円形に近い穴しかきれいに範囲指定できません（仕様です）。特殊な形状の穴を縮める際は、`Advanced > Use MeshCollider` にチェックを入れた上で、`MeshHoleShrinker` 内 `Cylinder` オブジェクトの `MeshCollider` および `MeshFilter` に指定されているメッシュを `Cube` 等の任意の形状に変更することで選択できる場合がありますが、素直に Blender で作業した方が確実かもしれません。
- 法線の向きには影響がありません。

## Tips

- `Scale` を `0` にすると穴を完全にふさぐことができます。
- 穴の中心点を微調整したい場合は `Offset` に移動量を指定してください。
- `Scale` を `1` にして `Offset` を指定すると、頂点が平行移動するシェイプキーを作成することができます。
- プレハブ内に含まれる `Cylinder` を複製して他の箇所に位置合わせすると、複数の頂点群を一度に処理することができます。

## ライセンス

[MIT License](./LICENSE)

## 更新履歴

- v1.2.0
  - 複数の円筒を一度に処理
  - 最後に保存したファイルパスを次回保存時のデフォルトパスとする
  - Cylinder 自体の Transform を変更すると正常に範囲を特定できない問題を修正
  - リファクタリング
- v1.1.0
  - UIの改善
    - Source Mesh を Target とは独立して保持し、リトライ時の手間を軽減
    - 保存ダイアログの初期ディレクトリを Source Mesh と同一に
    - Revert Target ボタンを追加
    - ツールチップ表示
  - 精度の向上
    - Skinned Mesh Renderer のスナップショット作成を BakeMesh よりも高精度な方法に変更
    - 頂点範囲の判定をデフォルトで数学的な円筒を用いるよう変更
  - 機能追加
    - Offset プロパティを追加
- v1.0.1
  - unitypackage に含まれるファイルのパスを修正
- v1.0.0
  - 初回リリース
