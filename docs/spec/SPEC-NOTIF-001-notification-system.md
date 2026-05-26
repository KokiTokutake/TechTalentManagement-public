# SPEC-NOTIF-001: 通知・アラートシステム

| 項目 | 内容 |
|------|------|
| Spec ID | SPEC-NOTIF-001 |
| ステータス | approved |
| バージョン | 1.0.0 |
| 最終更新 | 2026-05-26 |
| オーナー | @KokiTokutake |
| 関連 Spec | SPEC-EVAL-001, SPEC-EVAL-002, SPEC-MDC-002, SPEC-MDC-005, SPEC-SKILL-002 |

---

## 1. 背景・目的（Why）

### 1.1 課題・背景

各機能仕様書（SPEC-EVAL-001/002、SPEC-MDC-002/005 等）で「システム内通知」「アラート」が受入基準に含まれているが、通知の仕組み自体は未実装である。

- **考課期限の見逃し**: 目標設定・自己評価・上長評価の提出期限を各自が管理しており、未提出のまま期限を超えることがある
- **チェックイン催促の欠如**: KPI 中間振り返りが行われず、期末に評価が集中する
- **重要なステータス変更の見落とし**: 考課差し戻し・重点管理候補フラグ・後継者ギャップなど重要なイベントを担当者が気づかないことがある
- **委員会準備コストが高い**: 会議前に委員が最新データを確認するトリガーがない

### 1.2 解決する価値

- 期限・未対応アクションをシステムが自動検知し、担当者に通知する
- 重要な状態変化（差し戻し・重点管理候補選定・後継者ギャップ）を即時通知する
- 通知から対応画面へ直接遷移できるため、アクションまでの時間を短縮する

### 1.3 スコープ外（やらないこと）

- メール・Slack 通知（v2 以降の検討事項。Constitution.md に記載）
- プッシュ通知（モバイルアプリ非対象）
- 通知設定のカスタマイズ（v2 以降）

---

## 2. 要件（What）

### 2.1 機能要件

#### ユーザーストーリー

```
As a タレント,
I want to 未対応タスクや期限をシステムから知らせてもらいたい,
So that 重要な考課・キャリア関連のアクションを見逃さずに対応できる。

As a 上長（Manager）,
I want to 部下の考課状況や重要イベントを通知で受け取りたい,
So that 適切なタイミングでフィードバックや承認を行える。

As a Senior / Executive,
I want to MDC関連のアラート（後継者ギャップ・重点管理候補等）を受け取りたい,
So that 委員会準備や人材施策の意思決定を迅速に行える。
```

#### 通知トリガー一覧（Must）

| ID | トリガー | 送信先 | 関連 Spec |
|----|---------|--------|-----------|
| NT-01 | 考課期間が開始された | 全タレント・全上長 | SPEC-EVAL-001 |
| NT-02 | 自己評価の未提出（期限3日前） | 対象タレント | SPEC-EVAL-001 |
| NT-03 | 自己評価が提出された | 上長 | SPEC-EVAL-001 |
| NT-04 | 上長評価で差し戻しされた | タレント | SPEC-EVAL-001 |
| NT-05 | 上長評価が確定した | タレント | SPEC-EVAL-001 |
| NT-06 | KPI チェックイン未実施（期間50%経過時） | タレント・上長 | SPEC-EVAL-002 |
| NT-07 | 9ボックス評価で重点管理候補にフラグが立った | Senior・Executive | SPEC-MDC-002 |
| NT-08 | Critical ポジションに Ready Now 候補がいない（後継者ギャップ） | Senior・Executive | SPEC-MDC-005 |
| NT-09 | 後継者候補として登録された | 対象タレントの上長 | SPEC-MDC-005 |
| NT-10 | スキルコンピテンシー評価の未実施（考課期間終了1週間前） | タレント・上長 | SPEC-SKILL-002 |

#### 要件一覧

| ID | 要件 | 優先度 |
|----|------|--------|
| REQ-01 | 通知センターをヘッダーに常時表示し、未読件数バッジを表示する | Must |
| REQ-02 | 通知一覧を新着順で表示し、既読/未読を管理できる | Must |
| REQ-03 | 通知をクリックすると対応画面へ遷移し、既読にする | Must |
| REQ-04 | トリガーイベント発生時にリアルタイムで通知を生成する | Must |
| REQ-05 | 全通知を既読にする「一括既読」ボタンを提供する | Should |
| REQ-06 | 通知を種別（考課・MDC・スキル）でフィルタできる | Could |

### 2.2 非機能要件

| 項目 | 要件 |
|------|------|
| パフォーマンス | 通知一覧取得 200ms 以内 |
| リアルタイム性 | イベント発生から30秒以内に通知が生成される |
| 保持期間 | 既読通知は90日で自動削除。未読は無期限保持 |

---

## 3. データモデル

### Notification エンティティ

