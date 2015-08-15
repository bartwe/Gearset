using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Gearset.Components.CSExpressionParser {
    sealed class CsExpression<T> {
        readonly ListDictionary _parameters;

        /// <summary>
        /// Delegate to evaluate expression without using MethodInfo.Invoke.
        /// </summary>
        ExpressionDelegate _expressionMethod;

        String _expression = String.Empty;

        /// <summary>
        /// Construct a new CSExpression that returns default(T).
        /// Add parameters the this expression before changing the
        /// Expression, otherwise it will be invalid.
        /// </summary>
        public CsExpression() {
            _parameters = new ListDictionary();
        }

        /// <summary>
        /// Evaluates the expression and return its value.
        /// </summary>
        public T Value { get { return Evaluate(); } }

        /// <summary>
        /// A c# expression that can be evaluated and return a value of type T.
        /// </summary>
        public String Expression { get { return _expression; } set { OnExpressionChanged(value); } }

        void OnExpressionChanged(String expression) {
            MethodInfo method = null;
            try {
                var unfriendlyExpression = expression;
                foreach (var key in _parameters.Keys) {
                    var name = key as String;
                    if (name != null) {
                        // Regular expression that matches the name of the parameter and won't
                        // match if that parameters is a substring of something else. It will match
                        // the preceeding and succeeding character as well.
                        var regexp = String.Format(@"(?<![\.a-zA-Z0-9_]){0}(?![a-zA-Z0-9_])", name);

                        // Replace string that puts the preceeding and suceeding characters matched
                        // and changes the name of the variable to a reference in the paramter dict.
                        var replaceWith = String.Format("(({0})((IDictionary)p[0])[\"{1}\"])", _parameters[key].GetType().FullName, key);

                        // Replace the parameter.
                        unfriendlyExpression = Regex.Replace(unfriendlyExpression, regexp, replaceWith);
                    }
                }
                method = ReflectionHelper.CompileCSharpMethod(unfriendlyExpression, typeof(T));
            }
            catch {
                GearsetResources.Console.Log("Gearset", "Error while compiling CSExpression.");
                GearsetResources.Console.Log("Gearset", "Invalid Expression: {0}", expression);
            }

            if (method != null) {
                _expressionMethod = (ExpressionDelegate)Delegate.CreateDelegate(typeof(ExpressionDelegate), method);
                _expression = expression;
            }
        }

        /// <summary>
        /// Evaluates the expression if any, else return de default <c>T</c>.
        /// </summary>
        /// <returns></returns>
        T Evaluate() {
            if (_expressionMethod != null)
                return _expressionMethod(new Object[] { _parameters });
            return default(T);
        }

        /// <summary>
        /// Sets a parameter that will be used in the expression.
        /// </summary>
        public void SetParameter(String name, Object value) {
            if (name.Contains(" ")) {
                throw new InvalidOperationException("The name of the parameter cannot contain whitespaces");
            }
            if (value == null) {
                throw new ArgumentNullException("value", "The value of the parameter cannot be null");
            }
            if (!_parameters.Contains(name)) {
                _parameters.Add(name, value);
            }
            else {
                _parameters[name] = value;
            }
        }

        delegate T ExpressionDelegate(Object[] parameters);
    }
}
