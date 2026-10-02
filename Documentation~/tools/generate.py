#!/usr/bin/env python3
"""Genera las páginas de componentes (en/es) y el mkdocs.yml del sitio de documentación.

Fuentes:
  data/components.json          referencia extraída de Unity por reflexión (campos, defaults, API)
  content/components/<cat>/*.yml contenido escrito a mano, bilingüe (descripción, uso, ejemplo...)
  tools/mkdocs.template.yml     configuración base; el generador le agrega el nav

Uso (desde Documentation~):  python tools/generate.py && mkdocs serve
"""
from __future__ import annotations

import json
import re
import shutil
import sys
from pathlib import Path

import yaml

ROOT = Path(__file__).resolve().parent.parent
DOCS = ROOT / "docs"
CONTENT = ROOT / "content" / "components"
CLIPS = DOCS / "assets" / "clips"
LANGS = ("en", "es")

CATEGORIES = [
    # slug, namespace, en, es, icon
    ("core", "GameplayKit.Core", "Core", "Núcleo", "material/cog"),
    ("movement", "GameplayKit.Movement", "Movement", "Movimiento", "material/run-fast"),
    ("health", "GameplayKit.Health", "Health & Physics", "Vida y física", "material/heart-pulse"),
    ("combat", "GameplayKit.Combat", "Combat", "Combate", "material/sword"),
    ("ai", "GameplayKit.AI", "AI & Enemies", "IA y enemigos", "material/robot"),
    ("environment", "GameplayKit.Environment", "Environment", "Entorno", "material/terrain"),
    ("camera", "GameplayKit.CameraSystem", "Camera", "Cámara", "material/video"),
    ("managers", "GameplayKit.Managers", "Managers", "Managers", "material/tune"),
    ("ui", "GameplayKit.UI", "UI", "UI", "material/monitor-dashboard"),
]
CAT_BY_NS = {c[1]: c for c in CATEGORIES}

T = {
    "en": {
        "overview": "Overview", "setup": "Setup", "inspector": "Inspector", "events": "Events",
        "api": "Scripting API", "example": "Example", "tips": "Tips", "related": "See also",
        "field": "Field", "type": "Type", "default": "Default", "description": "Description",
        "member": "Member", "namespace": "Namespace", "base": "Base class", "requires": "Requires",
        "implements": "Implements", "abstract": "Abstract base class — extend it, don't add it directly.",
        "addcomp": "Add it from **Add Component** by searching for", "noclip": "This component has no visual behaviour of its own, so it has no clip.",
        "unityevent": "UnityEvent (assign listeners in the Inspector)", "csevent": "C# event",
        "property": "Property", "method": "Method", "static": "static", "inherited": "Fields inherited from",
        "components": "Components", "catalog_intro": "Every component in this category. Click a card to see what it does, how to set it up and its full Inspector reference.",
        "experimental": "Experimental",
    },
    "es": {
        "overview": "Descripción", "setup": "Cómo se usa", "inspector": "Inspector", "events": "Eventos",
        "api": "API para scripts", "example": "Ejemplo", "tips": "Consejos", "related": "Ver también",
        "field": "Campo", "type": "Tipo", "default": "Por defecto", "description": "Descripción",
        "member": "Miembro", "namespace": "Namespace", "base": "Clase base", "requires": "Requiere",
        "implements": "Implementa", "abstract": "Clase base abstracta: se hereda, no se agrega directamente.",
        "addcomp": "Agrégalo desde **Add Component** buscando", "noclip": "Este componente no tiene un comportamiento visual propio, por eso no tiene clip.",
        "unityevent": "UnityEvent (asigna listeners en el Inspector)", "csevent": "Evento C#",
        "property": "Propiedad", "method": "Método", "static": "estático", "inherited": "Campos heredados de",
        "components": "Componentes", "catalog_intro": "Todos los componentes de esta categoría. Haz clic en una tarjeta para ver qué hace, cómo se configura y la referencia completa del Inspector.",
        "experimental": "Experimental",
    },
}