| プロパティ | 型 | 制約 | 説明 |
|-----------|-----|------|------|
| Id | string (ULID) | PK | |
| UserId | string | FK → User.Id, Required | 通知受信者 |
| Type | enum | Required | 通知種別（下記） |
| Title | string | max:200, Required | 通知タイトル |
| Body | string | max:500, Required | 通知本文 |
| LinkUrl | string? | max:500, nullable | 遷移先URL（相対パス） |
| IsRead | bool | Default: false | 既読フラグ |
| ReadAt | DateTime? | nullable | 既読日時 |
| CreatedAt | DateTime | Required | 通知生成日時 |
| ExpiresAt | DateTime? | nullable | 有効期限（既読後90日） |

### NotificationType 列挙型

```csharp
public enum NotificationType
{
    EvaluationPeriodStarted,    // NT-01
    SelfEvaluationDueSoon,      // NT-02
    SelfEvaluationSubmitted,    // NT-03
    EvaluationRejected,         // NT-04
    EvaluationApproved,         // NT-05
    KpiCheckinOverdue,          // NT-06
    NineBoxKeyPersonCandidate,  // NT-07
    SuccessionGapAlert,         // NT-08
    SuccessionCandidateAdded,   // NT-09
    CompetencyEvaluationDueSoon // NT-10
}
```

---

## 4. APIエンドポイント

| メソッド | パス | 説明 | 認可 |
|---------|------|------|------|
| GET | /api/notifications?userId={id}&unreadOnly={bool} | 通知一覧取得 | 本人のみ |
| GET | /api/notifications/unread-count?userId={id} | 未読件数取得 | 本人のみ |
| PUT | /api/notifications/{id}/read | 1件既読化 | 本人のみ |
| PUT | /api/notifications/read-all?userId={id} | 全件一括既読 | 本人のみ |

---

## 5. UIコンポーネント設計

### 5.1 通知ベルアイコン（ヘッダー共通）

- 全ページのヘッダーに表示
- 未読件数を赤バッジで表示（99件超は「99+」）
- クリックで通知ドロップダウンを表示

### 5.2 通知ドロップダウン

- 最新10件を表示（「すべて見る」リンクあり）
- 未読は背景色（light blue）で区別
- 各通知クリック → 対応画面に遷移 + 既読化

### 5.3 通知一覧ページ（/notifications）

- 全通知を新着順でリスト表示
- 「一括既読」ボタン
- 種別フィルタ（Could: REQ-06）

### data-testid 定義

| 要素 | data-testid |
|------|-------------|
| 通知ベルアイコン | `notification-bell` |
| 未読件数バッジ | `notification-unread-badge` |
| 通知ドロップダウン | `notification-dropdown` |
| 通知アイテム（行） | `notification-item-{id}` |
| 一括既読ボタン | `notification-read-all-button` |
| 通知一覧ページ | `notification-list` |

---

## 6. 受入基準

### REQ-01: 通知センター表示

**AC-01-01: 未読バッジの表示**
- Given: ログイン中のユーザーに未読通知が3件ある
- When: 任意のページを表示する
- Then: ヘッダーの通知ベルに「3」の赤バッジが表示される

**AC-01-02: 未読がない場合はバッジを非表示**
- Given: ログイン中のユーザーの未読通知が0件
- When: 任意のページを表示する
- Then: 通知ベルにバッジが表示されない

### REQ-02: 通知一覧

**AC-02-01: 通知一覧の表示**
- Given: ユーザーに通知が5件ある（うち未読2件）
- When: 通知ベルをクリックする
- Then: ドロップダウンに通知が新着順で表示され、未読2件が青背景で区別される

### REQ-03: 既読化と画面遷移

**AC-03-01: 通知クリックで既読化と遷移**
- Given: 未読の「自己評価が差し戻されました」通知がある
- When: 通知をクリックする
- Then: 自己評価入力画面へ遷移し、通知が既読になる（バッジ数が減る）

**AC-03-02: 一括既読**
- Given: 未読通知が5件ある
- When: 「一括既読」ボタンをクリックする
- Then: 全通知が既読になり、バッジが非表示になる

### REQ-04: トリガー別通知生成

**AC-04-01: 考課差し戻し通知（NT-04）**
- Given: タレントが自己評価を提出している
- When: 上長が「差し戻し」を実行する
- Then: タレントの未読通知に「上長評価で差し戻されました」が追加される

**AC-04-02: 後継者ギャップアラート（NT-08）**
- Given: Critical ポジションが登録されており、Ready Now 候補が存在しない
- When: 候補者管理システムが状態を検知する（登録時・定期チェック）
- Then: Senior・Executive ロールのユーザーに「後継者未定アラート」通知が生成される

**AC-04-03: 重点管理候補フラグ通知（NT-07）**
- Given: 9ボックス評価で特定タレントが High Potential セルに入った
- When: キャリブレーション確定後にフラグが自動設定される
- Then: Senior・Executive に「重点管理候補が自動フラグされました」通知が生成される

---

## 7. 変更履歴

| 日付 | バージョン | 変更者 | 変更内容 |
|------|-----------|--------|---------|
| 2026-05-26 | 1.0.0 | @KokiTokutake | 初版作成（/spec-write により生成） |
