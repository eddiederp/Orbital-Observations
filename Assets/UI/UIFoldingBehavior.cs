using TMPro;
using UnityEngine;

public class UIFoldingBehavior : MonoBehaviour
{
    public GameObject infoDisplay;
    public GameObject infoDisplayScrollView;
    public GameObject barChartDisplay;
    public GameObject barChartDisplayScrollView;

    public void SetInfoDisplayFolded(bool folded)
    {
        infoDisplayScrollView.SetActive(!folded);
        infoDisplay.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, folded ? 140 : 700);
    }
    
    public void SetBarChartDisplayFolded(bool folded)
    {
        barChartDisplayScrollView.SetActive(!folded);
        barChartDisplay.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, folded ? 70 : 450);
        barChartDisplay.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, folded ? 450 : 700);
        barChartDisplay.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().alignment = folded ? TextAlignmentOptions.Right : TextAlignmentOptions.Center;
    }
}
