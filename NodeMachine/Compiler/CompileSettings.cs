namespace NodeMachine.Compiler
{
    public class CompileSettings
    {
        public bool CompileBuildings { get; private set; }

        public CompileSettings(bool compileBuildings = true)
        {
            CompileBuildings = compileBuildings;
        }

        public static CompileSettings Default()
        {
            return new CompileSettings();
        }
    }
}
