using NodeCanvas.StateMachines;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NodeCanvas.Editor
{
    /// <summary>
    /// Odin-powered editor window to view and manage all FSMOwners in the scene.
    /// </summary>
    public class FSMOwnerSceneViewer : OdinEditorWindow
    {
        [MenuItem("Tools/FSM Scene Viewer")]
        private static void OpenWindow()
        {
            GetWindow<FSMOwnerSceneViewer>().Show();
        }

        [Title("FSM Owner Scene Viewer", "View and manage all FSM instances in the current scene", TitleAlignment = TitleAlignments.Centered)]
        [InfoBox("This window shows all GameObjects with FSMOwner components in the active scene.", InfoMessageType.Info)]
        [PropertySpace(10)]
        [Button("Refresh List", ButtonSizes.Large)]
        [GUIColor(0.4f, 0.8f, 1f)]
        private void RefreshList()
        {
            FindAllFSMOwners();
        }

        [PropertySpace(15)]
        [Title("FSM Owners in Scene")]
        [ListDrawerSettings(
            ShowIndexLabels = false,
            DraggableItems = false,
            HideAddButton = true,
            HideRemoveButton = true,
            OnBeginListElementGUI = "BeginDrawListElement",
            OnEndListElementGUI = "EndDrawListElement"
        )]
        [ShowInInspector]
        private List<FSMOwnerInfo> fsmOwners = new List<FSMOwnerInfo>();

        [PropertySpace(10)]
        [ShowInInspector, ReadOnly]
        [PropertyOrder(100)]
        private string Summary
        {
            get
            {
                if (fsmOwners == null || fsmOwners.Count == 0)
                    return "No FSM Owners found. Click 'Refresh List' to scan the scene.";

                int running = fsmOwners.Count(f => f.IsRunning);
                int stopped = fsmOwners.Count - running;
                int missing = fsmOwners.Count(f => f.HasMissingFSM);

                return $"Total: {fsmOwners.Count} | Running: {running} | Stopped: {stopped} | Missing FSM: {missing}";
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            FindAllFSMOwners();
        }

        private void OnInspectorUpdate()
        {
            // Auto-refresh when in play mode to show runtime status
            if (UnityEngine.Application.isPlaying)
            {
                // Update display info for each FSM owner
                foreach (var info in fsmOwners)
                {
                    info.RefreshDisplay();
                }
                Repaint();
            }
        }

        private void FindAllFSMOwners()
        {
            fsmOwners.Clear();

            var allOwners = FindObjectsOfType<FSMOwner>();
            foreach (var owner in allOwners)
            {
                fsmOwners.Add(new FSMOwnerInfo(owner));
            }

            // Sort by GameObject name
            fsmOwners = fsmOwners.OrderBy(f => f.GetGameObjectName()).ToList();
        }

        private void BeginDrawListElement(int index)
        {
            if (index >= 0 && index < fsmOwners.Count)
            {
                var info = fsmOwners[index];
                if (info.HasMissingFSM)
                {
                    GUI.backgroundColor = new Color(1f, 0.5f, 0.5f); // Red tint for errors
                }
                else if (info.IsRunning)
                {
                    GUI.backgroundColor = new Color(0.5f, 1f, 0.5f); // Green tint for running
                }
            }
        }

        private void EndDrawListElement(int index)
        {
            if (index >= 0 && index < fsmOwners.Count)
            {
                var info = fsmOwners[index];
                if (info.HasMissingFSM || info.IsRunning)
                {
                    GUI.backgroundColor = Color.white;
                }
            }
        }

        [System.Serializable]
        private class FSMOwnerInfo
        {
            [HorizontalGroup("Row")]
            [ShowInInspector, DisplayAsString, HideLabel]
            public string DisplayInfo { get; private set; }

            [HorizontalGroup("Row", Width = 70)]
            [Button("Select"), GUIColor(0.7f, 0.9f, 1f)]
            private void SelectGameObject()
            {
                if (owner != null)
                {
                    Selection.activeGameObject = owner.gameObject;
                    EditorGUIUtility.PingObject(owner.gameObject);
                }
            }

            [HorizontalGroup("Row", Width = 90)]
            [Button("Edit FSM"), GUIColor(0.9f, 0.8f, 0.5f)]
            [EnableIf("@!HasMissingFSM")]
            private void OpenFSMGraph()
            {
                if (owner != null && owner.graph != null)
                {
                    NodeCanvas.Editor.GraphEditor.OpenWindow(owner.graph);
                }
            }

            private FSMOwner owner;

            public bool IsRunning
            {
                get
                {
                    if (!UnityEngine.Application.isPlaying || owner == null)
                        return false;
                    return owner.isRunning;
                }
            }

            public bool HasMissingFSM
            {
                get { return owner != null && owner.graph == null; }
            }

            public string GetGameObjectName()
            {
                return owner != null ? owner.gameObject.name : "";
            }

            public FSMOwnerInfo(FSMOwner fsmOwner)
            {
                owner = fsmOwner;
                RefreshDisplay();
            }

            public void RefreshDisplay()
            {
                if (owner == null) return;
                
                string gameObjectName = owner.gameObject.name;
                string fsmName = owner.graph != null ? owner.graph.name : "⚠️ MISSING FSM!";
                string status = "";
                
                if (UnityEngine.Application.isPlaying)
                {
                    status = IsRunning ? " [✓ Running]" : " [⏸ Stopped]";
                }
                
                // Build display string: "GameObject → FSM [Status]"
                DisplayInfo = $"{gameObjectName} → {fsmName}{status}";
            }
        }
    }
}

