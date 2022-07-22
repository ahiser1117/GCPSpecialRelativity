using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Graph : MonoBehaviour
{

    public Color backgroundColor;
    public Color minorGridColor;
    public Color majorGridColor;
    public Color invalidWorldLine;
    public Color highlightColor;
    public Color timeLineColor;
    public Color gridDivideColor;

    public Vector3Int originOffset;

    GameManager gm;

    [SerializeField] int thickness;
    [SerializeField] int maxEvents;
    [SerializeField] int maxObjects;

    [SerializeField] RawImage img;
    [SerializeField] ComputeShader shader;

    ComputeBuffer eventBuffer;
    ComputeBuffer objectBuffer;
    ComputeBuffer objColorBuffer;

    int width;
    int height;
    RenderTexture tex;
    Texture2D result;
    RectTransform rect;

    void Awake(){
        rect = GetComponent<RectTransform>();
        gm = GameManager.instance;
        width = (int) rect.rect.width;
        height = (int) rect.rect.height;
        eventBuffer = new ComputeBuffer(maxEvents, 3 * sizeof(float));
        objectBuffer = new ComputeBuffer(maxObjects, 2 * sizeof(int));
        objColorBuffer = new ComputeBuffer(maxObjects, 4 * sizeof(float));
    }

    void OnDisable(){
        eventBuffer.Release();
        eventBuffer = null;
        objColorBuffer.Release();
        objColorBuffer = null;
        objectBuffer.Release();
        objectBuffer = null;
    }

    public void UpdateGraph(){
        width = (int) rect.rect.width;
        height = (int) rect.rect.height;
        eventBuffer.SetData(gm.events);
        objectBuffer.SetData(gm.objectsIdx);
        objColorBuffer.SetData(gm.objectColors);
        shader.SetInt("_Width", width);
        shader.SetInt("_Height", height);
        shader.SetInt("_Thickness", thickness);
        shader.SetInt("_NumObjects", gm.objects.Count);
        shader.SetInt("_xOffset", originOffset.x);
        shader.SetInt("_yOffset", originOffset.y);
        shader.SetInt("_IntervalIdx", gm.highlightedInterval);
        shader.SetFloat("_CurrentTime", gm.currentTime);
        shader.SetVector("_BackgroundColor", backgroundColor);
        shader.SetVector("_MinorGridColor", minorGridColor);
        shader.SetVector("_MajorGridColor", majorGridColor);
        shader.SetVector("_InvalidWorldLine", invalidWorldLine);
        shader.SetVector("_HighlightColor", highlightColor);
        shader.SetVector("_TimeLineColor", timeLineColor);
        shader.SetVector("_GridDivideColor", gridDivideColor);
        int kernelHandle = shader.FindKernel("RenderGraph");
        if(tex != null){
            Destroy(tex);
        }
        tex = new RenderTexture(width, height, 24);
        tex.enableRandomWrite = true;
        tex.Create();

        shader.SetBuffer(kernelHandle, "_Objects", objectBuffer);
        shader.SetBuffer(kernelHandle, "_ObjColors", objColorBuffer);
        shader.SetBuffer(kernelHandle, "_Events", eventBuffer);
        shader.SetTexture(kernelHandle, "_Result", tex);

        Vector2Int groups = new Vector2Int(Mathf.CeilToInt(width / 8f), Mathf.CeilToInt(height / 8f));
        shader.Dispatch(kernelHandle, groups.x, groups.y, 1);

        RenderTexture.active = tex;
        if(result != null){
            Destroy(result);
        }
        result = new Texture2D(width, height);
        result.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        result.Apply();
        img.texture = result;

        gm.betaGraph.UpdateGraph();
    }

    Vector3 mouseInit;
    Vector3 oldPosition;

    void Update(){
        if(Input.GetMouseButtonDown(1)){
            mouseInit = Input.mousePosition;
            oldPosition = originOffset;
        } else if(Input.GetMouseButton(1)){
            originOffset = Vector3Int.CeilToInt(oldPosition + Input.mousePosition - mouseInit);
            foreach(ObjectPanel obj in gm.objects){
                obj.MoveFromGridDrag();
            }
            UpdateGraph();
        }
    }
}
