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
            var runtimeJson = new SpiderRuntimeTraceWebSerializer().Serialize(
                options.RuntimeTraceSummaries,
                options.RuntimeTraces);
            var runtimeEndpoint = HtmlEncoder.Default.Encode(options.RuntimeTracesEndpoint ?? string.Empty);
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
                $"  <div id=\"spider-documentation-app\" class=\"spider-shell spider-architecture-app\" data-theme=\"light\" data-show-evidence=\"{BooleanAttribute(options.IncludeEvidence)}\" data-show-graph=\"{BooleanAttribute(options.IncludeGraph)}\" data-show-json=\"{BooleanAttribute(options.IncludeJsonPanel)}\" data-show-search=\"{BooleanAttribute(options.IncludeSearch)}\" data-show-runtime=\"{BooleanAttribute(options.IncludeRuntimeTraces)}\" data-runtime-endpoint=\"{runtimeEndpoint}\">");
            html.AppendLine("    <aside class=\"spider-sidebar\" aria-label=\"Spider architecture navigation\">");
            html.AppendLine("      <div class=\"spider-brand\">");
            html.AppendLine("        <div class=\"spider-logo\" aria-hidden=\"true\">S</div>");
            html.AppendLine("        <div class=\"spider-brand-copy\">");
            html.AppendLine($"          <div class=\"spider-title\">{title}</div>");
            html.AppendLine("          <div class=\"spider-subtitle\">Architecture</div>");
            html.AppendLine("        </div>");
            html.AppendLine("      </div>");
            html.AppendLine("      <nav class=\"spider-menu\" aria-label=\"Architecture sections\">");
            html.AppendLine("        <div class=\"spider-menu-section\">Map</div>");
            html.AppendLine("        <button class=\"spider-menu-item\" type=\"button\" data-menu-view=\"pipelines\"><span class=\"spider-menu-icon pipeline\" aria-hidden=\"true\"></span><span class=\"spider-menu-text\"><span>Pipelines</span><small>Execution wrappers</small></span><strong id=\"spider-pipeline-count\">0</strong></button>");
            html.AppendLine("        <button class=\"spider-menu-item\" type=\"button\" data-menu-view=\"flows\"><span class=\"spider-menu-icon flow\" aria-hidden=\"true\"></span><span class=\"spider-menu-text\"><span>Flows</span><small>Business processes</small></span><strong id=\"spider-flow-count\">0</strong></button>");
            html.AppendLine("        <button class=\"spider-menu-item spider-runtime-menu-item\" type=\"button\" data-menu-view=\"runtime\"><span class=\"spider-menu-icon runtime\" aria-hidden=\"true\"></span><span class=\"spider-menu-text\"><span>Runtime</span><small>Live executions</small></span><strong id=\"spider-runtime-count\">0</strong></button>");
            html.AppendLine("      </nav>");
            html.AppendLine("      <div class=\"spider-sidebar-footer\">");
            html.AppendLine("        <button id=\"spider-json-link\" class=\"spider-json-link\" type=\"button\">Manifest JSON</button>");
            html.AppendLine("      </div>");
            html.AppendLine("    </aside>");
            html.AppendLine("    <section class=\"spider-workspace\">");
            html.AppendLine("      <header class=\"spider-topbar\">");
            html.AppendLine("        <div class=\"spider-topbar-left\">");
            html.AppendLine("          <button id=\"spider-sidebar-toggle\" class=\"spider-sidebar-toggle\" type=\"button\" aria-label=\"Collapse navigation\" aria-expanded=\"true\" title=\"Collapse navigation\"><span class=\"spider-sidebar-toggle-line\"></span><span class=\"spider-sidebar-toggle-line\"></span></button>");
            html.AppendLine("          <div id=\"spider-topbar-title\" class=\"spider-topbar-title\">Pipelines</div>");
            html.AppendLine("        </div>");
            html.AppendLine("        <div class=\"spider-topbar-actions\">");
            html.AppendLine("          <button id=\"spider-theme-toggle\" class=\"spider-theme-toggle\" type=\"button\" aria-label=\"Use dark mode\" aria-pressed=\"false\" title=\"Use dark mode\"><span class=\"spider-theme-toggle-dot\" aria-hidden=\"true\"></span><span id=\"spider-theme-toggle-label\">Light</span></button>");
            html.AppendLine("          <div class=\"spider-topbar-badge\">Generated metadata</div>");
            html.AppendLine("        </div>");
            html.AppendLine("      </header>");
            html.AppendLine("      <main class=\"spider-main\">");
            html.AppendLine("        <section id=\"spider-content\" class=\"spider-content\" aria-live=\"polite\"></section>");
            html.AppendLine("      </main>");
            html.AppendLine("    </section>");
            html.AppendLine("  </div>");
            html.AppendLine($"  <script id=\"spider-manifest-data\" type=\"application/json\">{manifestJson}</script>");
            html.AppendLine($"  <script id=\"spider-runtime-trace-data\" type=\"application/json\">{runtimeJson}</script>");
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
  --spider-sidebar-width: 248px;
  --spider-sidebar-collapsed-width: 72px;
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
  --spider-topbar-bg: #ffffff;
  --spider-input-bg: #ffffff;
  --spider-graph-bg: #ffffff;
  --spider-node-bg: #ffffff;
  --spider-node-soft: #fbfcff;
  --spider-tooltip-bg: rgba(17, 24, 39, 0.96);
  --spider-tooltip-text: #ffffff;
  --spider-tooltip-muted: #aeb8c7;
  --spider-edge: #9aa4b2;
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
  --spider-shadow: 0 8px 18px rgba(17, 24, 39, 0.06);
  --spider-radius: 8px;
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", "Noto Sans", Helvetica, Arial, sans-serif;
}

.spider-shell[data-theme="dark"] {
  color-scheme: dark;
  --spider-sidebar-bg: #070d1a;
  --spider-sidebar-brand: #050914;
  --spider-sidebar-border: #1d2638;
  --spider-sidebar-text: #edf2ff;
  --spider-sidebar-muted: #95a2b8;
  --spider-sidebar-hover: rgba(230, 36, 45, 0.14);
  --spider-sidebar-active: rgba(230, 36, 45, 0.24);
  --spider-sidebar-active-text: #ffe3e5;
  --spider-bg: #0e1422;
  --spider-panel: #141c2b;
  --spider-panel-soft: #192335;
  --spider-topbar-bg: #101827;
  --spider-input-bg: #0f1726;
  --spider-graph-bg: #0f1726;
  --spider-node-bg: #151f30;
  --spider-node-soft: #111b2b;
  --spider-tooltip-bg: rgba(247, 250, 255, 0.96);
  --spider-tooltip-text: #101827;
  --spider-tooltip-muted: #526176;
  --spider-edge: #64748b;
  --spider-line: #263349;
  --spider-line-strong: #34445f;
  --spider-text: #edf2ff;
  --spider-muted: #9aa8bd;
  --spider-black: #e6edf8;
  --spider-red: #ff4651;
  --spider-red-strong: #ff6b72;
  --spider-red-soft: rgba(255, 70, 81, 0.14);
  --spider-blue: #74a9ff;
  --spider-blue-strong: #9ac0ff;
  --spider-blue-soft: rgba(116, 169, 255, 0.14);
  --spider-shadow: 0 12px 28px rgba(0, 0, 0, 0.28);
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
  transition: flex-basis 0.16s ease, width 0.16s ease;
}