def load_reference() -> dict:
    data = json.loads((ROOT / "data" / "components.json").read_text(encoding="utf-8"))
    return {c["name"]: c for c in data}


def as_text(item) -> str:
    """Un ítem de lista YAML con ': ' sin comillas llega como dict de una clave; se vuelve a unir."""
    if isinstance(item, dict):
        return "; ".join(f"{k}: {v}" for k, v in item.items())
    return str(item)


def load_content() -> dict:
    out = {}
    for path in sorted(CONTENT.rglob("*.yml")):
        doc = yaml.safe_load(path.read_text(encoding="utf-8"))
        if not doc or "name" not in doc:
            sys.exit(f"{path}: falta 'name'")
        out[doc["name"]] = doc
    return out


def cell(text) -> str:
    if text is None:
        return ""
    s = str(text).strip().replace("\n", " ")
    return s.replace("|", "\\|")


def code(text) -> str:
    if text in (None, "", "null"):
        return "—"
    return f"`{cell(text)}`"


def link_component(name: str, ref: dict, from_cat: str) -> str:
    c = ref.get(name)
    if not c:
        return f"`{name}`"
    cat = CAT_BY_NS[c["namespace"]][0]
    prefix = "" if cat == from_cat else f"../{cat}/"
    return f"[{name}]({prefix}{name}.md)"


def assets(lang: str, depth: int) -> str:
    """Ruta relativa a /assets: los assets viven una sola vez en la raíz; el idioma no default suma un nivel."""
    return "../" * (depth + (0 if lang == LANGS[0] else 1)) + "assets"


def clip_html(name: str, caption: str | None, lang: str) -> str:
    poster = f"{assets(lang, 3)}/clips/{name}.jpg"
    src = f"{assets(lang, 3)}/clips/{name}.mp4"
    cap = f'\n  <figcaption>{caption}</figcaption>' if caption else ""
    return (
        f'<figure class="gk-clip" markdown="0">\n'
        f'  <video src="{src}" poster="{poster}" autoplay loop muted playsinline preload="metadata"></video>{cap}\n'
        f"</figure>\n"
    )


