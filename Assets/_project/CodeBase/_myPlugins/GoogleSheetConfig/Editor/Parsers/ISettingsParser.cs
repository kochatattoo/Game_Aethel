using Newtonsoft.Json.Linq;

namespace GoogleSheetConfig
{
    public interface ISettingsParser
    {
        string SheetName { get; }

        void Parse(JArray jArray);
    }
}