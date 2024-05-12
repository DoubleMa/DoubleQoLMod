using DoubleQoL.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

namespace DoubleQoL.XML {

    internal class XElementWithComment {
        public string Tag { get; private set; }
        public string Comment { get; private set; }

        public XElementWithComment(string tag, string comment = null) {
            Tag = tag;
            Comment = comment;
        }
    }

    internal class XSectionWithComment : XElementWithComment {
        public XElement Element { get; private set; }

        public XSectionWithComment(IXmlLoader loader, string tag, string comment = null) : base(tag, comment) {
            Element = loader.GetSectionOrCreate(this);
        }
    }

    internal class XKeyWithComment<T> : XElementWithComment where T : IComparable, IConvertible {
        public XSectionWithComment SectionWithComment { get; private set; }
        public string Key { get; private set; }
        public string StrValue { get; private set; }
        private T[] AcceptedValues { get; set; }
        public T DefaultValue { get; private set; }
        public T Value { get; private set; }

        public XKeyWithComment(IXmlLoader loader, XSectionWithComment xSectionWithComment, string key, T[] acceptedValues, T defaultValue, string comment = null, string tag = "add") : base(tag, comment) {
            SectionWithComment = xSectionWithComment;
            Key = key;
            DefaultValue = defaultValue;
            AcceptedValues = acceptedValues;
            StrValue = loader.GetValueOrCreate(this);
            Value = Convert(StrValue);
        }

        public XKeyWithComment(IXmlLoader loader, XSectionWithComment xSectionWithComment, string key, T defaultValue, string comment = null, string tag = "add") : base(tag, comment) {
            SectionWithComment = xSectionWithComment;
            Key = key;
            DefaultValue = defaultValue;
            StrValue = loader.GetValueOrCreate(this);
            Value = Convert(StrValue);
        }

        private void HandleAcceptedValues(object acceptedValues) {
            if (typeof(T).IsArray && acceptedValues is Array valuesArray) AcceptedValues = valuesArray.Cast<T>().ToArray();
            else if (acceptedValues is T[] values) AcceptedValues = values;
            else throw new ArgumentException("Accepted values must be of type T[]", nameof(acceptedValues));
            if (AcceptedValues.Length == 0) throw new ArgumentException("Accepted values can't be empty");
        }

        private T Convert(string value) {
            try {
                if (typeof(T) == typeof(string)) return (T)(object)value;
                else if (typeof(T) == typeof(int) && int.TryParse(value, out int intValue)) return (T)(object)intValue.Between(System.Convert.ToInt32(AcceptedValues[0]), System.Convert.ToInt32(AcceptedValues[1]));
                else if (typeof(T) == typeof(bool) && bool.TryParse(value, out bool boolValue) && (AcceptedValues == null || AcceptedValues.Contains((T)(object)boolValue))) return (T)(object)boolValue;
                else if (typeof(T) == typeof(KeyCode) && Enum.TryParse(value, out KeyCode keyCode) && (AcceptedValues == null || AcceptedValues.Contains((T)(object)keyCode))) return (T)(object)keyCode;
            }
            catch (Exception) {
                Logging.Log.Warning($"Error while reading and converting the config value of {Key}");
            }

            return DefaultValue;
        }

        public string[] ConvertToStringArray() {
            string[] acceptedValues = AcceptedValues as string[];

            try {
                string[] array = Value.ToString().Split(',');
                List<string> stringList = new List<string>();
                foreach (var item in array) {
                    string str = item.CleanString();
                    if (acceptedValues.Contains(str)) stringList.Add(str);
                }
                return stringList.ToArray();
            }
            catch (Exception) {
                Logging.Log.Warning($"Error while reading and converting the config value of {Key}");
            }

            return acceptedValues;
        }

        public override string ToString() {
            return Value.ToString();
        }
    }
}