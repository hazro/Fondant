using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ProjectWindowCommentExtension
{
	private static AssetCommentData commentData;

	static ProjectWindowCommentExtension()
	{
		// コメントデータをロード
		commentData = AssetDatabase.LoadAssetAtPath<AssetCommentData>("Assets/Scripts/Editor/AssetCommentData.asset");

		EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
	}

	private static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
	{
		if (commentData == null) return;

		// コメントを取得
		string comment = GetCommentForAsset(guid);

		if (!string.IsNullOrEmpty(comment))
		{
			// コメントを表示するためのRectを計算
			Rect commentRect = new Rect(selectionRect.xMax - 200, selectionRect.y, 200, selectionRect.height);

			// コメントを描画
			GUI.Label(commentRect, comment, EditorStyles.label);
		}
	}

	private static string GetCommentForAsset(string guid)
	{
		foreach (var assetComment in commentData.comments)
		{
			if (assetComment.guid == guid)
			{
				return assetComment.comment;
			}
		}
		return null;
	}

	[MenuItem("Assets/Add Comment")]
	private static void AddComment()
	{
		string guid = Selection.assetGUIDs[0];
		CommentWindow.ShowWindow(guid);
	}

	[MenuItem("Assets/Remove Comment")]
	private static void RemoveComment()
	{
		string guid = Selection.assetGUIDs[0];
		RemoveCommentFromAsset(guid);
	}

	private static void AddCommentToAsset(string guid, string comment)
	{
		if (commentData == null) return;

		foreach (var assetComment in commentData.comments)
		{
			if (assetComment.guid == guid)
			{
				assetComment.comment = comment;
				EditorUtility.SetDirty(commentData);
				AssetDatabase.SaveAssets();
				return;
			}
		}

		ArrayUtility.Add(ref commentData.comments, new AssetComment { guid = guid, comment = comment });
		EditorUtility.SetDirty(commentData);
		AssetDatabase.SaveAssets();
	}

	private static void RemoveCommentFromAsset(string guid)
	{
		if (commentData == null) return;

		for (int i = 0; i < commentData.comments.Length; i++)
		{
			if (commentData.comments[i].guid == guid)
			{
				ArrayUtility.RemoveAt(ref commentData.comments, i);
				EditorUtility.SetDirty(commentData);
				AssetDatabase.SaveAssets();
				return;
			}
		}
	}

	public class CommentWindow : EditorWindow
	{
		private string guid;
		private string comment;

		public static void ShowWindow(string guid)
		{
			CommentWindow window = GetWindow<CommentWindow>("Add Comment");
			window.guid = guid;
			window.comment = "";
			window.Show();
		}

		private void OnGUI()
		{
			GUILayout.Label("Enter your comment:", EditorStyles.boldLabel);
			comment = EditorGUILayout.TextField("Comment", comment);

			if (GUILayout.Button("Save"))
			{
				AddCommentToAsset(guid, comment);
				Close();
			}
		}
	}
}