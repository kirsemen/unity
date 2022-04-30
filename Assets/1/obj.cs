using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class obj : MonoBehaviour
{
    public ComputeShader computeShader;
    public CustomRenderTexture texture;
    public CustomRenderTexture texture1;
    public Vector4 vec;
    public CustomRenderTexture tex;
    public CustomRenderTexture tex1;
    public bool b;
    private void Start()
    {

        tex = new CustomRenderTexture(texture.width, texture.height);
        tex.enableRandomWrite = true;
        tex1 = new CustomRenderTexture(texture.width, texture.height);
        tex1.enableRandomWrite = true;
    }
    public void update()
    {
        if (b)
        {

            Graphics.CopyTexture(texture, tex);
            Graphics.CopyTexture(texture1, tex1);
            computeShader.SetVector("vec", vec);
            computeShader.SetTexture(0, "tex", tex);
            computeShader.SetTexture(0, "tex1", tex1);
            computeShader.Dispatch(0, texture.width, texture.height, 1);
            Graphics.CopyTexture(tex, texture);
            Graphics.CopyTexture(tex1, texture1);
        }
    }
}