def render_component(name: str, ref: dict, content: dict, lang: str) -> str:
    c = ref[name]
    t = T[lang]
    cat = CAT_BY_NS[c["namespace"]]
    doc = content.get(name, {})
    loc = doc.get(lang, {}) or {}
    other = doc.get("es" if lang == "en" else "en", {}) or {}
    lines: list[str] = []

    summary = loc.get("summary") or ""
    lines.append("---")
    lines.append(f"title: {name}")
    if summary:
        lines.append("description: " + json.dumps(summary, ensure_ascii=False))
    lines.append("---")
    lines.append("")
    lines.append(f"# {name}")
    lines.append("")

    badges = [f'<span class="gk-badge">{cat[2] if lang == "en" else cat[3]}</span>']
    if c.get("menu") and "Experimental" in c["menu"] or doc.get("experimental"):
        badges.append(f'<span class="gk-badge gk-badge--warn">{t["experimental"]}</span>')
    if c["base"] not in ("MonoBehaviour",):
        badges.append(f'<span class="gk-badge gk-badge--muted">{c["base"]}</span>')
    lines.append('<div class="gk-badges">' + " ".join(badges) + "</div>")
    lines.append("")
    if summary:
        lines.append(f'<p class="gk-lead">{summary}</p>')
        lines.append("")

    has_clip = (CLIPS / f"{name}.mp4").exists()
    if has_clip:
        lines.append(clip_html(name, loc.get("clip_caption"), lang))
    elif doc.get("clip") is False and not c["abstract"]:
        lines.append(f'!!! note ""\n    {t["noclip"]}\n')

    if c["abstract"]:
        lines.append(f'!!! info ""\n    {t["abstract"]}\n')

    if loc.get("description"):
        lines.append(f"## {t['overview']}\n")
        lines.append(loc["description"].strip() + "\n")

    meta = [f"**{t['namespace']}** `{c['namespace']}`", f"**{t['base']}** `{c['base']}`"]
    if c["requires"]:
        meta.append(f"**{t['requires']}** " + ", ".join(f"`{r}`" for r in c["requires"]))
    if c["interfaces"]:
        meta.append(f"**{t['implements']}** " + ", ".join(f"`{i}`" for i in c["interfaces"]))
    lines.append(" &nbsp;·&nbsp; ".join(meta) + "\n{ .gk-meta }\n")

    if loc.get("setup"):
        lines.append(f"## {t['setup']}\n")
        if not c["abstract"]:
            lines.append(f"{t['addcomp']} `{name}`.\n")
        for i, step in enumerate(loc["setup"], 1):
            lines.append(f"{i}. {as_text(step).strip()}")
        lines.append("")

    fields = [f for f in c["fields"] if not f["unityEvent"]]
    events = [f for f in c["fields"] if f["unityEvent"]]
    fdesc = loc.get("fields") or {}
    if fields:
        lines.append(f"## {t['inspector']}\n")
        lines.append(f"| {t['field']} | {t['type']} | {t['default']} | {t['description']} |")
        lines.append("|---|---|---|---|")
        for f in fields:
            desc = fdesc.get(f["name"]) or (f["tooltip"] if lang == "es" else None) or ""
            if f.get("range"):
                desc = (desc + f" ({f['range']})").strip()
            label = f"**{f['label']}**"
            lines.append(f"| {label}<br><small>`{f['name']}`</small> | `{cell(f['type'])}` | {code(f['default'])} | {cell(desc)} |")
        lines.append("")

    edesc = loc.get("events") or {}
    if events or c["events"]:
        lines.append(f"## {t['events']}\n")
        lines.append(f"| {t['member']} | {t['type']} | {t['description']} |")
        lines.append("|---|---|---|")
        for f in events:
            lines.append(f"| **{f['label']}**<br><small>`{f['name']}`</small> | {t['unityevent']} | {cell(edesc.get(f['name'], ''))} |")
        for e in c["events"]:
            lines.append(f"| `{e['name']}` | {t['csevent']} `{cell(e['type'])}` | {cell(edesc.get(e['name'], ''))} |")
        lines.append("")

    adesc = loc.get("api") or {}
    props = [p for p in c["properties"]]
    methods = [m for m in c["methods"] if m["declaredIn"] == name or m["name"] in adesc]
    if props or methods:
        lines.append(f"## {t['api']}\n")
        lines.append(f"| {t['member']} | {t['description']} |")
        lines.append("|---|---|")
        for p in props:
            access = "{ get; set; }" if p["settable"] else "{ get; }"
            st = "static " if p["static"] else ""
            lines.append(f"| `{st}{cell(p['type'])} {p['name']} {access}` | {cell(adesc.get(p['name'], ''))} |")
        seen = set()
        for m in methods:
            if m["signature"] in seen:
                continue
            seen.add(m["signature"])
            st = "static " if m["static"] else ""
            lines.append(f"| `{st}{cell(m['signature'])}` | {cell(adesc.get(m['name'], ''))} |")
        lines.append("")

    if loc.get("example"):
        lines.append(f"## {t['example']}\n")
        lines.append(loc["example"].strip() + "\n")

    if loc.get("tips"):
        lines.append(f"## {t['tips']}\n")
        for tip in loc["tips"]:
            lines.append(f"- {as_text(tip).strip()}")
        lines.append("")

    related = doc.get("related") or []
    if related:
        lines.append(f"## {t['related']}\n")
        lines.append(" · ".join(link_component(r, ref, cat[0]) for r in related) + "\n")

    return "\n".join(lines).rstrip() + "\n"


