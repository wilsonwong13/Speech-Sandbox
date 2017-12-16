using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PolyToolkit;

public class singleAssestDisplay : MonoBehaviour {
    public Text textName;
    public Image panelImage;
    public static bool assetAppeared;
    public static bool assetGenerate;
    public static string assetName;
    private PolyAsset objectAsset;
    Camera m_MainCamera;

    //When giving an assest, this method is called to initialize
    public void Setup(PolyAsset asset) {
        objectAsset = asset;
        textName.text = asset.displayName;
        Texture2D tex2D = asset.thumbnailTexture;
        panelImage.sprite = Sprite.Create(tex2D, new Rect(0.0f, 0.0f, tex2D.width, tex2D.height), new Vector2(0.5f, 0.5f));
    }

    //When user choose an assest, this method is called. 
    public void ChooseAssest() {
        PolyImportOptions options = PolyImportOptions.Default();
        options.rescalingMode = PolyImportOptions.RescalingMode.FIT;
        options.desiredSize = .5f;
        options.recenter = true;
        PolyApi.Import(objectAsset,options,generatingAsset);
    }

    //This method is importing the poly assest
    void generatingAsset(PolyAsset asset, PolyStatusOr<PolyImportResult> result)
    {
        if (!result.Ok)
        {
            Debug.LogError("Failed to import asset. :( Reason: " + result.Status);
            return;
        }
        assetAppeared = true;
        assetGenerate = true;
        assetName = "Creating: " + asset.displayName;
        result.Value.gameObject.gameObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward;
        result.Value.gameObject.gameObject.transform.rotation = Camera.main.transform.rotation;
        result.Value.gameObject.AddComponent<BoxCollider>();
        result.Value.gameObject.AddComponent<Rigidbody>();
        var rb = result.Value.gameObject.GetComponent<Rigidbody>();
        rb.useGravity = false;
        result.Value.gameObject.AddComponent<MiraPhysicsGrabExample>();
        result.Value.gameObject.AddComponent<MiraPhysicsRaycast>();

    }


}
