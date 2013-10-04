/* Greg Lukosek
 * 0.1
 * www.the-app-developers.co.uk
*/



using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class NGUIMultiToolEditor : EditorWindow 
{
	#region Menu
	[MenuItem("NGUI Tools/Show Tools")]
	
	static void ShowTools () 
	{
		NGUIMultiToolEditor window = (NGUIMultiToolEditor)EditorWindow.GetWindow (typeof (NGUIMultiToolEditor));
	}
	#endregion
	
	
	string lastMessage = "";
	
	int toolbarInt = -1;
	string[] tools = new string[]{"Sprite Tool", "Atlas Tool", "Depth Tool"};
	

	public string oldSpriteName = "";
	public string newSpriteName = "" ;
	public List<UISprite> spritesFound = new List<UISprite>();
	public string lastSearchValue = "";
	
	//atlas tool
	public UIAtlas oldAtlas;
	public UIAtlas newAtlas;
	int toolbarIntAtlasTool = 0;
	string[] atlasTools = new string[]{"Selected", "Search"};
	string searchField = "";
	
	//depth
	Transform root = null;
	int depth = 0;
	
	


	public void OnGUI() 
	{

		toolbarInt = GUILayout.Toolbar (toolbarInt, tools);

		#region Sprite Tool

		if (toolbarInt == 0)
		{
			GUILayout.Label("Find all sprite by spriteName and swap:");
			GUILayout.Space(5f);
			
			this.oldSpriteName = EditorGUILayout.TextField("Old sprite name", this.oldSpriteName); 
			
				if (this.oldSpriteName.Length > 0)
				{
					
				    if(GUILayout.Button("Find Sprites"))
					{
						this.lastSearchValue = this.oldSpriteName;
						Undo.RegisterSceneUndo("findingSprites");
						this.FindSprites();
					}
					
					if (this.oldSpriteName == this.lastSearchValue)
					{
						if (this.spritesFound.Count > 0)
						{
							
							EditorGUILayout.LabelField("Found " + this.spritesFound.Count.ToString() + " sprites");
							this.newSpriteName = EditorGUILayout.TextField("New sprite name", this.newSpriteName); 
						}
						else
						{
							EditorGUILayout.LabelField("Found " + this.spritesFound.Count.ToString() + " sprites");	
						}
						
						if (this.newSpriteName.Length > 0)
						{
							if(GUILayout.Button("Swap Sprites"))
							{
								Undo.RegisterSceneUndo("seplaceSprites");
								this.SwapSprites();
							}
						}
						
					}
					//oldname field changed
					else if (this.oldSpriteName == this.lastSearchValue && this.spritesFound.Count > 0)
					{
						this.ClearFoundSprites();
					}

				}
			GUILayout.Space(20f);
			EditorGUILayout.LabelField("To use sprite tool enter spriteName\nTool will search whole scene for sprites using this name\nThen enter new sprite name and press Swap", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)});
		}
#endregion
		//Atlas tool
		else if (toolbarInt == 1)
		{
			toolbarIntAtlasTool = GUILayout.Toolbar (toolbarIntAtlasTool, atlasTools);

#region Sprite Tool
			
			//selected
			if (toolbarIntAtlasTool == 0)
			{
				GUILayout.Label("Swap atlases on selected sprites");
				GUILayout.Space(5f);
	
				oldAtlas = (UIAtlas)EditorGUILayout.ObjectField("Old Atlas", oldAtlas, typeof(UIAtlas), true );
				
				
				if (oldAtlas)	
				{	
					Transform[] selection = Selection.transforms;
					
					
					newAtlas = (UIAtlas)EditorGUILayout.ObjectField("New Atlas", newAtlas, typeof(UIAtlas), true );
					
					
					if (newAtlas)
					{
						if(GUILayout.Button("Swap Atlases"))
						{
							Undo.RegisterSceneUndo("swapAtlases");
							this.SwapAtlas(selection);
						}
					}
				} 
				
				GUILayout.Space(20f);
				EditorGUILayout.LabelField("Manual:\n1. Drag old atlas prefab\n2. Select sprites in the scene\n3. Drag new atlas prefab\n4. Press button", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)});
		
			}
			//search
			else if (toolbarIntAtlasTool == 1)
			{
				GUILayout.Label("Swap atlases on found sprites");
				GUILayout.Space(5f);
	
				oldAtlas = (UIAtlas)EditorGUILayout.ObjectField("Old Atlas", oldAtlas, typeof(UIAtlas), true );
				
				
				if (oldAtlas)	
				{	
					searchField = EditorGUILayout.TextField("Sprite Name", searchField);
					
					if (searchField.Length >0)
					{
						if(GUILayout.Button("Search for sprites"))
						{
							spritesFound = FindSprites(oldAtlas, searchField);
						}
						
						if (spritesFound.Count > 0)
						{
							EditorGUILayout.LabelField("Found " + this.spritesFound.Count.ToString() + " sprites");	
							
					
							newAtlas = (UIAtlas)EditorGUILayout.ObjectField("New Atlas", newAtlas, typeof(UIAtlas), true );
							
							if (newAtlas)
							{
								if(GUILayout.Button("Swap Atlases"))
								{
									
									if (AtlasHaveSprite(newAtlas, searchField))
									{
										SetAtlas(spritesFound, newAtlas);
										lastMessage = "Successful";
									}
									else
									{
										lastMessage = "Error: Looks like sprite is not exist in new atlas.\nPlease add sprite to your new atlas first and try again.";
									}
	
								}
							}
		

						}
						else
						{
							EditorGUILayout.LabelField("No sprites found");
						}
					}
				} 
				
				GUILayout.Space(20f);
				EditorGUILayout.LabelField("Manual:\n1. Drag in old atlas prefab\n2. Enter sprite name\n3. Search for sprites\n4. Drag in new atlas\n5. Press swap atlases", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)});
	
				
				EditorGUILayout.LabelField(lastMessage);
			}
			
			
			
			
		}
		
