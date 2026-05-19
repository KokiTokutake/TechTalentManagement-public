# SPEC-MDC-001: 人材開発委員会機能

| 項目 | 値 |
|------|----|
| Spec ID | SPEC-MDC-001 |
| バージョン | v1.0.0 draft |
| 作成日 | 2026-05-18 |
| ステータス | approved |
| オーナー | @KokiTokutake |

---

## 1. 背景・目的（Why）

### 1.1 解決する問題

現状、金融系グループ会社の人材開発委員会は Excel・紙ベースで運営されており、以下の課題がある：

- 委員会の準備（対象人物の情報収集・資料作成）に多大な工数がかかる
- 決定した育成方針がメールや口頭で共有され、記録・追跡が困難
- 重点管理人物の指定状況が組織内で一元的に把握できていない
- 前回委員会の決議事項をフォローアップする仕組みがない

### 1.2 ビジネス価値

- **Constitution.md の成功定義に直結**: 「人材開発委員会など会議体の準備・記録コストが50%以上削減される」「重点管理人物の設定と育成計画が組織として管理できる」
- 委員会準備をシステムが自動サポート（Will-Can-Must データを集約提供）
- 育成方針をタレント本人にも開示し、透明性による動機付けを実現
- 委員会の決議・育成方針を一元管理し、継続的な成長支援を実現

### 1.3 対象ユーザー

| ロール | 役割 |
|--------|------|
| Senior / Executive / Admin | 重点管理人物の指定・解除、委員会セッション管理、全体閲覧 |
| Manager | 担当タレントの育成方針入力（直属のみ） |
| Talent | 自分の重点管理指定状況・育成方針の閲覧 |

---

## 2. 要件（What）

### REQ-01: 重点管理人物の指定・管理 【Must】

Senior / Executive / Admin が任意のタレントを「重点管理人物」に指定・解除できる。

- 重点管理人物の一覧を閲覧できる（Senior / Executive / Admin、担当 Manager）
- タレント本人も自分が重点管理人物に指定されていることを確認できる
- 指定理由（任意テキスト）を記録できる

### REQ-02: 育成方針・アクションプランの記録 【Must】

Manager（直属上長）が担当タレントの育成方針を入力・更新できる。

- 育成方針の構成: 課題・目標・支援アクション・期限（任意）
- タレント本人も育成方針を閲覧できる
- 育成方針は更新履歴を保持する（上書きではなくバージョン管理）
- Senior / Executive / Admin も閲覧できる（書き込みは担当 Manager のみ）

### REQ-03: 委員会セッション管理 【Must】

Senior / Executive / Admin が委員会セッションを作成・記録できる。

- セッションの構成: 開催日、参加者（ユーザー選択・全ロールを対象、制限なし）、議事録（テキスト）、対象重点管理人物
- セッション一覧・詳細を閲覧できる（Senior / Executive / Admin）
- Manager は自分が関係する（担当部下が対象の）セッションを閲覧できる

### REQ-04: 委員会向けサマリービュー 【Must】

委員会準備のため、重点管理人物の一覧に各人の Will-Can-Must 概要を集約表示する。

- SPEC-CAREER-001 の統合ビューデータを流用
- 1ページで全重点管理人物の状況を俯瞰できる

### REQ-05: タレント向けマイページ表示 【Should】

重点管理人物に指定されているタレントが、自分のキャリアプランページで指定状況と育成方針を確認できる。

> 受入基準は AC-01-05 および AC-02-04 にて確認（REQ-01・REQ-02 の基準が本要件を包含する）。

---

## 3. スコープ外（作らないこと）

- メール・Slack 通知（v2 以降）
- 委員会の定期スケジュール管理・カレンダー連携
- 育成方針と評価点数の自動連動
- 複数期間の重点管理人物の比較分析

---

## 4. 受入基準

### REQ-01: 重点管理人物の指定・管理

**AC-01-01: Senior が重点管理人物を指定できる**
- Given: Senior ロールでログイン済みである
- When: タレントを重点管理人物として指定する（理由を入力して登録）
- Then: そのタレントが重点管理人物として登録され、一覧に表示される

**AC-01-02: 重複指定はできない**
- Given: すでに重点管理人物に指定済みのタレントが存在する
- When: 同じタレントを再度指定しようとする
- Then: 409 Conflict が返り、重複登録されない

**AC-01-03: Senior が重点管理人物を解除できる**
- Given: Senior ロールでログイン済みで、重点管理人物が1名以上存在する
- When: 重点管理人物の指定を解除する
- Then: そのタレントが一覧から削除され、Talent 本人のページにも表示されなくなる

**AC-01-04: Talent / Manager は重点管理人物を指定・解除できない**
- Given: Talent または Manager ロールでログイン済みである
- When: 重点管理人物指定 API（POST /api/key-persons）を実行する
- Then: 403 が返る

**AC-01-05: 対象タレント本人が自分の指定状況を確認できる**
- Given: タレントが重点管理人物に指定されている
- When: 自分のキャリアプランページを開く
- Then: 「重点管理人物」バッジと指定理由が表示される

### REQ-02: 育成方針・アクションプランの記録

**AC-02-01: 直属上長が担当タレントの育成方針を入力できる**
- Given: Manager ロールでログイン済みで、担当する重点管理人物が存在する
- When: 育成方針（課題・目標・支援アクション）を入力して保存する
- Then: 育成方針が保存され、タレント本人と Senior 以上が閲覧できる

**AC-02-02: 直属上長以外は育成方針を書き込めない**
- Given: Senior ロールのユーザーが対象タレントの育成方針画面を表示している
- When: POST /api/key-persons/{id}/development-plans を実行する
- Then: 403 が返る（閲覧は可能）

