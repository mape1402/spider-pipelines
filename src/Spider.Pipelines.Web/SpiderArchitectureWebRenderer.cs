namespace Spider.Pipelines.Web
{
    using System.Text;
    using System.Text.Encodings.Web;
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
            html.AppendLine("          <div class=\"spider-subtitle\">Architecture</div>");
            html.AppendLine("        </div>");
            html.AppendLine("      </div>");
            html.AppendLine("      <nav class=\"spider-menu\" aria-label=\"Architecture sections\">");
            html.AppendLine("        <button class=\"spider-menu-item\" type=\"button\" data-menu-view=\"pipelines\"><span>Pipelines</span><strong id=\"spider-pipeline-count\">0</strong></button>");
            html.AppendLine("        <button class=\"spider-menu-item\" type=\"button\" data-menu-view=\"flows\"><span>Flows</span><strong id=\"spider-flow-count\">0</strong></button>");
            html.AppendLine("      </nav>");
            html.AppendLine("      <button id=\"spider-json-link\" class=\"spider-json-link\" type=\"button\">Manifest JSON</button>");
            html.AppendLine("    </aside>");
            html.AppendLine("    <section class=\"spider-workspace\">");
            html.AppendLine("      <header class=\"spider-topbar\">");
            html.AppendLine("        <div class=\"spider-topbar-left\">");
            html.AppendLine("          <div id=\"spider-topbar-title\" class=\"spider-topbar-title\">Pipelines</div>");
            html.AppendLine("        </div>");
            html.AppendLine("        <div class=\"spider-topbar-badge\">Generated metadata</div>");
            html.AppendLine("      </header>");
            html.AppendLine("      <main class=\"spider-main\">");
            html.AppendLine("        <section id=\"spider-content\" class=\"spider-content\" aria-live=\"polite\"></section>");
            html.AppendLine("      </main>");
            html.AppendLine("    </section>");
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
  --spider-sidebar-width: 244px;
  --spider-sidebar-bg: #111827;
  --spider-sidebar-brand: #0b1020;
  --spider-sidebar-border: #252b3a;
  --spider-sidebar-text: #e5e7eb;
  --spider-sidebar-muted: #9ca3af;
  --spider-sidebar-hover: rgba(230, 36, 45, 0.1);
  --spider-sidebar-active: rgba(230, 36, 45, 0.17);
  --spider-sidebar-active-text: #ffd5d8;
  --spider-sidebar-active-border: #e6242d;
  --spider-topbar-height: 52px;
  --spider-bg: #f5f6fa;
  --spider-panel: #ffffff;
  --spider-panel-soft: #f8fafc;
  --spider-line: #d8dee8;
  --spider-line-strong: #c4ccda;
  --spider-text: #111827;
  --spider-muted: #64748b;
  --spider-black: #111827;
  --spider-red: #e6242d;
  --spider-red-strong: #bd1018;
  --spider-red-soft: #fff0f1;
  --spider-blue: #1d5fbf;
  --spider-blue-strong: #174c99;
  --spider-blue-soft: #eef5ff;
  --spider-primary: var(--spider-red);
  --spider-primary-strong: var(--spider-red-strong);
  --spider-primary-soft: var(--spider-red-soft);
  --spider-green: var(--spider-blue);
  --spider-green-soft: var(--spider-blue-soft);
  --spider-amber: var(--spider-red-strong);
  --spider-amber-soft: var(--spider-red-soft);
  --spider-purple: var(--spider-blue-strong);
  --spider-purple-soft: var(--spider-blue-soft);
  --spider-shadow: 0 10px 28px rgba(17, 24, 39, 0.08);
  --spider-radius: 8px;
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", "Noto Sans", Helvetica, Arial, sans-serif;
}

html,
body {
  height: 100%;
  margin: 0;
  padding: 0;
  background: var(--spider-bg);
  color: var(--spider-text);
  font-size: 0.875rem;
  line-height: 1.6;
  -webkit-font-smoothing: antialiased;
}

button,
input {
  font: inherit;
}

button {
  color: inherit;
}

.spider-shell {
  display: flex;
  height: 100vh;
  overflow: hidden;
  background: var(--spider-bg);
}

