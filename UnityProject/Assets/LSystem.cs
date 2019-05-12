// vim: fdc=5
using System;
// using System.Collections;
using System.Collections.Generic;
using UnityEngine;


class Turtle {
    Vector3 position = new Vector3(0, 0, 0);
    Vector3 heading = new Vector3(0, 1, 0);
    Vector3 left = new Vector3(1, 0, 0);
    Vector3 up = new Vector3(0, 0, 1);
    float radius;
    float stepsize;
    float stepangle;
    Action<Vector3, Vector3, Vector3, Vector3, int, float> addface;
    bool inpoly = false;
    Vector3 poly_start;

    int submesh = 0;
    float poly_stepsize;

    int stepped = 0;
    int nsteps_limit;
    int nsteps;

    // Optimization-related
    float nsteps_inv;
    float stepsize_inv;

    public Turtle(float _radius, float _stepsize, float _stepangle, Action<Vector3, Vector3, Vector3, Vector3, int, float> _addface, int _nsteps) {
        radius = _radius;
        stepsize = _stepsize;
        stepangle = _stepangle;
        addface = _addface;
        nsteps = _nsteps;
        nsteps_inv = 1 / ((float)_nsteps);
        stepsize_inv = 1 / ((float)_stepsize);
    }

    public Turtle clone() {
        Turtle newturtle = new Turtle(radius, stepsize, stepangle, addface, nsteps);
        newturtle.position = position;
        newturtle.heading = heading;
        newturtle.left = left;
        newturtle.up = up;
        newturtle.inpoly = inpoly;
        newturtle.poly_start = poly_start;
        newturtle.submesh = submesh;
        newturtle.poly_stepsize = poly_stepsize;
        newturtle.stepped = stepped;
        newturtle.nsteps_limit = nsteps_limit;
        // newturtle.nsteps_inv = nsteps_inv;
        // newturtle.stepsize_inv = stepsize_inv;
        return newturtle;
    }

    public void start(float _radius, int _step_limit) {
        position = new Vector3(0, 0, 0);
        heading = new Vector3(0, 1, 0);
        left = new Vector3(1, 0, 0);
        up = new Vector3(0, 0, 1);
        radius = _radius;
        inpoly = false;

        submesh = 0;
        stepped = 0;
        nsteps_limit = _step_limit;
    }

    Vector3 rodrigues(Vector3 vec, Vector3 axis, float theta) {
        return vec*Mathf.Cos(theta) + Vector3.Cross(axis, vec)*Mathf.Sin(theta) + axis*Vector3.Dot(axis, vec)*(1 - Mathf.Cos(theta));
    }

    public void forward() {
        if (stepped >= nsteps_limit)
            return;

        Vector3 north = position - radius*up;
        Vector3 east = position - radius*left;
        Vector3 south = position + radius*up;
        Vector3 west = position + radius*left;

        float tostep = Mathf.Min(((float) (nsteps_limit - stepped)), (float)nsteps);
        float scalar = stepsize*tostep*nsteps_inv;

        Vector3 direction = scalar*heading;

        addface(north, north+direction, east+direction, east, submesh, tostep*nsteps_inv);
        addface(east, east+direction, south+direction, south, submesh, tostep*nsteps_inv);
        addface(south, south+direction, west+direction, west, submesh, tostep*nsteps_inv);
        addface(west, west+direction, north+direction, north, submesh, tostep*nsteps_inv);

        position += direction;

        stepped += Mathf.RoundToInt(tostep);
    }

    public void forward(float amount) {
        if (stepped >= nsteps_limit)
            return;

        Vector3 north = position - radius*up;
        Vector3 east = position - radius*left;
        Vector3 south = position + radius*up;
        Vector3 west = position + radius*left;

        //int tostep = nsteps_limit - stepped >= nsteps ? nsteps : nsteps_limit - stepped;

        int tostep = Mathf.Min(nsteps_limit - stepped, nsteps);
        float _tostep = (float) tostep;
        float _nsteps = (float) nsteps;
        float scalar = amount*_tostep*nsteps_inv;

        Vector3 direction = scalar*heading;

        addface(north, north+direction, east+direction, east, submesh, _tostep*nsteps_inv);
        addface(east, east+direction, south+direction, south, submesh, _tostep*nsteps_inv);
        addface(south, south+direction, west+direction, west, submesh, _tostep*nsteps_inv);
        addface(west, west+direction, north+direction, north, submesh, _tostep*nsteps_inv);

        position += direction;

        stepped += tostep;
    }