**AC-02-03: 育成方針の更新履歴が保持される**
- Given: 育成方針がすでに保存されている
- When: 内容を変更して再保存する
- Then: 新しいバージョンが追加され、過去のバージョンも参照できる

**AC-02-04: タレント本人が育成方針を閲覧できる**
- Given: 直属上長が育成方針を登録済みである
- When: タレントが自分のキャリアプランページを開く
- Then: 育成方針（課題・目標・支援アクション）が表示される

**AC-02-05: 空の育成方針は保存できない**
- Given: 課題・目標・支援アクションがすべて空の状態である
- When: 保存ボタンをクリックする
- Then: 400 が返り、保存されない

**AC-02-06: 解除済みの重点管理人物には育成方針を追加できない**
- Given: IsActive=false（指定解除済み）の KeyPerson が存在する
- When: POST /api/key-persons/{id}/development-plans を実行する
- Then: 409 Conflict が返り、登録されない

### REQ-03: 委員会セッション管理

**AC-03-01: Senior が委員会セッションを作成できる**
- Given: Senior ロールでログイン済みである
- When: 開催日・参加者・議事録・対象重点管理人物を入力してセッションを作成する
- Then: セッションが保存され、一覧に表示される

**AC-03-02: Talent / Manager は委員会セッションを作成できない**
- Given: Talent または Manager ロールでログイン済みである
- When: POST /api/committee-sessions を実行する
- Then: 403 が返る

**AC-03-03: Manager は担当部下が対象のセッションを閲覧できる**
- Given: Manager ロールでログイン済みで、担当タレントが対象となるセッションが存在する
- When: セッション一覧を取得する
- Then: 担当タレントが対象に含まれるセッションが返る（関係ないセッションは返らない）

**AC-03-04: 開催日が未来のセッションも作成できる（事前登録）**
- Given: Senior ロールでログイン済みである
- When: 未来の日付でセッションを作成する
- Then: 正常に保存される

### REQ-04: 委員会向けサマリービュー

**AC-04-01: Senior が重点管理人物のサマリーを一覧で閲覧できる**
- Given: Senior ロールでログイン済みで、重点管理人物が複数名存在する
- When: 委員会サマリーページを開く
- Then: 全重点管理人物の氏名・中長期目標（Will）・注力テーマ・スキル数・育成方針の有無が一覧表示される

**AC-04-02: Talent / Manager は委員会サマリーにアクセスできない**
- Given: Talent または Manager ロールでログイン済みである
- When: GET /api/key-persons/committee-summary を実行する
- Then: 403 が返る

---

## 5. データモデル（概要）

```
KeyPerson（重点管理人物）
  Id: Guid
  TalentId: Guid → User
  DesignatedBy: Guid → User（指定者）
  Reason: string?（指定理由）
  DesignatedAt: DateTimeOffset
  IsActive: bool（解除済みは false）

DevelopmentPlan（育成方針）
  Id: Guid
  KeyPersonId: Guid → KeyPerson
  AuthorId: Guid → User（担当 Manager）
  Challenge: string（課題）
  Goal: string（目標）
  SupportAction: string（支援アクション）
  DueDate: DateOnly?（期限）
  Version: int（履歴管理用）
  CreatedAt: DateTimeOffset

CommitteeSession（委員会セッション）
  Id: Guid
  HeldOn: DateOnly（開催日）
  Minutes: string（議事録）
  CreatedBy: Guid → User
  CreatedAt: DateTimeOffset
  Participants: ICollection<CommitteeSessionParticipant>
  KeyPersons: ICollection<CommitteeSessionKeyPerson>

CommitteeSessionParticipant
  SessionId: Guid
  UserId: Guid

CommitteeSessionKeyPerson
  SessionId: Guid
  KeyPersonId: Guid
```

---

## 6. API エンドポイント（概要）

```
# 重点管理人物
GET    /api/key-persons                   → 一覧（Senior/Exec/Admin）※ IsActive=true のみ返却
POST   /api/key-persons                   → 指定（Senior/Exec/Admin）
DELETE /api/key-persons/{id}              → 解除（Senior/Exec/Admin）
GET    /api/key-persons/committee-summary → 委員会サマリー（Senior/Exec/Admin）
GET    /api/key-persons/my               → 自分の指定状況（Talent本人）※ IsActive=true のみ返却

# 育成方針
GET    /api/key-persons/{id}/development-plans        → 一覧・履歴
POST   /api/key-persons/{id}/development-plans        → 新規（直属Manager）

# 委員会セッション
GET    /api/committee-sessions            → 一覧（権限フィルタ済み）
POST   /api/committee-sessions            → 作成（Senior/Exec/Admin）
GET    /api/committee-sessions/{id}       → 詳細
```

---

## 7. 非機能要件

- 重点管理人物リストの取得は 200ms 以内（件数 < 100 件を想定）
- 育成方針は過去バージョンを含め全件保持（削除不可）
- 委員会セッションの議事録は最大 10,000 文字

---

## 8. 関連仕様

| Spec ID | 関連内容 |
|---------|---------|
| SPEC-CAREER-001 | 統合ビュー（Will-Can-Must）データを委員会サマリーに活用 |
| SPEC-EVAL-001 | 評価ワークフローとの将来的な連携（v2 以降） |

---

## 変更履歴

| バージョン | 日付 | 変更内容 |
|-----------|------|---------|
| v1.0.0 | 2026-05-18 | 初版作成 |
| v1.0.1 | 2026-05-18 | /spec-review による改善（REQ-05 AC 参照追加・AC-02-06 追加・API 返却条件明記・参加者ロール制約明記） |
