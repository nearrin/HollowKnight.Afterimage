using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using Modding;
using UnityEngine;
namespace Afterimage
{
    public class Settings
    {
        public bool on = true;
        public int r = 2;
        public int g = 4;
        public int b = 4;
    }
    public class ImageAnimation : MonoBehaviour
    {
        public tk2dSpriteAnimationClip clip;
        public ImagePool pool;
        float time;
        void Update()
        {
            var newAnimator = gameObject.GetComponent<tk2dSpriteAnimator>();
            if (newAnimator == null)
            {
                newAnimator= gameObject.AddComponent<tk2dSpriteAnimator>();
            }
            newAnimator.Play(clip);
            time += Time.deltaTime;
            newAnimator.Sprite.color = new Color(Afterimage.afterimage.settings_.r / 4.0f, Afterimage.afterimage.settings_.g / 4.0f, Afterimage.afterimage.settings_.b / 4.0f, 0.5f * (1 - time / 0.5f));
            if (time > 0.5)
            {
                time = 0;
                gameObject.SetActive(false);
                pool.inactiveKnights.Add(gameObject);
            }
            gameObject.tag = "Untagged";
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
            newKnight.transform.position = new Vector3(positon.x, positon.y, positon.z + 1e-3f);
            newKnight.transform.rotation = rotation;
            newKnight.transform.localScale = scale;
            newKnight.name = "newKnight";
            newKnight.tag = "Untagged";
            return newKnight;
        }
    }
    public class ImageGenerator : MonoBehaviour
    {
        public ImagePool pool = new ImagePool();
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
                    var newAnimator = newKnight.GetComponent<tk2dSpriteAnimator>();
                    if (newAnimator == null)
                    {
                        newAnimator=newKnight.AddComponent<tk2dSpriteAnimator>();
                    }
                    newAnimator.SetSprite(originalAnimator.Sprite.Collection, originalAnimator.Sprite.spriteId);
                    newAnimator.Library = originalAnimator.Library;
                    var originalClip = originalAnimator.CurrentClip;
                    var newClip = new tk2dSpriteAnimationClip();
                    newClip.CopyFrom(originalClip);
                    newClip.frames = new tk2dSpriteAnimationFrame[1];
                    newClip.frames[0] = originalClip.frames[originalAnimator.CurrentFrame];
                    newClip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Once;
                    var imageAnimation = newKnight.GetComponent<ImageAnimation>();
                    if (imageAnimation == null)
                    {
                        imageAnimation = newKnight.AddComponent<ImageAnimation>();
                    }
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
    public class Afterimage : Mod, ITogglableMod
    {
        public static Afterimage afterimage;
        public Settings settings_ = new Settings();
        ImagePool pool = new ImagePool();
        public override string GetVersion() => "1.1.0.0";
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
                HeroController.instance.gameObject.AddComponent<ImageGenerator>().pool = pool;
            }
        }
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            afterimage = this;
            ModHooks.Instance.HeroUpdateHook += HeroUpdateHook;
            var battleControl = preloadedObjects["GG_Mighty_Zote"]["Battle Control"];
            var knightTemplate = battleControl.transform.Find("Zotelings").gameObject.transform.Find("Ordeal Zoteling").gameObject;
            UnityEngine.Object.Destroy(knightTemplate.GetComponent<PersistentBoolItem>());
            UnityEngine.Object.Destroy(knightTemplate.GetComponent<ConstrainPosition>());
            UnityEngine.Object.Destroy(knightTemplate.GetComponent<HealthManager>());
            UnityEngine.Object.Destroy(knightTemplate.GetComponent<DamageHero>());
            UnityEngine.Object.Destroy(knightTemplate.GetComponent<UnityEngine.BoxCollider2D>());
            UnityEngine.Object.Destroy(knightTemplate.GetComponent<PlayMakerFSM>());
            UnityEngine.Object.Destroy(knightTemplate.GetComponent<PlayMakerFSM>());
            UnityEngine.Object.Destroy(knightTemplate.GetComponent<PlayMakerFixedUpdate>());
            UnityEngine.Object.Destroy(knightTemplate.GetComponent<UnityEngine.AudioSource>());
            UnityEngine.Object.Destroy(knightTemplate.GetComponent<SpriteFlash>());
            UnityEngine.Object.Destroy(knightTemplate.GetComponent<PersonalObjectPool>());
            UnityEngine.Object.Destroy(knightTemplate.GetComponent<Recoil>());
            UnityEngine.Object.Destroy(knightTemplate.GetComponent<EnemyDreamnailReaction>());
            UnityEngine.Object.Destroy(knightTemplate.GetComponent<EnemyDeathEffectsUninfected>());
            UnityEngine.Object.Destroy(knightTemplate.GetComponent<EnemyHitEffectsUninfected>());
            UnityEngine.Object.Destroy(knightTemplate.GetComponent<ExtraDamageable>());
            knightTemplate.tag = "Untagged";
            pool.knightTemplate = knightTemplate;
        }
        public void Unload()
        {
        }
    }
}
