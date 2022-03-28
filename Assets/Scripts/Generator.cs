using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
    [SerializeField] GameObject powerUpPrefab;
    [SerializeField] GameObject scorePrefab;
    [SerializeField] PhysicMaterial wallsMaterial;
    [SerializeField] Material material;

    [SerializeField] float holeHeight = 10;
    [SerializeField] float constriction = 0.001f;
    [SerializeField] float noise = 2;
    [SerializeField] float noiseGain = 0.005f;

    [SerializeField] int octaves;
    [SerializeField] float persistance;
    [SerializeField] float scale = 5;
    [SerializeField] int height;

    int chunkSize = 50;
    int chunksCount = 5;
    int seed = 100;

    Queue<Chunk> chunks = new Queue<Chunk>();
    void Start()
    {
        seed = Random.Range(0, 9999);
        NoiseS3D.seed = seed;
        StartCoroutine(AutoRegenerate());
    }

    IEnumerator AutoRegenerate()
    {
        while(true)
        {
            Generate((int)transform.position.x / chunkSize -1);
            yield return new WaitForSeconds(1f);
        }
    }

    int lastOffset = -2;
    void Generate(int offset)
    {
        if (lastOffset >= offset)
            return;
        int deleteCount = offset - lastOffset;
        if (chunks.Count > deleteCount)
        {
            for (int i = 0; i < deleteCount; i++)
            {
                var chunk = chunks.Dequeue();
                chunk.DestroyAll();
            }
        }

        while (chunks.Count < chunksCount)
        {
            int index = offset + chunks.Count;
            Chunk chunk = GenerateChunk(index);
            chunks.Enqueue(chunk);
        }

        lastOffset = offset;
    }

    Chunk GenerateChunk(int position)
    {
        float[] heights = new float[chunkSize];
        NoiseS3D.octaves = octaves;
        NoiseS3D.falloff = persistance;
        for (int i = 0; i < chunkSize; i++)
        {
            float pos = (float)i / (chunkSize - 1) + position;
            heights[i] = (float)NoiseS3D.NoiseCombinedOctaves(((float)i / (chunkSize - 1) + position)*scale) * (noise + (pos * noiseGain)) + (float)NoiseS3D.NoiseCombinedOctaves((float)i / (chunkSize-1)+position) - pos*7f;
            int distanceToPikes = 500-(position * (chunkSize - 1) + i);
            var distance = 250-Mathf.Clamp(Mathf.Abs(distanceToPikes), 0, 250);
            Debug.Log(distance);
            if (distance % 2 == 0)
            {
                heights[i] += 0.005f * distance;
            } else
            {
                heights[i] -= 0.005f * distance;
            }
        }

        Transform chunkTransfrom = new GameObject("Chunk(" + position + ")").transform;
        chunkTransfrom.position = new Vector3(position * (chunkSize - 1), 0, 0);
        GameObject bottom = new GameObject("bottom");
        bottom.layer = 8;
        bottom.transform.parent = chunkTransfrom;
        bottom.transform.localPosition = Vector3.zero;
        bottom.AddComponent<MeshFilter>().mesh = GenerateMesh(heights, true, position);
        bottom.AddComponent<MeshRenderer>().material = material;
        bottom.AddComponent<MeshCollider>().material = wallsMaterial;

        GameObject top = new GameObject("top");
        top.layer = 8;
        top.transform.parent = chunkTransfrom;
        top.transform.localPosition = Vector3.zero;
        top.AddComponent<MeshFilter>().mesh = GenerateMesh(heights, false, position);
        top.AddComponent<MeshRenderer>().material = material;
        top.AddComponent<MeshCollider>().material = wallsMaterial;

        if (position > 1)
        {
            System.Random random = new System.Random(seed + position);
            if (random.Next(100) > 50)
            {
                var powerup = Instantiate(powerUpPrefab, chunkTransfrom);
                powerup.transform.localPosition = Vector3.up * heights[0];
            } else if (random.Next(100) > 20)
            {
                for (int i = 0; i < chunkSize/2; i++)
                {
                    var score = Instantiate(scorePrefab, chunkTransfrom);
                    score.transform.localPosition = new Vector3(i * 2, heights[i * 2], 0.5f);
                }
            }
        }
        return new Chunk(top, bottom, chunkTransfrom.gameObject);
    }

    Mesh GenerateMesh(float[] heights, bool bottom, int offset)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();
        for (int i = 0; i < chunkSize - 1; i++)
        {
            float scaledHoleHeight = holeHeight - (offset*chunkSize + i) * constriction;
            vertices.Add(new Vector3(i, heights[i] + (bottom ? -scaledHoleHeight : scaledHoleHeight), 0));                        //0
            vertices.Add(new Vector3(i, heights[i] + (bottom ? -scaledHoleHeight : scaledHoleHeight), 1));                        //1
            vertices.Add(new Vector3(i + 1, heights[i + 1] + (bottom ? -(scaledHoleHeight - 0.01f) : scaledHoleHeight - 0.01f), 0));              //2
            vertices.Add(new Vector3(i + 1, heights[i + 1] + (bottom ? -(scaledHoleHeight - 0.01f) : scaledHoleHeight - 0.01f), 1));                //3
            vertices.Add(new Vector3(i, heights[i] + (bottom ? -scaledHoleHeight - height : scaledHoleHeight + height), 0));               //4
            vertices.Add(new Vector3(i + 1, heights[i + 1] + (bottom ? -scaledHoleHeight - height : scaledHoleHeight + height), 0));       //5

            uvs.Add(new Vector2(0f, height / (height + 1f)));   //0
            uvs.Add(new Vector2(0f, 1f));                       //1
            uvs.Add(new Vector2(1f, height / (height + 1f)));   //2
            uvs.Add(new Vector2(1f, 1f));                       //3
            uvs.Add(new Vector2(0f, 0f));                       //4
            uvs.Add(new Vector2(1f, 0f));                       //5

            if (bottom)
            {
                triangles.Add(i * 6 + 0);   //0
                triangles.Add(i * 6 + 1);   //1
                triangles.Add(i * 6 + 2);   //2

                triangles.Add(i * 6 + 2);   //2
                triangles.Add(i * 6 + 1);   //1
                triangles.Add(i * 6 + 3);   //3

                triangles.Add(i * 6 + 0);   //0
                triangles.Add(i * 6 + 2);   //2
                triangles.Add(i * 6 + 4);   //4

                triangles.Add(i * 6 + 2);   //2
                triangles.Add(i * 6 + 5);   //5
                triangles.Add(i * 6 + 4);   //4
            }
            else
            {
                triangles.Add(i * 6 + 2);   //0
                triangles.Add(i * 6 + 1);   //1
                triangles.Add(i * 6 + 0);   //2

                triangles.Add(i * 6 + 3);   //2
                triangles.Add(i * 6 + 1);   //1
                triangles.Add(i * 6 + 2);   //3

                triangles.Add(i * 6 + 4);   //0
                triangles.Add(i * 6 + 2);   //2
                triangles.Add(i * 6 + 0);   //4

                triangles.Add(i * 6 + 4);   //2
                triangles.Add(i * 6 + 5);   //5
                triangles.Add(i * 6 + 2);   //4
            }
        }

        
        mesh.SetVertices(vertices.ToArray());
        mesh.SetTriangles(triangles.ToArray(), 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        return mesh;
    }

    class Chunk
    {
        public GameObject parent;
        public GameObject top;
        public GameObject bottom;

        public Chunk(GameObject top, GameObject bottom, GameObject parent)
        {
            this.top = top;
            this.bottom = bottom;
            this.parent = parent;
        }

        public void DestroyAll()
        {
            Destroy(parent);
        }
    }
}
