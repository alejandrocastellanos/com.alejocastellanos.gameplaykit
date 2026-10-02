#!/usr/bin/env python3
"""Valida archivos de contenido de componentes: YAML válido, claves conocidas, campos/eventos/API que existen."""
import json, sys
from pathlib import Path
import yaml

ROOT = Path(__file__).resolve().parent.parent
REF = {c["name"]: c for c in json.loads((ROOT / "data/components.json").read_text(encoding="utf-8"))}
TOP = {"name", "clip", "related", "experimental", "en", "es"}
LANG = {"summary", "clip_caption", "description", "setup", "fields", "events", "api", "example", "tips"}

def check(path):
    errs = []
    try:
        d = yaml.safe_load(Path(path).read_text(encoding="utf-8"))
    except Exception as e:
        return [f"YAML inválido: {e}"]
    name = d.get("name")
    if name not in REF:
        return [f"'name' {name!r} no es un componente"]
    c = REF[name]
    errs += [f"clave desconocida '{k}'" for k in d if k not in TOP]
    fields = {f["name"] for f in c["fields"] if not f["unityEvent"]}
    events = {f["name"] for f in c["fields"] if f["unityEvent"]} | {e["name"] for e in c["events"]}
    api = {p["name"] for p in c["properties"]} | {m["name"] for m in c["methods"]}
    for r in d.get("related") or []:
        if r not in REF: errs.append(f"related '{r}' no existe")
    for lang in ("en", "es"):
        loc = d.get(lang)
        if not isinstance(loc, dict):
            errs.append(f"falta '{lang}'"); continue
        errs += [f"{lang}: clave desconocida '{k}'" for k in loc if k not in LANG]
        if not loc.get("summary"): errs.append(f"{lang}: falta summary")
        if not c["abstract"] and not loc.get("setup"): errs.append(f"{lang}: falta setup")
        for k in (loc.get("fields") or {}):
            if k not in fields: errs.append(f"{lang}: campo '{k}' no existe (válidos: {sorted(fields)})")
        missing = fields - set((loc.get("fields") or {}).keys())
        if missing: errs.append(f"{lang}: faltan descripciones de campos {sorted(missing)}")
        for k in (loc.get("events") or {}):
            if k not in events: errs.append(f"{lang}: evento '{k}' no existe (válidos: {sorted(events)})")
        for k in (loc.get("api") or {}):
            if k not in api: errs.append(f"{lang}: miembro de API '{k}' no existe (válidos: {sorted(api)})")
        for k in ("setup", "tips"):
            if loc.get(k) is not None and not isinstance(loc[k], list): errs.append(f"{lang}: '{k}' debe ser lista")
    return errs

bad = 0
for p in sys.argv[1:]:
    e = check(p)
    if e:
        bad += 1
        print(f"✗ {p}"); [print("   -", x) for x in e]
    else:
        print(f"✓ {p}")
sys.exit(1 if bad else 0)
