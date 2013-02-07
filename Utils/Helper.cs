using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ArcReaction
{
    public class ModelError : IEnumerable<ValidationError>
    {
        IEnumerable<ValidationError> errors;

        private ModelError(IEnumerable<ValidationError> errors)
        {
            this.errors = errors;
        }

        public static implicit operator ModelError(ModelState model)
        {
            if (model)
                return new ModelError(model);
            else
                return null;
        }

        public IEnumerator<ValidationError> GetEnumerator()
        {
            return errors.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public abstract class ValidationError
    {

    }

    public sealed class ModelState : IEnumerable<ValidationError>
    {
        bool is_valid = true;

        
        [ThreadStatic]
        static Stack<ModelError> errors;

        static ModelState()
        {
            errors = new Stack<ModelError>();
        }

        static Stack<ModelError> EnsureModelErrorObject()
        {
            return (errors = errors ?? new Stack<ModelError>());
        }

        public static void AddError(ModelError error)
        {
            if (error != null)
            {
                EnsureModelErrorObject().Push(error);
            }
        }

        public static void ClearErrors()
        {
            EnsureModelErrorObject().Clear();
        }

        public static ModelError GetLastError()
        {
            if (EnsureModelErrorObject().Any())
                return errors.Pop();
            else
                return null;
        }

        List<ValidationError> validation_errors = new List<ValidationError>();

        public void AddError(ValidationError error)
        {
            is_valid = false;
            validation_errors.Add(error);
        }

        public static bool operator true(ModelState model)
        {
            return !object.ReferenceEquals(null, model) && model.is_valid;
        }

        public static bool operator false(ModelState model)
        {
            return object.ReferenceEquals(null, model) || !model.is_valid;
        }

        public static implicit operator bool(ModelState model)
        {
            return !object.ReferenceEquals(null, model) && model.is_valid;
        }

        public IEnumerator<ValidationError> GetEnumerator()
        {
            return validation_errors.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public abstract class HttpFieldValidationError : ValidationError
    {
        readonly string key;

        public HttpFieldValidationError(string key, string message)
        {
            this.key = key;
        }
    }

    public static class Utils
    {        

        sealed class NullValueError : HttpFieldValidationError
        {
            public NullValueError(string key) : base(key, "FIELD CANNOT BE NULL") { }
        }

        sealed class InvalidIntegerFormatError : HttpFieldValidationError
        {
            public InvalidIntegerFormatError(string key) : base(key, "INVALID STRING FORMAT FOR INTEGER") { }
        }

        sealed class InvalidBooleanFormatError : HttpFieldValidationError
        {
            public InvalidBooleanFormatError(string key) : base(key, "INVALID STRING FORMAT FOR BOOLEAN") { }
        }

        sealed class InvalidDateTimeFormatError : HttpFieldValidationError
        {
            public InvalidDateTimeFormatError(string key) : base(key, "INVALID STRING FORMAT FOR DATETIME") { }
        }
        
        public static string GetNonEmptyString(this HttpContextEx context, string key, ref ModelState state)
        {
            var s = context.Request.Form[key];

            if (s == null || s.Length == 0)
                state.AddError(new NullValueError(key));
                
            return s;
        }

        public static int GetInt32(this HttpContextEx context, string key, ref ModelState state)
        {
            int n = 0;

            string s = null;

            if (!int.TryParse(s = context.Request.Form[key], out n))
                state.AddError(s == null ? (HttpFieldValidationError) new NullValueError(key) : new InvalidIntegerFormatError(key));

            return n;
        }

        public static int? MaybeGetInt32(this HttpContextEx context, string key)
        {
            int n;

            if (int.TryParse(key, out n))
                return n;
            else
                return null;
            
        }

        public static DateTime GetDateTime(this HttpContextEx context, string key, ref ModelState state)
        {
            DateTime n;
            string s = null;

            if (!DateTime.TryParse(s = context.Request.Form[key], out n))
                state.AddError(s == null ? (HttpFieldValidationError)new NullValueError(key) : new InvalidDateTimeFormatError(key));

            return n;
        }

        public static DateTime? MaybeGetDateTime(this HttpContextEx context, string key)
        {
            DateTime n;

            if (DateTime.TryParse(context.Request.Form[key], out n))
                return n;
            else
                return null;
        }

        public static bool GetBoolean(this HttpContextEx context, string key, ref ModelState state)
        {
            bool b = default(bool);

            string s = null;

            if (!bool.TryParse(s = context.Request.Form[key], out b))
                state.AddError(s == null ? (HttpFieldValidationError) new NullValueError(key) : new InvalidBooleanFormatError(key));            

            return b;
        }

        public static string GetString(this HttpContextEx context, string key, ref ModelState state)
        {
            var s = context.Request.Form[key];

            if (s == null)
                state.AddError(new NullValueError(key));

            return s;
        }

        public static string MaybeGetString(this HttpContextEx context, string key)
        {
            var s = context.Request.Form[key];

            return s;
        }


        public struct ParseAttemptResult<T>
        {
            bool success;
            public T value;

            public ParseAttemptResult(T obj, bool success)
            {
                this.value = obj;
                this.success = success;
            }

            public static bool operator true(ParseAttemptResult<T> x)
            {
                return x.success;
            }

            public static bool operator false(ParseAttemptResult<T> x)
            {
                return !x.success;
            }
        }

        public static IEnumerable<T> Get<T>(this HttpContextEx context, string key, Func<string, ParseAttemptResult<T>> typeMap = null)
        {
            if (typeMap != null)
            {
                var ys = new List<T>();
                var val = context.Request.Form[key];

                if (val == null)
                    return ys;
                
                foreach (var x in val.Split(','))
                {
                    var result = typeMap(x);

                    if (result)
                        ys.Add(result.value);
                }

                return ys;
            }
            else
            {
                if (typeof(T) == typeof(string))
                {
                    return context.Request.Form.GetValues(key) as IEnumerable<T>;
                }
                
                if (typeof(T) == typeof(int))
                {
                    return (IEnumerable<T>)context.Get(key, s =>
                    {
                        int n;
                        var b = int.TryParse(s, out n);

                        return new ParseAttemptResult<int>(n, b);

                    });
                }

                if (typeof(T) == typeof(bool))
                {
                    return (IEnumerable<T>)context.Get(key, s =>
                    {
                        bool n;
                        var b = bool.TryParse(s, out n);

                        return new ParseAttemptResult<bool>(n, b);

                    });
                }

                if (typeof(T) == typeof(byte))
                {
                    return (IEnumerable<T>)context.Get(key, s =>
                    {
                        byte n;
                        var b = byte.TryParse(s, out n);

                        return new ParseAttemptResult<byte>(n, b);

                    });
                }

                if (typeof(T) == typeof(DateTime))
                {
                    return (IEnumerable<T>)context.Get(key, s =>
                    {
                        DateTime n;
                        var b = DateTime.TryParse(s, out n);

                        return new ParseAttemptResult<DateTime>(n, b);

                    });
                }

                if (typeof(T) == typeof(Guid))
                {
                    return (IEnumerable<T>)context.Get(key, s =>
                    {
                        Guid n;
                        var b = Guid.TryParse(s, out n);

                        return new ParseAttemptResult<Guid>(n, b);

                    });
                }

                throw new ArgumentException("Unable to convert string to " + typeof(T).FullName);
            }
        }
        
        public static string ToBase64String(this string s)
        {
            var b64 = Convert.ToBase64String(Encoding.UTF32.GetBytes(s)).ToCharArray();

            for (var i = 0; i < b64.Length; i++)
            {
                var c = b64[i];

                if (c == '=')
                    b64[i] = '-';
            }

            return new string(b64);
        }
    }
    
}
