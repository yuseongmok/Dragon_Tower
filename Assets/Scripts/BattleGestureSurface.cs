using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DragonTower
{
    // EventSystem dispatches both mobile touches and editor mouse through this one path.
    public sealed class BattleGestureSurface : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IInitializePotentialDragHandler
    {
        public Func<bool> CanInteract;
        public Action Tap;
        public Action<int> Swipe;
        public const float TapDistance=18, SwipeDistance=44, TapSeconds=.35f, SwipeSeconds=.65f;
        int pointer=int.MinValue;
        Vector2 start;float started,maxTravel,lastTouch=-10;
        bool consumed;
        RectTransform Rect=>(RectTransform)transform;
        public bool Tracking=>pointer!=int.MinValue;
        bool Allowed=>isActiveAndEnabled&&CanInteract!=null&&CanInteract();
        public void Cancel(){pointer=int.MinValue;consumed=false;maxTravel=0;}
        void OnDisable(){Cancel();}
        void OnApplicationFocus(bool value){if(!value)Cancel();}
        void OnApplicationPause(bool value){if(value)Cancel();}
        void Update(){if(Tracking&&!Allowed)Cancel();}
        public void OnInitializePotentialDrag(PointerEventData e){e.useDragThreshold=false;}
        bool Point(PointerEventData e,out Vector2 p)
        {
            p=default;
            return RectTransformUtility.RectangleContainsScreenPoint(Rect,e.position,e.pressEventCamera)&&RectTransformUtility.ScreenPointToLocalPointInRectangle(Rect,e.position,e.pressEventCamera,out p);
        }
        public void OnPointerDown(PointerEventData e)
        {
            if(e.button!=PointerEventData.InputButton.Left||Tracking||!Allowed)return;
            // Browsers can synthesize a mouse event after a touch. Never count it twice.
            if(e.pointerId<0&&(Input.touchCount>0||Time.unscaledTime-lastTouch<.5f))return;
            if(!Point(e,out start))return;
            pointer=e.pointerId;started=Time.unscaledTime;maxTravel=0;consumed=false;
            if(pointer>=0)lastTouch=Time.unscaledTime;
        }
        bool TouchCanceled(int id)
        {for(int i=0;i<Input.touchCount;i++){var t=Input.GetTouch(i);if(t.fingerId==id&&t.phase==TouchPhase.Canceled)return true;}return false;}
        public void OnDrag(PointerEventData e)
        {
            if(e.pointerId!=pointer)return;
            if(!Allowed||TouchCanceled(pointer)||!Point(e,out var point)){Cancel();return;}
            Vector2 d=point-start;maxTravel=Mathf.Max(maxTravel,d.magnitude);
            if(consumed)return;
            float elapsed=Time.unscaledTime-started;
            if(elapsed>SwipeSeconds){consumed=true;return;}
            if(Mathf.Abs(d.y)>=SwipeDistance&&Mathf.Abs(d.y)>Mathf.Abs(d.x)){consumed=true;return;}
            if(Mathf.Abs(d.x)>=SwipeDistance&&Mathf.Abs(d.x)>=Mathf.Abs(d.y)*1.4f)
            {consumed=true;Swipe?.Invoke(d.x>0?1:-1);}
        }
        public void OnPointerUp(PointerEventData e)
        {
            if(e.pointerId!=pointer)return;
            // Also classify displacement on release if the browser did not send a drag event.
            OnDrag(e);
            if(!Tracking)return;
            bool tap=!consumed&&Allowed&&maxTravel<=TapDistance&&Time.unscaledTime-started<=TapSeconds;
            if(pointer>=0)lastTouch=Time.unscaledTime;
            Cancel();if(tap)Tap?.Invoke();
        }
    }
}
