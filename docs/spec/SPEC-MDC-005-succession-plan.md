# SPEC-MDC-005: 後継者計画（サクセッションプラン）

| 項目 | 内容 |
|------|------|
| Spec ID | SPEC-MDC-005 |
| バージョン | v1.0.0 |
| 作成日 | 2026-05-20 |
| ステータス | **approved** |
| オーナー | @KokiTokutake |

---

## 1. 背景・目的（Why）

### 1.1 解決する問題

一般に、重要ポジション（技術リード・プロジェクトマネージャー等）の後継者計画はHRの頭の中や非公式な議論に留まりがちで、組織として可視化・管理されないことが多い。担当者の突然の離職・異動時に後継者が不在となるリスクがある。

### 1.2 ビジネス価値

- GEのセッションC・リクルートのMDCで実施されているサクセッションプランのプラクティスを参考に、重要ポジションごとに後継者候補とReadiness（準備度）を管理する
- 9ボックス（SPEC-MDC-002）のHigh Potential人材と連動し、後継者候補のロングリストを効率的に作成できる
- 後継者の育成ギャップ（現状のスキルとポジション要件のギャップ）を可視化し、育成計画に反映できる

### 1.3 対象ユーザー

| ユーザー | 権限 |
|---------|------|
| 管理者（人事評価チーム）/ Senior / Executive | サクセッションプランの作成・編集・閲覧 |
| 管理職 | 自部門の後継者候補の推薦・育成ギャップの入力 |

---

## 2. 要件（What）

### REQ-01: 重要ポジションマスタの管理 【Must】

組織内の重要ポジション（引き継ぎリスクが高い役割）を登録・管理できる。
- ポジション名（例: バックエンドテックリード）
- 現担当者
- ポジションの重要度（Critical / High / Medium）
- 要求スキル・コンピテンシー（任意）

### REQ-02: 後継者候補の登録とReadiness評価 【Must】

各重要ポジションに対して、後継者候補を複数名登録し、Readiness（準備度）を評価できる：
- **Ready Now**: 今すぐ引き継ぎ可能（6ヶ月以内）
- **Ready Soon**: 1〜2年で準備完了見込み
- **Ready Mid Term**: 3〜5年で準備完了見込み

候補者ごとに、育成ギャップ（不足スキル・経験）と育成アクションを記録できる。

### REQ-03: 9ボックスとの連動による候補者推薦 【Should】

9ボックス評価（SPEC-MDC-002）でHigh Potential（ポテンシャル High/Mid）に評価されたタレントを、後継者候補のサジェストリストとして提示する。

### REQ-04: サクセッションマップの表示 【Should】

重要ポジションを列として、後継者候補をReadiness別に3行（Ready Now / Soon / Mid Term）で表示する「サクセッションマップ」画面を提供する。

### REQ-05: 育成進捗の追跡 【Could】

後継者候補の育成アクションに対して、進捗状況（未着手・進行中・完了）を記録し、次回MDCで確認できる。

---

## 3. スコープ外（作らないこと）

- ポジション要件と全タレントの自動スキルマッチングスコアリング
- 組織全体の人員計画・ヘッドカウント管理
- タレント本人への後継者候補指定の通知（社内機密情報として非公開）

---

## 4. 受入基準

### REQ-01: 重要ポジションマスタ

**AC-01-01: ポジションの登録**
- Given: 人材開発委員が重要ポジション管理画面を開いている
- When: ポジション名「バックエンドテックリード」・現担当者「山田花子」・重要度「Critical」を入力して保存する
- Then: ポジションが一覧に追加される

**AC-01-02: ポジションの一覧表示**
- Given: 5件の重要ポジションが登録されている
- When: 重要ポジション一覧を開く
- Then: 重要度順（Critical → High → Medium）でソートされて表示される

### REQ-02: 後継者候補の登録とReadiness評価

**AC-02-01: 候補者の追加**
- Given: 重要ポジション「バックエンドテックリード」の詳細画面が表示されている
- When: タレント「田中太郎」をReadiness「Ready Soon」・育成ギャップ「アーキテクチャ設計経験が不足」で候補者として登録する
- Then: 候補者が「Ready Soon」グループに表示される

**AC-02-02: 複数候補者の登録**
- Given: 1つの重要ポジションに候補者を追加しようとしている
- When: 既に3名の候補者が登録されているポジションに4名目を追加する
- Then: 候補者が追加される（上限制限なし）

