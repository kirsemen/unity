using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;


public class Water51 : Water5
{

    public ComputeShader computeShader3;
    public UnityEngine.Vector2 velosity;


    override public void update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit))
        {
            computeShader3.SetBuffer(0, "data", water.computeBuffer);
            computeShader3.SetInt("countData", water.computeBuffer.count);
            UnityEngine.Vector2 pos = hit.textureCoord * water.displasementTextureSize - new UnityEngine.Vector2(50, 50);
            computeShader3.SetInts("pos", new int[] { (int)pos.x, (int)pos.y });
            computeShader3.SetInts("sizePoint", new int[] { 100, 100 });
            computeShader3.SetFloats("velosity", new float[] { velosity.x, velosity.y });
            computeShader3.SetInt("index", water.overflow);
            computeShader3.Dispatch(0, 1, 1, 1);
            water.overflow += 100 * 100;
            water.overflow %= water.computeBuffer.count;

        }

    }
}
