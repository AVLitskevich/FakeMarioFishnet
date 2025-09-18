using UnityEngine;
using UnityEngine.Pool;

namespace DefaultNamespace
{
    public static class Extensions
    {
        public static void SetLayerWithChildren(this GameObject target, int layer)
        {
            target.layer = layer;
            using var childrenPooled = ListPool<Transform>.Get(out var children);
            target.GetComponentsInChildren(children);
            foreach (var child in children)
            {
                child.gameObject.layer = layer;
            }
        }
    }
}