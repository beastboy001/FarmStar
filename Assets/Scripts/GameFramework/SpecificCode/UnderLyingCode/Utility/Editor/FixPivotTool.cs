using UnityEngine;
using UnityEditor;

public class FixPivotTool : EditorWindow
{
    private GameObject targetObject;
    private bool autoCalculate = true;
    private Vector3 manualOffset = Vector3.zero;

    [MenuItem("Tools/轴心修正工具")]
    public static void ShowWindow()
    {
        GetWindow<FixPivotTool>("轴心修正工具");
    }

    void OnGUI()
    {
        GUILayout.Label("轴心修正工具", EditorStyles.boldLabel);
        GUILayout.Space(10);

        targetObject = (GameObject)EditorGUILayout.ObjectField(
            "目标物体", targetObject, typeof(GameObject), true);

        EditorGUILayout.Space(10);
        GUILayout.Label("修正设置", EditorStyles.boldLabel);

        autoCalculate = EditorGUILayout.Toggle("自动计算中心位置", autoCalculate);

        if (!autoCalculate)
        {
            manualOffset = EditorGUILayout.Vector3Field("手动偏移量", manualOffset);
        }

        EditorGUILayout.Space(10);

        if (targetObject != null)
        {
            // 显示当前轴心信息
            EditorGUILayout.HelpBox(
                $"物体：{targetObject.name}\n" +
                $"当前轴心位置（本地）：(0,0,0)\n" +
                $"几何中心位置（本地）：{GetLocalCenter(targetObject)}",
                MessageType.Info);
        }

        GUILayout.Space(10);

        GUI.enabled = targetObject != null;
        if (GUILayout.Button("修正轴心（将中心变为轴心）", GUILayout.Height(40)))
        {
            FixPivot();
        }
        GUI.enabled = true;

        GUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "原理：\n" +
            "1. 计算物体的几何中心\n" +
            "2. 创建一个空父物体，位置放在几何中心\n" +
            "3. 将原物体设为子物体，偏移量使轴心对齐中心\n" +
            "4. 修正后旋转父物体，物体会绕中心旋转",
            MessageType.Info);
    }

    void FixPivot()
    {
        if (targetObject == null)
        {
            EditorUtility.DisplayDialog("错误", "请先拖入目标物体！", "确定");
            return;
        }

        // 1. 计算几何中心
        Vector3 centerOffset;
        if (autoCalculate)
        {
            centerOffset = GetLocalCenter(targetObject);
            Debug.Log($"几何中心（本地坐标）：{centerOffset}");
        }
        else
        {
            centerOffset = manualOffset;
        }

        // 检查是否需要修正
        if (centerOffset == Vector3.zero)
        {
            if (!EditorUtility.DisplayDialog("确认",
                "检测到物体中心已经在轴心位置，是否继续？",
                "继续", "取消"))
            {
                return;
            }
        }

        // 2. 记录原始信息
        Vector3 originalWorldPos = targetObject.transform.position;
        Quaternion originalWorldRot = targetObject.transform.rotation;
        Vector3 originalWorldScale = targetObject.transform.lossyScale;

        // 3. 创建父物体（新轴心）
        GameObject parent = new GameObject(targetObject.name + "_Pivot");
        Undo.RegisterCreatedObjectUndo(parent, "创建轴心父物体");

        // 4. 设置父物体的位置（在几何中心）
        Vector3 worldCenter = targetObject.transform.TransformPoint(centerOffset);
        parent.transform.position = worldCenter;
        parent.transform.rotation = originalWorldRot;
        parent.transform.localScale = Vector3.one;

        // 5. 将原物体设为子物体
        Undo.SetTransformParent(targetObject.transform, parent.transform, "重新设置父物体");

        // 6. 调整原物体的本地位置（使轴心对齐父物体）
        targetObject.transform.localPosition = -centerOffset;
        targetObject.transform.localRotation = Quaternion.identity;
        targetObject.transform.localScale = Vector3.one;

        // 7. 保持原物体在世界中的位置不变
        parent.transform.position = originalWorldPos;
        parent.transform.rotation = originalWorldRot;

        // 8. 选中父物体，方便查看效果
        Selection.activeGameObject = parent;

        Debug.Log($"✅ 已将 {targetObject.name} 的轴心移动到几何中心");
        EditorUtility.DisplayDialog("完成",
            $"已将物体 {targetObject.name} 的轴心移动到几何中心\n\n" +
            $"修正后的物体在父物体 {parent.name} 下\n" +
            "旋转父物体即可看到绕中心旋转的效果",
            "确定");
    }

    Vector3 GetLocalCenter(GameObject obj)
    {
        Vector3 center = Vector3.zero;
        int count = 0;

        // 方法1：通过Renderer获取
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Vector3 worldCenter = renderer.bounds.center;
            center = obj.transform.InverseTransformPoint(worldCenter);
            count++;
        }

        // 方法2：通过MeshFilter获取
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            Vector3 meshCenter = meshFilter.sharedMesh.bounds.center;
            center = meshCenter;
            count++;
        }

        // 方法3：通过子物体计算平均位置
        if (count == 0 && obj.transform.childCount > 0)
        {
            Vector3 sum = Vector3.zero;
            foreach (Transform child in obj.transform)
            {
                sum += child.localPosition;
            }
            center = sum / obj.transform.childCount;
            count++;
        }

        // 如果都没有，返回零
        if (count == 0)
        {
            Debug.LogWarning("无法计算物体中心，使用(0,0,0)");
            return Vector3.zero;
        }

        return center;
    }
}