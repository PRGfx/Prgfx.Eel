using System;
using System.Collections.Generic;
using Prgfx.Eel.Ast;

namespace Prgfx.Eel
{
    public class EelEvaluator
    {
        ExpressionNode expression;

        Context context;
        public EelEvaluator(ExpressionNode expression, Context context)
        {
            this.expression = expression;
            this.context = context;
        }

        public object Evaluate()
        {
            return EvaluateNode(expression, context);
        }

        private object EvaluateNode(ExpressionNode expression, Context context)
        {
            return EvaluateNode(expression.exp, context);
        }

        private object EvaluateNode(ConditionalExpressionNode expression, Context context)
        {
            var condition = EvaluateNode(expression.cond, context);
            if (expression.no != null && expression.yes != null)
            {
                if ((bool)condition)
                {
                    return EvaluateNode(expression.yes, context);
                }
                else
                {
                    return EvaluateNode(expression.no, context);
                }
            }
            else
            {
                return condition;
            }
        }

        private object EvaluateNode(DisjunctionNode expression, Context context)
        {
            var value = EvaluateNode(expression.lft, context);
            if (expression.rgt == null || expression.rgt.Length == 0)
            {
                return value;
            }
            foreach (var rgt in expression.rgt)
            {
                if (value != null)
                {
                    return value;
                }
                value = EvaluateNode(rgt, context);
            }
            return value;
        }

        private object EvaluateNode(ConjunctionNode expression, Context context)
        {
            var value = EvaluateNode(expression.lft, context);
            if (expression.rgt == null || expression.rgt.Length == 0)
            {
                return value;
            }
            foreach (var rgt in expression.rgt)
            {
                if ((bool)value == false)
                {
                    return null;
                }
                value = EvaluateNode(rgt, context);
            }
            if ((bool)value == false)
            {
                return null;
            }
            return value;
        }

        private object EvaluateNode(ComparisonNode expression, Context context)
        {
            var lft = EvaluateNode(expression.lft, context);
            if (expression.rgt == null)
            {
                return lft;
            }
            var rgt = EvaluateNode(expression.rgt, context);
            switch (expression.comp)
            {
                case "==":
                    return lft == rgt;
                case "!=":
                    return lft != rgt;
            }
            if (!(lft is IComparable) || !(rgt is IComparable))
            {
                return false;
            }
            try
            {
                var lftC = (IComparable)lft;
                var rgtC = (IComparable)rgt;
                switch (expression.comp)
                {
                    case ">=":
                        return lftC.CompareTo(rgtC) >= 0;
                    case ">":
                        return lftC.CompareTo(rgtC) > 0;
                    case "<=":
                        return lftC.CompareTo(rgtC) <= 0;
                    case "<":
                        return lftC.CompareTo(rgtC) < 0;
                }
            }
            catch (System.Exception)
            {
            }
            return false;
        }

        private object EvaluateNode(SumCalculationNode expression, Context context)
        {
            var lft = EvaluateNode(expression.lft, context);
            var value = lft;
            if (expression.args == null || expression.args.Length == 0) {
                return value;
            }
            float floatVal = 0;
            var isConcatenation = (value is string) || !float.TryParse(value.ToString(), out floatVal);
            foreach (var term in expression.args) {
                float floatRgt = 0;
                var rgt = EvaluateNode(term.rgt, context).ToString();
                if (!isConcatenation) {
                    isConcatenation = !float.TryParse(rgt, out floatRgt);
                }
                switch (term.op) {
                    case "+":
                        if (isConcatenation) {
                            value = (string)value + rgt.ToString();
                            isConcatenation = true;
                        } else {
                            floatVal += floatRgt;
                        }
                        break;
                    case "-":
                        if (isConcatenation) {
                            value = string.Empty;
                        } else {
                            floatVal -= floatRgt;
                        }
                        break;
                }
            }
            return isConcatenation ? value : floatVal;
        }

