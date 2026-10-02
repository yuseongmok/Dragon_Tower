import sys
sys.path.insert(0,'work/video-tools')
import av
from PIL import Image,ImageDraw
p='C:/Users/PC/Videos/Bandicam/bandicam 2026-10-02 09-45-17-355.mp4'
c=av.open(p);s=c.streams.video[0];print(s.width,s.height,s.average_rate,float(s.duration*s.time_base))
frames=[]
for i,f in enumerate(c.decode(video=0)):
 if i%4==0:
  im=f.to_image();im.thumbnail((320,220));frames.append((i,float(f.time),im))
canvas=Image.new('RGB',(320*5,245*((len(frames)+4)//5)),(20,20,20));d=ImageDraw.Draw(canvas)
for j,(i,t,im) in enumerate(frames):
 x=j%5*320;y=j//5*245;canvas.paste(im,(x,y));d.text((x+5,y+222),f'{i} / {t:.3f}s',fill='white')
canvas.save('work/reference-contact.png');print(len(frames))