.spider-sidebar {
  display: flex;
  flex: 0 0 var(--spider-sidebar-width);
  flex-direction: column;
  width: var(--spider-sidebar-width);
  height: 100vh;
  overflow: hidden;
  border-right: 1px solid var(--spider-sidebar-border);
  background: var(--spider-sidebar-bg);
}

.spider-brand {
  display: flex;
  align-items: center;
  height: var(--spider-topbar-height);
  flex: 0 0 var(--spider-topbar-height);
  gap: 10px;
  overflow: hidden;
  border-bottom: 1px solid var(--spider-sidebar-border);
  background: var(--spider-sidebar-brand);
  padding: 0 18px;
}

.spider-logo {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  flex: 0 0 auto;
  border-radius: 7px;
  background: linear-gradient(135deg, var(--spider-red), var(--spider-blue));
  color: #ffffff;
  font-size: 0.86rem;
  font-weight: 800;
}

.spider-title {
  overflow: hidden;
  color: #e0e2f0;
  font-size: 0.94rem;
  font-weight: 650;
  line-height: 1.2;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.spider-subtitle {
  margin-top: 2px;
  color: var(--spider-sidebar-muted);
  font-size: 0.78rem;
}

.spider-menu {
  display: grid;
  flex: 1 1 auto;
  gap: 6px;
  align-content: start;
  grid-auto-rows: max-content;
  overflow-y: auto;
  padding: 10px 0;
}

.spider-menu-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: calc(100% - 8px);
  border: 1px solid transparent;
  border-left: 2px solid transparent;
  border-radius: 0 var(--spider-radius) var(--spider-radius) 0;
  background: transparent;
  color: var(--spider-sidebar-text);
  cursor: pointer;
  font-size: 0.875rem;
  font-weight: 500;
  margin-right: 8px;
  padding: 7px 14px 7px 18px;
  text-align: left;
  transition: background 0.12s, border-color 0.12s, color 0.12s;
}

.spider-menu-item:hover {
  background: var(--spider-sidebar-hover);
  border-left-color: var(--spider-sidebar-border);
  color: #e0e2f0;
}

.spider-menu-item.is-active {
  background: var(--spider-sidebar-active);
  border-left-color: var(--spider-sidebar-active-border);
  color: var(--spider-sidebar-active-text);
}

.spider-menu-item strong {
  color: currentColor;
  font-size: 0.75rem;
  font-weight: 600;
  opacity: 0.8;
}

.spider-json-link {
  display: none;
  width: calc(100% - 28px);
  margin: 0 14px 14px;
  border: 1px solid var(--spider-sidebar-border);
  border-radius: var(--spider-radius);
  background: transparent;
  color: var(--spider-sidebar-muted);
  cursor: pointer;
  font-size: 0.85rem;
  padding: 8px 10px;
  text-align: center;
}

.spider-json-link:hover {
  background: var(--spider-sidebar-hover);
  color: #e0e2f0;
}

[data-show-json="true"] .spider-json-link {
  display: block;
}

.spider-workspace {
  display: flex;
  flex: 1 1 auto;
  flex-direction: column;
  min-width: 0;
  overflow: hidden;
}

.spider-topbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  height: var(--spider-topbar-height);
  flex: 0 0 var(--spider-topbar-height);
  gap: 12px;
  border-bottom: 1px solid #e2e3ef;
  background: #ffffff;
  padding: 0 16px;
}

.spider-topbar-left {
  min-width: 0;
}

