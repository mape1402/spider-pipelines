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
            html.AppendLine("        <div class=\"spider-logo\" aria-hidden=\"true\"><svg class='spider-logo-svg' viewBox='0 0 32 32' focusable='false'><path class='spider-logo-link' d='M10 9L16 16L22 9M16 16L10 23M16 16L22 23'></path><circle cx='10' cy='9' r='3'></circle><circle cx='22' cy='9' r='3'></circle><circle cx='16' cy='16' r='3'></circle><circle cx='10' cy='23' r='3'></circle><circle cx='22' cy='23' r='3'></circle></svg></div>");
            html.AppendLine("        <div class=\"spider-brand-copy\">");
            html.AppendLine($"          <div class=\"spider-title\">{title}</div>");
            html.AppendLine("          <div class=\"spider-subtitle\">Architecture</div>");
            html.AppendLine("        </div>");
            html.AppendLine("      </div>");
            html.AppendLine("      <nav class=\"spider-menu\" aria-label=\"Architecture sections\">");
            html.AppendLine("        <div class=\"spider-menu-section\">Map</div>");
            html.AppendLine("        <button class=\"spider-menu-item\" type=\"button\" data-menu-view=\"pipelines\"><span class=\"spider-menu-icon pipeline\" aria-hidden=\"true\"><svg class='spider-menu-svg' viewBox='0 0 24 24' focusable='false'><path d='M12 4v16'></path><rect x='7' y='3' width='10' height='5' rx='2'></rect><rect x='7' y='10' width='10' height='5' rx='2'></rect><rect x='7' y='17' width='10' height='4' rx='2'></rect></svg></span><span class=\"spider-menu-text\"><span>Pipelines</span><small>Execution wrappers</small></span><strong id=\"spider-pipeline-count\">0</strong></button>");
            html.AppendLine("        <button class=\"spider-menu-item\" type=\"button\" data-menu-view=\"flows\"><span class=\"spider-menu-icon flow\" aria-hidden=\"true\"><svg class='spider-menu-svg' viewBox='0 0 24 24' focusable='false'><path d='M12 4v5M12 15v5M12 9L6 15M12 9l6 6'></path><path d='M12 8l4 4-4 4-4-4z'></path><circle cx='6' cy='16' r='2'></circle><circle cx='18' cy='16' r='2'></circle></svg></span><span class=\"spider-menu-text\"><span>Flows</span><small>Business processes</small></span><strong id=\"spider-flow-count\">0</strong></button>");
            html.AppendLine("        <button class=\"spider-menu-item\" type=\"button\" data-menu-view=\"boundaries\"><span class=\"spider-menu-icon boundary\" aria-hidden=\"true\"><svg class='spider-menu-svg' viewBox='0 0 24 24' focusable='false'><path d='M8 5H5v14h3'></path><path d='M10 12h9'></path><path d='M15 8l4 4-4 4'></path><path d='M12 6h7v12h-7'></path></svg></span><span class=\"spider-menu-text\"><span>Boundaries</span><small>Entry points</small></span><strong id=\"spider-boundary-count\">0</strong></button>");
            html.AppendLine("        <button class=\"spider-menu-item spider-runtime-menu-item\" type=\"button\" data-menu-view=\"runtime\"><span class=\"spider-menu-icon runtime\" aria-hidden=\"true\"><svg class='spider-menu-svg' viewBox='0 0 24 24' focusable='false'><path d='M3 12h4l2-5 4 10 2-5h6'></path><circle cx='12' cy='12' r='8'></circle></svg></span><span class=\"spider-menu-text\"><span>Runtime</span><small>Live executions</small></span><strong id=\"spider-runtime-count\">0</strong></button>");
            html.AppendLine("      </nav>");
            html.AppendLine("      <div class=\"spider-sidebar-footer\">");
            html.AppendLine("        <button id=\"spider-json-link\" class=\"spider-json-link\" type=\"button\">Manifest JSON</button>");
            html.AppendLine("      </div>");
            html.AppendLine("    </aside>");
            html.AppendLine("    <section class=\"spider-workspace\">");
            html.AppendLine("      <header class=\"spider-topbar\">");
            html.AppendLine("        <div class=\"spider-topbar-left\">");
            html.AppendLine("          <button id=\"spider-sidebar-toggle\" class=\"spider-sidebar-toggle\" type=\"button\" aria-label=\"Collapse navigation\" aria-expanded=\"true\" title=\"Collapse navigation\"><span class=\"spider-sidebar-toggle-line\"></span><span class=\"spider-sidebar-toggle-line\"></span></button>");
            html.AppendLine("          <button id=\"spider-topbar-back\" class=\"spider-topbar-back\" type=\"button\" data-back-list>Back to Runtime</button>");
            html.AppendLine("          <div id=\"spider-topbar-title\" class=\"spider-topbar-title\">Pipelines</div>");
            html.AppendLine("        </div>");
            html.AppendLine("        <div class=\"spider-topbar-actions\">");
            html.AppendLine("          <button id=\"spider-runtime-import\" class=\"spider-topbar-action spider-runtime-file-action spider-runtime-import-action\" type=\"button\" data-import-runtime-trace>Import trace</button>");
            html.AppendLine("          <button id=\"spider-runtime-export\" class=\"spider-topbar-action spider-runtime-file-action spider-runtime-export-action\" type=\"button\" data-export-runtime-trace>Export trace</button>");
            html.AppendLine("          <input id=\"spider-runtime-import-input\" class=\"spider-runtime-file-input\" type=\"file\" accept=\"application/json,.json\" />");
            html.AppendLine("          <button id=\"spider-theme-toggle\" class=\"spider-theme-toggle\" type=\"button\" aria-label=\"Use dark mode\" aria-pressed=\"false\" title=\"Use dark mode\"><span class=\"spider-theme-toggle-icon\" aria-hidden=\"true\"><svg class=\"spider-theme-icon spider-theme-icon-sun\" viewBox=\"0 0 24 24\" focusable=\"false\"><circle cx=\"12\" cy=\"12\" r=\"4\"></circle><path d=\"M12 2v2M12 20v2M4.93 4.93l1.41 1.41M17.66 17.66l1.41 1.41M2 12h2M20 12h2M4.93 19.07l1.41-1.41M17.66 6.34l1.41-1.41\"></path></svg><svg class=\"spider-theme-icon spider-theme-icon-moon\" viewBox=\"0 0 24 24\" focusable=\"false\"><path d=\"M20 15.5A8.5 8.5 0 0 1 8.5 4A7 7 0 1 0 20 15.5Z\"></path></svg></span><span id=\"spider-theme-toggle-label\">Dark</span></button>");
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
  gap: 12px;
  overflow: hidden;
  border-bottom: 1px solid var(--spider-sidebar-border);
  background:
    radial-gradient(circle at 22px 24px, rgba(230, 36, 45, 0.18), transparent 36px),
    linear-gradient(145deg, rgba(29, 95, 191, 0.1), transparent 48%),
    var(--spider-sidebar-brand);
  padding: 13px 14px;
}

.spider-logo {
  position: relative;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  flex: 0 0 auto;
  overflow: hidden;
  border: 1px solid rgba(255, 255, 255, 0.18);
  border-left-color: var(--spider-red);
  border-radius: 10px;
  background: rgba(255, 255, 255, 0.06);
  color: #f8fafc;
  box-shadow: inset 0 0 0 1px rgba(255, 255, 255, 0.04);
}

.spider-logo-svg {
  width: 24px;
  height: 24px;
  overflow: visible;
}

.spider-logo-svg path,
.spider-logo-svg circle {
  fill: none;
  stroke: currentColor;
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 2;
}

.spider-logo-svg circle {
  fill: var(--spider-sidebar-brand);
}

.spider-logo-link {
  opacity: 0.82;
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
  position: relative;
  display: grid;
  grid-template-columns: 30px minmax(0, 1fr) auto;
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
  background:
    linear-gradient(90deg, rgba(255, 255, 255, 0.04), transparent),
    var(--spider-sidebar-hover);
  border-color: rgba(255, 255, 255, 0.08);
  color: #e0e2f0;
}

.spider-menu-item.is-active {
  background:
    linear-gradient(90deg, rgba(230, 36, 45, 0.16), rgba(29, 95, 191, 0.08)),
    var(--spider-sidebar-active);
  border-color: rgba(230, 36, 45, 0.36);
  color: var(--spider-sidebar-active-text);
}

.spider-menu-item.is-active .spider-menu-icon {
  border-color: color-mix(in srgb, var(--menu-icon-accent) 54%, rgba(255, 255, 255, 0.18));
  background: color-mix(in srgb, var(--menu-icon-accent) 24%, transparent);
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--menu-icon-accent) 18%, transparent);
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
  --menu-icon-accent: var(--spider-red);
  position: relative;
  display: inline-grid;
  place-items: center;
  width: 28px;
  height: 28px;
  border: 1px solid color-mix(in srgb, var(--menu-icon-accent) 34%, rgba(255, 255, 255, 0.12));
  border-radius: 999px;
  background: color-mix(in srgb, var(--menu-icon-accent) 13%, transparent);
  color: var(--menu-icon-accent);
}

.spider-menu-svg {
  width: 18px;
  height: 18px;
  fill: none;
  stroke: currentColor;
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 1.8;
}

.spider-menu-icon.pipeline {
  --menu-icon-accent: #60a5fa;
}

.spider-menu-icon.flow {
  --menu-icon-accent: #ff4d57;
}

.spider-menu-icon.boundary {
  --menu-icon-accent: #fda4af;
}

