/**
 * Static site generator for Taver. Reads Data/site-data.json and outputs HTML to docs/.
 * Run from repo root: node static-site/generate.js
 * GitHub Pages will serve from docs/ (or configure workflow to deploy docs/).
 */

const fs = require('fs');
const path = require('path');

const REPO_ROOT = path.resolve(__dirname, '..');
const DATA_PATH = path.join(REPO_ROOT, 'Data', 'site-data.json');
const OUTPUT_DIR = path.join(REPO_ROOT, 'docs');
const PAGE_SIZE = 12;

function escapeHtml(s) {
  if (s == null) return '';
  return String(s)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');
}

function layout(title, metaDescription, body, base = '') {
  const desc = escapeHtml(metaDescription || 'Artist portfolio — drawings and illustrations.');
  const t = escapeHtml(title);
  return `<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <meta name="description" content="${desc}" />
  <title>${t} - Taver</title>
  <link rel="preconnect" href="https://fonts.googleapis.com" />
  <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
  <link href="https://fonts.googleapis.com/css2?family=Playfair+Display:wght@400;600;700&family=Poppins:wght@300;400;500;600&display=swap" rel="stylesheet" />
  <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" crossorigin="anonymous" />
  <link rel="stylesheet" href="${base}css/site.css" />
  <script>
    (function () {
      var stored = localStorage.getItem('theme');
      var prefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
      var theme = stored === 'dark' || stored === 'light' ? stored : (prefersDark ? 'dark' : 'light');
      document.documentElement.setAttribute('data-theme', theme);
    })();
  </script>
</head>
<body>
  <a class="skip-link" href="#main-content">Skip to main content</a>
  <header>
    <nav class="navbar navbar-expand-sm navbar-toggleable-sm navbar-light border-bottom box-shadow mb-3 site-nav">
      <div class="container-fluid">
        <a class="navbar-brand site-brand" href="${base}index.html">Taver</a>
        <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target=".navbar-collapse" aria-controls="navbarSupportedContent" aria-expanded="false" aria-label="Toggle navigation">
          <span class="navbar-toggler-icon"></span>
        </button>
        <div class="navbar-collapse collapse d-sm-inline-flex justify-content-between align-items-center">
          <ul class="navbar-nav flex-grow-1">
            <li class="nav-item"><a class="nav-link" href="${base}index.html">Home</a></li>
            <li class="nav-item"><a class="nav-link" href="${base}works/index.html">Works</a></li>
            <li class="nav-item"><a class="nav-link" href="${base}about.html">About</a></li>
          </ul>
          <button type="button" id="theme-toggle" class="navbar-nav align-self-center" aria-label="Toggle dark mode" title="Toggle dark mode">
            <span class="icon-light" aria-hidden="true"><svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5"><path stroke-linecap="round" stroke-linejoin="round" d="M12 3v2.25m6.364.386l-1.591 1.591M21 12h-2.25m-.386 6.364l-1.591-1.591M12 18.75V21m-4.773-4.227l-1.591 1.591M5.25 12H3m4.227-4.773L5.636 5.636M15.75 12a3.75 3.75 0 11-7.5 0 3.75 3.75 0 017.5 0z" /></svg></span>
            <span class="icon-dark" aria-hidden="true"><svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5"><path stroke-linecap="round" stroke-linejoin="round" d="M21.752 15.002A9.718 9.718 0 0118 15.75c-5.385 0-9.75-4.365-9.75-9.75 0-1.33.266-2.597.748-3.752A9.753 9.753 0 003 11.25C3 16.635 7.365 21 12.75 21a9.753 9.753 0 009.002-5.998z" /></svg></span>
          </button>
        </div>
      </div>
    </nav>
  </header>
  <div class="container">
    <main id="main-content" role="main" class="pb-3">
${body}
    </main>
  </div>
  <footer class="border-top footer text-theme-muted">
    <div class="container">
      <a href="https://instagram.com" target="_blank" rel="noopener">Instagram</a> · <a href="mailto:artist@example.com">Email</a> · &copy; ${new Date().getFullYear()} Taver
    </div>
  </footer>
  <script src="https://code.jquery.com/jquery-3.7.1.min.js" crossorigin="anonymous"></script>
  <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js" crossorigin="anonymous"></script>
  <script src="${base}js/site.js"></script>
</body>
</html>`;
}

