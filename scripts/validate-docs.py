"""Check active documentation links and JSON syntax without contacting the network."""
import json
import re
from pathlib import Path
from urllib.parse import unquote

root = Path(__file__).resolve().parent.parent
errors = []
documents = [root / 'README.md', *sorted((root / 'docs').rglob('*.md'))]
for file in documents:
    for target in re.findall(r'(?<!!)\[[^\]]+\]\(([^)]+)\)', file.read_text(encoding='utf-8-sig')):
        if target.startswith(('https:', 'http:', 'mailto:', '#')):
            continue
        relative = unquote(target.strip('<>').split('#')[0])
        if not (file.parent / relative).exists():
            errors.append(f'{file.relative_to(root)} -> {relative}')
for file in (root / 'docs').rglob('*.json'):
    try:
        json.loads(file.read_text(encoding='utf-8-sig'))
    except (ValueError, UnicodeError) as error:
        errors.append(f'{file.relative_to(root)}: {error}')
if errors:
    print('\n'.join(errors))
    raise SystemExit(1)
print(f'Checked {len(documents)} Markdown documents and all documentation JSON files.')
