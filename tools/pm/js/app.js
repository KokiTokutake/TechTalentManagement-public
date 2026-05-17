/* =====================================================
   SDD Project Manager — app.js
   ルーター + 全ビュー + モーダル + イベント
   ===================================================== */

'use strict';

window.SDD = window.SDD || {};

/* ═══════════════════════════════════════════════════════
   UTILITIES
═══════════════════════════════════════════════════════ */

const $ = id => document.getElementById(id);
const h = str => String(str).replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
const fmt = iso => iso ? iso.slice(0, 10) : '';
const fmtDT = iso => iso ? iso.slice(0, 16).replace('T', ' ') : '';

function badge(status) {
  const label = {
    approved:'approved', review:'review', draft:'draft', deprecated:'deprecated',
    done:'done', in_progress:'進行中', pending:'未着手', open:'open',
    in_progress2:'in_progress', resolved:'resolved',
    Must:'Must', Should:'Should', Could:'Could',
    High:'High', Medium:'Medium', Low:'Low',
    bug:'bug', blocker:'blocker', question:'question', risk:'risk'
  };
  const cls = status.replace(/_/g, '_');
  return `<span class="badge badge-${h(cls)}">${h(status)}</span>`;
}

function statusBadge(status) { return badge(status); }

function el(tag, attrs, inner) {
  const parts = [`<${tag}`];
  if (attrs) Object.entries(attrs).forEach(([k,v]) => parts.push(` ${k}="${h(String(v))}"`));
  parts.push(`>${inner || ''}</${tag}>`);
  return parts.join('');
}

/* ═══════════════════════════════════════════════════════
   ROUTER
═══════════════════════════════════════════════════════ */

const VIEWS = ['dashboard','specs','tasks','sprints','retros','issues','settings'];

function navigate(view) {
  if (!VIEWS.includes(view)) view = 'dashboard';
  SDD.app.currentView = view;
  document.querySelectorAll('.nav-item').forEach(el => {
    el.classList.toggle('active', el.dataset.view === view);
  });
  renderView(view);
}

function renderView(view) {
  const content = $('content');
  const topbar  = $('topbar-title');
  const actions = $('topbar-actions');

  const titles = {
    dashboard: 'ダッシュボード',
    specs:     '仕様書管理',
    tasks:     'タスクボード',
    sprints:   'スプリント管理',
    retros:    'ふりかえり',
    issues:    '課題・ブロッカー',
    settings:  '設定'
  };
  topbar.textContent = titles[view] || view;

  const renders = {
    dashboard: renderDashboard,
    specs:     renderSpecs,
    tasks:     renderTasks,
    sprints:   renderSprints,
    retros:    renderRetros,
    issues:    renderIssues,
    settings:  renderSettings
  };

  const actionBuilders = {
    dashboard: () => `
      <button class="btn btn-primary" onclick="SDD.app.openModal('spec','new')">＋ 仕様書</button>
      <button class="btn btn-primary" onclick="SDD.app.openModal('task','new')">＋ タスク</button>
      <button class="btn btn-warning" onclick="SDD.app.openModal('issue','new')">＋ 課題</button>`,
    specs:     () => `<button class="btn btn-primary" onclick="SDD.app.openModal('spec','new')">＋ 仕様書を追加</button>`,
    tasks:     () => `<button class="btn btn-ghost" onclick="SDD.app.downloadTaskListMd()" title="task-list.md をダウンロード">⬇ MD エクスポート</button><button class="btn btn-primary" onclick="SDD.app.openModal('task','new')">＋ タスクを追加</button>`,
    sprints:   () => `<button class="btn btn-primary" onclick="SDD.app.openModal('sprint','new')">＋ スプリント作成</button>`,
    retros:    () => `<button class="btn btn-primary" onclick="SDD.app.openModal('retro','new')">＋ ふりかえり作成</button>`,
    issues:    () => `<button class="btn btn-warning" onclick="SDD.app.openModal('issue','new')">＋ 課題を追加</button>`,
    settings:  () => ''
  };

  actions.innerHTML = actionBuilders[view] ? actionBuilders[view]() : '';
  content.innerHTML = renders[view] ? renders[view]() : '';
  bindViewEvents(view);
}

/* ═══════════════════════════════════════════════════════
   DASHBOARD VIEW
═══════════════════════════════════════════════════════ */

function renderDashboard() {
  const store = SDD.store;
  const d = store.data;
  const score = store.calcSddScore();
  const scoreCls = score >= 80 ? 'excellent' : score >= 60 ? 'good' : score >= 40 ? 'warning' : 'poor';
  const scoreColor = score >= 80 ? 'var(--col-approved)' : score >= 60 ? 'var(--col-review)' : score >= 40 ? 'var(--col-draft)' : 'var(--col-high)';

  // SVG ring: circumference = 2π×34 ≈ 213.6
  const R = 34, C = 2 * Math.PI * R;
  const dash = (score / 100) * C;
  const scoreRing = `
    <div class="score-kpi">
      <div class="score-ring-svg ${scoreCls}">
        <svg width="88" height="88" viewBox="0 0 88 88">
          <circle cx="44" cy="44" r="${R}" fill="none" stroke="var(--bg-sidebar)" stroke-width="6"/>
          <circle class="ring-fg" cx="44" cy="44" r="${R}" fill="none" stroke-width="6"
            stroke-linecap="round"
            stroke-dasharray="${dash.toFixed(1)} ${C.toFixed(1)}"
            stroke-dashoffset="0"/>
        </svg>
        <div class="score-ring-val ring-val" style="color:${scoreColor}">${score}</div>
      </div>
      <div class="score-name">SDD スコア</div>
      <div class="score-desc">仕様先行遵守率</div>
    </div>`;

  // spec status
  const specCounts = {approved:0, review:0, draft:0, deprecated:0};
  d.specs.forEach(s => { if (specCounts[s.status] !== undefined) specCounts[s.status]++; });
  const specTotal = d.specs.length || 1;

  // task status
  const taskCounts = {done:0, in_progress:0, review:0, pending:0};
  d.tasks.forEach(t => { if (taskCounts[t.status] !== undefined) taskCounts[t.status]++; });
  const taskTotal = d.tasks.length || 1;

  // segment bar helper
  function segBar(counts, total, keys) {
    const bars = keys.map(([k, cls]) => {
      const pct = Math.round(((counts[k]||0) / total) * 100);
      return pct > 0 ? `<div class="seg-bar-item ${cls}" style="flex:${pct}"></div>` : '';
    }).join('');
    const legend = keys.map(([k, cls, label]) => {
      const n = counts[k] || 0;
      return n > 0 ? `<span class="seg-legend-item"><span class="seg-legend-dot" style="background:var(--col-${cls})"></span>${label} ${n}</span>` : '';
    }).join('');
    return `<div class="seg-bar">${bars}</div><div class="seg-legend">${legend}</div>`;
  }

  // status rows helper
  function statusRows(counts, total, defs) {
    return defs.map(([k, label]) => {
      const v = counts[k] || 0;
      const pct = Math.round((v / total) * 100);
      return `
        <div class="status-row">
          <div class="status-row-label">
            <span class="status-dot ${k}"></span>${label}
          </div>
          <div class="status-row-bar">
            <div class="status-row-bar-fill ${k}" style="width:${pct}%;background:var(--col-${k === 'in_progress' ? 'in-progress' : k})"></div>
          </div>
          <div class="status-row-count">${v}</div>
        </div>`;
    }).join('');
  }

  // open issues
  const openIssues = d.issues.filter(i => i.status !== 'resolved').slice(0, 5);
  const openCount  = d.issues.filter(i => i.status !== 'resolved').length;

  // sprint
  const activeSprint = store.getActiveSprint();
  const sprintHtml = activeSprint ? (() => {
    const tasks = store.getSprintTasks(activeSprint.id);
    const total = tasks.reduce((s,t) => s + (t.sp||0), 0);
    const done  = tasks.filter(t => t.status === 'done').reduce((s,t) => s + (t.sp||0), 0);
    const pct   = total ? Math.round((done/total)*100) : 0;
    const rem   = store.getRemainingDays(activeSprint.endDate);
    const spDash  = (pct / 100) * C;
    return `
      <div class="sprint-dash-card section-gap">
        <div class="sprint-dash-header">
          <div>
            <div style="font-size:10px;font-weight:700;color:var(--muted);letter-spacing:.1em;text-transform:uppercase;margin-bottom:4px;font-family:'JetBrains Mono',monospace;">ACTIVE SPRINT</div>
            <div class="sprint-dash-name">${h(activeSprint.name)}</div>
            <div class="sprint-dash-goal">${h(activeSprint.goal)}</div>
          </div>
          <div style="display:flex;flex-direction:column;align-items:flex-end;gap:6px">
            <div class="sprint-dash-pct">${pct}<span style="font-size:16px">%</span></div>
            ${rem !== null ? `<span class="sprint-dash-days">残 ${rem} 日</span>` : ''}
          </div>
        </div>
        <div style="display:flex;align-items:center;gap:12px;margin-bottom:6px">
          <div class="sprint-dash-sp"><span>${done}</span> / ${total} SP 完了</div>
        </div>
        <div class="progress-wrap"><div class="progress-bar blue" style="width:${pct}%;height:5px"></div></div>
      </div>`;
  })() : `
    <div class="card section-gap" style="border-style:dashed;text-align:center;padding:20px">
      <div style="font-size:12px;color:var(--muted)">アクティブなスプリントがありません</div>
      <button class="btn btn-primary btn-sm" style="margin-top:10px" onclick="SDD.app.openModal('sprint','new')">＋ スプリントを開始</button>
    </div>`;

  // activity
  const activity = d.activityLog.slice(0, 8);

  return `
    <!-- KPI row -->
    <div class="dash-kpi-grid section-gap">
      <div class="kpi-card">${scoreRing}</div>

      <div class="kpi-card">
        <div class="kpi-label">仕様書</div>
        <div class="kpi-number" style="color:var(--accent)">${d.specs.length}</div>
        ${segBar(specCounts, specTotal, [['approved','approved','approved'],['review','review','review'],['draft','draft','draft'],['deprecated','deprecated','deprecated']])}
      </div>

      <div class="kpi-card">
        <div class="kpi-label">タスク</div>
        <div class="kpi-number" style="color:var(--accent-3)">${d.tasks.length}</div>
        ${segBar(taskCounts, taskTotal, [['done','done','done'],['in_progress','in-progress','in_progress'],['review','review','review'],['pending','pending','pending']])}
      </div>

      <div class="kpi-card">
        <div class="kpi-label">未解決の課題</div>
        <div class="kpi-number" style="color:${openCount > 0 ? 'var(--col-high)' : 'var(--col-approved)'}">${openCount}</div>
        <div class="kpi-sub" style="margin-top:10px">${openCount > 0 ? `${openIssues.filter(i=>i.priority==='High').length} 件 High 優先度` : '課題なし'}</div>
      </div>
    </div>

    <!-- Sprint -->
    ${sprintHtml}

    <!-- Status breakdown + Issues -->
    <div class="grid-2 section-gap">
      <div class="card">
        <div class="card-title">仕様書 ステータス</div>
        ${statusRows(specCounts, specTotal, [
          ['approved','Approved'],['review','Review'],['draft','Draft'],['deprecated','Deprecated']
        ])}
        <div style="margin-top:14px">
        <div class="card-title">タスク ステータス</div>
        ${statusRows(taskCounts, taskTotal, [
          ['done','Done'],['in_progress','In Progress'],['review','Review'],['pending','Pending']
        ])}
        </div>
      </div>

      <div class="card">
        <div class="card-title">未解決の課題・ブロッカー</div>
        ${openIssues.length ? openIssues.map(i => `
          <div class="activity-item">
            <div class="activity-dot" style="background:${i.priority==='High'?'var(--col-high)':i.priority==='Medium'?'var(--col-medium)':'var(--col-low)'};box-shadow:0 0 4px ${i.priority==='High'?'var(--col-high)':i.priority==='Medium'?'var(--col-medium)':'var(--col-low)'}"></div>
            <div style="flex:1;min-width:0">
              <div style="display:flex;align-items:center;gap:6px;flex-wrap:wrap">
                <span class="badge badge-${h(i.type)}">${h(i.type)}</span>
                <span style="color:var(--text)">${h(i.title)}</span>
              </div>
              <div class="activity-time mt-4">${h(i.id)} · ${fmt(i.createdAt)} · ${h(i.priority||'')}</div>
            </div>
          </div>`).join('') : '<div class="empty-state" style="padding:24px">未解決の課題はありません ✓</div>'}
      </div>
    </div>

    <!-- Activity -->
    <div class="card section-gap">
      <div class="card-title">アクティビティ</div>
      <div style="display:grid;grid-template-columns:repeat(2,1fr);gap:0 24px">
        ${activity.length ? activity.map(a => `
          <div class="activity-item">
            <div class="activity-dot"></div>
            <div style="flex:1;min-width:0">
              <div class="activity-text">${h(a.text)}</div>
              <div class="activity-time">${fmtDT(a.timestamp)} · ${h(a.user)}</div>
            </div>
          </div>`).join('') : '<div class="empty-state" style="padding:16px;grid-column:span 2">アクティビティがありません</div>'}
      </div>
    </div>`;
}

