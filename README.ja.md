<!-- Generated from docs/content/locales.json by build/Build-Documentation.mjs. Do not edit generated localized READMEs by hand. -->
<p align="center"><img src="https://raw.githubusercontent.com/MukaSanches/PermissionScope/main/docs/brand/repository-banner.png" alt="PermissionScope — See who has access. Understand why." width="100%"></p>

<p align="center"><img src="https://raw.githubusercontent.com/MukaSanches/PermissionScope/main/docs/brand/project-avatar.png" alt="PermissionScope mark" width="72" height="72"></p>

<p align="center">
  <a href="https://github.com/MukaSanches/PermissionScope/actions/workflows/build.yml"><img alt="Windows build" src="https://github.com/MukaSanches/PermissionScope/actions/workflows/build.yml/badge.svg"></a>
  <a href="https://github.com/MukaSanches/PermissionScope/actions/workflows/codeql.yml"><img alt="CodeQL" src="https://github.com/MukaSanches/PermissionScope/actions/workflows/codeql.yml/badge.svg"></a>
  <a href="https://github.com/MukaSanches/PermissionScope/blob/main/LICENSE"><img alt="Apache-2.0" src="https://img.shields.io/badge/license-Apache--2.0-2764E7"></a>
  <a href="https://github.com/MukaSanches/PermissionScope/releases"><img alt="Release" src="https://img.shields.io/github/v/release/MukaSanches/PermissionScope?display_name=tag&color=2764E7"></a>
</p>

<p align="center"><strong>Windows 権限を、検証できる形に。</strong><br>フォルダーにアクセスできる人と、その結果を裏付ける規則を確認できます。</p>
<table><tr><td align="center"><a href="https://mukasanches.github.io/PermissionScope/index.ja.html"><strong>Website</strong></a></td><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/releases/latest"><strong>リリースのダウンロード</strong></a></td><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/docs/TRUST-CENTER.md"><strong>Trust Center</strong></a></td></tr><tr><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/docs/courses/README.md">Academy</a></td><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/ROADMAP.md">Roadmap</a></td><td align="center"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/SECURITY.md">Security</a></td></tr></table>

<p align="center"><sub>言語</sub></p>
<table><tr><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.md"><strong>English</strong></a><br><sub>en-US</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.pt-BR.md"><strong>Português</strong></a><br><sub>pt-BR</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.es.md"><strong>Español</strong></a><br><sub>es</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.fr.md"><strong>Français</strong></a><br><sub>fr</sub></td></tr><tr><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.de.md"><strong>Deutsch</strong></a><br><sub>de</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.ar.md"><strong>العربية</strong></a><br><sub>ar</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.ja.md"><strong>日本語</strong></a><br><sub>ja</sub></td><td align="center" width="25%"><a href="https://github.com/MukaSanches/PermissionScope/blob/main/README.zh-Hans.md"><strong>简体中文</strong></a><br><sub>zh-Hans</sub></td></tr></table>

---

<table><tr><td width="33%"><strong>ローカル優先</strong><br><sub>PermissionScope アカウント、アプリのテレメトリ、必須クラウドサービスはありません。</sub></td><td width="33%"><strong>Windows ネイティブの判定</strong><br><sub>有効アクセスは手書きの近似ではなく Windows Authz で評価します。</sub></td><td width="33%"><strong>Unknown は Unknown のまま</strong><br><sub>不足している文脈を勝手に Granted や Denied へ変換しません。</sub></td></tr></table>

<p align="center"><img src="https://raw.githubusercontent.com/MukaSanches/PermissionScope/main/docs/screenshots/ja/access-light.png" alt="LAB\Alex は LAB\Finance を通じて変更権限を得ます。Authz で評価した合成シナリオのネイティブ画面です。実際の企業データや加工した結果ではありません。" width="94%"></p>

## 2分で始める

