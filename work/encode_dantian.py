import sys
sys.path.insert(0,'work/video-tools')
import av
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
root=Path('C:/Users/PC/Documents/Codex/2026-10-01/referenced-chatgpt-conversation-this-is-an/outputs/DragonTower-Idle/Validation/ZephyrIdle')
font=ImageFont.truetype('C:/Windows/Fonts/malgun.ttf',18)
frames=[Image.open(root/f'dantian-{i:02d}.png').convert('RGB') for i in range(70)]
output=av.open('work/Dantian-preview.mp4','w');stream=output.add_stream('libx264',rate=50);stream.width=480;stream.height=910;stream.pix_fmt='yuv420p';stream.options={'crf':'18'}
for repeat in range(3):
 for i in range(100):
  index=min(i,69);im=Image.new('RGB',(480,910),(5,14,25));im.paste(frames[index],(0,60));d=ImageDraw.Draw(im)
  d.text((14,5),'단천 · 제피르 전용 Legendary',font=font,fill=(182,255,230))
  t=(index+1)*.02
  phase='암전 / 압축' if t<.30 else '절단 / 정적' if t<.45 else '폭발 / 무적 유지' if t<1.1 else '복구' if t<1.3 else '무적 종료'
  d.text((14,31),phase,font=font,fill=(160,210,230))
  for packet in stream.encode(av.VideoFrame.from_image(im)):output.mux(packet)
for packet in stream.encode():output.mux(packet)
output.close()
contact=Image.new('RGB',(1920,850))
for i,n in enumerate([12,19,25,36]):contact.paste(frames[n],(i*480,0))
contact.save('work/Dantian-phases.png')
