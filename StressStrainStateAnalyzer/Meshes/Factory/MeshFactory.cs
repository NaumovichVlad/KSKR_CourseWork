﻿using StressStrainStateAnalyzer.FiniteElements;
using StressStrainStateAnalyzer.MeshBulders;
using StressStrainStateAnalyzer.Nodes;

namespace StressStrainStateAnalyzer.Meshes.Factory
{
    public static class MeshFactory
    {
        public static Mesh? GetMesh(FiniteElementsTypes type, List<INode> nodes, double maxElementSquare, double minAngle)
        {
            Mesh mesh;
            switch (type)
            {
                case FiniteElementsTypes.Triangular:
                    mesh = new Mesh(new TriangleMeshBuilder(), nodes, maxElementSquare, minAngle);
                    break;
                default:
                    mesh = null;
                    break;
            }
            return mesh;
        }
    }
}
