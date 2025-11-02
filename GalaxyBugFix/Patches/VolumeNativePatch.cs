using AIGraph;
using BepInEx.Unity.IL2CPP.Hook;
using GTFO.API;
using Il2CppInterop.Runtime.Runtime;
using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace GalaxyBugFix.Patches
{
    internal static class VolumeNativePatch
    {
        private const float EarlyExitValidDist = 1f;
        private static INativeDetour? TryGetVoxelNodeDetour;
        private static d_TryGetVoxelNode? orig_TryGetVoxelNode;
        private unsafe delegate bool d_TryGetVoxelNode(IntPtr _this, Vector3 pos, out IntPtr node, Il2CppMethodInfo* methodInfo);

        public unsafe static void Init()
        {
            TryGetVoxelNodeDetour = INativeDetour.CreateAndApply(
                (nint)Il2CppAPI.GetIl2CppMethod<AIG_VoxelNodeVolume>(
                    nameof(AIG_VoxelNodeVolume.TryGetCloseVoxelNode),
                    typeof(bool).Name,
                    false,
                    new[] {
                        typeof(Vector3).FullName,
                        typeof(AIG_INode).MakeByRefType().FullName
                    }),
                TryGetCloseVoxelNodePatch,
                out orig_TryGetVoxelNode
                );
        }

        // Copy/paste TryGetCloseVoxelNode but get the closest node instead of first viable node
        private unsafe static bool TryGetCloseVoxelNodePatch(IntPtr _this, Vector3 pos, [MaybeNullWhen(false)] out IntPtr nodePtr, Il2CppMethodInfo* methodInfo)
        {
            var volume = new AIG_VoxelNodeVolume(_this);
            float minHeight = pos.y - 0.5f;
            float maxHeight = pos.y + 0.5f;
            volume.GetGridPosition(pos, out var x, out var z);

            float nodeBestDist = float.MaxValue;
            AIG_INode? nodeBest = null;
            if (volume.TryGetPillar(x, z, out var pillar) && TryGetVoxelNode(pillar, pos, minHeight, maxHeight, out var node) && UseBestCheckExit(node.Position - pos, node, ref nodeBest, ref nodeBestDist))
            {
                nodePtr = nodeBest.Pointer;
                return true;
            }
            if (volume.TryGetPillar(x + 1, z, out pillar) && TryGetVoxelNode(pillar, pos, minHeight, maxHeight, out node) && UseBestCheckExit(node.Position - pos, node, ref nodeBest, ref nodeBestDist))
            {
                nodePtr = nodeBest.Pointer;
                return true;
            }
            if (volume.TryGetPillar(x - 1, z, out pillar) && TryGetVoxelNode(pillar, pos, minHeight, maxHeight, out node) && UseBestCheckExit(node.Position - pos, node, ref nodeBest, ref nodeBestDist))
            {
                nodePtr = nodeBest.Pointer;
                return true;
            }
            if (volume.TryGetPillar(x, z + 1, out pillar) && TryGetVoxelNode(pillar, pos, minHeight, maxHeight, out node) && UseBestCheckExit(node.Position - pos, node, ref nodeBest, ref nodeBestDist))
            {
                nodePtr = nodeBest.Pointer;
                return true;
            }
            if (volume.TryGetPillar(x, z - 1, out pillar) && TryGetVoxelNode(pillar, pos, minHeight, maxHeight, out node) && UseBestCheckExit(node.Position - pos, node, ref nodeBest, ref nodeBestDist))
            {
                nodePtr = nodeBest.Pointer;
                return true;
            }
            if (volume.TryGetPillar(x + 1, z + 1, out pillar) && TryGetVoxelNode(pillar, pos, minHeight, maxHeight, out node) && UseBestCheckExit(node.Position - pos, node, ref nodeBest, ref nodeBestDist))
            {
                nodePtr = nodeBest.Pointer;
                return true;
            }
            if (volume.TryGetPillar(x + 1, z - 1, out pillar) && TryGetVoxelNode(pillar, pos, minHeight, maxHeight, out node) && UseBestCheckExit(node.Position - pos, node, ref nodeBest, ref nodeBestDist))
            {
                nodePtr = nodeBest.Pointer;
                return true;
            }
            if (volume.TryGetPillar(x - 1, z + 1, out pillar) && TryGetVoxelNode(pillar, pos, minHeight, maxHeight, out node) && UseBestCheckExit(node.Position - pos, node, ref nodeBest, ref nodeBestDist))
            {
                nodePtr = nodeBest.Pointer;
                return true;
            }
            if (volume.TryGetPillar(x - 1, z - 1, out pillar) && TryGetVoxelNode(pillar, pos, minHeight, maxHeight, out node) && UseBestCheckExit(node.Position - pos, node, ref nodeBest, ref nodeBestDist))
            {
                nodePtr = nodeBest.Pointer;
                return true;
            }

            nodePtr = nodeBest?.Pointer ?? IntPtr.Zero;
            return nodePtr != IntPtr.Zero;
        }

        private static bool TryGetVoxelNode(AIG_VoxelNodePillar pillar, Vector3 orig, float minHeight, float maxHeight, [MaybeNullWhen(false)] out AIG_INode node)
        {
            var nodes = pillar.m_nodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                var aiNode = nodes[i];
                var pos = aiNode.Position;
                if (pos.y >= minHeight && pos.y <= maxHeight)
                {
                    node = aiNode.Cast<AIG_INode>();
                    return true;
                }
            }
            node = null;
            return false;
        }

        private static bool UseBestCheckExit<T>(Vector3 diff, T candidate, [NotNullWhen(true)] ref T best, ref float bestDistSqr)
        {
            float sqrDist = diff.sqrMagnitude;
            if (sqrDist <= EarlyExitValidDist)
            {
                best = candidate!;
                return true;
            }

            if (sqrDist < bestDistSqr)
            {
                best = candidate;
                bestDistSqr = sqrDist;
            }
            return false;
        }
    }
}
