import sys
sys.path.insert(0,'work/video-tools')
import av
from pathlib import Path
from PIL import Image
root=Path('C:/Users/PC/Documents/Codex/2026-10-01/referenced-chatgpt-conversation-this-is-an/outputs/DragonTower-Idle/Validation/ZephyrIdle')
images=[Image.open(root/f'claw-{i:02d}.png').convert('RGB') for i in range(35)]
ready=Image.open(root/'claw-ready.png').convert('RGB')
output=av.open('work/WindClawV4-preview.mp4','w');stream=output.add_stream('libx264',rate=50);stream.width=480;stream.height=850;stream.pix_fmt='yuv420p';stream.options={'crf':'18'}
for _ in range(3):
 for im in [ready]*20+images+[images[-1]]*30:
  frame=av.VideoFrame.from_image(im)
  for packet in stream.encode(frame):output.mux(packet)
for packet in stream.encode():output.mux(packet)
output.close()
