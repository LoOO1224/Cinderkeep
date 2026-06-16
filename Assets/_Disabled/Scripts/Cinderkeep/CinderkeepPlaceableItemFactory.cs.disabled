using UnityEngine;

namespace OODong.Cinderkeep
{
    // 퀵슬롯에서 Stone/Ore를 월드에 배치하는 Factory.
    // 런타임 new GameObject/CreatePrimitive 대신, 씬 빌더가 미리 만든 Template을 Instantiate한다.
    public sealed class CinderkeepPlaceableItemFactory : MonoBehaviour
    {
        [SerializeField] private Transform Transform_PlacedItemRoot;
        [SerializeField] private GameObject GameObject_StoneTemplate;
        [SerializeField] private GameObject GameObject_OreTemplate;
        [SerializeField] private float _placeDistance = 3.2f;

        public void SetReferences(Transform placedItemRoot, GameObject stoneTemplate, GameObject oreTemplate)
        {
            Transform_PlacedItemRoot = placedItemRoot;
            GameObject_StoneTemplate = stoneTemplate;
            GameObject_OreTemplate = oreTemplate;
        }

        public bool TryPlaceItem(CinderkeepFirstPersonPlayer player, Camera playerCamera, CinderkeepItemId itemId)
        {
            // TODO(팀원 작업 요청): 진짜 건축 시스템은 자유 배치보다 CinderkeepBuildSite를 우선 사용해 주세요.
            // 이 Factory는 발표용 "아이템을 내려놓는 기능" 정도로 유지하는 것이 좋다.
            if (player == null || playerCamera == null)
            {
                return false;
            }

            if (itemId != CinderkeepItemId.Stone && itemId != CinderkeepItemId.Ore)
            {
                return false;
            }

            if (!player.Inventory.TryRemoveItem(itemId, 1))
            {
                player.ShowStatus($"{CinderkeepItemCatalog.GetDisplayName(itemId)} is empty");
                return false;
            }

            GameObject template = itemId == CinderkeepItemId.Ore ? GameObject_OreTemplate : GameObject_StoneTemplate;
            if (template == null)
            {
                player.ShowStatus($"No placeable prefab for {CinderkeepItemCatalog.GetDisplayName(itemId)}");
                player.Inventory.AddItem(itemId, 1);
                return false;
            }

            Vector3 placePosition = GetPlacePosition(playerCamera);
            GameObject placedItem = Instantiate(template, placePosition, Quaternion.identity, Transform_PlacedItemRoot != null ? Transform_PlacedItemRoot : transform);
            placedItem.name = $"Placed_{CinderkeepItemCatalog.GetDisplayName(itemId)}";
            placedItem.SetActive(true);
            GameObjectManager.Instance?.RegisterGameObject(placedItem);

            player.ShowStatus($"Placed {CinderkeepItemCatalog.GetDisplayName(itemId)}");
            return true;
        }

        private Vector3 GetPlacePosition(Camera playerCamera)
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, _placeDistance, ~0, QueryTriggerInteraction.Ignore))
            {
                return hit.point + (hit.normal * 0.35f);
            }

            Vector3 forwardPoint = playerCamera.transform.position + playerCamera.transform.forward * _placeDistance;
            forwardPoint.y = Mathf.Max(0.35f, forwardPoint.y - 0.6f);
            return forwardPoint;
        }
    }
}
