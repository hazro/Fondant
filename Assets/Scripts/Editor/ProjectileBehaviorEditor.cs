using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ProjectileBehaviorクラスのカスタムエディターを定義するクラス。
/// インスペクターに属性に応じたフィールドを動的に表示する。
/// </summary>
[CustomEditor(typeof(ProjectileBehavior))]
public class ProjectileBehaviorEditor : Editor
{
    /// <summary>
    /// インスペクターGUIを描画するメソッド。
    /// </summary>
    public override void OnInspectorGUI()
    {
        ProjectileBehavior projectileBehavior = (ProjectileBehavior)target;

        // デフォルトのインスペクターを描画
        DrawDefaultInspector();

        // Attributesリストを取得
        List<ProjectileBehavior.Attribute> attributes = projectileBehavior.attributes;

        // StatusAilmentが選ばれている場合のみ表示
        if (attributes.Contains(ProjectileBehavior.Attribute.StatusAilment))
        {
            // ステータス異常の選択フィールドを表示
            projectileBehavior.CurrentStatusAilment = (ProjectileBehavior.StatusAilment)EditorGUILayout.EnumPopup("Status Ailment", projectileBehavior.CurrentStatusAilment);
        }

        // StatusEffectが選ばれている場合のみ表示
        if (attributes.Contains(ProjectileBehavior.Attribute.StatusEffect))
        {
            // ステータス効果の選択フィールドを表示
            projectileBehavior.CurrentStatusEffect = (ProjectileBehavior.StatusEffect)EditorGUILayout.EnumPopup("Status Effect", projectileBehavior.CurrentStatusEffect);
        }
    }
}
