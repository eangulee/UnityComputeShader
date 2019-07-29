using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
public class DrawRainTexture : MonoBehaviour
{
    private CommandBuffer buffer;//List of graphics commands
    public RenderTexture targetTexture;//渲染到目标RT
    static int colors = Shader.PropertyToID("colors");//获取名为colors的PropertyToID，速度比直接用name访问更快
    Matrix4x4[] matrix = new Matrix4x4[COUNT];//为每一个线程分配一个位置矩阵
    float[] times = new float[COUNT];//为每一个线程分配计时器
    public Material mat;
    public ComputeShader shader;
    private int kernal;//核函数
    ComputeBuffer matrixBuffers;//GPU data buffer
    ComputeBuffer timeSliceBuffers;
    const int COUNT = 1023;
    const float SCALE = 0.03f;
    void Awake()
    {
        buffer = new CommandBuffer();
        for (int i = 0; i < COUNT; ++i)
        {
            times[i] = Random.Range(-1f, 1f);//在-1,1之间随机一个数组，雨点出现时间随机先后
            matrix[i] = Matrix4x4.identity;
            matrix[i].m00 = SCALE;//x轴缩放
            matrix[i].m11 = SCALE;//y轴缩放
            matrix[i].m22 = SCALE;//z轴缩放
            matrix[i].m03 = Random.Range(-1f, 1f);//x轴平移
            matrix[i].m13 = Random.Range(-1f, 1f);//y轴平移
        }
        matrixBuffers = new ComputeBuffer(COUNT, 64);//COUNT是GPU data的个数，64是每个data的大小，64 = (矩阵数量)x sizof(float) = 4x4x4 = 64
        matrixBuffers.SetData(matrix);//设置位置矩阵到ComputeBuffer中
        timeSliceBuffers = new ComputeBuffer(COUNT, 4);//COUNT是GPU data的个数，64是每个data的大小，4 = sizof(float) = 4
        timeSliceBuffers.SetData(times);//设置计时器到ComputeBuff中
        kernal = shader.FindKernel("CSMain");//获取核函数
        shader.SetBuffer(kernal, ShaderIDs.matrixBuffer, matrixBuffers);//将ComputeBuffer设置到Compute Shader中
        shader.SetBuffer(kernal, ShaderIDs.timeSliceBuffer, timeSliceBuffers);//将ComputeBuffer设置到Compute Shader中
    }

    private void Update()
    {
        shader.SetFloat(ShaderIDs._DeltaFlashSpeed, Time.deltaTime * 1.5f);//设置时间
        shader.Dispatch(kernal, COUNT, 1, 1);//调用核函数
        buffer.Clear();
        buffer.SetRenderTarget(targetTexture);//设置渲染目标
        buffer.ClearRenderTarget(true, true, new Color(0.5f, 0.5f, 1, 1));
        //buffer.SetGlobalBuffer(ShaderIDs.timeSliceBuffer, timeSliceBuffers);
        matrixBuffers.GetData(matrix);//获取compute shader中计算后的位置矩阵数据
        buffer.DrawMeshInstanced(GraphicsUtility.fullScreenMesh, 0, mat, 0, matrix);//使用compute shader计算后的位置矩阵批量绘制mesh
        mat.SetBuffer(ShaderIDs.timeSliceBuffer, timeSliceBuffers);
        //shader.SetFloat(ShaderIDs._DeltaFlashSpeed, Time.deltaTime * 1.5f);
        Graphics.ExecuteCommandBuffer(buffer);//执行渲染命令列表
    }

    // Update is called once per frame
    void OnDestroy()
    {
        buffer.Dispose();
        timeSliceBuffers.Dispose();
        matrixBuffers.Dispose();
    }
}