1. Releases から完全なインストーラーまたはポータブル ZIP をダウンロードします。Intel/AMD は x64、ARM PC は ARM64 を選びます。
2. 自分のアカウントにインストールするか、ZIP 全体を展開して PermissionScope.exe を開きます。
3. LAB\Alex のデモを試します。自分のファイルでは分析を選び、フォルダーを指定します。
4. ID を空欄にすると現在の Windows トークンを使います。Access Path で根拠を確認します。

## 結果から証拠へ

Windows Authz がマスクを計算します。Access Path は寄与するエントリと記録された所属を示します。継承フラグは元の祖先を証明しません。ACL のフルコントロールもファイルを開ける保証にはなりません。

```text
Windows identity
      ↓
SID + recorded membership context
      ↓
ACL / discretionary permission entries
      ↓
Windows Authz evaluation
      ↓
Granted · Partial · Denied · Unknown
      ↓
Access Path → contributing evidence
```

## 結果を読む

許可は評価した任意アクセス制御の規則が操作を許すことです。一部は特定の操作のみ許可、拒否はアクセスの付与なしです。不明は判断に必要な情報が不足している状態で、許可として扱いません。

## 説明を検証する

Windows Authz がマスクを計算します。Access Path は寄与するエントリと記録された所属を示します。継承フラグは元の祖先を証明しません。ACL のフルコントロールもファイルを開ける保証にはなりません。

<p align="center"><img src="https://raw.githubusercontent.com/MukaSanches/PermissionScope/main/docs/screenshots/ja/access-path-light.png" alt="Access Path" width="94%"></p>

## 製品機能

<table><tr><td><strong>分析</strong><br><sub>フォルダーと ID の有効アクセスを確認します。</sub></td><td><strong>説明</strong><br><sub>Access Path と寄与した証拠を追跡します。</sub></td><td><strong>スナップショット</strong><br><sub>ローカル観測を保存し、後で確認します。</sub></td></tr><tr><td><strong>比較</strong><br><sub>見つからないリソースを削除済みと決めつけずに比較します。</sub></td><td><strong>シミュレーション</strong><br><sub>実変更の前にルール削除をメモリ上でモデル化します。</sub></td><td><strong>エクスポート</strong><br><sub>HTML、CSV、JSON、XLSX、PDF 出力。</sub></td></tr></table>

## 根拠を保存する

ローカルのスナップショットを保存・比較し、メモリ内で規則の削除をシミュレーションして HTML、CSV、JSON、XLSX、PDF に出力できます。後の観測で見つからないリソースを削除済みと断定しません。

```powershell
./permissionscope-cli.exe demo --output ./demo-reports
./permissionscope-cli.exe explain "C:\Finance"
./permissionscope-cli.exe scan "C:\Finance" --save
./permissionscope-cli.exe export <snapshot-id> --format html --output report.html
```

## 範囲とプライバシー

リモートログオン、S4U、条件付き規則、未確認のリンク先は不明のままです。整合性ポリシー、暗号化、ロック、特権は判定範囲外です。テレメトリ、アカウント、クラウドは不要です。実データのレポートには機密のパスや名前が含まれ得ます。

> 分析とシミュレーションは読み取り専用です。適用は明示的な確認を伴う別の手順で、通常のローカルファイルのみが対象です。保存、永続ログ、検証、ロールバックを行います。フォルダー、リンク、複数のハードリンクを持つファイルは対象外です。

## ダウンロードして検証

Windows 10 1809 以降。完全なパッケージにはランタイムが含まれます。現在のインストーラーは未署名のため SHA-256 を確認してください。ARM64 は CI でクロスビルドしており実機認証はありません。Store と WinGet はリリース状況をご確認ください。

