#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using UnityEditor.SceneManagement;

public class ExporterSKFB : EditorWindow {

	[MenuItem("GlTF/export")]
	static void Init()
	{
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX // edit: added Platform Dependent Compilation - win or osx standalone
		ExporterSKFB window = (ExporterSKFB)EditorWindow.GetWindow(typeof(ExporterSKFB));
		window.titleContent.text = "GlTF";
		window.Show();
#else // and error dialog if not standalone
		EditorUtility.DisplayDialog("Error", "Your build target must be set to standalone", "Okay");
#endif
	}

	GameObject exporterGo;
	SceneToGlTFWiz exporter;

	private string exportPath;

	private bool opt_exportAnimation = true;

	private string status = "";
	private Color blueColor = new Color(69 / 255.0f, 185 / 255.0f, 223 / 255.0f);
	private Color greyColor = Color.white;

	void Awake()
	{
		System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/gltf-exports");
		exportPath = Application.persistentDataPath + "/gltf-exports/";
		exporterGo = new GameObject("Exporter");
		exporter = exporterGo.AddComponent<SceneToGlTFWiz>();
		exporterGo.hideFlags = HideFlags.HideAndDontSave;
		this.minSize = new Vector2 (300, 150);
		this.maxSize = new Vector2 (600, 300);
	}

	private bool updateExporterStatus()
	{
		status = "";

		int nbSelectedObjects = Selection.GetTransforms(SelectionMode.Deep).Length;
		if (nbSelectedObjects == 0)
		{
			status = "No object selected to export";
			return false;
		}

		status = "select " + nbSelectedObjects + " object" + (nbSelectedObjects != 1 ? "s" : "");
		return true;
	}

	void OnGUI()
	{
        // Export path
        GUILayout.Label("Export Path", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        exportPath = EditorGUILayout.TextField(exportPath);
        if (GUILayout.Button("...", GUILayout.Width(48), GUILayout.Height(16))) {
            exportPath = EditorUtility.OpenFolderPanel("Export Path", exportPath, "");
        }
        GUILayout.EndHorizontal();

		GUILayout.Label("Options", EditorStyles.boldLabel);
		GUILayout.Space(5);
		GUILayout.BeginHorizontal();
		opt_exportAnimation = EditorGUILayout.Toggle("Export animation (beta)", opt_exportAnimation);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		bool enable = updateExporterStatus();

		if (enable)
			GUI.color = blueColor;
		else
			GUI.color = greyColor;

		GUI.enabled = enable;
		GUILayout.BeginVertical();
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button(status, GUILayout.Width(250), GUILayout.Height(40)))
		{
			if (!enable)
			{
				EditorUtility.DisplayDialog("Error", status, "Ok");
			}
			else
			{
				Transform[] transforms = Selection.GetTransforms(SelectionMode.TopLevel);
				string exportFileName = Path.Combine(exportPath, transforms[0].name + ".gltf");
				string zipFileName = Path.Combine(exportPath, transforms[0].name + ".zip");
				if (System.IO.File.Exists(zipFileName))
				{
					System.IO.File.Delete(zipFileName);
				}

				exporter.ExportCoroutine(exportFileName, null, true, true, opt_exportAnimation, true);
			}
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
	}

	void OnDestroy()
	{

		if (exporterGo)
		{
			DestroyImmediate(exporterGo);
			exporter = null;
		}
	}
}

#endif