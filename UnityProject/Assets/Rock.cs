//using System.Collections;
using System.Collections.Generic;
using UnityEngine;


class Metaball {
    public Vector3 position;
    public float max_radius;
    public Vector3 invradii;

    public Metaball(float x, float y, float z, float radius_x, float radius_y, float radius_z) {
        position = new Vector3(x, y, z);
        max_radius = Mathf.Max(radius_x, radius_y, radius_z);
        invradii = new Vector3(1/radius_x, 1/radius_y, 1/radius_z);
    }
}

class Interval {
    public float inf;
    public float sup;

    public Interval(float _inf, float _sup) {
        inf = _inf;
        sup = _sup;
    }

    public Interval(float x) {
        inf = x;
        sup = x;
    }

    public static Interval operator +(Interval self, Interval other) {
        return new Interval(self.inf + other.inf, self.sup + other.sup);
    }

    public static Interval operator +(float self, Interval other) {
        return new Interval(self) + other;
    }

    public static Interval operator +(Interval self, float other) {
        return self + new Interval(other);
    }

    public static Interval operator -(Interval self, Interval other) {
        return new Interval(self.inf - other.sup, self.sup - other.inf);
    }

    public static Interval operator -(float self, Interval other) {
        return new Interval(self) - other;
    }

    public static Interval operator -(Interval self, float other) {
        return self - new Interval(other);
    }

    public static Interval operator *(Interval self, Interval other) {
        float a = self.inf * other.inf;
        float b = self.inf * other.sup;
        float c = self.sup * other.inf;
        float d = self.sup * other.sup;
        return new Interval(Mathf.Min(a, b, c, d), Mathf.Max(a, b, c, d));
    }

    public static Interval operator *(float self, Interval other) {
        return new Interval(self) * other;
    }

    public static Interval operator *(Interval self, float other) {
        return self * new Interval(other);
    }

    public static Interval operator /(Interval self, Interval other) {
        float a = self.inf / other.inf;
        float b = self.inf / other.sup;
        float c = self.sup / other.inf;
        float d = self.sup / other.sup;
        return new Interval(Mathf.Min(a, b, c, d), Mathf.Max(a, b, c, d));
    }

    public static Interval operator /(float self, Interval other) {
        return new Interval(self) / other;
    }

    public static Interval operator /(Interval self, float other) {
        return self / new Interval(other);
    }

    public Interval Exp() {
        return new Interval(Mathf.Exp(inf), Mathf.Exp(sup));
    }

    public bool contains(float val) {
        return inf <= val && val <= sup;
    }

    public float half() {
        return 0.5f*(inf + sup);
    }

    public Interval first() {
        return new Interval(inf, half());
    }

