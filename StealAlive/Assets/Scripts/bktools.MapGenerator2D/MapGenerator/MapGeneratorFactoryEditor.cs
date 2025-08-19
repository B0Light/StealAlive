using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
/// <summary>
/// MapGeneratorFactory의 커스텀 에디터
/// 4종류의 맵 생성 알고리즘을 버튼으로 선택하고 해당 설정을 표시합니다.
/// </summary>
[UnityEditor.CustomEditor(typeof(MapGeneratorFactory))]
public class MapGeneratorFactoryEditor : UnityEditor.Editor
{
    private bool showBasicSettings = true;
    private bool showIsaacSettings = false;
    private bool showDelaunaySettings = false;
    private bool showBSPSettings = false;
    private bool showBSPFullSettings = false;
    
    public override void OnInspectorGUI()
    {
        MapGeneratorFactory factory = (MapGeneratorFactory)target;
        
        // 맵 생성기 타입 선택 버튼들
        EditorGUILayout.LabelField("맵 생성기 선택", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("BSP", GUILayout.Height(30)))
        {
            factory.SetGeneratorType(MapGeneratorType.BSP);
            showBSPSettings = true;
            showBSPFullSettings = false;
            showIsaacSettings = false;
            showDelaunaySettings = false;
        }
        if (GUILayout.Button("BSP Full", GUILayout.Height(30)))
        {
            factory.SetGeneratorType(MapGeneratorType.BSPFull);
            showBSPFullSettings = true;
            showBSPSettings = false;
            showIsaacSettings = false;
            showDelaunaySettings = false;
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Isaac", GUILayout.Height(30)))
        {
            factory.SetGeneratorType(MapGeneratorType.Isaac);
            showIsaacSettings = true;
            showBSPSettings = false;
            showBSPFullSettings = false;
            showDelaunaySettings = false;
        }
        if (GUILayout.Button("Delaunay", GUILayout.Height(30)))
        {
            factory.SetGeneratorType(MapGeneratorType.Delaunay);
            showDelaunaySettings = true;
            showBSPSettings = false;
            showBSPFullSettings = false;
            showIsaacSettings = false;
        }
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(20);
        
        // 기본 설정 (항상 표시)
        showBasicSettings = EditorGUILayout.Foldout(showBasicSettings, "기본 설정", true);
        if (showBasicSettings)
        {
            EditorGUI.indentLevel++;
            SerializedProperty gridSizeProp = serializedObject.FindProperty("gridSize");
            SerializedProperty cubeSizeProp = serializedObject.FindProperty("cubeSize");
            SerializedProperty seedProp = serializedObject.FindProperty("seed");
            SerializedProperty slotProp = serializedObject.FindProperty("slot");
            SerializedProperty tileMappingDataProp = serializedObject.FindProperty("tileMappingDataSO");
            
            EditorGUILayout.PropertyField(gridSizeProp);
            EditorGUILayout.PropertyField(cubeSizeProp);
            EditorGUILayout.PropertyField(seedProp);
            EditorGUILayout.PropertyField(slotProp);
            EditorGUILayout.PropertyField(tileMappingDataProp);
            EditorGUI.indentLevel--;
        }
        
        GUILayout.Space(10);
        
        // Isaac 맵 생성기 설정
        if (showIsaacSettings)
        {
            EditorGUILayout.LabelField("Isaac 맵 생성기 설정", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            SerializedProperty isaacMaxRoomsProp = serializedObject.FindProperty("isaacMaxRooms");
            SerializedProperty isaacSpecialRoomCountProp = serializedObject.FindProperty("isaacSpecialRoomCount");
            SerializedProperty isaacHorizontalSizeProp = serializedObject.FindProperty("isaacHorizontalSize");
            SerializedProperty isaacVerticalSizeProp = serializedObject.FindProperty("isaacVerticalSize");
            
            EditorGUILayout.PropertyField(isaacMaxRoomsProp);
            EditorGUILayout.PropertyField(isaacSpecialRoomCountProp);
            EditorGUILayout.PropertyField(isaacHorizontalSizeProp);
            EditorGUILayout.PropertyField(isaacVerticalSizeProp);
            
            // 경로 설정
            GUILayout.Space(10);
            EditorGUILayout.LabelField("경로 설정", EditorStyles.boldLabel);
            SerializedProperty isaacPathTypeProp = serializedObject.FindProperty("isaacPathType");
            SerializedProperty isaacPathValueProp = serializedObject.FindProperty("isaacPathValue");
            
            if (isaacPathTypeProp != null)
                EditorGUILayout.PropertyField(isaacPathTypeProp, new UnityEngine.GUIContent("경로 타입"));
            if (isaacPathValueProp != null)
                EditorGUILayout.PropertyField(isaacPathValueProp, new UnityEngine.GUIContent("경로 생성 확률"));
                
            EditorGUI.indentLevel--;
        }
        
        // Delaunay 맵 생성기 설정
        if (showDelaunaySettings)
        {
            EditorGUILayout.LabelField("Delaunay 맵 생성기 설정", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            SerializedProperty delaunayMinRoomSizeProp = serializedObject.FindProperty("delaunayMinRoomSize");
            SerializedProperty delaunayMaxRoomSizeProp = serializedObject.FindProperty("delaunayMaxRoomSize");
            SerializedProperty delaunayPathValueProp = serializedObject.FindProperty("delaunayPathValue");
            
            EditorGUILayout.PropertyField(delaunayMinRoomSizeProp);
            EditorGUILayout.PropertyField(delaunayMaxRoomSizeProp);
            EditorGUILayout.PropertyField(delaunayPathValueProp);
            
            // 경로 설정
            GUILayout.Space(10);
            EditorGUILayout.LabelField("경로 설정", EditorStyles.boldLabel);
            SerializedProperty delaunayPathTypeProp = serializedObject.FindProperty("delaunayPathType");
            
            if (delaunayPathTypeProp != null)
                EditorGUILayout.PropertyField(delaunayPathTypeProp, new UnityEngine.GUIContent("경로 타입"));
                
            EditorGUI.indentLevel--;
        }
        
        // BSP 맵 생성기 설정
        if (showBSPSettings)
        {
            EditorGUILayout.LabelField("BSP 맵 생성기 설정", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            SerializedProperty bspMinRoomSizeProp = serializedObject.FindProperty("bspMinRoomSize");
            SerializedProperty bspMaxRoomSizeProp = serializedObject.FindProperty("bspMaxRoomSize");
            SerializedProperty bspMaxDepthProp = serializedObject.FindProperty("bspMaxDepth");
            
            EditorGUILayout.PropertyField(bspMinRoomSizeProp);
            EditorGUILayout.PropertyField(bspMaxRoomSizeProp);
            EditorGUILayout.PropertyField(bspMaxDepthProp);
            
            // 경로 설정
            GUILayout.Space(10);
            EditorGUILayout.LabelField("경로 설정", EditorStyles.boldLabel);
            SerializedProperty bspPathTypeProp = serializedObject.FindProperty("bspPathType");
            SerializedProperty bspPathValueProp = serializedObject.FindProperty("bspPathValue");
            
            if (bspPathTypeProp != null)
                EditorGUILayout.PropertyField(bspPathTypeProp, new UnityEngine.GUIContent("경로 타입"));
            if (bspPathValueProp != null)
                EditorGUILayout.PropertyField(bspPathValueProp, new UnityEngine.GUIContent("경로 생성 확률"));
                
            EditorGUI.indentLevel--;
        }
        
        // BSP Full 맵 생성기 설정
        if (showBSPFullSettings)
        {
            EditorGUILayout.LabelField("BSP Full 맵 생성기 설정", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            SerializedProperty bspFullMinSplitSizeProp = serializedObject.FindProperty("bspFullMinSplitSize");
            SerializedProperty bspFullMaxDepthProp = serializedObject.FindProperty("bspFullMaxDepth");
            
            EditorGUILayout.PropertyField(bspFullMinSplitSizeProp);
            EditorGUILayout.PropertyField(bspFullMaxDepthProp);
            
            // 경로 설정
            GUILayout.Space(10);
            EditorGUILayout.LabelField("경로 설정", EditorStyles.boldLabel);
            SerializedProperty bspFullPathTypeProp = serializedObject.FindProperty("bspFullPathType");
            SerializedProperty bspFullPathValueProp = serializedObject.FindProperty("bspFullPathValue");
            
            if (bspFullPathTypeProp != null)
                EditorGUILayout.PropertyField(bspFullPathTypeProp, new UnityEngine.GUIContent("경로 타입"));
            if (bspFullPathValueProp != null)
                EditorGUILayout.PropertyField(bspFullPathValueProp, new UnityEngine.GUIContent("경로 생성 확률"));
                
            EditorGUI.indentLevel--;
        }
        
        // 자동 생성 설정
        SerializedProperty autoGenerateProp = serializedObject.FindProperty("autoGenerateOnStart");
        EditorGUILayout.PropertyField(autoGenerateProp);
        
        GUILayout.Space(20);
        
        // 액션 버튼들
        EditorGUILayout.LabelField("액션", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("맵 생성", GUILayout.Height(25)))
        {
            factory.GenerateMap();
        }
        if (GUILayout.Button("생성기 정리", GUILayout.Height(25)))
        {
            factory.ClearAllGenerators();
        }
        EditorGUILayout.EndHorizontal();
        
        // 맵 관리 버튼들
        if (factory.IsMapGenerated())
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("맵 제거", GUILayout.Height(25)))
            {
                factory.ClearMap();
            }
            if (GUILayout.Button("맵 재생성", GUILayout.Height(25)))
            {
                factory.RegenerateMap();
            }
            EditorGUILayout.EndHorizontal();
        }
        
        // 상태 표시
        EditorGUILayout.LabelField("상태", EditorStyles.boldLabel);
        
        if (factory.IsMapGenerated())
        {
            EditorGUILayout.HelpBox("맵이 생성되었습니다.", MessageType.Info);
            
            // 맵 데이터 정보 표시
            var mapData = factory.GetCurrentMapData();
            if (mapData != null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField($"그리드 크기: {mapData.gridSize.x} x {mapData.gridSize.y}");
                EditorGUILayout.LabelField($"방 개수: {mapData.roomCount}");
                EditorGUILayout.LabelField($"시드: {mapData.seed}");
                EditorGUI.indentLevel--;
            }
        }
        else
        {
            EditorGUILayout.HelpBox("맵이 생성되지 않았습니다.", MessageType.Warning);
        }
        
        // 현재 생성기 정보 표시
        if (factory.CurrentGenerator != null)
        {
            EditorGUILayout.HelpBox($"현재 생성기: {factory.CurrentGenerator.GetType().Name}", MessageType.Info);
            
            // 생성기 상태 확인
            if (factory.CurrentGenerator.HasGeneratedMap())
            {
                EditorGUILayout.HelpBox("생성된 맵이 있습니다.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("생성된 맵이 없습니다.", MessageType.Warning);
            }
        }
        
        // 변경사항 적용
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
