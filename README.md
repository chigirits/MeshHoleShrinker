# MeshHoleShrinker

メッシュに開いた穴を縮めるためのシェイプキーを追加し、新しいメッシュとして保存するUnityエディタ拡張です。

Unity上でアバター頭部を他の素体に接合するとき、接合部にすきまが開いてしまうのを防ぐなどの用途に有用です。

## 動作環境

Unity 2018.4 以降

## インポート手順

### unitypackageをインポートする方法

[Releasesページ](https://github.com/chigirits/MeshHoleShrinker/releases) より最新版の `MeshHoleShrinker-vX.X.X.unitypackage` をダウンロードし、Unityにインポートする

### パッケージマネージャを用いる方法

1. インポート先プロジェクトの `Packages/manifest.json` をテキストエディタ等で開く
2. `dependencies` の配列内に以下の行を追加
   
   ```
   "com.github.chigirits.MeshHoleShrinker": "https://github.com/chigirits/MeshHoleShrinker.git",
   ```

こちらの方法でインポートした場合、以下の説明文中で示される本パッケージのプレハブやプリセットは `Assets/Chigiri/MeshHoleShrinker/...` 下ではなく `Packages/MeshHoleShrinker/Assets/Chigiri/MeshHoleShrinker/...` 下から選択してください。

## 使い方

1. シーンにアバターを配置し、頭部以外を非表示にする
2. プロジェクトタブから `Assets/Chigiri/MeshHoleShrinker/Prefabs/MeshHoleShrinker` を探し、同シーンに配置（後述の一部販売モデルはプリセットを利用できます）
3. `MeshHoleShrinker` の `Target` に、アバター頭部の `SkinnedMeshRenderer`（一般的には `Body` オブジェクト）を指定
4. `New Shape Key Name` に追加するシェイプキーの名前を指定
   
   ![usage-01](https://user-images.githubusercontent.com/61717977/93084783-88ae2a00-f6cf-11ea-8c92-a8433986f283.png)
5. `MeshHoleShrinker` の位置・角度・スケールを調整し、縮めたい穴を構成する頂点が半透明の円筒にすべて含まれるようにする（Shaded Wireframe 表示での作業を推奨）
   
   ![usage-02](https://user-images.githubusercontent.com/61717977/93084788-89df5700-f6cf-11ea-8d8b-5166c0bbc20a.png)<br>
   穴の辺ループ上にない頂点や舌などを巻き込まないよう注意し、必要十分な頂点が含まれているかを全方位から眺めて確認<br>
   ![usage-03](https://user-images.githubusercontent.com/61717977/93084790-8a77ed80-f6cf-11ea-9f0a-491ec3954c05.png)
6. `Process And Save As...` を押し、作成されたメッシュの保存先ファイル名を指定
   
   ![usage-04](https://user-images.githubusercontent.com/61717977/93084794-8ba91a80-f6cf-11ea-8aa9-a4c7144152a4.png)
7. 頭部メッシュが差し替えられて新しいシェイプキーがウェイト100で追加されていることを確認
   
   ![usage-05](https://user-images.githubusercontent.com/61717977/93084798-8c41b100-f6cf-11ea-88b7-012b4444ccda.png)

### 処理後の調整

- 穴が均一に縮んでいない場合は、Undo して 5. からやり直してください。余計な頂点が範囲に含まれていると捻じれ等が起こりやすいです。
- 穴の周辺ポリゴンが重なり合うなど表示上の不都合が発生している場合は、シェイプキーのウェイトを小さめに調整してください。必要最低限の値に設定することを推奨します。

### プリセットについて

以下の販売モデルは、あらかじめ位置合わせ済みのプリセットを `Assets/Chigiri/MeshHoleShrinker/Prefabs/Presets/` 下に用意しています。モデルのプレハブを未加工のまま原点に配置した状態で適用してください。

- [みみの](https://booth.pm/ja/items/1336133)
- [アーモンド](https://booth.pm/ja/items/2012982)
- [メープル](https://booth.pm/ja/items/1948102)

他のモデルについては、各自 **ご自分のモデルへの適用時の `Transform` コンポーネントのスクリーンショットを [#MeshHoleShrinker](https://twitter.com/search?q=%23MeshHoleShrinker&src=typed_query) タグで Twitter に共有** することをおすすめします。

なお、私が所持しているモデルしか確認が取れないため、プリセット追加に関する Pull Request は原則として Reject させていただきます。

## 備考

- 通常は楕円形に近い穴しかきれいに範囲指定できません（仕様です）。特殊な形状の穴を縮める際は、`MeshHoleShrinker` 内 `Cylinder` オブジェクトの `MeshCollider` に指定するメッシュを `Cube` 等の任意の形状に変更することで選択できる場合がありますが、素直に Blender で作業した方が確実かもしれません。

## ライセンス

[MIT License](./LICENSE)
