using System;
using System.Xml.Linq;

namespace DoubleQoL.XML {

    internal interface IXmlLoader {

        XElement GetSectionOrCreate(XSectionWithComment x);

        string GetValueOrCreate<T>(XKeyWithComment<T> x) where T : IComparable, IConvertible;

        bool ReplaceKey(string tag, string toReplace, string newKey);

        void Save();
    }
}