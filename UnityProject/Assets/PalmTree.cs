//using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LSystem))]
public class PalmTree : MonoBehaviour
{
    public int depth;
    public float stepsize;
    public float stepangle;
    public float trunk_radius;
    public float leaves_radius;

    public float trunk_length;
    public float nleaves;
    public float total_trunk_theta;

    List<float> leaves_thetas = new List<float>();

    LSystem lsystem;

    List<Instruction> generateLeaves(Instruction s) {
        List<Instruction> output = new List<Instruction>();

        output.Add(new Instruction("&", 0.5f * Mathf.PI));
        output.Add(new Instruction("!", 2f*leaves_radius));
        foreach (float theta in leaves_thetas) {
            output.Add(new Instruction("-", theta));
            output.Add(new Instruction("1"));
            output.Add(new Instruction("[", theta));
            output.Add(new Instruction("initialL", theta));
            output.Add(new Instruction("%"));
            output.Add(new Instruction("]", theta));
        }

        return output;
    }

    void Awake() {
        lsystem = GetComponent<LSystem>();

        lsystem.depth = 5;
        lsystem.stepsize = stepsize;
        lsystem.stepangle = stepangle;
        lsystem.radius = trunk_radius;

        // Axiom
        lsystem.axiom.Add(new Instruction("0"));
        lsystem.axiom.Add(new Instruction("T", trunk_length));
        lsystem.axiom.Add(new Instruction("C", nleaves));
        bool ok = false;
        const float tol = 0.5f;
        float newtheta = float.NaN;
        for (int i = 0; i < nleaves; i++) {
            while (!ok) {
                newtheta = Random.Range(0, 2*Mathf.PI);
                ok = true;
                foreach (float theta in leaves_thetas) {
                    if (Mathf.Abs(theta - newtheta) <= tol) {
                        ok = false;
                        break;
                    }
                }
            }
            leaves_thetas.Add(newtheta);
        }

        // Rules
        lsystem.rules.Add(new Rule(1, "T", s => new List<Instruction>() {
            new Instruction("T", 0.5f*s.param0),
            new Instruction("-", Mathf.Deg2Rad*total_trunk_theta / Mathf.Pow(2, depth)),
            new Instruction("T", 0.5f*s.param0),
        } ));
        lsystem.rules.Add(new Rule(1, "C", generateLeaves));
        lsystem.rules.Add(new Rule(0.33f, "initialL", s => new List<Instruction>() {
            new Instruction("&"),
            new Instruction("F"),
            new Instruction("["),
            new Instruction("+"),
            new Instruction("+"),
            new Instruction("+"),
            new Instruction("!", leaves_radius),
            new Instruction("F"),
            new Instruction("%"),
            new Instruction("]"),
            new Instruction("["),
            new Instruction("-"),
            new Instruction("-"),
            new Instruction("-"),
            new Instruction("!", leaves_radius),
            new Instruction("F"),
            new Instruction("%"),
            new Instruction("]"),
            new Instruction("L"),
        } ));
        lsystem.rules.Add(new Rule(0.33f, "initialL", s => new List<Instruction>() {
            new Instruction("^"),
            new Instruction("F"),
            new Instruction("["),
            new Instruction("+"),
            new Instruction("+"),
            new Instruction("+"),
            new Instruction("!", leaves_radius),
            new Instruction("F"),
            new Instruction("%"),
            new Instruction("]"),
            new Instruction("["),
            new Instruction("-"),
            new Instruction("-"),
            new Instruction("-"),
            new Instruction("!", leaves_radius),
            new Instruction("F"),
            new Instruction("%"),
            new Instruction("]"),
            new Instruction("L"),
        } ));
        lsystem.rules.Add(new Rule(0.34f, "initialL", s => new List<Instruction>() {
            new Instruction("^"),
            new Instruction("^"),
            new Instruction("^"),
            new Instruction("L"),
        } ));
        lsystem.rules.Add(new Rule(1, "L", s => new List<Instruction>() {
            new Instruction("&"),
            new Instruction("F"),
            new Instruction("["),
            new Instruction("+"),
            new Instruction("+"),
            new Instruction("+"),
            new Instruction("!", leaves_radius),
            new Instruction("F"),
            new Instruction("%"),
            new Instruction("]"),
            new Instruction("["),
            new Instruction("-"),
            new Instruction("-"),
            new Instruction("-"),
            new Instruction("!", leaves_radius),
            new Instruction("F"),
            new Instruction("%"),
            new Instruction("]"),
            new Instruction("L"),
        }));
    }

    // Update is called once per frame
    void Update() {
    }
}
