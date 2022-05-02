using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Displasement : MonoBehaviour
{
    public struct Pixel
    {
        public UnityEngine.Vector2 pos;
        public UnityEngine.Vector2 vec;
        public float width;
        public Pixel(float x, float y, float vecX, float vecY)
        {
            this.pos = new UnityEngine.Vector2(x, y);
            this.vec = new UnityEngine.Vector2(vecX, vecY);
            this.width = 1;
        }
        public Pixel(UnityEngine.Vector2 pos, UnityEngine.Vector2 vec)
        {
            this.pos = pos;
            this.vec = vec;
            this.width = 1;
        }
    }

    public ComputeShader computeShader;
    public ComputeShader computeShader1;
    public ComputeShader computeShader2;
    public int displasementTextureSize = 512;
    public Water5[] obj;


    private CustomRenderTexture tex;
    [HideInInspector] public int overflow = 0;
    [HideInInspector] public ComputeBuffer computeBuffer;
    [HideInInspector] public CustomRenderTexture texture;


    private void Start()
    {
        tex = new CustomRenderTexture(displasementTextureSize, displasementTextureSize);
        tex.enableRandomWrite = true;
        tex.initializationColor = new Color(0.5f, 0.5f, 1, 0);

        texture = new CustomRenderTexture(displasementTextureSize, displasementTextureSize);
        texture.initializationColor = new Color(0.5f, 0.5f, 1, 0);

        Pixel[] data = new Pixel[1048576];
        computeBuffer = new ComputeBuffer(data.Length, sizeof(float) * 5);
        computeBuffer.SetData(data);
        for (int i = 0; i < obj.Length; i++)
        {
            obj[i].water = this;
        }
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < obj.Length; i++)
        {
            obj[i].update();
        }

        computeShader.SetBuffer(0, "data", computeBuffer);
        computeShader.Dispatch(0, computeBuffer.count / 1024, 1, 1);

        computeShader1.SetBuffer(0, "data", computeBuffer);
        computeShader1.SetTexture(0, "tex", tex);
        computeShader1.Dispatch(0, computeBuffer.count / 1024, 1, 1);



        computeShader2.SetTexture(0, "tex", tex);
        computeShader2.Dispatch(0, tex.width, tex.height, 1);

        Graphics.CopyTexture(tex, texture);
        tex.Initialize();
        GetComponent<Renderer>().sharedMaterial.SetTexture("_BumpTex", texture);
    }
    private void OnDestroy()
    {
        computeBuffer.Dispose();

        var tex1 = new CustomRenderTexture(1, 1);
        tex1.initializationColor = new Color(0.5f, 0.5f, 1, 0);
        tex1.Initialize();
        GetComponent<Renderer>().sharedMaterial.SetTexture("_BumpTex", tex1);
    }
}
