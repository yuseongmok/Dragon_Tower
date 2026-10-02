import sys
sys.path.insert(0,'work/video-tools')
import av
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
root=Path('C:/Users/PC/Documents/Codex/2026-10-01/referenced-chatgpt-conversation-this-is-an/outputs/DragonTower-Idle/Validation/ZephyrIdle')
font=ImageFont.truetype('C:/Windows/Fonts/malgun.ttf',20)
small=ImageFont.truetype('C:/Windows/Fonts/malgun.ttf',15)
sets=[('질풍강타 · Common','gale',20,30,4),('바람칼날 · Rare / 3연격','blade',30,50,6),('질풍참 · Rare / 단일 횡베기','galeimpact',30,50,9),('귀참 · Unique / 변경 없음','claw',35,50,17)]
loaded=[]
for title,prefix,count,fps,key in sets:
 frames=[Image.open(root/f'{prefix}-{i:02d}.png').convert('RGB') for i in range(count)]
 loaded.append(frames)
def panel(im,title):
 out=Image.new('RGB',(480,910),(6,16,25));out.paste(im,(0,60));draw=ImageDraw.Draw(out)
 draw.text((14,6),title,font=font,fill=(160,241,227));draw.text((14,33),'Unity 전투 캡처 · 기존 타격 타이밍 유지',font=small,fill=(190,202,215))
 return out
output=av.open('work/WindVFX-Remaster.mp4','w');stream=output.add_stream('libx264',rate=50);stream.width=480;stream.height=910;stream.pix_fmt='yuv420p';stream.options={'crf':'18'}
for s,frames in zip(sets,loaded):
 for repeat in range(2):
  for i in range(70):
   index=min(len(frames)-1,int(i/50*s[3]))
   im=panel(frames[index],s[0])
   for packet in stream.encode(av.VideoFrame.from_image(im)):output.mux(packet)
for packet in stream.encode():output.mux(packet)
output.close()
contact=Image.new('RGB',(1920,910))
for i,(s,frames) in enumerate(zip(sets,loaded)):contact.paste(panel(frames[s[4]],s[0]),(480*i,0))
contact.save('work/WindVFX-Remaster-comparison.png')
