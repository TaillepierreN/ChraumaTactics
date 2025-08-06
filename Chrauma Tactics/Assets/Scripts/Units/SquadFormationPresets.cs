using System.Collections.Generic;
using UnityEngine;

public static class SquadFormationPresets
{
    public static List<Vector3> GetFormation(int unitCount)
    {
        switch (unitCount)
        {
            case 1:
                return new List<Vector3> {
                    Vector3.zero
                };
            case 2:
                return new List<Vector3> {
                    new Vector3(-0.5f, 0, 0),
                    new Vector3(0.5f, 0, 0)
                };
            case 4:
                return new List<Vector3> {
                    new Vector3(-0.5f, 0, 0.5f),
                    new Vector3(0.5f, 0, 0.5f),
                    new Vector3(-0.5f, 0, -0.5f),
                    new Vector3(0.5f, 0, -0.5f)
                };
            case 8:
                return new List<Vector3> {
                    new Vector3(-1, 0, 1), new Vector3(0, 0, 1), new Vector3(1, 0, 1),
                    new Vector3(-1, 0, 0),                        new Vector3(1, 0, 0),
                    new Vector3(-1, 0, -1), new Vector3(0, 0, -1), new Vector3(1, 0, -1)
                };
            case 12:
                return new List<Vector3> {
                    new Vector3(-1.5f, 0, 1), new Vector3(-0.5f, 0, 1), new Vector3(0.5f, 0, 1), new Vector3(1.5f, 0, 1),
                    new Vector3(-1.5f, 0, 0), new Vector3(-0.5f, 0, 0), new Vector3(0.5f, 0, 0), new Vector3(1.5f, 0, 0),
                    new Vector3(-1.5f, 0, -1), new Vector3(-0.5f, 0, -1), new Vector3(0.5f, 0, -1), new Vector3(1.5f, 0, -1)
                };
            case 16:
                return new List<Vector3> {
                    new Vector3(-1.5f, 0, 1.5f), new Vector3(-0.5f, 0, 1.5f), new Vector3(0.5f, 0, 1.5f), new Vector3(1.5f, 0, 1.5f),
                    new Vector3(-1.5f, 0, 0.5f), new Vector3(-0.5f, 0, 0.5f), new Vector3(0.5f, 0, 0.5f), new Vector3(1.5f, 0, 0.5f),
                    new Vector3(-1.5f, 0, -0.5f), new Vector3(-0.5f, 0, -0.5f), new Vector3(0.5f, 0, -0.5f), new Vector3(1.5f, 0, -0.5f),
                    new Vector3(-1.5f, 0, -1.5f), new Vector3(-0.5f, 0, -1.5f), new Vector3(0.5f, 0, -1.5f), new Vector3(1.5f, 0, -1.5f)
                };
            default:
                Debug.LogWarning("Unsupported unit count for formation");
                return new List<Vector3>();
        }
    }
}
