from pathlib import Path
from PIL import Image
root=Path('C:/Users/PC/Documents/Codex/2026-10-01/referenced-chatgpt-conversation-this-is-an/outputs/DragonTower-Idle/Validation/ZephyrIdle')
frames=[Image.open(root/f'claw-{i:02d}.png').convert('RGB') for i in range(35)]
palette=frames[16].quantize(colors=256)
frames=[im.quantize(palette=palette,dither=Image.Dither.NONE) for im in frames]
frames[0].save('work/WindClawV3-preview.gif',save_all=True,append_images=frames[1:],duration=[20]*34+[750],loop=0,disposal=2)


