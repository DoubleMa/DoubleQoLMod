using DoubleQoL.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

namespace DoubleQoL.Config {

    internal class XElementWithComment {
        public string Tag { get; private set; }
        public string Comment { get; private set; }

        public XElementWithComment(string tag, string comment = null) {
            Tag = tag;
            Comment = comment;
        }
    }

    internal class XSectionWithComment : XElementWithComment {
        public XElement Element { get; protected set; }

        public XSectionWithComment(string tag, string comment = null) : base(tag, comment) {
            Element = ConfigLoader.Instance.GetSectionOrCreate(this);
        }
    }

    internal class XKeyWithComment<T> : XElementWithComment where T : IComparable, IConvertible {
        public XSectionWithComment SectionWithComment { get; private set; }
        public string Key { get; private set; }
        public string StrValue { get; private set; }
        private T[] AcceptedValues { get; }
        public T DefaultValue { get; private set; }
        public T Value { get; private set; }

        public XKeyWithComment(XSectionWithComment xSectionWithComment, string key, T[] acceptedValues, T defaultValue, string comment = null) : base("add", comment) {
            SectionWithComment = xSectionWithComment;
            Key = key;
            DefaultValue = defaultValue;
            AcceptedValues = acceptedValues;
            StrValue = ConfigLoader.Instance.GetValueOrCreate(this);
            Value = Convert(StrValue);
        }

        private T Convert(string value) {
            try {
                if (typeof(T) == typeof(KeyCode) && Enum.TryParse(value, out KeyCode keyCode) && AcceptedValues.Contains((T)(object)keyCode)) return (T)(object)keyCode;
                else if (typeof(T) == typeof(bool) && bool.TryParse(value, out bool boolValue) && AcceptedValues.Contains((T)(object)boolValue)) return (T)(object)boolValue;
                else if (typeof(T) == typeof(int) && int.TryParse(value, out int intValue)) return (T)(object)intValue.Between(System.Convert.ToInt32(AcceptedValues[0]), System.Convert.ToInt32(AcceptedValues[1]));
                else if (typeof(T) == typeof(string)) return (T)(object)value;
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
    }
}