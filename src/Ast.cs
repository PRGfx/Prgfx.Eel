namespace Prgfx.Eel.Ast
{
    public class ExpressionNode : SimpleExpressionNode
    {
        public ConditionalExpressionNode exp;
    }

    public class ConditionalExpressionNode
    {
        public DisjunctionNode cond;
        public ExpressionNode yes;
        public ExpressionNode no;
    }

    public class DisjunctionNode
    {
        public ConjunctionNode lft;
        public ConjunctionNode[] rgt;
    }

    public class ConjunctionNode
    {
        public ComparisonNode lft;
        public ComparisonNode[] rgt;
    }

    public class ComparisonNode
    {
        public SumCalculationNode lft;
        public string comp;
        public SumCalculationNode rgt;
    }

    public class NodeWithArgsArg<T>
    {
        public string op;
        public T rgt;
    }

    public abstract class NodeWithArgs<T>
    {
        public T lft;
        public NodeWithArgsArg<T>[] args;
    }

    public class SumCalculationNode : NodeWithArgs<ProdCalculationNode>
    {
    }

    public class ProdCalculationNode : NodeWithArgs<SimpleExpressionNode>
    {
    }

    public abstract class SimpleExpressionNode
    {
    }

    public class ArrowFunctionNode : SimpleExpressionNode
    {
        public MethodArgumentsNode arguments;
        public ExpressionNode exp;
    }

    public class NotExpressionNode : SimpleExpressionNode
    {
    }

    public class WrappedExpressionNode : ExpressionNode
    {
    }

    public class ArrayLiteralNode : SimpleExpressionNode
    {
        public ExpressionNode[] values;
    }

    public class ObjectLiteralNode : SimpleExpressionNode
    {
        public ObjectLiteralPropertyNode[] properties;
    }

    public abstract class TermNode : SimpleExpressionNode
    {
    }

    public class BooleanLiteralNode : TermNode
    {
        public bool value;
    }

    public class NumberLiteralNode : TermNode
    {
        public float value;
    }

    public class StringLiteralNode : TermNode
    {
        public string value;
    }

    public class ObjectPathNode : TermNode
    {
        public ObjectPathPartNode[] path;
    }

    public abstract class ObjectPathPartNode
    {
    }

    public class StaticObjectPathPartNode : ObjectPathPartNode
    {
        public string name;
    }

    public class OffsetAccessNode : ObjectPathPartNode
    {
        public ExpressionNode exp;
    }

    public class MethodCallNode : ObjectPathPartNode
    {
        public string name;
        public ExpressionNode[] arguments;
    }

    public class MethodArgumentsNode
    {
        public string[] arguments;
    }

    public class ObjectLiteralPropertyNode
    {
        public string key;
        public ExpressionNode value;
    }
}