.spider-menu-icon.runtime {
  --menu-icon-accent: #7aa7ff;
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
  background:
    linear-gradient(180deg, color-mix(in srgb, var(--spider-topbar-bg) 92%, #ffffff), var(--spider-topbar-bg));
  box-shadow: 0 1px 0 rgba(15, 23, 42, 0.02);
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

.spider-topbar-back {
  display: none;
  flex: 0 0 auto;
  border: 1px solid var(--spider-line);
  border-radius: 999px;
  background: var(--spider-input-bg);
  color: var(--spider-text);
  cursor: pointer;
  font-size: 0.76rem;
  font-weight: 650;
  line-height: 1;
  padding: 7px 10px;
}

.spider-topbar-back:hover {
  border-color: rgba(230, 36, 45, 0.36);
  box-shadow: 0 0 0 3px rgba(230, 36, 45, 0.08);
  color: var(--spider-red);
}

.spider-shell.is-runtime-trace .spider-topbar-back,
.spider-shell.is-process-detail .spider-topbar-back {
  display: inline-flex;
}

.spider-topbar-actions {
  display: inline-flex;
  align-items: center;
  flex: 0 0 auto;
  gap: 8px;
}

.spider-runtime-file-input {
  display: none;
}

.spider-topbar-action {
  align-items: center;
  min-height: 28px;
  border: 1px solid var(--spider-line);
  border-radius: 999px;
  background: var(--spider-input-bg);
  color: var(--spider-text);
  cursor: pointer;
  font-size: 0.75rem;
  font-weight: 650;
  line-height: 1;
  padding: 6px 10px;
  transition: border-color 0.12s, box-shadow 0.12s, color 0.12s, background 0.12s;
}

.spider-topbar-action:hover {
  border-color: rgba(29, 95, 191, 0.36);
  box-shadow: 0 0 0 3px rgba(29, 95, 191, 0.08);
  color: var(--spider-blue);
}

.spider-runtime-file-action {
  display: none;
}

.spider-shell.is-runtime-list .spider-runtime-import-action,
.spider-shell.is-runtime-trace .spider-runtime-import-action,
.spider-shell.is-runtime-trace .spider-runtime-export-action {
  display: inline-flex;
}

.spider-theme-toggle {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  min-height: 28px;
  border: 1px solid var(--spider-line);
  border-radius: 999px;
  background: var(--spider-input-bg);
  color: var(--spider-text);
  cursor: pointer;
  font-size: 0.75rem;
  font-weight: 650;
  line-height: 1;
  padding: 5px 10px 5px 8px;
  transition: border-color 0.12s, box-shadow 0.12s, color 0.12s, background 0.12s;
}

.spider-theme-toggle:hover {
  border-color: rgba(29, 95, 191, 0.36);
  box-shadow: 0 0 0 3px rgba(29, 95, 191, 0.08);
  color: var(--spider-blue);
}

.spider-theme-toggle-icon {
  display: inline-grid;
  place-items: center;
  width: 16px;
  height: 16px;
  flex: 0 0 auto;
}

.spider-theme-icon {
  width: 15px;
  height: 15px;
  fill: none;
  stroke: currentColor;
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 2;
}

.spider-theme-icon-sun {
  display: none;
  color: #f7c948;
}

.spider-theme-icon-moon {
  display: block;
  color: var(--spider-blue);
}

.spider-shell[data-theme="dark"] .spider-theme-icon-sun {
  display: block;
}

.spider-shell[data-theme="dark"] .spider-theme-icon-moon {
  display: none;
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

.spider-shell.is-runtime-trace .spider-main {
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.spider-content {
  width: 100%;
  max-width: none;
}

.spider-shell.is-runtime-trace .spider-content {
  flex: 1 1 auto;
  min-height: 0;
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

.spider-runtime-toolbar {
  align-items: center;
  margin-bottom: 0;
}

.spider-runtime-toolbar > div {
  display: grid;
  grid-template-columns: auto minmax(0, 1fr);
  align-items: center;
  gap: 4px 10px;
  min-width: 0;
  width: 100%;
}

.spider-runtime-toolbar .spider-detail-actions {
  grid-column: 1;
  grid-row: 1;
  margin-top: 0;
}

.spider-runtime-toolbar .spider-detail-title {
  grid-column: 2;
  grid-row: 1;
  overflow: hidden;
  font-size: 1.08rem;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.spider-runtime-toolbar .spider-detail-subtitle {
  grid-column: 1 / -1;
  grid-row: 2;
  overflow: hidden;
  margin: 0;
  font-size: 0.78rem;
  line-height: 1.25;
  text-overflow: ellipsis;
  white-space: nowrap;
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
  grid-template-columns: repeat(auto-fill, minmax(min(340px, 100%), 1fr));
  grid-auto-rows: 96px;
  justify-content: stretch;
  align-items: stretch;
  width: 100%;
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

.spider-process-row.boundary .spider-process-accent {
  background: var(--spider-blue);
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

.spider-status-not-executed {
  border-color: var(--spider-line);
  background: var(--spider-panel-soft);
  color: var(--spider-muted);
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

.spider-chip.boundary,
.spider-kind-boundary {
  background: var(--spider-blue-soft);
  border: 1px solid rgba(29, 95, 191, 0.18);
  color: var(--spider-blue);
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

.spider-boundary-layout {
  display: grid;
  grid-template-columns: minmax(260px, 340px) minmax(0, 1fr);
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

.spider-detail-view.is-graph-maximized {
  display: grid;
  min-height: 0;
}

.spider-detail-view.is-graph-maximized .spider-detail-toolbar,
.spider-detail-view.is-graph-maximized .spider-process-summary,
.spider-detail-view.is-graph-maximized .spider-node-detail {
  display: none;
}

.spider-detail-view.is-graph-maximized .spider-detail-layout {
  grid-template-columns: minmax(0, 1fr);
  min-height: calc(100vh - 96px);
}

.spider-detail-view.is-graph-maximized .spider-graph-panel {
  height: calc(100vh - 96px);
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

.spider-boundary-layout .spider-process-summary,
.spider-boundary-layout .spider-node-detail {
  height: calc(100vh - 220px);
}

.spider-panel-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 12px;
  margin-bottom: 10px;
}

.spider-panel-title {
  display: flex;
  align-items: center;
  min-width: 0;
}

.spider-panel-title h2 {
  white-space: nowrap;
}

.spider-graph-actions {
  display: inline-flex;
  align-items: center;
  justify-content: flex-end;
  flex: 1 1 360px;
  flex-wrap: wrap;
  gap: 8px;
  min-width: 0;
}

.spider-graph-actions .spider-graph-legend {
  flex: 1 1 260px;
  min-width: 0;
}

.spider-graph-maximize-button {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border: 1px solid var(--spider-line);
  border-radius: 8px;
  background: var(--spider-input-bg);
  color: var(--spider-muted);
  cursor: pointer;
  padding: 0;
  transition: background 0.12s, border-color 0.12s, box-shadow 0.12s, color 0.12s;
}

.spider-graph-maximize-button:hover,
.spider-graph-maximize-button[aria-pressed="true"] {
  background: rgba(29, 95, 191, 0.08);
  border-color: rgba(29, 95, 191, 0.36);
  box-shadow: 0 0 0 3px rgba(29, 95, 191, 0.06);
  color: var(--spider-blue);
}

.spider-graph-maximize-icon {
  display: block;
  width: 18px;
  height: 18px;
  opacity: 0.82;
  transition: opacity 0.12s, transform 0.12s;
}

.spider-graph-maximize-button:hover .spider-graph-maximize-icon,
.spider-graph-maximize-button[aria-pressed="true"] .spider-graph-maximize-icon {
  opacity: 0.96;
}

.spider-graph-maximize-button[aria-pressed="true"] .spider-graph-maximize-icon {
  transform: scale(0.94);
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

.spider-edge.is-runtime-completed {
  stroke: var(--spider-blue);
  stroke-width: 1.8;
}

.spider-edge.is-runtime-faulted {
  stroke: var(--spider-red);
  stroke-width: 2;
}

.spider-edge.is-runtime-not-executed {
  stroke: var(--spider-line-strong);
  stroke-dasharray: 5 5;
  opacity: 0.55;
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
  fill: var(--pipeline-stage-accent, var(--spider-black));
}

.spider-graph-node.is-boundary .spider-node-accent {
  fill: var(--spider-blue);
}

.spider-graph-node.is-boundary .spider-node-box {
  stroke: color-mix(in srgb, var(--spider-blue) 34%, var(--spider-line-strong));
}

.spider-graph-node.is-pipeline-pre {
  --pipeline-stage-accent: var(--spider-red);
}

.spider-graph-node.is-pipeline-middleware {
  --pipeline-stage-accent: var(--spider-black);
}

.spider-graph-node.is-pipeline-target {
  --pipeline-stage-accent: var(--spider-blue);
}

.spider-graph-node.is-pipeline-parallel {
  --pipeline-stage-accent: var(--spider-blue-strong);
}

.spider-graph-node.is-pipeline-success {
  --pipeline-stage-accent: var(--spider-blue);
}

.spider-graph-node.is-pipeline-failure {
  --pipeline-stage-accent: var(--spider-red-strong);
}

.spider-graph-node.is-pipeline-success .spider-node-box,
.spider-graph-node.is-pipeline-target .spider-node-box,
.spider-graph-node.is-pipeline-parallel .spider-node-box {
  stroke: color-mix(in srgb, var(--pipeline-stage-accent) 34%, var(--spider-line-strong));
}

.spider-graph-node.is-pipeline-failure .spider-node-box {
  stroke: color-mix(in srgb, var(--pipeline-stage-accent) 42%, var(--spider-line-strong));
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

.spider-graph-node.is-selected.is-stage .spider-node-box {
  stroke: var(--pipeline-stage-accent, var(--spider-black));
}

.spider-graph-node.is-selected.is-condition .spider-node-box {
  stroke: var(--spider-red-strong);
}

.spider-graph-node.is-selected.is-boundary .spider-node-box {
  stroke: var(--spider-blue);
}

.spider-architecture-graph.is-runtime-graph .spider-graph-node.is-runtime-not-executed {
  opacity: 0.58;
}

.spider-architecture-graph.is-runtime-graph .spider-graph-node.is-runtime-not-executed .spider-node-box {
  fill: var(--spider-panel-soft);
  stroke: var(--spider-line);
}

.spider-architecture-graph.is-runtime-graph .spider-graph-node.is-runtime-not-executed .spider-node-accent {
  fill: var(--spider-muted);
}

.spider-architecture-graph.is-runtime-graph .spider-graph-node.is-runtime-completed .spider-node-box,
.spider-architecture-graph.is-runtime-graph .spider-graph-node.is-runtime-running .spider-node-box {
  fill: var(--spider-blue-soft);
  stroke: rgba(29, 95, 191, 0.42);
}

.spider-architecture-graph.is-runtime-graph .spider-graph-node.is-runtime-completed .spider-node-accent,
.spider-architecture-graph.is-runtime-graph .spider-graph-node.is-runtime-running .spider-node-accent {
  fill: var(--spider-blue);
}

.spider-architecture-graph.is-runtime-graph .spider-graph-node.is-runtime-faulted .spider-node-box,
.spider-architecture-graph.is-runtime-graph .spider-graph-node.is-runtime-cancelled .spider-node-box {
  fill: var(--spider-red-soft);
  stroke: rgba(230, 36, 45, 0.58);
}

.spider-architecture-graph.is-runtime-graph .spider-graph-node.is-runtime-faulted .spider-node-accent,
.spider-architecture-graph.is-runtime-graph .spider-graph-node.is-runtime-cancelled .spider-node-accent {
  fill: var(--spider-red);
}

.spider-architecture-graph.is-runtime-graph .spider-graph-node.is-selected .spider-node-box {
  stroke-width: 2.4;
  filter: drop-shadow(0 8px 14px rgba(29, 95, 191, 0.18));
}

.spider-architecture-graph.is-runtime-graph .spider-graph-node.is-selected.is-runtime-faulted .spider-node-box {
  filter: drop-shadow(0 8px 14px rgba(230, 36, 45, 0.18));
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

.spider-node-role-pill {
  --node-role-accent: var(--pipeline-stage-accent, var(--spider-black));
}

.spider-node-role-pill.is-boundary,
.spider-node-role-pill.is-target,
.spider-node-role-pill.is-parallel,
.spider-node-role-pill.is-success {
  --node-role-accent: var(--spider-blue);
}

.spider-node-role-pill.is-failure,
.spider-node-role-pill.is-pre {
  --node-role-accent: var(--spider-red);
}

.spider-node-role-pill.is-middleware,
.spider-node-role-pill.is-pipeline {
  --node-role-accent: var(--spider-black);
}

.spider-node-stage-pill-bg,
.spider-node-role-pill-bg {
  fill: color-mix(in srgb, var(--node-role-accent, var(--pipeline-stage-accent, var(--spider-black))) 9%, var(--spider-node-bg));
  stroke: color-mix(in srgb, var(--node-role-accent, var(--pipeline-stage-accent, var(--spider-black))) 36%, var(--spider-line));
  stroke-width: 1;
}

.spider-node-stage-pill-text,
.spider-node-role-pill-text {
  fill: var(--node-role-accent, var(--pipeline-stage-accent, var(--spider-black)));
  font-size: 7.2px;
  font-weight: 760;
  letter-spacing: 0;
}

.spider-node-role-pill-mark {
  fill: none;
  stroke: var(--node-role-accent, var(--pipeline-stage-accent, var(--spider-black)));
  stroke-width: 1.35;
  stroke-linecap: round;
  stroke-linejoin: round;
}

.spider-node-role-pill-fill {
  fill: var(--node-role-accent, var(--pipeline-stage-accent, var(--spider-black)));
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

.spider-flowchart {
  --flowchart-node-width: 236px;
  --flowchart-route-gap: 32px;
  display: grid;
  justify-items: center;
  min-width: max-content;
  width: max-content;
  max-width: none;
  padding: 8px 0 24px;
}

.spider-flowchart-sequence,
.spider-flowchart-branch {
  display: grid;
  justify-items: center;
  min-width: max-content;
}

.spider-flowchart-route,
.spider-flowchart-route-body {
  display: grid;
  justify-items: center;
  min-width: max-content;
  width: max-content;
}

.spider-flowchart-connector {
  width: 2px;
  height: 22px;
  background: var(--spider-line-strong);
}

.spider-flowchart-node {
  --node-accent: var(--spider-red);
  --node-border: var(--spider-line);
  --node-fill: var(--spider-panel);
  --node-radius: 8px;
  position: relative;
  display: grid;
  grid-template-columns: minmax(32px, max-content) minmax(0, 1fr) auto;
  gap: 9px;
  align-items: center;
  width: var(--flowchart-node-width);
  min-height: 62px;
  border: 0;
  background: transparent;
  color: var(--spider-text);
  cursor: pointer;
  isolation: isolate;
  overflow: visible;
  padding: 9px 11px 9px 10px;
  text-align: left;
  transition: border-color 0.12s, box-shadow 0.12s, transform 0.12s;
}

.spider-flowchart-node::before {
  content: "";
  position: absolute;
  inset: 0;
  z-index: 0;
  border: 1px solid var(--node-border);
  border-left: 2px solid color-mix(in srgb, var(--node-accent) 56%, var(--node-border));
  border-radius: var(--node-radius);
  background: var(--node-fill);
  box-shadow: 0 8px 18px rgba(17, 24, 39, 0.05);
  pointer-events: none;
  transition: border-color 0.12s, box-shadow 0.12s, background 0.12s;
}

.spider-flowchart-node > * {
  position: relative;
  z-index: 1;
}

.spider-flowchart-node:hover {
  transform: translateY(-1px);
}

.spider-flowchart-node:hover::before {
  border-color: color-mix(in srgb, var(--node-accent) 34%, var(--spider-line));
  box-shadow: 0 12px 22px rgba(17, 24, 39, 0.08);
}

.spider-flowchart-node.is-selected {
  filter: drop-shadow(0 0 0 rgba(0, 0, 0, 0));
}

.spider-flowchart-node.is-selected::before {
  border-color: var(--node-accent);
  box-shadow: 0 0 0 2px color-mix(in srgb, var(--node-accent) 16%, transparent), 0 12px 22px rgba(17, 24, 39, 0.08);
}

.spider-flowchart-node.is-root,
.spider-flowchart-node.is-stage {
  --node-accent: var(--spider-black);
  --node-radius: 999px;
  min-height: 68px;
  padding-inline: 16px;
}

.spider-flowchart-node.is-root::before,
.spider-flowchart-node.is-stage::before {
  border-left-width: 1px;
}

.spider-flowchart-node.is-branch,
.spider-flowchart-node.is-condition {
  --node-accent: var(--spider-blue);
  --node-border: color-mix(in srgb, var(--spider-blue) 55%, var(--spider-line));
  --node-fill: color-mix(in srgb, var(--spider-blue-soft) 26%, var(--spider-panel));
  grid-template-columns: minmax(0, 1fr);
  justify-items: center;
  width: 236px;
  min-height: 124px;
  padding: 26px 40px 22px;
  text-align: center;
}

.spider-flowchart-node.is-branch::before,
.spider-flowchart-node.is-condition::before {
  inset: 5px 22px;
  border: 0;
  border-radius: 0;
  background: color-mix(in srgb, var(--node-accent) 48%, var(--spider-line));
  clip-path: polygon(50% 0, 100% 50%, 50% 100%, 0 50%);
}

.spider-flowchart-node.is-branch::after,
.spider-flowchart-node.is-condition::after {
  content: "";
  position: absolute;
  inset: 7px 24px;
  z-index: 0;
  background: var(--node-fill);
  clip-path: polygon(50% 0, 100% 50%, 50% 100%, 0 50%);
  pointer-events: none;
}

.spider-flowchart-node.is-route {
  --node-accent: var(--spider-blue-strong);
  --node-border: color-mix(in srgb, var(--spider-blue-strong) 48%, var(--spider-line));
  --node-fill: color-mix(in srgb, var(--spider-blue-soft) 32%, var(--spider-panel));
  width: var(--flowchart-node-width);
  min-height: 54px;
  grid-template-columns: minmax(34px, max-content) minmax(0, 1fr);
  padding: 9px 32px 9px 22px;
}

.spider-flowchart-node.is-route::before {
  inset: 0;
  border: 0;
  border-radius: 0;
  background: color-mix(in srgb, var(--node-accent) 55%, var(--spider-line));
  box-shadow: 0 8px 18px rgba(17, 24, 39, 0.05);
  clip-path: polygon(0 0, calc(100% - 24px) 0, 100% 50%, calc(100% - 24px) 100%, 0 100%, 16px 50%);
}

.spider-flowchart-node.is-route::after {
  content: "";
  position: absolute;
  inset: 2px 4px;
  z-index: 0;
  background: var(--node-fill);
  clip-path: polygon(0 0, calc(100% - 20px) 0, 100% 50%, calc(100% - 20px) 100%, 0 100%, 13px 50%);
  pointer-events: none;
}

.spider-flowchart-node.is-transform {
  --node-accent: var(--spider-red-strong);
}

.spider-flowchart-node.is-parallel {
  --node-accent: var(--spider-blue);
  --node-border: var(--spider-line);
  --node-fill: color-mix(in srgb, var(--spider-blue-soft) 12%, var(--spider-panel));
}

.spider-flowchart-node.is-parallel::before {
  border-left-width: 1px;
  border-top: 4px solid color-mix(in srgb, var(--node-accent) 72%, var(--node-border));
  border-bottom: 4px solid color-mix(in srgb, var(--node-accent) 72%, var(--node-border));
}

.spider-flowchart-node.is-foreach,
.spider-flowchart-node.is-batch,
.spider-flowchart-node.is-loop {
  --node-accent: var(--spider-blue-strong);
  --node-border: color-mix(in srgb, var(--spider-blue-strong) 48%, var(--spider-line));
  --node-fill: color-mix(in srgb, var(--spider-blue-soft) 30%, var(--spider-panel));
  min-height: 58px;
  padding-inline: 24px 32px;
}

.spider-flowchart-node.is-foreach::before,
.spider-flowchart-node.is-batch::before,
.spider-flowchart-node.is-loop::before {
  border: 0;
  border-radius: 0;
  background: color-mix(in srgb, var(--node-accent) 48%, var(--spider-line));
  clip-path: polygon(12% 0, 88% 0, 100% 50%, 88% 100%, 12% 100%, 0 50%);
}

.spider-flowchart-node.is-foreach::after,
.spider-flowchart-node.is-batch::after,
.spider-flowchart-node.is-loop::after {
  content: "";
  position: absolute;
  inset: 2px 7px;
  z-index: 0;
  background:
    linear-gradient(
      90deg,
      transparent calc(100% - 9px),
      color-mix(in srgb, var(--node-accent) 44%, transparent) calc(100% - 9px),
      color-mix(in srgb, var(--node-accent) 44%, transparent) calc(100% - 6px),
      transparent calc(100% - 6px)
    ),
    var(--node-fill);
  clip-path: polygon(12% 0, 88% 0, 100% 50%, 88% 100%, 12% 100%, 0 50%);
  pointer-events: none;
}

.spider-flowchart-node.is-merge,
.spider-flowchart-node.is-aggregate {
  --node-accent: var(--spider-blue-strong);
}

.spider-flowchart-node.is-merge::before,
.spider-flowchart-node.is-aggregate::before {
  border-left-width: 1px;
  clip-path: polygon(16px 0, calc(100% - 16px) 0, 100% 50%, calc(100% - 16px) 100%, 16px 100%, 0 50%);
}

.spider-flowchart-node.is-checkpoint {
  --node-accent: var(--spider-blue-strong);
  width: 172px;
  min-height: 112px;
  grid-template-columns: minmax(0, 1fr);
  justify-items: center;
  padding: 18px 22px;
  text-align: center;
}

.spider-flowchart-node.is-checkpoint::before {
  border-left-width: 1px;
  border-radius: 999px;
}

.spider-flowchart-node-shape {
  display: grid;
  place-items: center;
  width: auto;
  min-width: 28px;
  max-width: 58px;
  height: 28px;
  padding-inline: 5px;
  overflow: hidden;
  border: 1px solid var(--spider-line);
  border-radius: 7px;
  background: var(--spider-panel-soft);
  color: var(--spider-muted);
  font-size: 0.66rem;
  font-weight: 660;
  font-variant-numeric: tabular-nums;
  letter-spacing: 0;
  line-height: 1;
  white-space: nowrap;
}

.spider-flowchart-node-shape.is-long {
  font-size: 0.56rem;
  padding-inline: 4px;
}

.spider-flowchart-node-shape.is-compact {
  max-width: 54px;
  font-size: 0.54rem;
  padding-inline: 4px;
}

.spider-flowchart-node.is-branch .spider-flowchart-node-shape,
.spider-flowchart-node.is-condition .spider-flowchart-node-shape {
  position: absolute;
  top: 10px;
  left: 50%;
  width: auto;
  min-width: 28px;
  max-width: 54px;
  height: 18px;
  padding-inline: 5px;
  border-color: color-mix(in srgb, var(--node-accent) 38%, var(--spider-line));
  border-radius: 999px;
  background: var(--spider-panel);
  color: var(--node-accent);
  transform: translateX(-50%);
}

.spider-flowchart-node.is-branch .spider-flowchart-node-index,
.spider-flowchart-node.is-condition .spider-flowchart-node-index {
  transform: none;
}

.spider-flowchart-node-index {
  display: block;
  max-width: 100%;
  overflow: hidden;
  text-overflow: clip;
  white-space: nowrap;
}

.spider-flowchart-node.is-checkpoint .spider-flowchart-node-shape {
  margin-inline: auto;
}

.spider-flowchart-node.is-branch .spider-flowchart-node-main,
.spider-flowchart-node.is-condition .spider-flowchart-node-main,
.spider-flowchart-node.is-checkpoint .spider-flowchart-node-main {
  justify-items: center;
}

.spider-flowchart-node.is-branch .spider-flowchart-node-main,
.spider-flowchart-node.is-condition .spider-flowchart-node-main {
  width: 132px;
}

.spider-flowchart-node-main {
  display: grid;
  gap: 3px;
  min-width: 0;
}

.spider-flowchart-node-kind {
  width: fit-content;
  max-width: 100%;
  overflow: hidden;
  border: 1px solid var(--spider-line);
  border-radius: 999px;
  background: var(--spider-panel-soft);
  color: var(--spider-muted);
  font-size: 0.62rem;
  font-weight: 640;
  line-height: 1;
  padding: 3px 6px;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.spider-flowchart-node-title {
  display: -webkit-box;
  overflow: hidden;
  color: var(--spider-text);
  font-size: 0.72rem;
  font-weight: 590;
  line-height: 1.28;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 2;
}

.spider-flowchart-node.is-branch .spider-flowchart-node-title,
.spider-flowchart-node.is-condition .spider-flowchart-node-title {
  line-height: 1.22;
  -webkit-line-clamp: 3;
}

.spider-flowchart-node.is-branch .spider-flowchart-node-kind,
.spider-flowchart-node.is-condition .spider-flowchart-node-kind,
.spider-flowchart-node.is-route .spider-flowchart-node-kind {
  display: none;
}

.spider-flowchart-node.is-foreach .spider-flowchart-node-kind,
.spider-flowchart-node.is-batch .spider-flowchart-node-kind,
.spider-flowchart-node.is-loop .spider-flowchart-node-kind,
.spider-flowchart-node.is-parallel .spider-flowchart-node-kind {
  border-color: color-mix(in srgb, var(--node-accent) 42%, var(--spider-line));
  background: color-mix(in srgb, var(--node-accent) 12%, var(--spider-panel-soft));
  color: color-mix(in srgb, var(--node-accent) 78%, var(--spider-text));
}

.spider-flowchart-node-subtitle {
  display: -webkit-box;
  overflow: hidden;
  color: var(--spider-muted);
  font-size: 0.72rem;
  line-height: 1.28;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 2;
}

.spider-flowchart-link {
  display: inline-flex;
  align-items: center;
  align-self: start;
  gap: 4px;
  border: 1px solid rgba(230, 36, 45, 0.3);
  border-radius: 999px;
  background: color-mix(in srgb, var(--spider-red-soft) 62%, var(--spider-panel));
  color: var(--spider-red);
  font-size: 0.66rem;
  font-weight: 780;
  line-height: 1;
  padding: 4px 7px;
}

.spider-flowchart-split {
  position: relative;
  width: 100%;
  height: 24px;
}

.spider-flowchart-split::before {
  content: "";
  position: absolute;
  top: 0;
  left: 50%;
  width: 2px;
  height: 14px;
  background: var(--spider-line-strong);
  transform: translateX(-50%);
}

.spider-flowchart-split::after {
  content: "";
  position: absolute;
  right: calc(var(--flowchart-node-width) / 2);
  bottom: 8px;
  left: calc(var(--flowchart-node-width) / 2);
  border-top: 2px solid var(--spider-line-strong);
}

.spider-flowchart-routes {
  display: grid;
  grid-template-columns: repeat(var(--route-count), max-content);
  gap: 16px var(--flowchart-route-gap);
  align-items: start;
  justify-content: center;
  justify-items: center;
  min-width: max-content;
}

.spider-flowchart-route {
  position: relative;
  gap: 0;
}

.spider-flowchart-route::before {
  content: "";
  width: 2px;
  height: 18px;
  background: var(--spider-line-strong);
}

.spider-flowchart-route-body {
  padding: 0;
}

.spider-flowchart-join {
  position: relative;
  width: 100%;
  height: 34px;
}

.spider-flowchart-join::before {
  content: "";
  position: absolute;
  right: calc(var(--flowchart-node-width) / 2);
  top: 10px;
  left: calc(var(--flowchart-node-width) / 2);
  border-top: 2px solid var(--spider-line-strong);
}

.spider-flowchart-join::after {
  content: "";
  position: absolute;
  top: 10px;
  left: 50%;
  width: 2px;
  height: 24px;
  background: var(--spider-line-strong);
  transform: translateX(-50%);
}

.spider-flowchart-node.is-runtime-not-executed {
  opacity: 0.58;
}

.spider-flowchart-node.is-runtime-not-executed {
  --node-accent: var(--spider-muted);
  --node-fill: var(--spider-panel-soft);
}

.spider-flowchart-node.is-runtime-completed,
.spider-flowchart-node.is-runtime-running {
  --node-accent: var(--spider-blue);
  --node-border: rgba(29, 95, 191, 0.42);
  --node-fill: var(--spider-blue-soft);
}

.spider-flowchart-node.is-runtime-faulted,
.spider-flowchart-node.is-runtime-cancelled {
  --node-accent: var(--spider-red);
  --node-border: rgba(230, 36, 45, 0.58);
  --node-fill: var(--spider-red-soft);
}

.spider-graph-legend {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  justify-content: flex-end;
  margin-left: auto;
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
  border-radius: 2px;
  transform: rotate(45deg);
}

.spider-legend-swatch.route {
  background: var(--spider-blue-strong);
  width: 12px;
  border-radius: 0;
  clip-path: polygon(0 0, calc(100% - 3px) 0, 100% 50%, calc(100% - 3px) 100%, 0 100%, 3px 50%);
}

.spider-legend-swatch.stage {
  background: var(--spider-black);
}

.spider-legend-swatch.parallel {
  width: 12px;
  height: 9px;
  border-top: 3px solid var(--spider-blue);
  border-bottom: 3px solid var(--spider-blue);
  border-radius: 1px;
  background: transparent;
}

.spider-legend-swatch.foreach {
  width: 12px;
  border-radius: 0;
  background: var(--spider-blue-strong);
  clip-path: polygon(18% 0, 82% 0, 100% 50%, 82% 100%, 18% 100%, 0 50%);
}

.spider-legend-swatch.subflow {
  width: 12px;
  border-radius: 2px;
  background: var(--spider-red-soft);
  box-shadow: inset 3px 0 0 var(--spider-red), inset -3px 0 0 var(--spider-red);
}

.spider-flow-legend-icon {
  --legend-accent: var(--spider-red);
  --legend-fill: var(--spider-panel);
  position: relative;
  display: inline-block;
  flex: 0 0 auto;
  width: 26px;
  height: 15px;
  border: 1px solid color-mix(in srgb, var(--legend-accent) 34%, var(--spider-line));
  border-left: 2px solid color-mix(in srgb, var(--legend-accent) 62%, var(--spider-line));
  border-radius: 5px;
  background: color-mix(in srgb, var(--legend-accent) 6%, var(--legend-fill));
}

.spider-flow-legend-icon.start {
  --legend-accent: var(--spider-black);
  border-left-width: 1px;
  border-radius: 999px;
}

.spider-flow-legend-icon.process {
  --legend-accent: var(--spider-red);
}

.spider-flow-legend-icon.decision {
  --legend-accent: var(--spider-blue);
  width: 17px;
  height: 17px;
  border-left-width: 1px;
  border-radius: 1px;
  background: color-mix(in srgb, var(--legend-accent) 12%, var(--legend-fill));
  transform: rotate(45deg);
}

.spider-flow-legend-icon.route {
  --legend-accent: var(--spider-blue-strong);
  width: 30px;
  height: 16px;
  border-left-width: 1px;
  border-radius: 0;
  background: color-mix(in srgb, var(--legend-accent) 52%, var(--spider-line));
  clip-path: polygon(0 0, calc(100% - 8px) 0, 100% 50%, calc(100% - 8px) 100%, 0 100%, 6px 50%);
}

.spider-flow-legend-icon.route::after {
  content: "";
  position: absolute;
  inset: 2px 3px;
  background: color-mix(in srgb, var(--legend-accent) 22%, var(--legend-fill));
  clip-path: polygon(0 0, calc(100% - 6px) 0, 100% 50%, calc(100% - 6px) 100%, 0 100%, 4px 50%);
}

.spider-flow-legend-icon.parallel {
  --legend-accent: var(--spider-blue);
  border-left-width: 1px;
  border-top: 4px solid color-mix(in srgb, var(--legend-accent) 70%, var(--spider-line));
  border-bottom: 4px solid color-mix(in srgb, var(--legend-accent) 70%, var(--spider-line));
  background: color-mix(in srgb, var(--legend-accent) 8%, var(--legend-fill));
}

.spider-flow-legend-icon.foreach {
  --legend-accent: var(--spider-blue-strong);
  width: 30px;
  height: 16px;
  border-left-width: 1px;
  border: 0;
  border-radius: 0;
  background: color-mix(in srgb, var(--legend-accent) 48%, var(--spider-line));
  clip-path: polygon(12% 0, 88% 0, 100% 50%, 88% 100%, 12% 100%, 0 50%);
}

.spider-flow-legend-icon.foreach::after {
  content: "";
  position: absolute;
  inset: 2px 4px;
  background:
    linear-gradient(
      90deg,
      transparent calc(100% - 7px),
      color-mix(in srgb, var(--legend-accent) 44%, transparent) calc(100% - 7px),
      color-mix(in srgb, var(--legend-accent) 44%, transparent) calc(100% - 5px),
      transparent calc(100% - 5px)
    ),
    color-mix(in srgb, var(--legend-accent) 24%, var(--legend-fill));
  clip-path: polygon(12% 0, 88% 0, 100% 50%, 88% 100%, 12% 100%, 0 50%);
}

.spider-flow-legend-icon.subflow {
  --legend-accent: var(--spider-red);
  border-left-color: color-mix(in srgb, var(--legend-accent) 48%, var(--spider-line));
  background: color-mix(in srgb, var(--legend-accent) 5%, var(--legend-fill));
}

.spider-flow-legend-icon.subflow::after {
  content: "";
  position: absolute;
  top: 3px;
  right: 3px;
  width: 8px;
  height: 6px;
  border: 1px solid color-mix(in srgb, var(--legend-accent) 45%, var(--spider-line));
  border-radius: 999px;
  background: color-mix(in srgb, var(--legend-accent) 12%, var(--legend-fill));
}

.spider-pipeline-legend-icon {
  --pipeline-legend-accent: var(--spider-black);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex: 0 0 auto;
  width: 34px;
  height: 18px;
}

.spider-pipeline-legend-icon.pre {
  --pipeline-legend-accent: var(--spider-red);
}

.spider-pipeline-legend-icon.middleware {
  --pipeline-legend-accent: var(--spider-black);
}

.spider-pipeline-legend-icon.target {
  --pipeline-legend-accent: var(--spider-blue);
}

.spider-pipeline-legend-icon.parallel {
  --pipeline-legend-accent: var(--spider-blue-strong);
}

.spider-pipeline-legend-icon.success {
  --pipeline-legend-accent: var(--spider-blue);
}

.spider-pipeline-legend-icon.failure {
  --pipeline-legend-accent: var(--spider-red-strong);
}

.spider-pipeline-legend-icon.boundary {
  --pipeline-legend-accent: var(--spider-blue);
}

.spider-pipeline-legend-svg {
  display: block;
  width: 34px;
  height: 18px;
  overflow: visible;
}

.spider-pipeline-legend-frame {
  fill: color-mix(in srgb, var(--pipeline-legend-accent) 6%, var(--spider-panel));
  stroke: color-mix(in srgb, var(--pipeline-legend-accent) 45%, var(--spider-line));
  stroke-width: 1.2;
}

.spider-pipeline-legend-accent {
  fill: var(--pipeline-legend-accent);
}

.spider-pipeline-legend-mark {
  fill: none;
  stroke: var(--pipeline-legend-accent);
  stroke-width: 1.55;
  stroke-linecap: round;
  stroke-linejoin: round;
}

.spider-pipeline-legend-fill {
  fill: var(--pipeline-legend-accent);
}

.spider-legend-swatch.runtime-completed {
  background: var(--spider-blue);
}

.spider-legend-swatch.runtime-faulted {
  background: var(--spider-red);
}

.spider-legend-swatch.runtime-muted {
  background: var(--spider-muted);
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

.spider-stage-summary {
  border: 1px solid var(--spider-line);
  border-radius: 8px;
  background: var(--spider-panel-soft);
  margin-bottom: 12px;
  padding: 10px;
}

.spider-stage-summary-title {
  color: var(--spider-text);
  font-size: 0.82rem;
  font-weight: 700;
  margin: 0 0 8px;
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
  gap: 10px;
  margin-top: -6px;
}

.spider-shell.is-runtime-trace .spider-runtime-detail {
  grid-template-rows: auto auto minmax(0, 1fr);
  height: 100%;
  min-height: 0;
}

.spider-runtime-workspace {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(340px, 420px);
  gap: 16px;
  align-items: start;
  min-width: 0;
}

.spider-shell.is-runtime-trace .spider-runtime-workspace {
  align-items: stretch;
  min-height: 0;
  overflow: hidden;
}

.spider-runtime-visual-panel,
.spider-runtime-context {
  min-width: 0;
}

.spider-runtime-visual-panel {
  display: grid;
  gap: 14px;
}

.spider-shell.is-runtime-trace .spider-runtime-visual-panel {
  grid-template-rows: auto minmax(0, 1fr);
  min-height: 0;
  overflow: hidden;
}

.spider-runtime-visual-header {
  align-items: center;
  gap: 14px;
}

.spider-runtime-visual-actions {
  display: inline-flex;
  align-items: center;
  flex: 0 0 auto;
  gap: 8px;
}

.spider-runtime-view-switch {
  display: inline-flex;
  flex: 0 0 auto;
  gap: 3px;
  border: 1px solid var(--spider-line);
  border-radius: 999px;
  background: var(--spider-soft);
  padding: 3px;
}

.spider-runtime-view-button {
  border: 0;
  border-radius: 999px;
  background: transparent;
  color: var(--spider-muted);
  cursor: pointer;
  font: inherit;
  font-size: 0.76rem;
  font-weight: 650;
  line-height: 1;
  padding: 8px 11px;
}

.spider-runtime-view-button:hover {
  color: var(--spider-text);
}

.spider-runtime-view-button.is-active {
  background: var(--spider-blue);
  box-shadow: 0 8px 18px rgba(37, 99, 235, 0.22);
  color: #ffffff;
}

.spider-runtime-context {
  display: grid;
  align-content: start;
  gap: 12px;
  position: sticky;
  top: 12px;
  max-height: calc(100vh - 108px);
  overflow: auto;
  padding-right: 2px;
}

.spider-shell.is-runtime-trace .spider-runtime-context {
  grid-template-rows: minmax(0, 1fr);
  align-content: stretch;
  height: 100%;
  min-height: 0;
  max-height: none;
  overflow: hidden;
  position: static;
}

.spider-runtime-flow-body {
  display: grid;
  gap: 12px;
  min-width: 0;
}

.spider-shell.is-runtime-trace .spider-runtime-flow-body {
  grid-template-rows: auto minmax(0, 1fr);
  min-height: 0;
}

.spider-runtime-flow-meta {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  min-width: 0;
}

.spider-runtime-flow-name {
  overflow: hidden;
  color: var(--spider-muted);
  font-size: 0.78rem;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.spider-runtime-graph-wrap {
  min-height: 560px;
  max-height: calc(100vh - 330px);
  overflow: auto;
  padding: 14px;
}

.spider-shell.is-runtime-trace .spider-runtime-graph-wrap {
  height: 100%;
  min-height: 0;
  max-height: none;
}

.spider-runtime-graph-wrap .spider-architecture-graph {
  width: min(100%, var(--spider-graph-width, 520px));
  max-width: 100%;
}

.spider-runtime-node-detail {
  align-self: start;
  padding: 12px;
}

.spider-shell.is-runtime-trace .spider-runtime-node-detail {
  align-self: stretch;
  min-height: 0;
  overflow: auto;
}

.spider-runtime-overview {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(112px, 1fr));
  gap: 8px;
}

.spider-runtime-metric {
  display: grid;
  gap: 4px;
  min-width: 0;
  border: 1px solid var(--spider-line);
  border-radius: 8px;
  background: var(--spider-panel);
  padding: 7px 9px;
}

.spider-runtime-metric span {
  color: var(--spider-muted);
  font-size: 0.64rem;
  font-weight: 800;
  letter-spacing: 0;
  text-transform: uppercase;
}

.spider-runtime-metric strong {
  overflow: hidden;
  color: var(--spider-text);
  font-size: 0.8rem;
  font-weight: 700;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.spider-runtime-node-detail .spider-node-header {
  gap: 6px;
  margin-bottom: 10px;
}

.spider-runtime-node-detail .spider-node-description {
  margin-top: 4px;
  line-height: 1.34;
}

.spider-runtime-node-detail .spider-node-tags {
  margin-top: 5px;
}

.spider-runtime-node-detail .spider-definition {
  grid-template-columns: 90px minmax(0, 1fr);
  gap: 5px 9px;
}

.spider-runtime-node-detail .spider-detail-section {
  margin-top: 10px;
  padding-top: 10px;
}

.spider-runtime-timeline {
  display: grid;
  gap: 10px;
}

.spider-runtime-timeline.is-main {
  position: relative;
  gap: 0;
}

.spider-shell.is-runtime-trace .spider-runtime-timeline.is-main {
  min-height: 0;
  overflow: auto;
}

.spider-runtime-timeline-track {
  position: relative;
  display: grid;
  gap: 0;
  min-width: 0;
  padding: 12px 0 18px;
}

.spider-runtime-timeline-track::before {
  content: "";
  position: absolute;
  top: 18px;
  bottom: 18px;
  left: 50%;
  width: 3px;
  border-radius: 999px;
  background: var(--spider-line-strong);
  transform: translateX(-50%);
}

.spider-runtime-timeline-item {
  position: relative;
  display: grid;
  grid-template-columns: minmax(0, 1fr) 62px minmax(0, 1fr);
  align-items: center;
  min-height: 112px;
}

.spider-runtime-timeline-side {
  min-width: 0;
}

.spider-runtime-timeline-left {
  display: flex;
  justify-content: flex-end;
  padding-right: 18px;
}

.spider-runtime-timeline-right {
  display: flex;
  justify-content: flex-start;
  padding-left: 18px;
}

.spider-runtime-timeline-center {
  position: relative;
  display: grid;
  place-items: center;
  min-height: 118px;
}

.spider-runtime-timeline-center::before,
.spider-runtime-timeline-center::after {
  content: "";
  position: absolute;
  top: 50%;
  width: 30px;
  border-top: 2px dashed var(--spider-line-strong);
}

.spider-runtime-timeline-center::before {
  right: 50%;
  transform: translateX(-31px);
}

.spider-runtime-timeline-center::after {
  left: 50%;
  transform: translateX(31px);
}

.spider-runtime-timeline-item.is-left .spider-runtime-timeline-center::after,
.spider-runtime-timeline-item.is-right .spider-runtime-timeline-center::before {
  display: none;
}

.spider-runtime-timeline-number {
  position: relative;
  z-index: 1;
  display: grid;
  place-items: center;
  width: 46px;
  height: 46px;
  border: 3px solid var(--spider-blue);
  border-radius: 999px;
  background: var(--spider-panel);
  color: var(--spider-blue);
  font-size: 0.92rem;
  font-weight: 850;
  box-shadow: 0 0 0 5px var(--spider-bg), 0 8px 18px rgba(17, 24, 39, 0.08);
}

.spider-runtime-timeline-item.is-faulted .spider-runtime-timeline-number {
  border-color: var(--spider-red);
  color: var(--spider-red);
}

.spider-runtime-timeline-item.is-running .spider-runtime-timeline-number {
  animation: spiderPulse 1.4s ease-in-out infinite;
}

.spider-runtime-timeline-card {
  display: grid;
  gap: 6px;
  width: min(100%, 340px);
  border: 1px solid rgba(29, 95, 191, 0.24);
  border-radius: 8px;
  background: var(--spider-panel);
  cursor: pointer;
  padding: 12px 13px;
  text-align: left;
  box-shadow: 0 8px 18px rgba(17, 24, 39, 0.05);
  transition: border-color 0.12s, box-shadow 0.12s, transform 0.12s;
}

.spider-runtime-timeline-card:hover {
  border-color: rgba(29, 95, 191, 0.45);
  box-shadow: 0 12px 24px rgba(17, 24, 39, 0.08);
  transform: translateY(-1px);
}

.spider-runtime-timeline-card.is-selected {
  border-color: var(--spider-blue);
  box-shadow: 0 0 0 3px rgba(29, 95, 191, 0.14), 0 12px 24px rgba(17, 24, 39, 0.08);
}

.spider-runtime-timeline-card.is-faulted {
  border-color: rgba(230, 36, 45, 0.48);
  background: var(--spider-red-soft);
}

.spider-runtime-timeline-card.is-faulted.is-selected {
  border-color: var(--spider-red);
  box-shadow: 0 0 0 3px rgba(230, 36, 45, 0.14), 0 12px 24px rgba(17, 24, 39, 0.08);
}

.spider-runtime-timeline-card.is-running {
  background: var(--spider-blue-soft);
}

.spider-runtime-timeline-card strong {
  overflow: hidden;
  color: var(--spider-text);
  font-size: 0.88rem;
  font-weight: 700;
  line-height: 1.25;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.spider-runtime-timeline-kind {
  width: fit-content;
  border: 1px solid var(--spider-line);
  border-radius: 999px;
  background: var(--spider-panel-soft);
  color: var(--spider-muted);
  font-size: 0.68rem;
  font-weight: 780;
  line-height: 1;
  padding: 4px 7px;
}

.spider-runtime-timeline-description,
.spider-runtime-timeline-fault {
  margin: 0;
  color: var(--spider-muted);
  font-size: 0.76rem;
  line-height: 1.35;
}

.spider-runtime-timeline-fault {
  color: var(--spider-red);
  font-weight: 700;
}

.spider-runtime-timeline-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 7px;
  color: var(--spider-muted);
  font-size: 0.72rem;
}

.spider-runtime-span {
  position: relative;
  display: grid;
  gap: 7px;
  padding-left: calc(var(--runtime-depth, 0) * 28px);
}

.spider-runtime-span-card,
.spider-runtime-event {
  display: grid;
  grid-template-columns: 86px 22px minmax(0, 1fr) auto;
  align-items: center;
  gap: 11px;
  border: 0;
  border-radius: 8px;
  background: transparent;
  padding: 0;
}

.spider-runtime-span-card {
  cursor: pointer;
  transition: transform 0.12s;
}

.spider-runtime-span-card:hover {
  transform: translateY(-1px);
}

.spider-runtime-span-card.is-faulted,
.spider-runtime-event.is-faulted {
  background: transparent;
}

.spider-runtime-span-card.is-running,
.spider-runtime-event.is-running {
  background: transparent;
}

.spider-runtime-span-card.is-completed {
  border-color: transparent;
}

.spider-runtime-timeblock {
  display: grid;
  gap: 2px;
  color: var(--spider-muted);
  font-size: 0.7rem;
  line-height: 1.1;
  text-align: right;
}

.spider-runtime-timeblock strong {
  color: var(--spider-text);
  font-size: 0.76rem;
  font-weight: 750;
}

.spider-runtime-timeblock small {
  color: var(--spider-muted);
  font-size: 0.68rem;
  font-weight: 650;
}

.spider-runtime-rail {
  position: relative;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-height: 54px;
}

.spider-runtime-rail::before {
  content: "";
  position: absolute;
  top: -9px;
  bottom: -9px;
  left: 50%;
  width: 2px;
  background: var(--spider-line-strong);
  transform: translateX(-50%);
}

.spider-runtime-span-card .spider-runtime-dot {
  position: relative;
  z-index: 1;
}

.spider-runtime-span-card .spider-runtime-span-main {
  border: 1px solid var(--spider-line);
  border-radius: 8px;
  background: var(--spider-panel);
  box-shadow: 0 6px 15px rgba(17, 24, 39, 0.04);
  padding: 10px 12px;
}

.spider-runtime-span-card.is-completed .spider-runtime-span-main {
  border-color: rgba(29, 95, 191, 0.25);
}

.spider-runtime-span-card.is-running .spider-runtime-span-main {
  border-color: rgba(29, 95, 191, 0.38);
  background: var(--spider-blue-soft);
}

.spider-runtime-span-card.is-faulted .spider-runtime-span-main {
  border-color: rgba(230, 36, 45, 0.48);
  background: var(--spider-red-soft);
}

.spider-runtime-span-card.is-selected .spider-runtime-span-main {
  border-color: var(--spider-blue);
  box-shadow: 0 0 0 3px rgba(29, 95, 191, 0.14), 0 8px 18px rgba(17, 24, 39, 0.06);
}

.spider-runtime-span-card.is-selected.is-faulted .spider-runtime-span-main {
  border-color: var(--spider-red);
  box-shadow: 0 0 0 3px rgba(230, 36, 45, 0.15), 0 8px 18px rgba(17, 24, 39, 0.06);
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
  align-self: center;
  justify-content: flex-start;
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

.spider-runtime-raw-button {
  display: inline-flex;
  align-items: center;
  gap: 7px;
  border: 1px solid var(--spider-line);
  border-radius: 999px;
  background: var(--spider-input-bg);
  color: var(--spider-text);
  cursor: pointer;
  font-size: 0.76rem;
  font-weight: 680;
  line-height: 1;
  padding: 7px 10px;
  transition: border-color 0.12s, box-shadow 0.12s, color 0.12s, transform 0.12s;
}

.spider-runtime-raw-button:hover {
  border-color: rgba(29, 95, 191, 0.42);
  box-shadow: 0 8px 16px rgba(29, 95, 191, 0.12);
  color: var(--spider-blue);
  transform: translateY(-1px);
}

.spider-runtime-raw-button-count {
  border: 1px solid rgba(29, 95, 191, 0.18);
  border-radius: 999px;
  background: var(--spider-blue-soft);
  color: var(--spider-blue);
  font-size: 0.72rem;
  font-weight: 760;
  padding: 3px 7px;
}

.spider-runtime-maximize-button {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border: 1px solid var(--spider-line);
  border-radius: 8px;
  background: var(--spider-input-bg);
  color: var(--spider-muted);
  cursor: pointer;
  padding: 0;
  transition: background 0.12s, border-color 0.12s, box-shadow 0.12s, color 0.12s;
}

.spider-runtime-maximize-button:hover,
.spider-runtime-maximize-button[aria-pressed="true"] {
  background: rgba(29, 95, 191, 0.08);
  border-color: rgba(29, 95, 191, 0.36);
  box-shadow: 0 0 0 3px rgba(29, 95, 191, 0.06);
  color: var(--spider-blue);
}

.spider-runtime-maximize-icon {
  width: 18px;
  height: 18px;
  display: block;
  opacity: 0.78;
  transition: opacity 0.12s, transform 0.12s;
}

.spider-runtime-maximize-button:hover .spider-runtime-maximize-icon,
.spider-runtime-maximize-button[aria-pressed="true"] .spider-runtime-maximize-icon {
  opacity: 0.94;
}

.spider-runtime-maximize-button[aria-pressed="true"] .spider-runtime-maximize-icon {
  transform: scale(0.94);
}

.spider-shell.is-runtime-trace .spider-runtime-detail.is-visual-maximized {
  grid-template-rows: minmax(0, 1fr);
  margin-top: 0;
}

.spider-runtime-detail.is-visual-maximized .spider-runtime-toolbar,
.spider-runtime-detail.is-visual-maximized .spider-runtime-overview,
.spider-runtime-detail.is-visual-maximized .spider-runtime-context {
  display: none;
}

.spider-runtime-detail.is-visual-maximized .spider-runtime-workspace {
  grid-template-columns: minmax(0, 1fr);
  gap: 0;
  height: 100%;
}

.spider-runtime-detail.is-visual-maximized .spider-runtime-visual-panel {
  height: 100%;
}

.spider-runtime-raw-list {
  display: grid;
  grid-template-columns: minmax(0, 1fr);
  gap: 10px;
  padding: 0;
}

.spider-runtime-raw-list.is-inline {
  border-top: 0;
  padding: 0;
}

.spider-runtime-raw-list.is-empty {
  display: grid;
  min-height: 100%;
  place-items: center;
}

.spider-runtime-raw-list .spider-runtime-event {
  display: grid;
  grid-template-columns: minmax(0, 1fr);
  align-items: start;
  gap: 9px;
  border: 1px solid var(--spider-line);
  border-left: 4px solid var(--spider-blue);
  border-radius: 8px;
  background: var(--spider-panel-soft);
  box-shadow: 0 8px 18px rgba(17, 24, 39, 0.04);
  min-width: 0;
  padding: 12px 13px;
}

.spider-runtime-raw-list .spider-runtime-event.is-faulted,
.spider-runtime-raw-list .spider-runtime-event.is-cancelled {
  border-left-color: var(--spider-red);
  background: var(--spider-red-soft);
}

.spider-runtime-raw-list .spider-runtime-event.is-running,
.spider-runtime-raw-list .spider-runtime-event.is-started {
  border-left-color: var(--spider-blue);
  background: var(--spider-blue-soft);
}

.spider-modal-backdrop {
  position: fixed;
  inset: 0;
  z-index: 60;
  display: grid;
  place-items: center;
  background: rgba(5, 10, 20, 0.58);
  padding: 24px;
}

.spider-modal-panel {
  display: grid;
  grid-template-rows: auto minmax(0, 1fr);
  width: min(960px, calc(100vw - 48px));
  height: min(780px, calc(100vh - 48px));
  max-height: none;
  overflow: hidden;
  border: 1px solid var(--spider-line);
  border-radius: 10px;
  background: var(--spider-panel);
  box-shadow: 0 24px 80px rgba(5, 10, 20, 0.35);
}

.spider-modal-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 18px;
  border-bottom: 1px solid var(--spider-line);
  padding: 16px 18px;
}

.spider-modal-eyebrow {
  color: var(--spider-muted);
  font-size: 0.72rem;
  font-weight: 760;
  text-transform: uppercase;
}

.spider-modal-title {
  margin: 3px 0 0;
  color: var(--spider-text);
  font-size: 1rem;
  line-height: 1.25;
}

.spider-modal-subtitle {
  margin-top: 4px;
  color: var(--spider-muted);
  font-size: 0.78rem;
  line-height: 1.35;
}

.spider-modal-search {
  display: block;
  margin-top: 12px;
  width: min(420px, 58vw);
}

.spider-modal-search input {
  width: 100%;
  border: 1px solid var(--spider-line);
  border-radius: 8px;
  background: var(--spider-input-bg);
  color: var(--spider-text);
  font: inherit;
  font-size: 0.82rem;
  line-height: 1;
  outline: none;
  padding: 9px 11px;
}

.spider-modal-search input:focus {
  border-color: rgba(29, 95, 191, 0.46);
  box-shadow: 0 0 0 3px rgba(29, 95, 191, 0.08);
}

.spider-modal-close {
  border: 1px solid var(--spider-line);
  border-radius: 999px;
  background: var(--spider-panel-soft);
  color: var(--spider-text);
  cursor: pointer;
  font-size: 0.78rem;
  font-weight: 680;
  padding: 7px 10px;
}

.spider-modal-body {
  min-height: 0;
  overflow: auto;
  padding: 14px 16px 16px;
}

.spider-runtime-event-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  min-width: 0;
}

.spider-runtime-event-kind {
  overflow: hidden;
  color: var(--spider-muted);
  font-size: 0.72rem;
  font-weight: 760;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.spider-runtime-event-body {
  display: grid;
  gap: 6px;
  min-width: 0;
}

.spider-runtime-event-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 7px;
  color: var(--spider-muted);
  font-size: 0.74rem;
  line-height: 1.35;
}

.spider-runtime-event-meta span {
  min-width: 0;
  overflow-wrap: anywhere;
}

.spider-runtime-event-fault {
  color: var(--spider-red);
  font-size: 0.76rem;
  font-weight: 700;
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

  .spider-boundary-layout {
    grid-template-columns: minmax(0, 1fr);
    min-height: 0;
  }

  .spider-runtime-workspace {
    grid-template-columns: minmax(0, 1fr);
  }

  .spider-runtime-context {
    position: static;
    max-height: none;
    overflow: visible;
  }

  .spider-runtime-graph-wrap {
    max-height: 420px;
  }

  .spider-detail-layout.is-inspector-collapsed {
    grid-template-columns: minmax(240px, 300px) minmax(320px, 1fr);
  }

  .spider-process-summary,
  .spider-graph-panel,
  .spider-node-detail,
  .spider-boundary-layout .spider-process-summary,
  .spider-boundary-layout .spider-node-detail {
    height: auto;
  }

  .spider-graph-wrap {
    max-height: calc(100vh - 220px);
  }

  .spider-flowchart {
    --flowchart-node-width: 236px;
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

  .spider-runtime-toolbar > div {
    grid-template-columns: auto minmax(0, 1fr);
  }

  .spider-runtime-toolbar .spider-detail-actions {
    grid-column: 1;
    grid-row: 1;
  }

  .spider-runtime-toolbar .spider-detail-title {
    grid-column: 2;
    grid-row: 1;
  }

  .spider-runtime-toolbar .spider-detail-subtitle {
    grid-column: 1 / -1;
    grid-row: 2;
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

  .spider-runtime-span-card,
  .spider-runtime-event {
    grid-template-columns: 16px minmax(0, 1fr);
  }

  .spider-runtime-span-actions {
    grid-column: 2;
    justify-content: flex-start;
  }

  .spider-runtime-timeline-track {
    padding-left: 0;
  }

  .spider-runtime-timeline-track::before {
    left: 24px;
  }

  .spider-runtime-timeline-item {
    grid-template-columns: 48px minmax(0, 1fr);
    min-height: 0;
    padding: 8px 0;
  }

  .spider-runtime-timeline-left,
  .spider-runtime-timeline-right {
    grid-column: 2;
    justify-content: flex-start;
    padding-left: 12px;
    padding-right: 0;
  }

  .spider-runtime-timeline-center {
    grid-column: 1;
    grid-row: 1;
    min-height: 86px;
  }

  .spider-runtime-timeline-center::before,
  .spider-runtime-timeline-center::after {
    display: none;
  }

  .spider-runtime-timeline-card {
    width: 100%;
  }

  .spider-runtime-visual-header,
  .spider-runtime-flow-meta {
    align-items: flex-start;
    flex-direction: column;
  }

  .spider-runtime-visual-actions {
    align-items: stretch;
    flex-direction: column;
    width: 100%;
  }

  .spider-runtime-view-switch {
    width: 100%;
  }

  .spider-runtime-view-button {
    flex: 1 1 0;
  }

  .spider-runtime-raw-list {
    grid-template-columns: 1fr;
  }

  .spider-modal-backdrop {
    padding: 12px;
  }

  .spider-modal-panel {
    width: calc(100vw - 24px);
    height: calc(100vh - 24px);
  }

  .spider-modal-header {
    display: grid;
    gap: 12px;
  }

  .spider-modal-search {
    width: 100%;
  }

  .spider-flowchart {
    --flowchart-node-width: min(248px, calc(100vw - 78px));
    min-width: 0;
    width: max-content;
  }

  .spider-flowchart-routes {
    grid-template-columns: minmax(220px, var(--flowchart-node-width));
  }

  .spider-flowchart-split::after,
  .spider-flowchart-join::before {
    display: none;
  }

  .spider-flowchart-route + .spider-flowchart-route {
    margin-top: 12px;
  }

  .spider-runtime-span {
    padding-left: calc(var(--runtime-depth, 0) * 10px);
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
  const importedRuntimeTraces = new Map();
  runtimeData = mergeImportedRuntimeData(runtimeData);
  const components = manifest.components || [];
  const relations = manifest.relations || [];
  const byId = new Map(components.map((component) => [component.id, component]));
  const flows = components.filter((component) => component.kind === "spider.flow").sort(compareByName);
  const pipelines = components.filter((component) => component.kind === "spider.pipeline").sort(compareByName);
  const boundaries = components.filter((component) => component.kind === "spider.boundary").sort(compareByName);
  const flowCount = document.getElementById("spider-flow-count");
  const pipelineCount = document.getElementById("spider-pipeline-count");
  const boundaryCount = document.getElementById("spider-boundary-count");
  const runtimeCount = document.getElementById("spider-runtime-count");
  const topbarTitle = document.getElementById("spider-topbar-title");
  const topbarBack = document.getElementById("spider-topbar-back");
  const sidebarToggle = document.getElementById("spider-sidebar-toggle");
  const themeToggle = document.getElementById("spider-theme-toggle");
  const themeToggleLabel = document.getElementById("spider-theme-toggle-label");
  const runtimeImportInput = document.getElementById("spider-runtime-import-input");
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
    runtimeSelectedComponentId: "",
    runtimeSelectedItemKey: "",
    runtimeVisualView: "story",
    runtimeVisualMaximized: false,
    processGraphMaximized: false,
    runtimeListSignature: "",
    inspectorCollapsed: false,
    rawEventsModalOpen: false,
    rawEventsQuery: ""
  };

  flowCount.textContent = String(flows.length);
  pipelineCount.textContent = String(pipelines.length);
  if (boundaryCount) {
    boundaryCount.textContent = String(boundaries.length);
  }
  updateRuntimeCount();

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

    const importTrace = event.target.closest("[data-import-runtime-trace]");
    if (importTrace && showRuntime && runtimeImportInput) {
      runtimeImportInput.value = "";
      runtimeImportInput.click();
      return;
    }

    const exportTrace = event.target.closest("[data-export-runtime-trace]");
    if (exportTrace && state.view === "runtime" && state.mode === "trace") {
      exportCurrentRuntimeTrace();
      return;
    }

    const rawEvents = event.target.closest("[data-open-raw-events]");
    if (rawEvents && state.view === "runtime" && state.mode === "trace") {
      openRuntimeRawEventsModal();
      return;
    }

    const runtimeVisual = event.target.closest("[data-runtime-visual-view]");
    if (runtimeVisual && state.view === "runtime" && state.mode === "trace") {
      setRuntimeVisualView(runtimeVisual.getAttribute("data-runtime-visual-view"));
      return;
    }

    const runtimeVisualMaximize = event.target.closest("[data-runtime-visual-maximize]");
    if (runtimeVisualMaximize && state.view === "runtime" && state.mode === "trace") {
      setRuntimeVisualMaximized(!state.runtimeVisualMaximized);
      return;
    }

    const processGraphMaximize = event.target.closest("[data-process-graph-maximize]");
    if (processGraphMaximize && state.mode === "detail") {
      setProcessGraphMaximized(!state.processGraphMaximized);
      return;
    }

    const closeRawEvents = event.target.closest("[data-close-raw-events]");
    if (closeRawEvents) {
      closeRuntimeRawEventsModal();
      return;
    }

    if (event.target.classList && event.target.classList.contains("spider-modal-backdrop")) {
      closeRuntimeRawEventsModal();
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

    const runtimeItem = event.target.closest("[data-runtime-item-key]");
    if (runtimeItem && state.view === "runtime" && state.mode === "trace") {
      selectRuntimeItem(runtimeItem.getAttribute("data-runtime-item-key"));
      return;
    }

    const node = event.target.closest("[data-node-id]");
    if (node) {
      selectNode(node.getAttribute("data-node-id"));
    }
  });

  document.addEventListener("keydown", (event) => {
    if (event.key === "Escape" && state.rawEventsModalOpen) {
      closeRuntimeRawEventsModal();
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

  if (runtimeImportInput) {
    runtimeImportInput.addEventListener("change", async () => {
      const file = runtimeImportInput.files && runtimeImportInput.files[0];
      runtimeImportInput.value = "";
      if (!file) {
        return;
      }

      await importRuntimeTraceFile(file);
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

    if (hash === "flows" || hash === "pipelines" || hash === "boundaries" || hash === "runtime") {
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
    closeRuntimeRawEventsModal();
    setRuntimeTraceShell(false);
    setRuntimeListShell(false);
    setProcessDetailShell(false);
    state.view = view === "runtime" && showRuntime
      ? "runtime"
      : view === "boundaries" ? "boundaries" : view === "flows" ? "flows" : "pipelines";
    setRuntimeListShell(state.view === "runtime");
    state.mode = "list";
    state.processId = "";
    state.nodeId = "";
    state.query = "";
    state.runtimeSelectedComponentId = "";
    state.runtimeSelectedItemKey = "";
    state.runtimeVisualMaximized = false;
    state.processGraphMaximized = false;
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

    const items = view === "boundaries" ? boundaries : view === "flows" ? flows : pipelines;
    const title = view === "boundaries" ? "Boundaries" : view === "flows" ? "Flows" : "Pipelines";
    const description = view === "boundaries"
      ? "Entry points, contracts, policies, and runtime crossing points documented at compile time."
      : view === "flows"
        ? "Method-level business flows documented from ComposeFlow calls."
        : "Execution pipelines attached around Spider service invocations.";
    const listId = view === "boundaries" ? "spider-boundary-list" : view === "flows" ? "spider-flow-list" : "spider-pipeline-list";

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
    setRuntimeListShell(true);
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
      const importedChip = item.imported ? `<span class="spider-count-pill">Imported</span>` : "";
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
            ${importedChip}
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
      showList("runtime");
      return;
    }

    const signature = createRuntimeTraceSignature(trace);
    if (skipHash && state.runtimeTraceId === traceId && state.runtimeTraceSignature === signature) {
      return;
    }

    if (state.runtimeTraceId !== traceId) {
      closeRuntimeRawEventsModal();
    }

    state.view = "runtime";
    state.mode = "trace";
    if (state.runtimeTraceId !== traceId) {
      state.runtimeSelectedComponentId = "";
      state.runtimeSelectedItemKey = "";
      state.runtimeVisualMaximized = false;
    }

    state.runtimeTraceId = traceId;
    state.runtimeTraceSignature = signature;
    if (!skipHash) {
      setHash("runtime:" + traceId);
    }

    if (skipHash) {
      preserveRuntimeVisualScroll(() => renderTraceDetail(trace));
    } else {
      renderTraceDetail(trace);
      resetMainScroll();
    }
  }

  function renderTraceDetail(trace) {
    const overview = createRuntimeOverview(trace, findSummary(trace.traceId));
    const graphContext = createRuntimeGraphContext(overview);
    const visualView = getRuntimeVisualView();
    overview.graphContext = graphContext;
    setRuntimeTraceShell(true);
    setRuntimeListShell(false);
    setProcessDetailShell(false);
    setActiveMenu("runtime");
    setTopbarTitle(trace.traceId);
    setTopbarBackLabel("Back to Runtime");

    const maximizedClass = state.runtimeVisualMaximized ? " is-visual-maximized" : "";
    const maximizeText = state.runtimeVisualMaximized ? "Restore" : "Maximize";
    const maximizeIconPath = state.runtimeVisualMaximized
      ? "M4 4l6 6M10 10H4M10 10V4M20 4l-6 6M14 10h6M14 10V4M4 20l6-6M10 14H4M10 14v6M20 20l-6-6M14 14h6M14 14v6"
      : "M10 10L4 4M4 4h6M4 4v6M14 10l6-6M20 4h-6M20 4v6M10 14l-6 6M4 20h6M4 20v-6M14 14l6 6M20 20h-6M20 20v-6";

    content.innerHTML = `
      <article class="spider-runtime-detail${maximizedClass}">
        <header class="spider-detail-toolbar spider-runtime-toolbar">
          <div>
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
        <div class="spider-runtime-workspace">
          <section class="spider-panel spider-runtime-visual-panel ${visualView === "story" ? "spider-runtime-timeline-panel" : "spider-runtime-graph-panel"}">
            <div class="spider-panel-header spider-runtime-visual-header">
              <div>
                <h2>${escapeHtml(getRuntimeVisualTitle(visualView, graphContext))}</h2>
                <span class="spider-panel-note">${escapeHtml(getRuntimeVisualNote(visualView, graphContext))}</span>
              </div>
              <div class="spider-runtime-visual-actions">
                <button class="spider-runtime-raw-button" type="button" data-open-raw-events>
                  <span>Raw events</span>
                  <span class="spider-runtime-raw-button-count">${escapeHtml(String((trace.events || []).length))}</span>
                </button>
                ${renderRuntimeVisualSwitch(visualView)}
                <button class="spider-runtime-maximize-button" type="button" data-runtime-visual-maximize aria-label="${escapeAttribute(maximizeText)} view" title="${escapeAttribute(maximizeText)} view" aria-pressed="${state.runtimeVisualMaximized ? "true" : "false"}">
                  <svg class="spider-runtime-maximize-icon" viewBox="0 0 24 24" aria-hidden="true" focusable="false">
                    <path d="${escapeAttribute(maximizeIconPath)}" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" />
                  </svg>
                </button>
              </div>
            </div>
            ${visualView === "story"
              ? `<div class="spider-runtime-timeline is-main">
                  ${overview.rootSpans.length ? renderRuntimeTimeline(overview, graphContext) : renderRuntimeRawEventList(trace, true)}
                </div>`
              : renderRuntimeGraphPanel(graphContext)}
          </section>
          <aside class="spider-runtime-context">
            <section id="spider-runtime-node-detail" class="spider-panel spider-runtime-node-detail">
              ${renderRuntimeNodeDetail(graphContext)}
            </section>
          </aside>
        </div>
      </article>`;

    bindGraphTooltip();
  }

  function renderRuntimeMetric(label, value) {
    return `
      <div class="spider-runtime-metric">
        <span>${escapeHtml(label)}</span>
        <strong title="${escapeAttribute(value)}">${escapeHtml(value)}</strong>
      </div>`;
  }

  function getRuntimeVisualView() {
    return state.runtimeVisualView === "flow" ? "flow" : "story";
  }

  function getRuntimeVisualTitle(visualView, graphContext) {
    if (visualView === "story") {
      return "Execution story line";
    }

    if (!graphContext || !graphContext.process) {
      return "Execution flow";
    }

    return graphContext.process.kind === "spider.flow" ? "Execution flowchart" : "Execution graph";
  }

  function getRuntimeVisualNote(visualView, graphContext) {
    if (visualView === "story") {
      return "Runtime order, nested spans, selected branch, and faults";
    }

    if (!graphContext || !graphContext.process) {
      return "No graph metadata matched this trace";
    }

    return graphContext.process.displayName || graphContext.process.id;
  }

  function renderRuntimeVisualSwitch(activeView) {
    const storyClass = activeView === "story" ? " is-active" : "";
    const flowClass = activeView === "flow" ? " is-active" : "";

    return `
      <div class="spider-runtime-view-switch" role="tablist" aria-label="Runtime view">
        <button class="spider-runtime-view-button${storyClass}" type="button" role="tab" aria-selected="${activeView === "story" ? "true" : "false"}" data-runtime-visual-view="story">Story line</button>
        <button class="spider-runtime-view-button${flowClass}" type="button" role="tab" aria-selected="${activeView === "flow" ? "true" : "false"}" data-runtime-visual-view="flow">Flow</button>
      </div>`;
  }

  function renderRuntimeGraphPanel(graphContext) {
    if (!graphContext || !showGraph) {
      return `
        <div class="spider-runtime-flow-body">
          <div class="spider-empty-list">No graph metadata matched this trace.</div>
        </div>`;
    }

    const graph = renderProcessGraph(graphContext.process, graphContext.children, graphContext);
    return `
      <div class="spider-runtime-flow-body">
        <div class="spider-runtime-flow-meta">
          <span class="spider-runtime-flow-name">${escapeHtml(graphContext.process.displayName || graphContext.process.id)}</span>
          <div class="spider-graph-legend" aria-label="Runtime graph legend">
            <span class="spider-legend-item"><i class="spider-legend-swatch runtime-completed"></i>Executed</span>
            <span class="spider-legend-item"><i class="spider-legend-swatch runtime-faulted"></i>Faulted</span>
            <span class="spider-legend-item"><i class="spider-legend-swatch runtime-muted"></i>Not run</span>
          </div>
        </div>
        <div class="spider-graph-wrap spider-process-graph spider-runtime-graph-wrap">
          ${graph}
          <div id="spider-graph-tooltip" class="spider-graph-tooltip" role="tooltip"></div>
        </div>
      </div>`;
  }

  function renderRuntimeNodeDetail(graphContext) {
    if (!graphContext) {
      return `
        <div class="spider-panel-header">
          <h2>Runtime context</h2>
        </div>
        <div class="spider-empty-list">Select a timeline item after graph metadata is available.</div>`;
    }

    if (graphContext.selectedRuntimeItem) {
      return renderRuntimeItemDetail(graphContext.selectedRuntimeItem.value, graphContext);
    }

    const componentId = state.runtimeSelectedComponentId || graphContext.defaultComponentId;
    const component = byId.get(componentId) || graphContext.process;
    const span = graphContext.spanByComponentId.get(component.id) || null;
    const nodeState = graphContext.nodeStates.get(component.id) || "not-executed";
    const description = getMetadata(component, "description") || (span ? getRuntimeDescription(span) : "");
    const tagChips = renderTagChips(component) || (span ? renderRuntimeTagsAsChips(getRuntimeTags(span)) : "");
    const evidence = component.evidence || [];
    const declaredIn = evidence.length ? formatDeclaredIn(evidence[0]) : "";
    const source = evidence.length && evidence[0].filePath
      ? `${evidence[0].filePath}${evidence[0].lineNumber ? ":" + evidence[0].lineNumber : ""}`
      : "";
    const runtimeRows = span ? [
      ["Runtime status", span.status],
      ["Duration", formatDuration(span.durationMs)],
      ["Operation", span.operation],
      ["Input", shortName(span.inputType || "")],
      ["Output", shortName(span.outputType || "")],
      ["Span id", shortTraceId(span.spanId)]
    ] : [["Runtime status", "Not executed in this trace"]];
    const fault = span && span.exception ? `
      <div class="spider-detail-section">
        <dl class="spider-definition">
          <dt>Exception</dt><dd>${escapeHtml(span.exception.message || "Faulted")}</dd>
        </dl>
      </div>` : "";
    const evidenceRows = declaredIn || source ? `
      <div class="spider-detail-section spider-evidence">
        <dl class="spider-definition">
          ${declaredIn ? `<dt>Declared in</dt><dd>${escapeHtml(declaredIn)}</dd>` : ""}
          ${source ? `<dt>Source file</dt><dd>${escapeHtml(source)}</dd>` : ""}
        </dl>
      </div>` : "";

    return `
      <div class="spider-node-header">
        <span class="spider-node-kind ${getKindClass(component)}">${escapeHtml(getFriendlyKind(component))}</span>
        <h2>${escapeHtml(component.displayName || component.id)}</h2>
        <div class="spider-detail-actions">
          <span class="spider-status-chip spider-status-${escapeAttribute(getStatusClass(nodeState))}">${escapeHtml(formatRuntimeGraphState(nodeState))}</span>
        </div>
        ${description ? `<p class="spider-node-description">${escapeHtml(description)}</p>` : ""}
        ${tagChips ? `<div class="spider-node-tags">${tagChips}</div>` : ""}
      </div>
      <dl class="spider-definition">
        ${runtimeRows.map(([label, value]) => `<dt>${escapeHtml(label)}</dt><dd>${escapeHtml(value || "Not declared")}</dd>`).join("")}
        <dt>Component id</dt><dd>${escapeHtml(component.id)}</dd>
      </dl>
      ${fault}
      ${evidenceRows}`;
  }

  function renderRuntimeItemDetail(item, graphContext) {
    const componentId = item.resolvedComponentId || item.componentId || "";
    const component = componentId ? byId.get(componentId) : null;
    const displayName = getRuntimeDisplayName(item);
    const description = component && getMetadata(component, "description")
      ? getMetadata(component, "description")
      : getRuntimeDescription(item);
    const tagChips = component && renderTagChips(component)
      ? renderTagChips(component)
      : renderRuntimeTagsAsChips(getRuntimeTags(item));
    const nodeState = component
      ? graphContext.nodeStates.get(component.id) || normalizeStatus(item.status)
      : normalizeStatus(item.status);
    const runtimeRows = [
      ["Runtime status", item.status || "Started"],
      ["Duration", formatDuration(item.durationMs)],
      ["Operation", item.operation || "Not declared"],
      ["Event kind", item.kind || item.type || "Span"],
      ["Input", shortName(item.inputType || "")],
      ["Output", shortName(item.outputType || "")],
      ["Span id", shortTraceId(item.spanId)],
      ["Component id", componentId || "Runtime only"]
    ];
    const fault = item.exception ? `
      <div class="spider-detail-section">
        <dl class="spider-definition">
          <dt>Exception</dt><dd>${escapeHtml(item.exception.message || "Faulted")}</dd>
        </dl>
      </div>` : "";
    const evidence = component && component.evidence ? component.evidence : [];
    const declaredIn = evidence.length ? formatDeclaredIn(evidence[0]) : "";
    const source = evidence.length && evidence[0].filePath
      ? `${evidence[0].filePath}${evidence[0].lineNumber ? ":" + evidence[0].lineNumber : ""}`
      : "";
    const evidenceRows = declaredIn || source ? `
      <div class="spider-detail-section spider-evidence">
        <dl class="spider-definition">
          ${declaredIn ? `<dt>Declared in</dt><dd>${escapeHtml(declaredIn)}</dd>` : ""}
          ${source ? `<dt>Source file</dt><dd>${escapeHtml(source)}</dd>` : ""}
        </dl>
      </div>` : "";
    const manifestRows = component ? `
      <div class="spider-detail-section">
        <dl class="spider-definition">
          <dt>Manifest type</dt><dd>${escapeHtml(getFriendlyKind(component))}</dd>
          <dt>Manifest node</dt><dd>${escapeHtml(component.displayName || component.id)}</dd>
        </dl>
      </div>` : "";

    return `
      <div class="spider-node-header">
        <span class="spider-node-kind ${component ? getKindClass(component) : "spider-kind-step"}">${escapeHtml(getRuntimeKindLabel(item.componentKind))}</span>
        <h2>${escapeHtml(displayName)}</h2>
        <div class="spider-detail-actions">
          <span class="spider-status-chip spider-status-${escapeAttribute(getStatusClass(nodeState))}">${escapeHtml(formatRuntimeGraphState(nodeState))}</span>
        </div>
        ${description ? `<p class="spider-node-description">${escapeHtml(description)}</p>` : ""}
        ${tagChips ? `<div class="spider-node-tags">${tagChips}</div>` : ""}
      </div>
      <dl class="spider-definition">
        ${runtimeRows.map(([label, value]) => `<dt>${escapeHtml(label)}</dt><dd>${escapeHtml(value || "Not declared")}</dd>`).join("")}
      </dl>
      ${fault}
      ${manifestRows}
      ${evidenceRows}`;
  }

  function renderRuntimeTagsAsChips(tags) {
    return (tags || [])
      .map((tag) => `<span class="spider-chip tag">${escapeHtml(tag)}</span>`)
      .join("");
  }

  function renderRuntimeTimeline(overview, graphContext) {
    const items = createRuntimeTimelineItems(overview.rootSpans);
    return `<div class="spider-runtime-timeline-track">${items.map((item, index) => renderRuntimeTimelineItem(item, index, graphContext)).join("")}</div>`;
  }

  function createRuntimeTimelineItems(rootSpans) {
    const items = [];
    const visit = (span, depth) => {
      items.push({ type: "span", value: span, depth, timestamp: span.startedAt });
      (span.markers || []).forEach((marker) => {
        items.push({ type: "marker", value: marker, depth: depth + 1, timestamp: marker.timestamp });
      });
      (span.children || []).forEach((child) => visit(child, depth + 1));
    };

    (rootSpans || []).forEach((span) => visit(span, 0));
    return items.sort((left, right) => compareRuntimeValues(left.timestamp, right.timestamp));
  }

  function createRuntimeItemKey(item) {
    const value = item && item.value ? item.value : {};
    if (item && item.type === "span") {
      return `span:${value.spanId || value.componentId || value.displayName || value.startedAt || ""}`;
    }

    return [
      "event",
      value.spanId || "",
      value.kind || "",
      value.timestamp || "",
      value.operation || "",
      value.displayName || ""
    ].join(":");
  }

  function findRuntimeTimelineItem(rootSpans, key) {
    if (!key) {
      return null;
    }

    return createRuntimeTimelineItems(rootSpans).find((item) => createRuntimeItemKey(item) === key) || null;
  }

  function renderRuntimeTimelineItem(item, index, graphContext) {
    const value = item.value || {};
    const itemKey = createRuntimeItemKey(item);
    const statusClass = getStatusClass(value.status);
    const displayName = getRuntimeDisplayName(value);
    const description = getRuntimeDescription(value);
    const tags = getRuntimeTags(value);
    const tooltip = createRuntimeTooltip(value, displayName, description, tags);
    const componentId = value.resolvedComponentId || value.componentId || "";
    const componentAttribute = componentId
      ? ` data-runtime-component-id="${escapeAttribute(componentId)}"`
      : "";
    const itemAttribute = ` data-runtime-item-key="${escapeAttribute(itemKey)}"`;
    const selectedClass = itemKey && itemKey === state.runtimeSelectedItemKey ? " is-selected" : "";
    const sideClass = index % 2 === 0 ? " is-left" : " is-right";
    const depthLabel = item.depth ? `<span>Depth ${escapeHtml(String(item.depth))}</span>` : "";
    const descriptionHtml = description
      ? `<p class="spider-runtime-timeline-description">${escapeHtml(description)}</p>`
      : "";
    const tagHtml = tags.length
      ? `<div class="spider-runtime-tags">${tags.map((tag) => `<span class="spider-runtime-tag">${escapeHtml(tag)}</span>`).join("")}</div>`
      : "";
    const exceptionHtml = value.exception
      ? `<p class="spider-runtime-timeline-fault">${escapeHtml(value.exception.message || "Faulted")}</p>`
      : "";
    const duration = value.durationMs !== undefined && value.durationMs !== null
      ? formatDuration(value.durationMs)
      : item.type === "marker" ? "event" : "running";
    const technicalName = getRuntimeTechnicalName(value, displayName);
    const technicalHtml = technicalName
      ? `<span>${escapeHtml(technicalName)}</span>`
      : "";
    const content = `
      <button class="spider-runtime-timeline-card is-${escapeAttribute(statusClass)}${selectedClass}" type="button"${itemAttribute}${componentAttribute}>
        <span class="spider-runtime-timeline-kind">${escapeHtml(getRuntimeKindLabel(value.componentKind))}</span>
        <strong title="${escapeAttribute(tooltip)}">${escapeHtml(displayName)}</strong>
        ${descriptionHtml}
        ${tagHtml}
        ${exceptionHtml}
        <span class="spider-runtime-timeline-meta">
          <span>${escapeHtml(value.operation || item.type)}</span>
          ${technicalHtml}
          ${depthLabel}
          <span>${escapeHtml(formatTime(item.timestamp))}</span>
          <span>${escapeHtml(duration)}</span>
        </span>
      </button>`;

    return `
      <div class="spider-runtime-timeline-item${sideClass} is-${escapeAttribute(statusClass)}" style="--timeline-index: '${escapeAttribute(String(index + 1).padStart(2, "0"))}'">
        <div class="spider-runtime-timeline-side spider-runtime-timeline-left">${sideClass === " is-left" ? content : ""}</div>
        <div class="spider-runtime-timeline-center">
          <span class="spider-runtime-timeline-number">${escapeHtml(String(index + 1).padStart(2, "0"))}</span>
        </div>
        <div class="spider-runtime-timeline-side spider-runtime-timeline-right">${sideClass === " is-right" ? content : ""}</div>
      </div>`;
  }

  function renderRuntimeSpan(span, depth, graphContext) {
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
      ? span.children.map((child) => renderRuntimeSpan(child, depth + 1, graphContext)).join("")
      : "";
    const componentId = span.resolvedComponentId || span.componentId || "";
    const selectedClass = componentId && componentId === state.runtimeSelectedComponentId ? " is-selected" : "";
    const componentAttribute = componentId
      ? ` data-node-id="${escapeAttribute(componentId)}"`
      : "";
    const link = span.componentId && byId.has(span.componentId)
      ? `<button class="spider-related-button" type="button" data-open-process="${escapeAttribute(span.componentId)}"><span class="spider-related-label">Open component</span><span class="spider-related-name">${escapeHtml(byId.get(span.componentId).displayName || span.componentId)}</span></button>`
      : "";

    return `
      <div class="spider-runtime-span" style="--runtime-depth: ${escapeAttribute(String(depth))}">
        <div class="spider-runtime-span-card is-${escapeAttribute(statusClass)}${selectedClass}"${componentAttribute}>
          <span class="spider-runtime-timeblock">
            <strong>${escapeHtml(formatTime(span.startedAt))}</strong>
            <small>${escapeHtml(formatDuration(span.durationMs))}</small>
          </span>
          <span class="spider-runtime-rail" aria-hidden="true">
            <span class="spider-runtime-dot"></span>
          </span>
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
              <span>${escapeHtml(span.completedAt ? "completed " + formatTime(span.completedAt) : "still running")}</span>
              ${span.exception ? `<span>${escapeHtml(span.exception.message)}</span>` : ""}
            </span>
            ${link}
          </span>
          <span class="spider-runtime-span-actions">
            <span class="spider-count-pill">${escapeHtml(span.children.length ? String(span.children.length) + " nested" : "leaf")}</span>
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

  function renderRuntimeRawEventList(trace, inline, query) {
    const allEvents = trace.events || [];
    const events = filterRuntimeRawEvents(allEvents, query);
    if (allEvents.length > 0 && events.length === 0) {
      return `<div class="spider-runtime-raw-list${inline ? " is-inline" : ""} is-empty"><div class="spider-empty-list">No raw events match this search.</div></div>`;
    }

    if (events.length === 0) {
      return `<div class="spider-runtime-raw-list${inline ? " is-inline" : ""} is-empty"><div class="spider-empty-list">No raw events captured for this trace.</div></div>`;
    }

    return `<div class="spider-runtime-raw-list${inline ? " is-inline" : ""}">${events.map(renderRuntimeEvent).join("")}</div>`;
  }

  function filterRuntimeRawEvents(events, query) {
    const normalizedQuery = String(query || "").trim().toLowerCase();
    if (!normalizedQuery) {
      return events || [];
    }

    return (events || []).filter((event) => createRuntimeEventSearchText(event).includes(normalizedQuery));
  }

  function createRuntimeEventSearchText(event) {
    const component = event.componentId && byId.has(event.componentId) ? byId.get(event.componentId) : null;
    const values = [
      event.kind,
      event.status,
      event.operation,
      event.displayName,
      event.componentId,
      event.componentKind,
      event.spanId,
      event.parentSpanId,
      event.traceId,
      event.inputType,
      event.outputType,
      getRuntimeDisplayName(event),
      getRuntimeDescription(event),
      component && component.displayName,
      component && component.id,
      event.exception && event.exception.message,
      ...getRuntimeTags(event),
      ...Object.values(event.tags || {}),
      ...Object.values(event.metadata || {})
    ];

    return values.map((value) => String(value || "").toLowerCase()).join(" ");
  }

  function renderRuntimeEvent(event) {
    const statusClass = getStatusClass(event.status);
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
    const component = event.componentId && byId.has(event.componentId) ? byId.get(event.componentId) : null;
    const componentHtml = component
      ? `<span>${escapeHtml(component.displayName || component.id)}</span>`
      : event.componentId ? `<span>${escapeHtml(event.componentId)}</span>` : "";
    const faultHtml = event.exception
      ? `<span class="spider-runtime-event-fault">${escapeHtml(event.exception.message || "Faulted")}</span>`
      : "";

    return `
      <div class="spider-runtime-event is-${escapeAttribute(statusClass)}">
        <div class="spider-runtime-event-top">
          <span class="spider-runtime-event-kind">${escapeHtml(event.kind || "Runtime event")}</span>
          ${renderStatusChip(event.status || "Started")}
        </div>
        <div class="spider-runtime-event-body">
          <span class="spider-runtime-name" title="${escapeAttribute(tooltip)}">${escapeHtml(displayName)}</span>
          ${descriptionHtml}
          ${tagHtml}
          ${faultHtml}
          <span class="spider-runtime-event-meta">
            <span>${escapeHtml(formatTime(event.timestamp))}</span>
            <span>${escapeHtml(event.operation || "operation")}</span>
            <span>${escapeHtml(shortTraceId(event.spanId))}</span>
            ${componentHtml}
          </span>
        </div>
      </div>`;
  }

  function openRuntimeRawEventsModal() {
    const trace = getCurrentRuntimeTrace();
    if (!trace) {
      return;
    }

    closeRuntimeRawEventsModal();
    state.rawEventsQuery = "";
    root.insertAdjacentHTML("beforeend", renderRuntimeRawEventsModal(trace));
    state.rawEventsModalOpen = true;
    root.classList.add("is-modal-open");

    const search = document.getElementById("spider-runtime-raw-search");
    if (search) {
      search.addEventListener("input", () => {
        state.rawEventsQuery = search.value.trim().toLowerCase();
        refreshRuntimeRawEventsModal(getCurrentRuntimeTrace());
      });
      search.focus();
      return;
    }

    const close = document.getElementById("spider-runtime-raw-close");
    if (close) {
      close.focus();
    }
  }

  function closeRuntimeRawEventsModal() {
    const modal = document.getElementById("spider-runtime-raw-modal");
    if (modal) {
      modal.remove();
    }

    state.rawEventsModalOpen = false;
    state.rawEventsQuery = "";
    root.classList.remove("is-modal-open");
  }

  function refreshRuntimeRawEventsModal(trace) {
    const modal = document.getElementById("spider-runtime-raw-modal");
    if (!modal || !state.rawEventsModalOpen) {
      return;
    }

    if (!trace) {
      closeRuntimeRawEventsModal();
      return;
    }

    const count = modal.querySelector("[data-raw-event-count]");
    if (count) {
      count.textContent = formatRuntimeRawEventCount(trace, state.rawEventsQuery);
    }

    const list = modal.querySelector("[data-raw-event-list]");
    if (list) {
      list.innerHTML = renderRuntimeRawEventList(trace, false, state.rawEventsQuery);
    }
  }

  function renderRuntimeRawEventsModal(trace) {
    const events = trace.events || [];
    const query = state.rawEventsQuery || "";
    const summary = findSummary(trace.traceId);
    const title = summary && summary.rootDisplayName
      ? summary.rootDisplayName
      : getTraceTitle(trace);

    return `
      <div id="spider-runtime-raw-modal" class="spider-modal-backdrop">
        <section class="spider-modal-panel" role="dialog" aria-modal="true" aria-labelledby="spider-runtime-raw-title">
          <header class="spider-modal-header">
            <div>
              <div class="spider-modal-eyebrow">Raw events</div>
              <h2 id="spider-runtime-raw-title" class="spider-modal-title">${escapeHtml(title)}</h2>
              <div class="spider-modal-subtitle">
                <span data-raw-event-count>${escapeHtml(formatRuntimeRawEventCount(trace, query))}</span>
                <span> · Trace ${escapeHtml(shortTraceId(trace.traceId))}</span>
              </div>
              <label class="spider-modal-search" aria-label="Search raw events">
                <input id="spider-runtime-raw-search" type="search" autocomplete="off" placeholder="Search raw events" value="${escapeAttribute(query)}" />
              </label>
            </div>
            <button id="spider-runtime-raw-close" class="spider-modal-close" type="button" data-close-raw-events>Close</button>
          </header>
          <div class="spider-modal-body" data-raw-event-list>
            ${renderRuntimeRawEventList(trace, false, query)}
          </div>
        </section>
      </div>`;
  }

  function formatRuntimeRawEventCount(trace, query) {
    const events = trace && trace.events ? trace.events : [];
    const filtered = filterRuntimeRawEvents(events, query);
    if (!String(query || "").trim()) {
      return `${events.length} events`;
    }

    return `${filtered.length} of ${events.length} events`;
  }

  function exportCurrentRuntimeTrace() {
    const trace = getCurrentRuntimeTrace();
    if (!trace) {
      return;
    }

    const summary = findSummary(trace.traceId) || createRuntimeSummaryFromTrace(trace);
    const payload = {
      schema: "spider-runtime-trace",
      version: 1,
      exportedAt: new Date().toISOString(),
      summary,
      trace
    };
    const text = JSON.stringify(payload, null, 2);
    const blob = new Blob([text], { type: "application/json" });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.download = `${sanitizeFilename("spider-trace-" + (trace.traceId || "runtime"))}.json`;
    document.body.appendChild(link);
    link.click();
    link.remove();
    URL.revokeObjectURL(url);
  }

  async function importRuntimeTraceFile(file) {
    try {
      const text = await file.text();
      const payload = JSON.parse(text);
      const imported = extractRuntimeTraceImports(payload);
      if (!imported.length) {
        throw new Error("No trace found in file.");
      }

      imported.forEach((item) => {
        importedRuntimeTraces.set(item.trace.traceId, item);
      });
      runtimeData = mergeImportedRuntimeData(runtimeData);
      updateRuntimeCount();
      state.query = "";
      openTrace(imported[0].trace.traceId);
    } catch (error) {
      window.alert(`Could not import runtime trace: ${error && error.message ? error.message : "Invalid JSON file."}`);
    }
  }

  function extractRuntimeTraceImports(payload) {
    if (!payload || typeof payload !== "object") {
      return [];
    }

    if (payload.trace) {
      return [normalizeImportedRuntimeTrace(payload.trace, payload.summary || null)];
    }

    if (payload.traceId && Array.isArray(payload.events)) {
      return [normalizeImportedRuntimeTrace(payload, null)];
    }

    if (Array.isArray(payload.traces)) {
      return payload.traces
        .map((trace) => {
          const summary = Array.isArray(payload.summaries)
            ? payload.summaries.find((item) => item && item.traceId === trace.traceId)
            : null;
          return normalizeImportedRuntimeTrace(trace, summary || null);
        })
        .filter(Boolean);
    }

    return [];
  }

  function normalizeImportedRuntimeTrace(trace, summary) {
    if (!trace || typeof trace !== "object" || !trace.traceId) {
      throw new Error("Imported trace is missing traceId.");
    }

    const normalizedTrace = cloneJson(trace);
    normalizedTrace.events = Array.isArray(normalizedTrace.events) ? normalizedTrace.events : [];
    normalizedTrace.events.forEach((event) => {
      if (event && !event.traceId) {
        event.traceId = normalizedTrace.traceId;
      }
    });
    normalizedTrace.imported = true;

    const normalizedSummary = summary ? cloneJson(summary) : createRuntimeSummaryFromTrace(normalizedTrace);
    normalizedSummary.traceId = normalizedTrace.traceId;
    normalizedSummary.imported = true;

    return {
      trace: normalizedTrace,
      summary: normalizedSummary
    };
  }

  function createRuntimeSummaryFromTrace(trace) {
    const overview = createRuntimeOverview(trace, null);
    const firstEvent = trace && Array.isArray(trace.events) ? trace.events[0] || null : null;
    return {
      traceId: trace.traceId,
      rootComponentId: firstEvent && firstEvent.componentId ? firstEvent.componentId : "",
      rootDisplayName: overview.title,
      requestType: firstEvent && firstEvent.inputType ? firstEvent.inputType : "",
      responseType: firstEvent && firstEvent.outputType ? firstEvent.outputType : "",
      status: overview.status,
      startedAt: overview.startedAt,
      completedAt: overview.completedAt,
      durationMs: overview.durationMs,
      eventCount: overview.eventCount,
      droppedEventCount: 0
    };
  }

  function mergeImportedRuntimeData(data) {
    const next = data && typeof data === "object" ? data : {};
    const summaryMap = new Map((next.summaries || []).map((summary) => [summary.traceId, summary]));
    const traceMap = new Map((next.traces || []).map((trace) => [trace.traceId, trace]));

    importedRuntimeTraces.forEach((item, traceId) => {
      traceMap.set(traceId, item.trace);
      summaryMap.set(traceId, item.summary);
    });

    return {
      ...next,
      summaries: Array.from(summaryMap.values()),
      traces: Array.from(traceMap.values())
    };
  }

  function updateRuntimeCount() {
    if (runtimeCount) {
      runtimeCount.textContent = String((runtimeData.summaries || []).length);
    }
  }

  function cloneJson(value) {
    return JSON.parse(JSON.stringify(value));
  }

  function sanitizeFilename(value) {
    return String(value || "runtime-trace")
      .replace(/[^a-z0-9._-]+/gi, "-")
      .replace(/^-+|-+$/g, "")
      .slice(0, 120) || "runtime-trace";
  }

  function getCurrentRuntimeTrace() {
    return state.runtimeTraceId
      ? (runtimeData.traces || []).find((trace) => trace.traceId === state.runtimeTraceId) || null
      : null;
  }

  function renderProcessRows(items) {
    const matches = filterProcesses(items);
    if (matches.length === 0) {
      return `<div class="spider-empty-list">No items found.</div>`;
    }

    return matches.map((item) => {
      const children = orderChildren(item);
      const isBoundary = item.kind === "spider.boundary";
      const countLabel = isBoundary
        ? (getMetadata(item, "protocol") || getMetadata(item, "boundaryType") || "Boundary")
        : item.kind === "spider.pipeline"
          ? `${children.length} stages`
          : `${children.length} steps`;
      const kindLabel = isBoundary ? "Boundary" : item.kind === "spider.pipeline" ? "Pipeline" : "Flow";
      const kindClass = isBoundary ? "boundary" : item.kind === "spider.pipeline" ? "pipeline" : "flow";

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

    if (process.kind === "spider.boundary") {
      openBoundary(process, skipHash);
      return;
    }

    const previousProcessId = state.processId;
    closeRuntimeRawEventsModal();
    setRuntimeTraceShell(false);
    setRuntimeListShell(false);
    setProcessDetailShell(true);
    const children = orderChildren(process);
    state.view = process.kind === "spider.flow" ? "flows" : "pipelines";
    state.mode = "detail";
    state.processId = id;
    state.nodeId = children.length ? children[0].id : id;
    state.runtimeSelectedComponentId = "";
    state.runtimeSelectedItemKey = "";
    state.runtimeVisualMaximized = false;
    if (previousProcessId !== id) {
      state.processGraphMaximized = false;
    }
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
    const maximizedClass = state.processGraphMaximized ? " is-graph-maximized" : "";
    const maximizeText = state.processGraphMaximized ? "Restore" : "Maximize";
    const maximizeIconPath = getProcessGraphMaximizeIconPath(state.processGraphMaximized);
    const graph = showGraph
      ? renderProcessGraph(process, children)
      : `<div class="spider-empty-list">Graph disabled.</div>`;
    const selectedNode = byId.get(state.nodeId) || process;
    const selectedIndex = selectedNode.id === process.id
      ? 0
      : children.findIndex((child) => child.id === selectedNode.id) + 1;

    setActiveMenu(state.view);
    setTopbarTitle(process.displayName || kind);
    setTopbarBackLabel(backLabel);

    content.innerHTML = `
      <article class="spider-detail-view${maximizedClass}">
        <header class="spider-detail-toolbar">
          <div>
            <div class="spider-detail-actions">
              <span class="spider-chip ${process.kind === "spider.flow" ? "flow" : "pipeline"}">${escapeHtml(kind)}</span>
              <span class="spider-chip">${escapeHtml(getSignature(process))}</span>
              ${profiles.map((profile) => `<span class="spider-chip profile">${escapeHtml(profile.displayName || profile.id)}</span>`).join("")}
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
              <div class="spider-panel-title">
                <h2>${escapeHtml(getGraphPanelTitle(process))}</h2>
              </div>
              <div class="spider-graph-actions">
                ${renderGraphLegend(process)}
                <button class="spider-graph-maximize-button" type="button" data-process-graph-maximize aria-label="${escapeAttribute(maximizeText)} graph" title="${escapeAttribute(maximizeText)} graph" aria-pressed="${state.processGraphMaximized ? "true" : "false"}">
                  <svg class="spider-graph-maximize-icon" viewBox="0 0 24 24" aria-hidden="true" focusable="false">
                    <path d="${escapeAttribute(maximizeIconPath)}" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" />
                  </svg>
                </button>
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

  function openBoundary(boundary, skipHash) {
    closeRuntimeRawEventsModal();
    setRuntimeTraceShell(false);
    setRuntimeListShell(false);
    setProcessDetailShell(true);
    state.view = "boundaries";
    state.mode = "detail";
    state.processId = boundary.id;
    state.nodeId = boundary.id;
    state.runtimeSelectedComponentId = "";
    state.runtimeSelectedItemKey = "";
    state.runtimeVisualMaximized = false;
    state.processGraphMaximized = false;
    if (!skipHash) {
      setHash(boundary.id);
    }

    renderBoundaryDetail(boundary);
    resetMainScroll();
  }

  function renderBoundaryDetail(boundary) {
    setActiveMenu("boundaries");
    setTopbarTitle(boundary.displayName || "Boundary");
    setTopbarBackLabel("Back to Boundaries");

    content.innerHTML = `
      <article class="spider-detail-view spider-boundary-detail-view">
        <header class="spider-detail-toolbar">
          <div>
            <div class="spider-detail-actions">
              <span class="spider-chip boundary">Boundary</span>
              <span class="spider-chip">${escapeHtml(getSignature(boundary))}</span>
            </div>
            <h1 class="spider-detail-title">${escapeHtml(boundary.displayName || boundary.id)}</h1>
            <p class="spider-detail-subtitle">${escapeHtml(describeBoundary(boundary))}</p>
          </div>
        </header>
        <div class="spider-boundary-layout">
          <aside class="spider-panel spider-process-summary">
            ${renderBoundarySummary(boundary)}
          </aside>
          <section id="spider-node-detail" class="spider-node-detail spider-boundary-node-detail">
            ${renderNodeDetail(boundary, boundary, 0)}
          </section>
        </div>
      </article>`;
  }

  function renderBoundarySummary(boundary) {
    const summaryRows = [
      { label: "Purpose", key: "purpose", value: getMetadata(boundary, "purpose") },
      { label: "Entry point", key: "entryPoint", value: getMetadata(boundary, "entryPoint") },
      { label: "Protocol", key: "protocol", value: getMetadata(boundary, "protocol") },
      { label: "Operation", key: "operation", value: getMetadata(boundary, "operation") },
      { label: "Contract", key: "contract", value: getMetadata(boundary, "contract") },
      { label: "Policies", key: "policies", value: getMetadata(boundary, "policies") },
      { label: "Security", key: "security", value: getMetadata(boundary, "security") },
      { label: "SLA", key: "sla", value: getMetadata(boundary, "sla") },
      { label: "Timeout", key: "timeout", value: getMetadata(boundary, "timeout") }
    ].filter((row) => row.value);

    const tags = getTags(boundary);
    return `
      <div class="spider-panel-header">
        <h2>Boundary summary</h2>
      </div>
      <div class="spider-summary-grid">
        <div class="spider-summary-item">
          <span class="spider-summary-label">Type</span>
          <span class="spider-summary-value">${escapeHtml(getMetadata(boundary, "boundaryType") || "Execution boundary")}</span>
        </div>
        ${summaryRows.map((row) => `
        <div class="spider-summary-item">
          <span class="spider-summary-label">${escapeHtml(row.label)}</span>
          <span class="spider-summary-value">${escapeHtml(formatMetadataValue(row.key, row.value))}</span>
        </div>`).join("")}
        ${tags.length ? `
        <div class="spider-summary-item">
          <span class="spider-summary-label">Tags</span>
          <span class="spider-summary-value">${escapeHtml(tags.join(", "))}</span>
        </div>` : ""}
      </div>
      ${renderBoundaryRelations(boundary)}`;
  }

  function renderBoundaryRelations(boundary) {
    const pipelines = getOutgoing(boundary.id, "boundary-invokes-pipeline");
    const flows = getOutgoing(boundary.id, "boundary-invokes-flow");
    const sections = [];
    if (pipelines.length) {
      sections.push(renderRelatedButtons("Linked pipelines", pipelines, "Open pipeline"));
    }

    if (flows.length) {
      sections.push(renderRelatedButtons("Linked flows", flows, "Open flow"));
    }

    return sections.length ? `<div class="spider-detail-section">${sections.join("")}</div>` : "";
  }

  function renderProcessSummary(process, children, profiles) {
    const kind = process.kind === "spider.flow" ? "Flow" : "Pipeline";
    const childLabel = process.kind === "spider.flow" ? "Steps" : "Stages";
    const profileText = profiles.length
      ? profiles.map((profile) => profile.displayName || profile.id).join(", ")
      : "None";
    const description = getMetadata(process, "description");
    const tags = getTags(process);
    const extraSummary = [
      { label: "Purpose", key: "purpose", value: getMetadata(process, "purpose") },
      { label: "Trigger", key: "trigger", value: getMetadata(process, "trigger") },
      { label: "Wraps", key: "wraps", value: getMetadata(process, "wraps") },
      { label: "Input", key: "input", value: getMetadata(process, "input") },
      { label: "Output", key: "output", value: getMetadata(process, "output") },
      { label: "Policies", key: "policies", value: getMetadata(process, "policies") },
      { label: "Failure behavior", key: "failureBehavior", value: getMetadata(process, "failureBehavior") },
      { label: "Module", key: "module", value: getMetadata(process, "module") }
    ].filter((row) => row.value);

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
        ${extraSummary.map((row) => `
        <div class="spider-summary-item">
          <span class="spider-summary-label">${escapeHtml(row.label)}</span>
          <span class="spider-summary-value">${escapeHtml(formatMetadataValue(row.key, row.value))}</span>
        </div>`).join("")}
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
    const entryBoundaries = getGraphEntryBoundaries(process);
    const usedByPipelines = getIncoming(process.id, "pipeline-invokes-flow");
    const usedBySteps = getIncoming(process.id, "invokes-flow")
      .map((source) => {
        const owner = findOwningProcess(source.id);
        return owner || source;
      })
      .filter((item, index, items) => items.findIndex((candidate) => candidate.id === item.id) === index);

    const sections = [];
    if (entryBoundaries.length) {
      sections.push(renderRelatedButtons("Entry boundaries", entryBoundaries, "Open boundary"));
    }

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

  function getGraphPanelTitle(process) {
    return process.kind === "spider.flow" ? "Flowchart" : "Pipeline graph";
  }

  function renderGraphLegend(process) {
    if (process.kind === "spider.flow") {
      return `
        <div class="spider-graph-legend" aria-label="Flowchart operation legend">
          <span class="spider-legend-item"><i class="spider-flow-legend-icon start"></i>Start</span>
          <span class="spider-legend-item"><i class="spider-flow-legend-icon process"></i>Process</span>
          <span class="spider-legend-item"><i class="spider-flow-legend-icon decision"></i>Decision</span>
          <span class="spider-legend-item"><i class="spider-flow-legend-icon route"></i>Route</span>
          <span class="spider-legend-item"><i class="spider-flow-legend-icon parallel"></i>Parallel</span>
          <span class="spider-legend-item"><i class="spider-flow-legend-icon foreach"></i>For each / batch</span>
          <span class="spider-legend-item"><i class="spider-flow-legend-icon subflow"></i>Linked flow</span>
        </div>`;
    }

    return `
      <div class="spider-graph-legend" aria-label="Pipeline stage legend">
        ${renderPipelineLegendItem("pre", "Pre-process")}
        ${renderPipelineLegendItem("middleware", "Middleware")}
        ${renderPipelineLegendItem("target", "Target")}
        ${renderPipelineLegendItem("parallel", "Parallel")}
        ${renderPipelineLegendItem("success", "Success")}
        ${renderPipelineLegendItem("failure", "Failure")}
        ${renderPipelineLegendItem("boundary", "Boundary")}
      </div>`;
  }

  function renderPipelineLegendItem(role, label) {
    return `<span class="spider-legend-item"><span class="spider-pipeline-legend-icon ${escapeAttribute(role)}" aria-hidden="true">${renderPipelineLegendIcon(role)}</span>${escapeHtml(label)}</span>`;
  }

  function renderPipelineLegendIcon(role) {
    const rounded = role === "success" || role === "boundary" ? 7 : 5;
    const sideAccent = role === "parallel" || role === "success" || role === "boundary"
      ? ""
      : `<rect class="spider-pipeline-legend-accent" x="1.5" y="2.5" width="3" height="13" rx="1.5"></rect>`;
    const parallelAccent = role === "parallel"
      ? `<path class="spider-pipeline-legend-mark" d="M7 4.5H27M7 13.5H27"></path>`
      : "";
    const boundaryAccent = role === "boundary"
      ? `<circle class="spider-pipeline-legend-mark" cx="7.5" cy="9" r="4.4"></circle>`
      : "";

    return `
      <svg class="spider-pipeline-legend-svg" viewBox="0 0 34 18" focusable="false">
        <rect class="spider-pipeline-legend-frame" x="1.5" y="2.5" width="31" height="13" rx="${rounded}"></rect>
        ${sideAccent}
        ${parallelAccent}
        ${boundaryAccent}
        <g transform="translate(12, 3)">${renderRoleGlyphShape(role, "spider-pipeline-legend-mark", "spider-pipeline-legend-fill")}</g>
      </svg>`;
  }

  function renderRoleGlyphShape(role, markClass, fillClass) {
    switch (role) {
      case "pre":
        return `<path class="${markClass}" d="M1.5 6H9.5M6.8 3.4L9.5 6L6.8 8.6"></path>`;
      case "middleware":
        return `<path class="${markClass}" d="M4 2.3H2.3V9.7H4M8 2.3H9.7V9.7H8M5.2 4.1H6.8M5.2 7.9H6.8"></path>`;
      case "target":
        return `<circle class="${markClass}" cx="6" cy="6" r="4"></circle><circle class="${markClass}" cx="6" cy="6" r="1.4"></circle><path class="${markClass}" d="M6 1.2V3M6 9V10.8M1.2 6H3M9 6H10.8"></path>`;
      case "parallel":
        return `<path class="${markClass}" d="M3 2V10M9 2V10M3 4H9M3 8H9"></path>`;
      case "success":
        return `<path class="${markClass}" d="M2.3 6.2L4.7 8.5L9.8 3.4"></path>`;
      case "failure":
        return `<path class="${markClass}" d="M3 3L9 9M9 3L3 9"></path>`;
      case "boundary":
        return `<path class="${markClass}" d="M2.8 2.5H8.3V9.5H2.8ZM8.4 4.2L10.2 6L8.4 7.8M5 6H10.1"></path>`;
      case "pipeline":
        return `<path class="${markClass}" d="M2.2 3H9.8M2.2 6H9.8M2.2 9H9.8"></path>`;
      default:
        return `<circle class="${fillClass}" cx="6" cy="6" r="2.2"></circle>`;
    }
  }

  function renderJson() {
    setRuntimeTraceShell(false);
    setRuntimeListShell(false);
    setProcessDetailShell(false);
    state.processGraphMaximized = false;
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

  function renderProcessGraph(process, children, graphContext) {
    if (process.kind === "spider.flow") {
      return renderFlowchart(process, children, graphContext);
    }

    return renderVerticalGraph(process, children, graphContext);
  }

  function renderFlowchart(process, children, graphContext) {
    const content = children.length
      ? `${renderFlowchartNode(process, "0", graphContext, " is-root", getSignature(process))}
         <span class="spider-flowchart-connector" aria-hidden="true"></span>
         ${renderFlowchartSequence(children, "", graphContext)}`
      : renderFlowchartNode(process, "0", graphContext, " is-root", getSignature(process));

    return `
      <div class="spider-flowchart${graphContext ? " is-runtime-flowchart" : ""}" role="img" aria-label="${escapeAttribute(process.displayName || "Spider flowchart")}">
        ${content}
      </div>`;
  }

  function renderFlowchartSequence(items, prefix, graphContext) {
    return `
      <div class="spider-flowchart-sequence">
        ${items.map((item, index) => {
          const number = prefix ? `${prefix}.${index + 1}` : String(index + 1).padStart(2, "0");
          const connector = index === 0 ? "" : `<span class="spider-flowchart-connector" aria-hidden="true"></span>`;
          return `${connector}${renderFlowchartItem(item, number, graphContext)}`;
        }).join("")}
      </div>`;
  }

  function renderFlowchartItem(node, number, graphContext) {
    if (node.kind === "spider.flow-branch" && getBranchRoutes(node).length) {
      return renderFlowchartBranch(node, number, graphContext);
    }

    return renderFlowchartNode(node, number, graphContext, getFlowchartSemanticClass(node), getNodeSubtitle(node));
  }

  function renderFlowchartBranch(branch, number, graphContext) {
    const routes = getBranchRoutes(branch);
    return `
      <div class="spider-flowchart-branch" style="--route-count: ${escapeAttribute(String(Math.max(1, routes.length)))}">
        ${renderFlowchartNode(branch, number, graphContext, getFlowchartSemanticClass(branch), getNodeSubtitle(branch))}
        <span class="spider-flowchart-split" aria-hidden="true"></span>
        <div class="spider-flowchart-routes">
          ${routes.map((route, routeIndex) => renderFlowchartRoute(route, `${number}.${routeIndex + 1}`, graphContext)).join("")}
        </div>
        <span class="spider-flowchart-join" aria-hidden="true"></span>
      </div>`;
  }

  function renderFlowchartRoute(route, number, graphContext) {
    const steps = getRouteSteps(route);
    return `
      <div class="spider-flowchart-route">
        <div class="spider-flowchart-route-body">
          ${renderFlowchartNode(route, number, graphContext, getFlowchartSemanticClass(route), getNodeSubtitle(route))}
          ${steps.length ? `<span class="spider-flowchart-connector" aria-hidden="true"></span>${renderFlowchartSequence(steps, number, graphContext)}` : ""}
        </div>
      </div>`;
  }

  function normalizeFlowchartNumber(number) {
    const text = String(number || "");
    if (!text || text === "0") {
      return text;
    }

    return text
      .split(".")
      .map((segment) => segment.replace(/^0+(?=\d)/, ""))
      .join(".");
  }

  function createFlowchartNumberView(number) {
    const full = normalizeFlowchartNumber(number);
    if (!full || full === "0" || full.length <= 7) {
      return { full, display: full, compact: false };
    }

    const segments = full.split(".");
    if (segments.length < 4 && full.length <= 9) {
      return { full, display: full, compact: false };
    }

    const first = segments[0];
    const tail = segments.slice(-2).join(".");
    const anchored = `${first}.…${tail}`;
    if (anchored.length <= 9) {
      return { full, display: anchored, compact: true };
    }

    const tailOnly = `…${tail}`;
    if (tailOnly.length <= 8) {
      return { full, display: tailOnly, compact: true };
    }

    const last = segments[segments.length - 1] || full;
    const lastOnly = `…${last}`;
    if (lastOnly.length <= 8) {
      return { full, display: lastOnly, compact: true };
    }

    return { full, display: `…${last.slice(-7)}`, compact: true };
  }

  function renderFlowchartNode(node, number, graphContext, extraClass, subtitleOverride) {
    const selected = graphContext
      ? node.id === state.runtimeSelectedComponentId ? " is-selected" : ""
      : node.id === state.nodeId ? " is-selected" : "";
    const linkedFlow = getFirstLinkedFlow(node);
    const linkedClass = linkedFlow ? " is-linked-flow" : "";
    const semanticClass = extraClass || getFlowchartSemanticClass(node);
    const runtimeClass = getRuntimeGraphClass(node.id, graphContext);
    const tooltipAttributes = renderGraphTooltipAttributes(node);
    const subtitle = subtitleOverride || getNodeSubtitle(node);
    const numberView = createFlowchartNumberView(number);
    const numberSizeClass = `${numberView.display.length > 5 ? " is-long" : ""}${numberView.compact ? " is-compact" : ""}`;
    const subtitleHtml = shouldShowFlowchartSubtitle(node, semanticClass, subtitle)
      ? `<span class="spider-flowchart-node-subtitle">${escapeHtml(subtitle)}</span>`
      : "";
    const linkBadge = linkedFlow
      ? `<span class="spider-flowchart-link" data-open-process="${escapeAttribute(linkedFlow.id)}" title="Open related flow: ${escapeAttribute(linkedFlow.displayName || linkedFlow.id)}">Flow</span>`
      : "";

    return `
      <button class="spider-flowchart-node${semanticClass}${linkedClass}${runtimeClass}${selected}" type="button" data-node-id="${escapeAttribute(node.id)}"${tooltipAttributes}>
        <span class="spider-flowchart-node-shape${numberSizeClass}" aria-hidden="true" title="${escapeAttribute(numberView.full)}">
          <span class="spider-flowchart-node-index">${escapeHtml(numberView.display)}</span>
        </span>
        <span class="spider-flowchart-node-main">
          <span class="spider-flowchart-node-kind">${escapeHtml(getFlowchartRoleLabel(node))}</span>
          <span class="spider-flowchart-node-title">${escapeHtml(node.displayName || node.id)}</span>
          ${subtitleHtml}
        </span>
        ${linkBadge}
      </button>`;
  }

  function shouldShowFlowchartSubtitle(node, semanticClass, subtitle) {
    if (!subtitle) {
      return false;
    }

    if (semanticClass && (semanticClass.includes("is-branch") || semanticClass.includes("is-condition") || semanticClass.includes("is-route"))) {
      return false;
    }

    const compactRoles = ["parallel", "foreach", "batch", "loop", "merge", "aggregate", "checkpoint"];
    if (compactRoles.includes(getFlowchartRole(node))) {
      return false;
    }

    return node.kind === "spider.flow" || node.kind === "spider.pipeline";
  }

  function renderVerticalGraph(process, children, graphContext) {
    const branchWidth = getGraphBranchWidth(children);
    const entryBoundaries = getGraphEntryBoundaries(process);
    const nodeWidth = 250;
    const nodeHeight = 50;
    const boundaryNodeWidth = entryBoundaries.length > 1 ? 220 : nodeWidth;
    const boundaryGapX = 24;
    const boundaryWidth = entryBoundaries.length
      ? (entryBoundaries.length * boundaryNodeWidth) + ((entryBoundaries.length - 1) * boundaryGapX)
      : 0;
    const width = Math.max(420, branchWidth, boundaryWidth + 36);
    const nodeX = Math.round((width - nodeWidth) / 2);
    const top = 16;
    const gap = 74;
    const center = nodeX + (nodeWidth / 2);
    const edges = [];
    const renderedNodes = [];
    const entryExits = [];
    let y = top;

    if (entryBoundaries.length) {
      const boundaryStartX = Math.round((width - boundaryWidth) / 2);
      entryBoundaries.forEach((boundary, boundaryIndex) => {
        const boundaryX = boundaryStartX + (boundaryIndex * (boundaryNodeWidth + boundaryGapX));
        const boundaryCenter = boundaryX + (boundaryNodeWidth / 2);
        renderedNodes.push(renderGraphNode(boundary, `B${boundaryIndex + 1}`, boundaryX, y, boundaryNodeWidth, nodeHeight, getGraphClass(boundary), getNodeSubtitle(boundary), graphContext));
        entryExits.push({ x: boundaryCenter, y: y + nodeHeight, id: boundary.id });
      });
      y += gap;
    }

    renderedNodes.push(renderGraphNode(process, "0", nodeX, y, nodeWidth, nodeHeight, " is-root", getSignature(process), graphContext));
    entryExits.forEach((entry) => {
      edges.push(renderGraphEdge(entry.x, entry.y, center, y, entry.id, process.id, graphContext));
    });
    let previousExit = { x: center, y: y + nodeHeight, id: process.id };
    y += gap;

    for (let index = 0; index < children.length; index++) {
      const node = children[index];
      const number = String(index + 1).padStart(2, "0");

      edges.push(renderGraphEdge(previousExit.x, previousExit.y, center, y, previousExit.id, node.id, graphContext));

      if (node.kind === "spider.flow-branch" && getBranchRoutes(node).length) {
        renderedNodes.push(renderGraphNode(node, number, nodeX, y, nodeWidth, nodeHeight, getGraphClass(node), getNodeSubtitle(node), graphContext));
        const branchExit = { x: center, y: y + nodeHeight, id: node.id };
        const layout = renderBranchGraph(node, number, branchExit, y + 86, width, graphContext);
        edges.push(...layout.edges);
        renderedNodes.push(...layout.nodes);
        previousExit = { x: center, y: layout.exitY, id: node.id };
        y = layout.exitY + 48;
        continue;
      }

      renderedNodes.push(renderGraphNode(node, number, nodeX, y, nodeWidth, nodeHeight, getGraphClass(node), getNodeSubtitle(node), graphContext));
      previousExit = { x: center, y: y + nodeHeight, id: node.id };
      y += gap;
    }

    const height = Math.max(260, y + 24);

    return `
      <svg class="spider-architecture-graph${graphContext ? " is-runtime-graph" : ""}" style="--spider-graph-width: ${width}px" viewBox="0 0 ${width} ${height}" role="img" aria-label="${escapeAttribute(process.displayName || "Spider process graph")}" preserveAspectRatio="xMidYMin meet">
        <defs>
          <marker id="spider-arrow" viewBox="0 0 10 10" refX="8" refY="5" markerWidth="6" markerHeight="6" orient="auto-start-reverse">
            <path d="M 0 0 L 10 5 L 0 10 z" fill="#9aa4b2"></path>
          </marker>
        </defs>
        ${edges.join("")}
        ${renderedNodes.join("")}
      </svg>`;
  }

  function getGraphEntryBoundaries(process) {
    if (process.kind === "spider.pipeline") {
      return getIncoming(process.id, "boundary-invokes-pipeline");
    }

    if (process.kind === "spider.flow") {
      return getIncoming(process.id, "boundary-invokes-flow");
    }

    return [];
  }

  function renderBranchGraph(branch, branchNumber, branchExit, startY, width, graphContext) {
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
      nodes.push(renderGraphNode(route, routeNumber, routeX, startY, routeWidth, routeHeight, getGraphClass(route), getNodeSubtitle(route), graphContext));
      edges.push(renderGraphEdge(branchExit.x, branchExit.y, routeCenter, startY, branchExit.id, route.id, graphContext));

      let currentExit = { x: routeCenter, y: startY + routeHeight, id: route.id };
      let stepY = startY + routeGapY;
      const steps = getRouteSteps(route);

      steps.forEach((step, stepIndex) => {
        edges.push(renderGraphEdge(currentExit.x, currentExit.y, routeCenter, stepY, currentExit.id, step.id, graphContext));
        nodes.push(renderGraphNode(step, `${routeNumber}.${stepIndex + 1}`, routeX, stepY, routeWidth, routeHeight, " is-route-step", getNodeSubtitle(step), graphContext));
        currentExit = { x: routeCenter, y: stepY + routeHeight, id: step.id };
        stepY += routeGapY;
      });

      maxBottom = Math.max(maxBottom, currentExit.y);
      route._spiderGraphExit = currentExit;
    });

    const exitY = maxBottom + 34;
    routes.forEach((route) => {
      const exit = route._spiderGraphExit;
      if (exit) {
        edges.push(renderGraphEdge(exit.x, exit.y, branchExit.x, exitY, exit.id, branchExit.id, graphContext));
        delete route._spiderGraphExit;
      }
    });

    return { edges, nodes, exitY };
  }

  function renderGraphNode(node, number, x, y, width, height, extraClass, subtitle, graphContext) {
    const selected = graphContext
      ? node.id === state.runtimeSelectedComponentId ? " is-selected" : ""
      : node.id === state.nodeId ? " is-selected" : "";
    const linkedFlow = getFirstLinkedFlow(node);
    const linkedClass = linkedFlow ? " is-linked-flow" : "";
    const graphClass = (extraClass || "") + linkedClass + getRuntimeGraphClass(node.id, graphContext);
    const numberText = String(number || "");
    const roleMarker = getGraphRoleMarker(node, extraClass || "");
    const textX = numberText.length > 4 ? 58 : 43;
    const titleFontSize = numberText.length > 4 ? 9.4 : 10;
    const subtitleFontSize = numberText.length > 4 ? 8.5 : 8.8;
    const titleLimit = width > 220
      ? (linkedFlow ? 22 : roleMarker ? 24 : 30)
      : (linkedFlow ? 16 : roleMarker ? 18 : 21);
    const subtitleLimit = width > 220
      ? (linkedFlow ? 26 : roleMarker ? 28 : 34)
      : (linkedFlow ? 18 : roleMarker ? 21 : 24);
    const tooltipAttributes = renderGraphTooltipAttributes(node);
    const nativeTitle = tooltipAttributes
      ? ""
      : `<title>${escapeHtml(node.displayName || node.id)}</title>`;
    const roleMarkerX = roleMarker
      ? width - roleMarker.width - (linkedFlow ? 36 : 10)
      : 0;
    const roleMarkerSvg = roleMarker ? renderGraphRolePill(roleMarker, roleMarkerX) : "";
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
        ${roleMarkerSvg}
        ${linkBadge}
      </g>`;
  }

  function renderGraphRolePill(marker, x) {
    const icon = renderRoleGlyphShape(marker.role, "spider-node-role-pill-mark", "spider-node-role-pill-fill");
    return `
        <g class="spider-node-stage-pill spider-node-role-pill is-${escapeAttribute(marker.role)}" transform="translate(${x}, 8)">
          <rect class="spider-node-stage-pill-bg spider-node-role-pill-bg" width="${marker.width}" height="16" rx="8"></rect>
          <g transform="translate(5, 2.2)">${icon}</g>
          <text class="spider-node-stage-pill-text spider-node-role-pill-text" x="${marker.width - 5}" y="10.9" text-anchor="end">${escapeHtml(marker.label)}</text>
        </g>`;
  }

  function renderGraphEdge(fromX, fromY, toX, toY, fromId, toId, graphContext) {
    const midY = fromY + Math.max(16, Math.round((toY - fromY) / 2));
    return `<path class="spider-edge${getRuntimeEdgeClass(fromId, toId, graphContext)}" d="M ${fromX} ${fromY} C ${fromX} ${midY}, ${toX} ${midY}, ${toX} ${toY - 7}" marker-end="url(#spider-arrow)" />`;
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

    const node = target.closest(".spider-graph-node, .spider-flowchart-node");
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
    if (state.view === "runtime" && state.mode === "trace") {
      selectRuntimeGraphNode(id);
      return;
    }

    const node = byId.get(id);
    const process = byId.get(state.processId);
    if (!node || !process) {
      return;
    }

    state.nodeId = id;
    document.querySelectorAll(".spider-graph-node, .spider-flowchart-node").forEach((item) => {
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

  function selectRuntimeItem(key) {
    if (!key) {
      return;
    }

    state.runtimeSelectedItemKey = key;
    state.runtimeSelectedComponentId = "";
    const trace = (runtimeData.traces || []).find((item) => item.traceId === state.runtimeTraceId);
    if (!trace) {
      return;
    }

    refreshRuntimeSelectionDetail(trace);
  }

  function refreshRuntimeSelectionDetail(trace) {
    const overview = createRuntimeOverview(trace, findSummary(trace.traceId));
    const graphContext = createRuntimeGraphContext(overview);

    document.querySelectorAll(".spider-runtime-timeline-card").forEach((item) => {
      item.classList.toggle("is-selected", item.getAttribute("data-runtime-item-key") === state.runtimeSelectedItemKey);
    });
    document.querySelectorAll(".spider-graph-node, .spider-flowchart-node").forEach((item) => {
      item.classList.remove("is-selected");
    });

    const detail = document.getElementById("spider-runtime-node-detail");
    if (detail) {
      detail.innerHTML = renderRuntimeNodeDetail(graphContext);
    }
  }

  function setRuntimeVisualView(view) {
    const nextView = view === "flow" ? "flow" : "story";
    if (state.runtimeVisualView === nextView) {
      return;
    }

    state.runtimeVisualView = nextView;
    const trace = (runtimeData.traces || []).find((item) => item.traceId === state.runtimeTraceId);
    if (!trace) {
      return;
    }

    preserveMainScroll(() => renderTraceDetail(trace));
  }

  function setRuntimeVisualMaximized(maximized) {
    state.runtimeVisualMaximized = Boolean(maximized);
    const trace = (runtimeData.traces || []).find((item) => item.traceId === state.runtimeTraceId);
    if (!trace) {
      return;
    }

    preserveRuntimeVisualScroll(() => renderTraceDetail(trace));
  }

  function setProcessGraphMaximized(maximized) {
    state.processGraphMaximized = Boolean(maximized);
    const process = byId.get(state.processId);
    if (!process) {
      return;
    }

    preserveProcessGraphScroll(() => renderProcessDetail(process, orderChildren(process)));
  }

  function selectRuntimeGraphNode(id) {
    if (!id || !byId.has(id)) {
      return;
    }

    state.runtimeSelectedItemKey = "";
    state.runtimeSelectedComponentId = id;
    const trace = (runtimeData.traces || []).find((item) => item.traceId === state.runtimeTraceId);
    if (!trace) {
      return;
    }

    document.querySelectorAll(".spider-graph-node, .spider-flowchart-node").forEach((item) => {
      item.classList.toggle("is-selected", item.getAttribute("data-node-id") === id);
    });
    document.querySelectorAll(".spider-runtime-timeline-card").forEach((item) => {
      item.classList.remove("is-selected");
    });

    const overview = createRuntimeOverview(trace, findSummary(trace.traceId));
    const graphContext = createRuntimeGraphContext(overview);
    const detail = document.getElementById("spider-runtime-node-detail");
    if (detail) {
      detail.innerHTML = renderRuntimeNodeDetail(graphContext);
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
    const stageSummary = renderPipelineStageSummary(node);

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
        ${stageSummary}
        <dl class="spider-definition">
          ${rows.map(([label, value]) => `<dt>${escapeHtml(label)}</dt><dd>${escapeHtml(value || "Not declared")}</dd>`).join("")}
        </dl>
        ${branchRows}
        ${relationRows}
        ${evidenceRows}
      </div>`;
  }

  function renderPipelineStageSummary(node) {
    if (node.kind !== "spider.pipeline-stage") {
      return "";
    }

    const summaryRows = [
      ["Stage role", getPipelineStageLabel(node)],
      ["Configured actions", getMetadata(node, "count")],
      ["Purpose", getMetadata(node, "purpose")],
      ["Wraps", getMetadata(node, "wraps")],
      ["Input", getMetadata(node, "input")],
      ["Output", getMetadata(node, "output")],
      ["Module", getMetadata(node, "module")],
      ["Policies", getMetadata(node, "policies")],
      ["Security", getMetadata(node, "security")],
      ["Observability", getMetadata(node, "observability")],
      ["Failure behavior", getMetadata(node, "failureBehavior")],
      ["Timeout", getMetadata(node, "timeout")],
      ["SLA", getMetadata(node, "sla")],
      ["External system", getMetadata(node, "external")],
      ["Related flow", getMetadata(node, "relatedFlow")],
      ["Target method", getMetadata(node, "target")],
      ["Override configured", getMetadata(node, "hasOverride")]
    ]
      .filter(([, value]) => value)
      .map(([label, value]) => {
        const metadataKey = getMetadataKeyFromLabel(label);
        return `
          <div class="spider-summary-item">
            <span class="spider-summary-label">${escapeHtml(label)}</span>
            <span class="spider-summary-value">${escapeHtml(formatMetadataValue(metadataKey, value))}</span>
          </div>`;
      })
      .join("");

    if (!summaryRows) {
      return "";
    }

    return `
      <div class="spider-stage-summary">
        <h3 class="spider-stage-summary-title">Stage summary</h3>
        <div class="spider-summary-grid">
          ${summaryRows}
        </div>
      </div>`;
  }

  function getMetadataKeyFromLabel(label) {
    const keys = {
      "Configured actions": "count",
      "External system": "external",
      "Failure behavior": "failureBehavior",
      "Override configured": "hasOverride",
      "Related flow": "relatedFlow",
      "Stage role": "stage",
      "Target method": "target"
    };

    return keys[label] || label.charAt(0).toLowerCase() + label.slice(1).replace(/\s+/g, "");
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
      if (key === "name" || key === "description" || key === "tags" || isPromotedStageMetadataKey(node, key)) {
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

  function isPromotedStageMetadataKey(node, key) {
    if (node.kind !== "spider.pipeline-stage") {
      return false;
    }

    return [
      "count",
      "external",
      "failureBehavior",
      "hasOverride",
      "input",
      "module",
      "observability",
      "output",
      "policies",
      "purpose",
      "relatedFlow",
      "security",
      "sla",
      "stage",
      "target",
      "timeout",
      "wraps"
    ].includes(key);
  }

  function getMetadataLabel(key) {
    const labels = {
      boundary: "Boundary implementation",
      boundaryKind: "Boundary kind",
      boundaryType: "Boundary type",
      branchType: "Branch type",
      condition: "Condition",
      contract: "Contract",
      count: "Configured actions",
      delegate: "Action",
      entryPoint: "Entry point",
      external: "External system",
      failureBehavior: "Failure behavior",
      genericArguments: "Type arguments",
      hasOverride: "Override configured",
      hasResponse: "Returns value",
      input: "Input",
      invokesFlow: "Invokes flow",
      invokesPipeline: "Invokes pipeline",
      module: "Module",
      observability: "Observability",
      operation: "Fluent call",
      otherwise: "Otherwise",
      policies: "Policies",
      protocol: "Protocol",
      purpose: "Purpose",
      relatedFlow: "Related flow",
      request: "Input",
      response: "Output",
      routeKind: "Route type",
      security: "Security",
      service: "Service",
      sla: "SLA",
      stage: "Pipeline stage",
      target: "Target method",
      timeout: "Timeout",
      trigger: "Trigger",
      wraps: "Wraps"
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

    if (key === "tags" || key === "policies" || key === "security" || key === "observability") {
      return String(value || "").split(",").map((item) => item.trim()).filter(Boolean).join(", ");
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

  function describeBoundary(boundary) {
    return getMetadata(boundary, "description") ||
      getMetadata(boundary, "purpose") ||
      "Execution boundary documented at compile time.";
  }

  function getSignature(component) {
    if (component.kind === "spider.boundary") {
      return getMetadata(component, "contract") ||
        getMetadata(component, "entryPoint") ||
        getMetadata(component, "boundary") ||
        "Boundary";
    }

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
    if (component.kind === "spider.boundary") {
      return getMetadata(component, "entryPoint") ||
        getMetadata(component, "boundaryType") ||
        getMetadata(component, "protocol") ||
        "Entry boundary";
    }

    if (component.kind === "spider.pipeline-stage") {
      const count = getMetadata(component, "count") || "0";
      return `${getPipelineStageLabel(component)} · ${count} action${count === "1" ? "" : "s"}`;
    }

    return getMetadata(component, "delegate") || getMetadata(component, "operation") || getFriendlyKind(component);
  }

  function getPipelineStageKey(component) {
    return (getMetadata(component, "stage") || "").toLowerCase();
  }

  function getPipelineStageVisualKey(component) {
    switch (getPipelineStageKey(component)) {
      case "pre-process":
        return "pre";
      case "middleware":
        return "middleware";
      case "target":
        return "target";
      case "parallel":
        return "parallel";
      case "post-success":
        return "success";
      case "post-failure":
        return "failure";
      default:
        return "";
    }
  }

  function getPipelineStageLabel(component) {
    switch (getPipelineStageKey(component)) {
      case "pre-process":
        return "Pre-process";
      case "middleware":
        return "Middleware";
      case "target":
        return getMetadata(component, "hasOverride") === "true" ? "Target override" : "Target";
      case "parallel":
        return "Parallel";
      case "post-success":
        return "Success";
      case "post-failure":
        return "Failure";
      default:
        return "Pipeline stage";
    }
  }

  function getPipelineStageMarker(component) {
    if (component.kind !== "spider.pipeline-stage") {
      return null;
    }

    switch (getPipelineStageVisualKey(component)) {
      case "pre":
        return { label: "PRE", width: 31 };
      case "middleware":
        return { label: "MID", width: 31 };
      case "target":
        return { label: "TARGET", width: 49 };
      case "parallel":
        return { label: "PAR", width: 31 };
      case "success":
        return { label: "SUCCESS", width: 52 };
      case "failure":
        return { label: "FAILURE", width: 52 };
      default:
        return null;
    }
  }

  function getGraphRoleMarker(component, extraClass) {
    if (component.kind === "spider.boundary") {
      return createGraphRoleMarker("boundary", "BOUND");
    }

    if (component.kind === "spider.pipeline" && (extraClass || "").includes("is-root")) {
      return createGraphRoleMarker("pipeline", "PIPE");
    }

    const stageMarker = getPipelineStageMarker(component);
    if (!stageMarker) {
      return null;
    }

    return createGraphRoleMarker(getPipelineStageVisualKey(component), stageMarker.label);
  }

  function createGraphRoleMarker(role, label) {
    return {
      role,
      label,
      width: Math.max(42, Math.round((label.length * 5.2) + 26))
    };
  }

  function getFriendlyKind(component) {
    if (component.kind === "spider.boundary") {
      return "Boundary";
    }

    if (component.kind === "spider.pipeline") {
      return "Pipeline";
    }

    if (component.kind === "spider.flow") {
      return "Flow";
    }

    if (component.kind === "spider.pipeline-stage") {
      return `${getPipelineStageLabel(component)} stage`;
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
    if (component.kind === "spider.boundary") {
      return "spider-kind-boundary";
    }

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
    if (component.kind === "spider.boundary") {
      return " is-boundary";
    }

    if (component.kind === "spider.pipeline-stage") {
      const visualKey = getPipelineStageVisualKey(component);
      return ` is-stage${visualKey ? ` is-pipeline-${visualKey}` : ""}`;
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

  function getFlowchartSemanticClass(component) {
    const role = getFlowchartRole(component);
    if (component.kind === "spider.flow") {
      return " is-root";
    }

    if (component.kind === "spider.flow-branch") {
      return " is-branch";
    }

    if (component.kind === "spider.flow-condition") {
      return " is-condition";
    }

    if (component.kind === "spider.flow-branch-route") {
      return " is-route";
    }

    if (role === "transform") {
      return " is-transform";
    }

    if (role === "parallel") {
      return " is-parallel";
    }

    if (role === "foreach") {
      return " is-foreach";
    }

    if (role === "batch") {
      return " is-batch";
    }

    if (role === "loop") {
      return " is-loop";
    }

    if (role === "merge") {
      return " is-merge";
    }

    if (role === "aggregate") {
      return " is-aggregate";
    }

    if (role === "checkpoint") {
      return " is-checkpoint";
    }

    return "";
  }

  function getFlowchartRoleLabel(component) {
    const role = getFlowchartRole(component);
    if (role === "transform") {
      return "Transform";
    }

    if (role === "parallel") {
      return "Parallel";
    }

    if (role === "foreach") {
      return "For each";
    }

    if (role === "batch") {
      return "Batch";
    }

    if (role === "loop") {
      return "Loop";
    }

    if (role === "ensure") {
      return "Ensure";
    }

    if (role === "merge") {
      return "Merge";
    }

    if (role === "aggregate") {
      return "Aggregate";
    }

    if (role === "checkpoint") {
      return "Checkpoint";
    }

    return getFriendlyKind(component);
  }

  function getFlowchartRole(component) {
    const value = [
      getMetadata(component, "role"),
      getMetadata(component, "operation"),
      getMetadata(component, "fluentCall"),
      getMetadata(component, "kind")
    ].join(" ").toLowerCase();

    if (value.includes("transform")) {
      return "transform";
    }

    if (value.includes("parallel")) {
      return "parallel";
    }

    if (value.includes("foreach") || value.includes("for each")) {
      return "foreach";
    }

    if (value.includes("batch")) {
      return "batch";
    }

    if (value.includes("loop") || value.includes("repeat")) {
      return "loop";
    }

    if (value.includes("ensure")) {
      return "ensure";
    }

    if (value.includes("merge")) {
      return "merge";
    }

    if (value.includes("aggregate")) {
      return "aggregate";
    }

    if (value.includes("checkpoint")) {
      return "checkpoint";
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

  function createRuntimeGraphContext(overview) {
    if (!overview || !overview.rootSpans || !overview.rootSpans.length) {
      return null;
    }

    const spans = flattenRuntimeSpans(overview.rootSpans);
    const selectedRuntimeItem = findRuntimeTimelineItem(overview.rootSpans, state.runtimeSelectedItemKey);
    const process = resolveRuntimeSelectedProcess(selectedRuntimeItem ? selectedRuntimeItem.value : null, spans)
      || resolveRuntimeGraphProcess(spans, overview);
    if (!process) {
      return null;
    }

    const children = orderChildren(process);
    const nodes = collectRuntimeGraphNodes(process, children);
    const nodeStates = new Map(nodes.map((node) => [node.id, "not-executed"]));
    const spanByComponentId = new Map();
    const graphContext = {
      process,
      children,
      nodes,
      nodeStates,
      spanByComponentId,
      selectedRuntimeItem,
      defaultComponentId: process.id,
      faultComponentId: ""
    };

    for (const span of spans) {
      const componentId = resolveRuntimeComponentId(span, graphContext);
      if (componentId) {
        span.resolvedComponentId = componentId;
        spanByComponentId.set(componentId, span);
        applyRuntimeGraphState(graphContext, componentId, span.status);
      }

      for (const marker of span.markers || []) {
        const markerId = resolveRuntimeMarkerComponentId(marker, span, graphContext);
        if (!markerId) {
          continue;
        }

        marker.resolvedComponentId = markerId;
        if (!spanByComponentId.has(markerId)) {
          spanByComponentId.set(markerId, marker);
        }

        applyRuntimeGraphState(graphContext, markerId, marker.status || "Completed");
      }
    }

    if (selectedRuntimeItem) {
      const selectedRuntimeComponentId = resolveRuntimeComponentId(selectedRuntimeItem.value, graphContext);
      if (selectedRuntimeComponentId) {
        selectedRuntimeItem.value.resolvedComponentId = selectedRuntimeComponentId;
        state.runtimeSelectedComponentId = selectedRuntimeComponentId;
      }
    } else if (!state.runtimeSelectedComponentId) {
      state.runtimeSelectedComponentId = graphContext.faultComponentId
        || findFirstExecutedRuntimeNode(graphContext)
        || process.id;
    }

    const selectedStillExists = state.runtimeSelectedComponentId && nodeStates.has(state.runtimeSelectedComponentId);
    graphContext.defaultComponentId = selectedStillExists
      ? state.runtimeSelectedComponentId
      : graphContext.faultComponentId || findFirstExecutedRuntimeNode(graphContext) || process.id;
    return graphContext;
  }

  function resolveRuntimeSelectedProcess(selectedItem, spans) {
    if (selectedItem) {
      const selectedProcess = findRuntimeProcessCandidate(selectedItem);
      if (selectedProcess) {
        return selectedProcess;
      }

      const parentProcess = findRuntimeAncestorProcess(selectedItem, spans);
      if (parentProcess) {
        return parentProcess;
      }
    }

    const selected = state.runtimeSelectedComponentId ? byId.get(state.runtimeSelectedComponentId) : null;
    if (!selected) {
      return null;
    }

    if (selected.kind === "spider.flow" || selected.kind === "spider.pipeline") {
      return selected;
    }

    return findOwningProcess(selected.id);
  }

  function findRuntimeAncestorProcess(item, spans) {
    const bySpanId = new Map((spans || []).map((span) => [span.spanId, span]));
    let current = item && item.parentSpanId
      ? bySpanId.get(item.parentSpanId)
      : item && item.kind ? bySpanId.get(item.spanId) : null;
    const visited = new Set();

    while (current && !visited.has(current.spanId)) {
      visited.add(current.spanId);
      const process = findRuntimeProcessCandidate(current);
      if (process) {
        return process;
      }

      current = current.parentSpanId ? bySpanId.get(current.parentSpanId) : null;
    }

    return null;
  }

  function flattenRuntimeSpans(spans) {
    const flattened = [];
    const visit = (span) => {
      flattened.push(span);
      (span.children || []).forEach(visit);
    };

    (spans || []).forEach(visit);
    return flattened;
  }

  function collectRuntimeGraphNodes(process, children) {
    const nodes = [];
    const visited = new Set();
    const addNode = (node) => {
      if (!node || visited.has(node.id)) {
        return;
      }

      visited.add(node.id);
      nodes.push(node);
      if (node.kind !== "spider.flow-branch") {
        return;
      }

      for (const route of getBranchRoutes(node)) {
        addNode(route);
        for (const step of getRouteSteps(route)) {
          addNode(step);
        }
      }
    };

    addNode(process);
    for (const child of children || []) {
      addNode(child);
    }

    return nodes;
  }

  function resolveRuntimeGraphProcess(spans, overview) {
    const faultSpans = spans
      .filter((span) => normalizeStatus(span.status) === "faulted")
      .sort((left, right) => getRuntimeGraphSpecificity(right) - getRuntimeGraphSpecificity(left));
    const ordered = faultSpans.concat(spans.slice().sort((left, right) => getRuntimeGraphSpecificity(right) - getRuntimeGraphSpecificity(left)));
    for (const span of ordered) {
      const process = findRuntimeProcessCandidate(span);
      if (process) {
        return process;
      }
    }

    const title = normalizeRuntimeName(overview.title);
    return flows.concat(pipelines).find((process) => normalizeRuntimeName(process.displayName) === title)
      || null;
  }

  function getRuntimeGraphSpecificity(item) {
    const kind = String(item && item.componentKind || "");
    if (kind.includes("flow-step") || kind.includes("flow-condition")) {
      return 6;
    }

    if (kind.includes("flow-branch-route")) {
      return 5;
    }

    if (kind.includes("flow-branch")) {
      return 4;
    }

    if (kind === "spider.flow") {
      return 3;
    }

    if (kind.includes("pipeline-stage")) {
      return 2;
    }

    if (kind.includes("pipeline")) {
      return 1;
    }

    return 0;
  }

  function findRuntimeProcessCandidate(item) {
    if (item.componentId && byId.has(item.componentId)) {
      const owner = findOwningProcess(item.componentId) || byId.get(item.componentId);
      if (owner && (owner.kind === "spider.flow" || owner.kind === "spider.pipeline")) {
        return owner;
      }
    }

    const itemKind = String(item.componentKind || "");
    if (itemKind && itemKind !== "spider.flow" && itemKind !== "spider.pipeline") {
      return null;
    }

    const displayName = normalizeRuntimeName(getRuntimeDisplayName(item));
    const rawDisplayName = normalizeRuntimeName(item.displayName);
    const signature = normalizeRuntimeName(getRuntimeSignature(item.inputType, item.outputType));
    const candidates = itemKind === "spider.pipeline"
      ? pipelines
      : itemKind === "spider.flow"
        ? flows
        : flows.concat(pipelines);

    return candidates.find((process) =>
      normalizeRuntimeName(process.displayName) === displayName ||
      normalizeRuntimeName(process.displayName) === rawDisplayName ||
      normalizeRuntimeName(getSignature(process)) === signature) || null;
  }

  function resolveRuntimeComponentId(item, graphContext) {
    if (!item || !graphContext) {
      return "";
    }

    if (item.componentId && graphContext.nodeStates.has(item.componentId)) {
      return item.componentId;
    }

    const itemKind = String(item.componentKind || "");
    const displayName = normalizeRuntimeName(getRuntimeDisplayName(item));
    if (!displayName) {
      return "";
    }

    if ((itemKind === "spider.flow" || itemKind === "spider.pipeline") &&
        normalizeRuntimeName(graphContext.process.displayName) === displayName) {
      return graphContext.process.id;
    }

    const candidates = graphContext.nodes.filter((node) => runtimeKindsMatch(itemKind, node.kind));
    const exact = candidates.find((node) => normalizeRuntimeName(node.displayName) === displayName);
    if (exact) {
      return exact.id;
    }

    const metadataName = normalizeRuntimeName(getRuntimeMetadataValue(item, "name"));
    if (metadataName) {
      const metadataMatch = candidates.find((node) => normalizeRuntimeName(getMetadata(node, "name")) === metadataName);
      if (metadataMatch) {
        return metadataMatch.id;
      }
    }

    return "";
  }

  function resolveRuntimeMarkerComponentId(marker, span, graphContext) {
    const markerKind = String(marker && marker.componentKind || "");
    if (markerKind === "spider.flow-branch-route") {
      const branchId = span && span.resolvedComponentId
        ? span.resolvedComponentId
        : resolveRuntimeComponentId(span, graphContext);
      const branch = branchId ? byId.get(branchId) : null;
      if (branch && branch.kind === "spider.flow-branch") {
        const markerNames = [
          getRuntimeDisplayName(marker),
          marker && marker.displayName,
          getRuntimeMetadataValue(marker, "name"),
          getRuntimeMetadataValue(marker, "route")
        ].map(normalizeRuntimeName).filter(Boolean);
        const route = getBranchRoutes(branch).find((candidate) => {
          const candidateNames = [
            candidate.displayName,
            getMetadata(candidate, "name"),
            getMetadata(candidate, "route")
          ].map(normalizeRuntimeName).filter(Boolean);
          return candidateNames.some((candidateName) => markerNames.includes(candidateName));
        });

        if (route) {
          return route.id;
        }
      }
    }

    return resolveRuntimeComponentId(marker, graphContext);
  }

  function runtimeKindsMatch(runtimeKind, graphKind) {
    if (!runtimeKind) {
      return true;
    }

    if (runtimeKind === graphKind) {
      return true;
    }

    if (runtimeKind === "spider.flow-step") {
      return graphKind === "spider.flow-step";
    }

    if (runtimeKind === "spider.flow-condition") {
      return graphKind === "spider.flow-condition" || graphKind === "spider.flow-step";
    }

    if (runtimeKind === "spider.flow-branch") {
      return graphKind === "spider.flow-branch";
    }

    if (runtimeKind === "spider.flow-branch-route") {
      return graphKind === "spider.flow-branch-route";
    }

    if (runtimeKind === "spider.pipeline-stage") {
      return graphKind === "spider.pipeline-stage";
    }

    return false;
  }

  function applyRuntimeGraphState(graphContext, componentId, status) {
    const current = graphContext.nodeStates.get(componentId) || "not-executed";
    const next = normalizeStatus(status);
    const resolved = next === "faulted"
      ? "faulted"
      : next === "cancelled"
        ? "cancelled"
        : next === "running" || next === "started"
          ? "running"
          : "completed";

    graphContext.nodeStates.set(componentId, mostSevereRuntimeState(current, resolved));
    if (resolved === "faulted" && !graphContext.faultComponentId) {
      graphContext.faultComponentId = componentId;
    }
  }

  function mostSevereRuntimeState(current, next) {
    const order = {
      "not-executed": 0,
      completed: 1,
      running: 2,
      cancelled: 3,
      faulted: 4
    };

    return (order[next] || 0) >= (order[current] || 0) ? next : current;
  }

  function findFirstExecutedRuntimeNode(graphContext) {
    for (const node of graphContext.nodes) {
      if ((graphContext.nodeStates.get(node.id) || "not-executed") !== "not-executed") {
        return node.id;
      }
    }

    return "";
  }

  function getRuntimeGraphClass(componentId, graphContext) {
    if (!graphContext || !componentId) {
      return "";
    }

    const status = graphContext.nodeStates.get(componentId) || "not-executed";
    return ` is-runtime-${status}`;
  }

  function getRuntimeEdgeClass(fromId, toId, graphContext) {
    if (!graphContext) {
      return "";
    }

    const fromState = graphContext.nodeStates.get(fromId) || "not-executed";
    const toState = graphContext.nodeStates.get(toId) || "not-executed";
    if (toState === "faulted") {
      return " is-runtime-faulted";
    }

    if (fromState !== "not-executed" && toState !== "not-executed") {
      return " is-runtime-completed";
    }

    return " is-runtime-not-executed";
  }

  function formatRuntimeGraphState(stateValue) {
    const value = normalizeStatus(stateValue);
    if (value === "not-executed") {
      return "Not run";
    }

    return value.charAt(0).toUpperCase() + value.slice(1);
  }

  function normalizeRuntimeName(value) {
    return String(value || "")
      .toLowerCase()
      .replace(/[^a-z0-9]+/g, "")
      .trim();
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
    if (value === "not-executed") {
      return "not-executed";
    }

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

      runtimeData = mergeImportedRuntimeData(await response.json());
      updateRuntimeCount();

      if (state.view === "runtime" && state.mode === "list") {
        refreshRuntimeListRows();
      } else if (state.view === "runtime" && state.mode === "trace") {
        const hash = decodeURIComponent(window.location.hash.replace(/^#\/?/, ""));
        const traceId = hash.startsWith("runtime:") ? hash.substring("runtime:".length) : "";
        if (state.rawEventsModalOpen) {
          const trace = traceId
            ? (runtimeData.traces || []).find((item) => item.traceId === traceId) || null
            : null;
          refreshRuntimeRawEventsModal(trace);
          return;
        }

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

  function setTopbarBackLabel(value) {
    if (topbarBack) {
      const label = value || "Back";
      topbarBack.textContent = label;
      topbarBack.setAttribute("aria-label", label);
      topbarBack.setAttribute("title", label);
    }
  }

  function setRuntimeTraceShell(active) {
    root.classList.toggle("is-runtime-trace", Boolean(active));
  }

  function setRuntimeListShell(active) {
    root.classList.toggle("is-runtime-list", Boolean(active));
  }

  function setProcessDetailShell(active) {
    root.classList.toggle("is-process-detail", Boolean(active));
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
      themeToggleLabel.textContent = isDark ? "Light" : "Dark";
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

  function preserveRuntimeVisualScroll(action) {
    const visual = root.querySelector(".spider-runtime-timeline.is-main, .spider-runtime-graph-wrap");
    const top = visual ? visual.scrollTop : 0;
    const left = visual ? visual.scrollLeft : 0;

    preserveMainScroll(action);

    const nextVisual = root.querySelector(".spider-runtime-timeline.is-main, .spider-runtime-graph-wrap");
    if (nextVisual) {
      nextVisual.scrollTop = top;
      nextVisual.scrollLeft = left;
    }
  }

  function preserveProcessGraphScroll(action) {
    const graph = root.querySelector(".spider-process-graph");
    const top = graph ? graph.scrollTop : 0;
    const left = graph ? graph.scrollLeft : 0;

    preserveMainScroll(action);

    const nextGraph = root.querySelector(".spider-process-graph");
    if (nextGraph) {
      nextGraph.scrollTop = top;
      nextGraph.scrollLeft = left;
    }
  }

  function getProcessGraphMaximizeIconPath(restoring) {
    return restoring
      ? "M4 4l6 6M10 10H4M10 10V4M20 4l-6 6M14 10h6M14 10V4M4 20l6-6M10 14H4M10 14v6M20 20l-6-6M14 14h6M14 14v6"
      : "M10 10L4 4M4 4h6M4 4v6M14 10l6-6M20 4h-6M20 4v6M10 14l-6 6M4 20h6M4 20v-6M14 14l6 6M20 20h-6M20 20v-6";
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
