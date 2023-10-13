# Unity Physics 2D Plugin
 Unity Physics extension for adding pseudo 2D physics functionality

[![license](https://img.shields.io/badge/LICENSE-MIT-green.svg)](LICENSE)

[English README is here](README.md)

## 概要
Unity Physics 2D PluginはUnity Physicsを用いた擬似的な2Dの物理挙動を実装するライブラリです。

現在Unity Physicsは2Dをサポートしておらず、2Dのプロジェクトでこれを用いたい場合には3D向けのRigidbodyを使用する必要があります。

Unity Physics 2D PluginはZ軸を固定した3Dコライダを用いて2D向けの物理挙動を擬似的に再現します。`Rigidbody2D`および幾つかの2Dコライダのオーサリングをサポートし、SubScene内に配置された`Rigidbody2D`と`Collider2D`をUnity Physics用のコンポーネント群に変換します。

### 要件

* Unity 2022.3 以上
* Entities 1.0.0 以上
* Unity Physics 1.0.0以上

### インストール

1. Window > Package ManagerからPackage Managerを開く
2. 「+」ボタン > Add package from git URL
3. 以下のURLを入力する

```
https://github.com/AnnulusGames/UnityPhysics2DPlugin.git?path=Assets/UnityPhysics2DPlugin
```

あるいはPackages/manifest.jsonを開き、dependenciesブロックに以下を追記

```json
{
    "dependencies": {
        "com.annulusgames.unity-physics-2d-plugin": "https://github.com/AnnulusGames/UnityPhysics2DPlugin.git?path=Assets/UnityPhysics2DPlugin"
    }
}
```

## 基本的な使い方

Unity Physics 2D Pluginを導入することで、SubScene内の`Rigidbody2D`、およびサポートされている`Collider2D`が対応するコンポーネント群に変換されるようになります。

<img src="https://github.com/AnnulusGames/UnityPhysics2DPlugin/blob/main/Assets/UnityPhysics2DPlugin/Documentation~/img1.png" width="800">

friction(摩擦係数)やbounciness(反発係数)はRigidbody2Dやコライダに割り当てられている`PhysicsMaterial2D`アセットの値が反映されます。また、CollisionFilterに関してはPhysics2Dの`Layer Collision Matrix`の設定が適用されます。

## 利用可能なCollider

Unity Physics 2D Pluginは現在`BoxCollider2D`、`CircleCollider2D`、`CapsuleCollider2D`に対応しています。複雑な形状のコライダを作成したい場合は、これらを組み合わせた複合コライダを作成できます。

## Physics2DTag

2D用に作成されたEntityには`Physics2DTag`コンポーネントが付与されます。これによってクエリの際に2D用のPhysics Bodyと3D用のPhysics Bodyを区別することが可能です。

## メカニズム

Unity Physics 2D Pluginで作成されたPhysics Bodyは通常のものとは独立した`Physics2DSystemGroup`によって動作します。Entityの`PhysicsWorldIndex`の値は10に設定されているため、デフォルトのコライダとは干渉しません。

Physics2DSystemGroupでは通常のUnity PhysicsのSystem群に加え、前後に独自のSystemを追加します。これらのSystemは、物理挙動のシミュレーションが行われる間だけLocalTransformのZ軸の位置およびX/Y軸の回転を一時的に0に設定します。また、重心の位置やZ軸方向の速度、X/Y軸方向の回転速度は全て0に設定されます。

## 制限事項

* 2D用の衝突クエリ(Raycast、Overlap等)はサポートされていません (ただし実際のコライダは3Dで作成されているため、3D用の通常の衝突クエリを使用できます)
* 独自のSystemによる処理を追加するため、Unity Physics 2D Pluginのシミュレーションは決定論的でない可能性があります

## ライセンス

[MIT License](LICENSE)
