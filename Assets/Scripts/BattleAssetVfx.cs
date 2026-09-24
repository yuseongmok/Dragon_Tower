using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DragonTower
{
    // Renders third-party world-space particle prefabs into the existing overlay UI.
    // One selected prefab is created per battle and replayed without per-cast allocation.
    public sealed class BattleAssetVfx : MonoBehaviour
    {
        const int FrameWidth=480, FrameHeight=850, TextureWidth=240, TextureHeight=425, VfxLayer=30;
        const float EffectTop=170f,EffectBottom=625f;
        RenderTexture target;
        Camera vfxCamera;
        RawImage output;
        GameObject instance,currentPrefab;
        ParticleSystem[] systems;
        Animator[] animators;
        Material compositeMaterial;
        readonly List<Material> runtimeMaterials=new List<Material>();
        int renderableSystemCount;
        float scale,duration,age;
        Vector2 offset;
        Vector3 euler;

        public int InstanceCount=>instance==null?0:1;
        public int SystemCount=>systems==null?0:systems.Length;
        public int RenderableSystemCount=>renderableSystemCount;

        public void Initialize(RectTransform uiLayer)
        {
            // Render at half resolution and upscale without filtering. Besides reducing fill rate
            // to one quarter, this makes smooth 3D particles sit better beside pixel-art actors.
            target=new RenderTexture(TextureWidth,TextureHeight,16,RenderTextureFormat.ARGB32)
            {name="Battle VFX",filterMode=FilterMode.Point,antiAliasing=1,useMipMap=false,autoGenerateMips=false};
            target.Create();ClearTarget();

            var cameraObject=new GameObject("Battle VFX Camera");
            vfxCamera=cameraObject.AddComponent<Camera>();
            vfxCamera.clearFlags=CameraClearFlags.SolidColor;vfxCamera.backgroundColor=Color.clear;
            vfxCamera.orthographic=true;vfxCamera.orthographicSize=FrameHeight/200f;vfxCamera.aspect=FrameWidth/(float)FrameHeight;
            vfxCamera.nearClipPlane=.01f;vfxCamera.farClipPlane=50;vfxCamera.allowHDR=false;vfxCamera.allowMSAA=false;
            vfxCamera.cullingMask=1<<VfxLayer;vfxCamera.targetTexture=target;vfxCamera.transform.position=new Vector3(0,0,-10);
            vfxCamera.enabled=false;
            foreach(var camera in Camera.allCameras)if(camera!=vfxCamera)camera.cullingMask&=~(1<<VfxLayer);

            var imageObject=new GameObject("Eric VFX output",typeof(RectTransform),typeof(RawImage));
            var rect=imageObject.GetComponent<RectTransform>();rect.SetParent(uiLayer,false);
            // Only show the combat stage portion of the render texture. Enemy/player cards,
            // warnings and controls remain outside this viewport and can never be covered.
            rect.anchorMin=rect.anchorMax=new Vector2(.5f,1);rect.pivot=new Vector2(.5f,.5f);
            rect.sizeDelta=new Vector2(FrameWidth,EffectBottom-EffectTop);
            rect.anchoredPosition=new Vector2(0,-(EffectTop+EffectBottom)*.5f);
            output=imageObject.GetComponent<RawImage>();output.texture=target;output.raycastTarget=false;
            output.uvRect=new Rect(0,1-EffectBottom/FrameHeight,1,(EffectBottom-EffectTop)/FrameHeight);
            var compositeShader=Shader.Find("DragonTower/VFX Composite");
            if(compositeShader!=null){compositeMaterial=new Material(compositeShader);output.material=compositeMaterial;}
            rect.SetAsFirstSibling();
        }

        public void Configure(SkillData skill)
        {
            GameObject prefab=skill==null?null:skill.battleVfx;
            scale=skill==null ? .65f : skill.vfxScale;offset=skill==null ? Vector2.zero : skill.vfxOffset;
            euler=skill==null ? Vector3.zero : skill.vfxEuler;duration=skill==null ? 1.6f : skill.vfxDuration;
            if(prefab==currentPrefab&&instance!=null){StopAndClear();return;}
            if(instance!=null)Destroy(instance);DestroyRuntimeMaterials();
            currentPrefab=prefab;instance=null;systems=null;animators=null;renderableSystemCount=0;StopAndClear();
            if(prefab==null)return;

            instance=Instantiate(prefab);instance.name=prefab.name+" (pooled)";SetLayer(instance.transform,VfxLayer);
            instance.transform.localScale=Vector3.one*scale;instance.transform.rotation=Quaternion.Euler(euler);
            var allSystems=instance.GetComponentsInChildren<ParticleSystem>(true);
            systems=allSystems;
            // Eric prefabs are composite effects. Disabling everything after the first N systems
            // removes trails, flashes and impact pieces depending on hierarchy order. Keep the
            // whole composition and limit the particles produced by each child instead.
            int configuredCap=Mathf.Clamp(skill.vfxSystemLimit,3,24);
            int perSystemCap=allSystems.Length==0?configuredCap:
                Mathf.Clamp(Mathf.CeilToInt(512f/allSystems.Length),3,configuredCap);
            for(int i=0;i<allSystems.Length;i++)
            {
                var system=allSystems[i];var renderer=system.GetComponent<ParticleSystemRenderer>();
                system.Stop(false,ParticleSystemStopBehavior.StopEmittingAndClear);system.Clear(false);
                var main=system.main;if(main.maxParticles>perSystemCap)main.maxParticles=perSystemCap;
                if(renderer!=null)renderer.enabled=true;
            }
            foreach(var light in instance.GetComponentsInChildren<Light>(true))light.enabled=false;
            NormalizeMaterials(instance);
            foreach(var system in systems)
            {
                var renderer=system.GetComponent<ParticleSystemRenderer>();
                if(renderer!=null&&renderer.enabled)renderableSystemCount++;
            }
            animators=instance.GetComponentsInChildren<Animator>(true);instance.SetActive(false);
        }

        public bool PlayAt(Vector2 framePoint)
        {
            if(instance==null||renderableSystemCount==0)return false;
            StopParticles();instance.SetActive(true);
            Vector2 point=framePoint+offset;
            instance.transform.position=new Vector3(point.x/100f,(FrameHeight*.5f+point.y)/100f,0);
            instance.transform.localScale=Vector3.one*scale;instance.transform.rotation=Quaternion.Euler(euler);
            if(animators!=null)foreach(var animator in animators){animator.enabled=true;animator.Rebind();animator.Update(0);}
            // Every system is controlled separately. Recursive Play/Clear from every parent makes
            // nested systems restart several times and produces missing or oversized frames.
            if(systems!=null)foreach(var system in systems)if(system!=null){system.Clear(false);system.Play(false);}
            age=0;vfxCamera.enabled=true;
            return true;
        }

        public void Step(float delta)
        {
            if(instance==null||!instance.activeSelf)return;
            age+=delta;if(age>=duration)StopAndClear();
        }

        void StopParticles()
        {
            if(systems!=null)foreach(var system in systems)if(system!=null)system.Stop(false,ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        void StopAndClear()
        {
            StopParticles();if(instance!=null)instance.SetActive(false);
            if(vfxCamera!=null)vfxCamera.enabled=false;if(target!=null)ClearTarget();
        }
        void ClearTarget()
        {
            var previous=RenderTexture.active;RenderTexture.active=target;GL.Clear(true,true,Color.clear);RenderTexture.active=previous;
        }
        static void SetLayer(Transform root,int layer)
        {
            root.gameObject.layer=layer;for(int i=0;i<root.childCount;i++)SetLayer(root.GetChild(i),layer);
        }
        void NormalizeMaterials(GameObject root)
        {
            // This shader derives render-target alpha from visible light intensity. Black pixels
            // in many third-party textures therefore remain transparent instead of becoming a box.
            var fallback=Shader.Find("DragonTower/Battle VFX Additive");if(fallback==null)return;
            foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                var materials=renderer.sharedMaterials;bool changed=false,hasMaterial=false;
                for(int i=0;i<materials.Length;i++)
                {
                    // Several Eric VFX prefabs intentionally leave a secondary material slot empty.
                    // Unity skips that slot, but replacing it with an additive material draws a white quad.
                    var source=materials[i];if(source==null)continue;
                    hasMaterial=true;
                    // A shader can report itself as supported while still using an opaque pass or
                    // an incompatible blend mode in the transparent RenderTexture. Rebuild every
                    // VFX material with the project's known transparent additive shader.
                    var replacement=new Material(fallback);
                    replacement.name=source.name+" (Battle safe)";
                    Texture texture=source.HasProperty("_MainTex")?source.GetTexture("_MainTex"):source.mainTexture;
                    if(texture!=null)
                    {
                        replacement.SetTexture("_MainTex",texture);
                        if(source.HasProperty("_MainTex"))
                        {
                            replacement.SetTextureScale("_MainTex",source.GetTextureScale("_MainTex"));
                            replacement.SetTextureOffset("_MainTex",source.GetTextureOffset("_MainTex"));
                        }
                    }
                    Color tint=Color.white;
                    if(source.HasProperty("_TintColor"))tint=source.GetColor("_TintColor");
                    else if(source.HasProperty("_Color"))tint=source.GetColor("_Color");
                    tint.a=Mathf.Clamp01(tint.a);
                    if(replacement.HasProperty("_TintColor"))replacement.SetColor("_TintColor",tint);
                    if(source.HasProperty("_Brightness")&&replacement.HasProperty("_Brightness"))
                        replacement.SetFloat("_Brightness",Mathf.Clamp(source.GetFloat("_Brightness"),.25f,2f));
                    if(source.HasProperty("_FlowSpeed")&&replacement.HasProperty("_FlowSpeed"))
                        replacement.SetFloat("_FlowSpeed",source.GetFloat("_FlowSpeed"));
                    materials[i]=replacement;runtimeMaterials.Add(replacement);changed=true;
                }
                // Some downloaded prefabs also contain a particle renderer whose only material is
                // missing. Unity draws that renderer with its magenta error material, often as a
                // very large quad covering most of the portrait screen.
                if(!hasMaterial){renderer.enabled=false;continue;}
                if(changed)renderer.sharedMaterials=materials;
            }
        }
        void DestroyRuntimeMaterials()
        {
            foreach(var material in runtimeMaterials)if(material!=null)Destroy(material);runtimeMaterials.Clear();
        }
        void OnDestroy()
        {
            if(instance!=null)Destroy(instance);
            if(vfxCamera!=null)Destroy(vfxCamera.gameObject);
            DestroyRuntimeMaterials();if(compositeMaterial!=null)Destroy(compositeMaterial);
            if(target!=null){target.Release();Destroy(target);}
        }
    }
}
