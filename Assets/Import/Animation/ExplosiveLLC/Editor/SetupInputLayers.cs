using UnityEditor;
using UnityEngine;

namespace RPGCharacterAnims
{

	public class SetupMessageWindow:EditorWindow
	{
		void OnGUI()
		{
			EditorGUILayout.LabelField("Before attempting to use the Warrior Animation Mecaim Animation Packs, you must first ensure that the layers and inputs are correctly defined.  There is an included Input.preset and Layer.preset which contains all the settings that you can load in.\n \nYou can remove this message by deleting the SetupInputLayers.cs script in the Editor folder.", EditorStyles.wordWrappedLabel);
		}
	}
}