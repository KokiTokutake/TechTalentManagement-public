# SPEC-CAREER-002: Will-Can-Must深化（時間軸分割・Can強み/課題・やりがい）

| 項目 | 内容 |
|------|------|
| Spec ID | SPEC-CAREER-002 |
| バージョン | v1.0.0 |
| 作成日 | 2026-05-20 |
| ステータス | **approved** |
| オーナー | @KokiTokutake |

---

## 1. 背景・目的（Why）

### 1.1 解決する問題

現状のWill-Can-Must入力（SPEC-CAREER-001）は、時間軸の区別がなく「今のWill」と「将来のWill」が混在している。また、Canは「できること」の羅列に留まり、強み・弱みの構造化やモチベーションの把握ができていない。

### 1.2 ビジネス価値

- リクルート社の1on1設計を参考に、Will-Can-Mustを3つの時間軸（6ヶ月・1〜3年・3〜5年）に分割することで、短期目標と長期キャリアビジョンを区別して管理できる
- Canを「強み（活かしたいスキル）」「課題（伸ばしたいスキル）」の2軸で入力することで、育成計画と連動させやすくなる
- やりがい・価値観の項目を追加することで、モチベーション管理・動機付けに活用できる

### 1.3 対象ユーザー

| ユーザー | 権限 |
|---------|------|
| タレント（一般エンジニア） | 自分のWill-Can-Mustを入力・編集 |
| 上長（マネージャー） | 配下タレントのWill-Can-Mustを閲覧 |
| 人材開発委員（HR・委員長） | 全タレントのWill-Can-Mustを閲覧 |

---

## 2. 要件（What）

### REQ-01: Willの時間軸分割 【Must】

Willの入力を3つの時間軸で分けて入力できる：
- **短期Will（6ヶ月）**: 直近の考課期間でやりたいこと・挑戦したいこと
- **中期Will（1〜3年）**: 担いたい役割・ポジション・プロジェクト
- **長期Will（3〜5年）**: キャリアの到達イメージ・ありたい姿

各軸は任意入力とし、上位時間軸（長期）から入力するUIガイドを表示する。

### REQ-02: Canの強み・課題分割 【Must】

Can入力を2つのカテゴリに分けて構造化する：
- **強み（活かしたいCan）**: 自信を持って発揮できるスキル・経験
- **課題（伸ばしたいCan）**: 成長が必要・苦手意識のある領域

各カテゴリにテキスト形式（最大5件のリスト入力）とスキルマスタからの選択の両方で入力できる。

### REQ-03: やりがい・価値観の入力 【Should】

「仕事で大切にしていること」「モチベーションが上がる状況」「避けたい状況」を任意で入力できる「価値観・やりがい」セクションを追加する。

### REQ-04: Mustの上長コンテキスト表示 【Should】

Must欄に上長が入力した「組織として期待すること」を読み取り専用で表示し、タレント自身のMustと並べて確認できるようにする（上長コメントは SPEC-MDC-003 の `TalentManagerComment.expectedRole`（期待役割・ポジション）フィールドから自動取得）。

### REQ-05: 前期からの差分ハイライト 【Could】

前考課期間との差分（新しく追加された項目・削除された項目）をハイライト表示する。

### REQ-06: キャリア年表ビュー 【Could】

短期・中期・長期のWillを横並びのタイムライン形式で可視化する「キャリア年表」ビューを提供する。

---

## 3. スコープ外（作らないこと）

- AIによるWill-Can-Mustの自動提案・補完
- Will-Can-Mustと給与・昇格テーブルの直接連動
- タレント同士のWill-Can-Mustの比較・共有機能

---

## 4. 受入基準

### REQ-01: Willの時間軸分割

**AC-01-01: 3軸の入力と保存**
- Given: タレントがキャリアプラン入力画面を開いている
- When: 短期Will・中期Will・長期Willそれぞれにテキストを入力して「保存」をクリックする
- Then: 3つの時間軸のWillがそれぞれ考課期間と紐付いて保存される

**AC-01-02: 部分入力の許容**
- Given: キャリアプラン入力画面が表示されている
- When: 中期Willのみを入力して保存する（短期・長期は未入力）
- Then: 中期Willのみ保存される（バリデーションエラーにならない）

**AC-01-03: 上位軸からの入力ガイド**
- Given: すべての時間軸が未入力の状態
- When: キャリアプラン入力画面を開く
- Then: 「まず長期のWillから考えてみましょう」というガイドメッセージが短期Willの上部に表示される（長期Willが入力済みになると非表示になる）

### REQ-02: Canの強み・課題分割

**AC-02-01: 強みと課題の入力**
- Given: タレントがCan入力フォームを開いている
- When: 「強み」タブに3件、「課題」タブに2件のCan項目を入力して保存する
- Then: 強み3件・課題2件がそれぞれ別カテゴリとして保存され、プロファイルにも正しく表示される

