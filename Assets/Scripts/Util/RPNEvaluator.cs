using System;
using System.Collections.Generic;
using System.Globalization;

namespace RPNEvaluator
{
    public class RPNEvaluator
    {
        public static int Evaluate(string expression, Dictionary<string, int> variables)
        {
            return EvaluateInt(expression, variables);
        }

        public static float Evaluatef(string expression, Dictionary<string, float> variables)
        {
            return EvaluateFloat(expression, variables);
        }

        public static float Evaluatef(string expression, Dictionary<string, int> variables)
        {
            return EvaluateFloat(expression, variables);
        }

        // This overload allows using integer variables in integer evaluation
        private static int EvaluateInt(string expression, Dictionary<string, int>? variables)
        {
            string[] tokens = Tokenize(expression);
            Stack<int> stack = new Stack<int>();

            foreach (string token in tokens)
            {
                switch (token)
                {
                    case "+":
                    case "-":
                    case "*":
                    case "/":
                    case "%":
                        ApplyIntOperator(stack, token);
                        break;
                    default:
                        stack.Push(ResolveIntValue(token, variables));
                        break;
                }
            }

            return GetFinalResult(stack);
        }

        // This overload allows using float variables in integer evaluation
        private static float EvaluateFloat(string expression, Dictionary<string, float>? variables)
        {
            string[] tokens = Tokenize(expression);
            Stack<float> stack = new Stack<float>();

            foreach (string token in tokens)
            {
                switch (token)
                {
                    case "+":
                    case "-":
                    case "*":
                    case "/":
                        ApplyFloatOperator(stack, token);
                        break;
                    case "%":
                        throw new InvalidOperationException("Modulus is only supported for integer evaluation.");
                    default:
                        stack.Push(ResolveFloatValue(token, variables));
                        break;
                }
            }

            return GetFinalResult(stack);
        }

        // This overload allows using integer variables in float evaluation
        // Integer variables will be promoted to float values
        private static float EvaluateFloat(string expression, Dictionary<string, int>? variables)
        {
            string[] tokens = Tokenize(expression);
            Stack<float> stack = new Stack<float>();

            foreach (string token in tokens)
            {
                switch (token)
                {
                    case "+":
                    case "-":
                    case "*":
                    case "/":
                        ApplyFloatOperator(stack, token);
                        break;
                    case "%":
                        throw new InvalidOperationException("Modulus is only supported for integer evaluation.");
                    default:
                        stack.Push(ResolveFloatValue(token, variables));
                        break;
                }
            }

            return GetFinalResult(stack);
        }

        // This method splits the input expression into tokens based on whitespace
        private static string[] Tokenize(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                throw new ArgumentException("Expression cannot be empty.", nameof(expression));
            }

            return expression.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }


        // This method resolves a token to an integer value, checking both integer variables and integer literals
        private static int ResolveIntValue(string token, Dictionary<string, int>? variables)
        {
            if (variables != null && variables.TryGetValue(token, out int variableValue))
            {
                return variableValue;
            }

            if (int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out int literalValue))
            {
                return literalValue;
            }

            throw new KeyNotFoundException($"Token '{token}' is not a valid integer literal or variable.");
        }

        // This method resolves a token to a float value, checking both float and integer variables
        private static float ResolveFloatValue(string token, Dictionary<string, float>? variables)
        {
            if (variables != null && variables.TryGetValue(token, out float variableValue))
            {
                return variableValue;
            }

            if (float.TryParse(token, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out float literalValue))
            {
                return literalValue;
            }

            throw new KeyNotFoundException($"Token '{token}' is not a valid float literal or variable.");
        }

        // This method resolves a token to a float value, checking integer variables and promoting them to float if found
        private static float ResolveFloatValue(string token, Dictionary<string, int>? variables)
        {
            if (variables != null && variables.TryGetValue(token, out int variableValue))
            {
                return variableValue;
            }

            if (float.TryParse(token, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out float literalValue))
            {
                return literalValue;
            }

            throw new KeyNotFoundException($"Token '{token}' is not a valid float literal or variable.");
        }

        // This method resolves a token to a float value, checking both float and integer variables
        private static void ApplyIntOperator(Stack<int> stack, string token)
        {
            EnsureEnoughOperands(stack, token);

            int right = stack.Pop();
            int left = stack.Pop();

            switch (token)
            {
                case "+":
                    stack.Push(left + right);
                    break;
                case "-":
                    stack.Push(left - right);
                    break;
                case "*":
                    stack.Push(left * right);
                    break;
                case "/":
                    stack.Push(left / right);
                    break;
                case "%":
                    stack.Push(left % right);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported operator '{token}'.");
            }
        }

        // This method resolves a token to a float value, checking both float and integer variables
        private static void ApplyFloatOperator(Stack<float> stack, string token)
        {
            EnsureEnoughOperands(stack, token);

            float right = stack.Pop();
            float left = stack.Pop();

            switch (token)
            {
                case "+":
                    stack.Push(left + right);
                    break;
                case "-":
                    stack.Push(left - right);
                    break;
                case "*":
                    stack.Push(left * right);
                    break;
                case "/":
                    stack.Push(left / right);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported operator '{token}'.");
            }
        }

        // This method checks if there are enough operands on the stack for a binary operator
        private static void EnsureEnoughOperands<T>(Stack<T> stack, string token)
        {
            if (stack.Count < 2)
            {
                throw new InvalidOperationException($"Operator '{token}' requires two operands.");
            }
        }


        // This method checks if there is exactly one result left on the stack after evaluation
        private static T GetFinalResult<T>(Stack<T> stack)
        {
            if (stack.Count != 1)
            {
                throw new InvalidOperationException("The expression is malformed.");
            }

            return stack.Pop();
        }
    }
}
