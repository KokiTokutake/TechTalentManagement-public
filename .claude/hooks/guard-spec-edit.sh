#!/usr/bin/env bash
# guard-spec-edit.sh
# PreToolUse hook for Edit / Write tools.
# Blocks unauthorized edits to approved spec files when implementation is active.
# Exit 2 = block. Exit 0 = allow.

INPUT=$(cat)

# ── ファイルパスの抽出 ───────────────────────────────────────────────
if command -v python3 &>/dev/null; then
  FILE_PATH=$(printf '%s' "$INPUT" | python3 -c "
import sys, json
try:
    d = json.load(sys.stdin)
    ti = d.get('tool_input', {})
    # Edit tool uses 'file_path', Write tool also uses 'file_path'
    print(ti.get('file_path', ''))
except Exception:
    print('')
" 2>/dev/null)
elif command -v jq &>/dev/null; then
  FILE_PATH=$(printf '%s' "$INPUT" | jq -r '.tool_input.file_path // ""' 2>/dev/null)
else
  exit 0
fi

[ -z "$FILE_PATH" ] && exit 0

# ── docs/spec/ 以下のファイルのみ対象 ───────────────────────────────
if ! echo "$FILE_PATH" | grep -qE '(^|/)docs/spec/'; then
  exit 0
fi

# ── 対象ファイルが存在しない場合は新規作成なので許可 ────────────────
[ ! -f "$FILE_PATH" ] && exit 0

# ── 仕様書の現在のステータスを確認 ──────────────────────────────────
CURRENT_STATUS=$(grep -m1 '^\s*status\s*:' "$FILE_PATH" 2>/dev/null | sed 's/.*status\s*:\s*//' | tr -d '[:space:]"')

# approved でない場合は編集自由（draft / under_review 等）
if [ "$CURRENT_STATUS" != "approved" ]; then
  exit 0
fi

# ── 新しい内容でステータスが変更されるか確認 ─────────────────────────
# spec-change ワークフローの一部（status を approved 以外に変更）であれば許可する
if command -v python3 &>/dev/null; then
  NEW_CONTENT=$(printf '%s' "$INPUT" | python3 -c "
import sys, json
try:
    d = json.load(sys.stdin)
    ti = d.get('tool_input', {})
    # Edit: new_string / Write: content
    print(ti.get('new_string', ti.get('content', '')))
except Exception:
    print('')
" 2>/dev/null)
elif command -v jq &>/dev/null; then
  NEW_CONTENT=$(printf '%s' "$INPUT" | jq -r '(.tool_input.new_string // .tool_input.content // "")' 2>/dev/null)
fi

# 新しい内容が status: approved 以外のステータスに変更するなら spec-change 開始とみなし許可
if echo "$NEW_CONTENT" | grep -qE '^\s*status\s*:\s*(under_review|draft|change_requested)\s*$'; then
  exit 0
fi

# ── 実装が開始されているかを確認 ─────────────────────────────────────
IMPL_STARTED=0

# パターン1: tasks/ ディレクトリに in_progress / done のタスクがある
if [ -d "tasks" ]; then
  if grep -rl 'status:\s*\(in_progress\|done\)' tasks/ 2>/dev/null | grep -q .; then
    IMPL_STARTED=1
  fi
  # tasks/task-list.md の in_progress も確認
  if grep -q 'in_progress\|🔄\|✅' tasks/task-list.md 2>/dev/null; then
    IMPL_STARTED=1
  fi
fi

# パターン2: feat/ / fix/ / impl/ ブランチが存在する（実装ブランチ）
if command -v git &>/dev/null; then
  if git branch 2>/dev/null | grep -qE '^\s*(feat|fix|impl)/'; then
    IMPL_STARTED=1
  fi
fi

# 実装未開始なら編集許可（spec-change ワークフロー不要）
[ "$IMPL_STARTED" -eq 0 ] && exit 0

# ── ブロック ──────────────────────────────────────────────────────────
SPEC_ID=$(grep -m1 'spec[-_]id\|id\s*:' "$FILE_PATH" 2>/dev/null | sed 's/.*:\s*//' | tr -d '[:space:]"' | head -c 30)
[ -z "$SPEC_ID" ] && SPEC_ID="$(basename "$FILE_PATH")"

echo "⛔ [GUARD] approved 仕様書への無断編集をブロックしました。"
echo ""
echo "  ファイル : $FILE_PATH"
echo "  Spec ID  : $SPEC_ID"
echo "  ステータス: approved（実装フェーズ進行中）"
echo ""
echo "  approved 仕様書を変更するには /spec-change スキルを使用してください。"
echo "  /spec-change は変更影響範囲の評価・ステークホルダー承認・バージョニングを"
echo "  自動的に実施します。"
echo ""
echo "  どうしても直接編集が必要な場合は、ユーザーが明示的に承認してください。"
exit 2
