using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Graph : MonoBehaviour
{

    public Vector3 originOffset;

    GameManager gm;

    [SerializeField] int thickness;
    [SerializeField] int maxEvents;
    [SerializeField] int maxObjects;
    [SerializeField] float circleRadius;

    [SerializeField] RawImage img;
    [SerializeField] ComputeShader shader;

    ComputeBuffer eventBuffer;
    ComputeBuffer objectBuffer;
    ComputeBuffer objColorBuffer;

    int width;
    int height;
    RenderTexture tex;
    Texture2D result;

    void Awake(){
        RectTransform rect = GetComponent<RectTransform>();
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
        eventBuffer.SetData(gm.events);
        objectBuffer.SetData(gm.objectsIdx);
        shader.SetInt("_Width", width);
        shader.SetInt("_Height", height);
        shader.SetInt("_Thickness", thickness);
        shader.SetInt("_NumObjects", gm.objects.Count);
        shader.SetFloat("_CircleRadius", circleRadius);
        int kernelHandle = shader.FindKernel("RenderGraph");
        if(tex != null){
            Destroy(tex);
        }
        tex = new RenderTexture(width, height, 24);
        tex.enableRandomWrite = true;
        tex.Create();

        shader.SetBuffer(kernelHandle, "_Objects", objectBuffer);
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
    }
}
