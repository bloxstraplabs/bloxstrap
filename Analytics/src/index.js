// Config

const ALLOWED_STATS = {
    installAction: ["install", "uninstall", "upgrade"],
    packageDownloadState: ["httpFail"],
    robloxChannel: null
};

const VALUE_PATTERN = /^[a-z0-9_\-]{1,32}$/i;
const MAX_LOG_LENGTH = 200_000;
const EXCEPTION_RETENTION_DAYS = 90;

// Helpers

function getVersion(request) {
    const match = (request.headers.get("User-Agent") || "").match(/reistrap(?:-QA)?\/([0-9.]{1,20})/i);
    return match ? match[1] : "unknown";
}

function today() {
    return new Date().toISOString().slice(0, 10);
}

function escapeHtml(text) {
    return String(text).replace(/[&<>"']/g, c => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", "\"": "&quot;", "'": "&#39;" })[c]);
}

function isAuthorized(url, env) {
    const token = url.searchParams.get("token") || "";
    const expected = env.DASHBOARD_TOKEN || "";

    if (!expected || token.length !== expected.length)
        return false;

    let diff = 0;
    for (let i = 0; i < token.length; i++)
        diff |= token.charCodeAt(i) ^ expected.charCodeAt(i);

    return diff === 0;
}

function html(body, status = 200) {
    return new Response(body, { status, headers: { "Content-Type": "text/html; charset=utf-8", "Cache-Control": "no-store" } });
}

// Collection

async function postStat(request, url, env) {
    const key = url.searchParams.get("key") || "";
    const value = url.searchParams.get("value") || "";

    if (!(key in ALLOWED_STATS) || !VALUE_PATTERN.test(value))
        return new Response("Bad request", { status: 400 });

    const allowedValues = ALLOWED_STATS[key];

    if (allowedValues && !allowedValues.includes(value))
        return new Response("Bad request", { status: 400 });

    await env.DB.prepare(
        "INSERT INTO stats (day, key, value, version, count) VALUES (?1, ?2, ?3, ?4, 1) " +
        "ON CONFLICT (day, key, value, version) DO UPDATE SET count = count + 1"
    ).bind(today(), key, value.toLowerCase(), getVersion(request)).run();

    return new Response("OK");
}

async function postException(request, env) {
    const log = (await request.text()).slice(0, MAX_LOG_LENGTH);

    if (!log.trim())
        return new Response("Bad request", { status: 400 });

    await env.DB.prepare("INSERT INTO exceptions (time, version, log) VALUES (?1, ?2, ?3)")
        .bind(new Date().toISOString(), getVersion(request), log)
        .run();

    return new Response("OK");
}

// Dashboard

