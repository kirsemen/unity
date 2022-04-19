using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;


public class Water3 : MonoBehaviour
{
    public struct Pixel
    {
        public UnityEngine.Vector2 pos;
        public UnityEngine.Vector2 vec;
        public int inside;
        public Pixel(float x, float y, float vecX, float vecY)
        {
            this.pos = new UnityEngine.Vector2(x, y);
            this.vec = new UnityEngine.Vector2(vecX, vecY);
            this.inside = 1;
        }
        public Pixel(UnityEngine.Vector2 pos, UnityEngine.Vector2 vec)
        {
            this.pos = pos;
            this.vec = vec;
            this.inside = 1;
        }
    }

    public ComputeShader computeShader;
    public ComputeShader computeShader1;
    public CustomRenderTexture tex;
    public CustomRenderTexture texture;
    private Pixel[] data = new Pixel[131072];
    private int overflow = 0;
    private ComputeBuffer computeBuffer;
    private void Start()
    {
        tex = new CustomRenderTexture(texture.width, texture.height);
        tex.enableRandomWrite = true;
        tex.initializationColor = new Color(0, 0, 0, 0);


        computeBuffer = new ComputeBuffer(data.Length, sizeof(float) * 4 + sizeof(int));
        computeBuffer.SetData(data);

    }

    private void Update()
    {
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit))
        {
            for (int x = -10; x < 10; x++)
                for (int y = -10; y < 10; y++)
                {
                    data[overflow] = new Pixel(hit.textureCoord * new UnityEngine.Vector2(texture.width, texture.height) + new UnityEngine.Vector2(x, y), new UnityEngine.Vector2(1, 1));
                    overflow++;
                    overflow %= data.Length;
                }
        }
        
        computeBuffer.SetData(data);

        computeShader.SetBuffer(0, "date", computeBuffer);
        computeShader.SetVector("SizeMap", new UnityEngine.Vector2(texture.width, texture.height));
        computeShader.Dispatch(0, data.Length / 1024, 1, 1);


        computeShader1.SetBuffer(0, "date", computeBuffer);
        computeShader1.SetTexture(0, "tex", tex);
        computeShader1.Dispatch(0, data.Length / 1024, 1, 1);

        Graphics.CopyTexture(tex, texture);

        computeBuffer.GetData(data);

        tex.Initialize();
    }
    private void OnDestroy()
    {
        computeBuffer.Dispose();
        texture.Initialize();
    }
}
