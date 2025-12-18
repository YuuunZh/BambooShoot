using UnityEngine;

[CreateAssetMenu(fileName = "NewBambooStyle", menuName = "Bamboo/Bamboo Style Data")]
public class BambooStyleData : ScriptableObject
{
    public string StyleName;        // 造型名稱
    public string StyleID;          // 唯一識別碼 (e.g., "SR_01", "SSS_03")
    public Sprite StyleSprite;      // 造型圖片
    public Sprite StyleForDiary;
    public Sprite OriginalPlace;
    public Sprite Grade;
    public Sprite BoxSprite; // 該竹筍對應的箱子圖片

    public string Breif;            // 簡介產地
    public string Personality;      // 左下_個性
    public string Characteristic;   // 左下_特徵
    public string ReasonShowUp;     // 左下_出現原因
    [TextArea]
    public string PlaceIntro;       // 右下_地點介紹
    public string Level;            // 所屬等級
}
