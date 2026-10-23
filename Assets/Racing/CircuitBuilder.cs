using System.Collections.Generic;
using UnityEngine;

// Deterministic greybox circuit. Only attached to the MVP Circuit scene.
// Builds at runtime to keep track dimensions editable without hand-maintaining YAML geometry.
public class CircuitBuilder : MonoBehaviour
{
    [SerializeField] Movement vehicle;
    [SerializeField, Min(30f)] float radiusX = 65f;
    [SerializeField, Min(40f)] float radiusZ = 100f;
    [SerializeField, Range(10f, 24f)] float roadWidth = 16f;
    const int Segments = 128;
    const int GateCount = 8;
    readonly List<Object> ownedResources = new List<Object>();

    Vector3 Center(float angle) => new Vector3(radiusX * Mathf.Cos(angle), 0f, radiusZ * Mathf.Sin(angle));
    Vector3 Tangent(float angle) => new Vector3(-radiusX * Mathf.Sin(angle), 0f, radiusZ * Mathf.Cos(angle)).normalized;
    Vector3 Side(float angle) => Vector3.Cross(Vector3.up, Tangent(angle));

    void Awake()
    {
        if (vehicle == null)
        {
            Debug.LogError("CircuitBuilder requires the existing vehicle.", this);
            enabled = false;
            return;
        }
        Material asphalt = MakeMaterial(new Color(0.12f, 0.14f, 0.17f));
        Material white = MakeMaterial(new Color(0.92f, 0.94f, 0.96f));
        Material orange = MakeMaterial(new Color(1f, 0.4f, 0.08f));
        Material grass = MakeMaterial(new Color(0.15f, 0.28f, 0.18f));
        Material gateMaterial = MakeMaterial(new Color(0.1f, 0.8f, 0.95f));
        BuildRoad(asphalt);
        Box("Infield", new Vector3(0, -0.55f, 0), new Vector3(260, 1, 330), Quaternion.identity, grass);
        for (int i = 0; i < Segments; i++)
        {
            float a = i * Mathf.PI * 2f / Segments;
            float b = (i + 1) * Mathf.PI * 2f / Segments;
            foreach (int side in new[] { -1, 1 })
            {
                Vector3 from = Center(a) + Side(a) * side * (roadWidth * 0.5f + 0.6f);
                Vector3 to = Center(b) + Side(b) * side * (roadWidth * 0.5f + 0.6f);
                Vector3 delta = to - from;
                Box("Barrier", (from + to) * 0.5f + Vector3.up * 0.55f,
                    new Vector3(0.6f, 1.1f, delta.magnitude + 0.15f), Quaternion.LookRotation(delta),
                    i % 4 < 2 ? white : orange);
            }
            if (i % 8 == 3) BuildArrow(a, white);
        }
        Transform[] gates = new Transform[GateCount];
        for (int i = 0; i < GateCount; i++)
        {
            float angle = i * Mathf.PI * 2 / GateCount;
            Quaternion heading = Quaternion.LookRotation(Tangent(angle));
            GameObject gate = new GameObject(i == 0 ? "Start Finish" : "Gate " + i);
            gate.transform.SetParent(transform, false);
            gate.transform.SetPositionAndRotation(Center(angle) + Vector3.up, heading);
            gates[i] = gate.transform;
            for (int side = -1; side <= 1; side += 2)
                Box("Gate post", Center(angle) + Side(angle) * side * (roadWidth * 0.5f - 0.5f)
                    + Vector3.up * 2.5f, new Vector3(0.35f, 5, 0.35f), heading, gateMaterial);
            Box("Gate banner", Center(angle) + Vector3.up * 5f,
                new Vector3(roadWidth, 0.5f, 0.3f), heading, gateMaterial);
            GameObject label = new GameObject("Gate label");
            label.transform.SetParent(gate.transform, false);
            label.transform.localPosition = new Vector3(0, 3.4f, -0.3f);
            label.transform.localRotation = Quaternion.identity;
            TextMesh text = label.AddComponent<TextMesh>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.GetComponent<MeshRenderer>().sharedMaterial = text.font.material;
            text.text = i == 0 ? "START / FINISH" : i.ToString();
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.characterSize = i == 0 ? 0.5f : 0.8f;
            text.fontSize = 64;
            text.color = Color.white;
        }
        for (int stripe = 0; stripe < 16; stripe++)
            Box("Start stripe", Center(0) + Side(0) * ((stripe + 0.5f) * roadWidth / 16 - roadWidth * 0.5f)
                + Vector3.up * 0.025f, new Vector3(roadWidth / 16, 0.03f, 1.5f),
                Quaternion.LookRotation(Tangent(0)), stripe % 2 == 0 ? white : asphalt);
        gameObject.AddComponent<CircuitSession>().Initialize(vehicle, gates, roadWidth);
    }

    void BuildRoad(Material material)
    {
        Vector3[] vertices = new Vector3[(Segments + 1) * 2];
        int[] triangles = new int[Segments * 6];
        for (int i = 0; i <= Segments; i++)
        {
            float a = i * Mathf.PI * 2 / Segments;
            vertices[i * 2] = Center(a) - Side(a) * roadWidth * 0.5f;
            vertices[i * 2 + 1] = Center(a) + Side(a) * roadWidth * 0.5f;
            if (i == Segments) continue;
            int v = i * 2; int t = i * 6;
            triangles[t] = v; triangles[t + 1] = v + 2; triangles[t + 2] = v + 1;
            triangles[t + 3] = v + 1; triangles[t + 4] = v + 2; triangles[t + 5] = v + 3;
        }
        Mesh mesh = new Mesh { name = "Circuit road", vertices = vertices, triangles = triangles };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        ownedResources.Add(mesh);
        GameObject road = new GameObject("Road");
        road.transform.SetParent(transform, false);
        road.AddComponent<MeshFilter>().sharedMesh = mesh;
        road.AddComponent<MeshRenderer>().sharedMaterial = material;
        road.AddComponent<MeshCollider>().sharedMesh = mesh;
    }

    void BuildArrow(float angle, Material material)
    {
        Vector3 forward = Tangent(angle);
        Vector3 side = Side(angle);
        Vector3 tip = Center(angle) + Vector3.up * 0.025f + forward * 1.5f;
        foreach (int sign in new[] { -1, 1 })
        {
            Vector3 tail = tip - forward * 2.5f + side * sign * 1.5f;
            Vector3 delta = tip - tail;
            Box("Direction arrow", (tail + tip) * 0.5f, new Vector3(0.25f, 0.03f, delta.magnitude),
                Quaternion.LookRotation(delta), material, false);
        }
    }

    GameObject Box(string objectName, Vector3 position, Vector3 size, Quaternion rotation,
        Material material, bool collidable = true)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = objectName;
        box.transform.SetParent(transform, false);
        box.transform.SetPositionAndRotation(position, rotation);
        box.transform.localScale = size;
        box.GetComponent<Renderer>().sharedMaterial = material;
        if (!collidable) box.GetComponent<Collider>().enabled = false;
        return box;
    }

    Material MakeMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material material = new Material(shader);
        // Explicit URP property: Material.color can target the legacy _Color property.
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        ownedResources.Add(material);
        return material;
    }

    void OnDestroy()
    {
        foreach (Object resource in ownedResources) if (resource != null) Destroy(resource);
    }
}