[リリースのダウンロード](https://github.com/MukaSanches/PermissionScope/releases/latest) · [SHA-256](https://github.com/MukaSanches/PermissionScope/releases/latest) · [リリース状況](https://github.com/MukaSanches/PermissionScope/blob/main/docs/release-status.md)

## 検証可能性を重視

| 検証可能性を重視 | |
|---|---|
| ソース | 公開リポジトリとコミット履歴 |
| リリース | バージョン付き成果物と SHA-256 |
| サプライチェーン | CycloneDX SBOM と文書化された固定済み自動化 |
| セキュリティ | セキュリティモデル、開示ガイダンス、CodeQL |
| プラットフォーム | x64 と ARM64 のビルド経路 |
| ドキュメント | 8 言語のハンドブック、ビジュアルガイド、Academy コース |
| プライバシー | ローカル優先設計とエクスポートの機微情報に関する明示的な説明 |

[Trust Center を開く](https://github.com/MukaSanches/PermissionScope/blob/main/docs/TRUST-CENTER.md)

## ボタンではなくモデルを学ぶ

- [権限の基礎](https://github.com/MukaSanches/PermissionScope/blob/main/docs/courses/fundamentals.md)
- [Access Path の読み方](https://github.com/MukaSanches/PermissionScope/blob/main/docs/courses/access-path.md)
- [安全な診断](https://github.com/MukaSanches/PermissionScope/blob/main/docs/courses/troubleshooting.md)
- [Handbooks PDF · 8 languages](https://github.com/MukaSanches/PermissionScope/tree/main/docs/handbooks)

## 次の手順

- [画面付きガイド](https://github.com/MukaSanches/PermissionScope/blob/main/docs/guides/ja.md)
- [技術モデル](https://github.com/MukaSanches/PermissionScope/blob/main/docs/access-model.md)
- [質問とトラブル対処](https://github.com/MukaSanches/PermissionScope/blob/main/docs/faq.md)
- [やさしい用語集](https://github.com/MukaSanches/PermissionScope/blob/main/docs/glossary.md)
- [プライバシー](https://github.com/MukaSanches/PermissionScope/blob/main/docs/privacy.md)
- [リリース状況](https://github.com/MukaSanches/PermissionScope/blob/main/docs/release-status.md)
- [Governance](https://github.com/MukaSanches/PermissionScope/blob/main/GOVERNANCE.md)
- [Support](https://github.com/MukaSanches/PermissionScope/blob/main/SUPPORT.md)
- [Citation](https://github.com/MukaSanches/PermissionScope/blob/main/CITATION.cff)

## エンジニアリング上の境界

PermissionScope は、随意 ACL の評価だけですべてのファイルオープン結果を説明できるとは主張しません。整合性ポリシー、暗号化、ロック、一部の Remote/S4U コンテキスト、条件付きルール、管理者権限の影響、未検証の reparse ターゲットは利用可能な文脈の外にある場合があります。これらの境界は隠さず文書化します。

## ビルドとテスト

Windows と .NET 10 SDK を使用します。テストは統合テスト用の実行プログラムで、dotnet test ではありません。パッケージ化と文書検証は開発ガイドを参照してください。

[Development](https://github.com/MukaSanches/PermissionScope/blob/main/docs/development.md) · [Benchmarks](https://github.com/MukaSanches/PermissionScope/blob/main/docs/benchmarks.md) · [Roadmap](https://github.com/MukaSanches/PermissionScope/blob/main/ROADMAP.md)

## ヘルプと貢献

バージョン、Windows、操作、エラーコードを記載してください。技術タブではパス、アカウント、SID を除いた診断をコピーできます。翻訳はプレビューで、技術的根拠は英語の場合があります。母語話者によるレビューや支援技術の認証は主張しません。

---

<p align="center"><sub>作成者 Samuel Sanches · ssanches011@gmail.com · <a href="https://github.com/MukaSanches/PermissionScope/blob/main/LICENSE">Apache-2.0</a> · <a href="https://github.com/MukaSanches/PermissionScope/blob/main/SECURITY.md">Security</a> · <a href="https://github.com/MukaSanches/PermissionScope/blob/main/SUPPORT.md">Support</a></sub></p>
