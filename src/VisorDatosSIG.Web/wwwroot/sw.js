// Service Worker para VisorDatosSIG PWA (Caché offline de recursos estáticos)
const CACHE_NAME = 'visordatos-sig-v2';
const STATIC_ASSETS = [
  '/favicon.svg',
  '/logo.svg',
  '/manifest.json',
  '/css/site.css',
  'https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css',
  'https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css',
  'https://unpkg.com/leaflet@1.9.4/dist/leaflet.css',
  'https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'
];

self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(CACHE_NAME).then((cache) => {
      return cache.addAll(STATIC_ASSETS);
    }).then(() => self.skipWaiting())
  );
});

self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys().then((keys) => {
      return Promise.all(
        keys.filter((key) => key !== CACHE_NAME).map((key) => caches.delete(key))
      );
    }).then(() => self.clients.claim())
  );
});

self.addEventListener('fetch', (event) => {
  // Solo aplicar caché a solicitudes GET de recursos estáticos puros
  if (event.request.method !== 'GET') return;

  // Las solicitudes de navegación (páginas HTML dinámicas) siempre van directo a la red para preservar la sesión y claims del usuario
  if (event.request.mode === 'navigate') {
    return;
  }

  const url = new URL(event.request.url);

  // No almacenar en caché llamadas a la API de datos espaciales ni autenticación ni controladores dinámicos
  if (url.pathname.startsWith('/api/') || 
      url.pathname.startsWith('/Account/') || 
      url.pathname.startsWith('/Home/') || 
      url.pathname.startsWith('/Mapa') || 
      url.pathname.startsWith('/Consultas') || 
      url.pathname.startsWith('/Admin/')) {
    return;
  }

  event.respondWith(
    caches.match(event.request).then((cachedResponse) => {
      return cachedResponse || fetch(event.request);
    })
  );
});
