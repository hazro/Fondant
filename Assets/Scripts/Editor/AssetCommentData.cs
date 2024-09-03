using UnityEngine;

[CreateAssetMenu(fileName = "AssetCommentData", menuName = "ScriptableObjects/AssetCommentData", order = 1)]
public class AssetCommentData : ScriptableObject
{
	public AssetComment[] comments;
}

[System.Serializable]
public class AssetComment
{
	public string guid;
	public string comment;
}