#endregion		
		
		
		//Depth tool
		else if (toolbarInt == 2)
		{
			GUILayout.Label("Set widget depth in all children");
			GUILayout.Space(5f);
			
			root = (Transform)EditorGUILayout.ObjectField("Parent", root, typeof(Transform), true );
			

			depth = EditorGUILayout.IntField(depth);

			if (root != null)
			{
				if(GUILayout.Button("Set Depth"))
				{
					Undo.RegisterSceneUndo("setDepth");
					SetDepth(root, depth);
				}
			}
			else
			{
				GUILayout.Label("Assign parent first");
			}
			

			
		}
  }
	
	
	
	
	
	
	
	
	public void FindSprites()
	{

		spritesFound = new List<UISprite>();
		
		UISprite[] sprites = FindObjectsOfType(typeof(UISprite)) as UISprite[];
		
        foreach (UISprite sprite in sprites) 
		{
			if (sprite.spriteName == oldSpriteName) spritesFound.Add(sprite);
        }
	}
	
		
	
	public List<UISprite> FindSprites(UIAtlas atlas, string spriteName)
	{

		List<UISprite> found = new List<UISprite>();
		
		UISprite[] sprites = FindObjectsOfType(typeof(UISprite)) as UISprite[];
		
        foreach (UISprite sprite in sprites) 
		{
			if (sprite.atlas == atlas)
			{
				if (sprite.spriteName == spriteName) found.Add(sprite);
			}
			
        }
		return found;
	}
	
	
	
	public void SwapSprites()
	{
		foreach (UISprite sprite in spritesFound) 
		{
			sprite.spriteName = newSpriteName;
			sprite.MarkAsChanged();
        }
	}
									
									
									
	public bool AtlasHaveSprite (UIAtlas atlas, string spriteName)
	{
		bool result = false;
		
		foreach (UISpriteData spriteData in atlas.spriteList)
		{
			if (spriteData.name == spriteName)
			{
				result = true;	
			}
		}
		
		return result;
		
	}
								
	
	
	public void SwapAtlas(Transform[] selection)
	{
		foreach (Transform go in selection)
		{
			if (go.GetComponent<UISprite>() != null)
			{
				go.GetComponent<UISprite>().atlas = newAtlas;	
			}
		}
	}
	
	
	
	public void SetAtlas(List<UISprite> sprites, UIAtlas targetAtlas)
	{
		foreach (UISprite sprite in sprites)	
		{
			sprite.atlas = targetAtlas;
			sprite.MarkAsChanged();
		}
	}
	
	
	
	public void ClearFoundSprites()
	{
		spritesFound = new List<UISprite>();
	}
	
	
	
	public void SetDepth(Transform root, int depth)
	{

		UIWidget[] widgets = root.GetComponentsInChildren<UIWidget>();
		
		foreach (UIWidget widget in widgets)
		{
			widget.depth = depth;	
			widget.MarkAsChanged();
		}
		
		
		
	}
	
	
	
	
	
}