    public Interval second() {
        return new Interval(half(), sup);
    }
}

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Rock : MonoBehaviour {
    public int depth;
    public int nballs;
    public Vector3 minradii;
    public Vector3 maxradii;

    MeshFilter mf;
    Mesh mesh;

    List<Metaball> metaballs = new List<Metaball>();

    // C# seems to force me to put this into a class...
    MarchingCubesLookupTable lookup = new MarchingCubesLookupTable();

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    float FieldFunction(float d, float r) {
        return Mathf.Exp(-r * d);
    }

    Interval FieldFunction(Interval d, float r) {
        return (-r * d).Exp();
    }

    float MetaballDist2(Metaball metaball, Vector3 position) {
        Vector3 v = Vector3.Scale(position, metaball.invradii) - metaball.position;
        return v.x*v.x + v.y*v.y + v.z*v.z;
    }

    Interval MetaballDist2(Metaball metaball, Interval x, Interval y, Interval z) {
        Interval _x = x*metaball.invradii.x - metaball.position.x;
        Interval _y = y*metaball.invradii.y - metaball.position.y;
        Interval _z = z*metaball.invradii.z - metaball.position.z;
        return _x*_x + _y*_y + _z*_z;
    }

    float Sample(Vector3 position) {
        float output = 0;

        foreach (Metaball metaball in metaballs)
            output += FieldFunction(MetaballDist2(metaball, position), metaball.max_radius);

        return output - 1;
    }

    Interval Sample(Interval x, Interval y, Interval z) {
        Interval output = new Interval(0);

        foreach (Metaball metaball in metaballs)
            output += FieldFunction(MetaballDist2(metaball, x, y, z), metaball.max_radius);

        return output - 1;
    }

    void GenerateMetaballs() {
        float radiusx, radiusy, radiusz;
        float x, y, z;
        for (int i = 0; i < nballs; i++) {
            radiusx = Random.Range(minradii.x, maxradii.x);
            radiusy = Random.Range(minradii.y, maxradii.y);
            radiusz = Random.Range(minradii.z, maxradii.z);
            x = Random.Range(radiusx - maxradii.x, maxradii.x - radiusx);
            y = Random.Range(radiusy - maxradii.y, maxradii.y - radiusy);
            z = Random.Range(radiusz - maxradii.z, maxradii.z - radiusz);

            metaballs.Add(new Metaball(x, y, z, radiusx, radiusy, radiusz));
        }
    }

    void MarchingCubes(Interval x, Interval y, Interval z, int _depth) {
        Vector3 Vert2Coord(int i) {
            switch (i) {
                case 0:
                    return new Vector3(x.inf, y.inf, z.inf);
                case 1:
                    return new Vector3(x.sup, y.inf, z.inf);
                case 2:
                    return new Vector3(x.sup, y.sup, z.inf);
                case 3:
                    return new Vector3(x.inf, y.sup, z.inf);
                case 4:
                    return new Vector3(x.inf, y.inf, z.sup);
                case 5:
                    return new Vector3(x.sup, y.inf, z.sup);
                case 6:
                    return new Vector3(x.sup, y.sup, z.sup);
                case 7:
                    return new Vector3(x.inf, y.sup, z.sup);
                default:
                    return new Vector3(float.NaN, float.NaN, float.NaN);
            }
        }
        (int, int) Edge2Verts(int i) {
            switch (i) {
                case 0:
                    return (0, 1);
                case 1:
                    return (1, 2);
                case 2:
                    return (2, 3);
                case 3:
                    return (3, 0);
                case 4:
                    return (4, 5);
                case 5:
                    return (5, 6);
                case 6:
                    return (6, 7);
                case 7:
                    return (7, 4);
                case 8:
                    return (0, 4);
                case 9:
                    return (1, 5);
                case 10:
                    return (2, 6);
                case 11:
                    return (3, 7);
                default:
                    return (-1, -1);
            }
        }
        (Vector3, Vector3) Edge2Coords(int i) {
            (int v0, int v1) = Edge2Verts(i);
            return (Vert2Coord(v0), Vert2Coord(v1));
        }
        float LinearInterpolant(float fp, float fq) {
            return -fp / (fq - fp);
        }

        Interval fX = Sample(x, y, z);
        if (!fX.contains(0))
            return;

        if (_depth >= depth) {
            bool f0 = Sample(new Vector3(x.inf, y.inf, z.inf)) >= 0;
            bool f1 = Sample(new Vector3(x.sup, y.inf, z.inf)) >= 0;
            bool f2 = Sample(new Vector3(x.sup, y.sup, z.inf)) >= 0;
            bool f3 = Sample(new Vector3(x.inf, y.sup, z.inf)) >= 0;
            bool f4 = Sample(new Vector3(x.inf, y.inf, z.sup)) >= 0;
            bool f5 = Sample(new Vector3(x.sup, y.inf, z.sup)) >= 0;
            bool f6 = Sample(new Vector3(x.sup, y.sup, z.sup)) >= 0;
            bool f7 = Sample(new Vector3(x.inf, y.sup, z.sup)) >= 0;
            // Check lookup table
            int index = 0;
            if (f0) index +=   1; // 2^0
            if (f1) index +=   2; // 2^1
            if (f2) index +=   4; // 2^2
            if (f3) index +=   8; // 2^3
            if (f4) index +=  16; // 2^4
            if (f5) index +=  32; // 2^5
            if (f6) index +=  64; // 2^6
            if (f7) index += 128; // 2^7
            (int, int, int)[] from_table = lookup.lookup_table[index];

            Vector3 v00,  v01, v10, v11, v20, v21;
            int nverts = vertices.Count;
            foreach ((int edge0, int edge1, int edge2) in from_table) {
                if (edge0 == -1 || edge1 == -1 || edge2 == -1)
                    break;
                // Unpack
                (v00, v01) = Edge2Coords(edge0);
                (v10, v11) = Edge2Coords(edge1);
                (v20, v21) = Edge2Coords(edge2);
                // Add vertices (with Lerp)
                vertices.Add(Vector3.Lerp(v20, v21, LinearInterpolant(Sample(v20), Sample(v21))));
                vertices.Add(Vector3.Lerp(v10, v11, LinearInterpolant(Sample(v10), Sample(v11))));
                vertices.Add(Vector3.Lerp(v00, v01, LinearInterpolant(Sample(v00), Sample(v01))));
                // Set UV coordinates
                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(0, 1));
                uvs.Add(new Vector2(1, 1));
                // Add triangle
                triangles.Add(nverts++);
                triangles.Add(nverts++);
                triangles.Add(nverts++);
            }

            return;
        }

        // Subdivide
        _depth++;
        MarchingCubes( x.first(),  y.first(),  z.first(), _depth);
        MarchingCubes( x.first(),  y.first(), z.second(), _depth);
        MarchingCubes( x.first(), y.second(),  z.first(), _depth);
        MarchingCubes( x.first(), y.second(), z.second(), _depth);
        MarchingCubes(x.second(),  y.first(),  z.first(), _depth);
        MarchingCubes(x.second(),  y.first(), z.second(), _depth);
        MarchingCubes(x.second(), y.second(),  z.first(), _depth);
        MarchingCubes(x.second(), y.second(), z.second(), _depth);
    }

    void Awake() {
        mf = GetComponent<MeshFilter>();
        mesh = mf.mesh;

        GenerateMetaballs();
        MarchingCubes(new Interval(-2, 2), new Interval(-2, 2), new Interval(-2, 2), 0);

        mesh.Clear();

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0, true);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
    }
}
