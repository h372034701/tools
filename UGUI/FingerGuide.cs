using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using PK2.UI;
using UnityEngine;
using SGF.Unity;

public sealed class FingerGuide : UIWindowBase, ICanvasRaycastFilter
{
    private RectTransform _targetRectTransform;
    private GuideBlockImage _guideBlockImage;
    
    private static FingerGuide _fingerGuideOrigin;
    private Camera uiCamera;
    private RectTransform noBlcokRecttransform;

    private Action callBack;

    private ParticleSystem contactPoint2;

    private RectTransform upFinger;

    private RectTransform downFinger;

    private Transform obj;


    private static MyDirection myCurrentDirection;
    void Init()
    {
        if (uiCamera == null)
        {
            uiCamera = CameraManager.Instance.GetCamera(CameraManager.CameraType.UICamera);
        }
        if (inited) return;
        inited = true;
        
        _targetRectTransform = transform.FindChildRecursive("TargetRectTransform") as RectTransform;
        
        obj = transform.Find("Obj").transform;

        contactPoint2 = obj.transform.Find("ContactPoint2").GetComponent<ParticleSystem>();
        upFinger = obj.transform.Find("UpFinger").GetComponent<RectTransform>();
        downFinger = obj.transform.Find("DownFinger").GetComponent<RectTransform>();
        upFinger.gameObject.SetActive(false);
        downFinger.gameObject.SetActive(false);
        contactPoint2.gameObject.SetActive(false);
        contactPoint2.Stop(true);

        _guideBlockImage = transform.GetComponent<GuideBlockImage>();
        {


            //  this.transform.SetParent(ViewManager.MainCanvas.transform);


            ////  fingerGuid.transform.SetParent(ViewManager.Instance.GetCurrentView);
            //  RectTransform RECT = this.GetComponent<RectTransform>();
            //  RECT.anchorMin = new Vector2(0, 0);
            //  RECT.anchorMax = new Vector2(1, 1);
            //  RECT.pivot = new Vector2(0.5f, 0.5f);


            //  RECT.offsetMin = new Vector2(0, 0);
            //  RECT.offsetMax = new Vector2(0, 0);

            //  RECT.localScale = Vector3.one;
            //  RECT.localPosition = Vector3.zero;
            //  RECT.SetAsLastSibling();
        }
    }

    protected override void OnInit()
    {
        base.OnInit();
        Init();
    }

    private Coroutine t;
    public enum MyDirection
    {
        up,
        down,
        left,
        right

    }
    void ShowGuid(RectTransform rect, Action call = null, MyDirection type = MyDirection.up)
    {
        if (forceBlock)
        {
            _targetRectTransform.position = rect.position;
            _targetRectTransform.sizeDelta = rect.rect.size;
            _guideBlockImage.SetBlockRectTransform(_targetRectTransform);
        }
        else
            _guideBlockImage.SetBlockRectTransform(null);
        stop = false;
        noBlcokRecttransform = rect;
        callBack = call;

        if (type == MyDirection.up)
        {
            upFinger.gameObject.SetActive(true);
            downFinger.gameObject.SetActive(false);
            upFinger.position = rect.position;
            FingerAni(upFinger, type);

        }
        else
        {
            downFinger.gameObject.SetActive(true);
            upFinger.gameObject.SetActive(false);
            downFinger.position = rect.position;
            FingerAni(downFinger, type);

        }


    }


    private List<Timer> timerList = new List<Timer>();

    private void FingerAni(Transform fin, MyDirection direction)
    {
        if (fin != null || noBlcokRecttransform != null)
        {
            if (direction == MyDirection.up)
            {
                fin.position = fin.position + new Vector3(0f, 0.61f * 1f, 0);

            }
            else if (direction == MyDirection.down)
            {
                fin.position = fin.position + new Vector3(0f, 0.61f * (-1f), 0);

            }
            else if (direction == MyDirection.right)
            {
                contactPoint2.gameObject.SetActive(false);
                fin.position = fin.position + new Vector3(4f * 1f, 0f, 0);
            }
            else
            {
                contactPoint2.gameObject.SetActive(false);
                fin.position = fin.position + new Vector3(4f * -1f, 0f, 0);

            }

            try
            {
                tweenAni = fin.DOMove(noBlcokRecttransform.transform.position, 1f);
                tweenAni.OnComplete(() =>
                    OnMoveEnd(fin, direction));
            }
            catch (Exception e)
            {
                DebugLogWrapper.LogException(e);
                OnMoveEnd(fin, direction);
            }
        }

    }



