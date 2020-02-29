using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public  class Graphics : MonoBehaviour {

    // Object References
    private static Tetromino[] tetrominos;
    private static Sprite[] sprites;
    private static Material[] materials;
    private static Material fontMaterial;
    private static ParticleEffect explosion;
    private static ParticleEffect sparkle;
    public TextMeshPro uiFontPrefab;
    public GameObject[] activeMinos;
    public GameObject[] activeGhosts;

    // Color Transition Parameters
    private static float colorTransitionTime = 1.0f;
    private static int colorTransitionFrames = 20;
    private static float colorTransitionDelay = colorTransitionTime / colorTransitionFrames;
    private static float dLerpRatio = 1.0f / colorTransitionFrames;

    // Screen Flash Parameters
    private static Queue<Color32> flashColors = new Queue<Color32>(new Color32[] { Color.white, Color.black });
    private static float flashLerpRatio = 0.3f;
    private static uint numFlashes = 14;
    private static float flashDelay = 0.03f;

    // Sprite Change Parameters
    private static int spriteIndex = 0;

    // Trail Rendering Parameters
    private TrailRenderer beam;
    private static int frames = 3;
    private static int fadeFrames = 10;
    private static float da = 255 / fadeFrames;

    // Explosion Parameters
    private uint explosionPoolInitialSize = 40;
    private Queue<ParticleEffect> explosionPool = new Queue<ParticleEffect>(); 

    // Load in prefabs / Initialize
    void Awake() {
        sprites = Resources.LoadAll<Sprite>("Sprites/Minos");
        tetrominos = Resources.LoadAll<Tetromino>("Prefabs/Tetrominos");
        materials = Resources.LoadAll<Material>("Materials/Tetrominos");
        explosion = Resources.Load<ParticleEffect>("Prefabs/Explosion");
        sparkle = Instantiate(Resources.Load<ParticleEffect>("Prefabs/Sparkle"));

        beam = GetComponent<TrailRenderer>();
        initializeExplosionPool();

    }

    /*
     * Color
     */
    
    public void fadeToNextColorPallet() {
        BlockUpdater.nextColorPallet();
        
        StartCoroutine(fadeColorPalletRoutine());
    }

    // Lerps from one color to another in a smooth animation
    private IEnumerator fadeColorPalletRoutine( ) {

        uint[] pallet = BlockUpdater.currentPallet();

        float h, s, v;
        Color.RGBToHSV(Hex2Color(pallet[0]), out h, out s, out v);
        
        Color32 fontColor = Color.HSVToRGB(h, s*3, v*3);
        
        // Save Old Colors for Lerp
        Color32[] oldColors = new Color32[materials.Length];
        for (int i = 0; i < oldColors.Length; i++) {
            oldColors[i] = materials[i].color;
        }
        Color32 oldFontColor = uiFontPrefab.fontSharedMaterial.GetColor(ShaderUtilities.ID_FaceColor);
        
        // Slowly transition
        float lerp = 0;
        while (lerp <= 1) {
            uiFontPrefab.fontSharedMaterial.SetColor(ShaderUtilities.ID_FaceColor, Color32.Lerp(oldFontColor, fontColor, lerp));
            uiFontPrefab.fontSharedMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color32.Lerp(oldFontColor, fontColor, lerp));
            for (int i = 0; i < materials.Length; i++) {
                materials[i].SetColor("_Color", Color32.Lerp(oldColors[i], Hex2Color(pallet[i]), lerp));
            }
                
            lerp += dLerpRatio;
            yield return new WaitForSeconds(colorTransitionDelay);
        }
        //

        BlockUpdater.updateColors();
    }

    // Color conversion
    public static Color32 Hex2Color(uint hex) {
        byte r = (byte)((hex >> 16) & 0xFF);
        byte g = (byte)((hex >> 8) & 0xFF);
        byte b = (byte)(hex & 0xFF);
        Color32 c = new Color32(r, g, b, (byte)255);
        return c;
    }

    /*
     * Screen Flashing
     * */

    public void flash() {
        StartCoroutine(flashRoutine());
    }

    // Makes the screen flash rapidly
    private IEnumerator flashRoutine() {
        uint[] pallet = BlockUpdater.currentPallet();
        for (uint j = 0; j < numFlashes; j++) {
            Color32 c = flashColors.Dequeue();
            materials[0].SetColor("_Color", Color32.Lerp(Hex2Color(pallet[0]), c, flashLerpRatio));
            flashColors.Enqueue(c);
            yield return new WaitForSeconds(flashDelay);
        }
        materials[0].SetColor("_Color", Hex2Color(pallet[0]));
    }

    /*
     * Tetromino Beam (Animation for hard drops)
     */

    // Initializes the beam and starts the animation coroutine
    public void tetrominoBeam(Vector3 start, Vector3 end, Tetromino t) {
        beam.startColor = t.getColor();
        beam.endColor = beam.startColor;
        beam.startWidth = t.dimensions().x + 1;
        beam.endWidth = beam.startWidth;
        StartCoroutine(tetrominoBeamRoutine(start + Vector3.up * 2, end + Vector3.down * 2));
    }

    // Creates a beam that matches the color of the piece and travels from its start location to its end location
    public IEnumerator tetrominoBeamRoutine(Vector3 start, Vector3 end) {

        beam.Clear();
        beam.enabled = false;
        yield return new WaitForFixedUpdate();

        Vector3 dv = (end - start) / frames;
        transform.position = start;
        beam.enabled = true;
        yield return new WaitForFixedUpdate();

        for (int i = 0; i < frames; i++) {
            transform.position += dv;
            Color32 c = beam.startColor;
            c.a -= (byte)da;
            beam.startColor = c;
            beam.endColor = c;
            yield return new WaitForFixedUpdate();
        }
        for (int i = 0; i < fadeFrames - frames; i++) {
            Color32 c = beam.startColor;
            c.a -= (byte)da;
            beam.startColor = c;
            beam.endColor = c;
            yield return new WaitForFixedUpdate();
        }
    }

    /*
     * Explosions
     */
    
    // Initialies a pool of "Explosion" objects for object pooling
    private void initializeExplosionPool() {
        for (int i = 0; i < explosionPoolInitialSize; i++) {

            explosionPool.Enqueue(Instantiate(explosion));
        }
    }

    // Creates a colored explosion at a desired position
    public void createExplosion(Vector3 position, Color32 color) {
        ParticleEffect e = explosionPool.Dequeue();
        if (e.gameObject.activeSelf) {
            explosionPool.Enqueue(e);
            e = Instantiate(explosion);
        }
        e.activate(position, color);
        explosionPool.Enqueue(e);
    }

    /*
     * Sparkle Effect
     */

    // Creates a small sparkle of a desired color at the desired position
    public void createSparkle(Vector3 Position, Color32 color) {
        sparkle.activate(Position, color);
    }

}
