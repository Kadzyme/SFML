using SFML.Window;

namespace Game
{
    public struct KeysForMoving
    {
        public Keyboard.Key UpKey;
        public Keyboard.Key DownKey;
        public Keyboard.Key LeftKey;
        public Keyboard.Key RightKey;

        public KeysForMoving(Keyboard.Key UpKey, Keyboard.Key DownKey, Keyboard.Key LeftKey, Keyboard.Key RightKey)
        {
            this.UpKey = UpKey;
            this.DownKey = DownKey;
            this.LeftKey = LeftKey;
            this.RightKey = RightKey;
        }
    }
}
