using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Prgfx.Eel
{
    public class Context
    {
        protected object value;

        public Context(object value)
        {
            this.value = value;
        }

        public object Unwrap()
        {
            return UnwrapValue(value);
        }

        public object UnwrapValue(System.Collections.IEnumerable value)
        {
            var result = new List<object>();
            foreach (var item in value)
            {
                if (item is Context)
                {
                    result.Add(((Context)item).Unwrap());
                }
                else
                {
                    result.Add(UnwrapValue(item));
                }
            }
            return result;
        }

        public object UnwrapValue(object value)
        {
            return value;
        }

        public Context Push(object value, string key = null)
        {
            if (key == null)
            {
                if (!(value is List<object>))
                {
                    throw new EvaluationException("List operation on non-list context");
                }
                ((List<object>)value).Add(value);
            }
            else
            {
                if (!(value is Dictionary<string, object>))
                {
                    throw new EvaluationException("Dictionary operation on non-dictionary context");
                }
              ((Dictionary<string, object>)value).Add(key, value);
            }
            return this;
        }

        public Context Wrap(object value)
        {
            if (value is Context)
            {
                return (Context)value;
            }
            return new Context(value);
        }

        public object CallAndWrap(string method, object[] arguments = null)
        {
            return Wrap(Call(method, arguments));
        }

        public object Call(string method, object[] arguments = null)
        {
            object callback = null;
            object callee = null;
            if (value == null)
            {
                return null;
            }
            else if (value is Dictionary<string, object>)
            {
                var dict = (Dictionary<string, object>)value;
                if (!dict.ContainsKey(method))
                {
                    throw new EvaluationException($"Array has no function \"{method}\"");
                }
                callback = dict[method];
            }
            else
            {
                callee = value;
                callback = value.GetType().GetMethod(method);
            }
            if (!(callback is MethodInfo))
            {
                throw new EvaluationException($"Method \"{method}\" to call is no a callable method");
            }
            var callbackMethod = (MethodInfo)callback;
            if (callee == null && !callbackMethod.IsStatic)
            {
                try
                {
                    callee = Activator.CreateInstance(callbackMethod.DeclaringType);
                }
                catch (System.Exception e)
                {
                    throw new EvaluationException($"Could not create object to call \"{method}\" on");
                }
            }
            object[] args;
            if (arguments == null)
            {
                args = new object[] { };
            }
            else
            {
                args = arguments.Select(x => (x is Context) ? ((Context)x).Unwrap() : x).ToArray();
            }
            if (callbackMethod.IsStatic)
            {
                return callbackMethod.Invoke(null, args);
            }
            else
            {
                if (callee == null)
                {
                    throw new EvaluationException($"Method \"{method}\" cannot be called without reference object");
                }
            }
            return callbackMethod.Invoke(callee, args);
        }

        public Context GetAndWrap(string path = null)
        {
            return Wrap(Get(path));
        }

        public object Get(object path)
        {
            if (path is Context)
            {
                path = ((Context)path).Unwrap();
            }
            if (!(path is string) && !(path is int))
            {
                throw new EvaluationException("Path is not of type string or integer");
            }
            try
            {
                return Util.ObjectProperty(value, path.ToString());
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        /* public object Get(Context path)
        {
            var tmp = path.Unwrap();
            if (tmp == null) {
                return null;
            }
            if (tmp is string) {
                return Get((string)tmp);
            }
            if (tmp is int) {
                return Get((int)tmp);
            }
            return null;
        }

        public object Get(int path)
        {
            if (value is List<object>)
            {
                var list = (List<object>)value;
                if (list.Count > path) {
                    return list[path];
                }
                return null;
            }
            return null;
        }

        public object Get(string path)
        {
            if (path == null) {
                return null;
            }
            if (value is Dictionary<string, object>)
            {
                var dict = (Dictionary<string, object>)value;
                if (dict.ContainsKey(path)) {
                    return dict[path];
                }
                return null;
            }
            return null;
        } */
    }
}