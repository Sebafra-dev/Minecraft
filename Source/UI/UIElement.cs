using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics;

namespace Minecraft.Source.UI
{
    public class UIElement(Vector2 position)
    {
        private readonly HashSet<UIElement> _childs = [];
        protected Vector2 _position = position;

        public string Name { get; private set; }

        public virtual void Update()
        {
            foreach (UIElement child in _childs)
                child.Update();
        }

        public virtual void Draw()
        {
            foreach (UIElement child in _childs)
                child.Draw();
        }

        public void AddChild(UIElement child)
        {
            if(!_childs.Add(child))
            {
#if DEBUG
                Debug.WriteLine($"Cannot add child: {child.GetType()} to: {this.GetType()}");
#endif
            }
        }

        public bool RemoveChild(UIElement child)
        {
            if (!_childs.Remove(child))
            {
#if DEBUG
                Debug.WriteLine($"Cannot remove child: {child.GetType()} to: {this.GetType()}");
#endif
                return false;
            }

            return true;
        }

        public void SetPosition(Vector2 position)
        {
            _position = position;
        }
    }
}
