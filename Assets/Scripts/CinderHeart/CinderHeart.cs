using UnityEngine;
namespace Cinderkeep.Gameplay
{
    public class CinderHeart : MonoBehaviour
    {
        public static Transform InstanceTransform { get; private set; }

        private void Awake()
        {
            InstanceTransform = this.transform;
        }

        private void OnDestroy()
        {
            if (InstanceTransform == this.transform)
            {
                InstanceTransform = null;
            }
        }
    }
}