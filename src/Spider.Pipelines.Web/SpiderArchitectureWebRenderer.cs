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
            builder.AppendLine("        <button type=\"button\" class=\"is-active\" data-view=\"graph\">Graph</button>");
            builder.AppendLine("        <button type=\"button\" data-view=\"flows\">Flows</button>");
            builder.AppendLine("        <button type=\"button\" data-view=\"pipelines\">Pipelines</button>");
            builder.AppendLine("        <button type=\"button\" data-view=\"evidence\">Evidence</button>");
            builder.AppendLine("        <button type=\"button\" data-view=\"json\">Raw JSON</button>");
            builder.AppendLine("      </nav>");
            builder.AppendLine("      <div id=\"spider-kind-filters\" class=\"spider-filter-list\"></div>");
            builder.AppendLine("    </aside>");
            builder.AppendLine("    <main class=\"spider-main\">");
            builder.AppendLine("      <header class=\"spider-toolbar\">");
            builder.AppendLine("        <div>");
            builder.AppendLine("          <h1>Architecture Graph</h1>");
            builder.AppendLine("          <p>Generated from Spider flows and pipeline configuration at compile time.</p>");
            builder.AppendLine("        </div>");
            builder.AppendLine("        <div class=\"spider-actions\">");
            builder.AppendLine("          <input id=\"spider-search\" type=\"search\" placeholder=\"Search nodes\" aria-label=\"Search nodes\">");
            builder.AppendLine("          <button id=\"spider-fit\" type=\"button\">Fit Graph</button>");
            builder.AppendLine("        </div>");
            builder.AppendLine("      </header>");
            builder.AppendLine("      <section id=\"spider-view-graph\" class=\"spider-view is-active\" aria-label=\"Architecture graph\">");
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
            => @"
