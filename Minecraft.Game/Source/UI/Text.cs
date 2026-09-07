using Microsoft.Xna.Framework;

namespace Minecraft.Source.UI
{
    public class Text : UIElement
    {
        private string _text;

        public Text(Vector2 position, string text = "") : base(position)
        {
            SetText(text);
        }

        public void SetText(string text)
        {
            _text = text;
        }

        public override void Draw()
        {
            var spriteBatch = Globals.GetSpriteBatch();
            var font = Globals.GetFont();

            spriteBatch.DrawString(font, _text, _position, Color.White);

            base.Draw();
        }
    }
}
