using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;


namespace Stardew_100_Percent_Mod
{
    public interface IModConfigMenu
    {
        void RegisterModConfig(IManifest mod, System.Action revertToDefault, System.Action saveToFile);
        void UnregisterModConfig(IManifest mod);
        void SetDefaultIngameOptinValue(IManifest mod, bool optedIn);
        void StartNewPage(IManifest mod, string pageName);
        void OverridePageDisplayName(IManifest mod, string pageName, string displayName);
        void RegisterLabel(IManifest mod, string labelName, string labelDesc);
        void RegisterPageLabel(IManifest mod, string labelName, string labelDesc, string newPage);
        void RegisterParagraph(IManifest mod, string paragraph);
        void RegisterImage(IManifest mod, string texPath, Rectangle? texRect = null, int scale = 4);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc,
                                  Func<bool> optionGet, System.Action<bool> optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc,
                                  Func<int> optionGet, System.Action<int> optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc,
                                  Func<float> optionGet, System.Action<float> optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc,
                                  Func<string> optionGet, System.Action<string> optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc,
                                  Func<SButton> optionGet, System.Action<SButton> optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc,
                                  Func<KeybindList> optionGet, System.Action<KeybindList> optionSet);
        void RegisterClampedOption(IManifest mod, string optionName, string optionDesc,
                                   Func<int> optionGet, System.Action<int> optionSet,
                                   int min, int max);
        void RegisterClampedOption(IManifest mod, string optionName, string optionDesc,
                                   Func<float> optionGet, System.Action<float> optionSet,
                                   float min, float max);
        void RegisterClampedOption(IManifest mod, string optionName, string optionDesc,
                                   Func<int> optionGet, System.Action<int> optionSet,
                                   int min, int max, int interval);
        void RegisterClampedOption(IManifest mod, string optionName, string optionDesc,
                                   Func<float> optionGet, System.Action<float> optionSet,
                                   float min, float max, float interval);
        void RegisterChoiceOption(IManifest mod, string optionName, string optionDesc,
                                  Func<string> optionGet, System.Action<string> optionSet,
                                  string[] choices);
        void RegisterComplexOption(IManifest mod, string optionName, string optionDesc,
                                   Func<Vector2, object, object> widgetUpdate,
                                   Func<SpriteBatch, Vector2, object, object> widgetDraw,
                                   System.Action<object> onSave);
        void SubscribeToChange(IManifest mod, System.Action<string, bool> changeHandler);
        void SubscribeToChange(IManifest mod, System.Action<string, int> changeHandler);
        void SubscribeToChange(IManifest mod, System.Action<string, float> changeHandler);
        void SubscribeToChange(IManifest mod, System.Action<string, string> changeHandler);
        void OpenModMenu(IManifest mod);
    }
}
