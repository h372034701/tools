using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IScrollRectExCallBack
{
    void ScrollCellIndex(int idx);
    int GetCellIndex();
    void OnPoolDispose();
    Transform transform { get;}
}
[RequireComponent(typeof(ScrollRect))]
public class ScrollRectEx : MonoBehaviour
{
    [SerializeField]private int cellTotalCount;

    [SerializeField] private GameObject cellPrefab;

    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] public Transform CellParent;
    
    public Deque<IScrollRectExCallBack> Cells = new Deque<IScrollRectExCallBack>(16);
    private List<IScrollRectExCallBack> pools = new List<IScrollRectExCallBack>(8);

    [SerializeField] private float scrollRectHeight = 0f;
    [SerializeField] private float cellSizeY = 0f;
    [SerializeField] private float spacing = 0f;

    // Start is called before the first frame update
    public void FillCells(int totalCount)
    {
        scrollRect = GetComponent<ScrollRect>();
        cellTotalCount = totalCount;
        var verticalLayoutGroup = scrollRect.content.GetComponent<VerticalLayoutGroup>();
        spacing = verticalLayoutGroup.spacing;
        var cellSize = (cellPrefab.transform as RectTransform).sizeDelta;
        cellSizeY = cellSize.y;
        scrollRectHeight = (scrollRect.transform as RectTransform).rect.height;
        scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, 
            cellTotalCount * cellSizeY + (cellTotalCount-1) * spacing);

        int count = Mathf.CeilToInt(scrollRectHeight / (cellSizeY + spacing)) ;
        count = Mathf.Min(count, cellTotalCount);
        endIndex = count-1;
        startIndex = 0;
        for (int i = 0; i < count; i++)
        {
            var cell = GetCellFromPool();
            cell.ScrollCellIndex(i);
            cell.transform.name = i.ToString();
            Cells.AddTail(cell);
            (cell.transform as RectTransform).anchoredPosition = new Vector2(999999f, 0f);
        }

    }

    [SerializeField] public int startIndex = 0;

    [SerializeField] public int endIndex = 0;
    // Update is called once per frame
    void Update()
    {
        if(cellTotalCount==0 || Cells.Count == 0) return;
        int i = 0;
        foreach (IScrollRectExCallBack cell in Cells)
        {
            var index = startIndex+i;
            var pos = new Vector2(0, GetCellY(index));
            (cell.transform as RectTransform).anchoredPosition = scrollRect.content.anchoredPosition - pos;
            i++;
        }

        if (startIndex != 0)
        {
            var preIndex = startIndex - 1;
            var pos = new Vector2(0f, GetCellY(preIndex));
            var bottomY = (scrollRect.content.anchoredPosition - pos).y - cellSizeY / 2f;
            if (bottomY < 0f)
            {
                startIndex = preIndex;
                var cell = GetCellFromPool();
                cell.ScrollCellIndex(startIndex);
                cell.transform.name = startIndex.ToString();
                var siblingIndex = CellParent.childCount - (endIndex - startIndex);
                cell.transform.SetSiblingIndex(siblingIndex);
                Cells.AddHead(cell);
                (Cells.GetHead().transform as RectTransform).anchoredPosition = scrollRect.content.anchoredPosition - pos;

            }
        }

        if (endIndex != cellTotalCount - 1)
        {
            var nextIndex = endIndex + 1;
            var pos = new Vector2(0f, GetCellY(nextIndex));
            var topY = (scrollRect.content.anchoredPosition - pos).y + cellSizeY / 2f;
            if (topY > -scrollRectHeight)
            {
                endIndex = nextIndex;
                var cell = GetCellFromPool();
                cell.ScrollCellIndex(endIndex);
                cell.transform.name = endIndex.ToString();
                cell.transform.SetAsLastSibling();
                Cells.AddTail(cell);
                (Cells.GetTail().transform as RectTransform).anchoredPosition = scrollRect.content.anchoredPosition - pos;

            }
        }
        //
        var startPos = new Vector2(0f, GetCellY(startIndex));
        var cellBottomY = (scrollRect.content.anchoredPosition - startPos).y - cellSizeY / 2f;
        if (cellBottomY > 0f)
        {
            var cell = Cells.GetHead();
            cell.transform.SetAsFirstSibling();
            Cells.RemoveHead();
            (cell.transform as RectTransform).anchoredPosition = new Vector2(999999f, 0f);
            startIndex = startIndex + 1;
            ReleaseCell(cell);
        }

        var endPos = new Vector2(0f, GetCellY(endIndex));
        var itemTopY = (scrollRect.content.anchoredPosition - endPos).y + cellSizeY / 2f;
        if (itemTopY < -scrollRectHeight)
        {
            var cell = Cells.GetTail();
            cell.transform.SetAsFirstSibling();
            Cells.RemoveTail();
            (cell.transform as RectTransform).anchoredPosition = new Vector2(999999f, 0f);
            endIndex = endIndex - 1;
            ReleaseCell(cell);
        }
    }

    float GetCellY(int index)
    {
        return cellSizeY / 2 + (index) * (spacing + cellSizeY);
    }

    IScrollRectExCallBack GetCellFromPool()
    {
        if (pools.Count == 0)
        {
            var cell = GameObject.Instantiate(cellPrefab, CellParent, false);

            return cell.GetComponent<IScrollRectExCallBack>();
        }
        var go = pools[pools.Count-1];
        pools.RemoveAt(pools.Count - 1);
        return go;
    }

    void ReleaseCell(IScrollRectExCallBack cell)
    {
        cell.OnPoolDispose();
        pools.Add(cell);
    }
}
