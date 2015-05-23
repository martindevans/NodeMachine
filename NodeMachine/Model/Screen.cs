namespace NodeMachine.Model
{
    public class Screen
    {
        public string id { get; set; }

        public string TransitionState { get; set; }

        public string Name { get; set; }

        public int Index { get; set; }
    }

    public enum TransitionState
    {
        Shown,
        Hidden,
        On,
        Off
    }
}
