from pathlib import Path
old=Path('work/WindClawV4-backup/WindClawVfx.cs').read_text()
new=Path('Assets/Scripts/WindClawVfx.cs').read_text()
a='            for(int cut=0;cut<2;cut++)'
b='            if(final>=0&&final<.19f)'
x=old[old.index(a):old.index(b)]
y=new[new.index(a):new.index('            // The confirmed final hit')]
assert x==y
print('Previous slash rendering restored exactly; final impact is the only render change.')
