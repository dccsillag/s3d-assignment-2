//using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Vase : MonoBehaviour {
    public int degree = 3;
    public float initial_radius = 3;
    public float radiusmin = -3;
    public float radiusmax =  3;
    public float heightmin = 2;
    public float heightmax = 4;
    public int nrotations = 60;
    public int nts = 40;

    public List<Vector2> points;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    MeshFilter mf;
    Mesh mesh;

    public int Factorial(int n) {
        int output = 1;
        for (int i = 2; i <= n; i++)
            output *= i;
        return output;
    }

    public int Choose(int n, int k) {
        return Factorial(n) / (Factorial(n - k) * Factorial(k));
    }

    public Vector2 Bezier(float t) {
        Vector2 output = Vector2.zero;

        for (int i = 0; i <= degree; i++) {
            output += Choose(degree, i) * Mathf.Pow(1-t, degree-i) * Mathf.Pow(t, i) * points[i];
        }

        return output;
    }

    void Start() {
        mf = GetComponent<MeshFilter>();
        mesh = mf.mesh;

        points.Add(Vector2.zero);
        points.Add(new Vector2(initial_radius, 0));
        for (int i = 0; i < degree-1; i++)
            points.Add(points[points.Count-1] + new Vector2(Random.Range(radiusmin, radiusmax), Random.Range(heightmin, heightmax)));

        int index = 0;
        float total_u_inv = 1 / (2*Mathf.PI);
        float u_step = 2*Mathf.PI / nrotations;
        float v_step = 1 / ((float)nts);
        Vector2 b0, b1;
        for (float u = 0; u < 2*Mathf.PI; u += u_step) {
            for (float v = 0; v < 1; v += v_step) {
                b0 = Bezier(v);
                b1 = Bezier(v+v_step);

                vertices.Add(       new Vector3(b0.x * Mathf.Cos(u), b0.y,        b0.x * Mathf.Sin(u)));
                vertices.Add(new Vector3(b0.x * Mathf.Cos(u+u_step), b0.y, b0.x * Mathf.Sin(u+u_step)));
                vertices.Add(new Vector3(b1.x * Mathf.Cos(u+u_step), b1.y, b1.x * Mathf.Sin(u+u_step)));
                vertices.Add(       new Vector3(b1.x * Mathf.Cos(u), b1.y,        b1.x * Mathf.Sin(u)));

                vertices.Add(       new Vector3(b1.x * Mathf.Cos(u), b1.y,        b1.x * Mathf.Sin(u)));
                vertices.Add(new Vector3(b1.x * Mathf.Cos(u+u_step), b1.y, b1.x * Mathf.Sin(u+u_step)));
                vertices.Add(new Vector3(b0.x * Mathf.Cos(u+u_step), b0.y, b0.x * Mathf.Sin(u+u_step)));
                vertices.Add(       new Vector3(b0.x * Mathf.Cos(u), b0.y,        b0.x * Mathf.Sin(u)));

                uvs.Add(new Vector2((       u)*total_u_inv,        v));
                uvs.Add(new Vector2((u+u_step)*total_u_inv,        v));
                uvs.Add(new Vector2((u+u_step)*total_u_inv, v+v_step));
                uvs.Add(new Vector2((       u)*total_u_inv, v+v_step));

                uvs.Add(new Vector2((       u)*total_u_inv, v+v_step));
                uvs.Add(new Vector2((u+u_step)*total_u_inv, v+v_step));
                uvs.Add(new Vector2((u+u_step)*total_u_inv,        v));
                uvs.Add(new Vector2((       u)*total_u_inv,        v));

                triangles.Add(index+0);
                triangles.Add(index+1);
                triangles.Add(index+2);

                triangles.Add(index+2);
                triangles.Add(index+3);
                triangles.Add(index+0);

                triangles.Add(index+4);
                triangles.Add(index+5);
                triangles.Add(index+6);

                triangles.Add(index+6);
                triangles.Add(index+7);
                triangles.Add(index+4);

                index += 8;
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0, true);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
    }
}