    public void skip() {
        Vector3 newposition = position + poly_stepsize*heading;

        if (inpoly) {
            addface(poly_start, position, newposition, poly_start, submesh, poly_stepsize*stepsize_inv);
            addface(poly_start, newposition, position, poly_start, submesh, poly_stepsize*stepsize_inv);
        }

        position = newposition;
    }

    public void skip(float amount) {
        Vector3 newposition = position + amount*heading;

        if (inpoly) {
            addface(poly_start, position, newposition, poly_start, submesh, amount*stepsize_inv);
            addface(poly_start, newposition, position, poly_start, submesh, amount*stepsize_inv);
        }

        position = newposition;
    }

    public void open() {
        Vector3 north = position - radius*up;
        Vector3 east = position - radius*left;
        Vector3 south = position + radius*up;
        Vector3 west = position + radius*left;

        if (stepped < nsteps_limit) {
            addface(north, east, south, west, submesh, 1);
            //addface(east, north, west, south, submesh, 1);
        }
    }

    public void close() {
        Vector3 north = position - radius*up;
        Vector3 east = position - radius*left;
        Vector3 south = position + radius*up;
        Vector3 west = position + radius*left;

        if (stepped < nsteps_limit) {
            //addface(north, east, south, west, submesh, 1);
            addface(east, north, west, south, submesh, 1);
        }
    }

    public void shoulder(Vector3 newleft, Vector3 newup) {
        if (stepped >= nsteps_limit || inpoly)
            return;

        Vector3 north = position - radius*up;
        Vector3 east = position - radius*left;
        Vector3 south = position + radius*up;
        Vector3 west = position + radius*left;

        Vector3 newnorth = position - radius*newup;
        Vector3 neweast = position - radius*newleft;
        Vector3 newsouth = position + radius*newup;
        Vector3 newwest = position + radius*newleft;

        addface(east, north, newnorth, neweast, submesh, 1);
        addface(south, east, neweast, newsouth, submesh, 1);
        addface(west, south, newsouth, newwest, submesh, 1);
        addface(north, west, newwest, newnorth, submesh, 1);
    }

    public void yaw_right() {
        heading = rodrigues(heading, up, stepangle);
        Vector3 newleft = rodrigues(left, up, stepangle);
        shoulder(newleft, up);
        left = newleft;
    }

    public void yaw_right(float amount) {
        heading = rodrigues(heading, up, amount);
        Vector3 newleft = rodrigues(left, up, amount);
        shoulder(newleft, up);
        left = newleft;
    }

    public void yaw_left() {
        heading = rodrigues(heading, up, -stepangle);
        Vector3 newleft = rodrigues(left, up, -stepangle);
        shoulder(newleft, up);
        left = newleft;
    }

    public void yaw_left(float amount) {
        heading = rodrigues(heading, up, -amount);
        Vector3 newleft = rodrigues(left, up, -amount);
        shoulder(newleft, up);
        left = newleft;
    }

    public void pitch_up() {
        heading = rodrigues(heading, left, stepangle);
        Vector3 newup = rodrigues(up, left, stepangle);
        shoulder(left, newup);
        up = newup;
    }

    public void pitch_up(float amount) {
        heading = rodrigues(heading, left, amount);
        Vector3 newup = rodrigues(up, left, amount);
        shoulder(left, newup);
        up = newup;
    }

    public void pitch_down() {
        heading = rodrigues(heading, left, -stepangle);
        Vector3 newup = rodrigues(up, left, -stepangle);
        shoulder(left, newup);
        up = newup;
    }

    public void pitch_down(float amount) {
        heading = rodrigues(heading, left, -amount);
        Vector3 newup = rodrigues(up, left, -amount);
        shoulder(left, newup);
        up = newup;
    }

    public void roll_right() {
        Vector3 newup = rodrigues(up, heading, stepangle);
        Vector3 newleft = rodrigues(left, heading, stepangle);
        shoulder(newleft, newup);
        up = newup;
        left = newleft;
    }

    public void roll_right(float amount) {
        Vector3 newup = rodrigues(up, heading, amount);
        Vector3 newleft = rodrigues(left, heading, amount);
        shoulder(newleft, newup);
        up = newup;
        left = newleft;
    }

    public void roll_left() {
        Vector3 newup = rodrigues(up, heading, -stepangle);
        Vector3 newleft = rodrigues(left, heading, -stepangle);
        shoulder(newleft, newup);
        up = newup;
        left = newleft;
    }

