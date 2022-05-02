using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class Water : MonoBehaviour
{

    public struct Pixel
    {
        public UnityEngine.Vector2 pos;
        public UnityEngine.Vector2 vec;
        public int isOut;
        public Pixel(float x, float y, float vecX, float vecY)
        {
            this.pos = new UnityEngine.Vector2(x, y);
            this.vec = new UnityEngine.Vector2(vecX, vecY);
            this.isOut = 0;
        }
        public Pixel(UnityEngine.Vector2 pos, UnityEngine.Vector2 vec)
        {
            this.pos = pos;
            this.vec = vec;
            this.isOut = 0;
        }
    }

    public ComputeShader computeShader;
    public ComputeShader computeShader1;
    public CustomRenderTexture texture;
    public CustomRenderTexture tex;
    private List<Pixel> date;
    private void Start()
    {
        tex = new CustomRenderTexture(texture.width, texture.height);
        tex.enableRandomWrite = true;
        tex.initializationColor = new Color(0, 0, 0, 0);

        date = new List<Pixel>();
    }

    private void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit))
        {
            for (int x = -10; x < 10; x++)
                for (int y = -10; y < 10; y++)
                    date.Add(new Pixel(hit.textureCoord * new UnityEngine.Vector2(texture.width, texture.height) + new UnityEngine.Vector2(x, y), new UnityEngine.Vector2(1, 1)));
        }
        if (date.Count != 0)
        {
            ComputeBuffer computeBuffer = new ComputeBuffer(date.Count, sizeof(float) * 4 + 4);
            computeBuffer.SetData(date);

            computeShader.SetBuffer(0, "date", computeBuffer);
            computeShader.SetInt("size", date.Count);
            computeShader.SetVector("SizeMap", new UnityEngine.Vector2(texture.width, texture.height));
            computeShader.Dispatch(0, (date.Count + 31) / 32, 1, 1);


            computeShader1.SetBuffer(0, "date", computeBuffer);
            computeShader1.SetInt("size", date.Count);
            computeShader1.SetTexture(0, "tex", tex);
            computeShader1.Dispatch(0, (date.Count + 31) / 32, 1, 1);

            Graphics.CopyTexture(tex, texture);

            var newDate = new Pixel[computeBuffer.count];
            computeBuffer.GetData(newDate);
            date = new List<Pixel>(newDate);
            computeBuffer.Dispose();

            for (int i = 0; i < date.Count; i++)
            {
                if (date[i].isOut == 1)
                {
                    date.RemoveAt(i);
                    i--;
                }
            }
        }
        tex.Initialize();
    }
    private void OnDestroy()
    {
        texture.Initialize();
    }
}
