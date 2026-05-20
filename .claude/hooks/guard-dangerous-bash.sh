#!/usr/bin/env bash
# guard-dangerous-bash.sh
# PreToolUse hook for Bash tool — blocks destructive operations.
# Exit 2 = block and show message to Claude. Exit 0 = allow.

INPUT=$(cat)

# Extract command from JSON (python3 preferred, jq fallback)
if command -v python3 &>/dev/null; then
  COMMAND=$(printf '%s' "$INPUT" | python3 -c "
import sys, json
try:
    d = json.load(sys.stdin)
    print(d.get('tool_input', {}).get('command', ''))
except Exception:
    print('')
" 2>/dev/null)
elif command -v jq &>/dev/null; then
  COMMAND=$(printf '%s' "$INPUT" | jq -r '.tool_input.command // ""' 2>/dev/null)
else
  exit 0  # パーサが使えない場合はスキップ
fi

[ -z "$COMMAND" ] && exit 0

# ── 1. ファイル・ディレクトリの破壊的削除 ────────────────────────────
if echo "$COMMAND" | grep -qE 'rm\s+-[a-zA-Z]*r[a-zA-Z]*f\s+(\/|~\/|\*|\.\s*$|\.\/)'; then
  echo "⛔ [GUARD] 破壊的な削除コマンドを検出しました。"
  echo ""
  echo "  コマンド: $(echo "$COMMAND" | head -1)"
  echo ""
  echo "  rm -rf / や rm -rf * はプロジェクト外のファイルを破壊する恐れがあります。"
  echo "  削除対象を明示的なパスで指定してください。"
  exit 2
fi

# ── 2. git push --force / -f ────────────────────────────────────────
if echo "$COMMAND" | grep -qE 'git\s+push.*(--force|-f)\b'; then
  # main / master へのforce pushは完全ブロック
  if echo "$COMMAND" | grep -qE '(main|master)'; then
    echo "⛔ [GUARD] main/master への force push は禁止されています。"
    echo ""
    echo "  コマンド: $(echo "$COMMAND" | head -1)"
    echo ""
    echo "  保護ブランチへの強制上書きはチームの作業を失わせる危険があります。"
    echo "  GitHub Branch Protection Rules が設定されているか確認してください。"
    exit 2
  fi
  # 他ブランチは警告のみ（ブロックしない）
  echo "⚠️ [GUARD] force push を検出しました。リモートの履歴が書き換わります。"
  echo "  本当にこの操作が必要か確認してから承認してください。"
  # exit 2 せず、ユーザーの承認に委ねる
fi

# ── 3. git reset --hard ──────────────────────────────────────────────
if echo "$COMMAND" | grep -qE 'git\s+reset\s+--hard'; then
  echo "⛔ [GUARD] git reset --hard を検出しました。"
  echo ""
  echo "  コマンド: $(echo "$COMMAND" | head -1)"
  echo ""
  echo "  コミットされていない変更がすべて失われます。"
  echo "  意図した操作であれば、ユーザーが直接ターミナルで実行してください（! git reset --hard）。"
  exit 2
fi

# ── 4. git clean -f / -fd / -fdx ────────────────────────────────────
if echo "$COMMAND" | grep -qE 'git\s+clean\s+-[a-zA-Z]*f'; then
  echo "⛔ [GUARD] git clean -f を検出しました。"
  echo ""
  echo "  コマンド: $(echo "$COMMAND" | head -1)"
  echo ""
  echo "  未追跡ファイルが完全に削除されます。"
  echo "  意図した操作であれば、ユーザーが直接ターミナルで実行してください（! git clean -fd）。"
  exit 2
fi

# ── 5. git checkout -- . / git restore . ────────────────────────────
if echo "$COMMAND" | grep -qE 'git\s+(checkout\s+--\s+\.|restore\s+\.)'; then
  echo "⛔ [GUARD] ワーキングツリーの全変更を破棄するコマンドを検出しました。"
  echo ""
  echo "  コマンド: $(echo "$COMMAND" | head -1)"
  echo ""
  echo "  コミットされていない変更がすべて失われます。"
  echo "  意図した操作であれば、ユーザーが直接ターミナルで実行してください。"
  exit 2
fi

# ── 6. SQL の破壊的操作 ─────────────────────────────────────────────
if echo "$COMMAND" | grep -qiE '(DROP\s+(TABLE|DATABASE|SCHEMA)|TRUNCATE\s+TABLE)'; then
  echo "⛔ [GUARD] 破壊的なSQL操作を検出しました。"
  echo ""
  echo "  コマンド: $(echo "$COMMAND" | head -1)"
  echo ""
  echo "  DROP / TRUNCATE はデータを完全に失います。"
  echo "  マイグレーションスクリプトや /spec-change ワークフローを使用してください。"
  exit 2
fi

# ── 7. 本番環境変数が設定された状態での破壊的コマンド ──────────────
if echo "$COMMAND" | grep -qE 'NODE_ENV=production|RAILS_ENV=production|APP_ENV=production'; then
  if echo "$COMMAND" | grep -qE '(drop|delete|destroy|reset|truncate|migrate:reset)'; then
    echo "⛔ [GUARD] 本番環境 (production) での破壊的な操作を検出しました。"
    echo ""
    echo "  コマンド: $(echo "$COMMAND" | head -1)"
    echo ""
    echo "  本番データが失われる可能性があります。この操作は許可されていません。"
    exit 2
  fi
fi

# ── 8. main/master への直接コミットをブロック ────────────────────────
if echo "$COMMAND" | grep -qE '^\s*git\s+commit'; then
  CURRENT_BRANCH=$(git branch --show-current 2>/dev/null)
  if [ "$CURRENT_BRANCH" = "main" ] || [ "$CURRENT_BRANCH" = "master" ]; then
    echo "⛔ [GUARD] main/master ブランチへの直接コミットは禁止されています。"
    echo ""
    echo "  現在のブランチ: $CURRENT_BRANCH"
    echo ""
    echo "  ブランチを作成してから作業してください:"
    echo "    /task-start [TASK-ID]          — 実装タスク"
    echo "    /design-create [SPEC-ID]       — 設計書作成（design/ ブランチを自動作成）"
    echo "    git checkout -b chore/[概要]   — ハーネス・ツール修正"
    exit 2
  fi
fi

# ── 9. main/master への直接プッシュをブロック ────────────────────────
if echo "$COMMAND" | grep -qE 'git\s+push' && ! echo "$COMMAND" | grep -qE '(--force|-f)\b'; then
  # 明示的に main/master を対象にしている場合
  if echo "$COMMAND" | grep -qE '\b(origin\s+main|origin\s+master|main\s*$|master\s*$)'; then
    echo "⛔ [GUARD] main/master への直接プッシュは禁止されています。"
    echo ""
    echo "  PR を作成してレビュー・承認後にマージしてください:"
    echo "    /task-done   — タスク完了・PR 作成"
    echo "    gh pr create — 手動で PR を作成"
    exit 2
  fi
  # 引数なしの push で現在が main の場合
  if echo "$COMMAND" | grep -qE '^git\s+push\s*$'; then
    CURRENT_BRANCH=$(git branch --show-current 2>/dev/null)
    if [ "$CURRENT_BRANCH" = "main" ] || [ "$CURRENT_BRANCH" = "master" ]; then
      echo "⛔ [GUARD] main/master ブランチからの直接プッシュは禁止されています。"
      echo ""
      echo "  現在のブランチ: $CURRENT_BRANCH"
      echo ""
      echo "  PR を作成してレビュー・承認後にマージしてください:"
      echo "    /task-done   — タスク完了・PR 作成"
      exit 2
    fi
  fi
fi

exit 0
