using UnityEngine;

public class DrawingSingleton : MonoBehaviour
{
    [SerializeField] private GameObject brush;
    [SerializeField] private Transform brushHolder;

    LineRenderer currentLineRenderer;

    public static DrawingSingleton Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void CreateBrush(Vector2 mousePos)
    {
        GameObject brushInstance = Instantiate(brush, brushHolder);
        currentLineRenderer = brushInstance.GetComponent<LineRenderer>();

        currentLineRenderer.SetPosition(0, mousePos);
        currentLineRenderer.SetPosition(1, mousePos);
    }

    public void AddAPoint(Vector2 pointPos)
    {
        currentLineRenderer.positionCount++;
        int positionIndex = currentLineRenderer.positionCount - 1;
        currentLineRenderer.SetPosition(positionIndex, pointPos);
    }


    public void ResetBrush()
    {
        currentLineRenderer = null;
    }
}