using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using UnityEngine;
using UnityEngine.UI;

public interface IScrollRectExCallBack1
{
    void ScrollCellIndex(int idx);
    int GetCellIndex();
    void OnPoolDispose();
    Transform transform { get;}
    GameObject prefab { get; set; }
    void ReceiveMsg(IMessage message);
}

public class LayoutDesc
{
    public GameObject Prefab;
    public List<uint> Ids;
    public int ConstraintCount;
    public Vector2 CellSize;
    public Vector2 Spacing;
}

public class CellsDesc
{
    public List<uint> Ids;
    public List<Vector2> ArchoredPoss;
    public List<IScrollRectExCallBack1> CallBack1s;
    public LayoutDesc LayoutDesc;
}
[RequireComponent(typeof(ScrollRect))]
public class ScrollRectEx1 : MonoBehaviour
{
    [SerializeField] public ScrollRect scrollRect;
    [SerializeField] public Transform CellParent;
    
    public Deque<int> Cells = new Deque<int>(8);
    private Dictionary<GameObject, List<IScrollRectExCallBack1>>  pools = new Dictionary<GameObject, List<IScrollRectExCallBack1>>();
    private Dictionary<int, CellsDesc> CellsDescDic = new Dictionary<int, CellsDesc>();

    private float scrollRectHeight;
    // Start is called before the first frame update
    public void FillCells(List<LayoutDesc> layoutDescList)
    {
        scrollRect = GetComponent<ScrollRect>();
        scrollRectHeight = scrollRect.viewport.rect.height;
        float totalY = 0f;
        int initCount = -1;
        int totalCount = 0;

        CellsDesc cellsDesc = new CellsDesc();
        // layout
        for (int i = 0; i < layoutDescList.Count; i++)
        {
            var layoutDesc = layoutDescList[i];
            if(!pools.ContainsKey(layoutDesc.Prefab))
                pools.Add(layoutDesc.Prefab, new List<IScrollRectExCallBack1>(4));
            
            float y = 0f;
            for (int j = 0; j < layoutDesc.Ids.Count; j++)
            {
                var id = layoutDesc.Ids[j];
                int hIndex = j % layoutDesc.ConstraintCount;
                if (hIndex == 0)
                {
                    cellsDesc = new CellsDesc();
                    cellsDesc.ArchoredPoss = new List<Vector2>();
                    cellsDesc.Ids = new List<uint>();
                    cellsDesc.CallBack1s = new List<IScrollRectExCallBack1>();
                    CellsDescDic.Add(totalCount, cellsDesc);
                    totalCount++;
                    cellsDesc.LayoutDesc = layoutDesc;
                    y = totalY + (layoutDesc.Spacing.y + layoutDesc.CellSize.y) * 0.5f;
                    totalY += (layoutDesc.Spacing.y + layoutDesc.CellSize.y);
                    if (initCount == -1 && scrollRectHeight < totalY)
                    {
                        initCount = totalCount;
                    }
                }

                float x = hIndex * (layoutDesc.CellSize.x + layoutDesc.Spacing.x) +
                          (layoutDesc.CellSize.x + layoutDesc.Spacing.x) * 0.5f;
                cellsDesc.ArchoredPoss.Add( new Vector2(-x, y));
                cellsDesc.Ids.Add(id);
                
            }
        }

        scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, totalY);

        endIndex = initCount - 1;