/* ═══════════════════════════════════════════════════════
   SPECS VIEW
═══════════════════════════════════════════════════════ */

let specFilter = 'all';

function renderSpecs() {
  const specs = SDD.store.data.specs.filter(s =>
    specFilter === 'all' || s.status === specFilter
  );

  return `
    <div class="card">
      <div class="filter-tabs">
        ${['all','approved','review','draft','deprecated'].map(f =>
          `<button class="filter-tab${specFilter===f?' active':''}" data-filter="${f}">${f==='all'?'すべて':f}</button>`
        ).join('')}
      </div>
      <div class="table-wrap">
        <table>
          <thead><tr>
            <th>Spec ID</th><th>タイトル</th><th>ステータス</th><th>カテゴリ</th>
            <th>優先度</th><th>オーナー</th><th>タスク数</th><th>更新日</th><th>操作</th>
          </tr></thead>
          <tbody>
            ${specs.length ? specs.map(s => `
              <tr>
                <td style="font-family:monospace;font-size:11px">${h(s.id)}</td>
                <td style="max-width:200px">
                  <div style="font-weight:600">${h(s.title)}</div>
                  ${s.description ? `<div class="text-muted text-sm" style="margin-top:2px">${h(s.description.slice(0,50))}${s.description.length>50?'…':''}</div>` : ''}
                </td>
                <td><span class="badge badge-${s.status} clickable-status" data-spec-status="${s.id}">${h(s.status)}</span></td>
                <td><span style="font-size:11px;background:#ECEFF1;padding:2px 8px;border-radius:4px">${h(s.category)}</span></td>
                <td><span class="badge badge-${s.priority}">${h(s.priority)}</span></td>
                <td>${h(s.owner)}</td>
                <td style="text-align:center">${s.taskIds.length}</td>
                <td class="text-muted text-sm">${fmt(s.updatedAt)}</td>
                <td>
                  <button class="btn btn-ghost btn-sm" data-spec-detail="${s.id}">詳細</button>
                  <button class="btn btn-ghost btn-sm" data-spec-edit="${s.id}">編集</button>
                  <button class="btn btn-danger btn-sm" data-spec-delete="${s.id}">削除</button>
                </td>
              </tr>`).join('') : '<tr><td colspan="9" class="empty-state">仕様書がありません</td></tr>'}
          </tbody>
        </table>
      </div>
    </div>`;
}

/* ═══════════════════════════════════════════════════════
   TASKS VIEW
═══════════════════════════════════════════════════════ */

let taskSprintFilter = '';

function renderTasks() {
  const store = SDD.store;
  const sprints = store.data.sprints;
  const activeSprint = store.getActiveSprint();
  if (!taskSprintFilter && activeSprint) taskSprintFilter = activeSprint.id;

  const filtered = taskSprintFilter
    ? store.data.tasks.filter(t => t.sprintId === taskSprintFilter)
    : store.data.tasks;

  const cols = [
    { key:'pending',     label:'未着手',   color:'var(--col-draft)' },
    { key:'in_progress', label:'進行中',   color:'var(--col-review)' },
    { key:'review',      label:'レビュー中', color:'#6A1B9A' },
    { key:'done',        label:'完了',     color:'var(--col-approved)' }
  ];

  const sprintSelect = `
    <select id="sprint-filter" style="border:1px solid var(--border);border-radius:6px;padding:5px 10px;font-size:12px;font-family:inherit;outline:none">
      <option value="">全スプリント</option>
      ${sprints.map(s => `<option value="${s.id}" ${taskSprintFilter===s.id?'selected':''}>${h(s.name)}</option>`).join('')}
    </select>`;

  return `
    <div style="margin-bottom:14px">${sprintSelect}</div>
    <div class="kanban-board">
      ${cols.map(col => {
        const tasks = filtered.filter(t => t.status === col.key);
        const sp = tasks.reduce((s,t) => s + t.sp, 0);
        return `
          <div class="kanban-col" data-status="${col.key}">
            <div class="kanban-col-header">
              <span>${col.label}</span>
              <span class="col-count">${tasks.length}</span>
            </div>
            <div class="kanban-col-body">
              <div style="font-size:11px;color:var(--muted);margin-bottom:8px">${sp} SP</div>
              ${tasks.map(t => renderTaskCard(t, col.key)).join('')}
            </div>
          </div>`;
      }).join('')}
    </div>`;
}

