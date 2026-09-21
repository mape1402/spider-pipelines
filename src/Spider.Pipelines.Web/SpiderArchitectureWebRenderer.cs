namespace Spider.Pipelines.Web
{
    using System.Text;
    using System.Text.Encodings.Web;
    using Spider.Pipelines.Architecture;

    /// <summary>
    /// Renders Spider architecture manifests as focused graphical web documentation.
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
        /// <param name="serializer">The manifest serializer used to embed architecture metadata.</param>
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

            var title = HtmlEncoder.Default.Encode(options.Title ?? "Spider Architecture");
            var manifestJson = _serializer.Serialize(manifest);
            var html = new StringBuilder();

            html.AppendLine("<!doctype html>");
            html.AppendLine("<html lang=\"en\">");
            html.AppendLine("<head>");
            html.AppendLine("  <meta charset=\"utf-8\" />");
            html.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
            html.AppendLine($"  <title>{title}</title>");
            html.AppendLine("  <style>");
            html.AppendLine(CreateStyles());
            html.AppendLine("  </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine(
                $"  <div id=\"spider-documentation-app\" class=\"spider-shell spider-architecture-app\" data-show-evidence=\"{BooleanAttribute(options.IncludeEvidence)}\" data-show-graph=\"{BooleanAttribute(options.IncludeGraph)}\" data-show-json=\"{BooleanAttribute(options.IncludeJsonPanel)}\" data-show-search=\"{BooleanAttribute(options.IncludeSearch)}\">");
            html.AppendLine("    <aside class=\"spider-sidebar\" aria-label=\"Spider architecture navigation\">");
            html.AppendLine("      <div class=\"spider-brand\">");
            html.AppendLine("        <div class=\"spider-logo\" aria-hidden=\"true\">S</div>");
            html.AppendLine("        <div>");
            html.AppendLine($"          <div class=\"spider-title\">{title}</div>");
            html.AppendLine("          <div class=\"spider-subtitle\">Architecture documentation</div>");
            html.AppendLine("        </div>");
            html.AppendLine("      </div>");
            html.AppendLine("      <label class=\"spider-search\" data-search-region>");
            html.AppendLine("        <span>Search</span>");
            html.AppendLine("        <input id=\"spider-search\" type=\"search\" autocomplete=\"off\" placeholder=\"Flow, pipeline, step\" />");
            html.AppendLine("      </label>");
            html.AppendLine("      <nav class=\"spider-navigation\">");
            html.AppendLine("        <section class=\"spider-nav-section\" aria-labelledby=\"spider-pipeline-heading\">");
            html.AppendLine("          <div class=\"spider-nav-heading\" id=\"spider-pipeline-heading\"><span>Pipelines</span><strong id=\"spider-pipeline-count\">0</strong></div>");
            html.AppendLine("          <div id=\"spider-pipeline-list\" class=\"spider-nav-list\" data-kind=\"pipelines\"></div>");
            html.AppendLine("        </section>");
            html.AppendLine("        <section class=\"spider-nav-section\" aria-labelledby=\"spider-flow-heading\">");
            html.AppendLine("          <div class=\"spider-nav-heading\" id=\"spider-flow-heading\"><span>Flows</span><strong id=\"spider-flow-count\">0</strong></div>");
            html.AppendLine("          <div id=\"spider-flow-list\" class=\"spider-nav-list\" data-kind=\"flows\"></div>");
            html.AppendLine("        </section>");
            html.AppendLine("      </nav>");
            html.AppendLine("      <button id=\"spider-json-link\" class=\"spider-json-link\" type=\"button\">Manifest JSON</button>");
            html.AppendLine("    </aside>");
            html.AppendLine("    <main class=\"spider-main\">");
            html.AppendLine("      <section id=\"spider-detail\" class=\"spider-detail\" aria-live=\"polite\"></section>");
            html.AppendLine("    </main>");
            html.AppendLine("  </div>");
            html.AppendLine($"  <script id=\"spider-manifest-data\" type=\"application/json\">{manifestJson}</script>");
            html.AppendLine("  <script>");
            html.AppendLine(CreateScript());
            html.AppendLine("  </script>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        /// <summary>
        /// Converts a Boolean value to an HTML data attribute value.
        /// </summary>
        /// <param name="value">The Boolean value.</param>
        /// <returns>The encoded attribute value.</returns>
        private string BooleanAttribute(bool value)
            => value ? "true" : "false";

        /// <summary>
        /// Creates the stylesheet used by the documentation shell.
        /// </summary>
        /// <returns>The stylesheet text.</returns>
        private string CreateStyles()
            => """
* {
  box-sizing: border-box;
}

:root {
  color-scheme: light;
  --spider-bg: #f5f7fb;
  --spider-panel: #ffffff;
  --spider-panel-soft: #f9fafb;
  --spider-line: #d8e0eb;
  --spider-line-strong: #b8c5d6;
  --spider-text: #142033;
  --spider-muted: #65748a;
  --spider-blue: #255fda;
  --spider-blue-soft: #e8f0ff;
  --spider-green: #0f8a73;
  --spider-green-soft: #e7f7f3;
  --spider-amber: #a15d00;
  --spider-amber-soft: #fff2d8;
  --spider-purple: #6c3fc5;
  --spider-purple-soft: #f1ebff;
  --spider-red: #b23b3b;
  --spider-red-soft: #ffeaea;
  --spider-shadow: 0 16px 40px rgba(20, 32, 51, 0.08);
  font-family: Inter, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
}

html,
body {
  min-height: 100%;
  margin: 0;
  background: var(--spider-bg);
  color: var(--spider-text);
}

button,
input {
  font: inherit;
}

.spider-shell {
  display: grid;
  grid-template-columns: minmax(280px, 320px) minmax(0, 1fr);
  min-height: 100vh;
}

.spider-sidebar {
  position: sticky;
  top: 0;
  height: 100vh;
  overflow-y: auto;
  border-right: 1px solid var(--spider-line);
  background: rgba(255, 255, 255, 0.96);
  padding: 20px 18px;
}

.spider-brand {
  display: grid;
  grid-template-columns: 38px minmax(0, 1fr);
  align-items: center;
  gap: 12px;
  margin-bottom: 22px;
}

.spider-logo {
  display: grid;
  width: 38px;
  height: 38px;
  place-items: center;
  border-radius: 10px;
  background: #172033;
  color: #ffffff;
  font-weight: 800;
}

.spider-title {
  overflow: hidden;
  color: var(--spider-text);
  font-weight: 800;
  line-height: 1.15;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.spider-subtitle {
  margin-top: 3px;
  color: var(--spider-muted);
  font-size: 0.83rem;
}

.spider-search {
  display: grid;
  gap: 7px;
  margin-bottom: 18px;
  color: var(--spider-muted);
  font-size: 0.78rem;
  font-weight: 700;
  text-transform: uppercase;
}

.spider-search input {
  width: 100%;
  min-width: 0;
  border: 1px solid var(--spider-line);
  border-radius: 9px;
  background: #ffffff;
  color: var(--spider-text);
  outline: none;
  padding: 10px 11px;
  text-transform: none;
}

.spider-search input:focus {
  border-color: var(--spider-blue);
  box-shadow: 0 0 0 3px rgba(37, 95, 218, 0.12);
}

[data-show-search="false"] [data-search-region] {
  display: none;
}

.spider-navigation {
  display: grid;
  gap: 18px;
}

.spider-nav-section {
  display: grid;
  gap: 8px;
}

.spider-nav-heading {
  display: flex;
  align-items: center;
  justify-content: space-between;
  color: var(--spider-muted);
  font-size: 0.78rem;
  font-weight: 800;
  letter-spacing: 0;
  text-transform: uppercase;
}

.spider-nav-heading strong {
  display: inline-flex;
  min-width: 28px;
  justify-content: center;
  border: 1px solid var(--spider-line);
  border-radius: 999px;
  background: var(--spider-panel-soft);
  color: var(--spider-text);
  font-size: 0.76rem;
  padding: 2px 8px;
}

.spider-nav-list {
  display: grid;
  gap: 7px;
}

.spider-nav-item {
  width: 100%;
  border: 1px solid transparent;
  border-radius: 9px;
  background: transparent;
  color: inherit;
  cursor: pointer;
  padding: 10px;
  text-align: left;
}

.spider-nav-item:hover {
  border-color: var(--spider-line);
  background: var(--spider-panel-soft);
}

.spider-nav-item.is-active {
  border-color: rgba(37, 95, 218, 0.45);
  background: var(--spider-blue-soft);
  box-shadow: inset 3px 0 0 var(--spider-blue);
}

.spider-nav-title {
  display: block;
  overflow-wrap: anywhere;
  color: var(--spider-text);
  font-size: 0.94rem;
  font-weight: 750;
  line-height: 1.25;
}

.spider-nav-meta {
  display: block;
  margin-top: 5px;
  overflow-wrap: anywhere;
  color: var(--spider-muted);
  font-size: 0.78rem;
  line-height: 1.35;
}

.spider-empty-list {
  border: 1px dashed var(--spider-line);
  border-radius: 9px;
  color: var(--spider-muted);
  font-size: 0.84rem;
  padding: 11px;
}

.spider-json-link {
  display: none;
  width: 100%;
  margin-top: 22px;
  border: 1px solid var(--spider-line);
  border-radius: 9px;
  background: #ffffff;
  color: var(--spider-text);
  cursor: pointer;
  padding: 10px 12px;
  text-align: center;
}

[data-show-json="true"] .spider-json-link {
  display: block;
}

.spider-main {
  min-width: 0;
  overflow-y: auto;
  padding: 28px;
}

.spider-detail {
  width: min(100%, 1120px);
}

.spider-empty-state,
.spider-json-view,
.spider-detail-shell {
  display: grid;
  gap: 18px;
}

.spider-empty-state {
  min-height: calc(100vh - 56px);
  align-content: center;
  justify-items: start;
}

.spider-empty-kicker,
.spider-chip,
.spider-node-kind {
  display: inline-flex;
  align-items: center;
  width: fit-content;
  border-radius: 999px;
  font-size: 0.78rem;
  font-weight: 800;
  line-height: 1;
  padding: 6px 9px;
}

.spider-empty-kicker {
  background: var(--spider-blue-soft);
  color: var(--spider-blue);
}

.spider-empty-state h1,
.spider-detail-header h1 {
  margin: 0;
  color: var(--spider-text);
  font-size: clamp(1.8rem, 3vw, 2.75rem);
  line-height: 1.05;
}

.spider-empty-state p,
.spider-detail-header p {
  max-width: 680px;
  margin: 0;
  color: var(--spider-muted);
  font-size: 1rem;
  line-height: 1.6;
}

.spider-counts {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}

.spider-count {
  border: 1px solid var(--spider-line);
  border-radius: 10px;
  background: var(--spider-panel);
  padding: 10px 12px;
}

.spider-count strong {
  display: block;
  color: var(--spider-text);
  font-size: 1.1rem;
}

.spider-count span {
  color: var(--spider-muted);
  font-size: 0.78rem;
}

.spider-detail-header {
  display: grid;
  gap: 12px;
  margin-bottom: 22px;
}

.spider-header-row {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px;
}

.spider-chip {
  background: var(--spider-blue-soft);
  color: var(--spider-blue);
}

.spider-chip.flow {
  background: var(--spider-green-soft);
  color: var(--spider-green);
}

.spider-chip.pipeline {
  background: var(--spider-blue-soft);
  color: var(--spider-blue);
}

.spider-chip.profile {
  background: var(--spider-purple-soft);
  color: var(--spider-purple);
}

.spider-content-grid {
  display: grid;
  gap: 16px;
}

.spider-panel {
  min-width: 0;
  border: 1px solid var(--spider-line);
  border-radius: 12px;
  background: var(--spider-panel);
  box-shadow: var(--spider-shadow);
  padding: 18px;
}

.spider-panel-header {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 12px;
}

.spider-panel h2 {
  margin: 0;
  color: var(--spider-text);
  font-size: 1rem;
}

.spider-panel-note {
  color: var(--spider-muted);
  font-size: 0.84rem;
}

.spider-definition {
  display: grid;
  grid-template-columns: minmax(120px, 0.3fr) minmax(0, 1fr);
  gap: 8px 14px;
  margin: 0;
}

.spider-definition dt {
  color: var(--spider-muted);
  font-size: 0.84rem;
}

.spider-definition dd {
  min-width: 0;
  margin: 0;
  overflow-wrap: anywhere;
  color: var(--spider-text);
  font-size: 0.9rem;
}

.spider-profile-row {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-top: 14px;
}

.spider-evidence {
  margin-top: 16px;
  border-top: 1px solid var(--spider-line);
  padding-top: 14px;
}

[data-show-evidence="false"] .spider-evidence {
  display: none;
}

.spider-outline {
  display: grid;
  gap: 10px;
  margin: 0;
  padding: 0;
  list-style: none;
}

.spider-outline-row {
  display: grid;
  grid-template-columns: 34px minmax(0, 1fr);
  gap: 12px;
  align-items: start;
  border: 1px solid var(--spider-line);
  border-radius: 10px;
  background: var(--spider-panel-soft);
  padding: 12px;
}

.spider-step-number {
  display: grid;
  width: 34px;
  height: 34px;
  place-items: center;
  border-radius: 9px;
  background: #ffffff;
  color: var(--spider-muted);
  font-size: 0.78rem;
  font-weight: 850;
}

.spider-outline-title {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px;
  color: var(--spider-text);
  font-weight: 800;
  line-height: 1.3;
}

.spider-outline-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  margin-top: 8px;
}

.spider-meta-pill {
  max-width: 100%;
  border: 1px solid var(--spider-line);
  border-radius: 999px;
  background: #ffffff;
  color: var(--spider-muted);
  font-size: 0.76rem;
  line-height: 1.25;
  overflow-wrap: anywhere;
  padding: 5px 8px;
}

.spider-kind-step,
.spider-kind-stage {
  background: var(--spider-green-soft);
  color: var(--spider-green);
}

.spider-kind-condition {
  background: var(--spider-amber-soft);
  color: var(--spider-amber);
}

.spider-kind-branch {
  background: var(--spider-purple-soft);
  color: var(--spider-purple);
}

.spider-graph-panel {
  overflow: visible;
}

.spider-process-graph {
  width: 100%;
  overflow: visible;
}

.spider-architecture-graph {
  display: block;
  width: 100%;
  height: auto;
  overflow: visible;
}

.spider-edge {
  stroke: #8da0b8;
  stroke-width: 2;
}

.spider-node rect {
  fill: #ffffff;
  stroke: var(--spider-line-strong);
  stroke-width: 1.4;
}

.spider-node.is-root rect {
  fill: var(--spider-blue-soft);
  stroke: var(--spider-blue);
}

.spider-node.is-flow rect {
  fill: var(--spider-green-soft);
  stroke: var(--spider-green);
}

.spider-node.is-condition rect {
  fill: var(--spider-amber-soft);
  stroke: var(--spider-amber);
}

.spider-node.is-branch rect,
.spider-node.is-profile rect {
  fill: var(--spider-purple-soft);
  stroke: var(--spider-purple);
}

.spider-node text {
  fill: var(--spider-text);
  font-family: Inter, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
}

.spider-node .spider-node-subtitle {
  fill: var(--spider-muted);
}

.spider-json {
  overflow: auto;
  max-height: calc(100vh - 150px);
  margin: 0;
  border: 1px solid var(--spider-line);
  border-radius: 12px;
  background: #101827;
  color: #edf3ff;
  font-size: 0.84rem;
  line-height: 1.5;
  padding: 18px;
}

.spider-hidden {
  display: none !important;
}

@media (max-width: 880px) {
  .spider-shell {
    grid-template-columns: 1fr;
  }

  .spider-sidebar {
    position: relative;
    height: auto;
    max-height: none;
    border-right: 0;
    border-bottom: 1px solid var(--spider-line);
  }

  .spider-main {
    padding: 20px;
  }
}

@media (max-width: 560px) {
  .spider-main {
    padding: 16px;
  }

  .spider-definition {
    grid-template-columns: 1fr;
    gap: 4px;
  }

  .spider-outline-row {
    grid-template-columns: 1fr;
  }
}
""";

        /// <summary>
        /// Creates the client-side script used to navigate the manifest.
        /// </summary>
        /// <returns>The script text.</returns>
        private string CreateScript()
            => """
(() => {
  const root = document.getElementById("spider-documentation-app");
  const manifestElement = document.getElementById("spider-manifest-data");
  const manifest = JSON.parse(manifestElement.textContent || "{}");
  const components = manifest.components || [];
  const relations = manifest.relations || [];
  const byId = new Map(components.map((component) => [component.id, component]));
  const detail = document.getElementById("spider-detail");
  const flowList = document.getElementById("spider-flow-list");
  const pipelineList = document.getElementById("spider-pipeline-list");
  const flowCount = document.getElementById("spider-flow-count");
  const pipelineCount = document.getElementById("spider-pipeline-count");
  const searchInput = document.getElementById("spider-search");
  const jsonLink = document.getElementById("spider-json-link");
  const showGraph = root.dataset.showGraph === "true";
  const showJson = root.dataset.showJson === "true";
  const showSearch = root.dataset.showSearch === "true";
  const flows = components.filter((component) => component.kind === "spider.flow").sort(compareByName);
  const pipelines = components.filter((component) => component.kind === "spider.pipeline").sort(compareByName);
  let selectedId = "";
  let query = "";

  if (!showSearch && searchInput) {
    searchInput.value = "";
  }

  if (!showJson && jsonLink) {
    jsonLink.classList.add("spider-hidden");
  }

  if (searchInput) {
    searchInput.addEventListener("input", () => {
      query = searchInput.value.trim().toLowerCase();
      renderNavigation();
    });
  }

  if (jsonLink) {
    jsonLink.addEventListener("click", () => {
      selectedId = "__json";
      renderNavigation();
      renderJson();
    });
  }

  document.addEventListener("click", (event) => {
    const item = event.target.closest("[data-select-id]");
    if (!item) {
      return;
    }

    openProcess(item.getAttribute("data-select-id"));
  });

  window.addEventListener("hashchange", openFromHash);

  renderNavigation();
  openFromHash();

  if (!selectedId) {
    renderEmpty();
  }

  function openFromHash() {
    const id = decodeURIComponent(window.location.hash.replace(/^#\/?/, ""));
    if (!id || !byId.has(id)) {
      return;
    }

    openProcess(id, true);
  }

  function openProcess(id, skipHash) {
    const component = byId.get(id);
    if (!component) {
      return;
    }

    selectedId = id;
    if (!skipHash) {
      window.history.replaceState(null, "", "#" + encodeURIComponent(id));
    }

    renderNavigation();
    renderDetail(component);
    resetDetailScroll();
  }

  function renderNavigation() {
    renderNavigationList(pipelineList, pipelines, "pipelines");
    renderNavigationList(flowList, flows, "flows");
    pipelineCount.textContent = String(filterProcesses(pipelines).length);
    flowCount.textContent = String(filterProcesses(flows).length);
  }

  function renderNavigationList(container, source, emptyLabel) {
    const items = filterProcesses(source);

    if (items.length === 0) {
      container.innerHTML = `<div class="spider-empty-list">No ${escapeHtml(emptyLabel)} found.</div>`;
      return;
    }

    container.innerHTML = items.map((item) => {
      const children = orderChildren(item);
      const signature = getSignature(item);
      const countLabel = item.kind === "spider.pipeline"
        ? `${children.length} stages`
        : `${children.length} steps`;
      const activeClass = item.id === selectedId ? " is-active" : "";

      return `
        <button class="spider-nav-item${activeClass}" type="button" data-select-id="${escapeAttribute(item.id)}">
          <span class="spider-nav-title">${escapeHtml(item.displayName || item.id)}</span>
          <span class="spider-nav-meta">${escapeHtml(signature)} - ${escapeHtml(countLabel)}</span>
        </button>`;
    }).join("");
  }

  function filterProcesses(items) {
    if (!query) {
      return items;
    }

    return items.filter((item) => createSearchText(item).includes(query));
  }

  function renderEmpty() {
    detail.innerHTML = `
      <div class="spider-empty-state">
        <span class="spider-empty-kicker">Spider</span>
        <h1>Architecture map</h1>
        <p>Select a flow or pipeline from the side menu.</p>
        <div class="spider-counts">
          <div class="spider-count"><strong>${pipelines.length}</strong><span>Pipelines</span></div>
          <div class="spider-count"><strong>${flows.length}</strong><span>Flows</span></div>
        </div>
      </div>`;
  }

  function renderJson() {
    detail.innerHTML = `
      <div class="spider-json-view">
        <header class="spider-detail-header">
          <div class="spider-header-row">
            <span class="spider-chip">Manifest</span>
          </div>
          <h1>Manifest JSON</h1>
        </header>
        <pre class="spider-json">${escapeHtml(JSON.stringify(manifest, null, 2))}</pre>
      </div>`;
  }

  function renderDetail(component) {
    const children = orderChildren(component);
    const profiles = getRelated(component.id, "uses-profile");
    const graph = showGraph
      ? `<section class="spider-panel spider-graph-panel">
          <div class="spider-panel-header">
            <h2>Graph</h2>
            <span class="spider-panel-note">Top to bottom</span>
          </div>
          <div class="spider-process-graph">${renderVerticalGraph(component, children)}</div>
        </section>`
      : "";

    detail.innerHTML = `
      <article class="spider-detail-shell">
        <header class="spider-detail-header">
          <div class="spider-header-row">
            <span class="spider-chip ${component.kind === "spider.flow" ? "flow" : "pipeline"}">${escapeHtml(getProcessKind(component))}</span>
            <span class="spider-chip">${escapeHtml(getSignature(component))}</span>
          </div>
          <h1>${escapeHtml(component.displayName || component.id)}</h1>
          <p>${escapeHtml(getProcessSummary(component, children))}</p>
        </header>
        <div class="spider-content-grid">
          ${renderDetailsPanel(component, profiles)}
          ${renderSequencePanel(component, children)}
          ${graph}
        </div>
      </article>`;
  }

  function resetDetailScroll() {
    const main = root.querySelector(".spider-main");
    if (main) {
      main.scrollTop = 0;
    }

    window.scrollTo(0, 0);
  }

  function renderDetailsPanel(component, profiles) {
    const metadataRows = Object.entries(component.metadata || {})
      .filter(([key]) => key !== "request" && key !== "response" && key !== "hasResponse")
      .map(([key, value]) => `<dt>${escapeHtml(formatLabel(key))}</dt><dd>${escapeHtml(value)}</dd>`)
      .join("");
    const baseRows = [
      ["Id", component.id],
      ["Request", getMetadata(component, "request") || "Not declared"],
      ["Response", getMetadata(component, "response") || (getMetadata(component, "hasResponse") === "false" ? "No response" : "Not declared")]
    ].map(([label, value]) => `<dt>${escapeHtml(label)}</dt><dd>${escapeHtml(value)}</dd>`).join("");
    const profileRows = profiles.length === 0
      ? ""
      : `<div class="spider-profile-row">${profiles.map((profile) => `<span class="spider-chip profile">${escapeHtml(profile.displayName || profile.id)}</span>`).join("")}</div>`;
    const evidence = renderEvidence(component);

    return `
      <section class="spider-panel">
        <div class="spider-panel-header">
          <h2>Details</h2>
        </div>
        <dl class="spider-definition">${baseRows}${metadataRows}</dl>
        ${profileRows}
        ${evidence}
      </section>`;
  }

  function renderEvidence(component) {
    const evidence = component.evidence || [];
    if (evidence.length === 0) {
      return "";
    }

    const first = evidence[0];
    const source = first.filePath
      ? `${first.filePath}${first.lineNumber ? ":" + first.lineNumber : ""}`
      : "Not available";

    return `
      <div class="spider-evidence">
        <dl class="spider-definition">
          <dt>Member</dt><dd>${escapeHtml(first.memberName || "Not available")}</dd>
          <dt>Source</dt><dd>${escapeHtml(source)}</dd>
        </dl>
      </div>`;
  }

  function renderSequencePanel(component, children) {
    const label = component.kind === "spider.pipeline" ? "Stages" : "Steps";
    const rows = children.length === 0
      ? `<div class="spider-empty-list">No ${label.toLowerCase()} discovered.</div>`
      : `<ol class="spider-outline">${children.map(renderSequenceRow).join("")}</ol>`;

    return `
      <section class="spider-panel">
        <div class="spider-panel-header">
          <h2>${escapeHtml(label)}</h2>
          <span class="spider-panel-note">${children.length} total</span>
        </div>
        ${rows}
      </section>`;
  }

  function renderSequenceRow(child, index) {
    const pills = createMetadataPills(child);
    return `
      <li class="spider-outline-row">
        <span class="spider-step-number">${String(index + 1).padStart(2, "0")}</span>
        <div>
          <div class="spider-outline-title">
            <span>${escapeHtml(child.displayName || child.id)}</span>
            <span class="spider-node-kind ${getKindClass(child)}">${escapeHtml(getChildKind(child))}</span>
          </div>
          ${pills ? `<div class="spider-outline-meta">${pills}</div>` : ""}
        </div>
      </li>`;
  }

  function renderVerticalGraph(component, children) {
    const nodes = [component].concat(children);
    const width = 820;
    const nodeWidth = 560;
    const nodeHeight = 68;
    const nodeX = 130;
    const top = 24;
    const gap = 104;
    const height = Math.max(210, top + nodes.length * gap);
    const center = nodeX + (nodeWidth / 2);
    const edges = [];
    const renderedNodes = [];

    for (let index = 0; index < nodes.length; index++) {
      const node = nodes[index];
      const y = top + (index * gap);
      const className = index === 0 ? "is-root" : getGraphClass(node);
      const subtitle = index === 0 ? getSignature(node) : getChildSubtitle(node);

      renderedNodes.push(`
        <g class="spider-node ${className}" transform="translate(${nodeX}, ${y})">
          <title>${escapeHtml(node.displayName || node.id)}</title>
          <rect width="${nodeWidth}" height="${nodeHeight}" rx="12" />
          <text x="22" y="30" font-size="15" font-weight="800">${escapeHtml(truncate(node.displayName || node.id, 56))}</text>
          <text class="spider-node-subtitle" x="22" y="51" font-size="12">${escapeHtml(truncate(subtitle, 66))}</text>
        </g>`);

      if (index < nodes.length - 1) {
        const y1 = y + nodeHeight;
        const y2 = y + gap;
        edges.push(`<path class="spider-edge" d="M ${center} ${y1} L ${center} ${y2 - 8}" marker-end="url(#spider-arrow)" />`);
      }
    }

    return `
      <svg class="spider-architecture-graph" viewBox="0 0 ${width} ${height}" role="img" aria-label="${escapeAttribute(component.displayName || "Spider process graph")}" preserveAspectRatio="xMidYMin meet">
        <defs>
          <marker id="spider-arrow" viewBox="0 0 10 10" refX="8" refY="5" markerWidth="6" markerHeight="6" orient="auto-start-reverse">
            <path d="M 0 0 L 10 5 L 0 10 z" fill="#8da0b8"></path>
          </marker>
        </defs>
        ${edges.join("")}
        ${renderedNodes.join("")}
      </svg>`;
  }

  function orderChildren(component) {
    const children = relations
      .filter((relation) => relation.kind === "contains" && relation.sourceId === component.id)
      .map((relation) => byId.get(relation.targetId))
      .filter(Boolean);

    if (children.length <= 1) {
      return children;
    }

    const allHaveOrder = children.every((child) => Number.isFinite(Number(getMetadata(child, "order"))));
    if (allHaveOrder) {
      return children.slice().sort((left, right) => Number(getMetadata(left, "order")) - Number(getMetadata(right, "order")));
    }

    const ids = new Set(children.map((child) => child.id));
    const next = relations.filter((relation) => relation.kind === "next" && ids.has(relation.sourceId) && ids.has(relation.targetId));
    if (next.length === 0) {
      return children;
    }

    const targets = new Set(next.map((relation) => relation.targetId));
    const bySource = new Map(next.map((relation) => [relation.sourceId, relation.targetId]));
    const ordered = [];
    let current = children.find((child) => !targets.has(child.id)) || children[0];

    while (current && !ordered.some((item) => item.id === current.id)) {
      ordered.push(current);
      const nextId = bySource.get(current.id);
      current = nextId ? byId.get(nextId) : null;
    }

    for (const child of children) {
      if (!ordered.some((item) => item.id === child.id)) {
        ordered.push(child);
      }
    }

    return ordered;
  }

  function getRelated(sourceId, kind) {
    return relations
      .filter((relation) => relation.kind === kind && relation.sourceId === sourceId)
      .map((relation) => byId.get(relation.targetId))
      .filter(Boolean)
      .sort(compareByName);
  }

  function createMetadataPills(component) {
    return Object.entries(component.metadata || {})
      .filter(([key]) => key !== "order")
      .map(([key, value]) => `<span class="spider-meta-pill">${escapeHtml(formatLabel(key))}: ${escapeHtml(value)}</span>`)
      .join("");
  }

  function createSearchText(component) {
    const values = [
      component.id,
      component.kind,
      component.displayName,
      ...Object.values(component.metadata || {})
    ];

    for (const child of orderChildren(component)) {
      values.push(child.id, child.kind, child.displayName, ...Object.values(child.metadata || {}));
    }

    return values.filter(Boolean).join(" ").toLowerCase();
  }

  function getSignature(component) {
    const request = shortName(getMetadata(component, "request"));
    const response = shortName(getMetadata(component, "response"));

    if (request && response) {
      return `${request} -> ${response}`;
    }

    if (request && getMetadata(component, "hasResponse") === "false") {
      return `${request} -> no response`;
    }

    return request || response || "No signature";
  }

  function getProcessSummary(component, children) {
    const noun = component.kind === "spider.pipeline" ? "pipeline" : "flow";
    const childNoun = component.kind === "spider.pipeline" ? "stage" : "step";
    return `${capitalize(noun)} with ${children.length} ${childNoun}${children.length === 1 ? "" : "s"}.`;
  }

  function getChildSubtitle(component) {
    if (component.kind === "spider.pipeline-stage") {
      const count = getMetadata(component, "count") || "0";
      return `${count} configured action${count === "1" ? "" : "s"}`;
    }

    return getMetadata(component, "delegate") || getMetadata(component, "operation") || getChildKind(component);
  }

  function getProcessKind(component) {
    return component.kind === "spider.pipeline" ? "Pipeline" : "Flow";
  }

  function getChildKind(component) {
    if (component.kind === "spider.pipeline-stage") {
      return "Stage";
    }

    if (component.kind === "spider.flow-condition") {
      return "Condition";
    }

    if (component.kind === "spider.flow-branch") {
      return "Branch";
    }

    return "Step";
  }

  function getKindClass(component) {
    if (component.kind === "spider.flow-condition") {
      return "spider-kind-condition";
    }

    if (component.kind === "spider.flow-branch") {
      return "spider-kind-branch";
    }

    return component.kind === "spider.pipeline-stage" ? "spider-kind-stage" : "spider-kind-step";
  }

  function getGraphClass(component) {
    if (component.kind === "spider.flow-condition") {
      return "is-condition";
    }

    if (component.kind === "spider.flow-branch") {
      return "is-branch";
    }

    if (component.kind === "spider.flow") {
      return "is-flow";
    }

    return "";
  }

  function getMetadata(component, key) {
    return component && component.metadata ? component.metadata[key] || "" : "";
  }

  function compareByName(left, right) {
    return (left.displayName || left.id).localeCompare(right.displayName || right.id);
  }

  function shortName(value) {
    if (!value) {
      return "";
    }

    const index = value.lastIndexOf(".");
    return index < 0 ? value : value.substring(index + 1);
  }

  function formatLabel(value) {
    return String(value || "")
      .replace(/([a-z0-9])([A-Z])/g, "$1 $2")
      .replace(/[-_]+/g, " ")
      .replace(/\b\w/g, (letter) => letter.toUpperCase());
  }

  function capitalize(value) {
    return value.charAt(0).toUpperCase() + value.slice(1);
  }

  function truncate(value, length) {
    const text = String(value || "");
    return text.length > length ? text.slice(0, Math.max(0, length - 1)) + "..." : text;
  }

  function escapeHtml(value) {
    return String(value ?? "")
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#39;");
  }

  function escapeAttribute(value) {
    return escapeHtml(value);
  }
})();
""";
    }
}