.spider-topbar-title {
  overflow: hidden;
  color: var(--spider-text);
  font-size: 0.95rem;
  font-weight: 650;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.spider-topbar-badge {
  display: inline-flex;
  align-items: center;
  border: 1px solid rgba(29, 95, 191, 0.26);
  border-radius: 999px;
  background: var(--spider-blue-soft);
  color: var(--spider-blue);
  font-size: 0.75rem;
  font-weight: 500;
  line-height: 1.4;
  padding: 3px 10px;
}

.spider-main {
  flex: 1 1 auto;
  min-width: 0;
  overflow-y: auto;
  padding: 24px 28px;
}

.spider-content {
  width: min(100%, 1180px);
}

.spider-view-header,
.spider-detail-toolbar {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 16px;
}

.spider-view-title,
.spider-detail-title {
  margin: 0;
  color: var(--spider-text);
  font-size: 1.28rem;
  line-height: 1.25;
}

.spider-view-description,
.spider-detail-subtitle {
  margin: 4px 0 0;
  color: var(--spider-muted);
  font-size: 0.86rem;
  line-height: 1.45;
}

.spider-search {
  width: min(280px, 34vw);
  min-width: 190px;
}

.spider-search input {
  width: 100%;
  border: 1px solid var(--spider-line);
  border-radius: 8px;
  background: #ffffff;
  color: var(--spider-text);
  outline: none;
  padding: 8px 10px;
}

.spider-search input:focus {
  border-color: var(--spider-blue);
  box-shadow: 0 0 0 3px rgba(29, 95, 191, 0.12);
}

[data-show-search="false"] .spider-search {
  display: none;
}

.spider-process-list {
  display: grid;
  gap: 9px;
}

.spider-process-row {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  align-items: center;
  gap: 16px;
  width: 100%;
  border: 1px solid var(--spider-line);
  border-radius: 8px;
  background: #ffffff;
  cursor: pointer;
  padding: 13px 14px;
  text-align: left;
}

.spider-process-row:hover {
  border-color: var(--spider-line-strong);
  box-shadow: var(--spider-shadow);
}

.spider-process-name {
  overflow-wrap: anywhere;
  color: var(--spider-text);
  font-size: 0.95rem;
  font-weight: 750;
  line-height: 1.3;
}

.spider-process-meta {
  margin-top: 4px;
  color: var(--spider-muted);
  font-size: 0.8rem;
  line-height: 1.35;
}

.spider-count-pill,
.spider-chip,
.spider-node-kind {
  display: inline-flex;
  align-items: center;
  width: fit-content;
  border-radius: 999px;
  font-size: 0.75rem;
  font-weight: 750;
  line-height: 1;
  padding: 5px 8px;
}

.spider-count-pill {
  background: var(--spider-panel-soft);
  border: 1px solid var(--spider-line);
  color: var(--spider-muted);
}

.spider-chip {
  background: var(--spider-red-soft);
  color: var(--spider-red);
}

.spider-chip.flow,
.spider-kind-step,
.spider-kind-stage {
  background: var(--spider-blue-soft);
  color: var(--spider-blue);
}

.spider-chip.pipeline {
  background: var(--spider-red-soft);
  color: var(--spider-red);
}

.spider-kind-condition {
  background: var(--spider-amber-soft);
  color: var(--spider-amber);
}

.spider-kind-branch,
.spider-chip.profile {
  background: var(--spider-purple-soft);
  color: var(--spider-purple);
}

.spider-empty-list,
.spider-empty-state {
  border: 1px dashed var(--spider-line);
  border-radius: 8px;
  background: #ffffff;
  color: var(--spider-muted);
  font-size: 0.88rem;
  padding: 18px;
}

.spider-back-button {
  border: 1px solid var(--spider-line);
  border-radius: 8px;
  background: #ffffff;
  cursor: pointer;
  font-size: 0.84rem;
  padding: 8px 10px;
}

.spider-back-button:hover {
  border-color: var(--spider-line-strong);
}

.spider-detail-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 7px;
  margin-top: 8px;
}

.spider-detail-layout {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 340px;
  gap: 14px;
  align-items: start;
}

.spider-panel,
.spider-node-detail {
  min-width: 0;
  border: 1px solid var(--spider-line);
  border-radius: 8px;
  background: #ffffff;
  box-shadow: var(--spider-shadow);
}

.spider-panel {
  padding: 14px;
}

.spider-panel-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 10px;
}

.spider-panel h2,
.spider-node-detail h2 {
  margin: 0;
  color: var(--spider-text);
  font-size: 0.98rem;
  line-height: 1.3;
}

.spider-panel-note {
  color: var(--spider-muted);
  font-size: 0.78rem;
}

.spider-graph-wrap {
  overflow: visible;
}

.spider-architecture-graph {
  display: block;
  width: 100%;
  height: auto;
  min-height: 360px;
}

.spider-edge {
  fill: none;
  stroke: #9db6df;
  stroke-width: 2;
  filter: drop-shadow(0 1px 1px rgba(23, 76, 153, 0.16));
}

.spider-graph-node {
  cursor: pointer;
}

