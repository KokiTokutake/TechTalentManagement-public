#!/usr/bin/env bash
# guard-main-commit.sh
# PreToolUse hook for Bash tool — warns when committing directly to main.
# Exit 2 = block and show message. Exit 0 = allow.

INPUT=$(cat)

# Extract command
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
  exit 0
fi

[ -z "$COMMAND" ] && exit 0

# git commit コマンドを含む場合のみチェック
if ! echo "$COMMAND" | grep -qE 'git\s+commit'; then
  exit 0
fi

# 現在のブランチを確認
CURRENT_BRANCH=$(git branch --show-current 2>/dev/null)

if [ "$CURRENT_BRANCH" = "main" ]; then
  echo "⚠️ [GUARD] main ブランチへの直接コミットを検出しました。"
  echo ""
  echo "  現在のブランチ: main"
  echo "  コマンド: $(echo "$COMMAND" | head -1)"
  echo ""
  echo "  通常の実装変更は feat/ ブランチで行い、PR を通じて main にマージしてください。"
  echo ""
  echo "  main への直接コミットが許可されるのは以下の場合のみです:"
  echo "    - task-list.md / docs/ のメタデータのみの更新"
  echo "    - シードデータ・Docker 設定等の運用変更（CLAUDE.md のブランチルールを確認）"
  echo ""
  echo "  実装変更の場合は、先に /task-start を実行してフィーチャーブランチを作成してください。"
  echo "  この操作を続行する場合は承認してください。"
  exit 2
fi

exit 0
