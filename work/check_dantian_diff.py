from pathlib import Path
import difflib
for f in ['ContentDatabase.asset','BattleController.cs']:
 p=Path('Assets/Resources' if f.endswith('.asset') else 'Assets/Scripts')/f
 print(''.join(difflib.unified_diff(Path('work/Dantian-backup',f).read_text().splitlines(True),p.read_text().splitlines(True),fromfile='before/'+f,tofile='after/'+f)))
