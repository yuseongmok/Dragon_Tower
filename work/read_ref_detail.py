import sys
sys.path.insert(0,'work/video-tools')
import av
from PIL import Image,ImageDraw
c=av.open('C:/Users/PC/Videos/Bandicam/bandicam 2026-10-02 09-45-17-355.mp4');out=Image.new('RGB',(1200,600),(20,20,20))
for i,f in enumerate(c.decode(video=0)):
 if i in [43,45,47,85,86,88]:
  k=[43,45,47,85,86,88].index(i);im=f.to_image().crop((190,90,550,370));im=im.resize((400,280));out.paste(im,(k%3*400,k//3*300))
out.save('work/reference-detail.png')
