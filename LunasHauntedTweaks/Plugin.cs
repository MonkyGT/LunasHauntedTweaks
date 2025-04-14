using BepInEx;
using BepInEx.Bootstrap;
using System;
using System.ComponentModel;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Player = GorillaLocomotion.GTPlayer;

namespace HMMLunasTweaks
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class HauntedModMenuPlugin : BaseUnityPlugin
    {
        private bool inRoom;
        private GameObject menuObject = null;

        private AssetBundle LoadFontBundle() {
            // TODO: remake assetbundle
            var Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("LunasHauntedTweaks.Resources.hmmlunastweaksfont");
            return AssetBundle.LoadFromStream(Stream);
        }

        private void Start()
        {
            // usage of Awake() now locks the game up on boot so we need this to be in Start now :(
            Harmony.CreateAndPatchAll(GetType().Assembly, PluginInfo.GUID);
            var fontBundle = LoadFontBundle();
            if (fontBundle == null)
                return;

            Utils.RefCache.CustomFont = fontBundle.LoadAsset<Font>("BitCell");
            fontBundle.Unload(false);

            foreach (BepInEx.PluginInfo plugin in Chainloader.PluginInfos.Values)
            {
                BaseUnityPlugin modPlugin = plugin.Instance;
                Type type = modPlugin.GetType();
                DescriptionAttribute modDescription = type.GetCustomAttribute<DescriptionAttribute>();

                if (modDescription == null)
                    continue;

                if (modDescription.Description.Contains("HauntedModMenu"))
                {
                    var enableImp = AccessTools.Method(type, "OnEnable");
                    var disableImp = AccessTools.Method(type, "OnDisable");

                    if (enableImp != null && disableImp != null)
                        Utils.RefCache.ModList.Add(new Utils.ModInfo(modPlugin, plugin.Metadata.Name));
                }
            }

            GorillaTagger.OnPlayerSpawned(OnGameInitialized);
        }

        private void OnEnable()
        {
            if (menuObject != null && inRoom)
                menuObject.SetActive(true);
        }

        private void OnDisable()
        {
            if (menuObject != null)
                menuObject.SetActive(false);
        }

        private void OnGameInitialized()
        {
            Utils.RefCache.LeftHandFollower = Player.Instance.leftHandFollower.gameObject;
            Utils.RefCache.RightHandFollower = Player.Instance.rightHandFollower.gameObject;
            Utils.RefCache.CameraTransform = Player.Instance.headCollider.transform;
            Utils.RefCache.PlayerTransform = Player.Instance.turnParent.transform;

            Utils.RefCache.LeftHandRig = GorillaTagger.Instance.offlineVRRig.leftHandTransform.parent.gameObject;
            Utils.RefCache.RightHandRig = GorillaTagger.Instance.offlineVRRig.rightHandTransform.parent.gameObject;

            NetworkSystem.Instance.OnJoinedRoomEvent += OnJoin;
            NetworkSystem.Instance.OnReturnedToSinglePlayer += OnLeave;
        }

        /* TODO: consider unlocking for non modded rooms
         * hmm only toggles mods which should have their own
         * modded lobby restrictions anyway. This is a rather
         * redundant restriction and people can turn it off
         * at Computer Interface or whatever anyway
        */
        public void OnJoin()
        {
            if (NetworkSystem.Instance.GameModeString.Contains("MODDED"))
            {
                inRoom = true;

                if (menuObject != null)
                    return;

                menuObject = CreateTrigger();

                if (menuObject != null)
                {
                    menuObject.AddComponent<Menu.MenuController>();
                    menuObject.SetActive(enabled && inRoom);
                }
            }
        }
        
        public void OnLeave()
        {
            inRoom = false;
            Destroy(menuObject);
        }

        private GameObject CreateTrigger()
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            if (go == null)
                return null;

            Collider col = go.GetComponent<Collider>();
            if (col != null)
                col.isTrigger = true;

            MeshRenderer render = go.GetComponent<MeshRenderer>();
            if (render != null)
                Destroy(render);

            return go;
        }
    }
}