function ensureDir(dir) {
  if (!fs.existsSync(dir)) fs.mkdirSync(dir, { recursive: true });
}

// Load data
let data;
try {
  data = JSON.parse(fs.readFileSync(DATA_PATH, 'utf8'));
} catch (e) {
  console.error('Could not read', DATA_PATH, e.message);
  process.exit(1);
}

const artist = data.artist || { name: 'Artist', bio: 'Welcome to my portfolio.', profileImage: null };
const rawArtworks = Array.isArray(data.artworks) ? data.artworks : [];
const artworks = rawArtworks.map((a, i) => ({
  id: i + 1,
  title: a.title || '',
  description: a.description || '',
  imagePath: a.imagePath || '',
  category: a.category || '',
  year: a.year || null,
  artistName: artist.name
}));

ensureDir(OUTPUT_DIR);
ensureDir(path.join(OUTPUT_DIR, 'css'));
ensureDir(path.join(OUTPUT_DIR, 'js'));
ensureDir(path.join(OUTPUT_DIR, 'images'));
ensureDir(path.join(OUTPUT_DIR, 'works'));
ensureDir(path.join(OUTPUT_DIR, 'work'));

// Copy static assets
const wwwroot = path.join(REPO_ROOT, 'wwwroot');
if (fs.existsSync(path.join(wwwroot, 'css', 'site.css'))) {
  fs.copyFileSync(path.join(wwwroot, 'css', 'site.css'), path.join(OUTPUT_DIR, 'css', 'site.css'));
}
if (fs.existsSync(path.join(wwwroot, 'js', 'site.js'))) {
  fs.copyFileSync(path.join(wwwroot, 'js', 'site.js'), path.join(OUTPUT_DIR, 'js', 'site.js'));
}
const imagesDir = path.join(wwwroot, 'images');
if (fs.existsSync(imagesDir)) {
  const copyDir = (src, dest) => {
    ensureDir(dest);
    for (const name of fs.readdirSync(src)) {
      const s = path.join(src, name);
      const d = path.join(dest, name);
      if (fs.statSync(s).isDirectory()) copyDir(s, d);
      else fs.copyFileSync(s, d);
    }
  };
  copyDir(imagesDir, path.join(OUTPUT_DIR, 'images'));
}

const featured = artworks.slice(0, 6);
const bioShort = artist.bio && artist.bio.length > 200 ? artist.bio.substring(0, 200) + '…' : (artist.bio || 'Welcome to my portfolio.');
const name = artist.name || 'Artist';

// index.html
const indexBody = `
<section class="hero-section py-5 mb-5 text-center">
  <div class="container">
    <h1 class="display-3 fw-bold mb-3">${escapeHtml(name)}</h1>
    <p class="lead mb-4">Drawings &amp; illustrations</p>
    <a href="works/index.html" class="btn btn-danger btn-lg">View My Works</a>
  </div>
</section>
<section class="py-5">
  <div class="container">
    <h2 class="text-center mb-4">Featured Artworks</h2>
    ${featured.length ? `<div class="row g-4">
      ${featured.map((item) => `
        <div class="col-md-6 col-lg-4">
          <div class="card h-100 border-0 shadow-sm">
            <a href="work/${item.id}.html">
              <img src="${escapeHtml(item.imagePath)}" class="card-img-top" alt="${escapeHtml(item.title)}" style="height: 280px; object-fit: cover;" loading="lazy" />
            </a>
            <div class="card-body">
              <h5 class="card-title">${escapeHtml(item.title)}</h5>
              <a href="work/${item.id}.html" class="btn btn-outline-dark btn-sm">View Details</a>
            </div>
          </div>
        </div>`).join('')}
    </div>` : '<p class="text-center text-muted">No artworks yet. Check back soon.</p>'}
  </div>
</section>
<section class="py-5 section-surface">
  <div class="container">
    <div class="row align-items-center">
      <div class="col-lg-8">
        <h2 class="mb-3">About Me</h2>
        <p class="lead">${escapeHtml(bioShort)}</p>
        <a href="about.html" class="btn btn-outline-dark">Read more</a>
      </div>
    </div>
  </div>
</section>`;

fs.writeFileSync(path.join(OUTPUT_DIR, 'index.html'), layout('Home', null, indexBody, ''));