:root {
  color-scheme: light;
  --spider-bg: #f6f7f9;
  --spider-panel: #ffffff;
  --spider-ink: #17202a;
  --spider-muted: #697386;
  --spider-line: #d9dee7;
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
  grid-template-columns: 280px minmax(0, 1fr) 360px;
}
.spider-nav, .spider-detail {
  background: var(--spider-panel);
  border-color: var(--spider-line);
  border-style: solid;
  min-height: 100vh;
  overflow: auto;
}
.spider-nav { border-width: 0 1px 0 0; padding: 18px; }
.spider-detail { border-width: 0 0 0 1px; padding: 18px; }
.spider-main { min-width: 0; display: flex; flex-direction: column; }
.spider-brand { display: flex; gap: 10px; align-items: center; font-weight: 700; font-size: 18px; margin-bottom: 18px; }
.spider-brand-mark { width: 32px; height: 32px; display: grid; place-items: center; color: #fff; background: var(--spider-blue); border-radius: 8px; }
.spider-summary { display: grid; grid-template-columns: 1fr 1fr; gap: 8px; margin-bottom: 18px; }
.spider-metric { border: 1px solid var(--spider-line); border-radius: var(--spider-radius); padding: 10px; background: #fbfcfe; }
.spider-metric strong { display: block; font-size: 20px; }
.spider-metric span { color: var(--spider-muted); font-size: 12px; }
.spider-tabs { display: grid; gap: 6px; margin-bottom: 18px; }
.spider-tabs button, .spider-actions button, .spider-filter-list button {
  border: 1px solid var(--spider-line);
  background: #fff;
  color: var(--spider-ink);
  border-radius: var(--spider-radius);
  cursor: pointer;
}
.spider-tabs button { text-align: left; padding: 9px 10px; }
.spider-tabs button.is-active { background: #eef4ff; color: var(--spider-blue); border-color: #bfd4ff; }
.spider-filter-list { display: grid; gap: 8px; }
.spider-filter-list button { padding: 8px 10px; text-align: left; display: flex; justify-content: space-between; gap: 8px; }
.spider-filter-list button.is-muted { opacity: .45; }
.spider-toolbar {
  height: 86px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 18px;
  padding: 16px 20px;
  border-bottom: 1px solid var(--spider-line);
  background: rgba(255,255,255,.82);
  backdrop-filter: blur(8px);
}
.spider-toolbar h1 { margin: 0; font-size: 22px; line-height: 1.2; }
.spider-toolbar p { margin: 4px 0 0; color: var(--spider-muted); font-size: 13px; }
.spider-actions { display: flex; gap: 8px; align-items: center; }
.spider-actions input { width: min(320px, 34vw); padding: 9px 10px; border: 1px solid var(--spider-line); border-radius: var(--spider-radius); }
.spider-actions button { padding: 9px 12px; }
.spider-view { display: none; padding: 18px 20px; min-height: calc(100vh - 86px); }
.spider-view.is-active { display: block; }
.spider-graph-frame { height: calc(100vh - 122px); border: 1px solid var(--spider-line); background: #fff; border-radius: var(--spider-radius); overflow: hidden; }
.spider-graph { width: 100%; height: 100%; display: block; touch-action: none; }
.spider-node rect { fill: #fff; stroke: var(--spider-line); stroke-width: 1.4; rx: 8; }
.spider-node text { fill: var(--spider-ink); font-size: 12px; pointer-events: none; }
.spider-node .kind { fill: var(--spider-muted); font-size: 10px; }
.spider-node.is-selected rect { stroke: var(--spider-blue); stroke-width: 2.4; fill: #eef4ff; }
.spider-node[data-kind='spider.flow'] rect { stroke: var(--spider-green); }
.spider-node[data-kind='spider.pipeline'] rect { stroke: var(--spider-blue); }
.spider-node[data-kind='spider.flow-branch'] rect, .spider-node[data-kind='spider.flow-condition'] rect { stroke: var(--spider-yellow); }
.spider-node[data-kind='spider.flow-profile'] rect { stroke: var(--spider-violet); }
.spider-edge { stroke: #98a2b3; stroke-width: 1.4; fill: none; marker-end: url(#spider-arrow); }
.spider-edge.next { stroke: var(--spider-green); stroke-width: 2; }
.spider-edge.uses-profile { stroke: var(--spider-violet); stroke-dasharray: 6 5; }
.spider-edge.contains { stroke: #c2c8d2; }
.spider-lane-title { fill: var(--spider-muted); font-size: 12px; font-weight: 700; }
.spider-list { display: grid; gap: 12px; }
.spider-section {
  background: #fff;
  border: 1px solid var(--spider-line);
  border-radius: var(--spider-radius);
  padding: 14px;
}
.spider-section h2, .spider-detail h2 { margin: 0 0 10px; font-size: 18px; }
.spider-sequence { display: flex; flex-wrap: wrap; gap: 8px; align-items: center; }
.spider-step {
  border: 1px solid var(--spider-line);
  border-radius: var(--spider-radius);
  padding: 8px 10px;
  background: #fbfcfe;
}
.spider-step::after { content: ' ->'; color: var(--spider-muted); margin-left: 8px; }
.spider-step:last-child::after { content: ''; margin: 0; }
.spider-detail-empty { color: var(--spider-muted); }
.spider-detail dl { display: grid; grid-template-columns: 110px minmax(0, 1fr); gap: 8px; margin: 0; }
.spider-detail dt { color: var(--spider-muted); }
.spider-detail dd { margin: 0; overflow-wrap: anywhere; }
.spider-chip { display: inline-flex; align-items: center; border: 1px solid var(--spider-line); border-radius: 999px; padding: 3px 8px; font-size: 12px; color: var(--spider-muted); margin: 2px 4px 2px 0; }
pre { margin: 0; white-space: pre-wrap; word-break: break-word; background: #0f172a; color: #dbeafe; border-radius: var(--spider-radius); padding: 16px; min-height: calc(100vh - 122px); overflow: auto; }
@media (max-width: 1040px) {
  .spider-shell { grid-template-columns: 240px minmax(0, 1fr); }
  .spider-detail { grid-column: 1 / -1; min-height: auto; border-width: 1px 0 0 0; }
}
@media (max-width: 760px) {
  .spider-shell { display: block; }
  .spider-nav, .spider-detail { min-height: auto; border-width: 0 0 1px 0; }
  .spider-toolbar { height: auto; align-items: stretch; flex-direction: column; }
  .spider-actions { align-items: stretch; }
  .spider-actions input { width: 100%; }
}";

        /// <summary>
        /// Creates the client script used to render and interact with the graph.
        /// </summary>
        /// <returns>The client script content.</returns>
        private static string CreateJavaScript()
            => @"
(function () {
  'use strict';
  const root = document.getElementById('spider-architecture-app');
  const data = JSON.parse(document.getElementById('spider-manifest-data').textContent);
  const state = {
    view: root.dataset.showGraph === 'true' ? 'graph' : 'flows',
    selectedId: null,
    query: '',
    mutedKinds: new Set(),
    panX: 0,
    panY: 0,
    scale: 1
  };

  const components = data.components || [];
  const relations = data.relations || [];
  const byId = new Map(components.map(component => [component.id, component]));
  const contains = groupBy(relations.filter(relation => relation.kind === 'contains'), relation => relation.sourceId);
  const nextRelations = relations.filter(relation => relation.kind === 'next');

  const kindLabels = {
    'spider.pipeline': 'Pipelines',
    'spider.pipeline-stage': 'Stages',
    'spider.flow': 'Flows',
    'spider.flow-step': 'Steps',
    'spider.flow-condition': 'Conditions',
    'spider.flow-branch': 'Branches',
    'spider.flow-profile': 'Profiles'
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
      .replace(/""/g, '&quot;')
      .replace(/'/g, '&#039;');
  }

  function visibleComponents() {
    const query = state.query.toLowerCase();
    return components.filter(component => {
      if (state.mutedKinds.has(component.kind)) return false;
      if (!query) return true;
      const haystack = [component.id, component.kind, component.displayName, JSON.stringify(component.metadata || {})].join(' ').toLowerCase();
      return haystack.includes(query);
    });
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

  function layoutNodes(nodes) {
    const positions = new Map();
    const laneGap = 170;
    let lane = 0;

    function placeSequence(root, children, startY) {
      positions.set(root.id, { x: 70, y: startY, w: 180, h: 62 });
      const ordered = orderChildren(children.map(relation => byId.get(relation.targetId)).filter(Boolean));
      ordered.forEach((child, index) => {
        positions.set(child.id, { x: 310 + index * 210, y: startY, w: 180, h: 62 });
      });
    }

    const pipelines = nodes.filter(component => component.kind === 'spider.pipeline');
    pipelines.forEach(pipeline => {
      placeSequence(pipeline, contains[pipeline.id] || [], 70 + lane * laneGap);
      lane++;
    });

    const flows = nodes.filter(component => component.kind === 'spider.flow');
    flows.forEach(flow => {
      placeSequence(flow, contains[flow.id] || [], 70 + lane * laneGap);
      lane++;
    });

    const profiles = nodes.filter(component => component.kind === 'spider.flow-profile');
    profiles.forEach((profile, index) => {
      positions.set(profile.id, { x: 70 + (index % 4) * 210, y: 70 + lane * laneGap + Math.floor(index / 4) * 90, w: 180, h: 56 });
    });

    nodes.forEach((component, index) => {
      if (!positions.has(component.id)) {
        positions.set(component.id, { x: 70 + (index % 5) * 210, y: 70 + (lane + Math.floor(index / 5)) * laneGap, w: 180, h: 58 });
      }
    });

    return positions;
  }

  function orderChildren(children) {
    if (children.length < 2) return children;
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

  function renderGraph() {
    if (root.dataset.showGraph !== 'true') return;
    const svg = document.getElementById('spider-architecture-graph');
    const nodes = visibleComponents();
    const visibleIds = new Set(nodes.map(node => node.id));
    const positions = layoutNodes(nodes);
    const graphRelations = relations.filter(relation => visibleIds.has(relation.sourceId) && visibleIds.has(relation.targetId));
    const maxX = Math.max(1000, ...Array.from(positions.values()).map(position => position.x + position.w + 80));
    const maxY = Math.max(640, ...Array.from(positions.values()).map(position => position.y + position.h + 80));
    svg.setAttribute('viewBox', `${-state.panX} ${-state.panY} ${maxX / state.scale} ${maxY / state.scale}`);
    svg.innerHTML = `
      <defs>
        <marker id='spider-arrow' viewBox='0 0 10 10' refX='9' refY='5' markerWidth='7' markerHeight='7' orient='auto-start-reverse'>
          <path d='M 0 0 L 10 5 L 0 10 z' fill='#98a2b3'></path>
        </marker>
      </defs>
      <text class='spider-lane-title' x='70' y='34'>Pipelines, flows, steps, and relationships</text>
      ${graphRelations.map(relation => renderEdge(relation, positions)).join('')}
      ${nodes.map(node => renderNode(node, positions.get(node.id))).join('')}
    `;
    svg.querySelectorAll('.spider-node').forEach(node => {
      node.addEventListener('click', () => {
        state.selectedId = node.dataset.id;
        renderDetail();
        renderGraph();
      });
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
    const mid = Math.max(x1 + 40, (x1 + x2) / 2);
    return `<path class='spider-edge ${escapeHtml(relation.kind)}' d='M ${x1} ${y1} C ${mid} ${y1}, ${mid} ${y2}, ${x2} ${y2}'><title>${escapeHtml(relation.kind)}</title></path>`;
  }

  function renderNode(component, position) {
    if (!position) return '';
    const selected = component.id === state.selectedId ? ' is-selected' : '';
    const label = truncate(component.displayName || component.id, 25);
    const kind = (kindLabels[component.kind] || component.kind).replace('spider.', '');
    return `
      <g class='spider-node${selected}' data-id='${escapeHtml(component.id)}' data-kind='${escapeHtml(component.kind)}' transform='translate(${position.x}, ${position.y})'>
        <rect width='${position.w}' height='${position.h}'></rect>
        <text x='12' y='25'>${escapeHtml(label)}</text>
        <text class='kind' x='12' y='45'>${escapeHtml(kind)}</text>
      </g>`;
  }

  function truncate(value, length) {
    return value && value.length > length ? value.slice(0, length - 1) + '...' : value;
  }

  function renderFlows() {
    const container = document.getElementById('spider-view-flows');
    const flows = components.filter(component => component.kind === 'spider.flow');
    container.innerHTML = `<div class='spider-list'>${flows.map(flow => renderSequenceSection(flow)).join('')}</div>`;
  }

  function renderPipelines() {
    const container = document.getElementById('spider-view-pipelines');
    const pipelines = components.filter(component => component.kind === 'spider.pipeline');
    container.innerHTML = `<div class='spider-list'>${pipelines.map(pipeline => renderSequenceSection(pipeline)).join('')}</div>`;
  }

  function renderSequenceSection(parent) {
    const children = orderChildren((contains[parent.id] || []).map(relation => byId.get(relation.targetId)).filter(Boolean));
    return `
      <article class='spider-section'>
        <h2>${escapeHtml(parent.displayName)}</h2>
        <div>${Object.entries(parent.metadata || {}).map(([key, value]) => `<span class='spider-chip'>${escapeHtml(key)}: ${escapeHtml(value)}</span>`).join('')}</div>
        <div class='spider-sequence'>${children.map(child => `<button type='button' class='spider-step' data-id='${escapeHtml(child.id)}'>${escapeHtml(child.displayName)}</button>`).join('')}</div>
      </article>`;
  }

  function renderEvidence() {
    const showEvidence = root.dataset.showEvidence === 'true';
    const container = document.getElementById('spider-view-evidence');
    if (!showEvidence) {
      container.innerHTML = `<div class='spider-section'><h2>Evidence disabled</h2><p>Enable evidence in renderer options to show source locations.</p></div>`;
      return;
    }
    const entries = components.flatMap(component => (component.evidence || []).map(evidence => ({ component, evidence })));
    container.innerHTML = `<div class='spider-list'>${entries.map(entry => `
      <article class='spider-section'>
        <h2>${escapeHtml(entry.component.displayName)}</h2>
        <dl>
          <dt>Kind</dt><dd>${escapeHtml(entry.component.kind)}</dd>
          <dt>Type</dt><dd>${escapeHtml(entry.evidence.typeName)}</dd>
          <dt>Member</dt><dd>${escapeHtml(entry.evidence.memberName)}</dd>
          <dt>File</dt><dd>${escapeHtml(entry.evidence.filePath)}</dd>
          <dt>Line</dt><dd>${escapeHtml(entry.evidence.lineNumber)}</dd>
        </dl>
      </article>`).join('')}</div>`;
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
    const component = byId.get(state.selectedId) || components.find(item => item.kind === 'spider.flow') || components[0];
    if (!component) {
      detail.innerHTML = `<p class='spider-detail-empty'>No components found.</p>`;
      return;
    }
    state.selectedId = component.id;
    const outgoing = relations.filter(relation => relation.sourceId === component.id);
    const incoming = relations.filter(relation => relation.targetId === component.id);
    const metadata = Object.entries(component.metadata || {});
    const evidence = root.dataset.showEvidence === 'true' ? (component.evidence || []) : [];
    detail.innerHTML = `
      <h2>${escapeHtml(component.displayName)}</h2>
      <p><span class='spider-chip'>${escapeHtml(component.kind)}</span></p>
      <dl>
        <dt>Id</dt><dd>${escapeHtml(component.id)}</dd>
        ${metadata.map(([key, value]) => `<dt>${escapeHtml(key)}</dt><dd>${escapeHtml(value)}</dd>`).join('')}
        <dt>Incoming</dt><dd>${incoming.length}</dd>
        <dt>Outgoing</dt><dd>${outgoing.length}</dd>
      </dl>
      ${evidence.length ? `<h2>Evidence</h2><dl>${evidence.map(item => `<dt>Member</dt><dd>${escapeHtml(item.memberName)}</dd><dt>Source</dt><dd>${escapeHtml(item.filePath)}:${escapeHtml(item.lineNumber)}</dd>`).join('')}</dl>` : ''}
      <h2>Relations</h2>
      <div>${outgoing.concat(incoming).map(relation => `<span class='spider-chip'>${escapeHtml(relation.kind)}</span>`).join('') || '<p class=""spider-detail-empty"">No direct relations.</p>'}</div>`;
  }

  function bindInteractions() {
    document.querySelectorAll('.spider-tabs button').forEach(button => {
      if (button.dataset.view === 'json' && root.dataset.showJson !== 'true') button.hidden = true;
      if (button.dataset.view === 'graph' && root.dataset.showGraph !== 'true') button.hidden = true;
      button.addEventListener('click', () => {
        state.view = button.dataset.view;
        document.querySelectorAll('.spider-tabs button').forEach(tab => tab.classList.toggle('is-active', tab === button));
        document.querySelectorAll('.spider-view').forEach(view => view.classList.toggle('is-active', view.id === `spider-view-${state.view}`));
        renderGraph();
      });
    });
    const search = document.getElementById('spider-search');
    if (root.dataset.showSearch !== 'true') search.hidden = true;
    search.addEventListener('input', () => {
      state.query = search.value;
      renderAll();
    });
    document.getElementById('spider-fit').addEventListener('click', () => {
      state.panX = 0;
      state.panY = 0;
      state.scale = 1;
      renderGraph();
    });
    document.addEventListener('click', event => {
      const step = event.target.closest('.spider-step');
      if (!step) return;
      state.selectedId = step.dataset.id;
      renderDetail();
      state.view = 'graph';
      document.querySelector('[data-view=""graph""]').click();
    });
  }

  function renderAll() {
    renderSummary();
    renderFilters();
    renderFlows();
    renderPipelines();
    renderEvidence();
    renderJson();
    renderDetail();
    renderGraph();
  }

  bindInteractions();
  renderAll();
})();";
    }
}
