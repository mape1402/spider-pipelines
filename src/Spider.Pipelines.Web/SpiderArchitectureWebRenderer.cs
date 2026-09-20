namespace Spider.Pipelines.Web
{
    using System.Net;
    using System.Text;
    using Spider.Pipelines.Architecture;

    /// <summary>
    /// Renders Spider architecture manifests as graphical web documentation.
    /// </summary>
    public sealed class SpiderArchitectureWebRenderer : ISpiderArchitectureWebRenderer
    {
        private readonly ISpiderArchitectureManifestSerializer _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiderArchitectureWebRenderer"/> class.
        /// </summary>
        public SpiderArchitectureWebRenderer()
            : this(new SpiderArchitectureManifestSerializer())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiderArchitectureWebRenderer"/> class.
        /// </summary>
        /// <param name="serializer">The manifest serializer.</param>
        public SpiderArchitectureWebRenderer(ISpiderArchitectureManifestSerializer serializer)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        /// <inheritdoc/>
        public string Render(SpiderArchitectureManifest manifest, SpiderArchitectureWebOptions options = null)
        {
            if (manifest == null)
                throw new ArgumentNullException(nameof(manifest));

            options ??= new SpiderArchitectureWebOptions();

            var json = _serializer.Serialize(manifest);
            var title = Html(options.Title);

            var builder = new StringBuilder();
            builder.AppendLine("<!doctype html>");
            builder.AppendLine("<html lang=\"en\">");
            builder.AppendLine("<head>");
            builder.AppendLine("  <meta charset=\"utf-8\">");
            builder.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
            builder.AppendLine($"  <title>{title}</title>");
            builder.AppendLine("  <style>");
            builder.AppendLine(CreateCss());
            builder.AppendLine("  </style>");
            builder.AppendLine("</head>");
            builder.AppendLine("<body>");
            builder.AppendLine("  <div id=\"spider-architecture-app\" class=\"spider-shell\" data-show-json=\"" + options.IncludeJsonPanel.ToString().ToLowerInvariant() + "\" data-show-evidence=\"" + options.IncludeEvidence.ToString().ToLowerInvariant() + "\" data-show-search=\"" + options.IncludeSearch.ToString().ToLowerInvariant() + "\" data-show-graph=\"" + options.IncludeGraph.ToString().ToLowerInvariant() + "\">");
            builder.AppendLine("    <aside class=\"spider-nav\">");
            builder.AppendLine($"      <div class=\"spider-brand\"><span class=\"spider-brand-mark\">S</span><span>{title}</span></div>");
            builder.AppendLine("      <div id=\"spider-summary\" class=\"spider-summary\"></div>");
            builder.AppendLine("      <nav class=\"spider-tabs\" aria-label=\"Architecture views\">");
            builder.AppendLine("        <button type=\"button\" class=\"is-active\" data-view=\"overview\" data-title=\"Architecture Overview\" data-subtitle=\"Business flows and execution pipelines generated at compile time.\">Overview</button>");
            builder.AppendLine("        <button type=\"button\" data-view=\"graph\" data-title=\"Architecture Graph\" data-subtitle=\"Visual relationship map for the selected components.\">Graph</button>");
            builder.AppendLine("        <button type=\"button\" data-view=\"flows\" data-title=\"Business Flows\" data-subtitle=\"Method-level process descriptions in execution order.\">Flows</button>");
            builder.AppendLine("        <button type=\"button\" data-view=\"pipelines\" data-title=\"Execution Pipelines\" data-subtitle=\"Cross-cutting pipeline stages attached around service execution.\">Pipelines</button>");
            builder.AppendLine("        <button type=\"button\" data-view=\"evidence\" data-title=\"Source Evidence\" data-subtitle=\"Where each documented component was discovered in code.\">Evidence</button>");
            builder.AppendLine("        <button type=\"button\" data-view=\"json\" data-title=\"Raw Manifest\" data-subtitle=\"The generated metadata model used by this page.\">Raw JSON</button>");
            builder.AppendLine("      </nav>");
            builder.AppendLine("      <h2 class=\"spider-nav-heading\">Component Types</h2>");
            builder.AppendLine("      <div id=\"spider-kind-filters\" class=\"spider-filter-list\"></div>");
            builder.AppendLine("    </aside>");
            builder.AppendLine("    <main class=\"spider-main\">");
            builder.AppendLine("      <header class=\"spider-toolbar\">");
            builder.AppendLine("        <div>");
            builder.AppendLine("          <h1 id=\"spider-view-title\">Architecture Overview</h1>");
            builder.AppendLine("          <p id=\"spider-view-subtitle\">Business flows and execution pipelines generated at compile time.</p>");
            builder.AppendLine("        </div>");
            builder.AppendLine("        <div class=\"spider-actions\">");
            builder.AppendLine("          <input id=\"spider-search\" type=\"search\" placeholder=\"Search processes or steps\" aria-label=\"Search processes or steps\">");
            builder.AppendLine("          <button id=\"spider-fit\" type=\"button\">Fit Graph</button>");
            builder.AppendLine("        </div>");
            builder.AppendLine("      </header>");
            builder.AppendLine("      <section id=\"spider-view-overview\" class=\"spider-view is-active\" aria-label=\"Architecture overview\"></section>");
            builder.AppendLine("      <section id=\"spider-view-graph\" class=\"spider-view\" aria-label=\"Architecture graph\">");
            builder.AppendLine("        <div class=\"spider-graph-frame\">");
            builder.AppendLine("          <svg id=\"spider-architecture-graph\" class=\"spider-graph\" role=\"img\" aria-label=\"Spider architecture graph\"></svg>");
            builder.AppendLine("        </div>");
            builder.AppendLine("      </section>");
            builder.AppendLine("      <section id=\"spider-view-flows\" class=\"spider-view\" aria-label=\"Flows\"></section>");
            builder.AppendLine("      <section id=\"spider-view-pipelines\" class=\"spider-view\" aria-label=\"Pipelines\"></section>");
            builder.AppendLine("      <section id=\"spider-view-evidence\" class=\"spider-view\" aria-label=\"Evidence\"></section>");
            builder.AppendLine("      <section id=\"spider-view-json\" class=\"spider-view\" aria-label=\"Raw manifest JSON\"><pre id=\"spider-json\"></pre></section>");
            builder.AppendLine("    </main>");
            builder.AppendLine("    <aside id=\"spider-detail\" class=\"spider-detail\" aria-label=\"Selected component details\"></aside>");
            builder.AppendLine("  </div>");
            builder.AppendLine("  <script id=\"spider-manifest-data\" type=\"application/json\">");
            builder.AppendLine(json);
            builder.AppendLine("  </script>");
            builder.AppendLine("  <script>");
            builder.AppendLine(CreateJavaScript());
            builder.AppendLine("  </script>");
            builder.AppendLine("</body>");
            builder.AppendLine("</html>");
            return builder.ToString();
        }

        /// <summary>
        /// Encodes a value for safe HTML output.
        /// </summary>
        /// <param name="value">The value to encode.</param>
        /// <returns>The encoded HTML value.</returns>
        private static string Html(string value)
            => WebUtility.HtmlEncode(value ?? string.Empty);

        /// <summary>
        /// Creates the stylesheet used by the standalone documentation page.
        /// </summary>
        /// <returns>The stylesheet content.</returns>
        private static string CreateCss()
            => """
:root {
  color-scheme: light;
  --spider-bg: #f4f6f8;
  --spider-panel: #ffffff;
  --spider-ink: #16202a;
  --spider-muted: #667085;
  --spider-soft: #f8fafc;
  --spider-line: #d8dee8;
  --spider-blue: #2563eb;
  --spider-green: #0f766e;
  --spider-red: #b42318;
  --spider-yellow: #a16207;
  --spider-violet: #7c3aed;
  --spider-radius: 8px;
  font-family: Inter, Segoe UI, Roboto, Arial, sans-serif;
}
* { box-sizing: border-box; }
body { margin: 0; background: var(--spider-bg); color: var(--spider-ink); }
button, input { font: inherit; }
.spider-shell {
  min-height: 100vh;
  display: grid;
  grid-template-columns: 280px minmax(0, 1fr) 340px;
}
.spider-nav, .spider-detail {
  background: var(--spider-panel);
  border-color: var(--spider-line);
  border-style: solid;
  min-height: 100vh;
  overflow: auto;
}
.spider-nav { border-width: 0 1px 0 0; padding: 18px; }
.spider-detail { border-width: 0 0 0 1px; padding: 20px; }
.spider-main { min-width: 0; display: flex; flex-direction: column; }
.spider-brand { display: flex; gap: 10px; align-items: center; font-weight: 750; font-size: 18px; line-height: 1.15; margin-bottom: 18px; }
.spider-brand-mark { width: 32px; height: 32px; display: grid; place-items: center; color: #fff; background: var(--spider-blue); border-radius: 8px; flex: 0 0 auto; }
.spider-summary { display: grid; grid-template-columns: 1fr 1fr; gap: 8px; margin-bottom: 18px; }
.spider-metric { border: 1px solid var(--spider-line); border-radius: var(--spider-radius); padding: 10px; background: var(--spider-soft); }
.spider-metric strong { display: block; font-size: 20px; line-height: 1; }
.spider-metric span { color: var(--spider-muted); font-size: 12px; }
.spider-nav-heading { margin: 20px 0 8px; font-size: 12px; color: var(--spider-muted); text-transform: uppercase; letter-spacing: 0; }
.spider-tabs { display: grid; gap: 6px; margin-bottom: 18px; }
.spider-tabs button, .spider-actions button, .spider-filter-list button {
  border: 1px solid var(--spider-line);
  background: #fff;
  color: var(--spider-ink);
  border-radius: var(--spider-radius);
  cursor: pointer;
}
.spider-tabs button { text-align: left; padding: 10px 11px; }
.spider-tabs button.is-active { background: #eef4ff; color: var(--spider-blue); border-color: #bfd4ff; font-weight: 650; }
.spider-filter-list { display: grid; gap: 8px; }
.spider-filter-list button { padding: 8px 10px; text-align: left; display: flex; justify-content: space-between; gap: 8px; }
.spider-filter-list button.is-muted { opacity: .42; }
.spider-toolbar {
  min-height: 88px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 18px;
  padding: 16px 20px;
  border-bottom: 1px solid var(--spider-line);
  background: rgba(255,255,255,.9);
  backdrop-filter: blur(8px);
}
.spider-toolbar h1 { margin: 0; font-size: 24px; line-height: 1.2; }
.spider-toolbar p { margin: 4px 0 0; color: var(--spider-muted); font-size: 13px; }
.spider-actions { display: flex; gap: 8px; align-items: center; }
.spider-actions input { width: min(340px, 34vw); padding: 10px 11px; border: 1px solid var(--spider-line); border-radius: var(--spider-radius); }
.spider-actions button { padding: 10px 12px; }
.spider-view { display: none; padding: 20px; min-height: calc(100vh - 88px); }
.spider-view.is-active { display: block; }
.spider-overview { display: grid; gap: 20px; }
.spider-group { display: grid; gap: 12px; }
.spider-group-title { display: flex; align-items: end; justify-content: space-between; gap: 16px; }
.spider-group-title h2 { margin: 0; font-size: 18px; }
.spider-group-title span { color: var(--spider-muted); font-size: 13px; }
.spider-card-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(360px, 1fr)); gap: 14px; align-items: start; }
.spider-section, .spider-process-card {
  background: #fff;
  border: 1px solid var(--spider-line);
  border-radius: var(--spider-radius);
  padding: 16px;
}
.spider-process-card { display: grid; gap: 14px; }
.spider-card-header { display: flex; justify-content: space-between; gap: 12px; align-items: start; }
.spider-card-header h3, .spider-section h2, .spider-detail h2 { margin: 0; font-size: 18px; line-height: 1.25; }
.spider-card-header p { margin: 5px 0 0; color: var(--spider-muted); font-size: 13px; }
.spider-card-meta { display: flex; flex-wrap: wrap; gap: 6px; justify-content: flex-end; }
.spider-timeline { display: grid; gap: 8px; margin: 0; padding: 0; list-style: none; }
.spider-timeline li { display: grid; grid-template-columns: 28px minmax(0, 1fr); gap: 10px; align-items: start; }
.spider-step-number {
  width: 28px;
  height: 28px;
  display: grid;
  place-items: center;
  border-radius: 999px;
  background: #eef4ff;
  color: var(--spider-blue);
  font-size: 12px;
  font-weight: 750;
}
.spider-step, .spider-stage {
  border: 1px solid var(--spider-line);
  border-radius: var(--spider-radius);
  padding: 9px 10px;
  background: var(--spider-soft);
  color: var(--spider-ink);
  text-align: left;
  cursor: pointer;
  min-width: 0;
}
.spider-step strong, .spider-stage strong { display: block; overflow-wrap: anywhere; }
.spider-step span, .spider-stage span { display: block; color: var(--spider-muted); font-size: 12px; margin-top: 2px; }
.spider-step:hover, .spider-stage:hover { border-color: #9db7ff; background: #f2f6ff; }
.spider-stage-strip { display: grid; grid-template-columns: repeat(auto-fit, minmax(150px, 1fr)); gap: 8px; }
.spider-empty { border: 1px dashed var(--spider-line); border-radius: var(--spider-radius); padding: 16px; color: var(--spider-muted); background: #fff; }
.spider-graph-frame { height: calc(100vh - 128px); border: 1px solid var(--spider-line); background: #fff; border-radius: var(--spider-radius); overflow: auto; }
.spider-graph { display: block; min-width: 100%; min-height: 100%; touch-action: none; }
.spider-node rect { fill: #fff; stroke: var(--spider-line); stroke-width: 1.4; rx: 8; }
.spider-node text { fill: var(--spider-ink); font-size: 13px; font-weight: 650; pointer-events: none; }
.spider-node .kind { fill: var(--spider-muted); font-size: 11px; font-weight: 500; }
.spider-node.is-selected rect { stroke: var(--spider-blue); stroke-width: 2.4; fill: #eef4ff; }
.spider-node[data-kind='spider.flow'] rect { stroke: var(--spider-green); }
.spider-node[data-kind='spider.pipeline'] rect { stroke: var(--spider-blue); }
.spider-node[data-kind='spider.flow-branch'] rect, .spider-node[data-kind='spider.flow-condition'] rect { stroke: var(--spider-yellow); }
.spider-node[data-kind='spider.flow-profile'] rect { stroke: var(--spider-violet); }
.spider-edge { stroke: #98a2b3; stroke-width: 1.4; fill: none; marker-end: url(#spider-arrow); }
.spider-edge.next { stroke: var(--spider-green); stroke-width: 2; }
.spider-edge.uses-profile { stroke: var(--spider-violet); stroke-dasharray: 6 5; }
.spider-edge.contains { stroke: #c2c8d2; }
.spider-lane-title { fill: var(--spider-muted); font-size: 13px; font-weight: 750; }
.spider-list { display: grid; gap: 14px; }
.spider-section dl, .spider-detail dl { display: grid; grid-template-columns: 108px minmax(0, 1fr); gap: 8px; margin: 0; }
.spider-section dt, .spider-detail dt { color: var(--spider-muted); }
.spider-section dd, .spider-detail dd { margin: 0; overflow-wrap: anywhere; }
.spider-detail-header { border-bottom: 1px solid var(--spider-line); padding-bottom: 14px; margin-bottom: 16px; }
.spider-detail-header h2 { margin-top: 6px; overflow-wrap: anywhere; }
.spider-detail-section { display: grid; gap: 8px; margin: 0 0 18px; }
.spider-detail-section h3 { margin: 0; font-size: 14px; color: var(--spider-muted); text-transform: uppercase; letter-spacing: 0; }
.spider-detail-section p { margin: 0; line-height: 1.4; }
.spider-detail-empty { color: var(--spider-muted); }
.spider-chip { display: inline-flex; align-items: center; border: 1px solid var(--spider-line); border-radius: 999px; padding: 3px 8px; font-size: 12px; color: var(--spider-muted); margin: 2px 4px 2px 0; background: #fff; }
.spider-chip.is-flow { color: var(--spider-green); border-color: #a7d6cf; background: #f0fdfa; }
.spider-chip.is-pipeline { color: var(--spider-blue); border-color: #bfd4ff; background: #eef4ff; }
.spider-chip.is-warning { color: var(--spider-yellow); border-color: #f0cf85; background: #fffbeb; }
.spider-chip.is-profile { color: var(--spider-violet); border-color: #d6bcfa; background: #f5f3ff; }
.spider-code { display: block; max-width: 100%; overflow-wrap: anywhere; color: #344054; background: var(--spider-soft); border: 1px solid var(--spider-line); border-radius: var(--spider-radius); padding: 8px; font-family: Consolas, Menlo, monospace; font-size: 12px; }
pre { margin: 0; white-space: pre-wrap; word-break: break-word; background: #0f172a; color: #dbeafe; border-radius: var(--spider-radius); padding: 16px; min-height: calc(100vh - 128px); overflow: auto; }
@media (max-width: 1180px) {
  .spider-shell { grid-template-columns: 260px minmax(0, 1fr); }
  .spider-detail { grid-column: 1 / -1; min-height: auto; border-width: 1px 0 0 0; }
}
@media (max-width: 760px) {
  .spider-shell { display: block; }
  .spider-nav, .spider-detail { min-height: auto; border-width: 0 0 1px 0; }
  .spider-toolbar { align-items: stretch; flex-direction: column; }
  .spider-actions { align-items: stretch; flex-direction: column; }
  .spider-actions input { width: 100%; }
  .spider-card-grid { grid-template-columns: 1fr; }
}
""";

        /// <summary>
        /// Creates the client script used to render and interact with the graph.
        /// </summary>
        /// <returns>The client script content.</returns>
        private static string CreateJavaScript()
            => """
(function () {
  'use strict';

  const root = document.getElementById('spider-architecture-app');
  const data = JSON.parse(document.getElementById('spider-manifest-data').textContent);
  const components = data.components || [];
  const relations = data.relations || [];
  const byId = new Map(components.map(component => [component.id, component]));
  const containsRelations = relations.filter(relation => relation.kind === 'contains');
  const contains = groupBy(containsRelations, relation => relation.sourceId);
  const nextRelations = relations.filter(relation => relation.kind === 'next');
  const title = document.getElementById('spider-view-title');
  const subtitle = document.getElementById('spider-view-subtitle');
  const fitButton = document.getElementById('spider-fit');

  const state = {
    view: 'overview',
    selectedId: null,
    query: '',
    mutedKinds: new Set(),
    scale: 1
  };

  const kindLabels = {
    'spider.pipeline': 'Pipelines',
    'spider.pipeline-stage': 'Pipeline stages',
    'spider.flow': 'Business flows',
    'spider.flow-step': 'Flow steps',
    'spider.flow-condition': 'Conditions',
    'spider.flow-branch': 'Branches',
    'spider.flow-profile': 'Profiles'
  };

  const kindNames = {
    'spider.pipeline': 'Pipeline',
    'spider.pipeline-stage': 'Pipeline stage',
    'spider.flow': 'Flow',
    'spider.flow-step': 'Step',
    'spider.flow-condition': 'Condition',
    'spider.flow-branch': 'Branch',
    'spider.flow-profile': 'Profile'
  };

  const stageNames = {
    'pre-process': 'Pre-process',
    'middleware': 'Middleware',
    'target': 'Target',
    'parallel': 'Parallel work',
    'post-success': 'Success post-process',
    'post-failure': 'Failure post-process'
  };

  function groupBy(items, keySelector) {
    return items.reduce((groups, item) => {
      const key = keySelector(item);
      if (!groups[key]) groups[key] = [];
      groups[key].push(item);
      return groups;
    }, {});
  }

  function escapeHtml(value) {
    return String(value == null ? '' : value)
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#039;');
  }

  function metadata(component, key) {
    return component && component.metadata ? component.metadata[key] : undefined;
  }

  function shortType(value) {
    if (!value) return '';
    const text = String(value);
    const index = text.lastIndexOf('.');
    return index >= 0 ? text.slice(index + 1) : text;
  }

  function friendlyKind(component) {
    return kindNames[component.kind] || component.kind.replace('spider.', '');
  }

  function chipClass(component) {
    if (component.kind === 'spider.flow') return ' is-flow';
    if (component.kind === 'spider.pipeline') return ' is-pipeline';
    if (component.kind === 'spider.flow-condition' || component.kind === 'spider.flow-branch') return ' is-warning';
    if (component.kind === 'spider.flow-profile') return ' is-profile';
    return '';
  }

  function getChildren(parent) {
    return (contains[parent.id] || [])
      .map(relation => byId.get(relation.targetId))
      .filter(Boolean);
  }

  function getProfiles(flow) {
    return relations
      .filter(relation => relation.kind === 'uses-profile' && relation.sourceId === flow.id)
      .map(relation => byId.get(relation.targetId))
      .filter(Boolean);
  }

  function orderChildren(children) {
    if (children.length < 2) return children;

    const hasOrder = children.every(child => metadata(child, 'order') != null);
    if (hasOrder) {
      return children.slice().sort((left, right) => Number(metadata(left, 'order')) - Number(metadata(right, 'order')));
    }

    const ids = new Set(children.map(child => child.id));
    const targets = new Set(nextRelations.filter(relation => ids.has(relation.sourceId) && ids.has(relation.targetId)).map(relation => relation.targetId));
    const start = children.find(child => !targets.has(child.id)) || children[0];
    const ordered = [start];
    const seen = new Set([start.id]);
    let current = start;

    while (current) {
      const next = nextRelations.find(relation => relation.sourceId === current.id && ids.has(relation.targetId));
      if (!next || seen.has(next.targetId)) break;
      const nextComponent = byId.get(next.targetId);
      if (!nextComponent) break;
      ordered.push(nextComponent);
      seen.add(nextComponent.id);
      current = nextComponent;
    }

    children.forEach(child => {
      if (!seen.has(child.id)) ordered.push(child);
    });
    return ordered;
  }

  function componentMatches(component) {
    if (state.mutedKinds.has(component.kind)) return false;
    const query = state.query.trim().toLowerCase();
    if (!query) return true;
    const haystack = [component.id, component.kind, component.displayName, JSON.stringify(component.metadata || {})].join(' ').toLowerCase();
    return haystack.includes(query);
  }

  function sequenceMatches(parent) {
    if (componentMatches(parent)) return true;
    return getChildren(parent).some(componentMatches) || getProfiles(parent).some(componentMatches);
  }

  function visibleComponents() {
    return components.filter(componentMatches);
  }

  function selectComponent(id) {
    state.selectedId = id;
    renderDetail();
    if (state.view === 'graph') renderGraph();
  }

  function renderSummary() {
    const counts = {
      Components: components.length,
      Relations: relations.length,
      Flows: components.filter(component => component.kind === 'spider.flow').length,
      Pipelines: components.filter(component => component.kind === 'spider.pipeline').length
    };
    document.getElementById('spider-summary').innerHTML = Object.entries(counts)
      .map(([label, value]) => `<div class='spider-metric'><strong>${value}</strong><span>${label}</span></div>`)
      .join('');
  }

  function renderFilters() {
    const counts = groupBy(components, component => component.kind);
    const container = document.getElementById('spider-kind-filters');
    container.innerHTML = Object.keys(counts).sort().map(kind => {
      const muted = state.mutedKinds.has(kind) ? ' is-muted' : '';
      return `<button type='button' class='${muted}' data-kind='${escapeHtml(kind)}'><span>${escapeHtml(kindLabels[kind] || kind)}</span><strong>${counts[kind].length}</strong></button>`;
    }).join('');

    container.querySelectorAll('button').forEach(button => {
      button.addEventListener('click', () => {
        const kind = button.dataset.kind;
        if (state.mutedKinds.has(kind)) state.mutedKinds.delete(kind);
        else state.mutedKinds.add(kind);
        renderAll();
      });
    });
  }

  function renderOverview() {
    const container = document.getElementById('spider-view-overview');
    const flows = components.filter(component => component.kind === 'spider.flow' && sequenceMatches(component));
    const pipelines = components.filter(component => component.kind === 'spider.pipeline' && sequenceMatches(component));

    container.innerHTML = `
      <div class='spider-overview'>
        ${renderOverviewGroup('Business Flows', 'Method-level processes, in the order they execute.', flows, renderFlowCard)}
        ${renderOverviewGroup('Execution Pipelines', 'Cross-cutting behavior around service calls.', pipelines, renderPipelineCard)}
      </div>`;
    bindSelectableCards(container);
  }

  function renderOverviewGroup(groupTitle, description, items, renderer) {
    if (!items.length) {
      return `
        <section class='spider-group'>
          <div class='spider-group-title'><div><h2>${escapeHtml(groupTitle)}</h2><span>${escapeHtml(description)}</span></div></div>
          <div class='spider-empty'>No matching items.</div>
        </section>`;
    }

    return `
      <section class='spider-group'>
        <div class='spider-group-title'>
          <div><h2>${escapeHtml(groupTitle)}</h2><span>${escapeHtml(description)}</span></div>
          <span>${items.length} item${items.length === 1 ? '' : 's'}</span>
        </div>
        <div class='spider-card-grid'>${items.map(renderer).join('')}</div>
      </section>`;
  }

  function renderFlowCard(flow) {
    const steps = orderChildren(getChildren(flow));
    const profiles = getProfiles(flow);
    const request = shortType(metadata(flow, 'request'));
    const response = shortType(metadata(flow, 'response'));
    const signature = response ? `${request} -> ${response}` : request;

    return `
      <article class='spider-process-card'>
        <header class='spider-card-header'>
          <div>
            <h3>${escapeHtml(flow.displayName)}</h3>
            <p>${escapeHtml(signature)}</p>
          </div>
          <div class='spider-card-meta'>
            <span class='spider-chip is-flow'>Flow</span>
            ${profiles.map(profile => `<span class='spider-chip is-profile'>${escapeHtml(profile.displayName)}</span>`).join('')}
          </div>
        </header>
        ${renderTimeline(steps)}
      </article>`;
  }

  function renderPipelineCard(pipeline) {
    const stages = orderChildren(getChildren(pipeline));
    const request = shortType(metadata(pipeline, 'request'));
    const response = shortType(metadata(pipeline, 'response'));
    const signature = response ? `${request} -> ${response}` : request;

    return `
      <article class='spider-process-card'>
        <header class='spider-card-header'>
          <div>
            <h3>${escapeHtml(pipeline.displayName)}</h3>
            <p>${escapeHtml(signature)}</p>
          </div>
          <div class='spider-card-meta'><span class='spider-chip is-pipeline'>Pipeline</span></div>
        </header>
        <div class='spider-stage-strip'>${stages.map(renderStageButton).join('')}</div>
      </article>`;
  }

  function renderTimeline(steps) {
    if (!steps.length) return `<div class='spider-empty'>No documented steps.</div>`;

    return `
      <ol class='spider-timeline'>
        ${steps.map((step, index) => `
          <li>
            <span class='spider-step-number'>${index + 1}</span>
            ${renderStepButton(step)}
          </li>`).join('')}
      </ol>`;
  }

  function renderStepButton(step) {
    const role = describeShort(step);
    return `
      <button type='button' class='spider-step' data-id='${escapeHtml(step.id)}'>
        <strong>${escapeHtml(step.displayName)}</strong>
        <span>${escapeHtml(role)}</span>
      </button>`;
  }

  function renderStageButton(stage) {
    const name = stageNames[metadata(stage, 'stage')] || stage.displayName;
    const count = metadata(stage, 'count');
    const detail = count == null ? friendlyKind(stage) : `${count} configured`;

    return `
      <button type='button' class='spider-stage' data-id='${escapeHtml(stage.id)}'>
        <strong>${escapeHtml(name)}</strong>
        <span>${escapeHtml(detail)}</span>
      </button>`;
  }

  function describeShort(component) {
    if (component.kind === 'spider.flow-condition') return metadata(component, 'otherwise') ? `Continue if true, otherwise ${metadata(component, 'otherwise')}` : 'Continue if true';
    if (component.kind === 'spider.flow-branch') return 'Decision branch';
    if (component.kind === 'spider.flow-step') return metadata(component, 'operation') || 'Flow step';
    if (component.kind === 'spider.pipeline-stage') return stageNames[metadata(component, 'stage')] || 'Pipeline stage';
    return friendlyKind(component);
  }

  function renderFlows() {
    const container = document.getElementById('spider-view-flows');
    const flows = components.filter(component => component.kind === 'spider.flow' && sequenceMatches(component));
    container.innerHTML = `<div class='spider-list'>${flows.map(flow => renderSequenceSection(flow)).join('') || `<div class='spider-empty'>No matching flows.</div>`}</div>`;
    bindSelectableCards(container);
  }

  function renderPipelines() {
    const container = document.getElementById('spider-view-pipelines');
    const pipelines = components.filter(component => component.kind === 'spider.pipeline' && sequenceMatches(component));
    container.innerHTML = `<div class='spider-list'>${pipelines.map(pipeline => renderSequenceSection(pipeline)).join('') || `<div class='spider-empty'>No matching pipelines.</div>`}</div>`;
    bindSelectableCards(container);
  }

  function renderSequenceSection(parent) {
    const children = orderChildren(getChildren(parent));
    const profiles = parent.kind === 'spider.flow' ? getProfiles(parent) : [];
    const request = shortType(metadata(parent, 'request'));
    const response = shortType(metadata(parent, 'response'));
    const signature = response ? `${request} -> ${response}` : request;

    return `
      <article class='spider-section'>
        <div class='spider-card-header'>
          <div>
            <h2>${escapeHtml(parent.displayName)}</h2>
            <p>${escapeHtml(signature)}</p>
          </div>
          <div class='spider-card-meta'>
            <span class='spider-chip${chipClass(parent)}'>${escapeHtml(friendlyKind(parent))}</span>
            ${profiles.map(profile => `<span class='spider-chip is-profile'>${escapeHtml(profile.displayName)}</span>`).join('')}
          </div>
        </div>
        ${parent.kind === 'spider.pipeline' ? `<div class='spider-stage-strip'>${children.map(renderStageButton).join('')}</div>` : renderTimeline(children)}
      </article>`;
  }

  function renderEvidence() {
    const showEvidence = root.dataset.showEvidence === 'true';
    const container = document.getElementById('spider-view-evidence');
    if (!showEvidence) {
      container.innerHTML = `<div class='spider-section'><h2>Evidence disabled</h2><p>Enable evidence in renderer options to show source locations.</p></div>`;
      return;
    }

    const entries = components
      .filter(componentMatches)
      .flatMap(component => (component.evidence || []).map(evidence => ({ component, evidence })));

    container.innerHTML = `<div class='spider-list'>${entries.map(entry => `
      <article class='spider-section'>
        <h2>${escapeHtml(entry.component.displayName)}</h2>
        <dl>
          <dt>Kind</dt><dd>${escapeHtml(friendlyKind(entry.component))}</dd>
          <dt>Type</dt><dd>${escapeHtml(entry.evidence.typeName)}</dd>
          <dt>Member</dt><dd>${escapeHtml(entry.evidence.memberName)}</dd>
          <dt>File</dt><dd>${escapeHtml(entry.evidence.filePath)}</dd>
          <dt>Line</dt><dd>${escapeHtml(entry.evidence.lineNumber)}</dd>
        </dl>
      </article>`).join('') || `<div class='spider-empty'>No matching evidence.</div>`}</div>`;
  }

  function renderJson() {
    const pre = document.getElementById('spider-json');
    if (root.dataset.showJson !== 'true') {
      pre.textContent = 'Raw JSON panel disabled.';
      return;
    }

    pre.textContent = JSON.stringify(data, null, 2);
  }

  function renderDetail() {
    const detail = document.getElementById('spider-detail');
    const component = byId.get(state.selectedId)
      || components.find(item => item.kind === 'spider.flow')
      || components.find(item => item.kind === 'spider.pipeline')
      || components[0];

    if (!component) {
      detail.innerHTML = `<p class='spider-detail-empty'>No components found.</p>`;
      return;
    }

    state.selectedId = component.id;
    const metadataEntries = Object.entries(component.metadata || {});
    const evidence = root.dataset.showEvidence === 'true' ? (component.evidence || []) : [];
    const parentRelation = containsRelations.find(relation => relation.targetId === component.id);
    const parent = parentRelation ? byId.get(parentRelation.sourceId) : null;
    const siblings = parent ? orderChildren(getChildren(parent)) : [];
    const stepNumber = siblings.findIndex(item => item.id === component.id) + 1;
    const outgoing = relations.filter(relation => relation.sourceId === component.id);
    const incoming = relations.filter(relation => relation.targetId === component.id);

    detail.innerHTML = `
      <header class='spider-detail-header'>
        <span class='spider-chip${chipClass(component)}'>${escapeHtml(friendlyKind(component))}</span>
        <h2>${escapeHtml(component.displayName)}</h2>
      </header>
      <section class='spider-detail-section'>
        <h3>Role</h3>
        <p>${escapeHtml(describeLong(component, parent, stepNumber))}</p>
      </section>
      ${metadataEntries.length ? `<section class='spider-detail-section'><h3>Metadata</h3><dl>${metadataEntries.map(([key, value]) => `<dt>${escapeHtml(cleanKey(key))}</dt><dd>${escapeHtml(value)}</dd>`).join('')}</dl></section>` : ''}
      ${evidence.length ? `<section class='spider-detail-section'><h3>Source</h3><dl>${evidence.map(item => `<dt>Member</dt><dd>${escapeHtml(item.memberName)}</dd><dt>File</dt><dd>${escapeHtml(item.filePath)}:${escapeHtml(item.lineNumber)}</dd>`).join('')}</dl></section>` : ''}
      <section class='spider-detail-section'>
        <h3>Relationships</h3>
        <p>${incoming.length} incoming, ${outgoing.length} outgoing</p>
        <div>${outgoing.concat(incoming).map(relation => `<span class='spider-chip'>${escapeHtml(relation.kind)}</span>`).join('') || `<span class='spider-detail-empty'>No direct relations.</span>`}</div>
      </section>
      <section class='spider-detail-section'>
        <h3>Technical id</h3>
        <code class='spider-code'>${escapeHtml(component.id)}</code>
      </section>`;
  }

  function cleanKey(key) {
    return key
      .replace(/([A-Z])/g, ' $1')
      .replace(/-/g, ' ')
      .replace(/^./, value => value.toUpperCase());
  }

  function describeLong(component, parent, stepNumber) {
    if (component.kind === 'spider.flow') return 'A method-level business process composed with Spider.';
    if (component.kind === 'spider.pipeline') return 'A cross-cutting execution pipeline attached around a service call.';
    if (component.kind === 'spider.pipeline-stage') return `${stageNames[metadata(component, 'stage')] || component.displayName} inside ${parent ? parent.displayName : 'the pipeline'}.`;
    if (component.kind === 'spider.flow-condition') return `Step ${stepNumber} in ${parent ? parent.displayName : 'the flow'}; the flow continues only when the condition passes.`;
    if (component.kind === 'spider.flow-branch') return `Step ${stepNumber} in ${parent ? parent.displayName : 'the flow'}; this step chooses one of the configured routes.`;
    if (component.kind === 'spider.flow-step') return `Step ${stepNumber} in ${parent ? parent.displayName : 'the flow'}.`;
    if (component.kind === 'spider.flow-profile') return 'A reusable execution profile selected by one or more flows.';
    return friendlyKind(component);
  }

  function layoutNodes(nodes) {
    const positions = new Map();
    const lanes = [];
    let laneIndex = 0;

    function placeSequence(rootNode, children, titleText) {
      const y = 86 + laneIndex * 150;
      const ordered = orderChildren(children);
      lanes.push({ title: titleText, y: y - 30 });
      positions.set(rootNode.id, { x: 60, y, w: 230, h: 68 });
      ordered.forEach((child, index) => {
        positions.set(child.id, { x: 350 + index * 260, y, w: 230, h: 68 });
      });
      laneIndex++;
    }

    nodes.filter(component => component.kind === 'spider.pipeline')
      .forEach(pipeline => placeSequence(pipeline, getChildren(pipeline), 'Execution pipeline'));

    nodes.filter(component => component.kind === 'spider.flow')
      .forEach(flow => placeSequence(flow, getChildren(flow), 'Business flow'));

    const profiles = nodes.filter(component => component.kind === 'spider.flow-profile');
    if (profiles.length) {
      const y = 86 + laneIndex * 150;
      lanes.push({ title: 'Flow profiles', y: y - 30 });
      profiles.forEach((profile, index) => {
        positions.set(profile.id, { x: 60 + (index % 4) * 260, y: y + Math.floor(index / 4) * 88, w: 230, h: 58 });
      });
      laneIndex += Math.max(1, Math.ceil(profiles.length / 4));
    }

    nodes.forEach((component, index) => {
      if (!positions.has(component.id)) {
        const y = 86 + laneIndex * 150 + Math.floor(index / 4) * 88;
        positions.set(component.id, { x: 60 + (index % 4) * 260, y, w: 230, h: 58 });
      }
    });

    return { positions, lanes };
  }

  function renderGraph() {
    if (root.dataset.showGraph !== 'true') return;
    const svg = document.getElementById('spider-architecture-graph');
    const nodes = visibleComponents();
    const visibleIds = new Set(nodes.map(node => node.id));
    const layout = layoutNodes(nodes);
    const positions = layout.positions;
    const graphRelations = relations.filter(relation => {
      if (!visibleIds.has(relation.sourceId) || !visibleIds.has(relation.targetId)) return false;
      if (relation.kind === 'next' || relation.kind === 'uses-profile') return true;
      if (relation.kind !== 'contains') return false;
      const parent = byId.get(relation.sourceId);
      if (!parent) return false;
      const first = orderChildren(getChildren(parent))[0];
      return first && first.id === relation.targetId;
    });
    const maxX = Math.max(900, ...Array.from(positions.values()).map(position => position.x + position.w + 70));
    const maxY = Math.max(560, ...Array.from(positions.values()).map(position => position.y + position.h + 80));
    svg.setAttribute('viewBox', `0 0 ${maxX} ${maxY}`);
    svg.style.width = `${maxX * state.scale}px`;
    svg.style.height = `${maxY * state.scale}px`;
    svg.innerHTML = `
      <defs>
        <marker id='spider-arrow' viewBox='0 0 10 10' refX='9' refY='5' markerWidth='7' markerHeight='7' orient='auto-start-reverse'>
          <path d='M 0 0 L 10 5 L 0 10 z' fill='#98a2b3'></path>
        </marker>
      </defs>
      ${layout.lanes.map(lane => `<text class='spider-lane-title' x='60' y='${lane.y}'>${escapeHtml(lane.title)}</text>`).join('')}
      ${graphRelations.map(relation => renderEdge(relation, positions)).join('')}
      ${nodes.map(node => renderNode(node, positions.get(node.id))).join('')}
    `;

    svg.querySelectorAll('.spider-node').forEach(node => {
      node.addEventListener('click', () => selectComponent(node.dataset.id));
    });
  }

  function renderEdge(relation, positions) {
    const source = positions.get(relation.sourceId);
    const target = positions.get(relation.targetId);
    if (!source || !target) return '';
    const x1 = source.x + source.w;
    const y1 = source.y + source.h / 2;
    const x2 = target.x;
    const y2 = target.y + target.h / 2;
    const mid = Math.max(x1 + 55, (x1 + x2) / 2);
    return `<path class='spider-edge ${escapeHtml(relation.kind)}' d='M ${x1} ${y1} C ${mid} ${y1}, ${mid} ${y2}, ${x2} ${y2}'><title>${escapeHtml(relation.kind)}</title></path>`;
  }

  function renderNode(component, position) {
    if (!position) return '';
    const selected = component.id === state.selectedId ? ' is-selected' : '';
    const lines = splitLabel(component.displayName || component.id, 26);
    return `
      <g class='spider-node${selected}' data-id='${escapeHtml(component.id)}' data-kind='${escapeHtml(component.kind)}' transform='translate(${position.x}, ${position.y})'>
        <rect width='${position.w}' height='${position.h}'></rect>
        <text x='13' y='24'>${escapeHtml(lines[0] || '')}</text>
        ${lines[1] ? `<text x='13' y='41'>${escapeHtml(lines[1])}</text>` : ''}
        <text class='kind' x='13' y='58'>${escapeHtml(friendlyKind(component))}</text>
      </g>`;
  }

  function splitLabel(value, length) {
    const words = String(value || '').split(/\s+/);
    const lines = [''];
    words.forEach(word => {
      const index = lines.length - 1;
      const next = lines[index] ? `${lines[index]} ${word}` : word;
      if (next.length > length && lines.length < 2) lines.push(word);
      else lines[index] = next;
    });
    return lines.map(line => line.length > length ? `${line.slice(0, length - 1)}...` : line);
  }

  function bindSelectableCards(scope) {
    scope.querySelectorAll('[data-id]').forEach(item => {
      item.addEventListener('click', () => selectComponent(item.dataset.id));
    });
  }

  function bindInteractions() {
    document.querySelectorAll('.spider-tabs button').forEach(button => {
      if (button.dataset.view === 'json' && root.dataset.showJson !== 'true') button.hidden = true;
      if (button.dataset.view === 'graph' && root.dataset.showGraph !== 'true') button.hidden = true;
      button.addEventListener('click', () => {
        state.view = button.dataset.view;
        title.textContent = button.dataset.title || button.textContent;
        subtitle.textContent = button.dataset.subtitle || '';
        document.querySelectorAll('.spider-tabs button').forEach(tab => tab.classList.toggle('is-active', tab === button));
        document.querySelectorAll('.spider-view').forEach(view => view.classList.toggle('is-active', view.id === `spider-view-${state.view}`));
        fitButton.hidden = state.view !== 'graph';
        if (state.view === 'graph') renderGraph();
      });
    });

    const search = document.getElementById('spider-search');
    if (root.dataset.showSearch !== 'true') search.hidden = true;
    search.addEventListener('input', () => {
      state.query = search.value;
      renderAll();
    });

    fitButton.addEventListener('click', () => {
      const frame = document.querySelector('.spider-graph-frame');
      const svg = document.getElementById('spider-architecture-graph');
      const viewBox = svg.getAttribute('viewBox').split(/\s+/).map(Number);
      const graphWidth = viewBox[2] || frame.clientWidth;
      state.scale = Math.max(0.35, Math.min(1, (frame.clientWidth - 24) / graphWidth));
      renderGraph();
    });
    fitButton.hidden = true;
  }

  function renderAll() {
    renderSummary();
    renderFilters();
    renderOverview();
    renderFlows();
    renderPipelines();
    renderEvidence();
    renderJson();
    renderDetail();
    renderGraph();
  }

  bindInteractions();
  renderAll();
})();
""";
    }
}
