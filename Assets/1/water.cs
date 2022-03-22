using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class water : MonoBehaviour
{
    public ComputeShader computeShader;
    public CustomRenderTexture texture;
    public CustomRenderTexture texture1;
    public CustomRenderTexture tex;
    public CustomRenderTexture tex1;
    public CustomRenderTexture outTex;
    public CustomRenderTexture outTex1;
    public obj[] g;
    void Start()
    {

        tex = new CustomRenderTexture(texture.width, texture.height);
        tex.enableRandomWrite = true;
        tex1 = new CustomRenderTexture(texture.width, texture.height);
        tex1.enableRandomWrite = true;

        outTex = new CustomRenderTexture(texture.width, texture.height);
        outTex.enableRandomWrite = true;
        outTex1 = new CustomRenderTexture(texture.width, texture.height);
        outTex1.enableRandomWrite = true;

        var color = new Color(0, 0, 0, 0);
        var color1 = new Color(0, 0, 0, 0);
        tex.initializationColor = color;
        tex1.initializationColor = color1;
        outTex.initializationColor = color;
        outTex1.initializationColor = color;
        texture.initializationColor = color;
        texture1.initializationColor = color1;
        tex.Initialize();
        tex1.Initialize();
        outTex.Initialize();
        outTex1.Initialize();
        texture.Initialize();
        texture1.Initialize();
        
    }

    void Update()
    {
        foreach (var item in g)
        {
            item.update();
        }
        Graphics.CopyTexture(texture, tex);
        Graphics.CopyTexture(texture1, tex1);
        computeShader.SetTexture(0, "tex", tex);
        computeShader.SetTexture(0, "tex1", tex1);
        computeShader.SetTexture(0, "out_tex", outTex);
        computeShader.SetTexture(0, "out_tex1", outTex1);

        computeShader.Dispatch(0, texture.width, texture.height, 1);
        Graphics.CopyTexture(outTex, texture);
        Graphics.CopyTexture(outTex1, texture1);
        tex.Initialize();
        tex1.Initialize();
        outTex.Initialize();
        outTex1.Initialize();

    }
    void OnDestroy()
    {
        texture.Initialize();
        texture1.Initialize();
    }
}
