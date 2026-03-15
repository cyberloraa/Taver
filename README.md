# Taver

Artist portfolio site — static HTML for [GitHub Pages](https://pages.github.com/).

## Edit content

Edit **`Data/site-data.json`**:

- **Artist:** `name`, `bio`, `profileImage` (path like `images/artist.jpg` or `null`).
- **Artworks:** add objects with `title`, `description`, `imagePath`, `category`, `year` (optional).

Put images in **`wwwroot/images/`** (e.g. `wwwroot/images/artworks/`) and set `imagePath` to the path from site root (e.g. `images/artworks/mywork.jpg`).

## Build static site locally

From the repo root (Node.js required):

```bash
node static-site/generate.js
```

Output is in **`docs/`**. Open `docs/index.html` in a browser or serve the `docs` folder locally.

## Deploy to GitHub Pages

1. Push the repo to GitHub.
2. In the repo go to **Settings → Pages**.
3. Under **Build and deployment**, set **Source** to **GitHub Actions** (not “Deploy from a branch”).
4. Push to `main` (or run the workflow manually from the Actions tab). The **Deploy to GitHub Pages** workflow will:
   - Run `node static-site/generate.js`
   - Deploy the `docs/` output to GitHub Pages.

Your site will be at `https://<username>.github.io/<repo>/` (or your custom domain if configured).

## Use your own domain

1. **Set your domain in the project**  
   In **`Data/site-data.json`**, set `"customDomain"` to your domain (e.g. `"www.mysite.com"` or `"mysite.com"`). Leave it as `""` if you don’t use a custom domain.

2. **Tell GitHub**  
   On GitHub: repo **Settings → Pages**. Under **Custom domain**, type the same domain and save.

3. **Point your domain here**  
   At your domain registrar (where you bought the domain), add a **CNAME** record: name `www` (or your subdomain), value `YOUR_USERNAME.github.io`. For a bare domain like `mysite.com`, use an **A** record to `185.199.108.153` (see [GitHub’s docs](https://docs.github.com/en/pages/configuring-a-custom-domain-for-your-github-pages-site)).

Then push your changes and redeploy. After DNS updates, turn on **Enforce HTTPS** in Settings → Pages.