**AC-02-03: Readinessの更新**
- Given: 候補者Aが「Ready Soon」で登録されている
- When: MDCで「Ready Now」に昇格させ更新する
- Then: Readinessが更新され、変更日時が記録される

### REQ-03: 9ボックスとの連動

**AC-03-01: 候補者サジェスト**
- Given: 現在の考課期間でHigh Potentialと評価されたタレントが8名いる
- When: 重要ポジションの候補者追加ダイアログで「推薦候補を表示」をクリックする
- Then: High Potentialの8名がサジェストリストに表示され、チェックボックスで選択して追加できる

### REQ-04: サクセッションマップ

**AC-04-01: マップの表示**
- Given: 3件の重要ポジションと各候補者が登録されている
- When: サクセッションマップ画面を開く
- Then: ポジションを列、Readiness（Ready Now / Soon / Mid Term）を行とするグリッドが表示される。各セルに候補者のバッジが配置される

**AC-04-02: カバレッジのアラート**
- Given: 重要度「Critical」のポジションに「Ready Now」候補者がいない
- When: サクセッションマップを表示する
- Then: 該当ポジションの「Ready Now」セルに「後継者未定」警告バッジが表示される

---

## 5. データモデル（概要）

```
CriticalPosition {
  id: string (ULID)
  positionName: string (max: 200)
  currentHolderId: string | null → User.id
  importance: enum { Critical, High, Medium }
  requiredSkills: string | null (max: 2000)
  isActive: boolean (default: true)
  createdAt: datetime
  updatedAt: datetime
}

SuccessionCandidate {
  id: string (ULID)
  positionId: string → CriticalPosition.id
  talentId: string → User.id
  readiness: enum { ReadyNow, ReadySoon, ReadyMidTerm }
  developmentGap: string | null (max: 1000)     # 育成ギャップ
  developmentActions: string | null (max: 2000)  # 育成アクション
  nominatedBy: string → User.id
  nominatedAt: datetime
  updatedAt: datetime
}

SuccessionDevelopmentAction {
  id: string (ULID)
  candidateId: string → SuccessionCandidate.id
  action: string (max: 500)
  status: enum { NotStarted, InProgress, Done }
  dueDate: date | null
  completedAt: datetime | null
  createdAt: datetime
}
```

---

## 6. APIエンドポイント（概要）

```
GET    /api/critical-positions                              → 重要ポジション一覧（MDC/Senior）
POST   /api/critical-positions                             → ポジション登録（MDC）
PUT    /api/critical-positions/{id}                        → ポジション更新（MDC）

GET    /api/critical-positions/{positionId}/candidates     → 候補者一覧
POST   /api/critical-positions/{positionId}/candidates     → 候補者追加（MDC/Senior）
PUT    /api/succession-candidates/{id}                     → Readiness/ギャップ更新（MDC/Senior）
DELETE /api/succession-candidates/{id}                     → 候補者除外（MDC）

GET    /api/succession-map                                 → サクセッションマップデータ（MDC）
GET    /api/talents/{talentId}/succession-candidacies      → タレントが候補の一覧（MDC）
```

---

## 7. 非機能要件

| 項目 | 要件 |
|------|------|
| パフォーマンス | サクセッションマップ（最大50ポジション × 候補者）の描画を3秒以内に完了する |
| セキュリティ | サクセッションプランは管理者（Admin/Senior/Executive）のみアクセス可能。タレント本人には非公開 |
| データ保持 | サクセッションプランのデータは組織改変後も3年間保持する |
| 監査ログ | 候補者の追加・Readiness変更はAuditLogに記録する |
| KeyPersonとの関係 | SPEC-MDC-001 の KeyPerson（重点管理人物）とは別概念。重点管理人物はタレント単位の育成管理、サクセッション候補はポジション単位の後継計画。同一タレントが両方に該当することは正常なケースである |

---

## 8. 関連仕様

- SPEC-MDC-001: 人材開発委員会管理（委員会での審議フロー）
- SPEC-MDC-002: 9ボックス評価（High Potential候補の抽出）
- SPEC-MDC-003: タレントプロファイル（候補指定状況の表示）
- SPEC-SKILL-001: スキル管理（ポジション要件との照合）

---

## 変更履歴

| バージョン | 日付 | 変更内容 | 変更者 |
|-----------|------|---------|--------|
| v1.0.0 | 2026-05-20 | 初版作成 | @KokiTokutake |
