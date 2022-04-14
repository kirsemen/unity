using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;


public class Water5 : MonoBehaviour
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
    public ComputeShader computeShader2;
    public ComputeShader computeShader3;
    public CustomRenderTexture tex;
    public CustomRenderTexture texture;

    private int overflow = 0;
    private ComputeBuffer computeBuffer;
    private void Start()
    {
        tex = new CustomRenderTexture(texture.width, texture.height);
        tex.enableRandomWrite = true;
        tex.initializationColor = new Color(0.5f, 0.5f, 1, 0);

        Pixel[] data = new Pixel[1048576];
        computeBuffer = new ComputeBuffer(data.Length, sizeof(float) * 4 + sizeof(int));
        computeBuffer.SetData(data);

    }

    private void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit))
        {
            //for (int x = -10; x < 10; x++)
            //    for (int y = -10; y < 10; y++)
            //    {
            //        data[overflow] = new Pixel(hit.textureCoord * new UnityEngine.Vector2(texture.width, texture.height) + new UnityEngine.Vector2(x, y), new UnityEngine.Vector2(1, 1));
            //        overflow++;
            //        overflow %= data.Length;
            //    }
            computeShader2.SetBuffer(0, "data", computeBuffer);
            computeShader2.SetInt("countData", computeBuffer.count);
            UnityEngine.Vector2 pos = hit.textureCoord * new UnityEngine.Vector2(texture.width, texture.height) - new UnityEngine.Vector2(50, 50);
            computeShader2.SetInts("pos", new int[] { (int)pos.x, (int)pos.y });
            computeShader2.SetInts("sizePoint", new int[] { 100, 100 });
            computeShader2.SetInt("index", overflow);
            computeShader2.Dispatch(0, 1, 1, 1);
            overflow +=  100* 100;
            overflow %= computeBuffer.count;

        }


        computeShader.SetBuffer(0, "data", computeBuffer);
        computeShader.Dispatch(0, computeBuffer.count / 1024, 1, 1);

        computeShader1.SetBuffer(0, "data", computeBuffer);
        computeShader1.SetTexture(0, "tex", tex);
        computeShader1.Dispatch(0, computeBuffer.count / 1024, 1, 1);



        computeShader3.SetTexture(0, "tex", tex);
        computeShader3.Dispatch(0, tex.width, tex.height, 1);

        Graphics.CopyTexture(tex, texture);

        tex.Initialize();
    }
    private void OnDestroy()
    {
        computeBuffer.Dispose();
        texture.Initialize();
    }
}
