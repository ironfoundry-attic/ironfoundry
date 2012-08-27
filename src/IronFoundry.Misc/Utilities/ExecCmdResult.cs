namespace IronFoundry.Misc.Utilities
{
    using System;

    public class ExecCmdResult
    {
        private readonly bool success = false;
        private readonly string output = null;

        public ExecCmdResult(bool success, string output)
        {
            this.success = success;
            this.output = output;
        }

        public bool Success { get { return success; } }
        public string Output { get { return output; } }

        public override string ToString()
        {
            return String.Format("success: {0} output: {1}", success, output);
        }
    }
}