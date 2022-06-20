using SFML.Window;

namespace Game
{
    public class Ability
    {
        public Keyboard.Key key;

        private float normalCooldown;
        private float currentCooldown;

        public void ResetCooldownOfAbility()
            => currentCooldown = normalCooldown;

        public bool IsKeyDown()
            => Keyboard.IsKeyPressed(key);

        public void UpdateCooldown(float time)
            => currentCooldown -= time;

        public void SetNormalCooldown(float cooldown)
            => normalCooldown = cooldown;

        public bool CanCastAbility()
            => currentCooldown > 0;
    }
}
