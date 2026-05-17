# GitHub リポジトリ設定ガイド

仕様駆動開発を支えるための GitHub リポジトリ設定手順です。
プロジェクト開始時（PHASE 1〜2）に一度設定します。

---

## 1. リポジトリ作成

```bash
# GitHub CLI でリポジトリ作成
gh repo create [org]/[repo-name] \
  --private \
  --description "[プロジェクトの説明]" \
  --clone

cd [repo-name]

# ハーネスのテンプレートをコピー
cp -r /path/to/harness/templates/. .
git add .
git commit -m "add: SDD ハーネステンプレートを追加"
git push origin main
```

---

## 2. Branch Protection Rules（必須）

`main` ブランチを保護し、直接プッシュを禁止します。

### GitHub Web UI での設定手順

1. リポジトリの **Settings** → **Branches** を開く
2. **Add branch protection rule** をクリック
3. **Branch name pattern**: `main`
4. 以下を設定する：

| 設定項目 | 値 | 理由 |
|---------|-----|------|
| Require a pull request before merging | ✅ ON | 直接プッシュを禁止 |
| Required approvals | 1（小規模）/ 2（中規模以上） | レビュー必須化 |
| Dismiss stale pull request approvals | ✅ ON | 変更後は再承認が必要 |
| Require review from Code Owners | ✅ ON | CODEOWNERS に従う |
| Require status checks to pass | ✅ ON | CI 必須化 |
| Required status checks | `CI / テスト` | テスト通過を必須に |
| Require branches to be up to date | ✅ ON | マージ前に最新化必須 |
| Do not allow bypassing | ✅ ON（推奨） | 管理者も例外なし |

### GitHub CLI での設定（参考）

```bash
gh api repos/{owner}/{repo}/branches/main/protection \
  --method PUT \
  --field required_status_checks='{"strict":true,"contexts":["CI / テスト"]}' \
  --field enforce_admins=true \
  --field required_pull_request_reviews='{"required_approving_review_count":1,"dismiss_stale_reviews":true,"require_code_owner_reviews":true}' \
  --field restrictions=null
```

---

## 3. CODEOWNERS 設定

`.github/CODEOWNERS` を作成し、領域別の責任者を定義します。

```bash
# テンプレートを編集
code .github/CODEOWNERS
# → 実際のユーザー名・チーム名に置き換える

git add .github/CODEOWNERS
git commit -m "add: CODEOWNERS を設定"
git push origin main
```

**優先度の高い設定:**
- `docs/spec/` → プロダクトオーナー + テックリード
- `.constitution/` → 全ステークホルダー
- `.github/workflows/` → テックリード

---

## 4. Issue ラベルの設定

```bash
# 必須ラベルを作成
gh label create "spec-change"        --color "0052CC" --description "仕様書の変更を伴う"
gh label create "constitution-change" --color "B60205" --description "憲章の変更を伴う"
gh label create "bug"                --color "d73a4a" --description "仕様との不一致"
gh label create "enhancement"        --color "a2eeef" --description "機能追加・改善"
gh label create "question"           --color "d876e3" --description "仕様の解釈に関する質問"
gh label create "blocked"            --color "e4e669" --description "ブロック中"
```

---

## 5. マイルストーンの設定

```bash
# スプリント or リリースごとにマイルストーンを作成
gh api repos/{owner}/{repo}/milestones \
  --method POST \
  --field title="v1.0.0" \
  --field description="最初のリリース" \
  --field due_on="YYYY-MM-DDT00:00:00Z"
```

---

## 6. GitHub Projects 設定（任意）

タスク管理を GitHub Projects で行う場合：

1. リポジトリの **Projects** タブ → **New project**
2. **Board** ビューを選択
3. カラム設定:
   - `未着手` / `進行中` / `レビュー中` / `完了`
4. Issue を作成すると自動でボードに追加されるように設定

---

## 7. Secrets・環境変数の設定

```bash
# CI/CD で使用するシークレットを設定
gh secret set ANTHROPIC_API_KEY   # Claude Code API キー
gh secret set OPENAI_API_KEY      # Codex API キー（使用する場合）

# 環境別のシークレット
gh secret set DATABASE_URL --env staging
gh secret set DATABASE_URL --env production
```

---

## 8. 規模別の推奨設定

### 小規模（3〜5人）

```
Branch Protection:
  - Required approvals: 1
  - Code Owners review: OFF（任意レビュアーで可）

CODEOWNERS:
  * @tech-lead
  docs/spec/ @tech-lead @product-owner
```

### 中規模（6〜30人）

```
Branch Protection:
  - Required approvals: 2
  - Code Owners review: ON

CODEOWNERS:
  docs/spec/   @org/product-team
  src/         @org/dev-team
  .github/     @org/platform-team
```

### 大規模（50人以上）

```
Branch Protection:
  - Required approvals: 2（機能ブランチ）/ 3（main）
  - Code Owners review: ON
  - Require signed commits: ON（監査対応）

CODEOWNERS:
  docs/spec/[domain]/   @org/[domain]-product-team
  src/[domain]/         @org/[domain]-dev-team

追加設定:
  - Rulesets（ブランチ保護の高度な設定）
  - Organization の Security Policy
  - Secret Scanning の有効化
```

---

## 9. 設定後の確認チェックリスト

- [ ] `main` ブランチへの直接プッシュが禁止されている
- [ ] PR なしではマージできない
- [ ] CI（テスト）が通らないとマージできない
- [ ] CODEOWNERS のファイルが正しく機能している（テスト PR で確認）
- [ ] 必須ラベルが作成されている
- [ ] Secrets が設定されている
- [ ] チームメンバー全員がリポジトリへのアクセス権を持っている
