using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class water3 : MonoBehaviour
{
    public CustomRenderTexture tex;
    public CustomRenderTexture tex1;
    public ComputeShader computeShader;
    public CustomRenderTexture t_tex;
    public CustomRenderTexture t_tex1;

    private void Start()
    {
        t_tex = new CustomRenderTexture(tex.width, tex.height);
        t_tex.enableRandomWrite = true;
        t_tex1 = new CustomRenderTexture(tex.width, tex.height);
        t_tex1.enableRandomWrite = true;
        Color color = new Color(0, 0, 0, 0);
        t_tex.initializationColor = color;
        t_tex1.initializationColor = color;

        tex.Initialize();
        tex1.Initialize();
    }
    private void Update()
    {
        Graphics.CopyTexture(tex, t_tex);
        Graphics.CopyTexture(tex1, t_tex1);

        computeShader.SetTexture(0, "tex", t_tex);
        computeShader.SetTexture(0, "tex1", t_tex1);
        computeShader.Dispatch(0, tex.width, tex.height, 1);

        Graphics.CopyTexture(t_tex, tex);
        Graphics.CopyTexture(t_tex1, tex1);
        t_tex.Initialize();
        t_tex1.Initialize();

    }


    private void OnDestroy()
    {
        tex.Initialize();
        tex1.Initialize();
    }
}
