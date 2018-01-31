using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PolyToolkit;

public class MulitAssets : MonoBehaviour
{
    public GameObject singleAssetsprefab;
    public GameObject backboard;
    public static Transform InstanceBackboard { get { return backboardSingleton; } }
    public static GameObject InstanceSingleAsset { get { return singleAssetPrefSingleton; } }
    public static bool creatingAssestsMessageFlag;
    public static string numberOfAssetsFoundFromPoly;
    private static Transform backboardSingleton;
    private static GameObject singleAssetPrefSingleton;
    //Moving the Panel back and forth
    private float startingPanelsSum;
    private float totalPanelSum;

    void Start()
    {
        //Setting up singleton 
        backboardSingleton = backboard.transform;
        singleAssetPrefSingleton = singleAssetsprefab;
        startingPanelsSum = 304;
        totalPanelSum = startingPanelsSum;
    }

    //Destorying each child on the backpanel
    public static void destoryChildern()
    {
        foreach (Transform child in backboardSingleton.transform)
        {
            Destroy(child.gameObject);
        }
    }

    //Creating Thumbnail 
    public void createAssetsThumbnail(PolyAsset asset, PolyStatus status)
    {
        if (!status.ok)
        {
            return;
        }
        else
        {
            //Getting Button
            GameObject newButton = Instantiate(singleAssetPrefSingleton) as GameObject;
            newButton.transform.SetParent(backboardSingleton, false);
            singleAssestDisplay newButtonObject = newButton.GetComponent<singleAssestDisplay>();
            //Setting up each button with props
            newButtonObject.Setup(asset);
        }
    }

    //Looping over the assets that matches your search 
    public void createAssest(PolyStatusOr<PolyListAssetsResult> result)
    {
        if (!result.Ok)
        {
            Debug.LogError("Failed to get featured assets. :( Reason: " + result.Status);
            return;
        }
        numberOfAssetsFoundFromPoly = result.Value.assets.ToArray().Length.ToString();
        creatingAssestsMessageFlag = true;

        foreach (PolyAsset asset in result.Value.assets)
        {
            PolyApi.FetchThumbnail(asset, createAssetsThumbnail);

        }

    }

    //When the user press the next button on the UI, the panel moves 
    public void movePanelNext()
    {
        if (totalPanelSum + 101 < backboard.transform.childCount * 101 + 101)
        {
            var panel = backboard.GetComponent<RectTransform>();
            panel.localPosition += Vector3.right * -101;
            totalPanelSum = totalPanelSum + 101;
        }
    }

    public void movePanelBack()
    {
        if (totalPanelSum - 101 > startingPanelsSum - 101)
        {
            var panel = backboard.GetComponent<RectTransform>();
            panel.localPosition += Vector3.right * 101;
            totalPanelSum = totalPanelSum - 101;
        }
    }
}
