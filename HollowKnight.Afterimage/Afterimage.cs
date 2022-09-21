namespace Afterimage;
[Serializable]
public class Settings
{
    public bool on = true;
    public int r = 4;
    public int g = 2;
    public int b = 4;
}
public class ImageAnimation : MonoBehaviour
{
    public tk2dSpriteAnimationClip clip;
    public ImagePool pool;
    float time;
    void Update()
    {
        var newAnimator = gameObject.GetAddComponent<tk2dSpriteAnimator>();
        newAnimator.Play(clip);
        time += Time.deltaTime;
        newAnimator.Sprite.color = new Color(Afterimage.afterimage.settings_.r/4.0f, Afterimage.afterimage.settings_.g/ 4.0f, Afterimage.afterimage.settings_.b/ 4.0f, 0.5f*(1 - time / 0.5f));
        if (time > 0.5)
        {
            time = 0;
            gameObject.SetActive(false);
            pool.inactiveKnights.Add(gameObject);
        }
    }
}
public class ImagePool
{
    public GameObject knightTemplate;
    public List<GameObject> inactiveKnights = new List<GameObject>();
    public GameObject instantiate(Vector3 positon, Quaternion rotation, Vector3 scale)
    {
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
        }
        newKnight.transform.position = new Vector3(positon.x,positon.y,positon.z+1e-3f);
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
        if (time > 0.1 && Afterimage.afterimage.settings_.on)
        {
            var originalKnight = HeroController.instance.gameObject;
            var newKnight = pool.instantiate(originalKnight.transform.position, originalKnight.transform.rotation, originalKnight.transform.localScale);
            DontDestroyOnLoad(newKnight);
            try
            {
                time = 0;
                var originalAnimator = originalKnight.GetComponent<tk2dSpriteAnimator>();
                var newAnimator = newKnight.GetAddComponent<tk2dSpriteAnimator>();
                newAnimator.SetSprite(originalAnimator.Sprite.Collection, originalAnimator.Sprite.spriteId);
                newAnimator.Library = originalAnimator.Library;
                var originalClip = originalAnimator.CurrentClip;
                var newClip = new tk2dSpriteAnimationClip();
                newClip.CopyFrom(originalClip);
                newClip.frames = new tk2dSpriteAnimationFrame[1];
                newClip.frames[0] = originalClip.frames[originalAnimator.CurrentFrame];
                newClip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Once;
                var imageAnimation = newKnight.GetAddComponent<ImageAnimation>();
                imageAnimation.clip = newClip;
                imageAnimation.pool = pool;
                newAnimator.enabled = false;
            }
            catch (Exception ex)
            {
                UnityEngine.Object.Destroy(newKnight);
            }
        }
    }
}
public class Afterimage : Mod, IGlobalSettings<Settings>, IMenuMod
{
    public static Afterimage afterimage;
    public Settings settings_ = new();
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
        afterimage = this;
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
        menus.Add(
            new()
            {
                Values = new string[]
                {
                    "Red=0.00",
                    "Red=0.25",
                    "Red=0.50",
                    "Red=0.75",
                    "Red=1.00",
                },
                Saver = i => settings_.r = i,
                Loader = () => settings_.r
            }
        );
        menus.Add(
            new()
            {
                Values = new string[]
                {
                    "Green=0.00",
                    "Green=0.25",
                    "Green=0.50",
                    "Green=0.75",
                    "Green=1.00",
                },
                Saver = i => settings_.g = i,
                Loader = () => settings_.g
            }
        );
        menus.Add(
            new()
            {
                Values = new string[]
                {
                    "Blue=0.00",
                    "Blue=0.25",
                    "Blue=0.50",
                    "Blue=0.75",
                    "Blue=1.00",
                },
                Saver = i => settings_.b = i,
                Loader = () => settings_.b
            }
        );
        return menus;
    }
}
