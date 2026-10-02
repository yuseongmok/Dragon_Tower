from pathlib import Path
from PIL import Image
root=Path('C:/Users/PC/Documents/Codex/2026-10-01/referenced-chatgpt-conversation-this-is-an/outputs/DragonTower-Idle/Validation/ZephyrIdle')
frames=[Image.open(root/f'galeimpact-{i:02d}.png').convert('RGB') for i in range(30)]
palette=frames[16].quantize(colors=256)
frames=[im.quantize(palette=palette,dither=Image.Dither.NONE) for im in frames]
frames[0].save('work/GaleImpact-preview.gif',save_all=True,append_images=frames[1:],duration=[20]*29+[750],loop=0,disposal=2)