function renderTaskCard(t, currentStatus) {
  const nextStatus = {
    pending:'in_progress', in_progress:'review', review:'done', done:'done'
  };
  const prevStatus = {
    pending:'pending', in_progress:'pending', review:'in_progress', done:'review'
  };
  return `
    <div class="kanban-card" draggable="true" data-task-id="${h(t.id)}">
      <div class="card-id">${h(t.id)} ${t.specId?`· <span style="font-size:10px;color:var(--primary-mid)">${h(t.specId)}</span>`:''}
        ${t.issueNumber?`· #${t.issueNumber}`:''}</div>
      <div class="card-title">${h(t.title)}</div>
      <div class="card-meta">
        <span class="card-sp">${t.sp} SP</span>
        ${t.assignee?`<span style="font-size:11px;color:var(--muted)">${h(t.assignee)}</span>`:''}
      </div>
      <div style="display:flex;gap:4px;margin-top:8px;flex-wrap:wrap">
        ${currentStatus !== 'pending' ? `<button class="btn btn-ghost btn-sm" data-task-move="${t.id}" data-move-to="${prevStatus[currentStatus]}">← 戻す</button>` : ''}
        ${currentStatus !== 'done'    ? `<button class="btn btn-primary btn-sm" data-task-move="${t.id}" data-move-to="${nextStatus[currentStatus]}">次へ →</button>` : ''}
        <button class="btn btn-ghost btn-sm" data-task-edit="${t.id}">編集</button>
        <button class="btn btn-danger btn-sm" data-task-delete="${t.id}">削除</button>
      </div>
    </div>`;
}

/* ═══════════════════════════════════════════════════════
   SPRINTS VIEW
═══════════════════════════════════════════════════════ */

function renderSprints() {
  const store = SDD.store;
  const sprints = store.data.sprints;
  const active = store.getActiveSprint();

  const activeHtml = active ? (() => {
    const tasks = store.getSprintTasks(active.id);
    const total = tasks.reduce((s,t) => s + t.sp, 0);
    const done  = tasks.filter(t => t.status === 'done').reduce((s,t) => s + t.sp, 0);
    const pct   = total ? Math.round((done/total)*100) : 0;
    const rem   = store.getRemainingDays(active.endDate);

    // burndown table (simple: per-day if range available)
    const statusRows = [
      ['未着手', tasks.filter(t=>t.status==='pending')],
      ['進行中', tasks.filter(t=>t.status==='in_progress')],
      ['レビュー中', tasks.filter(t=>t.status==='review')],
      ['完了',   tasks.filter(t=>t.status==='done')]
    ];

    return `
      <div class="card section-gap">
        <div class="card-title">アクティブスプリント — ${h(active.name)}</div>
        <div class="sprint-active-card">
          <div style="font-size:13px;font-weight:600;margin-bottom:8px">${h(active.goal)}</div>
          <div style="display:flex;gap:24px;margin-bottom:10px;font-size:12px">
            <span>開始: ${fmt(active.startDate)}</span>
            <span>終了: ${fmt(active.endDate)}</span>
            ${rem !== null ? `<span style="color:var(--primary-mid);font-weight:700">残 ${rem} 日</span>` : ''}
          </div>
          <div style="display:flex;justify-content:space-between;font-size:13px;margin-bottom:6px">
            <span>${done} / ${total} SP 完了</span>
            <strong>${pct}%</strong>
          </div>
          <div class="progress-wrap" style="margin-bottom:16px"><div class="progress-bar blue" style="width:${pct}%;height:12px"></div></div>

          <div class="table-wrap">
            <table>
              <thead><tr><th>ステータス</th><th>タスク数</th><th>SP合計</th></tr></thead>
              <tbody>
                ${statusRows.map(([label, list]) => `
                  <tr><td>${label}</td><td>${list.length}</td><td>${list.reduce((s,t)=>s+t.sp,0)}</td></tr>
                `).join('')}
              </tbody>
            </table>
          </div>
          <div style="margin-top:14px">
            <button class="btn btn-danger" data-sprint-complete="${active.id}">スプリントを終了する</button>
          </div>
        </div>
      </div>`;
  })() : '<div class="card section-gap"><div class="empty-state">アクティブなスプリントがありません</div></div>';

  const historyHtml = sprints.filter(s => s.status !== 'active').length ? `
    <div class="card">
      <div class="card-title">スプリント履歴</div>
      <div class="table-wrap">
        <table>
          <thead><tr><th>ID</th><th>名前</th><th>ゴール</th><th>期間</th><th>ステータス</th><th>操作</th></tr></thead>
          <tbody>
            ${sprints.filter(s => s.status !== 'active').map(s => {
              const tasks = store.getSprintTasks(s.id);
              const total = tasks.reduce((sum,t) => sum + t.sp, 0);
              const done  = tasks.filter(t => t.status === 'done').reduce((sum,t) => sum + t.sp, 0);
              return `<tr>
                <td style="font-family:monospace;font-size:11px">${h(s.id)}</td>
                <td>${h(s.name)}</td>
                <td style="max-width:160px;font-size:12px">${h(s.goal)}</td>
                <td style="font-size:11px">${fmt(s.startDate)} ~ ${fmt(s.endDate)}</td>
                <td>${statusBadge(s.status)}</td>
                <td>
                  ${s.status==='planned' ? `<button class="btn btn-primary btn-sm" data-sprint-activate="${s.id}">開始</button>` : `<span style="font-size:11px;color:var(--muted)">${done}/${total}SP</span>`}
                </td>
              </tr>`;
            }).join('')}
          </tbody>
        </table>
      </div>
    </div>` : '';

  return activeHtml + historyHtml;
}

/* ═══════════════════════════════════════════════════════
   RETROS VIEW
═══════════════════════════════════════════════════════ */

let currentRetroId = null;

function renderRetros() {
  const store = SDD.store;
  const retros = store.data.retros;

  if (currentRetroId) {
    const retro = retros.find(r => r.id === currentRetroId);
    if (retro) return renderRetroDetail(retro);
  }

  return `
    <div class="card">
      <div class="card-title">ふりかえり一覧</div>
      ${retros.length ? `
        <div class="table-wrap">
          <table>
            <thead><tr><th>ID</th><th>スプリント</th><th>日付</th><th>ファシリテーター</th><th>完了SP</th><th>操作</th></tr></thead>
            <tbody>
              ${retros.map(r => {
                const sprint = store.data.sprints.find(s => s.id === r.sprintId);
                return `<tr>
                  <td style="font-family:monospace;font-size:11px">${h(r.id)}</td>
                  <td>${sprint ? h(sprint.name) : h(r.sprintId)}</td>
                  <td>${fmt(r.date)}</td>
                  <td>${h(r.facilitator)}</td>
                  <td>${r.metrics.completedSP} / ${r.metrics.plannedSP} SP</td>
                  <td>
                    <button class="btn btn-primary btn-sm" data-retro-open="${r.id}">詳細・編集</button>
                  </td>
                </tr>`;
              }).join('')}
            </tbody>
          </table>
        </div>` : '<div class="empty-state">ふりかえりがありません</div>'}
    </div>`;
}

function renderRetroDetail(retro) {
  const store = SDD.store;
  const sprint = store.data.sprints.find(s => s.id === retro.sprintId);
  const m = retro.metrics;

  return `
    <div style="margin-bottom:12px">
      <button class="btn btn-ghost" onclick="SDD.app.setCurrentRetro(null)">← 一覧に戻る</button>
    </div>
    <div class="card section-gap">
      <div class="card-title">${h(retro.id)} — ${sprint ? h(sprint.name) : ''} ふりかえり</div>
      <div style="font-size:12px;color:var(--muted);margin-bottom:14px">
        日付: ${fmt(retro.date)} ／ ファシリテーター: ${h(retro.facilitator)}
      </div>

      <div class="card-title" style="margin-bottom:10px">メトリクス</div>
      <div class="metrics-grid section-gap">
        <div class="metric-item"><div class="metric-val">${m.completedSP}/${m.plannedSP}</div><div class="metric-label">完了SP / 計画SP</div></div>
        <div class="metric-item"><div class="metric-val">${m.bugCount}</div><div class="metric-label">バグ件数</div></div>
        <div class="metric-item"><div class="metric-val">${m.avgReviewMinutes}分</div><div class="metric-label">平均レビュー時間</div></div>
        <div class="metric-item"><div class="metric-val">${m.specFirstRate}%</div><div class="metric-label">仕様先行率</div></div>
        <div class="metric-item"><div class="metric-val">${m.ciPassRate}%</div><div class="metric-label">CI パス率</div></div>
      </div>

      <div class="card-title" style="margin-bottom:10px">KPT</div>
      <div class="kpt-grid section-gap">
        <div class="kpt-col keep">
          <h4>Keep</h4>
          ${retro.keeps.map(k => `<div class="kpt-item">${h(k.text)}</div>`).join('') || '<div class="text-muted text-sm">なし</div>'}
        </div>
        <div class="kpt-col problem">
          <h4>Problem</h4>
          ${retro.problems.map(p => `<div class="kpt-item">${h(p.text)}</div>`).join('') || '<div class="text-muted text-sm">なし</div>'}
        </div>
        <div class="kpt-col try">
          <h4>Try</h4>
          ${retro.tries.map(t => `<div class="kpt-item">${h(t.text)}</div>`).join('') || '<div class="text-muted text-sm">なし</div>'}
        </div>
      </div>

      <div class="card-title" style="margin-bottom:10px">アクションアイテム</div>
      <div id="action-items-list">
        ${retro.actionItems.length ? retro.actionItems.map(a => `
          <div class="action-item-row">
            <input type="checkbox" ${a.done?'checked':''} data-action-toggle="${retro.id}" data-action-id="${a.id}">
            <div>
              <span class="${a.done?'action-item-done':''}">${h(a.text)}</span>
              <span class="text-muted text-sm"> — ${h(a.assignee)} ${a.dueDate?'(期限: '+fmt(a.dueDate)+')':''}</span>
            </div>
          </div>`).join('') : '<div class="text-muted text-sm">アクションアイテムなし</div>'}
      </div>

      <div style="margin-top:14px">
        <button class="btn btn-primary" data-retro-edit="${retro.id}">編集する</button>
      </div>
    </div>`;
}

/* ═══════════════════════════════════════════════════════
   ISSUES VIEW
═══════════════════════════════════════════════════════ */

let issueFilter = 'all';

function renderIssues() {
  const issues = SDD.store.data.issues.filter(i =>
    issueFilter === 'all' || i.status === issueFilter
  );

  return `
    <div class="card">
      <div class="filter-tabs">
        ${['all','open','in_progress','resolved'].map(f =>
          `<button class="filter-tab${issueFilter===f?' active':''}" data-issue-filter="${f}">${
            f==='all'?'すべて':f==='in_progress'?'対応中':f==='resolved'?'解決済み':f
          }</button>`
        ).join('')}
      </div>
      <div class="table-wrap">
        <table>
          <thead><tr>
            <th>ID</th><th>タイトル</th><th>タイプ</th><th>優先度</th><th>ステータス</th>
            <th>Spec ID</th><th>Task ID</th><th>報告者</th><th>作成日</th><th>操作</th>
          </tr></thead>
          <tbody>
            ${issues.length ? issues.map(i => `
              <tr>
                <td style="font-family:monospace;font-size:11px">${h(i.id)}</td>
                <td style="max-width:180px">
                  <div style="font-weight:600">${h(i.title)}</div>
                  ${i.description ? `<div class="text-muted text-sm" style="margin-top:2px">${h(i.description.slice(0,50))}${i.description.length>50?'…':''}</div>` : ''}
                </td>
                <td><span class="badge badge-${i.type}">${i.type}</span></td>
                <td><span class="badge badge-${i.priority}">${i.priority}</span></td>
                <td>${statusBadge(i.status)}</td>
                <td style="font-size:11px">${h(i.specId)}</td>
                <td style="font-size:11px">${h(i.taskId)}</td>
                <td>${h(i.reporter)}</td>
                <td class="text-muted text-sm">${fmt(i.createdAt)}</td>
                <td>
                  ${i.status!=='resolved' ? `<button class="btn btn-success btn-sm" data-issue-resolve="${i.id}">解決</button>` : ''}
                  <button class="btn btn-ghost btn-sm" data-issue-edit="${i.id}">編集</button>
                  <button class="btn btn-danger btn-sm" data-issue-delete="${i.id}">削除</button>
                </td>
              </tr>`).join('') : '<tr><td colspan="10" class="empty-state">課題がありません</td></tr>'}
          </tbody>
        </table>
      </div>
    </div>`;
}

/* ═══════════════════════════════════════════════════════
   SETTINGS VIEW
═══════════════════════════════════════════════════════ */

function renderSettings() {
  const p  = SDD.store.data.project;
  const fs = SDD.fs;
  const connected = !!fs.dirHandle;

  const syncCard = fs.supported ? `
    <div class="card" style="margin-bottom:20px">
      <div class="card-title">📁 プロジェクトフォルダ同期</div>
      ${connected ? `
        <div style="background:#E8F5E9;border-radius:8px;padding:12px 14px;margin-bottom:16px;display:flex;align-items:center;gap:10px">
          <span style="font-size:18px">🟢</span>
          <div>
            <div style="font-weight:700;color:#2E7D32;font-size:13px">接続済み</div>
            <div class="text-muted text-sm" style="margin-top:2px;font-family:monospace">${h(fs.dirHandle.name)}</div>
          </div>
        </div>
        <div style="display:grid;grid-template-columns:1fr 1fr;gap:16px;margin-bottom:16px">
          <div style="background:#F5F7FA;border-radius:8px;padding:14px">
            <div style="font-weight:700;font-size:12px;margin-bottom:6px">↑ プッシュ（自動）</div>
            <div class="text-muted text-sm" style="margin-bottom:10px">タスク・仕様書を変更するたびに自動でファイルへ書き込みます。</div>
            <div style="font-family:monospace;font-size:10px;color:var(--muted);background:#fff;border-radius:4px;padding:6px 8px;margin-bottom:10px">
              tasks/pm-data.json<br>tasks/task-list.md
            </div>
            <button class="btn btn-primary btn-sm" onclick="SDD.fs.autoSync()">今すぐ同期</button>
          </div>
          <div style="background:#F5F7FA;border-radius:8px;padding:14px">
            <div style="font-weight:700;font-size:12px;margin-bottom:6px">↓ プル（手動）</div>
            <div class="text-muted text-sm" style="margin-bottom:10px">/pm-sync スキル実行後、更新されたファイルをブラウザに取り込みます。</div>
            <div style="font-family:monospace;font-size:10px;color:var(--muted);background:#fff;border-radius:4px;padding:6px 8px;margin-bottom:10px">
              tasks/pm-data.json → ブラウザ
            </div>
            <button class="btn btn-warning btn-sm" onclick="SDD.fs.pullFromProject()">プロジェクトから取得</button>
          </div>
        </div>
        <button class="btn btn-ghost btn-sm" onclick="SDD.fs.disconnect()">接続を解除</button>` : `
        <div style="background:#F5F7FA;border-radius:8px;padding:14px;margin-bottom:14px;font-size:12px;color:var(--muted);line-height:1.7">
          プロジェクトフォルダを接続すると、タスク・仕様書・スプリントの変更が
          <strong>自動で</strong> ファイルへ書き込まれます。<br>
          Claude Code の <code>/pm-sync</code> スキルと組み合わせると双方向同期が可能です。
        </div>
        <button class="btn btn-primary" onclick="SDD.fs.connect()">📁 プロジェクトフォルダを接続</button>
        <div class="text-muted text-sm" style="margin-top:8px">
          ※ Chrome または Edge が必要です（File System Access API）
        </div>`}
    </div>` : `
    <div class="card" style="margin-bottom:20px">
      <div class="card-title">📁 プロジェクトフォルダ同期</div>
      <div style="background:#FFF3E0;border-radius:8px;padding:12px 14px;color:#E65100;font-size:12px">
        ⚠️ このブラウザは File System Access API に対応していません。<br>
        Chrome または Edge をご利用ください。
      </div>
    </div>`;

  return `
    ${syncCard}
    <div class="grid-2" style="gap:20px">
      <div class="card">
        <div class="card-title">プロジェクト設定</div>
        <form id="project-form">
          <div class="form-group">
            <label>プロジェクト名</label>
            <input type="text" id="proj-name" value="${h(p.name)}">
          </div>
          <div class="form-group">
            <label>説明</label>
            <textarea id="proj-desc">${h(p.description)}</textarea>
          </div>
          <div class="form-group">
            <label>メンバー（カンマ区切り @handle）</label>
            <input type="text" id="proj-members" value="${h((p.members||[]).join(', '))}">
          </div>
          <div class="form-group">
            <label>仕様書パス</label>
            <input type="text" id="proj-spec-path" value="${h(p.specPath)}">
          </div>
          <div class="form-group">
            <label>憲章ファイルパス</label>
            <input type="text" id="proj-const-path" value="${h(p.constitutionPath)}">
          </div>
          <button type="submit" class="btn btn-primary">保存</button>
        </form>
      </div>
      <div class="card">
        <div class="card-title">データ管理</div>
        <div style="display:flex;flex-direction:column;gap:12px">
          <div>
            <div style="font-weight:600;margin-bottom:6px;font-size:13px">エクスポート</div>
            <div class="text-muted text-sm" style="margin-bottom:8px">全データを JSON ファイルとしてダウンロードします。</div>
            <button class="btn btn-primary" id="btn-export">JSON をダウンロード</button>
          </div>
          <hr style="border:none;border-top:1px solid var(--border)">
          <div>
            <div style="font-weight:600;margin-bottom:6px;font-size:13px">インポート</div>
            <div class="text-muted text-sm" style="margin-bottom:8px">JSON ファイルを読み込みます。現在のデータは上書きされます。</div>
            <input type="file" id="import-file" accept=".json" style="display:none">
            <button class="btn btn-warning" id="btn-import-trigger">JSON を読み込む</button>
          </div>
          <hr style="border:none;border-top:1px solid var(--border)">
          <div>
            <div style="font-weight:600;margin-bottom:6px;font-size:13px;color:var(--col-high)">データリセット</div>
            <div class="text-muted text-sm" style="margin-bottom:8px">全データをデモデータに戻します。この操作は元に戻せません。</div>
            <button class="btn btn-danger" id="btn-reset">データをリセット</button>
          </div>
        </div>
      </div>
    </div>`;
}

/* ═══════════════════════════════════════════════════════
   VIEW EVENT BINDING
═══════════════════════════════════════════════════════ */

function bindViewEvents(view) {
  const content = $('content');

  if (view === 'specs') {
    content.querySelectorAll('.filter-tab').forEach(btn => {
      btn.addEventListener('click', () => {
        specFilter = btn.dataset.filter;
        renderView('specs');
      });
    });
    content.addEventListener('click', e => {
      const t = e.target;
      if (t.dataset.specEdit)   SDD.app.openModal('spec', 'edit', t.dataset.specEdit);
      if (t.dataset.specDelete) {
        if (confirm(`${t.dataset.specDelete} を削除しますか？`)) {
          SDD.store.deleteSpec(t.dataset.specDelete);
          renderView('specs');
        }
      }
      if (t.dataset.specDetail) SDD.app.openModal('spec', 'detail', t.dataset.specDetail);
      if (t.dataset.specStatus) {
        const id = t.dataset.specStatus;
        const s = SDD.store.data.specs.find(x => x.id === id);
        if (!s) return;
        const cycle = ['draft','review','approved','deprecated'];
        const next = cycle[(cycle.indexOf(s.status) + 1) % cycle.length];
        SDD.store.updateSpec(id, {status: next});
        renderView('specs');
      }
    });
  }

  if (view === 'tasks') {
    const sel = $('sprint-filter');
    if (sel) sel.addEventListener('change', () => {
      taskSprintFilter = sel.value;
      renderView('tasks');
    });
    content.addEventListener('click', e => {
      const t = e.target;
      if (t.dataset.taskMove) {
        SDD.store.updateTask(t.dataset.taskMove, {status: t.dataset.moveTo});
        renderView('tasks');
      }
      if (t.dataset.taskEdit)   SDD.app.openModal('task', 'edit', t.dataset.taskEdit);
      if (t.dataset.taskDelete) {
        if (confirm(`${t.dataset.taskDelete} を削除しますか？`)) {
          SDD.store.deleteTask(t.dataset.taskDelete);
          renderView('tasks');
        }
      }
    });

    // ── drag & drop ──
    let draggedTaskId = null;

    content.addEventListener('dragstart', e => {
      const card = e.target.closest('.kanban-card');
      if (!card) return;
      draggedTaskId = card.dataset.taskId;
      card.classList.add('dragging');
      e.dataTransfer.effectAllowed = 'move';
      e.dataTransfer.setData('text/plain', draggedTaskId);
    });

    content.addEventListener('dragend', e => {
      content.querySelectorAll('.kanban-card.dragging').forEach(c => c.classList.remove('dragging'));
      content.querySelectorAll('.kanban-col.drag-over').forEach(c => c.classList.remove('drag-over'));
    });

    content.addEventListener('dragover', e => {
      e.preventDefault();
      e.dataTransfer.dropEffect = 'move';
      const col = e.target.closest('[data-status]');
      content.querySelectorAll('.kanban-col.drag-over').forEach(c => c.classList.remove('drag-over'));
      if (col) col.classList.add('drag-over');
    });

    content.addEventListener('dragleave', e => {
      const col = e.target.closest('.kanban-col');
      if (col && !col.contains(e.relatedTarget)) col.classList.remove('drag-over');
    });

    content.addEventListener('drop', e => {
      e.preventDefault();
      const col = e.target.closest('[data-status]');
      content.querySelectorAll('.kanban-col.drag-over').forEach(c => c.classList.remove('drag-over'));
      if (!col || !draggedTaskId) return;
      const newStatus = col.dataset.status;
      if (newStatus) {
        SDD.store.updateTask(draggedTaskId, {status: newStatus});
        renderView('tasks');
      }
      draggedTaskId = null;
    });
  }

  if (view === 'sprints') {
    content.addEventListener('click', e => {
      const t = e.target;
      if (t.dataset.sprintComplete) {
        if (confirm('スプリントを終了しますか？')) {
          SDD.store.completeSprint(t.dataset.sprintComplete);
          renderView('sprints');
        }
      }
      if (t.dataset.sprintActivate) {
        if (confirm('このスプリントを開始しますか？（現在のアクティブスプリントは完了になります）')) {
          SDD.store.activateSprint(t.dataset.sprintActivate);
          renderView('sprints');
        }
      }
    });
  }

  if (view === 'retros') {
    content.addEventListener('click', e => {
      const t = e.target;
      if (t.dataset.retroOpen) {
        currentRetroId = t.dataset.retroOpen;
        renderView('retros');
      }
      if (t.dataset.retroEdit) SDD.app.openModal('retro', 'edit', t.dataset.retroEdit);
      if (t.dataset.actionToggle) {
        const retro = SDD.store.data.retros.find(r => r.id === t.dataset.actionToggle);
        if (retro) {
          const item = retro.actionItems.find(a => a.id === t.dataset.actionId);
          if (item) { item.done = t.checked; SDD.store.save(); }
        }
      }
    });
    content.querySelector('[onclick*="setCurrentRetro"]')?.addEventListener('click', () => {});
  }

  if (view === 'issues') {
    content.querySelectorAll('[data-issue-filter]').forEach(btn => {
      btn.addEventListener('click', () => {
        issueFilter = btn.dataset.issueFilter;
        renderView('issues');
      });
    });
    content.addEventListener('click', e => {
      const t = e.target;
      if (t.dataset.issueResolve) {
        SDD.store.updateIssue(t.dataset.issueResolve, {status:'resolved'});
        renderView('issues');
      }
      if (t.dataset.issueEdit)   SDD.app.openModal('issue', 'edit', t.dataset.issueEdit);
      if (t.dataset.issueDelete) {
        if (confirm(`${t.dataset.issueDelete} を削除しますか？`)) {
          SDD.store.deleteIssue(t.dataset.issueDelete);
          renderView('issues');
        }
      }
    });
  }

  if (view === 'settings') {
    const form = $('project-form');
    if (form) form.addEventListener('submit', e => {
      e.preventDefault();
      SDD.store.updateProject({
        name: $('proj-name').value,
        description: $('proj-desc').value,
        members: $('proj-members').value.split(',').map(s => s.trim()).filter(Boolean),
        specPath: $('proj-spec-path').value,
        constitutionPath: $('proj-const-path').value
      });
      $('sidebar-project-name').textContent = $('proj-name').value;
      alert('設定を保存しました');
    });

    $('btn-export')?.addEventListener('click', () => {
      const blob = new Blob([SDD.store.exportJSON()], {type:'application/json'});
      const a = document.createElement('a');
      a.href = URL.createObjectURL(blob);
      a.download = `sdd-pm-${SDD.store.today()}.json`;
      a.click();
    });

    $('btn-import-trigger')?.addEventListener('click', () => $('import-file').click());

    $('import-file')?.addEventListener('change', e => {
      const file = e.target.files[0];
      if (!file) return;
      const reader = new FileReader();
      reader.onload = ev => {
        try {
          SDD.store.importJSON(ev.target.result);
          alert('インポートが完了しました');
          navigate('dashboard');
        } catch(err) {
          alert('JSON の読み込みに失敗しました: ' + err.message);
        }
      };
      reader.readAsText(file);
    });

    $('btn-reset')?.addEventListener('click', () => {
      if (confirm('本当にデータをリセットしますか？この操作は元に戻せません。')) {
        SDD.store.reset();
        navigate('dashboard');
      }
    });
  }
}

/* ═══════════════════════════════════════════════════════
   MODALS
═══════════════════════════════════════════════════════ */

function buildMembersOptions(selected) {
  const members = SDD.store.data.project.members || [];
  return members.map(m =>
    `<option value="${h(m)}" ${m===selected?'selected':''}>${h(m)}</option>`
  ).join('');
}

function buildSpecOptions(selected) {
  return SDD.store.data.specs.map(s =>
    `<option value="${h(s.id)}" ${s.id===selected?'selected':''}>${h(s.id)} ${h(s.title)}</option>`
  ).join('');
}

function buildSprintOptions(selected) {
  return SDD.store.data.sprints.map(s =>
    `<option value="${h(s.id)}" ${s.id===selected?'selected':''}>${h(s.name)}</option>`
  ).join('');
}

const MODAL_CONFIGS = {
  spec: {
    title: (mode, data) => mode === 'new' ? '仕様書を追加' : mode === 'edit' ? '仕様書を編集' : `${data.id} 詳細`,
    body: (mode, data) => {
      if (mode === 'detail') {
        return `
          <div style="font-size:13px">
            <div style="margin-bottom:12px"><strong>ID:</strong> ${h(data.id)}</div>
            <div style="margin-bottom:12px"><strong>タイトル:</strong> ${h(data.title)}</div>
            <div style="margin-bottom:12px"><strong>ステータス:</strong> ${statusBadge(data.status)}</div>
            <div style="margin-bottom:12px"><strong>カテゴリ:</strong> ${h(data.category)}</div>
            <div style="margin-bottom:12px"><strong>優先度:</strong> <span class="badge badge-${data.priority}">${data.priority}</span></div>
            <div style="margin-bottom:12px"><strong>オーナー:</strong> ${h(data.owner)}</div>
            <div style="margin-bottom:12px"><strong>ファイル:</strong> <code style="font-size:11px">${h(data.file)}</code></div>
            <div style="margin-bottom:12px"><strong>説明:</strong><br><div style="margin-top:4px;padding:8px;background:#F5F7FA;border-radius:6px;font-size:12px">${h(data.description||'—')}</div></div>
            <div><strong>受入基準:</strong>
              <ul style="margin-top:6px;padding-left:20px;font-size:12px">
                ${(data.acceptanceCriteria||[]).map(c => `<li style="margin-bottom:4px">${h(c)}</li>`).join('') || '<li style="color:var(--muted)">未設定</li>'}
              </ul>
            </div>
          </div>`;
      }
      return `
        <div class="form-row">
          <div class="form-group"><label>Spec ID</label>
            <input type="text" id="m-spec-id" value="${h(data.id||'')}" placeholder="SPEC-XXX-001" ${mode==='edit'?'readonly':''}>
          </div>
          <div class="form-group"><label>カテゴリ</label>
            <input type="text" id="m-spec-category" value="${h(data.category||'')}" placeholder="AUTH, UI ...">
          </div>
        </div>
        <div class="form-group"><label>タイトル</label>
          <input type="text" id="m-spec-title" value="${h(data.title||'')}" required>
        </div>
        <div class="form-row">
          <div class="form-group"><label>ステータス</label>
            <select id="m-spec-status">
              ${['draft','review','approved','deprecated'].map(s =>
                `<option value="${s}" ${(data.status||'draft')===s?'selected':''}>${s}</option>`).join('')}
            </select>
          </div>
          <div class="form-group"><label>優先度</label>
            <select id="m-spec-priority">
              ${['Must','Should','Could'].map(p =>
                `<option value="${p}" ${(data.priority||'Should')===p?'selected':''}>${p}</option>`).join('')}
            </select>
          </div>
        </div>
        <div class="form-group"><label>オーナー</label>
          <select id="m-spec-owner">
            <option value="">--</option>
            ${buildMembersOptions(data.owner||'')}
          </select>
        </div>
        <div class="form-group"><label>ファイルパス</label>
          <input type="text" id="m-spec-file" value="${h(data.file||'')}" placeholder="docs/spec/...">
        </div>
        <div class="form-group"><label>説明</label>
          <textarea id="m-spec-desc">${h(data.description||'')}</textarea>
        </div>
        <div class="form-group"><label>受入基準（1行1条件）</label>
          <textarea id="m-spec-ac" style="min-height:100px">${(data.acceptanceCriteria||[]).join('\n')}</textarea>
        </div>`;
    },
    save: (mode, id) => {
      const fields = {
        id:                 $('m-spec-id')?.value.trim(),
        category:           $('m-spec-category')?.value.trim(),
        title:              $('m-spec-title')?.value.trim(),
        status:             $('m-spec-status')?.value,
        priority:           $('m-spec-priority')?.value,
        owner:              $('m-spec-owner')?.value,
        file:               $('m-spec-file')?.value.trim(),
        description:        $('m-spec-desc')?.value.trim(),
        acceptanceCriteria: ($('m-spec-ac')?.value||'').split('\n').map(s=>s.trim()).filter(Boolean)
      };
      if (!fields.title) return alert('タイトルは必須です');
      if (mode === 'new') SDD.store.createSpec(fields);
      else SDD.store.updateSpec(id, fields);
      return true;
    }
  },

  task: {
    title: (mode) => mode === 'new' ? 'タスクを追加' : 'タスクを編集',
    body: (mode, data) => `
      <div class="form-group"><label>タイトル</label>
        <input type="text" id="m-task-title" value="${h(data.title||'')}" required>
      </div>
      <div class="form-row">
        <div class="form-group"><label>SP（ストーリーポイント）</label>
          <input type="number" id="m-task-sp" value="${data.sp||1}" min="1" max="99">
        </div>
        <div class="form-group"><label>ステータス</label>
          <select id="m-task-status">
            ${['pending','in_progress','review','done'].map(s =>
              `<option value="${s}" ${(data.status||'pending')===s?'selected':''}>${s}</option>`).join('')}
          </select>
        </div>
      </div>
      <div class="form-row">
        <div class="form-group"><label>担当者</label>
          <select id="m-task-assignee">
            <option value="">--</option>
            ${buildMembersOptions(data.assignee||'')}
          </select>
        </div>
        <div class="form-group"><label>スプリント</label>
          <select id="m-task-sprint">
            <option value="">--</option>
            ${buildSprintOptions(data.sprintId||'')}
          </select>
        </div>
      </div>
      <div class="form-group"><label>紐付き仕様書</label>
        <select id="m-task-spec">
          <option value="">--</option>
          ${buildSpecOptions(data.specId||'')}
        </select>
      </div>
      <div class="form-row">
        <div class="form-group"><label>Issue 番号</label>
          <input type="number" id="m-task-issue" value="${data.issueNumber||''}" placeholder="42">
        </div>
        <div class="form-group"><label>PR 番号</label>
          <input type="number" id="m-task-pr" value="${data.pr||''}" placeholder="43">
        </div>
      </div>
      <div class="form-group"><label>ブランチ名</label>
        <input type="text" id="m-task-branch" value="${h(data.branch||'')}" placeholder="feat/42-xxx">
      </div>
      <div class="form-group"><label>メモ</label>
        <textarea id="m-task-notes">${h(data.notes||'')}</textarea>
      </div>`,
    save: (mode, id) => {
      const fields = {
        title:       $('m-task-title')?.value.trim(),
        sp:          $('m-task-sp')?.value,
        status:      $('m-task-status')?.value,
        assignee:    $('m-task-assignee')?.value,
        sprintId:    $('m-task-sprint')?.value,
        specId:      $('m-task-spec')?.value,
        issueNumber: $('m-task-issue')?.value || null,
        pr:          $('m-task-pr')?.value || null,
        branch:      $('m-task-branch')?.value.trim(),
        notes:       $('m-task-notes')?.value.trim()
      };
      if (!fields.title) return alert('タイトルは必須です');
      if (mode === 'new') SDD.store.createTask(fields);
      else SDD.store.updateTask(id, fields);
      return true;
    }
  },

  sprint: {
    title: () => 'スプリントを作成',
    body: (mode, data) => `
      <div class="form-group"><label>スプリント名</label>
        <input type="text" id="m-sp-name" value="${h(data.name||'')}" placeholder="Sprint N">
      </div>
      <div class="form-group"><label>ゴール</label>
        <textarea id="m-sp-goal">${h(data.goal||'')}</textarea>
      </div>
      <div class="form-row">
        <div class="form-group"><label>開始日</label>
          <input type="date" id="m-sp-start" value="${h(data.startDate||SDD.store.today())}">
        </div>
        <div class="form-group"><label>終了日</label>
          <input type="date" id="m-sp-end" value="${h(data.endDate||'')}">
        </div>
      </div>`,
    save: () => {
      const fields = {
        name:      $('m-sp-name').value.trim(),
        goal:      $('m-sp-goal').value.trim(),
        startDate: $('m-sp-start').value,
        endDate:   $('m-sp-end').value
      };
      if (!fields.name) return alert('スプリント名は必須です');
      SDD.store.createSprint(fields);
      return true;
    }
  },

  retro: {
    title: (mode) => mode === 'new' ? 'ふりかえりを作成' : 'ふりかえりを編集',
    body: (mode, data) => `
      <div class="form-row">
        <div class="form-group"><label>スプリント</label>
          <select id="m-retro-sprint">
            <option value="">--</option>
            ${buildSprintOptions(data.sprintId||'')}
          </select>
        </div>
        <div class="form-group"><label>日付</label>
          <input type="date" id="m-retro-date" value="${h(data.date||SDD.store.today())}">
        </div>
      </div>
      <div class="form-group"><label>ファシリテーター</label>
        <select id="m-retro-fac">
          <option value="">--</option>
          ${buildMembersOptions(data.facilitator||'')}
        </select>
      </div>
      <div style="font-weight:700;font-size:13px;color:var(--primary);margin:12px 0 8px">KPT</div>
      <div class="form-group"><label>Keep（改行区切り）</label>
        <textarea id="m-retro-keeps">${(data.keeps||[]).map(k=>k.text).join('\n')}</textarea>
      </div>
      <div class="form-group"><label>Problem（改行区切り）</label>
        <textarea id="m-retro-problems">${(data.problems||[]).map(p=>p.text).join('\n')}</textarea>
      </div>
      <div class="form-group"><label>Try（改行区切り）</label>
        <textarea id="m-retro-tries">${(data.tries||[]).map(t=>t.text).join('\n')}</textarea>
      </div>
      <div style="font-weight:700;font-size:13px;color:var(--primary);margin:12px 0 8px">メトリクス</div>
      <div class="form-row">
        <div class="form-group"><label>計画SP</label>
          <input type="number" id="m-retro-planned" value="${(data.metrics||{}).plannedSP||0}">
        </div>
        <div class="form-group"><label>完了SP</label>
          <input type="number" id="m-retro-completed" value="${(data.metrics||{}).completedSP||0}">
        </div>
      </div>
      <div class="form-row">
        <div class="form-group"><label>バグ件数</label>
          <input type="number" id="m-retro-bugs" value="${(data.metrics||{}).bugCount||0}">
        </div>
        <div class="form-group"><label>平均レビュー時間（分）</label>
          <input type="number" id="m-retro-review" value="${(data.metrics||{}).avgReviewMinutes||0}">
        </div>
      </div>
      <div class="form-row">
        <div class="form-group"><label>仕様先行率(%)</label>
          <input type="number" id="m-retro-specfirst" value="${(data.metrics||{}).specFirstRate||0}">
        </div>
        <div class="form-group"><label>CI パス率(%)</label>
          <input type="number" id="m-retro-ci" value="${(data.metrics||{}).ciPassRate||0}">
        </div>
      </div>
      <div style="font-weight:700;font-size:13px;color:var(--primary);margin:12px 0 8px">アクションアイテム</div>
      <div class="form-group"><label>アクション（1行: テキスト | @担当者 | 期限YYYY-MM-DD）</label>
        <textarea id="m-retro-actions" placeholder="リリース前チェックリスト整備 | @alice | 2024-01-26&#10;PR テンプレート更新 | @bob |">${
          (data.actionItems||[]).map(a => `${a.text} | ${a.assignee} | ${a.dueDate||''}`).join('\n')
        }</textarea>
      </div>`,
    save: (mode, id) => {
      const parseKPT = val => (val||'').split('\n').map((t,i)=>({text:t.trim(),id:`k${i}`})).filter(x=>x.text);
      const keeps    = parseKPT($('m-retro-keeps')?.value).map((k,i)=>({...k,id:`k${i}`}));
      const problems = parseKPT($('m-retro-problems')?.value).map((p,i)=>({...p,id:`p${i}`}));
      const tries    = parseKPT($('m-retro-tries')?.value).map((t,i)=>({...t,id:`t${i}`}));
      const actionItems = ($('m-retro-actions')?.value||'').split('\n')
        .map((line,i) => { const [text,assignee,dueDate] = line.split('|').map(s=>s.trim()); return text ? {id:`a${i}`,text,assignee:assignee||'',dueDate:dueDate||'',done:false} : null; })
        .filter(Boolean);

      const fields = {
        sprintId:    $('m-retro-sprint')?.value,
        date:        $('m-retro-date')?.value,
        facilitator: $('m-retro-fac')?.value,
        keeps, problems, tries, actionItems,
        metrics: {
          plannedSP:        Number($('m-retro-planned')?.value)||0,
          completedSP:      Number($('m-retro-completed')?.value)||0,
          bugCount:         Number($('m-retro-bugs')?.value)||0,
          avgReviewMinutes: Number($('m-retro-review')?.value)||0,
          specFirstRate:    Number($('m-retro-specfirst')?.value)||0,
          ciPassRate:       Number($('m-retro-ci')?.value)||0
        }
      };
      if (mode === 'new') SDD.store.createRetro(fields);
      else SDD.store.updateRetro(id, fields);
      return true;
    }
  },

  issue: {
    title: (mode) => mode === 'new' ? '課題を追加' : '課題を編集',
    body: (mode, data) => `
      <div class="form-group"><label>タイトル</label>
        <input type="text" id="m-issue-title" value="${h(data.title||'')}" required>
      </div>
      <div class="form-row">
        <div class="form-group"><label>タイプ</label>
          <select id="m-issue-type">
            ${['bug','blocker','question','risk'].map(t =>
              `<option value="${t}" ${(data.type||'question')===t?'selected':''}>${t}</option>`).join('')}
          </select>
        </div>
        <div class="form-group"><label>優先度</label>
          <select id="m-issue-priority">
            ${['High','Medium','Low'].map(p =>
              `<option value="${p}" ${(data.priority||'Medium')===p?'selected':''}>${p}</option>`).join('')}
          </select>
        </div>
      </div>
      <div class="form-row">
        <div class="form-group"><label>Spec ID</label>
          <select id="m-issue-spec">
            <option value="">--</option>
            ${buildSpecOptions(data.specId||'')}
          </select>
        </div>
        <div class="form-group"><label>Task ID（手入力）</label>
          <input type="text" id="m-issue-task" value="${h(data.taskId||'')}" placeholder="TASK-001">
        </div>
      </div>
      <div class="form-group"><label>報告者</label>
        <select id="m-issue-reporter">
          <option value="">--</option>
          ${buildMembersOptions(data.reporter||'')}
        </select>
      </div>
      <div class="form-group"><label>説明</label>
        <textarea id="m-issue-desc">${h(data.description||'')}</textarea>
      </div>
      ${mode==='edit' && data.status==='resolved' ? `
        <div class="form-group"><label>解決内容</label>
          <textarea id="m-issue-resolution">${h(data.resolution||'')}</textarea>
        </div>` : ''}`,
    save: (mode, id) => {
      const fields = {
        title:       $('m-issue-title')?.value.trim(),
        type:        $('m-issue-type')?.value,
        priority:    $('m-issue-priority')?.value,
        specId:      $('m-issue-spec')?.value,
        taskId:      $('m-issue-task')?.value.trim(),
        reporter:    $('m-issue-reporter')?.value,
        description: $('m-issue-desc')?.value.trim(),
        resolution:  $('m-issue-resolution')?.value?.trim() || undefined
      };
      if (!fields.title) return alert('タイトルは必須です');
      if (mode === 'new') SDD.store.createIssue(fields);
      else SDD.store.updateIssue(id, fields);
      return true;
    }
  }
};

/* ═══════════════════════════════════════════════════════
   APP CONTROLLER
═══════════════════════════════════════════════════════ */

SDD.app = {
  currentView: 'dashboard',
  modalType: null,
  modalMode: null,
  modalId: null,

  init() {
    SDD.store.load();
    this.updateSidebarProjectName();
    this.bindNav();
    this.bindModal();
    navigate('dashboard');
  },

  updateSidebarProjectName() {
    const el = $('sidebar-project-name');
    if (el) el.textContent = SDD.store.data.project.name;
  },

  bindNav() {
    document.querySelectorAll('.nav-item[data-view]').forEach(item => {
      item.addEventListener('click', () => {
        currentRetroId = null;
        navigate(item.dataset.view);
      });
    });
  },

  bindModal() {
    $('modal-overlay').addEventListener('click', e => {
      if (e.target === $('modal-overlay')) this.closeModal();
    });
    $('modal-close-btn').addEventListener('click', () => this.closeModal());
    $('modal-cancel-btn').addEventListener('click', () => this.closeModal());
    $('modal-save-btn').addEventListener('click', () => this.saveModal());
  },

  openModal(type, mode, id) {
    this.modalType = type;
    this.modalMode = mode;
    this.modalId   = id || null;

    const cfg = MODAL_CONFIGS[type];
    if (!cfg) return;

    let data = {};
    if (mode === 'edit' || mode === 'detail') {
      const map = {
        spec:   () => SDD.store.data.specs.find(s => s.id === id),
        task:   () => SDD.store.data.tasks.find(t => t.id === id),
        sprint: () => SDD.store.data.sprints.find(s => s.id === id),
        retro:  () => SDD.store.data.retros.find(r => r.id === id),
        issue:  () => SDD.store.data.issues.find(i => i.id === id)
      };
      data = (map[type] && map[type]()) || {};
    }

    $('modal-title').textContent = cfg.title(mode, data);
    $('modal-body').innerHTML    = cfg.body(mode, data);

    const saveBtn = $('modal-save-btn');
    if (mode === 'detail') {
      saveBtn.style.display = 'none';
    } else {
      saveBtn.style.display = '';
      saveBtn.textContent = mode === 'new' ? '追加' : '保存';
    }

    $('modal-overlay').classList.remove('hidden');
  },

  closeModal() {
    $('modal-overlay').classList.add('hidden');
    this.modalType = this.modalMode = this.modalId = null;
  },

  saveModal() {
    const cfg = MODAL_CONFIGS[this.modalType];
    if (!cfg) return;
    const result = cfg.save(this.modalMode, this.modalId);
    if (result === false) return; // validation failed
    this.closeModal();
    navigate(this.currentViewFromType(this.modalType));
  },

  currentViewFromType(type) {
    const map = { spec:'specs', task:'tasks', sprint:'sprints', retro:'retros', issue:'issues' };
    return map[type] || 'dashboard';
  },

  setCurrentRetro(id) {
    currentRetroId = id;
    renderView('retros');
  },

  generateTaskListMd() {
    const store = SDD.store;
    const today = store.today();
    const sprints = store.data.sprints;
    const tasks   = store.data.tasks;

    let md = `# タスク一覧\n\n`;
    md += `> 最終更新: ${today} （SDD Project Manager より自動生成）\n\n`;
    md += `---\n\n`;

    // sprints
    sprints.forEach(sprint => {
      const sprintTasks = tasks.filter(t => t.sprintId === sprint.id);
      const totalSP = sprintTasks.reduce((s,t) => s + (t.sp||0), 0);
      const doneSP  = sprintTasks.filter(t=>t.status==='done').reduce((s,t) => s + (t.sp||0), 0);

      md += `## ${sprint.name}`;
      if (sprint.status === 'active') md += ' 🏃 アクティブ';
      else if (sprint.status === 'completed') md += ' ✅ 完了';
      else md += ' 📋 計画中';
      md += `\n\n`;
      md += `- **ゴール**: ${sprint.goal || '—'}\n`;
      md += `- **期間**: ${sprint.startDate} 〜 ${sprint.endDate}\n`;
      md += `- **進捗**: ${doneSP} / ${totalSP} SP 完了\n\n`;

      if (sprintTasks.length) {
        md += `| ID | タイトル | ステータス | 担当者 | SP | 仕様書 | Issue | PR | ブランチ |\n`;
        md += `|----|---------|-----------|--------|:--:|--------|-------|:--:|----------|\n`;
        sprintTasks.forEach(t => {
          const statusIcon = {pending:'⬜', in_progress:'🔵', review:'🟡', done:'✅'}[t.status] || '';
          md += `| ${t.id} | ${t.title} | ${statusIcon} ${t.status} | ${t.assignee||'—'} | ${t.sp} | ${t.specId||'—'} | ${t.issueNumber?'#'+t.issueNumber:'—'} | ${t.pr?'#'+t.pr:'—'} | ${t.branch||'—'} |\n`;
        });
        md += '\n';
      } else {
        md += `_タスクなし_\n\n`;
      }
    });

    // unassigned
    const unassigned = tasks.filter(t => !t.sprintId);
    if (unassigned.length) {
      md += `## スプリント未割当\n\n`;
      md += `| ID | タイトル | ステータス | 担当者 | SP | 仕様書 | メモ |\n`;
      md += `|----|---------|-----------|--------|:--:|--------|------|\n`;
      unassigned.forEach(t => {
        const statusIcon = {pending:'⬜', in_progress:'🔵', review:'🟡', done:'✅'}[t.status] || '';
        md += `| ${t.id} | ${t.title} | ${statusIcon} ${t.status} | ${t.assignee||'—'} | ${t.sp} | ${t.specId||'—'} | ${t.notes||'—'} |\n`;
      });
      md += '\n';
    }

    // summary
    const allDone = tasks.filter(t=>t.status==='done').length;
    md += `---\n\n`;
    md += `## サマリー\n\n`;
    md += `| 項目 | 値 |\n|------|----|\n`;
    md += `| 総タスク数 | ${tasks.length} |\n`;
    md += `| 完了 | ${allDone} |\n`;
    md += `| 進行中 | ${tasks.filter(t=>t.status==='in_progress').length} |\n`;
    md += `| 未着手 | ${tasks.filter(t=>t.status==='pending').length} |\n`;
    md += `| レビュー中 | ${tasks.filter(t=>t.status==='review').length} |\n`;

    return md;
  },

  downloadTaskListMd() {
    const md   = this.generateTaskListMd();
    const blob = new Blob([md], {type: 'text/markdown'});
    const a    = document.createElement('a');
    a.href     = URL.createObjectURL(blob);
    a.download = 'task-list.md';
    a.click();
    URL.revokeObjectURL(a.href);
  }
};

