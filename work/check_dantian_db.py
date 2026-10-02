from pathlib import Path
import difflib
old=Path('work/Dantian-backup/ContentDatabase.asset').read_text(encoding='utf-8-sig').splitlines()
new=Path('Assets/Resources/ContentDatabase.asset').read_text(encoding='utf-8-sig').splitlines()
print('\n'.join(difflib.unified_diff(old,new,fromfile='before database',tofile='after database')))
