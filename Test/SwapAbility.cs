using SFML.System;

namespace Game
{
    public class PlayerSwapAbility
    {
        public Ability ability = new();

        public float CalculateDistance(Vector2f playerPos, Vector2f otherPlayerPos)
            => Math.Abs((otherPlayerPos.X - playerPos.X) + (otherPlayerPos.Y - playerPos.Y));

        public void SwapPlayers(Player playerToSwap1, Player playerToSwap2)
            => (playerToSwap2.playerShape, playerToSwap1.playerShape) = (playerToSwap1.playerShape, playerToSwap2.playerShape);
    }
}
