using UnityEngine;

[CreateAssetMenu(fileName = "NewBambooStyle", menuName = "Bamboo/Bamboo Style Data")]
public class BambooStyleData : ScriptableObject
{
    public string StyleName;        // 造型名稱
    public string StyleID;          // 唯一識別碼 (e.g., "SR_01", "SSS_03")
    public Sprite StyleSprite;      // 造型圖片
    public Sprite StyleForDiary;
    public Sprite OriginalPlace;
    [TextArea]
    public string Introduction;     // 造型介紹/青農日記內容
    public string Level;            // 所屬等級 (e.g., "SSS", "SR")
}
