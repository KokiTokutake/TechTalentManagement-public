# TechTalentManagement

IT エンジニア組織向けのタレントマネジメントシステム（考課、9ボックス、後継者計画、スキル管理、1on1、キャリア計画）の設計・実装の習作です。
**仕様駆動開発**（仕様書 → 設計書 → 実装 → E2E テスト）を、AI（Claude Code）と進める実践として作りました。約12日間（2026-05-17〜05-28）の開発で、実運用はしていません。

> 画面に表示される氏名・メールアドレスはすべて架空のシードデータです。実在の組織の情報は含みません。

## 何ができるか

| 領域 | 内容 |
|---|---|
| 考課 | 目標とKPI、進捗チェックイン、自己評価、上長評価、フィードバック面談 |
| 人材開発委員会 | 9ボックス評価、重点管理人物、後継者計画、委員会の決定事項とアクション |
| スキル | スキルマスタ（レベル定義つき）、スキルマップ、ギャップ分析、アサイン |
| キャリア | Will-Can-Must、キャリアプラン、コメント |
| 1on1 | テンプレート、前回面談の引き継ぎ、アクションアイテム |
| 通知 | 通知センター |

## 画面

シードデータ（架空）で起動した状態の画面です。ユーザーのロールごとに、見られる画面が変わります。

| 画面 | ロール | 画像 |
|---|---|---|
| ログイン（ユーザーと考課期間の選択） | — | [01-login.png](docs/screenshots/01-login.png) |
| 9ボックス評価マップ | 管理職 | [02-nine-box-map.png](docs/screenshots/02-nine-box-map.png) |
| 9ボックス キャリブレーション（チーム別の評価分布） | 管理職 | [03-nine-box-calibration.png](docs/screenshots/03-nine-box-calibration.png) |
| 重点管理人物 | 幹部 | [04-key-persons.png](docs/screenshots/04-key-persons.png) |
| 委員会サマリー（Will・Can・Must の俯瞰） | 幹部 | [05-committee-summary.png](docs/screenshots/05-committee-summary.png) |
| 組織スキルマップ | 管理職 | [06-skill-map.png](docs/screenshots/06-skill-map.png) |
| コンピテンシー評価 | タレント | [07-competency.png](docs/screenshots/07-competency.png) |
| 目標設定（Will・Can・Must） | タレント | [08-goals.png](docs/screenshots/08-goals.png) |
| キャリアプラン | タレント | [09-will-can-must.png](docs/screenshots/09-will-can-must.png) |

## 構成

ASP.NET Core 8 の Web API（Api / Application / Domain / Infrastructure の4層）と、Nuxt 3 のフロントエンドで構成します。DB は SQL Server 2022 です。
全体像は [アーキテクチャ設計書](docs/design/architecture.md)、データの関連は [ER 図](docs/design/er-diagram.md) を参照してください。

## 読む順番（おすすめ）

1. [architecture.md](docs/design/architecture.md) — 全体構成、認証の制約、品質保証
2. [er-diagram.md](docs/design/er-diagram.md) — 37エンティティの関連
3. [SPEC-MDC-002-design.md](docs/design/SPEC-MDC-002-design.md) — 9ボックス評価の設計書（エンティティ、制約、API、認可、エラー）
4. [tests/e2e/](tests/e2e/) — 仕様書 ID と対応する E2E テスト（30ファイル）

## 起動

Docker が必要です。

```bash
cp .env.example .env     # DB_SA_PASSWORD を設定する
docker compose up -d --build
```

- フロントエンド: http://localhost:3000
- API: http://localhost:5108
- 起動時にマイグレーションの適用とシードデータの投入が自動で行われます。
- ログインは、画面でユーザーを選ぶ方式です（下の「制約」を参照）。

## テスト

```bash
cd backend && dotnet test         # バックエンド（xUnit）
cd frontend && npm run test       # フロントエンド（Vitest、Pinia ストア）
npm run test:e2e                  # E2E（Playwright。アプリを起動してから実行）
```

CI（GitHub Actions）では、仕様書のステータスと書式の確認、.NET のビルドとテスト、Nuxt の型チェックとビルドを行います。E2E は CI に組み込んでいません。

## 開発の進め方

仕様書（`docs/spec/`）を先に書き、設計書（`docs/design/`）を承認してから実装します。実装は、タスクごとにブランチを切り、E2E テストを添えた PR で行いました。AI の出力は、人間がレビューして承認する前提です。

## 制約

- **認証は未実装です。** 画面で選んだユーザーの ID を `X-User-Id` ヘッダーで渡すデモ用の仕組みです。ロールに応じた認可（配下のタレントだけ評価できる、など）は実装しています。
- ロール制限のある画面（重点管理人物、委員会サマリー）は、URL を直接開くとホームに戻されます。サーバー側の描画時にはログイン状態を読めず、ロール判定が先に走るためです。メニューから開くと表示されます。
- Azure への配備、Entra ID による認証、監視は構想のみです。
- フロントエンドの単体テストは、現状は Pinia ストアに限られます。
- 大量データでの性能検証は行っていません。

## ライセンス

[MIT License](LICENSE) です。