/* ═══════════════════════════════════════════════════════
   FILE SYSTEM SYNC  (File System Access API)
═══════════════════════════════════════════════════════ */

SDD.fs = {
  dirHandle: null,
  _busy: false,
  supported: ('showDirectoryPicker' in window),

  /* ── ユーザーがフォルダを選択して接続 ── */
  async connect() {
    if (!this.supported) {
      alert('このブラウザは File System Access API に対応していません。\nChrome または Edge をご利用ください。');
      return;
    }
    try {
      this.dirHandle = await window.showDirectoryPicker({ mode: 'readwrite' });
      this._setStatus('connected');
      // 既存の pm-data.json があればインポート
      const imported = await this._readPmData(true);
      if (imported) {
        SDD.app.updateSidebarProjectName();
        navigate(SDD.app.currentView || 'dashboard');
        this._toast('✅ pm-data.json を読み込みました');
      } else {
        // 初回接続：現在データをプッシュ
        await this._writeAll();
        this._toast('✅ プロジェクトフォルダに接続しました');
      }
      this._refreshSettingsIfOpen();
    } catch (e) {
      if (e.name !== 'AbortError') {
        this._setStatus('error');
        this._toast('❌ 接続エラー: ' + e.message);
      }
    }
  },

  /* ── 接続解除 ── */
  disconnect() {
    this.dirHandle = null;
    this._setStatus('disconnected');
    const btn = document.getElementById('sync-connect-btn');
    if (btn) { btn.textContent = '📁 接続'; btn.onclick = () => SDD.fs.connect(); }
    this._refreshSettingsIfOpen();
  },

  /* ── 自動同期（save() から呼ばれる） ── */
  async autoSync() {
    if (!this.dirHandle || this._busy) return;
    this._busy = true;
    this._setStatus('syncing');
    try {
      await this._writeAll();
      this._setStatus('connected');
    } catch (e) {
      console.warn('[SDD Sync]', e);
      this._setStatus('error');
    } finally {
      this._busy = false;
    }
  },

  /* ── プル：pm-data.json → ブラウザ ── */
  async pullFromProject() {
    if (!this.dirHandle) return;
    this._setStatus('syncing');
    const ok = await this._readPmData(false);
    if (ok) {
      SDD.app.updateSidebarProjectName();
      navigate(SDD.app.currentView || 'dashboard');
      this._toast('✅ プロジェクトからデータを取得しました');
    } else {
      this._toast('⚠️ tasks/pm-data.json が見つかりません\n先に同期を実行してください');
    }
    this._setStatus('connected');
    this._refreshSettingsIfOpen();
  },

  /* ── ファイル書き込み ── */
  async _writeAll() {
    await this._writePmData();
    await this._writeTaskListMd();
  },

  async _writePmData() {
    const dir = await this.dirHandle.getDirectoryHandle('tasks', { create: true });
    const fh  = await dir.getFileHandle('pm-data.json', { create: true });
    const w   = await fh.createWritable();
    await w.write(JSON.stringify(SDD.store.data, null, 2));
    await w.close();
  },

  async _writeTaskListMd() {
    const dir = await this.dirHandle.getDirectoryHandle('tasks', { create: true });
    const fh  = await dir.getFileHandle('task-list.md', { create: true });
    const w   = await fh.createWritable();
    await w.write(SDD.app.generateTaskListMd());
    await w.close();
  },

  /* ── ファイル読み込み ── */
  async _readPmData(silent) {
    try {
      const dir  = await this.dirHandle.getDirectoryHandle('tasks');
      const fh   = await dir.getFileHandle('pm-data.json');
      const file = await fh.getFile();
      const text = await file.text();
      SDD.store.importJSON(text);
      return true;
    } catch (e) {
      if (e.name === 'NotFoundError') return false;
      if (!silent) this._toast('❌ 読込エラー: ' + e.message);
      return false;
    }
  },

  /* ── UI ── */
  _setStatus(status) {
    const el  = document.getElementById('sync-status');
    const btn = document.getElementById('sync-connect-btn');
    if (!el) return;
    const cfg = {
      disconnected: { dot: 'disconnected', label: '未接続',   btnText: '📁 接続',  btnFn: () => SDD.fs.connect() },
      connected:    { dot: 'connected',    label: '同期済み', btnText: '⏏ 解除',  btnFn: () => SDD.fs.disconnect() },
      syncing:      { dot: 'syncing',      label: '同期中…',  btnText: '⏏ 解除',  btnFn: () => SDD.fs.disconnect() },
      error:        { dot: 'error',        label: 'エラー',   btnText: '📁 再接続', btnFn: () => SDD.fs.connect() }
    }[status] || { dot: 'disconnected', label: '未接続', btnText: '📁 接続', btnFn: () => SDD.fs.connect() };

    el.innerHTML = `<span class="sync-dot ${cfg.dot}"></span>${cfg.label}`;
    if (btn) { btn.textContent = cfg.btnText; btn.onclick = cfg.btnFn; }
  },

  _toast(msg) {
    const t = document.createElement('div');
    t.className = 'sdd-toast';
    t.textContent = msg;
    document.body.appendChild(t);
    setTimeout(() => { t.style.opacity = '0'; setTimeout(() => t.remove(), 350); }, 3000);
  },

  _refreshSettingsIfOpen() {
    if (SDD.app.currentView === 'settings') renderView('settings');
  }
};

/* ── bootstrap ── */
document.addEventListener('DOMContentLoaded', () => SDD.app.init());
