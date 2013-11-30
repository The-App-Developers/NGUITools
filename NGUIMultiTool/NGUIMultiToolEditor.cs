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
	string[] tools = new string[]{"Sprite", "Atlas", "Depth", "Font", "Select"};
	

	public string oldSpriteName = "";
	public string newSpriteName = "" ;
	public List<UIWidget> widgetsFound = new List<UIWidget>();
	public string lastSearchValue = "";
	
	//atlas tool
	public UIAtlas oldAtlas;
	public UIAtlas newAtlas;
	int toolbarIntAtlasTool = 0;
	string[] atlasTools = new string[]{"Selected", "Search"};
	string searchField = "";
	
	//depth tools
	int toolbarIntDepthTool = 0;
	string[] depthTools = new string[]{"Parent", "Selected"};
	
	Transform root = null;
	int depth = 0;
	
	//font tools
	UIFont oldFont = new UIFont();
	UIFont newFont = new UIFont();
	List<UILabel> foundLabels = new List<UILabel>();
	
	//select tool
	public UIAtlas selectAtlas;
	public List<Transform> selection = new List<Transform>();
	
	
	
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
						this.FindWidgets();
					}
					
					if (this.oldSpriteName == this.lastSearchValue)
					{
						if (this.widgetsFound.Count > 0)
						{
							
							EditorGUILayout.LabelField("Found " + this.widgetsFound.Count.ToString() + " sprites");
							this.newSpriteName = EditorGUILayout.TextField("New sprite name", this.newSpriteName); 
						}
						else
						{
							EditorGUILayout.LabelField("Found " + this.widgetsFound.Count.ToString() + " sprites");	
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
					else if (this.oldSpriteName == this.lastSearchValue && this.widgetsFound.Count > 0)
					{
						this.ClearFoundWidgets();
					}

				}
			GUILayout.Space(20f);
			EditorGUILayout.LabelField("To use sprite tool enter spriteName\nTool will search whole scene for sprites using this name\nThen enter new sprite name and press Swap", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)});
		}
#endregion
		

