from pathlib import Path
from PIL import Image
root=Path('C:/Users/PC/Documents/Codex/2026-10-01/referenced-chatgpt-conversation-this-is-an/outputs/DragonTower-Idle/Validation/ZephyrIdle')
frames=[Image.open(root/'gale-v2-zero.png').convert('RGB')]+[Image.open(root/f'gale-{i:02d}.png').convert('RGB') for i in range(20)]
palette=frames[6].quantize(colors=256)
frames=[im.quantize(palette=palette,dither=Image.Dither.NONE) for im in frames]
frames[0].save('work/GaleStrike-v2.gif',save_all=True,append_images=frames[1:],duration=[33]*20+[800],loop=0,disposal=2)
