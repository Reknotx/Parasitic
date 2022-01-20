using UnityEngine;

public static class GridGenerator
{
    public static GridMesh GenerateGrid(int columns, int rows, float height, float scale)
    {
        GridMesh gridMesh = new GridMesh(columns, rows);
        int currentVert = 0;
        for (int z = 0; z < rows+1; z++)
        {
            for (int x = 0; x < columns+1; x++)
            {
                gridMesh.verts[currentVert] = new Vector3(x*scale, height, z*scale);
                gridMesh.uvs[currentVert] = new Vector2(x / ((float)columns + 1f), z / ((float)rows + 1f));

                if ((x < columns) && (z < rows))
                {
                    gridMesh.AddTri(currentVert, currentVert + columns + 1, currentVert + 1);
                    gridMesh.AddTri(currentVert + columns + 1, currentVert + columns + 2, currentVert + 1);
                }
                currentVert++;
            }
        }
        return gridMesh;
    }
}

public class GridMesh
{
    public Vector3[] verts;
    public int[] tris;
    public Vector2[] uvs;
    int triIndex;

    public GridMesh(int width, int height)
    {
        verts = new Vector3[(width + 1) * (height + 1)];
        tris = new int[width * height * 6];
        uvs = new Vector2[(width + 1) * (height + 1)];
    }

    public void AddTri(int a, int b, int c)
    {
        tris[triIndex] = a;
        tris[triIndex + 1] = b;
        tris[triIndex + 2] = c;
        triIndex += 3;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }
}
