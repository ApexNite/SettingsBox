using System;

namespace SettingsBox {
    public class Setting {
        private object _value;

        public string Name { get; private set; }

        public string Description { get; private set; }

        public string Icon { get; private set; }

        internal string Section { get; private set; }

        public object Value {
            get { return _value; }
            set {
                switch (value.GetType().FullName) {
                    case "System.Boolean":
                        PlayerConfig.setOptionBool(Name, (bool)value);
                        break;
                    case "System.Int32":
                        PlayerConfig.setOptionInt(Name, (int)value);
                        break;
                    default: throw new ArgumentException("Value must be either a string or int.");
                }

                _value = value;
            }
        }

        internal Setting(string pName, string pDescription, bool pValue, string pIcon, string pSection) {
            Name = pName;
            Description = pDescription;
            _value = pValue;
            Icon = pIcon;
            Section = pSection;
        }

        internal Setting(string pName, string pDescription, int pValue, string pIcon, string pSection) {
            Name = pName;
            Description = pDescription;
            _value = pValue;
            Icon = pIcon;
            Section = pSection;
        }
    }
}
