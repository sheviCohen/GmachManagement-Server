namespace Gmach.Core.Settings
{
    // כמה נקודות אמינות מקבל או מאבד לקוח בעת החזר (סעיף "Scoring" בקונפיגורציה)
    public class ScoringSettings
    {
        public int OnTimeReturnPoints { get; set; } = 10;
        public int LateReturnPenalty { get; set; } = 20;
    }
}
