using UnityEngine;
using UnityEngine.UI;

namespace HMMLunasTweaks.Buttons
{
    class ButtonTrigger : HandTrigger
    {
        public Color EnabledColor { get; set; }
        public Color DisabledColor { get; set; }
        public Text ButtonText { get; private set; }
        public Material ButtonMaterial { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            ButtonText = this.gameObject.GetComponentInChildren<Text>();
            ButtonMaterial = this.gameObject.GetComponent<Renderer>()?.material;
        }

        public void SetColour(bool enabled)
        {
            if (ButtonMaterial == null)
                return;

            ButtonMaterial.color = enabled ? EnabledColor : DisabledColor;
        }
    }
}