async function dashboard(url, env) {
    const token = encodeURIComponent(url.searchParams.get("token"));

    const totals = await env.DB.prepare(
        "SELECT key, value, SUM(count) AS total FROM stats GROUP BY key, value ORDER BY key, total DESC"
    ).all();

    const recent = await env.DB.prepare(
        "SELECT day, key, value, SUM(count) AS total FROM stats WHERE day >= date('now', '-30 day') GROUP BY day, key, value ORDER BY day DESC, key, value"
    ).all();

    const versions = await env.DB.prepare(
        "SELECT version, SUM(count) AS total FROM stats WHERE key = 'installAction' GROUP BY version ORDER BY total DESC"
    ).all();

    const exceptions = await env.DB.prepare(
        "SELECT id, time, version, length(log) AS size FROM exceptions ORDER BY id DESC LIMIT 50"
    ).all();

    const table = (headers, rows) =>
        `<table><tr>${headers.map(h => `<th>${h}</th>`).join("")}</tr>${rows.join("") || `<tr><td colspan="${headers.length}" class="empty">Nothing yet</td></tr>`}</table>`;

    const body = `<!doctype html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<meta name="robots" content="noindex">
<title>reistrap analytics</title>
<style>
:root { color-scheme: light dark; --bg: #f7f7f5; --card: #ffffff; --text: #1d1d1b; --muted: #6b6b66; --line: #e4e4df; --accent: #e0b400; }
@media (prefers-color-scheme: dark) { :root { --bg: #161615; --card: #1f1f1d; --text: #ededea; --muted: #9a9a94; --line: #2e2e2b; --accent: #ffd21f; } }
* { box-sizing: border-box; }
body { margin: 0; padding: 24px 16px 48px; background: var(--bg); color: var(--text); font: 15px/1.5 system-ui, -apple-system, "Segoe UI", sans-serif; }
main { max-width: 960px; margin: 0 auto; }
h1 { font-size: 24px; margin: 0 0 4px; }
p.sub { margin: 0 0 24px; color: var(--muted); }
section { background: var(--card); border: 1px solid var(--line); border-radius: 12px; padding: 16px; margin-bottom: 16px; overflow-x: auto; }
h2 { font-size: 16px; margin: 0 0 12px; }
table { width: 100%; border-collapse: collapse; font-variant-numeric: tabular-nums; }
th, td { text-align: left; padding: 6px 8px; border-bottom: 1px solid var(--line); white-space: nowrap; }
th { color: var(--muted); font-weight: 500; font-size: 13px; }
td.num { text-align: right; }
td.empty { color: var(--muted); }
a { color: inherit; }
</style>
</head>
<body>
<main>
<h1>reistrap analytics</h1>
<p class="sub">Anonymous install, update and crash reports sent by reistrap.</p>
<section><h2>All time</h2>${table(["Event", "Value", "Count"], totals.results.map(r => `<tr><td>${escapeHtml(r.key)}</td><td>${escapeHtml(r.value)}</td><td class="num">${r.total}</td></tr>`))}</section>
<section><h2>Installs and updates by version</h2>${table(["Version", "Count"], versions.results.map(r => `<tr><td>${escapeHtml(r.version)}</td><td class="num">${r.total}</td></tr>`))}</section>
<section><h2>Last 30 days</h2>${table(["Day", "Event", "Value", "Count"], recent.results.map(r => `<tr><td>${escapeHtml(r.day)}</td><td>${escapeHtml(r.key)}</td><td>${escapeHtml(r.value)}</td><td class="num">${r.total}</td></tr>`))}</section>
<section><h2>Latest crash reports</h2>${table(["Time (UTC)", "Version", "Size", ""], exceptions.results.map(r => `<tr><td>${escapeHtml(r.time.replace("T", " ").slice(0, 19))}</td><td>${escapeHtml(r.version)}</td><td class="num">${Math.ceil(r.size / 1024)} KB</td><td><a href="/exception/${r.id}?token=${token}">View</a></td></tr>`))}</section>
</main>
</body>
</html>`;

    return html(body);
}

async function viewException(id, env) {
    const row = await env.DB.prepare("SELECT log FROM exceptions WHERE id = ?1").bind(id).first();

    if (!row)
        return new Response("Not found", { status: 404 });

    return new Response(row.log, { headers: { "Content-Type": "text/plain; charset=utf-8", "Cache-Control": "no-store" } });
}

// Entry

export default {
    async fetch(request, env) {
        const url = new URL(request.url);

        if (request.method === "GET" && url.pathname === "/metrics/post")
            return postStat(request, url, env);

        if (request.method === "POST" && url.pathname === "/metrics/post-exception")
            return postException(request, env);

        if (request.method === "GET" && (url.pathname === "/" || url.pathname.startsWith("/exception/"))) {
            if (!isAuthorized(url, env))
                return new Response("Not found", { status: 404 });

            const match = url.pathname.match(/^\/exception\/([0-9]+)$/);

            return match ? viewException(Number(match[1]), env) : dashboard(url, env);
        }

        return new Response("Not found", { status: 404 });
    },

    async scheduled(event, env) {
        await env.DB.prepare("DELETE FROM exceptions WHERE time < ?1")
            .bind(new Date(Date.now() - EXCEPTION_RETENTION_DAYS * 86_400_000).toISOString())
            .run();
    }
};
