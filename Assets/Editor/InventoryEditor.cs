using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Inventory))]

public class InventoryEditor : Editor
{
    SerializedProperty sizeProp;
    SerializedProperty inventorySlotPrefabProp;
    SerializedProperty offsetProp;
    SerializedProperty slotTypeProp;
    SerializedProperty itemDicProp;

    private void OnEnable()
    {
        sizeProp = serializedObject.FindProperty("size");
        inventorySlotPrefabProp = serializedObject.FindProperty("inventorySlotPrefab");
        offsetProp = serializedObject.FindProperty("offset");
        slotTypeProp = serializedObject.FindProperty("slotType");
        itemDicProp = serializedObject.FindProperty("itemDic");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(sizeProp);
        EditorGUILayout.PropertyField(inventorySlotPrefabProp);
        EditorGUILayout.PropertyField(offsetProp);
        EditorGUILayout.PropertyField(slotTypeProp);
        if (slotTypeProp.enumValueIndex == (int)SlotType.Creative)
        {
            EditorGUILayout.PropertyField(itemDicProp);
        }

        serializedObject.ApplyModifiedProperties();
    }
}