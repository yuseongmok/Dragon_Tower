"""Validate a release and assemble GitHub-sized parts into a static Pages site."""
import hashlib
import json
from pathlib import Path
import shutil
import sys


def digest(path):
    with path.open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


source, destination = map(lambda p: Path(p).resolve(), sys.argv[1:3])
if destination.exists():
    raise SystemExit('Output must be a fresh directory')
manifest = json.loads((source / 'release.json').read_text(encoding='utf-8'))
destination.mkdir(parents=True)
for entry in manifest['files']:
    output = destination / entry['path']
    if not output.resolve().is_relative_to(destination):
        raise ValueError('Invalid release path')
    output.parent.mkdir(parents=True, exist_ok=True)
    if 'parts' in entry:
        with output.open('wb') as target:
            for part in entry['parts']:
                path = source / part['path']
                if not path.resolve().is_relative_to(source):
                    raise ValueError('Invalid part path')
                if path.stat().st_size != part['size'] or digest(path) != part['sha256']:
                    raise ValueError('Part checksum mismatch: ' + part['path'])
                with path.open('rb') as stream:
                    shutil.copyfileobj(stream, target)
    else:
        path = source / entry['path']
        if not path.resolve().is_relative_to(source):
            raise ValueError('Invalid source path')
        shutil.copy2(path, output)
    if output.stat().st_size != entry['size'] or digest(output) != entry['sha256']:
        raise ValueError('Release checksum mismatch: ' + entry['path'])
shutil.copy2(source / 'release.json', destination / 'release.json')
print('Verified and assembled release:', manifest['version'])
