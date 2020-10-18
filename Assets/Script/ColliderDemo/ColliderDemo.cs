using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderDemo : MonoBehaviour
{
    public Transform minTrans;
    public Transform maxTrans;
    public ComputeShader computeShader;
    public GameObject sphere;
    public int count = 50000;//总共多少个renderer
    private List<MeshRenderer> spheres = new List<MeshRenderer>();
    private Vector4[] centers;
    private int[] result;
    private Vector4 min;//aabb最小坐标
    private Vector4 max;//aabb最大坐标
    private int _kernel;//核函数
    ComputeBuffer pointsBuffer;//GPU输入坐标buffer
    ComputeBuffer output;//输出GPU data buffer
    void Start()
    {
        result = new int[count];
        centers = new Vector4[count];
        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(sphere);
            go.SetActive(true);
            float p1 = Random.Range(-100.0f, 100.0f);
            float p2 = Random.Range(-100.0f, 100.0f);
            float p3 = Random.Range(0f, 200.0f);
            go.transform.position = new Vector3(p1, p2, p3);
            MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
            //#if UNITY_EDITOR
            //            meshRenderer.material.color = new Color(1 - (p1 + 100) / 200f, 1 - (p2 + 100) / 200f, 1 - p3 / 200f);
            //#endif
            spheres.Add(meshRenderer);
            centers[i] = go.transform.position;
        }
        pointsBuffer = new ComputeBuffer(count, 16); //16 = 4 x sizof(float) = 4x4 = 16
        output = new ComputeBuffer(count, 16);//count是GPU data的个数，16是每个data的大小，16 = 4 x sizof(float) = 4x4 = 16
        _kernel = computeShader.FindKernel("CSMain");//获取核函数
    }

    private void Update()
    {
        max = maxTrans.position;
        min = minTrans.position;
        ProfilerSample.BeginSample("ContainsPoint");
        for (int i = 0, length = centers.Length; i < length; i++)
        {
            result[i] = ContainsPoint(centers[i]) ? 1 : 0;
        }
        ProfilerSample.EndSample();

        ProfilerSample.BeginSample("ContainsPointComputeShader");
        computeShader.SetVector(Shader.PropertyToID("box_min"), min);//cpu->gpu
        computeShader.SetVector(Shader.PropertyToID("box_max"), max);//cpu->gpu
        computeShader.SetInt(Shader.PropertyToID("size"), count);//cpu->gpu
        pointsBuffer.SetData(centers);
        computeShader.SetBuffer(_kernel, Shader.PropertyToID("points"), pointsBuffer);//cpu->gpu
        computeShader.SetBuffer(_kernel, Shader.PropertyToID("output"), output);////cpu->gpu->cpu
        //Debug.Log(output.count + "        " + result.Length);
        computeShader.Dispatch(_kernel, 1024, 1, 1);//调用核函数
        output.GetData(result);//获取compute shader中计算后的结果数据
        ProfilerSample.EndSample();


        for (int i = 0, length = result.Length; i < length; i++)
        {
            if (result[i] > 0)
            {
                spheres[i].material.color = Color.red;
            }
            else
            {
                spheres[i].material.color = Color.white;
            }
        }
    }

    private void OnDestroy()
    {
        pointsBuffer.Dispose();
        output.Dispose();
    }

    /// <summary>
    /// 判断点是否在包围盒内部
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    private bool ContainsPoint(Vector3 point)
    {
        if (point.x < min.x) return false;
        if (point.y < min.y) return false;
        if (point.z < min.z) return false;
        if (point.x > max.x) return false;
        if (point.y > max.y) return false;
        if (point.z > max.z) return false;
        return true;
    }

    private void OnDrawGizmos()
    {
        //长宽高
        float l = max.x - min.x;
        float w = max.z - min.z;
        float h = max.y - min.y;
        Vector3 p1 = min;
        Vector3 p2 = min + new Vector4(l, 0, 0);
        Vector3 p3 = min + new Vector4(l, 0, w);
        Vector3 p4 = min + new Vector4(0, 0, w);
        Vector3 p5 = min + new Vector4(0, h, 0);
        Vector3 p6 = min + new Vector4(l, h, 0);
        Vector3 p7 = max;
        Vector3 p8 = min + new Vector4(0, h, w);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(p1, 0.5f);
        Gizmos.DrawSphere(p2, 0.5f);
        Gizmos.DrawSphere(p3, 0.5f);
        Gizmos.DrawSphere(p4, 0.5f);
        Gizmos.DrawSphere(p5, 0.5f);
        Gizmos.DrawSphere(p6, 0.5f);
        Gizmos.DrawSphere(p7, 0.5f);
        Gizmos.DrawSphere(p8, 0.5f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p4);
        Gizmos.DrawLine(p1, p4);
        Gizmos.DrawLine(p1, p5);
        Gizmos.DrawLine(p5, p6);
        Gizmos.DrawLine(p6, p7);
        Gizmos.DrawLine(p7, p8);
        Gizmos.DrawLine(p5, p8);
        Gizmos.DrawLine(p2, p6);
        Gizmos.DrawLine(p3, p7);
        Gizmos.DrawLine(p4, p8);

    }
}