def render_category_index(cat, names, ref, content, lang) -> str:
    t = T[lang]
    title = cat[2] if lang == "en" else cat[3]
    intro = ((content.get("__categories__", {}) or {}).get(cat[0], {}) or {}).get(lang, "")
    out = ["---", f"title: {title}", "---", "", f"# {title}", ""]
    if intro:
        out.append(intro.strip() + "\n")
    out.append(t["catalog_intro"] + "\n")
    out.append('<div class="gk-cards" markdown="0">')
    for n in names:
        loc = (content.get(n, {}) or {}).get(lang, {}) or {}
        summary = loc.get("summary", "")
        has_clip = (CLIPS / f"{n}.jpg").exists()
        img = f'<img src="{assets(lang, 2)}/clips/{n}.jpg" alt="{n}" loading="lazy">' if has_clip else '<div class="gk-card__noimg"></div>'
        out.append(f'  <a class="gk-card" href="{n}/">{img}<div class="gk-card__body"><strong>{n}</strong><span>{summary}</span></div></a>')
    out.append("</div>\n")
    return "\n".join(out)


def build_nav(by_cat: dict) -> list:
    nav = [
        {"Home": "index.md"},
        {"Guides": [
            "guides/index.md",
            "guides/getting-started.md",
            "guides/platformer-character.md",
            "guides/top-down-character.md",
            "guides/input.md",
            "guides/combat.md",
            "guides/health-and-damage.md",
            "guides/enemies-and-ai.md",
            "guides/building-a-level.md",
            "guides/custom-abilities.md",
            "guides/troubleshooting.md",
        ]},
    ]
    comp = ["components/index.md"]
    for cat in CATEGORIES:
        names = by_cat.get(cat[0], [])
        if not names:
            continue
        comp.append({cat[2]: [f"components/{cat[0]}/index.md"] + [{n: f"components/{cat[0]}/{n}.md"} for n in names]})
    nav.append({"Components": comp})
    nav.append({"Reference": [
        "reference/architecture.md",
        "reference/core-api.md",
        "reference/testing.md",
        "reference/changelog.md",
    ]})
    return nav


def main() -> None:
    ref = load_reference()
    content = load_content()
    cats_path = ROOT / "content" / "categories.yml"
    if cats_path.exists():
        content["__categories__"] = yaml.safe_load(cats_path.read_text(encoding="utf-8"))

    missing = [n for n in ref if n not in content]
    unknown = [n for n in content if n not in ref and not n.startswith("__")]
    if unknown:
        sys.exit("Contenido para componentes que no existen: " + ", ".join(unknown))

    by_cat: dict[str, list[str]] = {}
    for name, c in sorted(ref.items(), key=lambda kv: kv[0].lower()):
        by_cat.setdefault(CAT_BY_NS[c["namespace"]][0], []).append(name)

    for lang in LANGS:
        base = DOCS / lang / "components"
        if base.exists():
            shutil.rmtree(base)
        for cat in CATEGORIES:
            names = by_cat.get(cat[0], [])
            folder = base / cat[0]
            folder.mkdir(parents=True, exist_ok=True)
            (folder / "index.md").write_text(render_category_index(cat, names, ref, content, lang), encoding="utf-8")
            for n in names:
                (folder / f"{n}.md").write_text(render_component(n, ref, content, lang), encoding="utf-8")
        index_src = ROOT / "content" / f"components_index.{lang}.md"
        if index_src.exists():
            (base / "index.md").write_text(index_src.read_text(encoding="utf-8"), encoding="utf-8")

    template = (ROOT / "tools" / "mkdocs.template.yml").read_text(encoding="utf-8")
    nav_yaml = yaml.safe_dump({"nav": build_nav(by_cat)}, allow_unicode=True, sort_keys=False, width=200)
    (ROOT / "mkdocs.yml").write_text(
        "# GENERADO por tools/generate.py a partir de tools/mkdocs.template.yml. No editar a mano.\n" + template.rstrip() + "\n\n" + nav_yaml,
        encoding="utf-8",
    )

    total = len(ref)
    clips = len(list(CLIPS.glob("*.mp4")))
    print(f"{total} componentes, {total - len(missing)} con contenido, {clips} clips.")
    if missing:
        print("Sin contenido:", ", ".join(missing))


if __name__ == "__main__":
    main()
