namespace Afterimage;
[Serializable]
public class Settings
{
    public bool on = true;
}
public class ImageAnimation : MonoBehaviour
{
    public tk2dSpriteAnimationClip clip;
    float time;
    void Update()
    {
        var newAnimator = gameObject.GetAddComponent<tk2dSpriteAnimator>();
        newAnimator.Play(clip);
        time += Time.deltaTime;
        if (time > 0.5)
        {
            time = 0;
            gameObject.SetActive(false);
        }
    }
}
public class ImagePool
{
    public GameObject knightTemplate;
    List<GameObject> activeKnights = new List<GameObject>();
    List<GameObject> inactiveKnights = new List<GameObject>();
    public GameObject instantiate(Vector3 positon, Quaternion rotation, Vector3 scale)
    {
        List<GameObject> stillActive = new List<GameObject>();
        foreach (var knight in activeKnights)
        {
            if (!knight.activeSelf)
            {
                inactiveKnights.Add(knight);
            }
            else
            {
                stillActive.Add(knight);
            }
        }
        activeKnights = stillActive;
        GameObject newKnight = null;
        if (inactiveKnights.Count != 0)
        {
            newKnight = inactiveKnights[0];
            newKnight.SetActive(true);
            inactiveKnights.RemoveAt(0);
        }
        else
        {
            newKnight = UnityEngine.Object.Instantiate(knightTemplate);
            UnityEngine.Object.DontDestroyOnLoad(newKnight);
            activeKnights.Add(newKnight);
        }
        newKnight.transform.position = positon;
        newKnight.transform.rotation = rotation;
        newKnight.transform.localScale = scale;
        newKnight.name = "newKnight";
        return newKnight;
    }
}
public class ImageGenerator : MonoBehaviour
{
    public ImagePool pool = new();
    float time;
    void Update()
    {
        time += Time.deltaTime;
        if (time > 0.1)
        {
            time = 0;
            var originalKnight = HeroController.instance.gameObject;
            var originalAnimator = originalKnight.GetComponent<tk2dSpriteAnimator>();
            var newKnight = pool.instantiate(originalKnight.transform.position, originalKnight.transform.rotation, originalKnight.transform.localScale);
            var newAnimator = newKnight.GetAddComponent<tk2dSpriteAnimator>();
            newAnimator.SetSprite(originalAnimator.Sprite.Collection, originalAnimator.Sprite.spriteId);
            newAnimator.Library = originalAnimator.Library;
            var originalClip = originalAnimator.CurrentClip;
            var newClip = new tk2dSpriteAnimationClip();
            newClip.CopyFrom(originalClip);
            newClip.frames = new tk2dSpriteAnimationFrame[1];
            newClip.frames[0] = originalClip.frames[originalAnimator.CurrentFrame];
            newClip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Once;
            newKnight.GetAddComponent<ImageAnimation>().clip = newClip;
            newAnimator.enabled = false;
        }
    }
}
public class Afterimage : Mod, IGlobalSettings<Settings>, IMenuMod
{
    private Settings settings_ = new();
    public bool ToggleButtonInsideMenu => true;
    ImagePool pool = new();
    public Afterimage() : base("Afterimage")
    {
    }
    public override string GetVersion() => "1.0.0.0";
    public override List<(string, string)> GetPreloadNames()
    {
        return new List<(string, string)>
            {
                 ("GG_Mighty_Zote","Battle Control"),
            };
    }
    private void HeroUpdateHook()
    {
        if (HeroController.instance.gameObject.GetComponent<ImageGenerator>() == null)
        {
            HeroController.instance.gameObject.GetAddComponent<ImageGenerator>().pool = pool;
        }
    }
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        ModHooks.HeroUpdateHook += HeroUpdateHook;
        var battleControl = preloadedObjects["GG_Mighty_Zote"]["Battle Control"];
        var knightTemplate = battleControl.transform.Find("Zotelings").gameObject.transform.Find("Ordeal Zoteling").gameObject;
        UnityEngine.Object.Destroy(knightTemplate.GetComponent<PersistentBoolItem>());
        UnityEngine.Object.Destroy(knightTemplate.GetComponent<ConstrainPosition>());
        knightTemplate.RemoveComponent<HealthManager>();
        knightTemplate.RemoveComponent<DamageHero>();
        knightTemplate.RemoveComponent<UnityEngine.BoxCollider2D>();
        knightTemplate.RemoveComponent<PlayMakerFSM>();
        knightTemplate.RemoveComponent<PlayMakerFSM>();
        pool.knightTemplate = knightTemplate;
    }
    public void OnLoadGlobal(Settings settings) => settings_ = settings;
    public Settings OnSaveGlobal() => settings_;
    public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? menu)
    {
        List<IMenuMod.MenuEntry> menus = new();
        menus.Add(
            new()
            {
                Values = new string[]
                {
                    Language.Language.Get("MOH_ON", "MainMenu"),
                    Language.Language.Get("MOH_OFF", "MainMenu"),
                },
                Saver = i => settings_.on = i == 0,
                Loader = () => settings_.on ? 0 : 1
            }
        );
        return menus;
    }
}
