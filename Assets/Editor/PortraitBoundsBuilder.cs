using System;using System.Collections.Generic;using System.IO;using UnityEngine;using UnityEditor;
namespace DragonTower.Editor {
 public static class PortraitBoundsBuilder {
  static Dictionary<Texture2D,Vector2Int> sizes=new Dictionary<Texture2D,Vector2Int>();
  static Dictionary<Texture2D,Color32[]> pixels=new Dictionary<Texture2D,Color32[]>();
  static Rect Visible(Sprite s){
   var t=s.texture;if(!pixels.TryGetValue(t,out var data)){var path=AssetDatabase.GetAssetPath(t);var copy=new Texture2D(2,2);if(!ImageConversion.LoadImage(copy,File.ReadAllBytes(path)))throw new Exception(path);data=copy.GetPixels32();sizes[t]=new Vector2Int(copy.width,copy.height);pixels[t]=data;UnityEngine.Object.DestroyImmediate(copy);}
   var source=s.rect;var size=sizes[t];var r=new Rect(source.x*size.x/t.width,source.y*size.y/t.height,source.width*size.x/t.width,source.height*size.y/t.height);int left=(int)r.xMax,right=(int)r.xMin,bottom=(int)r.yMax,top=(int)r.yMin;
   for(int y=(int)r.yMin;y<(int)r.yMax;y++)for(int x=(int)r.xMin;x<(int)r.xMax;x++)if(data[y*size.x+x].a>24){left=Math.Min(left,x);right=Math.Max(right,x+1);bottom=Math.Min(bottom,y);top=Math.Max(top,y+1);}
   float k=256/Math.Max(r.width,r.height);return new Rect((left-r.center.x)*k,(bottom-r.center.y)*k,Math.Max(1,right-left)*k,Math.Max(1,top-bottom)*k);
  }
  static Rect Union(Rect a,Rect b)=>Rect.MinMaxRect(Math.Min(a.xMin,b.xMin),Math.Min(a.yMin,b.yMin),Math.Max(a.xMax,b.xMax),Math.Max(a.yMax,b.yMax));
  public static void Run(){try{Build();EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
  public static void Build(){var list=new List<PortraitBoundsEntry>();foreach(var dragon in ContentDatabase.Load().dragons)for(int stage=0;stage<3;stage++){
   var set=dragon.LoadAnimationSet(stage);var idle=set!=null?set.idle:dragon.LegacyAnimation(DragonAnimationState.Idle,stage);var frames=idle!=null&&idle.frames!=null&&idle.frames.Length>0?idle.frames:new[]{dragon.SpriteForStage(stage)};bool first=true;Rect envelope=default;
   foreach(var sprite in frames){if(sprite==null)continue;var r=Visible(sprite);float scale=idle!=null?idle.displayScale:1;var offset=idle!=null?idle.OffsetFor(sprite)*256:Vector2.zero;r=new Rect(r.position*scale+offset,r.size*scale);envelope=first?r:Union(envelope,r);first=false;}
   if(set?.floatingWeapon?.sprite!=null){var w=set.floatingWeapon;foreach(var pose in w.poses??Array.Empty<FloatingWeaponPose>()){if(Array.IndexOf(frames,pose.characterFrame)<0)continue;float radius=w.size.magnitude*.5f+3;envelope=Union(envelope,new Rect(pose.position-Vector2.one*radius,Vector2.one*radius*2));}}
   if(first)throw new Exception("Missing portrait "+dragon.StableId+"/"+stage);list.Add(new PortraitBoundsEntry{id=dragon.StableId,stage=stage,bounds=envelope});
  }Directory.CreateDirectory("Assets/Resources/UI");File.WriteAllText("Assets/Resources/UI/portrait-bounds.json",JsonUtility.ToJson(new PortraitBoundsCatalogue{entries=list.ToArray()},true));AssetDatabase.Refresh();Debug.Log("PORTRAIT_BOUNDS_OK forms="+list.Count);pixels.Clear();}
 }
}
