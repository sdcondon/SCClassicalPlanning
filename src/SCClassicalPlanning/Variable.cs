namespace SCClassicalPlanning
{
    public sealed class Variable
    {
        public Variable(object identifier) => Identifier = identifier;

        public object Identifier { get; }
    }
}
