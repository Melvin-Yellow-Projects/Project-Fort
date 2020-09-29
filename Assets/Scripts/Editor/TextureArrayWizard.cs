/**
 * File Name: TextureArrayWizard.cs
 * Description: TODO: TextureArrayWizard script description
 * 
 * Authors: Catlike Coding, Will Lacey
 * Date Created: September 27, 2020
 * 
 * Additional Comments: 
 *      The original version of this file can be found here:
 *      https://catlikecoding.com/unity/tutorials/hex-map/ within Catlike Coding's tutorial series:
 *      Hex Map; this file has been updated it to better fit this project
 *
 *      Why create an asset vs. an instance during play mode?
 *      
 *          The advantage of using an asset is that we don't have to spend time in play mode to
 *          create the texture array. We don't have to include the individual textures in builds,
 *          only to copy them and then no longer use them.
 *
 *          The disadvantage is that the custom asset is fixed. Unity doesn't automatically change
 *          its texture format depending on the build target. So you have to make sure to create
 *          the asset with the correct texture format, and manually recreate it when you need a
 *          different format. Of course, you could automate this with a build script.
 **/


using UnityEditor;
using UnityEngine;

/// <summary>
/// TODO: TextureArrayWizard class description
/// </summary>
public class TextureArrayWizard : ScriptableWizard
{
    /********** MARK: Variables **********/
    #region Variables

    public Texture2D[] textures;

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    // "To access the wizard via the editor, we have to add this method to Unity's menu. This is 
    // done by adding the MenuItem attribute to the method. Let's add it to the Assets menu, 
    // specifically Assets / Create / Texture Array."
    [MenuItem("Assets/Create/Texture Array")]
    static void CreateWizard()
    {
        // "We can open our wizard via the generic static ScriptableWizard.DisplayWizard method. Its
        // parameters are the names of the wizard's window and its create button."
        ScriptableWizard.DisplayWizard<TextureArrayWizard>(             
            "Create Texture Array", "Create"
        );
    }

    // "When you press the wizard's Create button, it will disappear. Also, Unity will complain that
    // there's no OnWizardCreate method. This is the method that gets invoked when the create button
    // is pressed, so we should add it to our wizard."
    void OnWizardCreate()
    {
        // no textures were added to the panel
        if (textures.Length == 0) return;

        // save file options; "The next step is to ask the user where to save the texture array
        // asset. We can open a save file panel via the EditorUtility.SaveFilePanelInProject method.
        // Its parameters determine the panel name, default file name, the file extension, and
        // description. Texture arrays use the generic asset file extension."
        string path = EditorUtility.SaveFilePanelInProject(
            "Save Texture Array", "Texture Array", "asset", "Save Texture Array"
        );

        // user canceled the panel, return because the path was resultantly invalid
        if (path.Length == 0) return;

        // create a new Texture2DArray object
        Texture2D t = textures[0];

        // "Its constructor method requires the texture width and height, the array length, the
        // texture format, and whether there are mipmaps. These settings have to be the same for all
        // textures in the array. We'll use the first texture to configure the object. It's up to
        // the user to make sure that all textures have the same format."
        Texture2DArray textureArray = new Texture2DArray(
            t.width, t.height, textures.Length, t.format, t.mipmapCount > 1
        );

        // "As the texture array is a single GPU resource, it uses the same filter and wrap modes
        // for all textures. Once again, we'll use the first texture to configure this."
        textureArray.anisoLevel = t.anisoLevel;
        textureArray.filterMode = t.filterMode;
        textureArray.wrapMode = t.wrapMode;

        // "Now we can copy the textures to the array [...] This method copies the raw texture data,
        // one mip level at a time. So we have to loop through all textures and their mip levels.
        // The parameters of the method are two sets consisting of a texture resource, index, and
        // mip level. As the source textures aren't arrays, their index is always zero."
        for (int i = 0; i < textures.Length; i++)
        {
            for (int m = 0; m < t.mipmapCount; m++)
            {
                Graphics.CopyTexture(textures[i], 0, m, textureArray, i, m);
            }
        }

        // write the data to a file in the project, and it will appear in the project window
        AssetDatabase.CreateAsset(textureArray, path);

        // this needs to be done manually after creating the asset as this is a Read Only property;
        // we do this because we don't need to read pixel data from the array
        //textureArray.isReadable = false;
    }

    #endregion
}