.spider-brand {
  display: flex;
  align-items: center;
  min-height: 68px;
  flex: 0 0 auto;
  gap: 11px;
  overflow: hidden;
  border-bottom: 1px solid var(--spider-sidebar-border);
  background:
    linear-gradient(135deg, rgba(230, 36, 45, 0.16), transparent 42%),
    var(--spider-sidebar-brand);
  padding: 13px 14px;
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

.spider-brand-copy {
  min-width: 0;
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
  gap: 8px;
  align-content: start;
  grid-auto-rows: max-content;
  overflow-y: auto;
  padding: 14px 10px;
}

.spider-menu-section {
  color: var(--spider-sidebar-muted);
  font-size: 0.68rem;
  font-weight: 800;
  letter-spacing: 0;
  padding: 0 8px 2px;
  text-transform: uppercase;
}

.spider-menu-item {
  display: grid;
  grid-template-columns: 28px minmax(0, 1fr) auto;
  align-items: center;
  gap: 10px;
  width: 100%;
  border: 1px solid transparent;
  border-radius: var(--spider-radius);
  background: transparent;
  color: var(--spider-sidebar-text);
  cursor: pointer;
  font-size: 0.875rem;
  font-weight: 650;
  padding: 9px 10px;
  text-align: left;
  transition: background 0.12s, border-color 0.12s, color 0.12s, transform 0.12s;
}

.spider-menu-item:hover {
  background: var(--spider-sidebar-hover);
  border-color: rgba(255, 255, 255, 0.08);
  color: #e0e2f0;
}

.spider-menu-item.is-active {
  background: var(--spider-sidebar-active);
  border-color: rgba(230, 36, 45, 0.36);
  color: var(--spider-sidebar-active-text);
}

.spider-menu-item.is-active .spider-menu-icon {
  box-shadow: 0 0 0 3px rgba(230, 36, 45, 0.14);
}

.spider-menu-item strong {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 24px;
  border: 1px solid rgba(255, 255, 255, 0.12);
  border-radius: 999px;
  color: currentColor;
  font-size: 0.72rem;
  font-weight: 700;
  line-height: 1;
  padding: 4px 6px;
}

.spider-menu-icon {
  width: 26px;
  height: 26px;
  border-radius: 7px;
  background: var(--spider-red);
}

.spider-menu-icon.pipeline {
  background: linear-gradient(135deg, var(--spider-black), var(--spider-blue));
}

.spider-menu-icon.flow {
  background: linear-gradient(135deg, var(--spider-red), var(--spider-red-strong));
}

.spider-menu-icon.runtime {
  background: linear-gradient(135deg, var(--spider-blue), var(--spider-red));
}

[data-show-runtime="false"] .spider-runtime-menu-item {
  display: none;
}

.spider-menu-text {
  display: grid;
  gap: 1px;
  min-width: 0;
}

.spider-menu-text small {
  overflow: hidden;
  color: var(--spider-sidebar-muted);
  font-size: 0.72rem;
  font-weight: 500;
  line-height: 1.25;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.spider-sidebar-footer {
  border-top: 1px solid var(--spider-sidebar-border);
  padding: 12px 10px;
}

.spider-json-link {
  display: none;
  width: 100%;
  margin: 0;
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

@media (min-width: 761px) {
  .spider-shell.is-sidebar-collapsed {
    --spider-sidebar-width: var(--spider-sidebar-collapsed-width);
  }

  .spider-shell.is-sidebar-collapsed .spider-brand {
    justify-content: center;
    padding: 13px 8px;
  }

  .spider-shell.is-sidebar-collapsed .spider-brand-copy,
  .spider-shell.is-sidebar-collapsed .spider-menu-section,
  .spider-shell.is-sidebar-collapsed .spider-menu-text,
  .spider-shell.is-sidebar-collapsed .spider-menu-item strong,
  .spider-shell.is-sidebar-collapsed .spider-sidebar-footer {
    display: none;
  }

  .spider-shell.is-sidebar-collapsed .spider-menu {
    padding: 12px 9px;
  }

  .spider-shell.is-sidebar-collapsed .spider-menu-item {
    grid-template-columns: 1fr;
    justify-items: center;
    padding: 10px 0;
  }

  .spider-shell.is-sidebar-collapsed .spider-menu-icon {
    width: 30px;
    height: 30px;
  }
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
  border-bottom: 1px solid var(--spider-line);
  background: var(--spider-topbar-bg);
  padding: 0 16px;
}

.spider-topbar-left {
  display: flex;
  align-items: center;
  gap: 10px;
  min-width: 0;
}

.spider-sidebar-toggle {
  display: inline-grid;
  place-content: center;
  gap: 4px;
  width: 32px;
  height: 32px;
  flex: 0 0 auto;
  border: 1px solid var(--spider-line);
  border-radius: var(--spider-radius);
  background: var(--spider-input-bg);
  color: var(--spider-text);
  cursor: pointer;
  transition: border-color 0.12s, box-shadow 0.12s, color 0.12s;
}

.spider-sidebar-toggle:hover {
  border-color: rgba(230, 36, 45, 0.36);
  box-shadow: 0 0 0 3px rgba(230, 36, 45, 0.08);
  color: var(--spider-red);
}

.spider-sidebar-toggle-line {
  display: block;
  width: 14px;
  height: 2px;
  border-radius: 999px;
  background: currentColor;
}

.spider-topbar-title {
  overflow: hidden;
  color: var(--spider-text);
  font-size: 0.95rem;
  font-weight: 650;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.spider-topbar-actions {
  display: inline-flex;
  align-items: center;
  flex: 0 0 auto;
  gap: 8px;
}

.spider-theme-toggle {
  display: inline-flex;
  align-items: center;
  gap: 7px;
  min-height: 28px;
  border: 1px solid var(--spider-line);
  border-radius: 999px;
  background: var(--spider-input-bg);
  color: var(--spider-muted);
  cursor: pointer;
  font-size: 0.75rem;
  font-weight: 650;
  line-height: 1;
  padding: 5px 9px;
  transition: border-color 0.12s, box-shadow 0.12s, color 0.12s, background 0.12s;
}

.spider-theme-toggle:hover {
  border-color: rgba(230, 36, 45, 0.36);
  box-shadow: 0 0 0 3px rgba(230, 36, 45, 0.08);
  color: var(--spider-red);
}

.spider-theme-toggle-dot {
  width: 10px;
  height: 10px;
  border: 2px solid var(--spider-blue);
  border-radius: 999px;
  background: var(--spider-red);
  box-shadow: inset -3px 0 0 var(--spider-input-bg);
}

.spider-shell[data-theme="dark"] .spider-theme-toggle-dot {
  border-color: var(--spider-red);
  background: var(--spider-blue);
  box-shadow: inset -3px 0 0 var(--spider-panel);
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
  padding: 20px 24px;
}

.spider-content {
  width: 100%;
  max-width: none;
}

.spider-view-header,
.spider-detail-toolbar {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 18px;
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
  width: min(340px, 36vw);
  min-width: 220px;
}

.spider-search input {
  width: 100%;
  border: 1px solid var(--spider-line);
  border-radius: 8px;
  background: var(--spider-input-bg);
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
  grid-template-columns: repeat(auto-fill, minmax(320px, 360px));
  grid-auto-rows: 96px;
  justify-content: start;
  gap: 12px;
  overflow: visible;
  border: 0;
  border-radius: 0;
  background: transparent;
}

.spider-process-row {
  display: grid;
  grid-template-columns: 4px minmax(0, 1fr) auto;
  align-items: stretch;
  gap: 12px;
  width: 100%;
  height: 96px;
  min-height: 0;
  overflow: hidden;
  border: 1px solid var(--spider-line);
  border-radius: 8px;
  background: var(--spider-panel);
  cursor: pointer;
  padding: 0;
  text-align: left;
  transition: border-color 0.12s, box-shadow 0.12s, transform 0.12s;
}

.spider-process-row:last-child {
  border-bottom: 1px solid var(--spider-line);
}

.spider-process-row:hover {
  border-color: rgba(230, 36, 45, 0.28);
  box-shadow: 0 10px 22px rgba(17, 24, 39, 0.07);
  transform: translateY(-1px);
}

.spider-process-accent {
  border-radius: 8px 0 0 8px;
  background: var(--spider-red);
}

.spider-process-row.pipeline .spider-process-accent {
  background: var(--spider-black);
}

.spider-process-row.runtime .spider-process-accent {
  background: var(--spider-blue);
}

.spider-runtime-execution-list {
  position: relative;
  display: grid;
  gap: 10px;
  padding-left: 18px;
}

.spider-runtime-execution-list::before {
  content: "";
  position: absolute;
  top: 10px;
  bottom: 10px;
  left: 8px;
  width: 2px;
  border-radius: 999px;
  background: var(--spider-line-strong);
}

.spider-runtime-row {
  position: relative;
  display: grid;
  grid-template-columns: 96px 18px minmax(0, 1fr) auto;
  align-items: center;
  gap: 12px;
  width: 100%;
  min-height: 86px;
  border: 1px solid var(--spider-line);
  border-radius: 8px;
  background: var(--spider-panel);
  cursor: pointer;
  padding: 12px 14px;
  text-align: left;
  transition: border-color 0.12s, box-shadow 0.12s, transform 0.12s;
}

.spider-runtime-row:hover {
  border-color: rgba(29, 95, 191, 0.32);
  box-shadow: 0 10px 22px rgba(17, 24, 39, 0.07);
  transform: translateY(-1px);
}

.spider-runtime-row-time {
  display: grid;
  gap: 3px;
  color: var(--spider-muted);
  font-size: 0.74rem;
  line-height: 1.25;
}

.spider-runtime-row-time strong {
  color: var(--spider-text);
  font-size: 0.84rem;
}

.spider-runtime-dot {
  width: 12px;
  height: 12px;
  border: 2px solid var(--spider-panel);
  border-radius: 999px;
  background: var(--spider-muted);
  box-shadow: 0 0 0 3px var(--spider-line);
  z-index: 1;
}

.spider-runtime-row-body {
  display: grid;
  gap: 6px;
  min-width: 0;
}

.spider-runtime-row-title {
  overflow: hidden;
  color: var(--spider-text);
  font-size: 0.95rem;
  font-weight: 700;
  line-height: 1.25;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.spider-runtime-row-meta,
.spider-runtime-row-fault {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  min-width: 0;
  color: var(--spider-muted);
  font-size: 0.78rem;
}

.spider-runtime-row-fault {
  color: var(--spider-red);
}

.spider-runtime-row-stats {
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: 6px;
  min-width: 170px;
}

.spider-status-chip {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  width: fit-content;
  border: 1px solid var(--spider-line);
  border-radius: 999px;
  background: var(--spider-panel-soft);
  color: var(--spider-muted);
  font-size: 0.72rem;
  font-weight: 750;
  line-height: 1;
  padding: 5px 8px;
}

.spider-status-chip::before {
  content: "";
  width: 7px;
  height: 7px;
  border-radius: 999px;
  background: currentColor;
}

.spider-status-started,
.spider-status-running {
  border-color: rgba(29, 95, 191, 0.28);
  background: var(--spider-blue-soft);
  color: var(--spider-blue);
}

.spider-status-running .spider-runtime-dot,
.spider-runtime-row.is-running .spider-runtime-dot,
.spider-status-running::before {
  animation: spiderPulse 1.4s ease-in-out infinite;
}

.spider-status-completed {
  border-color: rgba(29, 95, 191, 0.28);
  background: var(--spider-blue-soft);
  color: var(--spider-blue);
}

.spider-status-faulted {
  border-color: rgba(230, 36, 45, 0.34);
  background: var(--spider-red-soft);
  color: var(--spider-red);
}

.spider-status-cancelled {
  border-color: rgba(189, 16, 24, 0.24);
  background: var(--spider-red-soft);
  color: var(--spider-red-strong);
}

.spider-runtime-row.is-running .spider-runtime-dot,
.spider-runtime-span-card.is-running .spider-runtime-dot {
  background: var(--spider-blue);
  box-shadow: 0 0 0 3px rgba(29, 95, 191, 0.16);
}

.spider-runtime-row.is-completed .spider-runtime-dot,
.spider-runtime-span-card.is-completed .spider-runtime-dot {
  background: var(--spider-blue);
  box-shadow: 0 0 0 3px rgba(29, 95, 191, 0.14);
}

.spider-runtime-row.is-faulted .spider-runtime-dot,
.spider-runtime-span-card.is-faulted .spider-runtime-dot {
  background: var(--spider-red);
  box-shadow: 0 0 0 3px rgba(230, 36, 45, 0.16);
}

.spider-runtime-row.is-cancelled .spider-runtime-dot,
.spider-runtime-span-card.is-cancelled .spider-runtime-dot {
  background: var(--spider-red-strong);
  box-shadow: 0 0 0 3px rgba(189, 16, 24, 0.14);
}

@keyframes spiderPulse {
  0%,
  100% {
    opacity: 1;
    transform: scale(1);
  }

  50% {
    opacity: 0.58;
    transform: scale(0.84);
  }
}

.spider-process-body {
  display: grid;
  align-content: center;
  gap: 6px;
  min-width: 0;
  overflow: hidden;
  padding: 12px 0;
}

.spider-process-name {
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  color: var(--spider-text);
  font-size: 0.96rem;
  font-weight: 700;
  line-height: 1.3;
}

.spider-process-meta {
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  color: var(--spider-muted);
  font-size: 0.8rem;
  line-height: 1.35;
}

.spider-process-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 5px;
  min-width: 0;
  overflow: hidden;
}

.spider-process-kind {
  display: inline-flex;
  align-items: center;
  width: fit-content;
  border: 1px solid rgba(230, 36, 45, 0.2);
  border-radius: 999px;
  background: var(--spider-red-soft);
  color: var(--spider-red);
  font-size: 0.7rem;
  font-weight: 750;
  line-height: 1;
  padding: 4px 7px;
}

.spider-process-kind.pipeline {
  border-color: rgba(17, 24, 39, 0.16);
  background: var(--spider-panel-soft);
  color: var(--spider-black);
}

.spider-process-kind.runtime {
  background: var(--spider-blue-soft);
  color: var(--spider-blue);
}

.spider-process-action {
  display: grid;
  align-content: center;
  justify-items: end;
  gap: 8px;
  min-width: 74px;
  padding: 12px 12px 12px 0;
}

.spider-process-arrow {
  color: var(--spider-muted);
  font-size: 1rem;
  line-height: 1;
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
  white-space: nowrap;
}

.spider-chip {
  background: var(--spider-red-soft);
  color: var(--spider-red);
}

.spider-chip.flow {
  background: var(--spider-red-soft);
  color: var(--spider-red);
}

.spider-kind-step,
.spider-kind-stage {
  background: var(--spider-panel-soft);
  border: 1px solid var(--spider-line);
  color: var(--spider-muted);
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
  background: var(--spider-panel-soft);
  border: 1px solid var(--spider-line);
  color: var(--spider-muted);
}

.spider-chip.tag {
  background: var(--spider-blue-soft);
  border: 1px solid rgba(29, 95, 191, 0.18);
  color: var(--spider-blue);
}

.spider-empty-list,
.spider-empty-state {
  border: 1px dashed var(--spider-line);
  border-radius: 8px;
  background: var(--spider-panel);
  color: var(--spider-muted);
  font-size: 0.88rem;
  padding: 18px;
}

.spider-back-button {
  border: 1px solid var(--spider-line);
  border-radius: 8px;
  background: var(--spider-input-bg);
  color: var(--spider-text);
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
  grid-template-columns: minmax(220px, 260px) minmax(560px, 1fr) minmax(220px, 280px);
  gap: 14px;
  align-items: stretch;
  min-height: calc(100vh - 220px);
}

.spider-detail-layout.is-inspector-collapsed {
  grid-template-columns: minmax(220px, 260px) minmax(640px, 1fr) 42px;
}

.spider-detail-layout.is-inspector-collapsed .spider-node-detail-body {
  display: none;
}

.spider-detail-layout.is-inspector-collapsed .spider-node-detail {
  min-height: 100%;
  padding: 10px 8px;
}

.spider-detail-layout.is-inspector-collapsed .spider-node-header {
  display: none;
}

.spider-panel,
.spider-node-detail {
  min-width: 0;
  border: 1px solid var(--spider-line);
  border-radius: 8px;
  background: var(--spider-panel);
  box-shadow: var(--spider-shadow);
}

.spider-panel {
  padding: 12px;
}

.spider-process-summary,
.spider-graph-panel,
.spider-node-detail {
  height: calc(100vh - 220px);
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

.spider-graph-panel {
  min-width: 0;
  display: flex;
  flex-direction: column;
}

.spider-process-summary {
  display: grid;
  gap: 12px;
  align-content: start;
  overflow: auto;
}

.spider-summary-grid {
  display: grid;
  gap: 9px;
}

.spider-summary-item {
  display: grid;
  gap: 2px;
}

.spider-summary-label {
  color: var(--spider-muted);
  font-size: 0.74rem;
  font-weight: 650;
  text-transform: uppercase;
}

.spider-summary-value {
  overflow-wrap: anywhere;
  color: var(--spider-text);
  font-size: 0.84rem;
  line-height: 1.35;
}

.spider-outline {
  display: grid;
  gap: 4px;
  border-top: 1px solid var(--spider-line);
  padding-top: 10px;
}

.spider-outline-title {
  color: var(--spider-muted);
  font-size: 0.74rem;
  font-weight: 650;
  text-transform: uppercase;
}

.spider-outline-row {
  display: grid;
  grid-template-columns: 26px minmax(0, 1fr);
  gap: 8px;
  align-items: center;
  width: 100%;
  border: 1px solid transparent;
  border-radius: 6px;
  background: transparent;
  cursor: pointer;
  padding: 5px 6px;
  text-align: left;
}

.spider-outline-row:hover {
  background: var(--spider-panel-soft);
}

.spider-outline-row.is-selected {
  border-color: rgba(230, 36, 45, 0.28);
  background: var(--spider-red-soft);
}

.spider-outline-row.is-selected.is-branch,
.spider-outline-row.is-selected.is-route {
  border-color: rgba(29, 95, 191, 0.28);
  background: var(--spider-blue-soft);
}

.spider-outline-row.is-selected.is-stage {
  border-color: rgba(17, 24, 39, 0.22);
  background: var(--spider-panel-soft);
}

.spider-outline-row.is-selected.is-condition {
  border-color: rgba(189, 16, 24, 0.28);
  background: var(--spider-red-soft);
}

.spider-outline-row.is-nested {
  margin-left: 18px;
  width: calc(100% - 18px);
}

.spider-outline-row.is-route {
  grid-template-columns: 26px minmax(0, 1fr);
  background: var(--spider-panel-soft);
}

.spider-outline-row.is-route-step {
  margin-left: 34px;
  width: calc(100% - 34px);
}

.spider-outline-index {
  color: var(--spider-muted);
  font-size: 0.72rem;
  font-weight: 800;
}

.spider-outline-name {
  overflow: hidden;
  color: var(--spider-text);
  font-size: 0.82rem;
  font-weight: 650;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.spider-graph-wrap {
  position: relative;
  flex: 1 1 auto;
  min-height: 0;
  overflow: auto;
  border: 1px solid var(--spider-line);
  border-radius: 8px;
  background: var(--spider-graph-bg);
  padding: 16px;
}

.spider-graph-tooltip {
  position: fixed;
  z-index: 20;
  max-width: 240px;
  border: 1px solid var(--spider-line);
  border-radius: 8px;
  background: var(--spider-tooltip-bg);
  box-shadow: 0 10px 26px rgba(17, 24, 39, 0.2);
  color: var(--spider-tooltip-text);
  font-size: 0.74rem;
  line-height: 1.35;
  opacity: 0;
  padding: 8px 10px;
  pointer-events: none;
  transform: translate3d(0, 0, 0);
  transition: opacity 0.08s ease;
  visibility: hidden;
}

.spider-graph-tooltip.is-visible {
  opacity: 1;
  visibility: visible;
}

.spider-tooltip-label {
  color: var(--spider-tooltip-muted);
  font-size: 0.66rem;
  font-weight: 750;
  letter-spacing: 0;
  text-transform: uppercase;
}

.spider-tooltip-value {
  margin-top: 2px;
}

.spider-tooltip-row + .spider-tooltip-row {
  margin-top: 7px;
}

.spider-architecture-graph {
  display: block;
  width: var(--spider-graph-width, 520px);
  max-width: none;
  height: auto;
  margin: 0 auto;
  min-height: 0;
}

.spider-edge {
  fill: none;
  stroke: var(--spider-edge);
  stroke-width: 1.35;
}

.spider-graph-node {
  cursor: pointer;
}

.spider-graph-node .spider-node-box {
  fill: var(--spider-node-bg);
  stroke: var(--spider-line-strong);
  stroke-width: 1;
  filter: drop-shadow(0 4px 8px rgba(23, 32, 51, 0.08));
}

.spider-graph-node .spider-node-accent {
  fill: var(--spider-red);
}

.spider-graph-node.is-root .spider-node-accent {
  fill: var(--spider-black);
}

.spider-graph-node.is-branch .spider-node-accent {
  fill: var(--spider-blue);
}

.spider-graph-node.is-route .spider-node-accent {
  fill: var(--spider-blue-strong);
}

.spider-graph-node.is-route-step .spider-node-accent {
  fill: var(--spider-red);
}

.spider-graph-node.is-stage .spider-node-accent {
  fill: var(--spider-black);
}

.spider-graph-node.is-condition .spider-node-accent {
  fill: var(--spider-red-strong);
}

.spider-graph-node.is-linked-flow .spider-node-box {
  stroke: rgba(230, 36, 45, 0.45);
}

.spider-graph-node.is-selected .spider-node-box {
  stroke: var(--spider-red);
  stroke-width: 2;
}

.spider-graph-node.is-selected.is-branch .spider-node-box,
.spider-graph-node.is-selected.is-route .spider-node-box {
  stroke: var(--spider-blue);
}

.spider-graph-node.is-selected.is-stage .spider-node-box,
.spider-graph-node.is-selected.is-root .spider-node-box {
  stroke: var(--spider-black);
}

.spider-graph-node.is-selected.is-condition .spider-node-box {
  stroke: var(--spider-red-strong);
}

.spider-graph-node.is-route .spider-node-box,
.spider-graph-node.is-route-step .spider-node-box {
  fill: var(--spider-node-soft);
}

.spider-graph-node text {
  fill: var(--spider-text);
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", "Noto Sans", Helvetica, Arial, sans-serif;
}

.spider-graph-node .spider-node-subtitle {
  fill: var(--spider-muted);
}

.spider-graph-node .spider-node-index {
  fill: var(--spider-muted);
}

.spider-graph-link .spider-link-dot {
  fill: var(--spider-node-bg);
  stroke: var(--spider-red);
  stroke-width: 1.4;
}

.spider-graph-link text {
  fill: var(--spider-red);
  font-size: 10px;
  font-weight: 700;
}

.spider-graph-link:hover .spider-link-dot {
  fill: var(--spider-red-soft);
  stroke-width: 1.8;
}

.spider-graph-legend {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.spider-legend-item {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  color: var(--spider-muted);
  font-size: 0.74rem;
}

.spider-legend-swatch {
  width: 8px;
  height: 8px;
  border-radius: 999px;
  background: var(--spider-red);
}

.spider-legend-swatch.branch {
  background: var(--spider-blue);
}

.spider-legend-swatch.route {
  background: var(--spider-blue-strong);
}

.spider-legend-swatch.stage {
  background: var(--spider-black);
}

.spider-node-detail {
  position: sticky;
  top: 22px;
  overflow: auto;
  padding: 12px;
}

.spider-node-header {
  display: grid;
  gap: 8px;
  margin-bottom: 12px;
}

.spider-node-description {
  margin: 8px 0 0;
  color: var(--spider-muted);
  font-size: 0.82rem;
  line-height: 1.45;
}

.spider-node-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 5px;
  margin-top: 8px;
}

.spider-node-detail-top {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 8px;
}

.spider-detail-layout.is-inspector-collapsed .spider-node-detail-top {
  align-items: center;
  justify-content: center;
}

.spider-inspector-toggle {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 28px;
  height: 28px;
  border: 1px solid var(--spider-line);
  border-radius: 7px;
  background: var(--spider-input-bg);
  color: var(--spider-muted);
  cursor: pointer;
  font-size: 0.8rem;
  font-weight: 800;
  line-height: 1;
}

.spider-inspector-toggle:hover {
  border-color: rgba(230, 36, 45, 0.38);
  background: var(--spider-red-soft);
  color: var(--spider-red);
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

.spider-related-list {
  display: grid;
  gap: 6px;
}

.spider-related-button {
  display: grid;
  width: 100%;
  border: 1px solid rgba(230, 36, 45, 0.32);
  border-radius: 7px;
  background: var(--spider-red-soft);
  color: var(--spider-text);
  cursor: pointer;
  gap: 2px;
  padding: 8px 9px;
  text-align: left;
}

.spider-related-button:hover {
  border-color: rgba(230, 36, 45, 0.62);
  box-shadow: 0 0 0 3px rgba(230, 36, 45, 0.08);
}

.spider-related-label {
  color: var(--spider-red);
  font-size: 0.72rem;
  font-weight: 700;
  text-transform: uppercase;
}

.spider-related-name {
  overflow-wrap: anywhere;
  font-size: 0.84rem;
  font-weight: 700;
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

.spider-runtime-detail {
  display: grid;
  gap: 12px;
}

.spider-runtime-overview {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 10px;
}

.spider-runtime-metric {
  display: grid;
  gap: 4px;
  min-width: 0;
  border: 1px solid var(--spider-line);
  border-radius: 8px;
  background: var(--spider-panel);
  padding: 10px 12px;
}

.spider-runtime-metric span {
  color: var(--spider-muted);
  font-size: 0.68rem;
  font-weight: 800;
  letter-spacing: 0;
  text-transform: uppercase;
}

.spider-runtime-metric strong {
  overflow: hidden;
  color: var(--spider-text);
  font-size: 0.9rem;
  font-weight: 700;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.spider-runtime-timeline {
  display: grid;
  gap: 10px;
}

.spider-runtime-span {
  display: grid;
  gap: 6px;
  padding-left: calc(var(--runtime-depth, 0) * 18px);
}

.spider-runtime-span-card,
.spider-runtime-event {
  display: grid;
  grid-template-columns: 18px minmax(0, 1fr) auto;
  align-items: center;
  gap: 10px;
  border: 1px solid var(--spider-line);
  border-radius: 8px;
  background: var(--spider-panel);
  padding: 9px 10px;
}

.spider-runtime-span-card.is-faulted,
.spider-runtime-event.is-faulted {
  border-color: rgba(230, 36, 45, 0.38);
  background: var(--spider-red-soft);
}

.spider-runtime-span-card.is-running,
.spider-runtime-event.is-running {
  border-color: rgba(29, 95, 191, 0.34);
  background: var(--spider-blue-soft);
}

.spider-runtime-span-card.is-completed {
  border-color: rgba(29, 95, 191, 0.22);
}

.spider-runtime-span-main {
  display: grid;
  gap: 4px;
  min-width: 0;
}

.spider-runtime-span-title {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 7px;
  min-width: 0;
}

.spider-runtime-span-title strong {
  overflow: hidden;
  color: var(--spider-text);
  font-size: 0.88rem;
  font-weight: 700;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.spider-runtime-span-title strong.is-unnamed {
  color: var(--spider-muted);
  font-style: italic;
}

.spider-runtime-description {
  display: block;
  overflow: hidden;
  color: var(--spider-muted);
  font-size: 0.76rem;
  line-height: 1.35;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.spider-runtime-span-meta,
.spider-runtime-time,
.spider-runtime-meta {
  color: var(--spider-muted);
  font-size: 0.75rem;
}

.spider-runtime-span-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.spider-runtime-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 5px;
}

.spider-runtime-tag {
  border: 1px solid var(--spider-line);
  border-radius: 999px;
  background: var(--spider-panel-soft);
  color: var(--spider-muted);
  font-size: 0.68rem;
  font-weight: 750;
  line-height: 1;
  padding: 4px 7px;
}

.spider-runtime-span-actions {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 6px;
}

.spider-runtime-marker-list {
  display: grid;
  gap: 5px;
  margin-left: 27px;
}

.spider-runtime-marker {
  display: grid;
  grid-template-columns: 18px minmax(0, 1fr) auto;
  align-items: center;
  gap: 8px;
  border: 1px dashed var(--spider-line);
  border-radius: 8px;
  background: var(--spider-panel-soft);
  color: var(--spider-muted);
  font-size: 0.77rem;
  padding: 7px 9px;
}

.spider-runtime-marker .spider-runtime-dot {
  width: 8px;
  height: 8px;
  border: 0;
  box-shadow: none;
}

.spider-runtime-raw {
  border: 1px solid var(--spider-line);
  border-radius: 8px;
  background: var(--spider-panel);
}

.spider-runtime-raw summary {
  cursor: pointer;
  color: var(--spider-text);
  font-size: 0.86rem;
  font-weight: 700;
  padding: 11px 13px;
}

.spider-runtime-raw .spider-runtime-timeline {
  border-top: 1px solid var(--spider-line);
  padding: 12px;
}

.spider-runtime-name {
  overflow: hidden;
  color: var(--spider-text);
  font-size: 0.86rem;
  font-weight: 650;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.spider-hidden {
  display: none !important;
}

@media (max-width: 1160px) {
  .spider-detail-layout {
    grid-template-columns: minmax(240px, 300px) minmax(320px, 1fr);
    min-height: 0;
  }

  .spider-detail-layout.is-inspector-collapsed {
    grid-template-columns: minmax(240px, 300px) minmax(320px, 1fr);
  }

  .spider-process-summary,
  .spider-graph-panel,
  .spider-node-detail {
    height: auto;
  }

  .spider-graph-wrap {
    max-height: calc(100vh - 220px);
  }

  .spider-node-detail {
    position: static;
    grid-column: 1 / -1;
  }
}

@media (max-width: 940px) {
  .spider-detail-layout {
    grid-template-columns: 1fr;
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

  .spider-sidebar-toggle {
    display: none;
  }

  .spider-menu {
    grid-template-columns: 1fr 1fr;
    padding: 8px;
  }

  .spider-menu-section,
  .spider-sidebar-footer {
    grid-column: 1 / -1;
  }

  .spider-main {
    padding: 16px;
  }

  .spider-view-header,
  .spider-detail-toolbar {
    grid-template-columns: 1fr;
    display: grid;
  }

  .spider-process-list {
    grid-template-columns: 1fr;
  }

  .spider-process-row {
    grid-template-columns: 4px minmax(0, 1fr) auto;
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
  const runtimeElement = document.getElementById("spider-runtime-trace-data");
  const manifest = JSON.parse(manifestElement.textContent || "{}");
  let runtimeData = JSON.parse(runtimeElement.textContent || "{\"summaries\":[],\"traces\":[]}");
  const components = manifest.components || [];
  const relations = manifest.relations || [];
  const byId = new Map(components.map((component) => [component.id, component]));
  const flows = components.filter((component) => component.kind === "spider.flow").sort(compareByName);
  const pipelines = components.filter((component) => component.kind === "spider.pipeline").sort(compareByName);
  const flowCount = document.getElementById("spider-flow-count");
  const pipelineCount = document.getElementById("spider-pipeline-count");
  const runtimeCount = document.getElementById("spider-runtime-count");
  const topbarTitle = document.getElementById("spider-topbar-title");
  const sidebarToggle = document.getElementById("spider-sidebar-toggle");
  const themeToggle = document.getElementById("spider-theme-toggle");
  const themeToggleLabel = document.getElementById("spider-theme-toggle-label");
  const jsonLink = document.getElementById("spider-json-link");
  const sidebarStorageKey = "spider:architecture:sidebar-collapsed";
  const themeStorageKey = "spider:architecture:theme";
  const showGraph = root.dataset.showGraph === "true";
  const showJson = root.dataset.showJson === "true";
  const showRuntime = root.dataset.showRuntime === "true";
  const runtimeEndpoint = root.dataset.runtimeEndpoint || "";
  const state = {
    view: "pipelines",
    mode: "list",
    processId: "",
    nodeId: "",
    query: "",
    runtimeTraceId: "",
    runtimeTraceSignature: "",
    runtimeListSignature: "",
    inspectorCollapsed: false
  };

  flowCount.textContent = String(flows.length);
  pipelineCount.textContent = String(pipelines.length);
  if (runtimeCount) {
    runtimeCount.textContent = String((runtimeData.summaries || []).length);
  }

  if (!showJson && jsonLink) {
    jsonLink.classList.add("spider-hidden");
  }

  setSidebarCollapsed(readSidebarCollapsedPreference());
  setTheme(readThemePreference());

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

    const trace = event.target.closest("[data-open-trace]");
    if (trace) {
      openTrace(trace.getAttribute("data-open-trace"));
      return;
    }

    const back = event.target.closest("[data-back-list]");
    if (back) {
      showList(state.view);
      return;
    }

    const inspectorToggle = event.target.closest("[data-toggle-inspector]");
    if (inspectorToggle) {
      state.inspectorCollapsed = !state.inspectorCollapsed;
      const process = byId.get(state.processId);
      if (process) {
        renderProcessDetail(process, orderChildren(process));
      }
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

  if (sidebarToggle) {
    sidebarToggle.addEventListener("click", () => {
      const collapsed = !root.classList.contains("is-sidebar-collapsed");
      setSidebarCollapsed(collapsed);
      writeSidebarCollapsedPreference(collapsed);
    });
  }

  if (themeToggle) {
    themeToggle.addEventListener("click", () => {
      const nextTheme = root.dataset.theme === "dark" ? "light" : "dark";
      setTheme(nextTheme);
      writeThemePreference(nextTheme);
    });
  }

  window.addEventListener("hashchange", openFromHash);

  openFromHash();

  if (state.mode === "list") {
    renderList(state.view);
  }

  if (showRuntime && runtimeEndpoint) {
    window.setInterval(refreshRuntimeData, 2500);
  }

  function openFromHash() {
    const hash = decodeURIComponent(window.location.hash.replace(/^#\/?/, ""));
    if (!hash) {
      return;
    }

    if (hash === "flows" || hash === "pipelines" || hash === "runtime") {
      showList(hash, true);
      return;
    }

    if (hash.startsWith("runtime:")) {
      openTrace(hash.substring("runtime:".length), true);
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
    state.view = view === "runtime" && showRuntime
      ? "runtime"
      : view === "flows" ? "flows" : "pipelines";
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
    if (view === "runtime") {
      renderRuntimeList();
      return;
    }

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

  function renderRuntimeList() {
    const summaries = runtimeData.summaries || [];
    state.runtimeListSignature = createRuntimeListSignature(summaries);
    setActiveMenu("runtime");
    setTopbarTitle("Runtime traces");

    content.innerHTML = `
      <div class="spider-list-view">
        <header class="spider-view-header">
          <div>
            <h1 class="spider-view-title">Runtime traces</h1>
            <p class="spider-view-description">Recent Spider executions captured while the application runs.</p>
          </div>
          <label class="spider-search" aria-label="Search runtime traces">
            <input id="spider-search" type="search" autocomplete="off" placeholder="Search traces" value="${escapeAttribute(state.query)}" />
          </label>
        </header>
        <div id="spider-runtime-list" class="spider-runtime-execution-list">
          ${renderRuntimeRows(summaries)}
        </div>
      </div>`;

    const search = document.getElementById("spider-search");
    if (search) {
      search.addEventListener("input", () => {
        state.query = search.value.trim().toLowerCase();
        document.getElementById("spider-runtime-list").innerHTML = renderRuntimeRows(runtimeData.summaries || []);
      });
    }
  }

  function renderRuntimeRows(items) {
    const matches = (items || []).filter((item) => {
      if (!state.query) {
        return true;
      }

      return [item.traceId, item.rootDisplayName, item.requestType, item.responseType, item.status]
        .some((value) => String(value || "").toLowerCase().includes(state.query));
    });

    if (matches.length === 0) {
      return `<div class="spider-empty-list">No runtime traces found.</div>`;
    }

    return matches.map((item) => {
      const overview = createRuntimeOverview(getTraceForSummary(item), item);
      const status = overview.status || item.status || "Started";
      const statusClass = getStatusClass(status);
      const fault = overview.firstFault ? `
        <span class="spider-runtime-row-fault">${escapeHtml(overview.firstFault)}</span>` : "";

      return `
      <button class="spider-runtime-row is-${escapeAttribute(statusClass)}" type="button" data-open-trace="${escapeAttribute(item.traceId)}">
        <span class="spider-runtime-row-time">
          <strong>${escapeHtml(formatTime(item.startedAt))}</strong>
          <span>${escapeHtml(formatDate(item.startedAt))}</span>
        </span>
        <span class="spider-runtime-dot" aria-hidden="true"></span>
        <span class="spider-runtime-row-body">
          <span class="spider-process-tags">
            ${renderStatusChip(status)}
            <span class="spider-count-pill">${escapeHtml(formatDuration(overview.durationMs))}</span>
          </span>
          <span class="spider-runtime-row-title">${escapeHtml(overview.title)}</span>
          <span class="spider-runtime-row-meta">
            <span>${escapeHtml(overview.rootKind)}</span>
            ${overview.title === overview.signature ? "" : `<span>${escapeHtml(overview.signature)}</span>`}
            <span>${escapeHtml(overview.shortTraceId)}</span>
          </span>
          ${fault}
        </span>
        <span class="spider-runtime-row-stats">
          <span class="spider-count-pill">${escapeHtml(String(overview.spanCount))} spans</span>
          <span class="spider-count-pill">${escapeHtml(String(overview.eventCount))} events</span>
          ${overview.faultCount ? `<span class="spider-count-pill">${escapeHtml(String(overview.faultCount))} faults</span>` : ""}
          <span class="spider-process-arrow" aria-hidden="true">↗</span>
        </span>
      </button>`;
    }).join("");
  }

  function openTrace(traceId, skipHash) {
    const trace = (runtimeData.traces || []).find((item) => item.traceId === traceId);
    if (!trace) {
      return;
    }

    const signature = createRuntimeTraceSignature(trace);
    if (skipHash && state.runtimeTraceId === traceId && state.runtimeTraceSignature === signature) {
      return;
    }

    state.view = "runtime";
    state.mode = "trace";
    state.runtimeTraceId = traceId;
    state.runtimeTraceSignature = signature;
    if (!skipHash) {
      setHash("runtime:" + traceId);
    }

    if (skipHash) {
      preserveMainScroll(() => renderTraceDetail(trace));
    } else {
      renderTraceDetail(trace);
      resetMainScroll();
    }
  }

  function renderTraceDetail(trace) {
    const overview = createRuntimeOverview(trace, findSummary(trace.traceId));
    setActiveMenu("runtime");
    setTopbarTitle(trace.traceId);

    content.innerHTML = `
      <article class="spider-runtime-detail">
        <header class="spider-detail-toolbar">
          <div>
            <button class="spider-back-button" type="button" data-back-list>Back to Runtime</button>
            <div class="spider-detail-actions">
              ${renderStatusChip(overview.status)}
              <span class="spider-chip">${escapeHtml(formatDuration(overview.durationMs))}</span>
            </div>
            <h1 class="spider-detail-title">${escapeHtml(overview.title)}</h1>
            <p class="spider-detail-subtitle">${escapeHtml(overview.rootKind)} · ${escapeHtml(overview.signature)} · ${escapeHtml(overview.shortTraceId)}</p>
          </div>
        </header>
        <section class="spider-runtime-overview">
          ${renderRuntimeMetric("Started", formatDateTime(overview.startedAt))}
          ${renderRuntimeMetric("Finished", overview.completedAt ? formatDateTime(overview.completedAt) : "Still running")}
          ${renderRuntimeMetric("Duration", formatDuration(overview.durationMs))}
          ${renderRuntimeMetric("Spans", String(overview.spanCount))}
          ${renderRuntimeMetric("Events", String(overview.eventCount))}
          ${renderRuntimeMetric("Faults", String(overview.faultCount))}
          ${renderRuntimeMetric("Flows", String(overview.flowCount))}
          ${renderRuntimeMetric("Boundaries", String(overview.boundaryCount))}
        </section>
        <section class="spider-panel">
          <div class="spider-panel-header">
            <h2>Execution timeline</h2>
            <span class="spider-panel-note">Started paired with terminal events</span>
          </div>
          <div class="spider-runtime-timeline">
            ${overview.rootSpans.length ? overview.rootSpans.map((span) => renderRuntimeSpan(span, 0)).join("") : renderRuntimeRawEvents(trace)}
          </div>
        </section>
        <details class="spider-runtime-raw">
          <summary>Raw events</summary>
          <div class="spider-runtime-timeline">
            ${renderRuntimeRawEvents(trace)}
          </div>
        </details>
      </article>`;
  }

  function renderRuntimeMetric(label, value) {
    return `
      <div class="spider-runtime-metric">
        <span>${escapeHtml(label)}</span>
        <strong title="${escapeAttribute(value)}">${escapeHtml(value)}</strong>
      </div>`;
  }

  function renderRuntimeSpan(span, depth) {
    const statusClass = getStatusClass(span.status);
    const displayName = getRuntimeDisplayName(span);
    const technicalName = getRuntimeTechnicalName(span, displayName);
    const description = getRuntimeDescription(span);
    const tags = getRuntimeTags(span);
    const tooltip = createRuntimeTooltip(span, displayName, description, tags);
    const unnamedClass = isRuntimeUnnamed(span, displayName) ? " is-unnamed" : "";
    const descriptionHtml = description
      ? `<span class="spider-runtime-description">${escapeHtml(description)}</span>`
      : "";
    const tagHtml = tags.length
      ? `<span class="spider-runtime-tags">${tags.map((tag) => `<span class="spider-runtime-tag">${escapeHtml(tag)}</span>`).join("")}</span>`
      : "";
    const technicalHtml = technicalName
      ? `<span>${escapeHtml(technicalName)}</span>`
      : "";
    const markers = span.markers.length
      ? `<div class="spider-runtime-marker-list">${span.markers.map(renderRuntimeMarker).join("")}</div>`
      : "";
    const children = span.children.length
      ? span.children.map((child) => renderRuntimeSpan(child, depth + 1)).join("")
      : "";
    const link = span.componentId && byId.has(span.componentId)
      ? `<button class="spider-related-button" type="button" data-open-process="${escapeAttribute(span.componentId)}"><span class="spider-related-label">Open component</span><span class="spider-related-name">${escapeHtml(byId.get(span.componentId).displayName || span.componentId)}</span></button>`
      : "";

    return `
      <div class="spider-runtime-span" style="--runtime-depth: ${escapeAttribute(String(depth))}">
        <div class="spider-runtime-span-card is-${escapeAttribute(statusClass)}">
          <span class="spider-runtime-dot" aria-hidden="true"></span>
          <span class="spider-runtime-span-main">
            <span class="spider-runtime-span-title">
              <strong class="${unnamedClass.trim()}" title="${escapeAttribute(tooltip)}">${escapeHtml(displayName)}</strong>
              ${renderStatusChip(span.status)}
            </span>
            ${descriptionHtml}
            ${tagHtml}
            <span class="spider-runtime-span-meta">
              <span>${escapeHtml(span.kindLabel)}</span>
              <span>${escapeHtml(span.operation || "operation")}</span>
              ${technicalHtml}
              <span>${escapeHtml(formatTime(span.startedAt))} -> ${escapeHtml(span.completedAt ? formatTime(span.completedAt) : "running")}</span>
              ${span.exception ? `<span>${escapeHtml(span.exception.message)}</span>` : ""}
            </span>
            ${link}
          </span>
          <span class="spider-runtime-span-actions">
            <span class="spider-count-pill">${escapeHtml(formatDuration(span.durationMs))}</span>
          </span>
        </div>
        ${markers}
        ${children}
      </div>`;
  }

  function renderRuntimeMarker(event) {
    const displayName = getRuntimeDisplayName(event);
    const description = getRuntimeDescription(event);
    const tags = getRuntimeTags(event);
    const tooltip = createRuntimeTooltip(event, displayName, description, tags);
    return `
      <div class="spider-runtime-marker is-${escapeAttribute(getStatusClass(event.status))}">
        <span class="spider-runtime-dot" aria-hidden="true"></span>
        <span>
          <span class="spider-runtime-name" title="${escapeAttribute(tooltip)}">${escapeHtml(displayName)}</span>
          <span class="spider-runtime-meta">${escapeHtml(event.kind)} · ${escapeHtml(event.operation || "")}</span>
        </span>
        <span class="spider-runtime-time">${escapeHtml(formatTime(event.timestamp))}</span>
      </div>`;
  }

  function renderRuntimeRawEvents(trace) {
    return (trace.events || []).map(renderRuntimeEvent).join("");
  }

  function renderRuntimeEvent(event) {
    const statusClass = String(event.status || "").toLowerCase();
    const displayName = getRuntimeDisplayName(event);
    const description = getRuntimeDescription(event);
    const tags = getRuntimeTags(event);
    const tooltip = createRuntimeTooltip(event, displayName, description, tags);
    const descriptionHtml = description
      ? `<span class="spider-runtime-meta">${escapeHtml(description)}</span>`
      : "";
    const tagHtml = tags.length
      ? `<span class="spider-runtime-tags">${tags.map((tag) => `<span class="spider-runtime-tag">${escapeHtml(tag)}</span>`).join("")}</span>`
      : "";
    const link = event.componentId && byId.has(event.componentId)
      ? `<button class="spider-related-button" type="button" data-open-process="${escapeAttribute(event.componentId)}"><span class="spider-related-label">Open component</span><span class="spider-related-name">${escapeHtml(byId.get(event.componentId).displayName || event.componentId)}</span></button>`
      : "";

    return `
      <div class="spider-runtime-event is-${escapeAttribute(statusClass)}">
        <span class="spider-runtime-dot" aria-hidden="true"></span>
        <span>
          <span class="spider-runtime-name" title="${escapeAttribute(tooltip)}">${escapeHtml(displayName)}</span>
          ${descriptionHtml}
          ${tagHtml}
          <span class="spider-runtime-meta">${escapeHtml(formatTime(event.timestamp))} · ${escapeHtml(event.kind)} · ${escapeHtml(event.operation || "")}</span>
          ${event.exception ? `<span class="spider-runtime-meta">${escapeHtml(event.exception.message)}</span>` : ""}
          ${link}
        </span>
        ${renderStatusChip(event.status || "Started")}
      </div>`;
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
      const kindLabel = item.kind === "spider.pipeline" ? "Pipeline" : "Flow";
      const kindClass = item.kind === "spider.pipeline" ? "pipeline" : "flow";

      return `
        <button class="spider-process-row ${kindClass}" type="button" data-open-process="${escapeAttribute(item.id)}">
          <span class="spider-process-accent" aria-hidden="true"></span>
          <span class="spider-process-body">
            <span class="spider-process-tags">
              <span class="spider-process-kind ${kindClass}">${escapeHtml(kindLabel)}</span>
              <span class="spider-process-meta">${escapeHtml(getSignature(item))}</span>
            </span>
            <span class="spider-process-name">${escapeHtml(item.displayName || item.id)}</span>
          </span>
          <span class="spider-process-action">
            <span class="spider-count-pill">${escapeHtml(countLabel)}</span>
            <span class="spider-process-arrow" aria-hidden="true">↗</span>
          </span>
        </button>`;
    }).join("");
  }

  function openProcess(id, skipHash) {
    const process = byId.get(id);
    if (!process) {
      return;
    }

    const children = orderChildren(process);
    state.view = process.kind === "spider.flow" ? "flows" : "pipelines";
    state.mode = "detail";
    state.processId = id;
    state.nodeId = children.length ? children[0].id : id;
    if (!skipHash) {
      setHash(id);
    }

    renderProcessDetail(process, children);
    resetMainScroll();
  }

  function renderProcessDetail(process, children) {
    const profiles = getRelated(process.id, "uses-profile");
    const kind = process.kind === "spider.flow" ? "Flow" : "Pipeline";
    const backLabel = process.kind === "spider.flow" ? "Back to Flows" : "Back to Pipelines";
    const graph = showGraph
      ? renderVerticalGraph(process, children)
      : `<div class="spider-empty-list">Graph disabled.</div>`;
    const selectedNode = byId.get(state.nodeId) || process;
    const selectedIndex = selectedNode.id === process.id
      ? 0
      : children.findIndex((child) => child.id === selectedNode.id) + 1;
    const processTags = renderTagChips(process);

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
              ${processTags}
            </div>
            <h1 class="spider-detail-title">${escapeHtml(process.displayName || process.id)}</h1>
            <p class="spider-detail-subtitle">${escapeHtml(describeProcess(process, children))}</p>
          </div>
        </header>
        <div class="spider-detail-layout${state.inspectorCollapsed ? " is-inspector-collapsed" : ""}">
          <aside class="spider-panel spider-process-summary">
            ${renderProcessSummary(process, children, profiles)}
          </aside>
          <section class="spider-panel spider-graph-panel">
            <div class="spider-panel-header">
              <h2>Graph</h2>
              <div class="spider-graph-legend" aria-label="Graph operation legend">
                <span class="spider-legend-item"><i class="spider-legend-swatch"></i>Step</span>
                <span class="spider-legend-item"><i class="spider-legend-swatch branch"></i>Branch</span>
                <span class="spider-legend-item"><i class="spider-legend-swatch route"></i>Route</span>
                <span class="spider-legend-item"><i class="spider-legend-swatch stage"></i>Pipeline</span>
                <span class="spider-legend-item">↗ Linked flow</span>
              </div>
            </div>
            <div class="spider-graph-wrap spider-process-graph">
              ${graph}
              <div id="spider-graph-tooltip" class="spider-graph-tooltip" role="tooltip"></div>
            </div>
          </section>
          <aside id="spider-node-detail" class="spider-node-detail">
            ${renderNodeDetail(selectedNode, process, selectedIndex)}
          </aside>
        </div>
      </article>`;

    bindGraphTooltip();
  }

  function renderProcessSummary(process, children, profiles) {
    const kind = process.kind === "spider.flow" ? "Flow" : "Pipeline";
    const childLabel = process.kind === "spider.flow" ? "Steps" : "Stages";
    const profileText = profiles.length
      ? profiles.map((profile) => profile.displayName || profile.id).join(", ")
      : "None";
    const description = getMetadata(process, "description");
    const tags = getTags(process);

    return `
      <div class="spider-panel-header">
        <h2>${escapeHtml(kind)} summary</h2>
      </div>
      <div class="spider-summary-grid">
        <div class="spider-summary-item">
          <span class="spider-summary-label">Signature</span>
          <span class="spider-summary-value">${escapeHtml(getSignature(process))}</span>
        </div>
        <div class="spider-summary-item">
          <span class="spider-summary-label">${escapeHtml(childLabel)}</span>
          <span class="spider-summary-value">${escapeHtml(String(children.length))}</span>
        </div>
        <div class="spider-summary-item">
          <span class="spider-summary-label">Profiles</span>
          <span class="spider-summary-value">${escapeHtml(profileText)}</span>
        </div>
        ${description ? `
        <div class="spider-summary-item">
          <span class="spider-summary-label">Description</span>
          <span class="spider-summary-value">${escapeHtml(description)}</span>
        </div>` : ""}
        ${tags.length ? `
        <div class="spider-summary-item">
          <span class="spider-summary-label">Tags</span>
          <span class="spider-summary-value">${escapeHtml(tags.join(", "))}</span>
        </div>` : ""}
      </div>
      ${renderProcessRelations(process)}
      ${renderProcessOutline(process, children)}`;
  }

  function renderProcessRelations(process) {
    const invokedFlows = getOutgoing(process.id, "pipeline-invokes-flow");
    const usedByPipelines = getIncoming(process.id, "pipeline-invokes-flow");
    const usedBySteps = getIncoming(process.id, "invokes-flow")
      .map((source) => {
        const owner = findOwningProcess(source.id);
        return owner || source;
      })
      .filter((item, index, items) => items.findIndex((candidate) => candidate.id === item.id) === index);

    const sections = [];
    if (invokedFlows.length) {
      sections.push(renderRelatedButtons("Linked flows", invokedFlows, "Open flow"));
    }

    if (usedByPipelines.length || usedBySteps.length) {
      sections.push(renderRelatedButtons("Referenced by", usedByPipelines.concat(usedBySteps), "Open parent"));
    }

    if (!sections.length) {
      return "";
    }

    return `<div class="spider-detail-section">${sections.join("")}</div>`;
  }

  function renderProcessOutline(process, children) {
    if (!children.length) {
      return "";
    }

    const title = process.kind === "spider.flow" ? "Flow outline" : "Pipeline outline";
    const rows = [];
    children.forEach((child, index) => {
      rows.push(renderOutlineRow(child, String(index + 1).padStart(2, "0"), ""));

      if (child.kind !== "spider.flow-branch") {
        return;
      }

      const routes = getBranchRoutes(child);
      routes.forEach((route, routeIndex) => {
        const routeNumber = `${index + 1}.${routeIndex + 1}`;
        rows.push(renderOutlineRow(route, routeNumber, " is-nested is-route"));

        getRouteSteps(route).forEach((step, stepIndex) => {
          rows.push(renderOutlineRow(step, `${routeNumber}.${stepIndex + 1}`, " is-nested is-route-step"));
        });
      });
    });

    return `
      <div class="spider-outline">
        <div class="spider-outline-title">${escapeHtml(title)}</div>
        ${rows.join("")}
      </div>`;
  }

  function renderOutlineRow(node, index, className) {
    const semanticClass = getGraphClass(node);
    return `
      <button class="spider-outline-row${semanticClass}${className || ""}${node.id === state.nodeId ? " is-selected" : ""}" type="button" data-node-id="${escapeAttribute(node.id)}">
        <span class="spider-outline-index">${escapeHtml(index)}</span>
        <span class="spider-outline-name">${escapeHtml(node.displayName || node.id)}</span>
      </button>`;
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
    const branchWidth = getGraphBranchWidth(children);
    const width = Math.max(420, branchWidth);
    const nodeWidth = 250;
    const nodeHeight = 50;
    const nodeX = Math.round((width - nodeWidth) / 2);
    const top = 16;
    const gap = 74;
    const center = nodeX + (nodeWidth / 2);
    const edges = [];
    const renderedNodes = [];
    let y = top;

    renderedNodes.push(renderGraphNode(process, "0", nodeX, y, nodeWidth, nodeHeight, " is-root", getSignature(process)));
    let previousExit = { x: center, y: y + nodeHeight };
    y += gap;

    for (let index = 0; index < children.length; index++) {
      const node = children[index];
      const number = String(index + 1).padStart(2, "0");

      edges.push(renderGraphEdge(previousExit.x, previousExit.y, center, y));

      if (node.kind === "spider.flow-branch" && getBranchRoutes(node).length) {
        renderedNodes.push(renderGraphNode(node, number, nodeX, y, nodeWidth, nodeHeight, getGraphClass(node), getNodeSubtitle(node)));
        const branchExit = { x: center, y: y + nodeHeight };
        const layout = renderBranchGraph(node, number, branchExit, y + 86, width);
        edges.push(...layout.edges);
        renderedNodes.push(...layout.nodes);
        previousExit = { x: center, y: layout.exitY };
        y = layout.exitY + 48;
        continue;
      }

      renderedNodes.push(renderGraphNode(node, number, nodeX, y, nodeWidth, nodeHeight, getGraphClass(node), getNodeSubtitle(node)));
      previousExit = { x: center, y: y + nodeHeight };
      y += gap;
    }

    const height = Math.max(260, y + 24);

    return `
      <svg class="spider-architecture-graph" style="--spider-graph-width: ${width}px" viewBox="0 0 ${width} ${height}" role="img" aria-label="${escapeAttribute(process.displayName || "Spider process graph")}" preserveAspectRatio="xMidYMin meet">
        <defs>
          <marker id="spider-arrow" viewBox="0 0 10 10" refX="8" refY="5" markerWidth="6" markerHeight="6" orient="auto-start-reverse">
            <path d="M 0 0 L 10 5 L 0 10 z" fill="#9aa4b2"></path>
          </marker>
        </defs>
        ${edges.join("")}
        ${renderedNodes.join("")}
      </svg>`;
  }

  function renderBranchGraph(branch, branchNumber, branchExit, startY, width) {
    const routes = getBranchRoutes(branch);
    const routeWidth = 190;
    const routeHeight = 46;
    const routeGapX = 24;
    const routeGapY = 64;
    const totalWidth = (routes.length * routeWidth) + ((routes.length - 1) * routeGapX);
    const startX = Math.max(18, Math.round((width - totalWidth) / 2));
    const edges = [];
    const nodes = [];
    let maxBottom = startY;

    routes.forEach((route, routeIndex) => {
      const routeX = startX + (routeIndex * (routeWidth + routeGapX));
      const routeCenter = routeX + (routeWidth / 2);
      const routeNumber = `${branchNumber}.${routeIndex + 1}`;
      nodes.push(renderGraphNode(route, routeNumber, routeX, startY, routeWidth, routeHeight, getGraphClass(route), getNodeSubtitle(route)));
      edges.push(renderGraphEdge(branchExit.x, branchExit.y, routeCenter, startY));

      let currentExit = { x: routeCenter, y: startY + routeHeight };
      let stepY = startY + routeGapY;
      const steps = getRouteSteps(route);

      steps.forEach((step, stepIndex) => {
        edges.push(renderGraphEdge(currentExit.x, currentExit.y, routeCenter, stepY));
        nodes.push(renderGraphNode(step, `${routeNumber}.${stepIndex + 1}`, routeX, stepY, routeWidth, routeHeight, " is-route-step", getNodeSubtitle(step)));
        currentExit = { x: routeCenter, y: stepY + routeHeight };
        stepY += routeGapY;
      });

      maxBottom = Math.max(maxBottom, currentExit.y);
      route._spiderGraphExit = currentExit;
    });

    const exitY = maxBottom + 34;
    routes.forEach((route) => {
      const exit = route._spiderGraphExit;
      if (exit) {
        edges.push(renderGraphEdge(exit.x, exit.y, branchExit.x, exitY));
        delete route._spiderGraphExit;
      }
    });

    return { edges, nodes, exitY };
  }

  function renderGraphNode(node, number, x, y, width, height, extraClass, subtitle) {
    const selected = node.id === state.nodeId ? " is-selected" : "";
    const linkedFlow = getFirstLinkedFlow(node);
    const linkedClass = linkedFlow ? " is-linked-flow" : "";
    const graphClass = (extraClass || "") + linkedClass;
    const numberText = String(number || "");
    const textX = numberText.length > 4 ? 58 : 43;
    const titleFontSize = numberText.length > 4 ? 9.4 : 10;
    const subtitleFontSize = numberText.length > 4 ? 8.5 : 8.8;
    const titleLimit = width > 220
      ? (linkedFlow ? 23 : 30)
      : (linkedFlow ? 17 : 21);
    const subtitleLimit = width > 220
      ? (linkedFlow ? 27 : 34)
      : (linkedFlow ? 19 : 24);
    const tooltipAttributes = renderGraphTooltipAttributes(node);
    const nativeTitle = tooltipAttributes
      ? ""
      : `<title>${escapeHtml(node.displayName || node.id)}</title>`;
    const linkBadge = linkedFlow
      ? `
        <g class="spider-graph-link" data-open-process="${escapeAttribute(linkedFlow.id)}" transform="translate(${width - 20}, 9)">
          <title>Open related flow: ${escapeHtml(linkedFlow.displayName || linkedFlow.id)}</title>
          <circle class="spider-link-dot" cx="8" cy="8" r="8"></circle>
          <text x="5" y="11">↗</text>
        </g>`
      : "";
    return `
      <g class="spider-graph-node${graphClass}${selected}" data-node-id="${escapeAttribute(node.id)}"${tooltipAttributes} transform="translate(${x}, ${y})">
        ${nativeTitle}
        <rect class="spider-node-box" width="${width}" height="${height}" rx="7"></rect>
        <rect class="spider-node-accent" width="4" height="${height}" rx="2"></rect>
        <text class="spider-node-index" x="14" y="${height > 48 ? 30 : 28}" font-size="9.5" font-weight="700">${escapeHtml(numberText)}</text>
        <text x="${textX}" y="${height > 48 ? 21 : 20}" font-size="${titleFontSize}" font-weight="650">${escapeHtml(truncate(node.displayName || node.id, titleLimit))}</text>
        <text class="spider-node-subtitle" x="${textX}" y="${height > 48 ? 37 : 35}" font-size="${subtitleFontSize}">${escapeHtml(truncate(subtitle, subtitleLimit))}</text>
        ${linkBadge}
      </g>`;
  }

  function renderGraphEdge(fromX, fromY, toX, toY) {
    const midY = fromY + Math.max(16, Math.round((toY - fromY) / 2));
    return `<path class="spider-edge" d="M ${fromX} ${fromY} C ${fromX} ${midY}, ${toX} ${midY}, ${toX} ${toY - 7}" marker-end="url(#spider-arrow)" />`;
  }

  function bindGraphTooltip() {
    const graphWrap = content.querySelector(".spider-graph-wrap");
    const tooltip = document.getElementById("spider-graph-tooltip");
    if (!graphWrap || !tooltip) {
      return;
    }

    graphWrap.addEventListener("mouseover", (event) => {
      const node = findTooltipNode(event.target, graphWrap);
      if (node) {
        showGraphTooltip(tooltip, node, event);
      }
    });

    graphWrap.addEventListener("mousemove", (event) => {
      const node = findTooltipNode(event.target, graphWrap);
      if (node) {
        moveGraphTooltip(tooltip, event);
      }
    });

    graphWrap.addEventListener("mouseout", (event) => {
      const node = findTooltipNode(event.target, graphWrap);
      if (node && (!event.relatedTarget || !node.contains(event.relatedTarget))) {
        hideGraphTooltip(tooltip);
      }
    });

    graphWrap.addEventListener("focusin", (event) => {
      const node = findTooltipNode(event.target, graphWrap);
      if (node) {
        showGraphTooltip(tooltip, node, null);
      }
    });

    graphWrap.addEventListener("focusout", () => hideGraphTooltip(tooltip));
  }

  function findTooltipNode(target, graphWrap) {
    if (!target || !target.closest) {
      return null;
    }

    const node = target.closest(".spider-graph-node");
    if (!node || !graphWrap.contains(node)) {
      return null;
    }

    return hasGraphTooltip(node) ? node : null;
  }

  function hasGraphTooltip(node) {
    return Boolean(node.dataset.tooltipName || node.dataset.tooltipDescription || node.dataset.tooltipTags);
  }

  function showGraphTooltip(tooltip, node, event) {
    tooltip.innerHTML = renderGraphTooltipContent(node.dataset);
    tooltip.classList.add("is-visible");

    if (event) {
      moveGraphTooltip(tooltip, event);
      return;
    }

    const rect = node.getBoundingClientRect();
    moveGraphTooltipToPoint(tooltip, rect.right + 8, rect.top + 8);
  }

  function moveGraphTooltip(tooltip, event) {
    moveGraphTooltipToPoint(tooltip, event.clientX + 12, event.clientY + 12);
  }

  function moveGraphTooltipToPoint(tooltip, preferredLeft, preferredTop) {
    const margin = 8;
    const rect = tooltip.getBoundingClientRect();
    let left = preferredLeft;
    let top = preferredTop;

    if (left + rect.width + margin > window.innerWidth) {
      left = preferredLeft - rect.width - 24;
    }

    if (top + rect.height + margin > window.innerHeight) {
      top = preferredTop - rect.height - 24;
    }

    tooltip.style.left = `${Math.max(margin, left)}px`;
    tooltip.style.top = `${Math.max(margin, top)}px`;
  }

  function hideGraphTooltip(tooltip) {
    tooltip.classList.remove("is-visible");
  }

  function renderGraphTooltipContent(dataset) {
    const rows = [];
    if (dataset.tooltipName) {
      rows.push(renderTooltipRow("Name", dataset.tooltipName));
    }

    if (dataset.tooltipDescription) {
      rows.push(renderTooltipRow("Description", dataset.tooltipDescription));
    }

    if (dataset.tooltipTags) {
      rows.push(renderTooltipRow("Tags", dataset.tooltipTags));
    }

    return rows.join("");
  }

  function renderTooltipRow(label, value) {
    return `
      <div class="spider-tooltip-row">
        <div class="spider-tooltip-label">${escapeHtml(label)}</div>
        <div class="spider-tooltip-value">${escapeHtml(value)}</div>
      </div>`;
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
    document.querySelectorAll(".spider-outline-row").forEach((item) => {
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
    const evidence = node.evidence || [];
    const source = evidence.length && evidence[0].filePath
      ? `${evidence[0].filePath}${evidence[0].lineNumber ? ":" + evidence[0].lineNumber : ""}`
      : "";
    const rows = createDetailRows(node, index);
    const evidenceLabels = getEvidenceLabels(node);
    const declaredIn = evidence.length
      ? formatDeclaredIn(evidence[0])
      : "";
    const evidenceRows = evidence.length
      ? `
        <div class="spider-detail-section spider-evidence">
          <dl class="spider-definition">
            <dt>${escapeHtml(evidenceLabels.declaredIn)}</dt><dd>${escapeHtml(declaredIn || "Not available")}</dd>
            <dt>${escapeHtml(evidenceLabels.sourceFile)}</dt><dd>${escapeHtml(source || "Not available")}</dd>
          </dl>
        </div>`
      : "";
    const branchRows = renderBranchDetail(node);
    const relationRows = renderNodeRelations(node, process);
    const description = getMetadata(node, "description");
    const tagChips = renderTagChips(node);

    return `
      <div class="spider-node-detail-top">
        <div class="spider-node-header">
          <span class="spider-node-kind ${getKindClass(node)}">${escapeHtml(getFriendlyKind(node))}</span>
          <h2>${escapeHtml(node.displayName || node.id)}</h2>
          ${description ? `<p class="spider-node-description">${escapeHtml(description)}</p>` : ""}
          ${tagChips ? `<div class="spider-node-tags">${tagChips}</div>` : ""}
        </div>
        <button class="spider-inspector-toggle" type="button" data-toggle-inspector title="${state.inspectorCollapsed ? "Show details" : "Hide details"}" aria-label="${state.inspectorCollapsed ? "Show details" : "Hide details"}">${state.inspectorCollapsed ? "i" : "×"}</button>
      </div>
      <div class="spider-node-detail-body">
        <dl class="spider-definition">
          ${rows.map(([label, value]) => `<dt>${escapeHtml(label)}</dt><dd>${escapeHtml(value || "Not declared")}</dd>`).join("")}
        </dl>
        ${branchRows}
        ${relationRows}
        ${evidenceRows}
      </div>`;
  }

  function renderBranchDetail(node) {
    if (node.kind === "spider.flow-branch") {
      const routes = getBranchRoutes(node);
      if (!routes.length) {
        return "";
      }

      return `
        <div class="spider-detail-section">
          ${renderRelatedButtons("Branch routes", routes, "Inspect route", "node")}
        </div>`;
    }

    if (node.kind === "spider.flow-branch-route") {
      const steps = getRouteSteps(node);
      if (!steps.length) {
        return "";
      }

      return `
        <div class="spider-detail-section">
          ${renderRelatedButtons("Route steps", steps, "Inspect step", "node")}
        </div>`;
    }

    return "";
  }

  function renderNodeRelations(node, process) {
    const outgoing = getOutgoing(node.id, "invokes-flow").concat(getOutgoing(node.id, "pipeline-invokes-flow"));
    const incoming = getIncoming(node.id, "invokes-flow").concat(getIncoming(node.id, "pipeline-invokes-flow"))
      .map((source) => findOwningProcess(source.id) || source)
      .filter((item, index, items) => items.findIndex((candidate) => candidate.id === item.id) === index);
    const sections = [];

    if (outgoing.length) {
      sections.push(renderRelatedButtons("Opens related flow", outgoing, "Open flow"));
    }

    if (incoming.length && node.id === process.id) {
      sections.push(renderRelatedButtons("Referenced by", incoming, "Open parent"));
    }

    if (!sections.length) {
      return "";
    }

    return `<div class="spider-detail-section">${sections.join("")}</div>`;
  }

  function renderRelatedButtons(title, items, action, mode) {
    if (!items.length) {
      return "";
    }

    const attribute = mode === "node" ? "data-node-id" : "data-open-process";
    return `
      <div class="spider-related-list">
        <div class="spider-summary-label">${escapeHtml(title)}</div>
        ${items.map((item) => `
          <button class="spider-related-button" type="button" ${attribute}="${escapeAttribute(item.id)}">
            <span class="spider-related-label">${escapeHtml(action)}</span>
            <span class="spider-related-name">${escapeHtml(item.displayName || item.id)}</span>
          </button>`).join("")}
      </div>`;
  }

  function getEvidenceLabels(node) {
    const action = getMetadata(node, "delegate");
    if (action === "lambda") {
      return {
        declaredIn: "Lambda declared in",
        sourceFile: "Lambda source file"
      };
    }

    if (node.kind === "spider.flow-condition" && action) {
      return {
        declaredIn: "Condition declared in",
        sourceFile: "Condition source file"
      };
    }

    if (node.kind === "spider.flow-step" && action) {
      return {
        declaredIn: "Action declared in",
        sourceFile: "Action source file"
      };
    }

    return {
      declaredIn: "Configured in",
      sourceFile: "Configuration source file"
    };
  }

  function createDetailRows(node, index) {
    const rows = [
      ["Component type", getFriendlyKind(node)],
      ["Position", index === 0 ? "Root" : String(index)],
      ["Component id", node.id]
    ];

    const metadata = Object.entries(node.metadata || {});
    for (const [key, value] of metadata) {
      if (key === "name" || key === "description" || key === "tags") {
        continue;
      }

      const label = getMetadataLabel(key);
      if (!label) {
        continue;
      }

      rows.push([label, formatMetadataValue(key, value)]);
    }

    return rows;
  }

  function getMetadataLabel(key) {
    const labels = {
      branchType: "Branch type",
      condition: "Condition",
      count: "Configured actions",
      delegate: "Action",
      genericArguments: "Type arguments",
      hasOverride: "Override configured",
      hasResponse: "Returns value",
      operation: "Fluent call",
      otherwise: "Otherwise",
      request: "Input",
      response: "Output",
      routeKind: "Route type",
      service: "Service",
      stage: "Pipeline stage",
      target: "Target method"
    };

    if (key === "order" ||
        key === "actionSymbolId" ||
        key === "conditionSymbolId" ||
        key === "declaringMemberSymbolId" ||
        key === "targetSymbolId") {
      return "";
    }

    return labels[key] || formatLabel(key);
  }

  function formatMetadataValue(key, value) {
    if (key === "hasResponse" || key === "hasOverride") {
      return value === "true" || value === "True" ? "Yes" : "No";
    }

    return value;
  }

  function formatDeclaredIn(evidence) {
    const typeName = evidence && evidence.typeName ? evidence.typeName : "";
    const memberName = evidence && evidence.memberName ? evidence.memberName : "";

    if (typeName && memberName) {
      return `${typeName}.${memberName}`;
    }

    return typeName || memberName;
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

  function getBranchRoutes(branch) {
    return orderRelatedChildren(branch.id, "branch-route", "");
  }

  function getRouteSteps(route) {
    return orderRelatedChildren(route.id, "route-contains", "route-next");
  }

  function orderRelatedChildren(sourceId, relationKind, nextKind) {
    const children = relations
      .filter((relation) => relation.kind === relationKind && relation.sourceId === sourceId)
      .map((relation) => byId.get(relation.targetId))
      .filter(Boolean);

    if (children.length <= 1) {
      return children;
    }

    const allHaveOrder = children.every((child) => Number.isFinite(Number(getMetadata(child, "order"))));
    if (allHaveOrder) {
      return children.slice().sort((left, right) => Number(getMetadata(left, "order")) - Number(getMetadata(right, "order")));
    }

    if (!nextKind) {
      return children;
    }

    const ids = new Set(children.map((child) => child.id));
    const next = relations.filter((relation) => relation.kind === nextKind && ids.has(relation.sourceId) && ids.has(relation.targetId));
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

  function getGraphBranchWidth(children) {
    let width = 360;
    for (const child of children) {
      if (child.kind !== "spider.flow-branch") {
        continue;
      }

      const routeCount = getBranchRoutes(child).length;
      if (routeCount > 1) {
        width = Math.max(width, (routeCount * 190) + ((routeCount - 1) * 24) + 48);
      }
    }

    return width;
  }

  function getRelated(sourceId, kind) {
    return relations
      .filter((relation) => relation.kind === kind && relation.sourceId === sourceId)
      .map((relation) => byId.get(relation.targetId))
      .filter(Boolean)
      .sort(compareByName);
  }

  function getOutgoing(sourceId, kind) {
    return getRelated(sourceId, kind);
  }

  function getIncoming(targetId, kind) {
    return relations
      .filter((relation) => relation.kind === kind && relation.targetId === targetId)
      .map((relation) => byId.get(relation.sourceId))
      .filter(Boolean)
      .sort(compareByName);
  }

  function findOwningProcess(componentId) {
    const visited = new Set();
    let currentId = componentId;

    while (currentId && !visited.has(currentId)) {
      visited.add(currentId);
      const current = byId.get(currentId);
      if (current && (current.kind === "spider.flow" || current.kind === "spider.pipeline")) {
        return current;
      }

      const parentRelation = relations.find((relation) =>
        (relation.kind === "contains" || relation.kind === "branch-route" || relation.kind === "route-contains") &&
        relation.targetId === currentId);
      currentId = parentRelation ? parentRelation.sourceId : "";
    }

    return null;
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
      for (const route of getBranchRoutes(child)) {
        values.push(route.id, route.kind, route.displayName, ...Object.values(route.metadata || {}));
        for (const step of getRouteSteps(route)) {
          values.push(step.id, step.kind, step.displayName, ...Object.values(step.metadata || {}));
        }
      }
    }

    return values.filter(Boolean).join(" ").toLowerCase();
  }

  function describeProcess(process, children) {
    const description = getMetadata(process, "description");
    if (description) {
      return description;
    }

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

    if (component.kind === "spider.flow-branch-route") {
      return "Branch route";
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

    if (component.kind === "spider.flow-branch-route") {
      return "spider-kind-branch";
    }

    return component.kind === "spider.pipeline-stage" ? "spider-kind-stage" : "spider-kind-step";
  }

  function getGraphClass(component) {
    if (component.kind === "spider.pipeline-stage") {
      return " is-stage";
    }

    if (component.kind === "spider.flow-condition") {
      return " is-condition";
    }

    if (component.kind === "spider.flow-branch") {
      return " is-branch";
    }

    if (component.kind === "spider.flow-branch-route") {
      return " is-route";
    }

    return "";
  }

  function getFirstLinkedFlow(component) {
    const linked = getOutgoing(component.id, component.kind === "spider.pipeline" ? "pipeline-invokes-flow" : "invokes-flow");
    if (linked.length) {
      return linked[0];
    }

    const pipelineLinked = getOutgoing(component.id, "pipeline-invokes-flow");
    return pipelineLinked.length ? pipelineLinked[0] : null;
  }

  function getMetadata(component, key) {
    return component && component.metadata ? component.metadata[key] || "" : "";
  }

  function renderGraphTooltipAttributes(component) {
    const name = getMetadata(component, "name");
    const description = getMetadata(component, "description");
    const tags = getTags(component);
    if (!name && !description && !tags.length) {
      return "";
    }

    const accessible = [
      name,
      description,
      tags.length ? `Tags: ${tags.join(", ")}` : ""
    ].filter(Boolean).join(" - ");

    return [
      ` tabindex="0"`,
      ` aria-label="${escapeAttribute(accessible)}"`,
      name ? ` data-tooltip-name="${escapeAttribute(name)}"` : "",
      description ? ` data-tooltip-description="${escapeAttribute(description)}"` : "",
      tags.length ? ` data-tooltip-tags="${escapeAttribute(tags.join(", "))}"` : ""
    ].join("");
  }

  function getTags(component) {
    const raw = getMetadata(component, "tags");
    if (!raw) {
      return [];
    }

    const value = String(raw).trim();
    if (!value) {
      return [];
    }

    if (value.startsWith("[")) {
      try {
        const parsed = JSON.parse(value);
        if (Array.isArray(parsed)) {
          return parsed.map((item) => String(item).trim()).filter(Boolean);
        }
      } catch {
        // Fall through to comma-separated tags for older manifests.
      }
    }

    return value.split(",").map((tag) => tag.trim()).filter(Boolean);
  }

  function renderTagChips(component) {
    return getTags(component)
      .map((tag) => `<span class="spider-chip tag">${escapeHtml(tag)}</span>`)
      .join("");
  }

  function createRuntimeOverview(trace, summary) {
    const events = trace && Array.isArray(trace.events) ? trace.events : [];
    const spans = buildRuntimeSpans(events);
    const rootSpans = spans.filter((span) => !span.parentSpanId || !spans.some((candidate) => candidate.spanId === span.parentSpanId));
    const firstEvent = events[0] || null;
    const rootSpan = rootSpans[0] || spans[0] || null;
    const rootEvent = rootSpan ? rootSpan.startEvent || rootSpan.terminalEvent : firstEvent;
    const status = trace && trace.status ? trace.status : summary && summary.status ? summary.status : rootSpan && rootSpan.status ? rootSpan.status : "Started";
    const startedAt = trace && trace.startedAt ? trace.startedAt : summary && summary.startedAt ? summary.startedAt : rootEvent && rootEvent.timestamp;
    const completedAt = trace && trace.completedAt ? trace.completedAt : summary && summary.completedAt ? summary.completedAt : rootSpan && rootSpan.completedAt;
    const durationMs = trace && trace.durationMs !== undefined && trace.durationMs !== null
      ? trace.durationMs
      : summary && summary.durationMs !== undefined && summary.durationMs !== null
        ? summary.durationMs
        : calculateDurationMs(startedAt, completedAt);
    const inputType = summary && summary.requestType ? summary.requestType : rootEvent && rootEvent.inputType;
    const outputType = summary && summary.responseType ? summary.responseType : rootEvent && rootEvent.outputType;
    const faultEvents = events.filter((event) => event.exception || normalizeStatus(event.status) === "faulted");
    const firstFault = faultEvents.length && faultEvents[0].exception ? faultEvents[0].exception.message : "";

    return {
      trace,
      summary,
      rootSpans,
      title: summary && summary.rootDisplayName
        ? summary.rootDisplayName
        : rootEvent && rootEvent.displayName
          ? rootEvent.displayName
          : trace && trace.traceId
            ? trace.traceId
            : summary && summary.traceId
              ? summary.traceId
              : "Runtime trace",
      status,
      startedAt,
      completedAt,
      durationMs,
      signature: getRuntimeSignature(inputType, outputType),
      rootKind: rootSpan && rootSpan.kindLabel ? rootSpan.kindLabel : "Runtime",
      shortTraceId: shortTraceId(trace && trace.traceId ? trace.traceId : summary && summary.traceId),
      spanCount: spans.length,
      eventCount: events.length || (summary && summary.eventCount) || 0,
      faultCount: faultEvents.length,
      flowCount: countRuntimeSpans(spans, "flow"),
      boundaryCount: countRuntimeSpans(spans, "boundary"),
      firstFault
    };
  }

  function countRuntimeSpans(spans, kind) {
    return spans.filter((span) => {
      const componentKind = String(span.componentKind || "");
      if (kind === "flow") {
        return componentKind === "spider.flow";
      }

      if (kind === "boundary") {
        return componentKind.includes("boundary");
      }

      return componentKind === kind;
    }).length;
  }

  function buildRuntimeSpans(events) {
    const ordered = (events || []).slice().sort(compareRuntimeEvents);
    const bySpan = new Map();

    for (const event of ordered) {
      if (!event.spanId) {
        continue;
      }

      const span = ensureRuntimeSpan(bySpan, event);
      if (isRuntimeStartEvent(event) && !span.startEvent) {
        span.startEvent = event;
      } else if (isRuntimeTerminalEvent(event)) {
        span.terminalEvent = event;
      } else {
        span.markers.push(event);
      }

      if (event.exception && !span.exception) {
        span.exception = event.exception;
      }
    }

    const spans = Array.from(bySpan.values()).map((span) => finalizeRuntimeSpan(span));
    const byId = new Map(spans.map((span) => [span.spanId, span]));

    for (const span of spans) {
      const parent = span.parentSpanId ? byId.get(span.parentSpanId) : null;
      if (parent && parent !== span) {
        parent.children.push(span);
      }
    }

    for (const span of spans) {
      span.children.sort(compareRuntimeSpans);
      span.markers.sort(compareRuntimeEvents);
    }

    return spans.sort(compareRuntimeSpans);
  }

  function ensureRuntimeSpan(bySpan, event) {
    let span = bySpan.get(event.spanId);
    if (span) {
      if (!span.parentSpanId && event.parentSpanId) {
        span.parentSpanId = event.parentSpanId;
      }

      span.tags = mergeRuntimeMetadata(span.tags, event.tags);
      span.metadata = mergeRuntimeMetadata(span.metadata, event.metadata);
      return span;
    }

    span = {
      traceId: event.traceId,
      spanId: event.spanId,
      parentSpanId: event.parentSpanId || "",
      componentId: event.componentId || "",
      componentKind: event.componentKind || "",
      displayName: event.displayName || event.operation || event.kind || "Runtime operation",
      operation: event.operation || "",
      inputType: event.inputType || "",
      outputType: event.outputType || "",
      tags: event.tags || {},
      metadata: event.metadata || {},
      startEvent: null,
      terminalEvent: null,
      markers: [],
      children: [],
      exception: event.exception || null
    };
    bySpan.set(event.spanId, span);
    return span;
  }

  function finalizeRuntimeSpan(span) {
    const start = span.startEvent;
    const terminal = span.terminalEvent;
    const representative = start || terminal || span.markers[0] || {};

    span.componentId = span.componentId || representative.componentId || "";
    span.componentKind = span.componentKind || representative.componentKind || "";
    span.displayName = representative.displayName || span.displayName || "Runtime operation";
    span.operation = representative.operation || span.operation || "";
    span.tags = mergeRuntimeMetadata(span.tags, representative.tags);
    span.metadata = mergeRuntimeMetadata(span.metadata, representative.metadata);
    span.kindLabel = getRuntimeKindLabel(span.componentKind);
    span.status = terminal && terminal.status
      ? terminal.status
      : start && start.status
        ? start.status
        : representative.status || "Started";
    span.startedAt = start && start.timestamp ? start.timestamp : representative.timestamp;
    span.completedAt = terminal && terminal.timestamp ? terminal.timestamp : "";
    span.durationMs = terminal && terminal.durationMs !== undefined && terminal.durationMs !== null
      ? terminal.durationMs
      : calculateDurationMs(span.startedAt, span.completedAt);
    span.exception = span.exception || terminal && terminal.exception || null;
    return span;
  }

  function compareRuntimeSpans(left, right) {
    return compareRuntimeValues(left.startedAt, right.startedAt);
  }

  function compareRuntimeEvents(left, right) {
    return compareRuntimeValues(left.timestamp, right.timestamp);
  }

  function compareRuntimeValues(left, right) {
    const leftTime = new Date(left || 0).getTime();
    const rightTime = new Date(right || 0).getTime();
    return leftTime - rightTime;
  }

  function isRuntimeStartEvent(event) {
    const kind = String(event.kind || "");
    return kind === "TraceStarted" || kind.endsWith("Started");
  }

  function isRuntimeTerminalEvent(event) {
    const status = normalizeStatus(event.status);
    if (status === "completed" || status === "faulted" || status === "cancelled") {
      return !isRuntimeStartEvent(event);
    }

    const kind = String(event.kind || "");
    return kind.endsWith("Completed") || kind.endsWith("Faulted") || kind.endsWith("Cancelled");
  }

  function getRuntimeKindLabel(kind) {
    const value = String(kind || "");
    if (value.includes("pipeline-stage")) {
      return "Pipeline stage";
    }

    if (value.includes("pipeline")) {
      return "Pipeline";
    }

    if (value.includes("boundary")) {
      return "Boundary";
    }

    if (value.includes("flow-branch-route")) {
      return "Branch route";
    }

    if (value.includes("flow-branch")) {
      return "Branch";
    }

    if (value.includes("flow-condition")) {
      return "Condition";
    }

    if (value.includes("flow-step")) {
      return "Step";
    }

    if (value.includes("flow")) {
      return "Flow";
    }

    return "Operation";
  }

  function getRuntimeDisplayName(item) {
    const metadataName = getRuntimeMetadataValue(item, "name");
    if (metadataName) {
      return metadataName;
    }

    const displayName = String(item && item.displayName ? item.displayName : "").trim();
    if (displayName && displayName.toLowerCase() !== "lambda") {
      return displayName;
    }

    return item && item.componentKind && String(item.componentKind).includes("flow")
      ? "Unnamed step"
      : displayName || item && item.operation || item && item.kind || "Runtime operation";
  }

  function getRuntimeTechnicalName(item, displayName) {
    const original = String(item && item.displayName ? item.displayName : "").trim();
    if (!original || original === displayName) {
      return "";
    }

    return original.toLowerCase() === "lambda"
      ? "delegate: lambda"
      : original;
  }

  function getRuntimeDescription(item) {
    return getRuntimeMetadataValue(item, "description");
  }

  function getRuntimeTags(item) {
    const tags = [];
    const directTags = item && item.tags ? item.tags : {};
    Object.keys(directTags || {}).forEach((key) => {
      const value = directTags[key];
      if (typeof value === "string" && value.trim()) {
        tags.push(value.trim());
      } else if (key && key.trim()) {
        tags.push(key.trim());
      }
    });

    const metadataTags = getRuntimeMetadataValue(item, "tags");
    if (metadataTags) {
      metadataTags.split(",").map((tag) => tag.trim()).filter(Boolean).forEach((tag) => tags.push(tag));
    }

    return Array.from(new Set(tags));
  }

  function getRuntimeMetadataValue(item, key) {
    const metadata = item && item.metadata ? item.metadata : {};
    const value = metadata[key];
    return typeof value === "string" && value.trim() ? value.trim() : "";
  }

  function createRuntimeTooltip(item, displayName, description, tags) {
    const values = [];
    if (displayName) {
      values.push(displayName);
    }

    if (description) {
      values.push(description);
    }

    if (tags && tags.length) {
      values.push(`Tags: ${tags.join(", ")}`);
    }

    const technicalName = getRuntimeTechnicalName(item, displayName);
    if (technicalName) {
      values.push(technicalName);
    }

    return values.join("\\n");
  }

  function isRuntimeUnnamed(item, displayName) {
    return displayName === "Unnamed step" && !getRuntimeMetadataValue(item, "name");
  }

  function mergeRuntimeMetadata(left, right) {
    return Object.assign({}, left || {}, right || {});
  }

  function getRuntimeSignature(inputType, outputType) {
    const input = shortName(inputType || "");
    const output = shortName(outputType || "");
    if (input && output) {
      return `${input} -> ${output}`;
    }

    return input || output || "No request metadata";
  }

  function getTraceForSummary(summary) {
    return summary && summary.traceId
      ? (runtimeData.traces || []).find((trace) => trace.traceId === summary.traceId) || null
      : null;
  }

  function findSummary(traceId) {
    return (runtimeData.summaries || []).find((summary) => summary.traceId === traceId) || null;
  }

  function renderStatusChip(status) {
    const value = status || "Started";
    return `<span class="spider-status-chip spider-status-${escapeAttribute(getStatusClass(value))}">${escapeHtml(value)}</span>`;
  }

  function getStatusClass(status) {
    const value = normalizeStatus(status);
    if (value === "faulted" || value === "cancelled" || value === "completed" || value === "running") {
      return value;
    }

    return "started";
  }

  function normalizeStatus(status) {
    return String(status || "Started").toLowerCase();
  }

  function calculateDurationMs(startedAt, completedAt) {
    if (!startedAt || !completedAt) {
      return null;
    }

    const start = new Date(startedAt).getTime();
    const end = new Date(completedAt).getTime();
    if (Number.isNaN(start) || Number.isNaN(end)) {
      return null;
    }

    return Math.max(0, end - start);
  }

  async function refreshRuntimeData() {
    try {
      const response = await fetch(runtimeEndpoint, { headers: { "accept": "application/json" } });
      if (!response.ok) {
        return;
      }

      runtimeData = await response.json();
      if (runtimeCount) {
        runtimeCount.textContent = String((runtimeData.summaries || []).length);
      }

      if (state.view === "runtime" && state.mode === "list") {
        refreshRuntimeListRows();
      } else if (state.view === "runtime" && state.mode === "trace") {
        const hash = decodeURIComponent(window.location.hash.replace(/^#\/?/, ""));
        const traceId = hash.startsWith("runtime:") ? hash.substring("runtime:".length) : "";
        if (traceId) {
          openTrace(traceId, true);
        }
      }
    } catch {
      // Runtime refresh is best-effort; stale UI data is safer than breaking navigation.
    }
  }

  function refreshRuntimeListRows() {
    const summaries = runtimeData.summaries || [];
    const signature = createRuntimeListSignature(summaries);
    if (state.runtimeListSignature === signature) {
      return;
    }

    state.runtimeListSignature = signature;
    const list = document.getElementById("spider-runtime-list");
    if (list) {
      list.innerHTML = renderRuntimeRows(summaries);
    }
  }

  function createRuntimeListSignature(summaries) {
    return (summaries || [])
      .map((item) => [
        item.traceId,
        item.status,
        item.eventCount,
        item.durationMs,
        item.completedAt
      ].join("|"))
      .join(";");
  }

  function createRuntimeTraceSignature(trace) {
    return [
      trace.traceId,
      trace.status,
      trace.durationMs,
      (trace.events || []).length,
      (trace.events || []).map((item) => `${item.spanId}:${item.kind}:${item.status}:${item.timestamp}`).join("|")
    ].join(";");
  }

  function getTraceTitle(trace) {
    const first = (trace.events || [])[0];
    return first ? first.displayName || first.operation || trace.traceId : trace.traceId;
  }

  function formatDuration(value) {
    if (value === null || value === undefined) {
      return "running";
    }

    const milliseconds = Number(value);
    if (!Number.isFinite(milliseconds)) {
      return "running";
    }

    return milliseconds < 1000
      ? `${Math.round(milliseconds)} ms`
      : `${(milliseconds / 1000).toFixed(2)} s`;
  }

  function formatTime(value) {
    if (!value) {
      return "";
    }

    const date = new Date(value);
    return Number.isNaN(date.getTime())
      ? String(value)
      : date.toLocaleTimeString();
  }

  function formatDate(value) {
    if (!value) {
      return "";
    }

    const date = new Date(value);
    return Number.isNaN(date.getTime())
      ? String(value)
      : date.toLocaleDateString();
  }

  function formatDateTime(value) {
    if (!value) {
      return "Not available";
    }

    const date = new Date(value);
    return Number.isNaN(date.getTime())
      ? String(value)
      : `${date.toLocaleDateString()} ${date.toLocaleTimeString()}`;
  }

  function shortTraceId(value) {
    const text = String(value || "");
    return text.length > 12 ? `Trace ${text.slice(0, 12)}` : text || "Trace";
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

  function shortPath(value) {
    const text = String(value || "");
    if (!text || text === "Not available") {
      return text;
    }

    const normalized = text.replace(/\\/g, "/");
    const parts = normalized.split("/");
    if (parts.length <= 2) {
      return text;
    }

    return parts.slice(-2).join("/");
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

  function setSidebarCollapsed(collapsed) {
    root.classList.toggle("is-sidebar-collapsed", collapsed);

    if (!sidebarToggle) {
      return;
    }

    sidebarToggle.setAttribute("aria-expanded", String(!collapsed));
    sidebarToggle.setAttribute("aria-label", collapsed ? "Expand navigation" : "Collapse navigation");
    sidebarToggle.setAttribute("title", collapsed ? "Expand navigation" : "Collapse navigation");
  }

  function setTheme(theme) {
    const normalized = normalizeTheme(theme);
    const isDark = normalized === "dark";
    root.dataset.theme = normalized;

    if (themeToggle) {
      themeToggle.setAttribute("aria-label", isDark ? "Use light mode" : "Use dark mode");
      themeToggle.setAttribute("aria-pressed", String(isDark));
      themeToggle.setAttribute("title", isDark ? "Use light mode" : "Use dark mode");
    }

    if (themeToggleLabel) {
      themeToggleLabel.textContent = isDark ? "Dark" : "Light";
    }
  }

  function readSidebarCollapsedPreference() {
    try {
      return window.localStorage.getItem(sidebarStorageKey) === "true";
    } catch {
      return false;
    }
  }

  function writeSidebarCollapsedPreference(collapsed) {
    try {
      window.localStorage.setItem(sidebarStorageKey, String(collapsed));
    } catch {
      // Ignore blocked storage; the visual state still changes for this page view.
    }
  }

  function readThemePreference() {
    try {
      const stored = window.localStorage.getItem(themeStorageKey);
      if (stored === "dark" || stored === "light") {
        return stored;
      }
    } catch {
      // Ignore blocked storage and fall back to the browser preference.
    }

    return window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches
      ? "dark"
      : "light";
  }

  function writeThemePreference(theme) {
    try {
      window.localStorage.setItem(themeStorageKey, normalizeTheme(theme));
    } catch {
      // Ignore blocked storage; the visual state still changes for this page view.
    }
  }

  function normalizeTheme(theme) {
    return theme === "dark" ? "dark" : "light";
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

  function preserveMainScroll(action) {
    const main = root.querySelector(".spider-main");
    const top = main ? main.scrollTop : 0;
    action();

    if (main) {
      main.scrollTop = top;
    }
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
