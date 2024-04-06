#if !UNITY_EDITOR
using EFT.UI;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using EFT;
using Aki.Reflection.Utils;
using System.Threading.Tasks;
using System.Linq;
using Comfort.Common;
using System.Collections.Generic;
using EFT.InventoryLogic;
using HeadVoiceSelector.Utils;


namespace HeadVoiceSelector.Core.UI
{
    internal class NewVoiceHeadDrawers
    {
        private static bool customizationDrawersCloned = false;
        private static readonly List<EquipmentSlot> _hiddenSlots = new List<EquipmentSlot>
        {
            EquipmentSlot.Earpiece,
            EquipmentSlot.Eyewear,
            EquipmentSlot.FaceCover,
            EquipmentSlot.Headwear
        };
        protected static readonly GClass766 gclass766_0 = new GClass766();
        private static string[] _availableCustomizations;
        private static Dictionary<int, TagBank> _voices = new Dictionary<int, TagBank>();
        private static int _selectedHeadIndex;
        private static int _selectedVoiceIndex;
        public static GClass723<EquipmentSlot, PlayerBody.GClass1860> slotViews;
        private static List<KeyValuePair<string, GClass2937>> _headTemplates;
        private static List<KeyValuePair<string, GClass2941>> _voiceTemplates;

        // Routes to handle server changes
        public static void WTTChangeHead(string id)
        {
            if (id == null)
            {
                Console.WriteLine("Error: id is null.");
                return;
            }

            var response = WebRequestUtils.Post<string>("/WTT/WTTChangeHead", id);
            if (response != null)
            {
                Console.WriteLine("HeadVoiceSelector: Change Head Route has been requested");
            }
        }

        public static void WTTChangeVoice(string id)
        {
            if (id == null)
            {
                Console.WriteLine("Error: id is null.");
                return;
            }
#if DEBUG
            Console.WriteLine($"WTTChangeVoice: id = {id}");
#endif
            var response = WebRequestUtils.Post<string>("/WTT/WTTChangeVoice", id);
            if (response != null)
            {
                Console.WriteLine("'HeadVoiceSelector': Change Voice Route has been requested");
            }
        }