    private Tweener tweenAni;

    private bool stop = false;
    private void OnMoveEnd(Transform fin, MyDirection direction)
    {
        if (contactPoint2 == null)
            return;
        if (direction == MyDirection.up || direction == MyDirection.down)
        {
            contactPoint2.gameObject.SetActive(true);
            contactPoint2.Play();
        }


        timerList.Add(Timer.Register(0.6f, () =>
        {
            if (fin == null)
            {
                return;
            }

            contactPoint2.Stop(true);
            if (!stop)
                FingerAni(fin, direction);

        }));

    }


    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        if (blockAll)
        {
            return true;
        }
        if (!forceBlock)
        {
            Event eve = new Event();
            Event.PopEvent(eve);

#if UNITY_EDITOR
            if (eve.type == EventType.MouseUp && forceBlockClickAction != null)
#else
            if (Input.GetTouch(0).phase == TouchPhase.Ended && forceBlockClickAction != null)
#endif
            {
                forceBlockClickAction();
            }
            return false;
        }
        bool contains = RectTransformUtility.RectangleContainsScreenPoint(noBlcokRecttransform, sp, uiCamera);
        bool isBlock = !contains;
        if (isBlock == false)
        {

            Event eve = new Event();
            Event.PopEvent(eve);

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            if (myCurrentDirection == MyDirection.up || myCurrentDirection == MyDirection.down)
            {
             if ( Input.GetTouch(0).phase == TouchPhase.Moved)
                return true;
       
            if (Input.GetTouch(0).phase == TouchPhase.Ended)
                 CallBack();
            }
            else
            {
                if (eve.type == EventType.MouseMove)
                 return true;
               if (eve.type == EventType.MouseDrag)
                 CallBack();
            }

#endif

#if UNITY_EDITOR

            if (myCurrentDirection == MyDirection.up || myCurrentDirection == MyDirection.down)
            {

                if (eve.type == EventType.MouseMove || eve.type == EventType.MouseDrag)
                    return true;
                if (eve.type == EventType.MouseUp)
                    CallBack();

            }
            else
            {
                if (eve.type == EventType.MouseMove)
                    return true;
                if (eve.type == EventType.MouseDrag)
                    CallBack();

            }


#endif

        }
        return isBlock;

    }

    private void CallBack()
    {
        stop = true;
        timerList.ForEach(t =>
        {
            if (t != null)
                t.Cancel();
        });

        CloseBlock();

        if (callBack != null)
        {
            callBack();
        }
    }

    //Very important! Must wait until next frame to remove the UI, 
    //othewise Unity 2018 will fail to call Raycast for UI and throw an out of bound exception
    private static IEnumerator LateCloseWindow()
    {
        yield return null;
        ViewManager.Instance.CloseWindow(WindowID.Win_FingerGuid);
    }

    private bool blockAll = false;

    public static void CloseBlock()
    {
        MonoHelper.StartCoroutine(LateCloseWindow());
    }


    private bool forceBlock = true;
    private Action forceBlockClickAction;


    public static void Show(RectTransform rect, Action call = null, MyDirection type = MyDirection.up,
        bool force = true, Action forceBlockClickAction = null)
    {
        ViewManager.Instance.OpenWindow(WindowID.Win_FingerGuid, false,
            new object[] { rect, call, type, force, forceBlockClickAction });
    }


    public override void OnOpen(params object[] paras)
    {
        base.OnOpen(paras);
        var rect = paras[0] as RectTransform;
        var call = paras[1] as Action;
        var type = (MyDirection)paras[2];
        var force = (bool)paras[3];
        var forceBlockClickAction = paras[4] as Action;


        this.obj.gameObject.SetActive(true);
        this.obj.position = rect.transform.position;
        this.transform.localScale = Vector3.one;
        this.forceBlock = force;
        this.forceBlockClickAction = forceBlockClickAction;
        this.blockAll = false;
        this.ShowGuid(rect, call, type);
        myCurrentDirection = type;


    }

    public override bool NeedAnimation()
    {
        return false;
    }
    public override bool EscKeyBack
    {
        get
        {
            return !this.forceBlock;
        }
    }
    public override bool EnabledBlock
    {
        get
        {
            return false;
        }
    }

    public override void OnClose()
    {
        this.blockAll = false;
        this.stop = true;
        this.forceBlock = true;
        this.forceBlockClickAction = null;

        timerList.ForEach(t =>
        {
            if (t != null)
                t.Cancel();
        });
        timerList.Clear();
        if (tweenAni != null)
            tweenAni.Kill();
    }

}
