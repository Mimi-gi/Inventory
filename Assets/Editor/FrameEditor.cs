using UnityEngine;
using UnityEditor; // これが必要です

[CustomEditor(typeof(Frame))] // Frameクラスのエディタであることを宣言
public class FrameEditor : Editor
{
    // プロパティの変数
    SerializedProperty highlightedSpriteProp;
    SerializedProperty slotTypeProp;
    SerializedProperty slotItemTypeProp;

    private void OnEnable()
    {
        // 変数名（文字列）でプロパティを取得して紐づける
        highlightedSpriteProp = serializedObject.FindProperty("highlightedSprite");
        slotTypeProp = serializedObject.FindProperty("SlotType");
        slotItemTypeProp = serializedObject.FindProperty("SlotItemType");
        
        // "Item"は大文字小文字を区別するので、元のコードの変数名と一致させる
    }

    public override void OnInspectorGUI()
    {
        // 変更の監視を開始
        serializedObject.Update();

        // 1. 常に表示したいものを描画
        // Frameスクリプト自体（MonoBehaviour）の参照フィールドを表示（任意）
        // DrawDefaultInspector(); // これを使うと全部出ちゃうので今回は使いません

        EditorGUILayout.PropertyField(highlightedSpriteProp);
        EditorGUILayout.PropertyField(slotTypeProp);

        // 2. 条件分岐
        // slotTypeの現在の値を取得 (enumのインデックス)
        SlotType currentType = (SlotType)slotTypeProp.enumValueIndex;

        // SlotTypeがCreativeのときだけItemを表示
        if (currentType == SlotType.Creative)
        {
            EditorGUILayout.PropertyField(slotItemTypeProp, new GUIContent("ItemType (Creative Only)"));
        }
        else
        {
            // それ以外のときはItemフィールドをクリア
            slotItemTypeProp.enumValueIndex = (int)ItemType.None;
        }

        // 変更を適用（Undo/Redo対応のため必須）
        serializedObject.ApplyModifiedProperties();
    }
}