        // Custom head and voice drawers logic
        public static async Task AddCustomizationDrawers()
        {
            try
            {
                if (customizationDrawersCloned)
                {
#if DEBUG
                    Console.WriteLine("Customization drawers already cloned.");
#endif
                    return;
                }
                else
                {
                    GameObject customizationDrawersPrefab = GameObject.Find("Common UI/Common UI/InventoryScreen/Overall Panel/LeftSide/ClothingPanel");
                    GameObject overallScreenParent = GameObject.Find("Common UI/Common UI/InventoryScreen/Overall Panel/LeftSide");

                    if (customizationDrawersPrefab != null && overallScreenParent != null)
                    {

                        GameObject clonedCustomizationDrawers = GameObject.Instantiate(customizationDrawersPrefab, overallScreenParent.transform);
                        clonedCustomizationDrawers.gameObject.name = "NewHeadVoiceCustomizationDrawers";

                        Vector3 newPosition = clonedCustomizationDrawers.transform.localPosition;
                        newPosition.y -= 50f;
                        clonedCustomizationDrawers.transform.localPosition = newPosition;

                        customizationDrawersCloned = true;

                        if (clonedCustomizationDrawers != null)
                        {
                            Transform headTransform = clonedCustomizationDrawers.transform.Find("Upper");
                            Transform voiceTransform = clonedCustomizationDrawers.transform.Find("Lower");
                            if (headTransform != null && voiceTransform != null)
                            {
                                headTransform.gameObject.name = "Head";
                                voiceTransform.gameObject.name = "Voice";


                                Transform headIconTransform = clonedCustomizationDrawers.transform.Find("Head/Icon");
                                Transform voiceIconTransform = clonedCustomizationDrawers.transform.Find("Voice/Icon");

                                if (headIconTransform != null && voiceIconTransform != null)
                                {
                                    Image headIcon = headIconTransform.GetComponent<Image>();
                                    Image voiceIcon = voiceIconTransform.GetComponent<Image>();

                                    var headIconPng = Path.Combine(HeadVoiceSelector.modPath, "db", "Images", "Icons", "icon_face_selector.png");
                                    var voiceIconPng = Path.Combine(HeadVoiceSelector.modPath, "db", "Images", "Icons", "icon_voice_selector.png");

                                    if (headIconPng != null && voiceIconPng != null)
                                    {
                                        byte[] headIconByte = File.ReadAllBytes(headIconPng);
                                        byte[] voiceIconByte = File.ReadAllBytes(voiceIconPng);

                                        Texture2D headIcontexture = new Texture2D(2, 2);
                                        Texture2D voiceIcontexture = new Texture2D(2, 2);

                                        ImageConversion.LoadImage(headIcontexture, headIconByte);
                                        ImageConversion.LoadImage(voiceIcontexture, voiceIconByte);

                                        Sprite headIconSprite = Sprite.Create(headIcontexture, new Rect(0, 0, headIcontexture.width, headIcontexture.height), Vector2.zero);
                                        Sprite voiceIconSprite = Sprite.Create(voiceIcontexture, new Rect(0, 0, voiceIcontexture.width, voiceIcontexture.height), Vector2.zero);

                                        headIcon.sprite = headIconSprite;
                                        voiceIcon.sprite = voiceIconSprite;
                                    }
                                    else
                                    {
                                        Console.WriteLine("Couldn't find icons/icon path for new customization dropdowns");
                                    }




                                }

                                Transform headSelectorTransform = clonedCustomizationDrawers.transform.Find("Head/ClothingSelector");
                                Transform voiceSelectorTransform = clonedCustomizationDrawers.transform.Find("Voice/ClothingSelector");
                                if (headSelectorTransform != null && voiceSelectorTransform != null)
                                {
                                    headSelectorTransform.gameObject.name = "HeadSelector";
                                    voiceSelectorTransform.gameObject.name = "VoiceSelector";

                                    DropDownBox headDropDownBox = headSelectorTransform.GetComponent<DropDownBox>();
                                    DropDownBox voiceDropDownBox = voiceSelectorTransform.GetComponent<DropDownBox>();

                                    if (headDropDownBox != null && voiceDropDownBox != null)
                                    {
                                        await getAvailableCustomizations(Singleton<ClientApplication<ISession>>.Instance.GetClientBackEndSession());
                                        InitCustomizationDropdowns(_availableCustomizations, headDropDownBox, voiceDropDownBox);
                                        setupCustomizationDrawers(headDropDownBox, voiceDropDownBox);


                                        clonedCustomizationDrawers.gameObject.SetActive(true);
#if DEBUG
                                        Console.WriteLine("Successfully cloned and setup new customization dropdowns!");
#endif
                                    }
                                    else
                                    {
                                        Console.WriteLine("headDropdownBox or voiceDropdownBox is null");
                                    }

                                }
                                else
                                {
                                    Console.WriteLine("headSelectorTransform or voiceSelectorTransform is null");
                                }
                            }
                            else
                            {
                                Console.WriteLine("headTransform or voiceTransform are null");
                            }

                        }
                        else
                        {
                            Console.WriteLine("clonedCustomizationDrawers is null");
                        }
                    }
                    else
                    {
                        Console.WriteLine("customizationDrawersPrefab or overallParent not found");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        public static void InitCustomizationDropdowns(string[] availableCustomizations, DropDownBox _headSelector, DropDownBox _voiceSelector)
        {
            try
            {

                gclass766_0.Dispose();

                _availableCustomizations = availableCustomizations;

                gclass766_0.SubscribeEvent<int>(_headSelector.OnValueChanged, new Action<int>(selectHeadEvent));

                gclass766_0.SubscribeEvent<int>(_voiceSelector.OnValueChanged, new Action<int>(selectVoiceEvent));

            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during initialization: {ex.Message}");
            }
        }
        public static async Task getAvailableCustomizations(ISession session)
        {
            try
            {
                string[] result = await session.GetAvailableAccountCustomization();
                _availableCustomizations = result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        public static void setupCustomizationDrawers(DropDownBox _headSelector, DropDownBox _voiceSelector)
        {
            try
            {

                GClass1444 instance = Singleton<GClass1444>.Instance;

                if (instance == null)
                {
                    Console.WriteLine("GClass1444 instance is null.");
                    return;
                }

                _headTemplates = new List<KeyValuePair<string, GClass2937>>();
                _voiceTemplates = new List<KeyValuePair<string, GClass2941>>();

                if (_availableCustomizations == null)
                {
                    Console.WriteLine("_availableCustomizations is null.");
                    return;
                }

                foreach (string text in _availableCustomizations)
                {
                    GClass2936 anyCustomizationItem = instance.GetAnyCustomizationItem(text);
                    if (anyCustomizationItem != null)
                    {
                        if (anyCustomizationItem.Side != null)
                        {
                            if (PatchConstants.BackEndSession.Profile != null)
                            {

                                if (anyCustomizationItem.Side.Contains(PatchConstants.BackEndSession.Profile.Side))
                                {
                                    GClass2937 gclass = anyCustomizationItem as GClass2937;
                                    GClass2941 gclass2 = anyCustomizationItem as GClass2941;
                                    if (gclass != null)
                                    {
                                        if (gclass.BodyPart == EBodyModelPart.Head)
                                        {
                                            _headTemplates.Add(new KeyValuePair<string, GClass2937>(text, gclass));
#if DEBUG
                                            Console.WriteLine($"Added head customization template: {text}");
#endif
                                        }
                                    }
                                    else if (gclass2 != null)
                                    {
                                        _voiceTemplates.Add(new KeyValuePair<string, GClass2941>(text, gclass2));
#if DEBUG
                                        Console.WriteLine($"Added voice customization template: {text}");
#endif
                                    }
                                }
                                else
                                {
#if DEBUG
                                    Console.WriteLine($"Player side {PatchConstants.BackEndSession.Profile.Side} is not contained in anyCustomizationItem.Side.");
#endif
                                }

                            }
                            else
                            {
                                Console.WriteLine("profile is null.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("anyCustomizationItem.Side is null.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("anyCustomizationItem is null.");
                    }
                }

#if DEBUG
                Console.WriteLine($"Added {_headTemplates.Count} head customization templates.");
                Console.WriteLine($"Added {_voiceTemplates.Count} voice customization templates.");

#endif
                _voices.Clear();

                if (_headSelector != null)
                {
                    setupHeadDropdownInfo(_headSelector);
                }
                else
                {
                    Console.WriteLine("Head dropdown is null.");
                }

                if (_voiceSelector != null)
                {
                    setupVoiceDropdownInfo(_voiceSelector);
                }
                else
                {
                    Console.WriteLine("Voice dropdown is null.");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during customization drawers setup: {ex.Message}");
            }
        }
        public static void setupHeadDropdownInfo(DropDownBox _headSelector)
        {
            try
            {

                string text = PatchConstants.BackEndSession.Profile.Customization[EBodyModelPart.Head];

                int num = 0;
                while (num < _headTemplates.Count && !(_headTemplates[num].Key == text))
                {
                    num++;
                }

                _selectedHeadIndex = num;

                _headSelector.Show(new Func<IEnumerable<string>>(initializeHeadDropdown), null);

                _headSelector.UpdateValue(_selectedHeadIndex, false, null, null);

                PatchConstants.BackEndSession.Profile.Customization[EBodyModelPart.Head] = _headTemplates[_selectedHeadIndex].Key;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during head dropdown info setup: {ex.Message}");
            }
        }
        public static void setupVoiceDropdownInfo(DropDownBox _voiceSelector)
        {
            try
            {
                string currentVoice = PatchConstants.BackEndSession.Profile.Info.Voice;

                // Find the index of the current voice in the _voiceTemplates collection
                int selectedIndex = _voiceTemplates.FindIndex(v => v.Value.Name == currentVoice);

                // If the current voice is not found, log an error and return
                if (selectedIndex == -1)
                {
                    Console.WriteLine($"Current voice '{currentVoice}' not found in the voice templates.");
                    return;
                }

                // Show the dropdown with the list of voices
                _voiceSelector.Show(new Func<IEnumerable<string>>(initializeVoiceDropdown), null);

                // Update the dropdown value with the index of the current voice
                _voiceSelector.UpdateValue(selectedIndex, false, null, null);

                // Update the profile's voice with the selected voice
                PatchConstants.BackEndSession.Profile.Info.Voice = _voiceTemplates[selectedIndex].Value.Name;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during voice dropdown info setup: {ex.Message}");
            }
        }

        public static IEnumerable<string> initializeHeadDropdown()
        {
            return _headTemplates.Select(new Func<KeyValuePair<string, GClass2937>, string>(getLocalizedHead)).ToArray<string>();
        }
        public static IEnumerable<string> initializeVoiceDropdown()
        {
            return _voiceTemplates.Select(new Func<KeyValuePair<string, GClass2941>, string>(getLocalizedVoice)).ToArray<string>();
        }
        public static string getLocalizedHead(KeyValuePair<string, GClass2937> x)
        {
#if DEBUG
            Console.WriteLine($"Localizing head: {x.Key}");
#endif
            return x.Value.NameLocalizationKey.Localized(null);
        }
        public static string getLocalizedVoice(KeyValuePair<string, GClass2941> x)
        {
#if DEBUG
            Console.WriteLine($"Localizing voice: {x.Key}");
#endif
            return x.Value.NameLocalizationKey.Localized(null);
        }
        public static void selectHeadEvent(int selectedIndex)
        {
            try
            {
#if DEBUG
                Console.WriteLine($"Selecting head event for index: {selectedIndex}");
#endif
                if (selectedIndex == _selectedHeadIndex)
                {
#if DEBUG
                    Console.WriteLine("Selected head index is already set.");
#endif
                    return;
                }

                _selectedHeadIndex = selectedIndex;
                string key = _headTemplates[_selectedHeadIndex].Key;
                PatchConstants.BackEndSession.Profile.Customization[EBodyModelPart.Head] = key;
#if DEBUG
                Console.WriteLine($"Head customization updated to: {key}");
#endif
                showPlayerPreview().HandleExceptions();

                WTTChangeHead(key);


            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during select head event: {ex.Message}");
            }
        }
        public static async Task showPlayerPreview()
        {
            try
            {

                GameObject overallPlayerModelView = GameObject.Find("Common UI/Common UI/InventoryScreen/Overall Panel/LeftSide/CharacterPanel");
                PlayerModelView playerModelViewScript = overallPlayerModelView.GetComponentInChildren<PlayerModelView>();
                await playerModelViewScript.Show(PatchConstants.BackEndSession.Profile, null, null, 0, null, true);

                changeSelectedHead(false, playerModelViewScript);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during player preview: {ex.Message}");
            }
        }
        public static void changeSelectedHead(bool active, PlayerModelView playerModelView)
        {
            try
            {

                slotViews = playerModelView.PlayerBody.SlotViews;
                foreach (GameObject gameObject in _hiddenSlots.Where(new Func<EquipmentSlot, bool>(getSlotType)).Select(new Func<EquipmentSlot, GameObject>(getSlotKey)).Where(new Func<GameObject, bool>(getModel)))
                {
                    gameObject.SetActive(active);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during change selected head: {ex.Message}");
            }
        }
        public static bool getSlotType(EquipmentSlot slotType)
        {
            return slotViews.ContainsKey(slotType);
        }
        public static GameObject getSlotKey(EquipmentSlot slotType)
        {
            return slotViews.GetByKey(slotType).ParentedModel.Value;
        }
        public static bool getModel(GameObject model)
        {
            return model != null;
        }
        public static void selectVoiceEvent(int selectedIndex)
        {
            try
            {
                _selectedVoiceIndex = selectedIndex;
                selectVoice(selectedIndex).HandleExceptions();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during select voice event: {ex.Message}");
            }
        }
        public static async Task selectVoice(int selectedIndex)
        {
            try
            {

                TagBank tagBank;
                if (!_voices.TryGetValue(selectedIndex, out tagBank))
                {
                    TagBank result = await Singleton<GClass799>.Instance.TakeVoice(_voiceTemplates[_selectedVoiceIndex].Value.Name, EPhraseTrigger.OnMutter);
                    _voices.Add(selectedIndex, result);
                    if (result == null)
                    {
                        Console.WriteLine($"Voice not available for index: {selectedIndex}");
                        return;
                    }
                }
                string key = _voiceTemplates[_selectedVoiceIndex].Value.Name;

                PatchConstants.BackEndSession.Profile.Info.Voice = key;


                int num = global::UnityEngine.Random.Range(0, _voices[selectedIndex].Clips.Length);
                TaggedClip taggedClip = _voices[selectedIndex].Clips[num];
                await Singleton<GUISounds>.Instance.ForcePlaySound(taggedClip.Clip);


                WTTChangeVoice(key);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during select voice: {ex.Message}");
            }
        }

    }
}

#endif