        startIndex = 0;
        for (int i = 0; i < initCount; i++)
        {
            var d = CellsDescDic[startIndex + i];
            d.CallBack1s.Clear();
            for (int j = 0; j < d.Ids.Count; j++)
            {
                var cell = GetCellFromPool(d.LayoutDesc.Prefab);
                d.CallBack1s.Add(cell);
                cell.ScrollCellIndex((int)d.Ids[j]);
                cell.transform.name = d.Ids[j].ToString();
                cell.prefab = d.LayoutDesc.Prefab;
                (cell.transform as RectTransform).anchoredPosition = new Vector2(999999f, 0f);
            }
            
            Cells.AddTail(startIndex + i);
        }
        foreach (int cellIndex in Cells)
        {
            var index = cellIndex;
            var cellDesc = CellsDescDic[index];
            for (int j = 0; j < cellDesc.Ids.Count; j++)
            {
                var cell = cellDesc.CallBack1s[j];
                (cell.transform as RectTransform).anchoredPosition = scrollRect.content.anchoredPosition - cellDesc.ArchoredPoss[j];
            }

        }
    }

    [SerializeField] public int startIndex = 0;

    [SerializeField] public int endIndex = 0;
    // Update is called once per frame
    void Update()
    {
        if (Cells.Count == 0) return;
 
        foreach (int cellIndex in Cells)
        {
            var index = cellIndex;
            var cellsDesc = CellsDescDic[index];
            for (int j = 0; j < cellsDesc.Ids.Count; j++)
            {
                var cell = cellsDesc.CallBack1s[j];
                (cell.transform as RectTransform).anchoredPosition = scrollRect.content.anchoredPosition - cellsDesc.ArchoredPoss[j];
            }

        }

        if (startIndex != 0)
        {
            var preIndex = startIndex - 1;
            var cellsDesc = CellsDescDic[preIndex];
            var pos = cellsDesc.ArchoredPoss[0].y;
            var cellSizeY = cellsDesc.LayoutDesc.CellSize.y + cellsDesc.LayoutDesc.Spacing.y;
            var bottomY = (scrollRect.content.anchoredPosition.y - pos) - cellSizeY / 2f;
            if (bottomY < 0f)
            {
                startIndex = preIndex;
                cellsDesc.CallBack1s.Clear();
                for (int i = 0; i < cellsDesc.Ids.Count; i++)
                {
                    var cell = GetCellFromPool(cellsDesc.LayoutDesc.Prefab);
                    cell.ScrollCellIndex((int)cellsDesc.Ids[i]);
                    cellsDesc.CallBack1s.Add(cell);
                    cell.transform.name = cellsDesc.Ids[i].ToString();
                    cell.prefab = cellsDesc.LayoutDesc.Prefab;
                    (cell.transform as RectTransform).anchoredPosition = scrollRect.content.anchoredPosition - cellsDesc.ArchoredPoss[0];
                }
                Cells.AddHead(startIndex);
            }
        }

        if (endIndex != CellsDescDic.Count - 1)
        {
            var nextIndex = endIndex + 1;
            var cellsDesc = CellsDescDic[nextIndex];
            var pos = cellsDesc.ArchoredPoss[0].y;
            var cellSizeY = cellsDesc.LayoutDesc.CellSize.y + cellsDesc.LayoutDesc.Spacing.y;
            var topY = (scrollRect.content.anchoredPosition.y - pos) + cellSizeY / 2f;
            if (topY > -scrollRectHeight)
            {
                endIndex = nextIndex;
                cellsDesc.CallBack1s.Clear();
                for (int i = 0; i < cellsDesc.Ids.Count; i++)
                {
                    var cell = GetCellFromPool(cellsDesc.LayoutDesc.Prefab);
                    cellsDesc.CallBack1s.Add(cell);
                    cell.ScrollCellIndex((int)cellsDesc.Ids[i]);
                    cell.transform.name = cellsDesc.Ids[i].ToString();
                    cell.prefab = cellsDesc.LayoutDesc.Prefab;
                    (cell.transform as RectTransform).anchoredPosition = scrollRect.content.anchoredPosition - cellsDesc.ArchoredPoss[0];
                }
                

                Cells.AddTail(endIndex);

            }
        }
        ////
        {
            var cellsDesc = CellsDescDic[startIndex];
            var startPos = cellsDesc.ArchoredPoss[0].y;
            var cellSizeY = cellsDesc.LayoutDesc.CellSize.y + cellsDesc.LayoutDesc.Spacing.y;

            var cellBottomY = (scrollRect.content.anchoredPosition.y - startPos) - cellSizeY / 2f;
            if (cellBottomY > 0f)
            {
                Cells.RemoveHead();
                for (int i = 0; i < cellsDesc.CallBack1s.Count; i++)
                {
                    ReleaseCell(cellsDesc.CallBack1s[i]);
                    (cellsDesc.CallBack1s[i].transform as RectTransform).anchoredPosition = new Vector2(999999f, 0f);
                }
                cellsDesc.CallBack1s.Clear();
                
                startIndex = startIndex + 1;
                
            }
        }


        {
            var cellsDesc = CellsDescDic[endIndex];
            var endPos = cellsDesc.ArchoredPoss[0].y;
            var cellSizeY = cellsDesc.LayoutDesc.CellSize.y + cellsDesc.LayoutDesc.Spacing.y;

            var itemTopY = (scrollRect.content.anchoredPosition.y - endPos) + cellSizeY / 2f;
            if (itemTopY < -scrollRectHeight)
            {
                Cells.RemoveTail();
                for (int i = 0; i < cellsDesc.CallBack1s.Count; i++)
                {
                    ReleaseCell(cellsDesc.CallBack1s[i]);
                    (cellsDesc.CallBack1s[i].transform as RectTransform).anchoredPosition = new Vector2(999999f, 0f);

                }
                endIndex = endIndex - 1;
            }
        }

    }

    float GetCellY(int index)
    {
        return CellsDescDic[index].ArchoredPoss[0].y;
    }

    IScrollRectExCallBack1 GetCellFromPool(GameObject prefab)
    {
        var pool = pools[prefab];
        if (pool.Count == 0)
        {
            var cell = GameObject.Instantiate(prefab, CellParent, false);
            return cell.GetComponent<IScrollRectExCallBack1>();
        }
        var go = pool[pool.Count-1];
        pool.RemoveAt(pool.Count - 1);
        return go;
    }

    void ReleaseCell(IScrollRectExCallBack1 cell)
    {
        cell.OnPoolDispose();
        var pool = pools[cell.prefab];
        pool.Add(cell);
    }

    public IScrollRectExCallBack1 GetCell(int Id)
    {
        foreach (int cellIndex in Cells)
        {
            var index = cellIndex;
            var cellsDesc = CellsDescDic[index];
            for (int j = 0; j < cellsDesc.Ids.Count; j++)
            {
                if (Id == cellsDesc.Ids[j])
                {
                    return cellsDesc.CallBack1s[j];
                }
                
            }
        }

        return null;
    }

    public void RefreshCells()
    {
        foreach (int cellIndex in Cells)
        {
            var index = cellIndex;
            var cellsDesc = CellsDescDic[index];
            for (int j = 0; j < cellsDesc.Ids.Count; j++)
            {
                var cell = cellsDesc.CallBack1s[j];
                cell.ScrollCellIndex((int)cellsDesc.Ids[j]);
            }

        }
    }
}
