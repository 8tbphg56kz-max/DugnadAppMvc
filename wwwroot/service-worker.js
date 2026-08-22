const CACHE_NAME = "dugnadapp-v1";

self.addEventListener("install", event => {
    self.skipWaiting();
});

self.addEventListener("activate", event => {
    event.waitUntil(self.clients.claim());
});

self.addEventListener("fetch", event => {
    // La nettleseren håndtere alle forespørsler normalt.
    // Vi bruker foreløpig ikke offline-cache.
});