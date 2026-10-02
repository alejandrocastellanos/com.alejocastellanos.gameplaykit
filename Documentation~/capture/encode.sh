#!/usr/bin/env bash
# Convierte los cuadros PNG grabados por los escenarios de captura en clips MP4 + póster JPG.
#   uso: capture/encode.sh <carpeta frames> <carpeta salida>   (por defecto ../DocCaptures/frames → docs/assets/clips)
# Requiere ffmpeg. Cada subcarpeta de frames (un id = nombre del componente) produce <id>.mp4 y <id>.jpg.
set -euo pipefail
FRAMES="${1:-../DocCaptures/frames}"
OUT="${2:-docs/assets/clips}"
mkdir -p "$OUT"
for dir in "$FRAMES"/*/; do
  id="$(basename "$dir")"
  count=$(ls "$dir" | grep -c '\.png$' || true)
  [ "$count" -gt 0 ] || continue
  ffmpeg -loglevel error -y -framerate 30 -i "$dir/f_%04d.png" \
    -c:v libx264 -pix_fmt yuv420p -crf 26 -preset slow -movflags +faststart "$OUT/$id.mp4"
  poster=$(( count * 55 / 100 ))
  ffmpeg -loglevel error -y -i "$dir/$(printf 'f_%04d.png' "$poster")" -vf scale=640:-2 -q:v 4 "$OUT/$id.jpg"
  echo "$id: $count frames → $(du -h "$OUT/$id.mp4" | cut -f1)"
done
