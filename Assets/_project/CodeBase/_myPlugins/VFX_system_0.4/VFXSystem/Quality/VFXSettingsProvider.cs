using UnityEngine;

namespace VFXSystem.Quality
{
    public class VFXSettingsProvider : IVFXSettingsProvider
    {
        public VFXQuality CurrentQuality => GetCurrentQuality();

        private VFXQuality GetCurrentQuality()
        {
            int qualityLevel = QualitySettings.GetQualityLevel();

            return qualityLevel switch
            {
                0 => VFXQuality.Ultra,    // Самый высокий уровень (High Fidelity)
                1 => VFXQuality.Medium,   // Средний уровень (Balanced)
                2 => VFXQuality.Low,      // Самый низкий уровень (Performant)
                _ => VFXQuality.Low       // Заглушка для любых других уровней
            };
        }
    }
}


