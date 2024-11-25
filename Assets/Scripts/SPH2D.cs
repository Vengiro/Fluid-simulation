using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

[System.Serializable]
public class SPH2D : MonoBehaviour
{
    

    
    public Vector2Int numToSpawn = new Vector2Int(10, 10);
    public float particleRadius = 0.1f;
    public Vector2 spawnBoxCenter = new Vector2(0, 4);
    public Vector2 spawnBox = new Vector2(4, 4);

    public Shader particleShader;
    private Material particleMaterial;

    private ComputeBuffer particleBuffer;
    private static readonly int ParticlesBufferProperty = Shader.PropertyToID("_ParticlesBuffer");

    void Start()
    {
        Vector2 spawnTopLeft = spawnBoxCenter - spawnBox / 2;

        // Generate particles
        for (int x = 0; x < numToSpawn.x; x++)
        {
            for (int y = 0; y < numToSpawn.y; y++)
            {
                Vector2 spawnPosition = spawnTopLeft + new Vector2(x * particleRadius * 2, y * particleRadius * 2);
               
            }
        }

        // Create the compute buffer for particles

        // Set up the material using the shader
        particleMaterial = new Material(particleShader);
    }

    void Update()
    {
        // Render all particles using the custom shader
    }

    void OnDestroy()
    {
        // Release the compute buffer
        if (particleBuffer != null)
        {
            particleBuffer.Release();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(spawnBoxCenter, spawnBox);


        Gizmos.color = particleColor;
        
        
    }
  
    public Color particleColor = Color.red;
    private void DrawCircle(Vector2 center, float radius, Color color) {
        int segments = 20;
        float angle = 0f;
        Gizmos.color = color;

        Vector3 lastPoint = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        for (int i = 1; i <= segments; i++) {
            angle += 2 * Mathf.PI / segments;
            Vector3 newPoint = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            Gizmos.DrawLine(lastPoint, newPoint);
            lastPoint = newPoint;
        }
    }

}
