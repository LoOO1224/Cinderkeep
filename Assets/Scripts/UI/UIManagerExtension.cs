using UnityEngine;

namespace OODong.UI
{
    public static class UIManagerExtension
    {
        public static void OpenCinderkeepInventory(this UIManager uiManager, MonoBehaviour hudView)
        {
            hudView?.SendMessage("SetInventoryOpen", true, SendMessageOptions.DontRequireReceiver);
        }

        public static void CloseCinderkeepInventory(this UIManager uiManager, MonoBehaviour hudView)
        {
            hudView?.SendMessage("SetInventoryOpen", false, SendMessageOptions.DontRequireReceiver);
        }
    }
}
