using UnityEngine;
using UnityEditor;

public class InventorySetupMenu
{
    [MenuItem("Tools/Inventory/Setup Player Inventory")]
    public static void SetupPlayerInventory()
    {
        // 씬에서 "Hero" 또는 선택된 오브젝트를 찾음
        GameObject player = GameObject.Find("Hero");
        if (player == null && Selection.activeGameObject != null)
            player = Selection.activeGameObject;

        if (player == null)
        {
            EditorUtility.DisplayDialog("Error", "Hero 오브젝트를 찾을 수 없습니다. 씬에 'Hero' 오브젝트를 만들거나 Hierarchy에서 오브젝트를 선택하세요.", "OK");
            return;
        }

        // Inventory 컴포넌트 추가
        if (player.GetComponent<Inventory>() == null)
        {
            Undo.AddComponent<Inventory>(player);
        }

        // EquipmentManager 싱글톤 오브젝트 생성
        EquipmentManager manager = Object.FindObjectOfType<EquipmentManager>();
        if (manager == null)
        {
            GameObject mgrGO = new GameObject("EquipmentManager");
            Undo.RegisterCreatedObjectUndo(mgrGO, "Create EquipmentManager");
            mgrGO.AddComponent<EquipmentManager>();
        }

        EditorUtility.DisplayDialog("Setup Complete", "Player Inventory and EquipmentManager have been setup.", "OK");
    }
}