// The published demo's service worker. The .NET runtime, the assemblies and the static assets are cached on the
// first visit, so a repeat visit starts without downloading .NET again and the demo keeps working offline.
// Pages always come from the network first: they are prerendered per route and change with every release.
self.importScripts("./service-worker-assets.js");

const cacheName = `demo-${self.assetsManifest.version}`;
// The manifest also lists index.html, which publishing for Pages rewrites; only unmodified assets are precached.
const cached = /^_(framework|content)\//;
const shell = new URL("app.html", self.registration.scope).href;

self.addEventListener("install", event => event.waitUntil(install()));
self.addEventListener("activate", event => event.waitUntil(activate()));
self.addEventListener("fetch", event => {
    if (event.request.method === "GET" && event.request.url.startsWith(self.registration.scope)) event.respondWith(respond(event.request));
});

async function install() {
    const cache = await caches.open(cacheName);
    const assets = self.assetsManifest.assets.filter(asset => cached.test(asset.url));
    // Integrity hashes from the build reject a corrupted or mismatched download.
    await cache.addAll(assets.map(asset => new Request(asset.url, { integrity: asset.hash, cache: "no-cache" })));
    await cache.add(new Request(shell, { cache: "no-cache" }));
    await self.skipWaiting();
}

async function activate() {
    for (const key of await caches.keys()) {
        if (key.startsWith("demo-") && key !== cacheName) await caches.delete(key);
    }
    await self.clients.claim();
}

async function respond(request) {
    if (request.mode === "navigate") {
        try {
            return await fetch(request);
        } catch {
            // Offline: the unrendered shell starts the app from the cache, and its router shows the requested page.
            return (await caches.match(shell)) ?? Response.error();
        }
    }
    return (await caches.match(request)) ?? fetch(request);
}