        private object EvaluateNode(ProdCalculationNode expression, Context context)
        {
            var lft = EvaluateNode(expression.lft, context);
            var value = lft;
            if (expression.args == null || expression.args.Length == 0) {
                return value;
            }
            var floatVal = float.Parse(value.ToString());
            foreach (var term in expression.args) {
                var rgt = float.Parse(EvaluateNode(term.rgt, context).ToString());
                switch (term.op) {
                    case "*":
                        floatVal *= rgt;
                        break;
                    case "/":
                        floatVal /= rgt;
                        break;
                    case "%":
                        floatVal = floatVal % rgt;
                        break;
                }
            }
            return floatVal;
        }

        private object EvaluateNode(TermNode expression, Context context)
        {
            if (expression is BooleanLiteralNode) {
                return EvaluateNode((BooleanLiteralNode)expression, context);
            }
            if (expression is NumberLiteralNode) {
                return EvaluateNode((NumberLiteralNode)expression, context);
            }
            if (expression is StringLiteralNode) {
                return EvaluateNode((StringLiteralNode)expression, context);
            }
            if (expression is ObjectPathNode) {
                return EvaluateNode((ObjectPathNode)expression, context);
            }
            return null;
        }

        private object EvaluateNode(SimpleExpressionNode expression, Context context)
        {
            if (expression is WrappedExpressionNode) {
                return EvaluateNode((WrappedExpressionNode)expression, context);
            }
            if (expression is NotExpressionNode) {
                return EvaluateNode((NotExpressionNode)expression, context);
            }
            if (expression is ArrayLiteralNode) {
                return EvaluateNode((ArrayLiteralNode)expression, context);
            }
            if (expression is ObjectLiteralNode) {
                return EvaluateNode((ObjectLiteralNode)expression, context);
            }
            return EvaluateNode((TermNode)expression, context);
        }

        private object EvaluateNode(ObjectPathNode expression, Context context)
        {
            var originalContext = context;
            var tmp = context;
            foreach (var path in expression.path) {
                if (path is StaticObjectPathPartNode) {
                    tmp = (Context)EvaluateNode((StaticObjectPathPartNode)path, context);
                }
            }
            return tmp.Unwrap();
        }

        private object EvaluateNode(StaticObjectPathPartNode expression, Context context)
        {
            return context.GetAndWrap(expression.name);
        }

        private void EvaluateNode(ObjectPathPartNode path, Context context)
        {
            throw new NotImplementedException();
        }

        private object EvaluateNode(WrappedExpressionNode expression, Context context)
        {
            return EvaluateNode(expression.exp, context);
        }

        private object EvaluateNode(NotExpressionNode expression, Context context)
        {
            return !((bool)EvaluateNode(expression, context));
        }

        private object EvaluateNode(ArrayLiteralNode expression, Context context)
        {
            var result = new List<object>();
            foreach (var exp in expression.values) {
                result.Add(EvaluateNode(exp, context));
            }
            return result;
        }

        private object EvaluateNode(ObjectLiteralNode expression, Context context)
        {
            var result = new Dictionary<string, object>();
            foreach (var kv in expression.properties) {
                result.Add(kv.key, EvaluateNode(kv.value, context));
            }
            return result;
        }

        private object EvaluateNode(ArrowFunctionNode expression, Context context)
        {
            throw new NotImplementedException();
        }

        private object EvaluateNode(BooleanLiteralNode expression, Context context)
        {
            return expression.value;
        }

        private object EvaluateNode(StringLiteralNode expression, Context context)
        {
            return expression.value;
        }

        private object EvaluateNode(NumberLiteralNode expression, Context context)
        {
            return expression.value;
        }
    }

    public class ParsingEelEvaluator
    {
        public ParsingEelEvaluator()
        {

        }

        public object Evaluate(string code, Context context)
        {
            var parser = new Parser();
            var ast = parser.Parse(code);
            var evaluator = new EelEvaluator(ast, context);
            return evaluator.Evaluate();
        }
    }
}