// about.html
const bioContent = artist.bio ? artist.bio.replace(/\n/g, '<br />') : '<p>Welcome to my portfolio.</p>';
const aboutProfile = artist.profileImage
  ? `<div class="col-md-4 mb-4 mb-md-0"><img src="${escapeHtml(artist.profileImage)}" alt="${escapeHtml(name)}" class="img-fluid rounded shadow" loading="lazy" /></div><div class="col-md-8">`
  : '<div class="col-12">';
const aboutBody = `
<div class="row align-items-center py-5">
  ${aboutProfile}
  <h1 class="mb-4">${escapeHtml(name)}</h1>
  <div class="bio-content">${bioContent}</div>
  <hr class="my-4" />
  <p class="text-muted">Contact: <a href="mailto:artist@example.com">artist@example.com</a> · <a href="https://instagram.com" target="_blank" rel="noopener">Instagram</a></p>
  </div>
</div>`;

fs.writeFileSync(path.join(OUTPUT_DIR, 'about.html'), layout('About', null, aboutBody, ''));

// works/index.html (all on one page for simplicity; optional pagination later)
const worksBody = `
<h2 class="text-center mb-5">My Works</h2>
${artworks.length ? `<div class="row g-4">
  ${artworks.map((item) => `
    <div class="col-md-6 col-lg-4">
      <div class="card h-100 border-0 shadow-sm">
        <a href="../work/${item.id}.html"><img src="${escapeHtml(item.imagePath).startsWith('http') ? item.imagePath : '../' + escapeHtml(item.imagePath)}" class="card-img-top" alt="${escapeHtml(item.title)}" style="height: 280px; object-fit: cover;" loading="lazy" /></a>
        <div class="card-body d-flex flex-column">
          <h5 class="card-title">${escapeHtml(item.title)}</h5>
          <a href="../work/${item.id}.html" class="btn btn-outline-dark mt-auto">View Details</a>
        </div>
      </div>
    </div>`).join('')}
</div>` : '<p class="text-center text-muted">No artworks yet.</p>'}
`;

fs.writeFileSync(path.join(OUTPUT_DIR, 'works', 'index.html'), layout('My Works', null, worksBody, '../'));

// work/1.html, work/2.html, ...
for (const item of artworks) {
  const metaDesc = item.description ? (item.description.length > 160 ? item.description.substring(0, 157) + '…' : item.description) : item.title;
  const imgSrc = item.imagePath.startsWith('http') ? item.imagePath : '../' + item.imagePath;
  const detailBody = `
<div class="row py-5">
  <div class="col-lg-7 mb-4 mb-lg-0">
    <img src="${escapeHtml(imgSrc)}" class="img-fluid rounded shadow" alt="${escapeHtml(item.title)}" loading="lazy" />
  </div>
  <div class="col-lg-5">
    <h1 class="mb-3">${escapeHtml(item.title)}</h1>
    ${item.description ? `<p class="lead">${escapeHtml(item.description)}</p>` : ''}
    <dl class="row">
      ${item.year != null ? `<dt class="col-sm-4">Year</dt><dd class="col-sm-8">${escapeHtml(String(item.year))}</dd>` : ''}
      ${item.category ? `<dt class="col-sm-4">Category</dt><dd class="col-sm-8">${escapeHtml(item.category)}</dd>` : ''}
      <dt class="col-sm-4">Artist</dt>
      <dd class="col-sm-8">${escapeHtml(item.artistName)}</dd>
    </dl>
    <a href="../works/index.html" class="btn btn-outline-dark">Back to gallery</a>
  </div>
</div>`;
  fs.writeFileSync(path.join(OUTPUT_DIR, 'work', `${item.id}.html`), layout(item.title, metaDesc, detailBody, '../'));
}

// Custom domain for GitHub Pages: if set, write CNAME so the site is served at your domain
const customDomain = (data.customDomain && String(data.customDomain).trim()) || null;
if (customDomain) {
  fs.writeFileSync(path.join(OUTPUT_DIR, 'CNAME'), customDomain.trim(), 'utf8');
  console.log('  CNAME:', customDomain);
}

console.log('Static site generated in docs/');
console.log('  index.html, about.html, works/index.html, work/*.html');
console.log('  css/, js/, images/');
