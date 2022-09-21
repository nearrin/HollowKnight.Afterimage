namespace Afterimage;
[Serializable]
public class Settings
{
    public bool on = true;
}
public class Animation:MonoBehaviour
{
    public tk2dSpriteAnimationClip clip=null;
    void Update()
    {
        if (clip != null)
        {
            var newAnimator = gameObject.GetAddComponent<tk2dSpriteAnimator>();
            newAnimator.Play(clip);
        }
    }
}
public class Afterimage : Mod, IGlobalSettings<Settings>, IMenuMod
{
    private Settings settings_ = new();
    public bool ToggleButtonInsideMenu => true;
    GameObject knightTemplate = null;
    List<GameObject> knightPool=new List<GameObject>();
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
    private void ActiveSceneChanged(UnityEngine.SceneManagement.Scene from, UnityEngine.SceneManagement.Scene to)
    {
    }
    private void HeroUpdateHook()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            LogDebug("Creating a new instance.");
            var originalKnight = HeroController.instance.gameObject;
            var originalAnimator = originalKnight.GetComponent<tk2dSpriteAnimator>();
            var newKnight = UnityEngine.Object.Instantiate(knightTemplate,originalKnight.transform.position,originalKnight.transform.rotation);
            newKnight.transform.localScale = originalKnight.transform.localScale;
            UnityEngine.Object.DontDestroyOnLoad(newKnight);
            newKnight.name = "newKnight";
            var newAnimator = newKnight.GetAddComponent<tk2dSpriteAnimator>();
            newAnimator.SetSprite(originalAnimator.Sprite.Collection, originalAnimator.Sprite.spriteId);
            newAnimator.Library=originalAnimator.Library;
            var originalClip = originalAnimator.CurrentClip;
            var newClip = new tk2dSpriteAnimationClip();
            newClip.CopyFrom(originalClip);
            newClip.frames = new tk2dSpriteAnimationFrame[1];
            newClip.frames[0] = originalClip.frames[originalAnimator.CurrentFrame];
            newClip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Once;
            newKnight.GetAddComponent<Animation>().clip = newClip;
        }
    }
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += ActiveSceneChanged;
        ModHooks.HeroUpdateHook += HeroUpdateHook;
        var battleControl = preloadedObjects["GG_Mighty_Zote"]["Battle Control"];
        knightTemplate = battleControl.transform.Find("Zotelings").gameObject.transform.Find("Ordeal Zoteling").gameObject;
        UnityEngine.Object.Destroy(knightTemplate.GetComponent<PersistentBoolItem>());
        UnityEngine.Object.Destroy(knightTemplate.GetComponent<ConstrainPosition>());
        knightTemplate.RemoveComponent<HealthManager>();
        knightTemplate.RemoveComponent<DamageHero>();
        knightTemplate.RemoveComponent<UnityEngine.BoxCollider2D>();
        knightTemplate.RemoveComponent<PlayMakerFSM>();
        knightTemplate.RemoveComponent<PlayMakerFSM>();
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
                Saver = i => settings_.on = i==0,
                Loader = () => settings_.on?0:1
            }
        );
        return menus;
    }
}
