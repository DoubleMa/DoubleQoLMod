using DoubleQoL.Global;
using System;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

namespace DoubleQoL.Config
{
    internal class XElementWithComment
    {
        public string tag { get; private set; }
        public string comment { get; private set; }

        public XElementWithComment(string tag, string comment = null)
        {
            this.tag = tag;
            this.comment = comment;
        }
    }

    internal class XSectionWithComment : XElementWithComment
    {
        public XElement xElement { get; protected set; }

        public XSectionWithComment(string tag, string comment = null) : base(tag, comment)
        {
            xElement = ConfigLoader.Instance.GetSectionOrCreate(this);
        }
    }

    internal class XKeyWithComment : XElementWithComment
    {
        public XSectionWithComment xSectionWithComment { get; private set; }
        public string key { get; private set; }
        public string defaultValue { get; private set; }
        private string[] acceptedValues;
        private int[] acceptedIntValues;
        private string valueOrDefault;

        public XKeyWithComment(XSectionWithComment xSectionWithComment, string key, string[] acceptedValues, string defaultValue, string comment = null) : base("add", comment)
        {
            this.xSectionWithComment = xSectionWithComment;
            this.key = key;
            this.defaultValue = defaultValue;
            this.acceptedValues = acceptedValues;
            valueOrDefault = acceptedOrDefault(ConfigLoader.Instance.GetValueOrCreate(this));
        }

        public XKeyWithComment(XSectionWithComment xSectionWithComment, string key, int[] acceptedIntValues, string defaultValue, string comment = null) : base("add", comment)
        {
            this.xSectionWithComment = xSectionWithComment;
            this.key = key;
            this.defaultValue = defaultValue;
            this.acceptedIntValues = acceptedIntValues;
            valueOrDefault = acceptedOrDefault(ConfigLoader.Instance.GetValueOrCreate(this));
        }

        private bool accepted(string value) => acceptedValues == null || acceptedValues.Length == 0 || acceptedValues.Contains(value);

        private string acceptedOrDefault(string value) => value != null && accepted(value) ? value : defaultValue;

        public bool getBoolValue() => Convert.ToBoolean(valueOrDefault);

        public KeyCode getKeyCodeValue()
        {
            var temp = Enum.TryParse(valueOrDefault, out KeyCode result);
            return temp ? result : KeyCode.None; // shouldn't happen
        }

        public int getIntValue()
        {
            int result = int.Parse(defaultValue);
            try
            {
                result = int.Parse(valueOrDefault);
            }
            catch { }
            if (acceptedIntValues.Length == 2) return result.Between(acceptedIntValues[0], acceptedIntValues[1]);
            else if (acceptedIntValues.Length == 1) return Math.Max(result, acceptedIntValues[0]);
            else return result;
        }

        public string getStringValue() => valueOrDefault;
    }
}