#region Atlas Tool
		
		//Atlas tool
		else if (toolbarInt == 1)
		{
			toolbarIntAtlasTool = GUILayout.Toolbar (toolbarIntAtlasTool, atlasTools);
		
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
							widgetsFound = FindWidgets(oldAtlas, searchField);
						}
						
						if (widgetsFound.Count > 0)
						{
							EditorGUILayout.LabelField("Found " + this.widgetsFound.Count.ToString() + " sprites");	
							
					
							newAtlas = (UIAtlas)EditorGUILayout.ObjectField("New Atlas", newAtlas, typeof(UIAtlas), true );
							
							if (newAtlas)
							{
								if(GUILayout.Button("Swap Atlases"))
								{
									
									if (AtlasHaveSprite(newAtlas, searchField))
									{
										SetAtlas(widgetsFound, newAtlas);
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
		
#region Depth Tool	
		//Depth tool
		else if (toolbarInt == 2)
		{
			toolbarIntDepthTool = GUILayout.Toolbar (toolbarIntDepthTool, depthTools);


			//parent
			if (toolbarIntDepthTool == 0)
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
			//selected
			else if (toolbarIntDepthTool == 1)
			{
				GUILayout.Label("Set widget depth on selected");
				GUILayout.Space(5f);
				
				Transform[] selection = Selection.transforms;
					
				depth = EditorGUILayout.IntField(depth);	
				
				if (selection.Length > 0)
				{
					if(GUILayout.Button("Set Depth"))
					{
						Undo.RegisterSceneUndo("setDepthOnSelection");
						this.SetDepth(selection, depth);
					}
				}
		
			
			}
			
		}
	
#endregion		
		
#region Font Tool
		//Font tool
		else if (toolbarInt == 3)
		{
			GUILayout.Label("Find and swap fonts");
			GUILayout.Space(5f);
			
			oldFont = (UIFont)EditorGUILayout.ObjectField("Old Font", oldFont, typeof(UIFont), true );
			
			
			
			if (oldFont != null)
			{
				if(GUILayout.Button("Find Fonts"))
				{
					foundLabels = FindLabels(oldFont);
				}
				
				if (foundLabels.Count > 0)
				{
					GUILayout.Label("Found " + foundLabels.Count + " fonts");
					
					newFont = (UIFont)EditorGUILayout.ObjectField("New Font", newFont, typeof(UIFont), true );
					
					if (newFont != null)
					{
						if(GUILayout.Button("Replace Fonts"))
						{
							Undo.RegisterSceneUndo("swapFonts");
							SwapFont(foundLabels, newFont);
							GUILayout.Space(5f);
							GUILayout.Label("Done");
							
						}
					}
				}
			}
		}
#endregion		
		
#region Select Tool	
		
		//Select tool
		else if (toolbarInt == 4)
		{
			GUILayout.Label("Find and select sprites & fonts using atlas");
			GUILayout.Space(5f);
			
			
			selectAtlas = (UIAtlas)EditorGUILayout.ObjectField("Atlas", selectAtlas, typeof(UIAtlas), true );
				
			if (selectAtlas)	
			{
				if (GUILayout.Button("Search Scene"))
				{
					selection = GetObjectsUsingAtlas(selectAtlas);
				}
				
				if (selection.Count > 0)
				{
					GUILayout.Label(selection.Count + " Objects using atlas");
					GUILayout.Space(10f);
					
					
					foreach (Transform trans in selection)
					{
						EditorGUILayout.ObjectField(trans, typeof(Transform), true );
						
					}
					
				}
			}
		
		}
			
#endregion
		
		
  }
	
	
	
	
	
	
	public void FindWidgets()
	{
		widgetsFound = new List<UIWidget>();
		
		UIWidget[] widgets = FindObjectsOfType(typeof(UIWidget)) as UIWidget[];
		
        foreach (UIWidget widget in widgets) 
		{
			if (GetSprite(widget) != null)
			{
				if (GetSprite(widget).spriteName == oldSpriteName) widgetsFound.Add(widget);
			}
        }
	}
	
	
	
	public List<UILabel> FindLabels( UIFont font)
	{
		List<UILabel> result = new List<UILabel>();
		
		UILabel[] labels = FindObjectsOfType(typeof(UILabel)) as UILabel[];	
		
		foreach (UILabel l in labels)
		{
			if (l.font == font) result.Add(l);
		}
		return result;
	}
	
	
	public void SwapFont(List<UILabel> labels, UIFont font)
	{
		foreach (UILabel label in labels) label.font = font; 	
	}
	
		
	
	
	public List<UIWidget> FindWidgets(UIAtlas atlas, string spriteName)
	{

		List<UIWidget> found = new List<UIWidget>();
		
		UIWidget[] widgets = FindObjectsOfType(typeof(UIWidget)) as UIWidget[];
		
        foreach (UIWidget widget in widgets) 
		{
			if (GetSprite(widget) != null)
			{
				if (GetSprite(widget).atlas == atlas)
				{
					if (GetSprite(widget).spriteName == spriteName) found.Add(widget);
				}	
			}
        }
		return found;
	}
	
	
	public UISprite GetSprite(UIWidget widget)
	{
		UISprite result = null;
		if (widget.gameObject.GetComponent<UISprite>() != null)
		{
			result = widget.gameObject.GetComponent<UISprite>();
		}
		return result;
	}
	
	
	public void SwapSprites()
	{
		foreach (UISprite sprite in widgetsFound) 
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
	
	
	
	public void SetAtlas(List<UIWidget> widgets, UIAtlas targetAtlas)
	{
		foreach (UIWidget widget in widgets)	
		{
			if (widget.gameObject.GetComponent<UISprite>() != null)
			{
				UISprite sprite = widget.gameObject.GetComponent<UISprite>();
				sprite.atlas = targetAtlas;
				sprite.MarkAsChanged();
			}
		}
	}
	
	
	
	public void ClearFoundWidgets()
	{
		widgetsFound = new List<UIWidget>();
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
	
	public void SetDepth(Transform[] selection, int depth)
	{
		
		
		foreach (Transform go in selection)
		{
			
			if (go.GetComponent<UIWidget>() != null)
			{
				go.GetComponent<UIWidget>().depth = depth;
				go.GetComponent<UIWidget>().MarkAsChanged();
			}
		}
	}
	
	
	
	public List<Transform> GetObjectsUsingAtlas( UIAtlas atlas )
	{
		
		List<Transform> result = new List<Transform>();
		
		UISprite[] sprites = FindObjectsOfType(typeof(UISprite)) as UISprite[];
		UILabel[] labels = FindObjectsOfType(typeof(UILabel)) as UILabel[];
       
		foreach (UISprite sprite in sprites) 
		{
			if (sprite.atlas == atlas) result.Add(sprite.transform);	
		}
		
		foreach (UILabel label in labels) 
		{
			if (label.font.atlas != null)
			{
				if (label.font.atlas == atlas) result.Add(label.transform);
			}
		}
		
		return result;
		
	}
	
	
	
}
