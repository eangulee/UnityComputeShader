using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderDemo : MonoBehaviour
{
    public GameObject sphere;
    public int count = 50000;
    private List<MeshRenderer> spheres = new List<MeshRenderer>();
    private List<Vector3> centers = new List<Vector3>();
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(sphere);
            float p1 = Random.Range(-100.0f, 100.0f);
            float p2 = Random.Range(-100.0f, 100.0f);
            float p3 = Random.Range(0f, 200.0f);
            go.transform.position = new Vector3(p1, p2, p3);
            MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
#if UNITY_EDITOR
            meshRenderer.material.color = new Color(1 - (p1 + 100) / 200f, 1 - (p2 + 100) / 200f, 1 - p3 / 200f);
#endif
            spheres.Add(meshRenderer);
            centers.Add(go.transform.position);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