.spider-graph-node rect {
  fill: #ffffff;
  stroke: var(--spider-line-strong);
  stroke-width: 1.25;
  filter: drop-shadow(0 5px 10px rgba(23, 32, 51, 0.07));
}

.spider-graph-node.is-root rect {
  fill: var(--spider-blue-soft);
  stroke: var(--spider-blue);
}

.spider-graph-node.is-condition rect {
  fill: var(--spider-amber-soft);
  stroke: var(--spider-amber);
}

.spider-graph-node.is-branch rect {
  fill: var(--spider-purple-soft);
  stroke: var(--spider-purple);
}

.spider-graph-node.is-selected rect {
  fill: var(--spider-red-soft);
  stroke: var(--spider-red);
  stroke-width: 2.25;
}

.spider-graph-node text {
  fill: var(--spider-text);
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", "Noto Sans", Helvetica, Arial, sans-serif;
}

.spider-graph-node .spider-node-subtitle {
  fill: var(--spider-muted);
}

.spider-node-detail {
  position: sticky;
  top: 22px;
  padding: 14px;
}

.spider-node-header {
  display: grid;
  gap: 8px;
  margin-bottom: 12px;
}

.spider-definition {
  display: grid;
  grid-template-columns: 98px minmax(0, 1fr);
  gap: 7px 10px;
  margin: 0;
}

.spider-definition dt {
  color: var(--spider-muted);
  font-size: 0.78rem;
}

.spider-definition dd {
  min-width: 0;
  margin: 0;
  overflow-wrap: anywhere;
  color: var(--spider-text);
  font-size: 0.84rem;
}

.spider-detail-section {
  border-top: 1px solid var(--spider-line);
  margin-top: 12px;
  padding-top: 12px;
}

