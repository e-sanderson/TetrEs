using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BlockUpdater {

    // Object References
    private static Tetromino[] tetrominos = Resources.LoadAll<Tetromino>("Prefabs/Tetrominos");
    private static Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Minos");
    private static Material[] materials = Resources.LoadAll<Material>("Materials/Tetrominos");
    private static TextMeshPro uiFontPrefab = Resources.Load<TextMeshPro>("Prefabs/uiText");
    public static GameObject[] activeMinos;
    public static GameObject[] activeGhosts;

    // Color Transition constants
    private static uint[][] colorPalletArray = new uint[][]
    {
        // {Border I L O R S T Z}
        // B P SSS PPP (B=Border, P=Primary, S=Secondary)
        new uint[] { 0x404040, 0x00FFFF, 0xFFA500, 0xFFFF00, 0x0000FF, 0x00FF00, 0xA020F0, 0xFF0000, },
        new uint[] { 0x404040, 0xFF0000, 0xFF00C0, 0x0000FF, 0xC000FF, 0xFFFF00, 0x00FF00, 0xFF8000, },  //  Rainbow
        new uint[] { 0x405040, 0x00FF00, 0x00E000, 0x00C000, 0x80FF80, 0x80FF00, 0x00FF80, 0x40C040, },  //  Green
        new uint[] { 0x504040, 0xFF0000, 0xE00000, 0xC00000, 0xFF8080, 0xFF8000, 0xFF0080, 0xC04040, },  //  Red
        new uint[] { 0x404050, 0x0000FF, 0x0000E0, 0x0000C0, 0x8080FF, 0x0080FF, 0x8000FF, 0x4040C0, },  //  Blue
        new uint[] { 0x505040, 0x80FF80, 0xE00000, 0xC00000, 0xFF0000, 0x80FF00, 0x00FF80, 0x40C040, },  //  Green/Red
        new uint[] { 0x405050, 0x8080FF, 0x00E000, 0x00C000, 0x00FF00, 0x0080FF, 0x8000FF, 0x4040C0, },  //  Blue/Green
        new uint[] { 0x504050, 0xFF0080, 0x0000E0, 0x0000C0, 0x0000FF, 0xFF0000, 0xE00000, 0xC00000, },  //  Red/Blue
        new uint[] { 0x404040, 0xFF0080, 0x606060, 0x808080, 0xA0A0A0, 0xE60074, 0xCC0066, 0xB3005A, },  //  Pink/Gray
        new uint[] { 0x405040, 0xFF0080, 0x80FF80, 0x80FF00, 0x00FF80, 0xE60074, 0xCC0066, 0xB3005A, },  //  Pink/Green
        new uint[] { 0x505040, 0xC040FF, 0xFFFF00, 0xFFFF40, 0xC0C040, 0xC000FF, 0x8100C0, 0x8140C0, },  //  Purple/Yellow
        new uint[] { 0x00076F, 0xAB00FF, 0x9F45B0, 0xE54ED0, 0xFF00A1, 0x8CD4FF, 0x0092FF, 0x7200EA, },  //  Galaxy
        new uint[] { 0x404040, 0xA0FAA0, 0xFFC0FA, 0xFF95FA, 0xFF6AFA, 0x75FF75, 0x4AFF4A, 0x20FF20, },  //  Mint Green/ Light Pink
        new uint[] { 0x404040, 0x00FF00, 0xA0A0A0, 0x606060, 0x808080, 0x00C000, 0x80FF00, 0x00FF80, },  //  Green/Gray
        new uint[] { 0x404040, 0xFF0080, 0x808080, 0xA0A0A0, 0x606060, 0xFF0000, 0xE00000, 0xC00000, },  //  Red/Gray
        new uint[] { 0x50404A, 0xFF0000, 0xC000FF, 0x8100C0, 0x8140C0, 0xE00000, 0x404040, 0x606060, },  //  Purple/Red
        new uint[] { 0x505040, 0x00FFFF, 0xFFFF00, 0x80FF00, 0xC0FF00, 0x00FF80, 0x00FFC0, 0x80FFC0, },  //  Teal/Yellow
        new uint[] { 0x708F8F, 0x00C0C0, 0xB8E3DD, 0xD3E6EB, 0xF0FFFF, 0x00FFFF, 0xC0FFFF, 0x60C0F0, },  //  Winter
        new uint[] { 0x7D685F, 0x65A33C, 0xCA8C4C, 0xC49536, 0x9C6437, 0x73CF49, 0x439E5D, 0x2B7B31, },  //  Forest
        new uint[] { 0x7D685F, 0xFF0080, 0xCA8C4C, 0xC49536, 0x9C6437, 0xE60074, 0xCC0066, 0xB3005A, },  //  Pink/Brown
    };

    private static int colorIndex = 0;
    private static uint[] pallet;

    // Sprite Change Parameters
    private static int spriteIndex = 0;


    /*
     * Color
     */

    // Color shuffling
    public static void shuffleColorPallet() {
        Queue<uint[]> palletQueue = new Queue<uint[]>();
        List<uint[]> pallets = new List<uint[]>(colorPalletArray);
        while (pallets.Count > 0) {
            pallet = pallets[Random.Range(0, pallets.Count)];
            palletQueue.Enqueue(pallet);
            pallets.Remove(pallet);
        }
        colorPalletArray = palletQueue.ToArray();
    }

    public static void nextColorPallet() {
        colorIndex = colorIndex + 1 < colorPalletArray.Length ? colorIndex + 1 : 0;
        
    }

    public static void prevColorPallet() {
        colorIndex = colorIndex - 1 >= 0 ? colorIndex - 1 : colorPalletArray.Length - 1;
    }

    public static uint[] currentPallet() {
        return colorPalletArray[colorIndex];
    }

    // Updates all blocks with the current color pallet
    public static void updateColors() {

        pallet = colorPalletArray[colorIndex];

        float h, s, v;
        Color.RGBToHSV(Graphics.Hex2Color(pallet[0]), out h, out s, out v);

        Color32 fontColor = Color.HSVToRGB(h, s * 3, v * 3);

        uiFontPrefab.fontSharedMaterial.SetColor(ShaderUtilities.ID_FaceColor, fontColor);
        uiFontPrefab.fontSharedMaterial.SetColor(ShaderUtilities.ID_OutlineColor, fontColor);
        for (int i = 0; i < materials.Length; i++) {
            materials[i].SetColor("_Color", Graphics.Hex2Color(pallet[i]));
        }
    }


    /*
     * Sprites
     */

    public static void nextSprite() {
        spriteIndex = spriteIndex + 1 < sprites.Length ? spriteIndex + 1 : 0;
    }

    public static void prevSprite() {
        spriteIndex = spriteIndex - 1 >= 0 ? spriteIndex - 1 : sprites.Length - 1;
    }

    // Updates all blocks with the current sprite selected
    public static void updateSprites() {

        Sprite sprite = sprites[spriteIndex];

        // Update all present Minos
        foreach (GameObject mino in GameObject.FindGameObjectsWithTag("Mino")) {
            mino.GetComponent<SpriteRenderer>().sprite = sprite;
        }
        if (activeMinos != null) {
            foreach (GameObject mino in activeMinos) {
                if (mino != null) {
                    mino.GetComponent<SpriteRenderer>().sprite = sprite;
                }
            }
        }
        // Update all present Ghost Minos
        foreach (GameObject ghost in GameObject.FindGameObjectsWithTag("GhostMino")) {
            ghost.GetComponent<SpriteRenderer>().sprite = sprite; ;
            Color32 c = ghost.GetComponent<SpriteRenderer>().color;
            ghost.GetComponent<SpriteRenderer>().color = new Color32(c.r, c.g, c.b, (byte)128);
        }
        if (activeGhosts != null) {
            foreach (GameObject ghost in activeGhosts) {
                if (ghost != null) {
                    ghost.GetComponent<SpriteRenderer>().sprite = sprite;
                    Color32 c = ghost.GetComponent<SpriteRenderer>().color;
                    ghost.GetComponent<SpriteRenderer>().color = new Color32(c.r, c.g, c.b, (byte)128);
                }
            }
        }
        // Update future prefab Tetrominos
        foreach (Tetromino t in tetrominos) {
            foreach (Transform minoTransform in t.getPiece().transform) {
                minoTransform.gameObject.GetComponent<SpriteRenderer>().sprite = sprite;
            }
            foreach (Transform minoTransform in t.getGhost().transform) {
                minoTransform.gameObject.GetComponent<SpriteRenderer>().sprite = sprite;
                Color32 c = minoTransform.gameObject.GetComponent<SpriteRenderer>().color;
                minoTransform.gameObject.GetComponent<SpriteRenderer>().color = new Color32(c.r, c.g, c.b, (byte)128);
            }
        }
    }

}