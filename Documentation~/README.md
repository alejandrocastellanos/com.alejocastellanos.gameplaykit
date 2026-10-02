# Documentation~

Source of the Gameplay Kit documentation site (MkDocs Material, English + Spanish), published to
GitHub Pages by `.github/workflows/docs.yml`. Unity ignores folders ending in `~`, so nothing here is
imported into projects that use the package.

## Layout

| Path | What it is |
|---|---|
| `docs/en/`, `docs/es/` | Hand-written pages (home, guides, reference) per language |
| `docs/assets/` | Shared assets: CSS, logo and `clips/` (one `<Component>.mp4` + `.jpg` poster per component) |
| `content/components/<category>/<Component>.yml` | Hand-written, bilingual content of each component page |
| `content/categories.yml`, `content/components_index.*.md` | Category intros and the Components landing page |
| `data/components.json` | Reference extracted from Unity by reflection (fields, defaults, events, API) |
| `tools/generate.py` | Builds `docs/<lang>/components/**` and `mkdocs.yml` from the above |
| `tools/validate_content.py` | Checks component YAML against `components.json` |
| `capture/` | Unity code that records the clips (see below) |

`mkdocs.yml` and `docs/*/components/` are generated (git-ignored).

## Preview locally

```bash
cd Documentation~
pip install -r requirements.txt
python tools/generate.py
mkdocs serve        # http://127.0.0.1:8000
```

## Publishing

Push to `main`/`master`, then in the GitHub repository go to **Settings → Pages → Build and deployment →
Source: GitHub Actions** (once). Every push that touches `Documentation~/` rebuilds the site.

## After changing the package code

1. **Reference:** copy `capture/` into a Unity project that uses the package (e.g. `Assets/DocCapture/`),
   run **Tools → GameplayKit Docs → Export Component Reference** and copy `DocCaptures/components.json`
   to `data/components.json`.
2. **Content:** edit the component's YAML (new fields need a description in both languages) and run
   `python tools/validate_content.py content/components/*/*.yml`.
3. **Clips:** the capture scenarios are explicit play-mode tests (`capture/Scenarios/*.cs`, category
   `DocCapture`). Add the project's `testables` entry for the package, run them from the Test Runner
   (they're `[Explicit]`, so select them explicitly), then encode the frames:
   `capture/encode.sh <project>/DocCaptures/frames docs/assets/clips` (needs ffmpeg).