.spider-profile-row {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

[data-show-evidence="false"] .spider-evidence {
  display: none;
}

.spider-json {
  overflow: auto;
  max-height: calc(100vh - 110px);
  margin: 0;
  border: 1px solid var(--spider-line);
  border-radius: 8px;
  background: #101827;
  color: #edf3ff;
  font-size: 0.8rem;
  line-height: 1.45;
  padding: 14px;
}

.spider-hidden {
  display: none !important;
}

@media (max-width: 940px) {
  .spider-detail-layout {
    grid-template-columns: 1fr;
  }

  .spider-node-detail {
    position: static;
  }
}

@media (max-width: 760px) {
  .spider-shell {
    display: block;
    height: auto;
    min-height: 100vh;
  }

  .spider-sidebar {
    position: relative;
    width: 100%;
    height: auto;
    flex: none;
    border-right: 0;
    border-bottom: 1px solid var(--spider-sidebar-border);
  }

  .spider-menu {
    grid-template-columns: 1fr 1fr;
    padding: 8px;
  }

  .spider-main {
    padding: 16px;
  }

  .spider-view-header,
  .spider-detail-toolbar,
  .spider-process-row {
    grid-template-columns: 1fr;
    display: grid;
  }

  .spider-search {
    width: 100%;
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
  const content = document.getElementById("spider-content");
  const manifestElement = document.getElementById("spider-manifest-data");
  const manifest = JSON.parse(manifestElement.textContent || "{}");
  const components = manifest.components || [];
  const relations = manifest.relations || [];
  const byId = new Map(components.map((component) => [component.id, component]));
  const flows = components.filter((component) => component.kind === "spider.flow").sort(compareByName);
  const pipelines = components.filter((component) => component.kind === "spider.pipeline").sort(compareByName);
  const flowCount = document.getElementById("spider-flow-count");
  const pipelineCount = document.getElementById("spider-pipeline-count");
  const topbarTitle = document.getElementById("spider-topbar-title");
  const jsonLink = document.getElementById("spider-json-link");
  const showGraph = root.dataset.showGraph === "true";
  const showJson = root.dataset.showJson === "true";
  const state = {
    view: "pipelines",
    mode: "list",
    processId: "",
    nodeId: "",
    query: ""
  };

  flowCount.textContent = String(flows.length);
  pipelineCount.textContent = String(pipelines.length);

  if (!showJson && jsonLink) {
    jsonLink.classList.add("spider-hidden");
  }

  document.addEventListener("click", (event) => {
    const menu = event.target.closest("[data-menu-view]");
    if (menu) {
      showList(menu.getAttribute("data-menu-view"));
      return;
    }

    const process = event.target.closest("[data-open-process]");
    if (process) {
      openProcess(process.getAttribute("data-open-process"));
      return;
    }

    const back = event.target.closest("[data-back-list]");
    if (back) {
      showList(state.view);
      return;
    }

    const node = event.target.closest("[data-node-id]");
    if (node) {
      selectNode(node.getAttribute("data-node-id"));
    }
  });

  if (jsonLink) {
    jsonLink.addEventListener("click", () => {
      state.mode = "json";
      state.processId = "";
      state.nodeId = "";
      setHash("json");
      renderJson();
      setActiveMenu("");
    });
  }

  window.addEventListener("hashchange", openFromHash);

  openFromHash();

  if (state.mode === "list") {
    renderList(state.view);
  }

  function openFromHash() {
    const hash = decodeURIComponent(window.location.hash.replace(/^#\/?/, ""));
    if (!hash) {
      return;
    }

    if (hash === "flows" || hash === "pipelines") {
      showList(hash, true);
      return;
    }

    if (hash === "json") {
      state.mode = "json";
      renderJson();
      return;
    }

    if (byId.has(hash)) {
      openProcess(hash, true);
    }
  }

  function showList(view, skipHash) {
    state.view = view === "flows" ? "flows" : "pipelines";
    state.mode = "list";
    state.processId = "";
    state.nodeId = "";
    state.query = "";
    if (!skipHash) {
      setHash(state.view);
    }

    renderList(state.view);
    resetMainScroll();
  }

  function renderList(view) {
    const items = view === "flows" ? flows : pipelines;
    const title = view === "flows" ? "Flows" : "Pipelines";
    const description = view === "flows"
      ? "Method-level business flows documented from ComposeFlow calls."
      : "Execution pipelines attached around Spider service invocations.";
    const listId = view === "flows" ? "spider-flow-list" : "spider-pipeline-list";

    setActiveMenu(view);
    setTopbarTitle(title);

    content.innerHTML = `
      <div class="spider-list-view">
        <header class="spider-view-header">
          <div>
            <h1 class="spider-view-title">${escapeHtml(title)}</h1>
            <p class="spider-view-description">${escapeHtml(description)}</p>
          </div>
          <label class="spider-search" aria-label="Search ${escapeAttribute(title)}">
            <input id="spider-search" type="search" autocomplete="off" placeholder="Search ${escapeAttribute(title.toLowerCase())}" value="${escapeAttribute(state.query)}" />
          </label>
        </header>
        <div id="${listId}" class="spider-process-list">
          ${renderProcessRows(items)}
        </div>
      </div>`;

    const search = document.getElementById("spider-search");
    if (search) {
      search.addEventListener("input", () => {
        state.query = search.value.trim().toLowerCase();
        document.getElementById(listId).innerHTML = renderProcessRows(items);
      });
    }
  }

  function renderProcessRows(items) {
    const matches = filterProcesses(items);
    if (matches.length === 0) {
      return `<div class="spider-empty-list">No items found.</div>`;
    }

    return matches.map((item) => {
      const children = orderChildren(item);
      const countLabel = item.kind === "spider.pipeline"
        ? `${children.length} stages`
        : `${children.length} steps`;

      return `
        <button class="spider-process-row" type="button" data-open-process="${escapeAttribute(item.id)}">
          <span>
            <span class="spider-process-name">${escapeHtml(item.displayName || item.id)}</span>
            <span class="spider-process-meta">${escapeHtml(getSignature(item))}</span>
          </span>
          <span class="spider-count-pill">${escapeHtml(countLabel)}</span>
        </button>`;
    }).join("");
  }

  function openProcess(id, skipHash) {
    const process = byId.get(id);
    if (!process) {
      return;
    }

    state.view = process.kind === "spider.flow" ? "flows" : "pipelines";
    state.mode = "detail";
    state.processId = id;
    state.nodeId = id;
    if (!skipHash) {
      setHash(id);
    }

    renderProcessDetail(process);
    resetMainScroll();
  }

  function renderProcessDetail(process) {
    const children = orderChildren(process);
    const profiles = getRelated(process.id, "uses-profile");
    const kind = process.kind === "spider.flow" ? "Flow" : "Pipeline";
    const backLabel = process.kind === "spider.flow" ? "Back to Flows" : "Back to Pipelines";
    const graph = showGraph
      ? renderVerticalGraph(process, children)
      : `<div class="spider-empty-list">Graph disabled.</div>`;

    setActiveMenu(state.view);
    setTopbarTitle(process.displayName || kind);

    content.innerHTML = `
      <article class="spider-detail-view">
        <header class="spider-detail-toolbar">
          <div>
            <button class="spider-back-button" type="button" data-back-list>${escapeHtml(backLabel)}</button>
            <div class="spider-detail-actions">
              <span class="spider-chip ${process.kind === "spider.flow" ? "flow" : "pipeline"}">${escapeHtml(kind)}</span>
              <span class="spider-chip">${escapeHtml(getSignature(process))}</span>
              ${profiles.map((profile) => `<span class="spider-chip profile">${escapeHtml(profile.displayName || profile.id)}</span>`).join("")}
            </div>
            <h1 class="spider-detail-title">${escapeHtml(process.displayName || process.id)}</h1>
            <p class="spider-detail-subtitle">${escapeHtml(describeProcess(process, children))}</p>
          </div>
        </header>
        <div class="spider-detail-layout">
          <section class="spider-panel">
            <div class="spider-panel-header">
              <h2>Graph</h2>
              <span class="spider-panel-note">Vertical execution order</span>
            </div>
            <div class="spider-graph-wrap spider-process-graph">${graph}</div>
          </section>
          <aside id="spider-node-detail" class="spider-node-detail">
            ${renderNodeDetail(process, process, 0)}
          </aside>
        </div>
      </article>`;
  }

  function renderJson() {
    setTopbarTitle("Manifest JSON");

    content.innerHTML = `
      <div class="spider-json-view">
        <header class="spider-view-header">
          <div>
            <h1 class="spider-view-title">Manifest JSON</h1>
            <p class="spider-view-description">Generated Spider architecture model.</p>
          </div>
        </header>
        <pre class="spider-json">${escapeHtml(JSON.stringify(manifest, null, 2))}</pre>
      </div>`;
  }

  function renderVerticalGraph(process, children) {
    const nodes = [process].concat(children);
    const width = 760;
    const nodeWidth = 430;
    const nodeHeight = 74;
    const nodeX = 165;
    const top = 24;
    const gap = 112;
    const height = Math.max(260, top + nodes.length * gap);
    const center = nodeX + (nodeWidth / 2);
    const edges = [];
    const renderedNodes = [];

    for (let index = 0; index < nodes.length; index++) {
      const node = nodes[index];
      const y = top + (index * gap);
      const selected = node.id === state.nodeId ? " is-selected" : "";
      const rootClass = index === 0 ? " is-root" : "";
      const graphClass = getGraphClass(node);
      const subtitle = index === 0 ? getSignature(node) : getNodeSubtitle(node);
      const number = index === 0 ? "0" : String(index).padStart(2, "0");

      renderedNodes.push(`
        <g class="spider-graph-node${rootClass}${graphClass}${selected}" data-node-id="${escapeAttribute(node.id)}" transform="translate(${nodeX}, ${y})">
          <title>${escapeHtml(node.displayName || node.id)}</title>
          <rect width="${nodeWidth}" height="${nodeHeight}" rx="10"></rect>
          <text x="18" y="28" font-size="13" font-weight="800">${escapeHtml(number)}</text>
          <text x="56" y="29" font-size="14" font-weight="800">${escapeHtml(truncate(node.displayName || node.id, 42))}</text>
          <text class="spider-node-subtitle" x="56" y="51" font-size="12">${escapeHtml(truncate(subtitle, 48))}</text>
        </g>`);

      if (index < nodes.length - 1) {
        const y1 = y + nodeHeight;
        const y2 = y + gap;
        edges.push(`<path class="spider-edge" d="M ${center} ${y1} C ${center} ${y1 + 24}, ${center} ${y2 - 24}, ${center} ${y2 - 8}" marker-end="url(#spider-arrow)" />`);
      }
    }

    return `
      <svg class="spider-architecture-graph" viewBox="0 0 ${width} ${height}" role="img" aria-label="${escapeAttribute(process.displayName || "Spider process graph")}" preserveAspectRatio="xMidYMin meet">
        <defs>
          <marker id="spider-arrow" viewBox="0 0 10 10" refX="8" refY="5" markerWidth="6" markerHeight="6" orient="auto-start-reverse">
            <path d="M 0 0 L 10 5 L 0 10 z" fill="#9db6df"></path>
          </marker>
        </defs>
        ${edges.join("")}
        ${renderedNodes.join("")}
      </svg>`;
  }

  function selectNode(id) {
    const node = byId.get(id);
    const process = byId.get(state.processId);
    if (!node || !process) {
      return;
    }

    state.nodeId = id;
    document.querySelectorAll(".spider-graph-node").forEach((item) => {
      item.classList.toggle("is-selected", item.getAttribute("data-node-id") === id);
    });

    const children = orderChildren(process);
    const index = node.id === process.id ? 0 : children.findIndex((child) => child.id === node.id) + 1;
    const detail = document.getElementById("spider-node-detail");
    if (detail) {
      detail.innerHTML = renderNodeDetail(node, process, index);
    }
  }

  function renderNodeDetail(node, process, index) {
    const metadata = Object.entries(node.metadata || {});
    const evidence = node.evidence || [];
    const source = evidence.length && evidence[0].filePath
      ? `${evidence[0].filePath}${evidence[0].lineNumber ? ":" + evidence[0].lineNumber : ""}`
      : "";
    const rows = [
      ["Kind", getFriendlyKind(node)],
      ["Order", index === 0 ? "Process root" : String(index)],
      ["Id", node.id]
    ].concat(metadata.map(([key, value]) => [formatLabel(key), value]));
    const evidenceRows = evidence.length
      ? `
        <div class="spider-detail-section spider-evidence">
          <dl class="spider-definition">
            <dt>Member</dt><dd>${escapeHtml(evidence[0].memberName || "Not available")}</dd>
            <dt>Source</dt><dd>${escapeHtml(source || "Not available")}</dd>
          </dl>
        </div>`
      : "";

    return `
      <div class="spider-node-header">
        <span class="spider-node-kind ${getKindClass(node)}">${escapeHtml(getFriendlyKind(node))}</span>
        <h2>${escapeHtml(node.displayName || node.id)}</h2>
      </div>
      <dl class="spider-definition">
        ${rows.map(([label, value]) => `<dt>${escapeHtml(label)}</dt><dd>${escapeHtml(value || "Not declared")}</dd>`).join("")}
      </dl>
      ${evidenceRows}`;
  }

  function filterProcesses(items) {
    if (!state.query) {
      return items;
    }

    return items.filter((item) => createSearchText(item).includes(state.query));
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

  function describeProcess(process, children) {
    const childNoun = process.kind === "spider.pipeline" ? "stage" : "step";
    return `${children.length} ${childNoun}${children.length === 1 ? "" : "s"} discovered at compile time.`;
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

  function getNodeSubtitle(component) {
    if (component.kind === "spider.pipeline-stage") {
      const count = getMetadata(component, "count") || "0";
      return `${count} configured action${count === "1" ? "" : "s"}`;
    }

    return getMetadata(component, "delegate") || getMetadata(component, "operation") || getFriendlyKind(component);
  }

  function getFriendlyKind(component) {
    if (component.kind === "spider.pipeline") {
      return "Pipeline";
    }

    if (component.kind === "spider.flow") {
      return "Flow";
    }

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
      return " is-condition";
    }

    if (component.kind === "spider.flow-branch") {
      return " is-branch";
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

  function truncate(value, length) {
    const text = String(value || "");
    return text.length > length ? text.slice(0, Math.max(0, length - 1)) + "..." : text;
  }

  function setActiveMenu(view) {
    document.querySelectorAll("[data-menu-view]").forEach((item) => {
      item.classList.toggle("is-active", item.getAttribute("data-menu-view") === view);
    });
  }

  function setTopbarTitle(value) {
    if (topbarTitle) {
      topbarTitle.textContent = value || "Spider Architecture";
    }
  }

  function setHash(value) {
    window.history.replaceState(null, "", "#" + encodeURIComponent(value));
  }

  function resetMainScroll() {
    const main = root.querySelector(".spider-main");
    if (main) {
      main.scrollTop = 0;
    }

    window.scrollTo(0, 0);
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