    public void roll_left(float amount) {
        Vector3 newup = rodrigues(up, heading, -amount);
        Vector3 newleft = rodrigues(left, heading, -amount);
        shoulder(newleft, newup);
        up = newup;
        left = newleft;
    }

    public void set_radius(float newradius) {
        radius = newradius;
    }

    public void set_submesh(int newsubmesh) {
        submesh = newsubmesh;
    }

    public void start_poly() {
        if (stepped >= nsteps_limit)
            return;

        inpoly = true;
        poly_start = position;

        int tostep = Mathf.Min(nsteps_limit - stepped, nsteps);
        float _tostep = (float) tostep;
        float _nsteps = (float) nsteps;
        poly_stepsize = stepsize*_tostep*nsteps_inv;
    }

    public void end_poly() {
        inpoly = false;
    }
}

//[Serializable]
public class Rule {
    public float probability = 1;
    public string primary;
    public Func<Instruction, List<Instruction>> secondary;

    public Rule(string _primary, Func<Instruction, List<Instruction>> _secondary) {
        probability = 1;
        primary = _primary;
        secondary = _secondary;
    }

    public Rule(float _probability, string _primary, Func<Instruction, List<Instruction>> _secondary) {
        probability = _probability;
        primary = _primary;
        secondary = _secondary;
    }
}

//[Serializable]
public class Instruction {
    public string id;
    public float param0 = float.NaN;
    public float param1 = float.NaN;

    public Instruction(string _id) {
        id = _id;
    }

    public Instruction(string _id, float _param0) {
        id = _id;
        param0 = _param0;
    }