**AC-02-02: スキルマスタからの選択**
- Given: Canの入力フォームが表示されている
- When: 「スキルから選択」ボタンをクリックしてスキルマスタから「Python」を選択する
- Then: 「Python」がCan項目に追加される（テキスト入力との併用可）

**AC-02-03: 上限制限**
- Given: 強みCan欄にすでに5件が入力されている
- When: 6件目を追加しようとする
- Then: 「強みは最大5件まで入力できます」とメッセージが表示され、追加できない

### REQ-03: やりがい・価値観

**AC-03-01: 価値観セクションの入力**
- Given: キャリアプラン画面の「価値観・やりがい」セクションが表示されている
- When: 各項目を入力して保存する
- Then: 入力内容が保存され、上長のプロファイル閲覧画面にも表示される

### REQ-03: やりがい・価値観

**AC-03-01: 価値観セクションの入力と保存**
- Given: キャリアプラン画面の「価値観・やりがい」セクションが表示されている
- When: 「仕事で大切にしていること」「やる気が上がる状況」「避けたい状況」を入力して保存する
- Then: 入力内容が保存され、上長のプロファイル閲覧画面（SPEC-MDC-003）の「価値観・やりがい」セクションにも表示される

### REQ-04: Mustの上長コンテキスト表示

**AC-04-01: 上長コメントの表示**
- Given: 上長がタレントの育成コメントに「期待役割」を入力済みである
- When: タレントがMust入力画面を開く
- Then: Must欄の上部に「上長からの期待（読み取り専用）」として育成コメントの期待役割が表示される

---

## 5. データモデル（概要）

```
# 既存の CareerPlan / Goal モデルを拡張
WillCanMustEntry {
  id: string (ULID)
  talentId: string → User.id
  periodId: string → EvaluationPeriod.id
  category: enum { Will, Can, Must }
  timeHorizon: enum { ShortTerm, MidTerm, LongTerm } | null  # Will のみ
  canType: enum { Strength, Challenge } | null               # Can のみ
  content: string (max: 2000)
  displayOrder: int
  linkedSkillId: string | null → SkillMaster.id             # Can のスキル連携
  createdAt: datetime
  updatedAt: datetime
}

CareerValues {
  id: string (ULID)
  talentId: string → User.id
  periodId: string → EvaluationPeriod.id
  importantThings: string | null (max: 1000)    # 仕事で大切にしていること
  motivationSituations: string | null (max: 1000)  # やる気が上がる状況
  avoidSituations: string | null (max: 1000)    # 避けたい状況
  createdAt: datetime
  updatedAt: datetime
}
```

---

## 6. APIエンドポイント（概要）

```
GET  /api/career/will-can-must?talentId=&periodId=   → WCM一覧取得（Talent/Manager/MDC）
PUT  /api/career/will-can-must                        → WCM一括保存（Talent）

GET  /api/career/values?talentId=&periodId=           → 価値観・やりがい取得
PUT  /api/career/values                               → 価値観・やりがい保存（Talent）

GET  /api/career/timeline?talentId=                   → キャリア年表用データ（Talent/Manager/MDC）
```

---

## 7. 非機能要件

| 項目 | 要件 |
|------|------|
| 後方互換 | 既存の SPEC-CAREER-001 形式のデータを SPEC-CAREER-002 のモデルに移行する際、以下のマッピングで変換する |
| パフォーマンス | WCM一覧取得を1秒以内に返す |
| セキュリティ | タレント自身のみが自分のWCMを編集可能。上長・管理者は閲覧のみ |

**SPEC-CAREER-001 → SPEC-CAREER-002 移行マッピング:**

| 旧フィールド（CareerPlan） | 新エンティティ（WillCanMustEntry） |
|--------------------------|----------------------------------|
| LongTermGoal | category=Will, timeHorizon=LongTerm |
| MediumTermGoal | category=Will, timeHorizon=MidTerm |
| CurrentFocus | category=Will, timeHorizon=ShortTerm |
| CanStrengths（テキスト） | category=Can, canType=Strength |
| CanDevelopment（テキスト） | category=Can, canType=Challenge |

移行スクリプト実行前に管理者が一括プレビューを確認し、承認してから実行する。移行後は CareerPlan の旧フィールドを読み取り専用でアーカイブとして保持する（削除しない）。

---

## 8. 関連仕様

- SPEC-CAREER-001: キャリアプラン管理（基盤機能）
- SPEC-1ON1-001: 1on1面談記録（Will面談との連動・育成コメント参照）
- SPEC-MDC-003: タレントプロファイル（WCMサマリーの表示）
- SPEC-SKILL-001: スキル管理（Can入力でのスキルマスタ連携）

---

## 変更履歴

| バージョン | 日付 | 変更内容 | 変更者 |
|-----------|------|---------|--------|
| v1.0.0 | 2026-05-20 | 初版作成 | @KokiTokutake |
