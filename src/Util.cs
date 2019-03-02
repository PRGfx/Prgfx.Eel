using System;
using System.Reflection;

namespace Prgfx.Eel
{
    public class Util
    {
        public static object ObjectPropertyByPath(object subject, string path)
        {
            var pathSegments = path.Split('.');
            foreach (var pathSegment in pathSegments)
            {
                var propertyExists = false;
                var propertyValue = GetPropertyInternal(subject, pathSegment, false, out propertyExists);

            }
            return subject;
        }

        private static object GetPropertyInternal(object subject, string pathSegment, bool forceDirectAccess, out bool propertyExists)
        {
            propertyExists = false;
            if (subject == null) {
                return null;
            }
            if (subject is System.Collections.IDictionary)
            {
                propertyExists = ((System.Collections.IDictionary)subject).Contains(pathSegment);
                if (propertyExists) {
                    return ((System.Collections.IDictionary)subject)[pathSegment];
                }
                return null;
            }
            if (int.TryParse(pathSegment, out int pathKey) && (subject is System.Collections.ICollection)) {
                propertyExists = ((System.Collections.ICollection)subject).Count > pathKey;
                if (propertyExists) {
                    var i = 0;
                    foreach (var item in (System.Collections.ICollection)subject) {
                        if (i++ == pathKey) {
                            return item;
                        }
                    }
                }
            }
            try {
                var accessFlags = BindingFlags.Public | BindingFlags.Instance;
                if (forceDirectAccess) {
                    accessFlags |= BindingFlags.NonPublic;
                }
                var getter = subject.GetType().GetMethod("Get" + pathSegment[0].ToString().ToUpper() + pathSegment.Substring(1), accessFlags);
                if (getter != null) {
                    propertyExists = true;
                    return getter.Invoke(subject, null);
                }
                var field = subject.GetType().GetField(pathSegment, accessFlags);
                if (field != null) {
                    propertyExists = true;
                    return field.GetValue(subject);
                }
                var property = subject.GetType().GetProperty(pathSegment, accessFlags);
                if (property != null) {
                    propertyExists = true;
                    return property.GetValue(subject);
                }
            } catch (System.Exception) {
            }
            return null;
        }

        public static object ObjectProperty(object subject, string propertyName, bool forceDirectAccess = false)
        {
            var propertyExists = false;
            var propertyValue = GetPropertyInternal(subject, propertyName, forceDirectAccess, out propertyExists);
            if (propertyExists) {
                return propertyValue;
            }
            throw new System.Exception($"The property \"{propertyName}\" on the subject was not accessible.");
        }

    }
}