    public Instruction(string _id, float _param0, float _param1) {
        id = _id;
        param0 = _param0;
        param1 = _param1;
    }
}

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class LSystem : MonoBehaviour {
    public List<Instruction> axiom = new List<Instruction>();
    public List<Rule> rules = new List<Rule>();
    public int depth = 1;
    public int nsteps = 60;

    public float stepsize;
    public float stepangle;
    public float radius;

    public int step = 0;

    List<Vector3> vertices = new List<Vector3>();
    List<List<int>> triangles = new List<List<int>>();
    List<Vector2> uvs = new List<Vector2>();

    Turtle turtle;
    List<Turtle> stack = new List<Turtle>();

    List<Instruction> instructions;

    MeshFilter mf;
    MeshRenderer mr;
    Mesh mesh;

    int nforwards;

    List<Instruction> lsystem_evolve(List<Instruction> axiom, int depth) {
        if (depth == 0)
            return axiom;

        List<Instruction> newaxiom = new List<Instruction>();

        List<Rule> matched_rules = new List<Rule>();
        float rsum;
        float rand;
        foreach (Instruction c in axiom) {
            matched_rules.Clear();
            foreach (Rule rule in rules) {
                if (c.id == rule.primary) {
                    matched_rules.Add(rule);
                }
            }
            rsum = 0;
            rand = UnityEngine.Random.value;
            foreach (Rule rule in matched_rules) {
                rsum += rule.probability;
                if (rand <= rsum) {
                    newaxiom.AddRange(rule.secondary(c));
                    goto next_symbol;
                }
            }
            newaxiom.Add(c);
next_symbol:
            continue;
        }

        return lsystem_evolve(newaxiom, depth-1);
    }

    void doturtle(List<Instruction> instructions) {
        turtle.start(radius, step++);

        foreach (Instruction c in instructions) {
            switch (c.id) {
                case "F":
                    if (float.IsNaN(c.param0))
                        turtle.forward();
                    else
                        turtle.forward(c.param0);
                    break;
                case "T":
                    if (float.IsNaN(c.param0))
                        turtle.forward();
                    else
                        turtle.forward(c.param0);
                    break;
                case "f":
                    if (float.IsNaN(c.param0))
                        turtle.skip();
                    else
                        turtle.skip(c.param0);
                    break;
                case "+":
                    if (float.IsNaN(c.param0))
                        turtle.yaw_left();
                    else
                        turtle.yaw_left(c.param0);
                    break;
                case "-":
                    if (float.IsNaN(c.param0))
                        turtle.yaw_right();
                    else
                        turtle.yaw_right(c.param0);
                    break;
                case "&":
                    if (float.IsNaN(c.param0))
                        turtle.pitch_down();
                    else
                        turtle.pitch_down(c.param0);
                    break;
                case "^":
                    if (float.IsNaN(c.param0))
                        turtle.pitch_up();
                    else
                        turtle.pitch_up(c.param0);
                    break;
                case "\\":
                    if (float.IsNaN(c.param0))
                        turtle.roll_left();
                    else
                        turtle.roll_left(c.param0);
                    break;
                case "/":
                    if (float.IsNaN(c.param0))
                        turtle.roll_right();
                    else
                        turtle.roll_right(c.param0);
                    break;
                case "$":
                    turtle.open();
                    break;
                case "%":
                    turtle.close();
                    break;
                case "[":
                    stack.Add(turtle.clone());
                    break;
                case "]":
                    turtle = stack[stack.Count-1];
                    stack.RemoveAt(stack.Count-1);
                    break;
                case "{":
                    turtle.start_poly();
                    break;
                case "}":
                    turtle.end_poly();
                    break;
                case "!":
                    turtle.set_radius(c.param0);
                    break;
                case "0":
                    turtle.set_submesh(0);
                    break;
                case "1":
                    turtle.set_submesh(1);
                    break;
                case "2":
                    turtle.set_submesh(2);
                    break;
                case "3":
                    turtle.set_submesh(3);
                    break;
                case "4":
                    turtle.set_submesh(4);
                    break;
                case "5":
                    turtle.set_submesh(5);
                    break;
                case "6":
                    turtle.set_submesh(6);
                    break;
                case "7":
                    turtle.set_submesh(7);
                    break;
                case "8":
                    turtle.set_submesh(8);
                    break;
                case "9":
                    turtle.set_submesh(9);
                    break;
            }
        }
    }

    void addface(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, int submesh, float uv_percent) {
        int i = vertices.Count;
        vertices.Add(v0);
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);


        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(uv_percent, 0));
        uvs.Add(new Vector2(uv_percent, 1));
        uvs.Add(new Vector2(0, 1));

        List<int> trianglesi = triangles[submesh];

        trianglesi.Add(i+0);
        trianglesi.Add(i+2);
        trianglesi.Add(i+1);

        trianglesi.Add(i+3);
        trianglesi.Add(i+2);
        trianglesi.Add(i+0);
    }

    int countForwards(List<Instruction> instructions) {
        List<int> counters = new List<int>();
        counters.Add(0);
        List<Instruction> bracketed = new List<Instruction>();
        int level = 0;

        foreach (Instruction c in instructions) {
            if (c.id == "]") {
                level--;
                if (level == 0) {
                    counters.Add(counters[0] + countForwards(bracketed));
                    bracketed.Clear();
                    continue;
                }
            }

            if (level > 0)
                bracketed.Add(c);

            if (c.id == "[")
                level++;
            if (level == 0 && c.id == "F")
                counters[0]++;
        }

        int maxcounter = 0;
        foreach (int counter in counters)
            maxcounter = counter > maxcounter ? counter : maxcounter;

        return maxcounter;
    }

    void Start() {
        // Get the Mesh and MeshFilter
        mf = GetComponent<MeshFilter>();
        mesh = mf.mesh;
        mr = GetComponent<MeshRenderer>();

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        instructions = lsystem_evolve(axiom, depth);
        nforwards = countForwards(instructions);
        turtle = new Turtle(radius, stepsize, Mathf.Deg2Rad*stepangle, addface, nsteps);

        for (int i = 0; i < mr.materials.Length; i++)
            triangles.Add(new List<int>());
    }

    void Update() {
        if (step < 0) {
            step++;
            return;
        }

        if (step > nforwards*nsteps)
            return;

        // Clear the Mesh
        UnityEngine.Profiling.Profiler.BeginSample("do Clears");
        mesh.Clear();
        vertices.Clear();
        uvs.Clear();
        foreach (List<int> trianglesi in triangles)
            trianglesi.Clear();
        UnityEngine.Profiling.Profiler.EndSample();
        mesh.subMeshCount = mr.materials.Length;
        // Run a turtle
        UnityEngine.Profiling.Profiler.BeginSample("doturtle");
        doturtle(instructions);
        UnityEngine.Profiling.Profiler.EndSample();

        // Set the mesh
        UnityEngine.Profiling.Profiler.BeginSample("Set Mesh");
        mesh.SetVertices(vertices);
        for (int i = 0; i < triangles.Count; i++)
            mesh.SetTriangles(triangles[i], i);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        UnityEngine.Profiling.Profiler.EndSample();
    }
}
