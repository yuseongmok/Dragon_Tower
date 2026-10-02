from pathlib import Path
s=Path('work/Dantian-backup/ContentDatabase.asset').read_text(encoding='utf-8-sig')
guid=[x.split(': ')[1] for x in Path('Assets/Data/Skills/skill_dantian.asset.meta').read_text().splitlines() if x.startswith('guid:')][0]
s=s.replace('  items:',f'  - {{fileID: 11400000, guid: {guid}, type: 2}}\n  items:')
Path('Assets/Resources/ContentDatabase.asset').write_text(s,encoding='utf-8')
