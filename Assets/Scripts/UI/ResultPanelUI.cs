using UnityEngine;
using TMPro;

public class ResultPanelUI : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_Text pillText;      // TMP 1: Keterangan singkat di kapsul atas
    public TMP_Text titleText;     // TMP 2: Tulisan utama (VICTORY / LOSE / DRAW)
    public TMP_Text detailText;    // TMP 3: Teks deskripsi detail di bawah

    public void Setup(string pill, string title, string details)
    {
        if (pillText != null && !string.IsNullOrEmpty(pill)) 
            pillText.text = pill;
            
        if (titleText != null && !string.IsNullOrEmpty(title)) 
            titleText.text = title;
            
        if (detailText != null && !string.IsNullOrEmpty(details)) 
            detailText.text = details;
    }
}
