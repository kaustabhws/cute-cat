"""Render the app's XPS control exports and code-drawn diagnostic animation frames.
No outputs from this script are inputs to runtime cat playback.
"""
import argparse
from pathlib import Path
import pymupdf as fitz
from PIL import Image

parser = argparse.ArgumentParser()
parser.add_argument('directory', type=Path)
args = parser.parse_args()
for source in (args.directory / 'ui').glob('*.xps'):
    with fitz.open(source) as document:
        page = document[0]
        page.get_pixmap(matrix=fitz.Matrix(1.25, 1.25), alpha=False).save(source.with_suffix('.png'))
for folder in ((args.directory / 'frames').iterdir() if (args.directory / 'frames').exists() else []):
    if not folder.is_dir():
        continue
    frames = [Image.open(file).convert('RGB') for file in sorted(folder.glob('*.png'))]
    if frames:
        frames[0].save(args.directory / f'{folder.name.lower()}.webp', append_images=frames[1:], save_all=True,
                       duration=20, loop=0, lossless=True)
        for frame in frames:
            frame.close()
print('Rendered native control exports and 50 Hz diagnostic previews.')
