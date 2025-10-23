using UnityEngine;
using UnityEditor;

public class InventorySetupMenu
{
    [MenuItem("Tools/Inventory/Setup Player Inventory")]
    public static void SetupPlayerInventory()
    {
        // ������ "Hero" �Ǵ� ���õ� ������Ʈ�� ã��
        GameObject player = GameObject.Find("Hero");
        if (player == null && Selection.activeGameObject != null)
            player = Selection.activeGameObject;

        if (player == null)
        {
            EditorUtility.DisplayDialog("Error", "Hero ������Ʈ�� ã�� �� �����ϴ�. ���� 'Hero' ������Ʈ�� ����ų� Hierarchy���� ������Ʈ�� �����ϼ���.", "OK");
            return;
        }

        // Inventory ������Ʈ �߰�
        if (player.GetComponent<Inventory>() == null)
        {
            Undo.AddComponent<Inventory>(player);
        }

        // EquipmentManager �̱��� ������Ʈ ����
        EquipmentManager manager = Object.FindFirstObjectByType<EquipmentManager>();
        if (manager == null)
        {
            GameObject mgrGO = new GameObject("EquipmentManager");
            Undo.RegisterCreatedObjectUndo(mgrGO, "Create EquipmentManager");
            mgrGO.AddComponent<EquipmentManager>();
        }

        EditorUtility.DisplayDialog("Setup Complete", "Player Inventory and EquipmentManager have been setup.", "OK");
    }
}