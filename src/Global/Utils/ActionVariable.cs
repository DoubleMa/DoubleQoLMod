using System;

namespace DoubleQoL.General.Utils {

    public class ActionVariable<T> where T : IComparable {

        public event Action<T> ValueChanged;

        private readonly bool AlwaysFireEvent;

        private T _value;

        public ActionVariable(T value, bool AlwaysFireEvent = false) {
            _value = value;
            this.AlwaysFireEvent = AlwaysFireEvent;
        }

        public T Value {
            get => _value;
            set {
                _value = value;
                if (AlwaysFireEvent || !_value.Equals(value)) OnValueChanged(value);
            }
        }

        protected virtual void OnValueChanged(T newValue) => ValueChanged?.Invoke(newValue);
    }
}